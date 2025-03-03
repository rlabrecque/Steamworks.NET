#if NET8_0_OR_GREATER && STEAMWORKS_FEATURE_VALUETASK
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Steamworks.CoreCLR {
	public static class ValueTaskCallback {
		public static ValueTask<T> AsTask<T>(this SteamAPICall_t handle,
			CancellationToken cancellationToken = default) 
				where T : struct {
			return ValueTaskDispatcher.Singleton.Register<T>(handle, cancellationToken);
		}

		public struct CallbackRegistration<T> : IDisposable
			where T : struct {
			private readonly Action<T> callback;
			private bool disposedValue;
			internal CallbackRegistration(Action<T> callback, bool isGameServer) {
				this.callback = callback;
				IsGameServer = isGameServer;
			}

			public bool IsGameServer { get; }

			public void Dispose() {
				if (!disposedValue) {
					ValueTaskDispatcher.Singleton.UnregisterCallback(callback, IsGameServer);
					disposedValue = true;
				}
			}
		}

		public static CallbackRegistration<T> RegisterCallback<T>(Action<T> callback, ValueTaskSourceOnCompletedFlags completedFlags
				= ValueTaskSourceOnCompletedFlags.FlowExecutionContext) where T:struct {
			ValueTaskDispatcher.Singleton.RegisterCallback(callback, false, completedFlags);
			
			return new(callback, false);
		}

		public static CallbackRegistration<T> RegisterServerCallback<T>(Action<T> callback, ValueTaskSourceOnCompletedFlags completedFlags
				= ValueTaskSourceOnCompletedFlags.FlowExecutionContext) where T:struct {
			ValueTaskDispatcher.Singleton.RegisterCallback(callback, true, completedFlags);

			return new(callback, true);
		}

	}
} 
#endif
