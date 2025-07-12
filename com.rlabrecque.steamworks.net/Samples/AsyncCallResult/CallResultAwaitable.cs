// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2025 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is provided as a sample, copy into your project if you need.

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
#error Unsupported Unity platform. Steamworks.NET requires Unity 4.7 or higher.
#elif UNITY_4_7 || UNITY_5 || UNITY_2017 || UNITY_2017_1_OR_NEWER
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
#define WINDOWS_BUILD
#endif
#elif STEAMWORKS_WIN
#define WINDOWS_BUILD
#elif STEAMWORKS_LIN_OSX
	// So that we don't enter the else block below.
#else
#error You need to define STEAMWORKS_WIN, or STEAMWORKS_LIN_OSX. Refer to the readme for more details.
#endif

#if UNITY_2022_3_OR_NEWER || NETSTANDARD2_1_OR_GREATER
// Unity 2022.3 is sure that using C# 9, which have NRT features
#define STEAMWORKS_SDK_FEATURE_NULLABLE
#endif

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;



#if STEAMWORKS_SDK_FEATURE_NULLABLE
#nullable enable
#endif

namespace Steamworks
{
	/// <summary>
	/// Abstraction for running <see cref="CallResultAwaitable{T}"/> callbacks and continuations.
	/// </summary>
	// A copy of PipeScheduler
	public abstract class CallResultScheduler
	{
#if STEAMWORKS_SDK_FEATURE_NULLABLE
		public abstract void Schedule(Action<object?> callback, object? state);
#else
		public abstract void Schedule(Action<object> callback, object state); 
#endif
		// UnsafeSchedule is meaningless since we are outside of mscorlib/System.Private.CoreLib

		private class InlineScheduler : CallResultScheduler
		{

#if STEAMWORKS_SDK_FEATURE_NULLABLE
			public override void Schedule(Action<object?> callback, object? state)
			{
#else
			public override void Schedule(Action<object> callback, object state) {
#endif

				callback(state);
			}
		}

		private class ThreadPoolScheduler : CallResultScheduler
		{
#if STEAMWORKS_SDK_FEATURE_NULLABLE
			public override void Schedule(Action<object?> callback, object? state)
			{
#else
			public override void Schedule(Action<object> callback, object state) {
#endif

				System.Threading.ThreadPool.QueueUserWorkItem(callback, state, preferLocal: false);
			}
		}

		private class SyncContextScheduler : CallResultScheduler
		{
			private readonly SynchronizationContext syncContext;

			public SyncContextScheduler()
			{
				syncContext = SynchronizationContext.Current;
			}

#if STEAMWORKS_SDK_FEATURE_NULLABLE
			public override void Schedule(Action<object?> callback, object? state)
#else
			public override void Schedule(Action<object> callback, object state)
#endif
			{
#if STEAMWORKS_SDK_FEATURE_NULLABLE
				syncContext.Post((state) => { callback(state); }, state!);
#else
				syncContext.Post((state) => { callback(state); }, state);
#endif
			}
		}

		private static readonly ThreadPoolScheduler s_threadPoolScheduler = new ThreadPoolScheduler();
		private static readonly InlineScheduler s_inlineScheduler = new InlineScheduler();

		public static CallResultScheduler ThreadPool => s_threadPoolScheduler;

		public static CallResultScheduler Inline => s_inlineScheduler;

		public static CallResultScheduler FromSynchronizationContext() { return new SyncContextScheduler(); }
	}

	public class CallResultAsyncOptions
	{
		public
#if STEAMWORKS_SDK_FEATURE_NULLABLE
			CallResultScheduler?
#else
			CallResultScheduler
#endif
			Scheduler
		{ get; set; }
	}

	public static class SteamAPICallExtensions
	{
		private static readonly CallResultAsyncOptions s_scheduleOnThreadPoolOptions = new CallResultAsyncOptions()
		{
			Scheduler = CallResultScheduler.ThreadPool
		};

		private static readonly CallResultAsyncOptions s_scheduleInlineOptions = new CallResultAsyncOptions()
		{
			Scheduler = CallResultScheduler.Inline
		};

		/// <summary>
		/// Construct a new <see langword="await"/>able CallResult from Steam API call handle.
		/// </summary>
		/// <remarks>
		///	All <see cref="CallResultAwaitable{T}"/> instances obtained from this method must be <see langword="await"/>ed
		///	</remarks>
		/// <param name="handle"></param>
		/// <returns>A <see cref="CallResultAwaitable{T}"/> that must be <see langword="await"/>ed</returns>
		/// <exception cref="IOException">IO failure during Steam API invocation</exception>
		public static CallResultAwaitable<T> Async<T>(this SteamAPICall_t handle,
			CallResultAsyncOptions options,
			CancellationToken cancellationToken = default)
			where T : struct
		{
			return new CallResultAwaitable<T>().ResetAsyncState(handle, options, cancellationToken);
		}

		/// <summary>
		/// Construct a new <see langword="await"/>able CallResult from Steam API call handle.
		/// When Steam API call completed, callback will run on .NET thread pool.
		/// </summary>
		/// <remarks>
		///	All <see cref="CallResultAwaitable{T}"/> instances obtained from this method must be <see langword="await"/>ed
		///	</remarks>
		public static CallResultAwaitable<T> ContinueOnThreadPool<T>(this SteamAPICall_t handle,
			CancellationToken cancellationToken = default)
			where T : struct
		{
			return Async<T>(handle, s_scheduleOnThreadPoolOptions, cancellationToken);
		}

		/// <summary>
		/// Construct a new <see langword="await"/>able CallResult from Steam API call handle.
		/// When Steam API call completed, callback will run on captured <see cref="SynchronizationContext"/>. 
		/// This is the most similar behavior to <see cref="System.Threading.Tasks.Task"/>.
		/// </summary>
		/// <remarks>
		/// All <see cref="CallResultAwaitable{T}"/> instances obtained from this method must be <see langword="await"/>ed
		/// </remarks>
		public static CallResultAwaitable<T> ContinueOnSynchronizationContext<T>(this SteamAPICall_t handle,
			CancellationToken cancellationToken = default)
			where T : struct
		{
			return Async<T>(handle,
				new CallResultAsyncOptions()
				{
					Scheduler = CallResultScheduler.FromSynchronizationContext()
				},
				cancellationToken);
		}

		/// <summary>
		/// Construct a new <see langword="await"/>able CallResult from Steam API call handle.
		/// When Steam API call completed, callback will run on at same thread of <see cref="SteamAPI.RunCallbacks"/>.
		/// </summary>
		/// <remarks>
		///	All <see cref="CallResultAwaitable{T}"/> instances obtained from this method must be <see langword="await"/>ed
		///	</remarks>
		public static CallResultAwaitable<T> ContinueNearRunCallbacks<T>(this SteamAPICall_t handle,
			CancellationToken cancellationToken = default)
			where T : struct
		{
			return Async<T>(handle, s_scheduleInlineOptions, cancellationToken);
		}
	}

	public sealed class CallResultAwaitable<T> : CallResult, ICriticalNotifyCompletion /* fulfills Awaitable, Awaiter */
		where T : struct
	{
		// async related fields
		private
#if STEAMWORKS_SDK_FEATURES_NULLABLE
			Action?
#else
			Action
#endif
			awaitableCallback;
		private T result;
		private bool ioFailed;
		private
#if STEAMWORKS_SDK_FEATURE_NULLABLE
			CallResultScheduler?
#else
			CallResultScheduler
#endif
				scheduler;
		private CancellationToken ct;
		private bool isCompleted;
		private CancellationTokenRegistration cancellationRegistration;
		private static readonly
#if STEAMWORKS_SDK_FEATURE_NULLABLE
			Action<object?>
#else
			Action<object>
#endif
				s_continuationFromThreadPoolDelegate =
#if NETSTANDARD2_1_OR_GREATER && STEAMWORKS_SDK_FEATURE_NULLABLE
						(cb) =>
						{
							Debug.Assert(cb is Action, "Passed wrong value to scheduler delegate, System.Action excepted. Code below don't do casting type check!");
							Unsafe.As<Action>(cb!)();
						};
#else
						(cb) => ((Action)cb)();
#endif

		public SteamAPICall_t Handle { get; private set; } = SteamAPICall_t.Invalid;


		public CallResultAwaitable()
		{

		}

		public CallResultAwaitable(SteamAPICall_t handle, CallResultAsyncOptions options,
			CancellationToken cancellationToken = default)
		{
			ResetAsyncState(handle, options, cancellationToken);
		}

		protected override Type GetCallbackType()
		{
			return typeof(T);
		}

		protected override void OnRunCallResult(IntPtr pvParam, bool bFailed, ulong hSteamAPICall)
		{
			isCompleted = true;

			Debug.Assert(Handle.m_SteamAPICall == hSteamAPICall, "CallResultAwaitable called with different handle than expected. This is a bug in Steamworks.NET or your code.");

			if (ct.IsCancellationRequested)
				return;

			// user code will get result by this.GetResult(), bFailed is converted to SteamCallResultException
			result = (T)Marshal.PtrToStructure(pvParam, typeof(T));
			ioFailed = bFailed;

			// try to notify user code
			SpinWait waiter = new SpinWait();
			for (int i = 0; i < 6; i++)
			{
#if STEAMWORKS_SDK_FEATURE_NULLABLE
				Action?
#else
				Action
#endif
					callbackCopy = awaitableCallback;

				if (callbackCopy != null)
				{
					ScheduleContinuation(callbackCopy);
					break;
				}

				waiter.SpinOnce();
			}
			// We didn't wait to callback registered, let UnsafeOnCompleted notify user
		}

		protected override void SetUnregistered()
		{
			Handle = SteamAPICall_t.Invalid;

		}

		#region Async

		// concept Awaitable
		// make this type `await`able like System.Threading.Tasks.Task
		/// <summary>
		/// Reserved for compiler, not meant to use directly.
		/// </summary>
		/// <remarks>
		/// Before writing code like <c>callResult.GetAwaiter().GetResult()</c>, you MUST CHECK <see cref="IsCompleted"/>.
		/// <see cref="GetResult"/> will not wait until completion, it returns cached result.
		/// </remarks>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public CallResultAwaitable<T> GetAwaiter() { return this; }

		// concept Awaiter
		public bool IsCompleted => isCompleted;

		/// <summary>
		/// Reserved for compiler, not meant to use directly.
		/// </summary>
		/// <returns>Cached result from last completed API call or default value</returns>
		/// <remarks>
		/// Before writing code like <c>callResult.GetAwaiter().GetResult()</c>, you MUST CHECK <see cref="IsCompleted"/>.
		/// <see cref="GetResult"/> will not wait until completion, it returns cached result.
		/// <exception cref="IOException">IO failure during Steam API invocation</exception>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public T GetResult()
		{
			if (ioFailed)
				throw new SteamCallResultException("Some error happened during Steam API call.", SteamUtils.GetAPICallFailureReason(Handle));

			return result;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void UnsafeOnCompleted(Action callback)
		{
			if (ct.IsCancellationRequested)
			{
				CancellationTokenRegistration ctr = cancellationRegistration;
				cancellationRegistration = default;
				ctr.Dispose();
				return;
			}

			Interlocked.MemoryBarrier();

			if (Interlocked.CompareExchange(ref awaitableCallback, callback, null) != null)
			{
				throw new InvalidOperationException("This CallResult instance is already being awaited");
			}

			if (isCompleted)
			{
				// we didn't register callback before CallResult completed, but we have cached result. Notify user here
				ScheduleContinuation(callback);
			}

			// Register self to dispatcher here to avoid race condition of completion and resume-delegate register
			CallbackDispatcher.Register(Handle, this);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void OnCompleted(Action callback) => UnsafeOnCompleted(callback);

		// public api
		/// <summary>
		/// Make this <see cref="CallResult{T}"/> instance awaitable again
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public CallResultAwaitable<T> ResetAsyncState(SteamAPICall_t handle,
			CallResultAsyncOptions options,
			CancellationToken cancellationToken = default)
		{

			CancellationTokenRegistration ctr = cancellationRegistration;
			cancellationRegistration = default;
			ctr.Dispose();

			isCompleted = false;
			scheduler = options.Scheduler;
			ct = cancellationToken;

			if (ct.CanBeCanceled)
			{
				cancellationRegistration = ct.Register(OnCancelTriggered);
			}

			if (handle == SteamAPICall_t.Invalid)
				throw new InvalidOperationException("Cannot set invalid steam api call handle");

			// check current handle to see whether unregister this from dispatcher
			if (Handle != SteamAPICall_t.Invalid)
				CallbackDispatcher.Unregister(Handle, this);

			Handle = handle;
			awaitableCallback = null;
			result = default;
			ioFailed = false;

			return this;
		}

		private void ScheduleContinuation(Action continuation)
		{
			if (ReferenceEquals(CallResultScheduler.ThreadPool, scheduler) || scheduler == null)
			{
				ThreadPool.QueueUserWorkItem(s_continuationFromThreadPoolDelegate, continuation, preferLocal: false);
			}
			else if (ReferenceEquals(CallResultScheduler.Inline, scheduler))
			{
				continuation();
			}
			else
			{
				scheduler.Schedule(s_continuationFromThreadPoolDelegate, continuation);
			}
		}

		private void OnCancelTriggered()
		{
			if (Handle != SteamAPICall_t.Invalid)
				CallbackDispatcher.Unregister(Handle, this);
		}

		#endregion
	}

	[Serializable]
	public class SteamCallResultException : Exception
	{

		public SteamCallResultException(string message, ESteamAPICallFailure failureReason) : base(message)
		{
			FailureReason = failureReason;
		}

		public SteamCallResultException(string message, ESteamAPICallFailure failureReason, Exception innerException) : base(message, innerException)
		{
			FailureReason = failureReason;
		}

		protected SteamCallResultException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public ESteamAPICallFailure FailureReason { get; private set; }
	}
}

#endif // !DISABLESTEAMWORKS
