#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks.Sources;

#nullable enable

namespace Steamworks.CoreCLR {
	internal class ValueTaskCallResultSource(ulong slot) : IValueTaskSource<object> {
		private ConcurrentDictionary<short, CallResultInvocationState> stateStore = new();
		internal ulong Slot { get; private set; } = slot;

		internal void Reset(ulong newSlot) {
			Slot = newSlot;
		}

		public event Action<ValueTaskCallResultSource>? IdleForRecycle;
		public long LastAliveCheckTicks { get; private set; } = Environment.TickCount64;
		private const long IdleDurationTicks = TimeSpan.TicksPerSecond * 20;
		private bool IsEmpty => stateStore.IsEmpty;

		public object GetResult(short token) {

			if (stateStore.TryRemove(token, out var value)) {
				value.CancellationToken.ThrowIfCancellationRequested();

				if (!value.IsCompleted) {
					throw new InvalidOperationException($"Steam API call(id: {Slot << 16 | (ushort)token}) is not completed yet.");
				}

				if (value.IOFail) {
					throw new SteamIOFailureException("IO operation failed during Steam API Call.", SteamUtils.GetAPICallFailureReason(new(Slot << 16 | (ushort)token)));
				}

				if (IsEmpty && Environment.TickCount64 - LastAliveCheckTicks >= IdleDurationTicks) {
					IdleForRecycle?.Invoke(this);
				}

				return value.Result ?? throw new InvalidDataException("Unable to retrieve result from steam api");
			}
			throw new InvalidOperationException($"Steam API call(id: {(Slot << 16) | (ushort)token}) is not registered yet.");
		}

		public ValueTaskSourceStatus GetStatus(short token) {
			if (stateStore.TryGetValue(token, out var state)) {
				if (state.CancellationToken.IsCancellationRequested) {
					return ValueTaskSourceStatus.Canceled;
				} else if (!state.IsCompleted) {
					return ValueTaskSourceStatus.Pending;
				} else if (state.IOFail) {
					return ValueTaskSourceStatus.Faulted;
				} else {
					return ValueTaskSourceStatus.Succeeded;
				}
			} else {
				// there must be something wrong, either awaited a vt twice or something else
				Debugger.Break();
				return ValueTaskSourceStatus.Pending;
			}
		}

		public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) {
			if (stateStore.TryGetValue(token, out var iState)) {
				iState.RegisterOnCompleted(continuation, state, flags);
			} else {
				throw new InvalidOperationException("Requesting api call may already completed or not registered");
			}
		}

		public void Register(short token, Type resultType, CancellationToken cancellationToken) {
			stateStore.GetOrAdd(token, new CallResultInvocationState(resultType, cancellationToken));
		}

		public void TriggerCompletion(short token, nint param, bool ioFail) {
			LastAliveCheckTicks = Environment.TickCount64;
			if (stateStore.TryGetValue(token, out var state)) {
				state.NotifyCompleted(param, ioFail);
			}
		}
	}
}
#endif