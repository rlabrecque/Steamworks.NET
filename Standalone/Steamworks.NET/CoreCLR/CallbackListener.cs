using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK
using System.Threading.Tasks.Sources;
using static Steamworks.CoreCLR.SchedulerContextHelpers;
#nullable enable
namespace Steamworks.CoreCLR {
	public class CallbackListener<T> : ICallbackListener
		where T : struct {
		public Type ResultType { get; } = typeof(T);

		internal int Identity { get; } = CallbackIdentities.GetCallbackIdentity(typeof(T));

		int ICallbackListener.Identity => Identity;

		private readonly ConcurrentDictionary<Action<T>, object?> listeners = [];
		private readonly Action<object?> cachedRunCallbackWrapper;

		public CallbackListener() {
			cachedRunCallbackWrapper = UnboxResultAndInvoke;
		}

		public void InvokeCallbacks(ref CallbackMsg_t callbackMsg) {
			if (callbackMsg.m_cubParam != Marshal.SizeOf(ResultType)) {
				throw new ArgumentException("Size of result struct from steam is mismatch to Steamworks.NET definition\n" +
					$"Size {callbackMsg.m_cubParam}, Excepted {Marshal.SizeOf(ResultType)}, Desired Type {ResultType}");
			}

			object result = Marshal.PtrToStructure(callbackMsg.m_pubParam, ResultType)!;

			foreach (var callbackContextPair in listeners) {
				(Action<T> cb, object result) runState = (callbackContextPair.Key, result);
				DispatchCompletion(cachedRunCallbackWrapper, runState, callbackContextPair.Value);
			}
		}

		public void Register(Action<T> cb, ValueTaskSourceOnCompletedFlags flags) {
			object? sc = CaptureSchedulerAndContext(flags);
			if (listeners.AddOrUpdate(cb, sc, (cb, sc) => sc) != sc) {
				throw new InvalidOperationException("Another thread is attempting to register the same callback");
			}
		}
		public void Unregister(Action<T> cb) {
			listeners.TryRemove(cb, out _);
		}

		private void UnboxResultAndInvoke(object? value) {
			Debug.Assert(value != null);
			if (CallbackIdentities.GetCallbackIdentity(value.GetType()) != Identity) {
				Debugger.Break();
				throw new ArgumentException("Result type for callback mismatch(detected by identity)"); 
			}

			(Action<T> realCallback, object result) = ((Action<T>, object))value;

			try {
				realCallback((T)result);
			} catch (Exception e) {
				ValueTaskDispatcher.DefaultOnUnhandledException(e);
			}
		}
	}
}
#endif