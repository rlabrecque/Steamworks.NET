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
			return ValueTaskDispatcher.Singleton.Register<T>(handle, isGameServer: false, cancellationToken);
		}

		public static ValueTask<T> AsServerTask<T>(this SteamAPICall_t handle,
			CancellationToken cancellationToken = default) 
				where T : struct {
			return ValueTaskDispatcher.Singleton.Register<T>(handle, isGameServer: true, cancellationToken);
		}

		public static void RegisterCallback<T>(Action<T> callback, ValueTaskSourceOnCompletedFlags completedFlags
				= ValueTaskSourceOnCompletedFlags.FlowExecutionContext) where T:struct {
			ValueTaskDispatcher.Singleton.RegisterCallback(callback, false, completedFlags);
		}

		public static void UnregisterCallback<T>(Action<T> callback) where T:struct {
			ValueTaskDispatcher.Singleton.UnregisterCallback(callback, false);
		}
		
		public static void RegisterServerCallback<T>(Action<T> callback, ValueTaskSourceOnCompletedFlags completedFlags
				= ValueTaskSourceOnCompletedFlags.FlowExecutionContext) where T:struct {
			ValueTaskDispatcher.Singleton.RegisterCallback(callback, true, completedFlags);
		}

		public static void UnregisterServerCallback<T>(Action<T> callback) where T:struct {
			ValueTaskDispatcher.Singleton.UnregisterCallback(callback, true);
		}
	}
} 
#endif
