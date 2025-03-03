#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

#nullable enable

namespace Steamworks.CoreCLR {
	internal class ValueTaskDispatcher {
		/// <summary>
		/// Indicates whether user want to use valuetask dispatcher
		/// </summary>
		/// <remarks>
		/// int Typed because wanted to use CAS API to ensure thread safety
		/// </remarks>
		internal static int s_valueTaskDispatchInitialized = DispatchInitState_NotInitialized;
		internal const int DispatchInitState_NotInitialized = 0;
		internal const int DispatchInitState_Initialized = 1;
		internal const int DispatchInitState_DisabledByClassic = 2;

		private readonly ConcurrentStack<ValueTaskCallResultSource> idleSources = [];
		private readonly ConcurrentDictionary<ulong, ValueTaskCallResultSource> callresultSources = new();
		private readonly ConcurrentDictionary<(int Identity, bool IsServer), ICallbackListener> callbackListeners = new();

		public static readonly ValueTaskDispatcher Singleton = new();


		#region API

		public ValueTask<TResult> Register<TResult>(SteamAPICall_t handle, CancellationToken cancellationToken = default)
			where TResult : struct {
			var sourceBoxed = PrepareRegister(handle, out short token);

			sourceBoxed.Register(token, typeof(TResult), cancellationToken);

			return new ValueTask<TResult>(new UnboxResultVTSource<TResult>(sourceBoxed), token);
		}

		public void RegisterCallback<TResult>(Action<TResult> callback,
				bool isGameServer,
				ValueTaskSourceOnCompletedFlags completedFlags = ValueTaskSourceOnCompletedFlags.FlowExecutionContext)
			where TResult : struct {
			int identity = CallbackIdentities.GetCallbackIdentity(typeof(TResult));
			var listener = (CallbackListener<TResult>)callbackListeners.GetOrAdd((identity, isGameServer), (packed) => new CallbackListener<TResult>());

			listener.Register(callback, completedFlags);
		}

		public void UnregisterCallback<TResult>(Action<TResult> callback, bool isGameServer) where TResult : struct {
			int identity = CallbackIdentities.GetCallbackIdentity(typeof(TResult));
			var listener = (CallbackListener<TResult>)callbackListeners.GetOrAdd((identity, isGameServer), (packed) => new CallbackListener<TResult>());

			listener.Unregister(callback);
		}

		#endregion

		#region Dispatch

		private static readonly uint CallBackMsgTSize = (uint)Marshal.SizeOf(typeof(CallbackMsg_t));

		[ThreadStatic]
		private static CallResultBuffer? s_callResultBuffer;
		[ThreadStatic]
		private static CallResultBuffer? s_callbackMsgBuffer;
		private sealed class CallResultBuffer : IDisposable {
			private const int DefaultBufferSize = 4096;
			private const int TooLargeSizeThreshold = (int)(DefaultBufferSize * 1.2);
			// shrink if buffer too large counter reached this amount:
			private const int ShrinkBufferThreshold = 330;

			private volatile int bufferTooLargeCounter = 0;

			private int currentCallResultBufferSize;
			private IntPtr pCallResultBuffer;

			private bool disposedValue;

			/// <summary>
			/// Acquire buffer. See exception details before use.
			/// </summary>
			/// <param name="requiredBufferSize">Exact desired buffer size for receiving result.</param>
			/// <returns>Unmanaged buffer for receiving result</returns>
			/// <exception cref="NotSupportedException">Route to <see cref="DefaultOnUnhandledException(Exception)"/></exception>
			/// <exception cref="ObjectDisposedException">Recreate an new instance if thrown</exception>
			/// <exception cref="NullReferenceException">Recreate an new instance if thrown</exception>
			public IntPtr AcquireBuffer(uint requiredBufferSize) {
				CheckIsDisposed(); // also checks if this reference is null

				if (currentCallResultBufferSize >= requiredBufferSize) {
					// buffer is enough, this case will happen mostly

					// check is there a large struct incoming
					if (requiredBufferSize >= TooLargeSizeThreshold) {
						// yes and guess we will have some big structs in near future
						// keep big buffer now and reset counter

						// thread-safe set
						var currentCounterValue = bufferTooLargeCounter;
						while (Interlocked.CompareExchange(ref bufferTooLargeCounter, 0, currentCounterValue) != currentCounterValue) {
							// this thread failed the race, try again
							currentCounterValue = bufferTooLargeCounter;
						}

						return pCallResultBuffer;
					}
					// check counter to see should we shrink, do thread-safe get
					else if (Interlocked.Increment(ref bufferTooLargeCounter) < ShrinkBufferThreshold) {
						return pCallResultBuffer;
					}
				}

				// have to resize buffer
				lock (this) {
					CheckIsDisposed();
					// double check buffer size to avoid resize the buffer smaller
					if (currentCallResultBufferSize >= requiredBufferSize) {
						return pCallResultBuffer;
					}

					requiredBufferSize = Math.Max(requiredBufferSize, DefaultBufferSize);

					// round buffer size to next multiple of 4096
					uint newBufferSize = requiredBufferSize;
					if ((newBufferSize & 0x1FFF) != 4096) {
						newBufferSize = (newBufferSize + 4095) & 0xFFFFF000;
					}

					if (newBufferSize > int.MaxValue) {
						// not to use enlarged size since we don't have enough space
						newBufferSize = requiredBufferSize;
						if (newBufferSize > int.MaxValue) {
							// this exception should route to ExceptionHandler()
							throw new NotSupportedException("The param size of a call result is larger than 2GiB");
						}
					}

#if NET5_0_OR_GREATER
					pCallResultBuffer = Marshal.ReAllocHGlobal(pCallResultBuffer, (nint)newBufferSize);
#else
					Marshal.FreeHGlobal(pCallResultBuffer);
					pCallResultBuffer = IntPtr.Zero; // is this necessary?
					pCallResultBuffer = Marshal.AllocHGlobal((int)newBufferSize);
#endif
					currentCallResultBufferSize = (int)newBufferSize;
					return pCallResultBuffer;
				}
			}


			private void CheckIsDisposed() {
				if (disposedValue) {
					throw new ObjectDisposedException(GetType().FullName, "Attempt to use a released call-result buffer.");
				}
			}

			private void Dispose(bool disposing) {
				if (!disposedValue) {
					lock (this) {
						Marshal.FreeHGlobal(pCallResultBuffer);
						disposedValue = true;
					}
				}
			}

			~CallResultBuffer() {
				Dispose(disposing: false);
			}

			public void Dispose() {
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}

		private int recursiveInitCount = 0;

		public void Initialize() {
			if (Interlocked.Increment(ref recursiveInitCount) != 1)
				return;

			int oldInitValue = Interlocked.CompareExchange(ref s_valueTaskDispatchInitialized, DispatchInitState_Initialized, DispatchInitState_NotInitialized);
			switch (oldInitValue) {
				case DispatchInitState_Initialized:
					throw new InvalidOperationException("ValueTask call-result dispatcher is already initialized.");
				case DispatchInitState_DisabledByClassic:
					throw new InvalidOperationException("ValueTask call-result dispatcher can't initialized, because classic dispatcher is already running.");
				case DispatchInitState_NotInitialized: {
					NativeMethods.SteamAPI_ManualDispatch_Init();
					break;
				}
				default:
					Debugger.Break();
					throw new ArgumentOutOfRangeException(nameof(s_valueTaskDispatchInitialized), oldInitValue, "[ASSERTION FAILURE] Unexcepted value!");
			}

			s_callbackMsgBuffer = new();
			s_callResultBuffer = new();
		}

		public void UnInitialize() {
			int newValue;
			if ((newValue = Interlocked.Decrement(ref recursiveInitCount)) != 0) {
				if (newValue < 0)
					recursiveInitCount = 0;
				return;
			}

			s_callbackMsgBuffer?.Dispose();
			s_callResultBuffer?.Dispose();
			s_callResultBuffer = null;
			s_callbackMsgBuffer = null;
		}

		internal void RunFrame(bool isGameServer) {
			if (s_valueTaskDispatchInitialized != DispatchInitState_Initialized)
				throw new InvalidOperationException("Callback dispatcher is not initialized.");

			HSteamPipe hSteamPipe = (HSteamPipe)(isGameServer ? NativeMethods.SteamGameServer_GetHSteamPipe() : NativeMethods.SteamAPI_GetHSteamPipe());
			NativeMethods.SteamAPI_ManualDispatch_RunFrame(hSteamPipe);

			try {
				IntPtr pCallbackMsg;
				CallResultBuffer? bufferHolderMsg = s_callbackMsgBuffer;
				#region Creation Prolog
				try {
					// In most cases s_callResultBuffer will have valid value,
					// by moving rare cases(recreate buffer holder) to exception path,
					// should avoid generating creation code into usage branch
					// and keep usage branch clear.

					pCallbackMsg = bufferHolderMsg!.AcquireBuffer(CallBackMsgTSize);
				} catch (ObjectDisposedException) {
					var bufferHolderNew = new CallResultBuffer();
					pCallbackMsg = bufferHolderNew.AcquireBuffer(CallBackMsgTSize);

					// try set shared buffer to newly created one, accept race failure
					Interlocked.CompareExchange(ref s_callResultBuffer, bufferHolderNew, bufferHolderMsg);
					// avoid new instance from being gc collected
					bufferHolderMsg = bufferHolderNew;
				} catch (NullReferenceException) {
					// keep same as above
					var bufferHolderNew = new CallResultBuffer();
					pCallbackMsg = bufferHolderNew.AcquireBuffer(CallBackMsgTSize);

					Interlocked.CompareExchange(ref s_callResultBuffer, bufferHolderNew, bufferHolderMsg);
					bufferHolderMsg = bufferHolderNew;
				}
				#endregion
				while (NativeMethods.SteamAPI_ManualDispatch_GetNextCallback(hSteamPipe, pCallbackMsg)) {
#if NET5_0_OR_GREATER
					// Do not modify the fields inside, or will violate some .NET runtime constraint!
					ref CallbackMsg_t callbackMsg = ref Unsafe.Unbox<CallbackMsg_t>(Marshal.PtrToStructure(pCallbackMsg, typeof(CallbackMsg_t))!);
#else
					CallbackMsg_t callbackMsg = (CallbackMsg_t)Marshal.PtrToStructure(m_pCallbackMsg, typeof(CallbackMsg_t));
#endif

					// Check for dispatching API call results
					if (callbackMsg.m_iCallback == SteamAPICallCompleted_t.k_iCallback) {
#if NET5_0_OR_GREATER
						// Same as above!
						ref SteamAPICallCompleted_t callCompletedCb = ref Unsafe.Unbox<SteamAPICallCompleted_t>(
							Marshal.PtrToStructure(callbackMsg.m_pubParam, typeof(SteamAPICallCompleted_t))!
						);
#else
						SteamAPICallCompleted_t callCompletedCb = (SteamAPICallCompleted_t)Marshal.PtrToStructure(callbackMsg.m_pubParam, typeof(SteamAPICallCompleted_t));
#endif
						// threading safe issues in allocating call-result buffer is handled by AcquireBuffer()
						IntPtr pCallResultBuffer;
						CallResultBuffer? bufferHolder = s_callResultBuffer;
						#region Buffer Creation Prolog
						try {
							// In most cases s_callResultBuffer will have valid value,
							// by moving rare cases(recreate buffer holder) to exception path,
							// should avoid generating creation code into usage branch
							// and keep usage branch clear.
							pCallResultBuffer = bufferHolder!.AcquireBuffer(callCompletedCb.m_cubParam);
						} catch (NotSupportedException ex) {
							DefaultOnUnhandledException(ex);
							continue;
						} catch (ObjectDisposedException) {
							var bufferHolderNew = new CallResultBuffer();
							pCallResultBuffer = bufferHolderNew.AcquireBuffer(callCompletedCb.m_cubParam);

							// try set shared buffer to newly created one, accept race failure
							Interlocked.CompareExchange(ref s_callResultBuffer, bufferHolderNew, bufferHolder);
							// avoid new instance from being gc collected
							bufferHolder = bufferHolderNew;
						} catch (NullReferenceException) {
							// keep same as above
							var bufferHolderNew = new CallResultBuffer();
							pCallResultBuffer = bufferHolderNew.AcquireBuffer(callCompletedCb.m_cubParam);

							Interlocked.CompareExchange(ref s_callResultBuffer, bufferHolderNew, bufferHolder);
							bufferHolder = bufferHolderNew;
						}
						#endregion

						bool bFailed;

						if (NativeMethods.SteamAPI_ManualDispatch_GetAPICallResult(
								hSteamPipe, callCompletedCb.m_hAsyncCall, pCallResultBuffer,
								(int)callCompletedCb.m_cubParam, callCompletedCb.m_iCallback,
								out bFailed)) {
							NotifyCompletion(callCompletedCb.m_hAsyncCall, isGameServer, pCallResultBuffer, bFailed);
						}
					} else {
						if (callbackListeners.TryGetValue((callbackMsg.m_iCallback, isGameServer), out var listener)) {
							listener.InvokeCallbacks(ref callbackMsg);
						}
					}
				}
			} catch (Exception e) {
				OnUnhandledException(e);
			} finally {
				NativeMethods.SteamAPI_ManualDispatch_FreeLastCallback(hSteamPipe);
			}
		}

		public static event Action<Exception> OnUnhandledException = DefaultOnUnhandledException;

		public static void DefaultOnUnhandledException(Exception e) {
#if UNITY_STANDALONE
			UnityEngine.Debug.LogException(e);
#elif STEAMWORKS_WIN || STEAMWORKS_LIN_OSX
			Console.WriteLine(e);
#endif
		}
		#endregion

		#region Tools

		private void RecycleIdleSource(ValueTaskCallResultSource obj) {
			if (idleSources.Count < 32)
				idleSources.Push(obj);
		}

		private ValueTaskCallResultSource PrepareRegister(SteamAPICall_t handle, out short token) {
			// use low bits as completion token
			//     0x1234 5678 ABCD          xxxx
			//  [api call long identity] [ vts token ]
			CutHandleParts(handle, out token, out ulong slot);

			return callresultSources.GetOrAdd(slot, (slot) => {
				if (idleSources.TryPop(out var idleSource)) {
					idleSource.Reset(slot);
					return idleSource;
				}

				ValueTaskCallResultSource source = new(slot);
				source.IdleForRecycle += RecycleIdleSource;
				return source;
			});
		}

		/// <summary>
		/// Notify completion for associated <see cref="ValueTask"/>s
		/// </summary>
		/// <devdoc>
		/// <remarks>
		/// internal for unit testing
		/// </remarks>
		/// </devdoc>
		internal void NotifyCompletion(SteamAPICall_t handle, bool isServer, IntPtr param, bool ioFail) {
			CutHandleParts(handle, out short token, out ulong slot);
			if (callresultSources.TryGetValue((slot, isServer), out var source)) {
				source.TriggerCompletion(token, param, ioFail);
			}
		}

		// internal for unit tests only
		internal static void CutHandleParts(SteamAPICall_t handle, out short token, out ulong slot) {
			slot = handle.m_SteamAPICall >> 16;
			token = unchecked((short)(handle.m_SteamAPICall & (ushort)0xffffu));
		}

		internal void InternalCaughtUnhandledException(Exception e) {
			OnUnhandledException(e);
		}

		private class UnboxResultVTSource<T>(IValueTaskSource<object> source) : IValueTaskSource<T> where T : struct {
			// internal static readonly UnboxResultVTSource<T> Singleton
			private readonly IValueTaskSource<object> source = source;

			public T GetResult(short token) {
				return (T)source.GetResult(token);
			}

			public ValueTaskSourceStatus GetStatus(short token) => source.GetStatus(token);

			public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
				=> source.OnCompleted(continuation, state, token, flags);
		}

		#endregion
	}
}
#endif
