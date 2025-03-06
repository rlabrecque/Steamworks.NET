using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK

using System.Threading.Tasks.Sources;
using static Steamworks.CoreCLR.SchedulerContextHelpers;

#nullable enable

namespace Steamworks.CoreCLR;

internal class CallResultInvocationState {
	public object? Result { get; set; }
	[MemberNotNullWhen(false, nameof(Result))]
	public bool IOFail { get; set; }
	public CancellationToken CancellationToken { get; }

	public Type ResultType { get; }
	public bool IsCompleted { get; set; }


	private (Action<object?> d, object? s, object? SchedContext)? firstCompletion;

	private ConcurrentBag<(Action<object?> d, object? s, object? SchedContext)>? additionalCompletion;

	public CallResultInvocationState(Type resultType, CancellationToken cancellationToken) {
		CancellationToken = cancellationToken;
		ResultType = resultType;
	}

	public void NotifyCompleted(IntPtr nativeBuffer, bool ioFailure) {
		Result = Marshal.PtrToStructure(nativeBuffer, ResultType);
		IOFail = ioFailure;
		IsCompleted = true;

		if (firstCompletion.HasValue) {
			(var c, var s, var sc) = firstCompletion.Value;
			DispatchCompletion(c, s, sc);
		}

		var additionalCompletion = this.additionalCompletion;
		if (additionalCompletion != null) {
			foreach (var (d, s, SchedContext) in additionalCompletion) {
				DispatchCompletion(d, s, SchedContext);
			}
		}	
	}

	public void RegisterOnCompleted(Action<object?> d, object? s, ValueTaskSourceOnCompletedFlags flags) {
		var schedContext = CaptureSchedulerAndContext(flags);

		if (!firstCompletion.HasValue) {
			firstCompletion = (d, s, schedContext);
		} else {
			ConcurrentBag<(Action<object?> d, object? s, object? SchedContext)> localCopy;
			if (additionalCompletion == null) {
				localCopy = [];
				Volatile.Write(ref additionalCompletion, localCopy);
			} else {
				localCopy = additionalCompletion;
			}

			localCopy.Add((d, s, schedContext));
		}
	}

}

#endif