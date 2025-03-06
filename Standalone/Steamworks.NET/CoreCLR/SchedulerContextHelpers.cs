using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK
using System.Threading.Tasks.Sources;
#nullable enable

namespace Steamworks.CoreCLR {
	internal static class SchedulerContextHelpers {
		internal static object? CaptureSchedulerAndContext(ValueTaskSourceOnCompletedFlags flags) {
			object? capturedContext = null;
			if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0) {
				capturedContext = ExecutionContext.Capture();
			}

			if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0) {
				if (SynchronizationContext.Current is SynchronizationContext sc &&
					sc.GetType() != typeof(SynchronizationContext)) {
					capturedContext = capturedContext is null ?
						sc :
						new CapturedSchedulerAndExecutionContext(sc, (ExecutionContext)capturedContext);
				} else {
					TaskScheduler ts = TaskScheduler.Current;
					if (ts != TaskScheduler.Default) {
						capturedContext = capturedContext is null ?
							ts :
							new CapturedSchedulerAndExecutionContext(ts, (ExecutionContext)capturedContext);
					}
				}
			}

			return capturedContext;
		}


		internal static void DispatchCompletion(Action<object?> continuation, object? state, object? capturedContext) {
			switch (capturedContext) {
				case null:
					ThreadPool.UnsafeQueueUserWorkItem(continuation, state, preferLocal: true);
					break;

				case ExecutionContext:
					ThreadPool.QueueUserWorkItem(continuation, state, preferLocal: true);
					break;

				default:
					ScheduleCapturedContext(capturedContext, continuation, state);
					break;
			}
		}

		internal static void ScheduleCapturedContext(object context, Action<object?> continuation, object? state) {
			Debug.Assert(
				context is SynchronizationContext or TaskScheduler or CapturedSchedulerAndExecutionContext,
				$"{nameof(context)} is {context}");

			switch (context) {
				case SynchronizationContext sc:
					ScheduleSynchronizationContext(sc, continuation, state);
					break;

				case TaskScheduler ts:
					ScheduleTaskScheduler(ts, continuation, state);
					break;

				default:
					CapturedSchedulerAndExecutionContext cc = (CapturedSchedulerAndExecutionContext)context;
					if (cc._scheduler is SynchronizationContext ccsc) {
						ScheduleSynchronizationContext(ccsc, continuation, state);
					} else {
						Debug.Assert(cc._scheduler is TaskScheduler, $"{nameof(cc._scheduler)} is {cc._scheduler}");
						ScheduleTaskScheduler((TaskScheduler)cc._scheduler, continuation, state);
					}
					break;
			}
		}

		internal static void ScheduleSynchronizationContext(SynchronizationContext sc, Action<object?> continuation, object? state) =>
				   sc.Post(continuation.Invoke, state);

		internal static void ScheduleTaskScheduler(TaskScheduler scheduler, Action<object?> continuation, object? state) =>
			Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);

		internal sealed class CapturedSchedulerAndExecutionContext {
			internal readonly object _scheduler;
			internal readonly ExecutionContext _executionContext;

			public CapturedSchedulerAndExecutionContext(object scheduler, ExecutionContext executionContext) {
				Debug.Assert(scheduler is SynchronizationContext or TaskScheduler, $"{nameof(scheduler)} is {scheduler}");
				Debug.Assert(executionContext is not null, $"{nameof(executionContext)} is null");

				_scheduler = scheduler;
				_executionContext = executionContext;
			}
		}
	}
}
#endif