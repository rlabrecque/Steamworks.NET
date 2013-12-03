using System;
using System.Runtime.InteropServices;

namespace Steamworks {
	public static class SteamAPI {
		public static bool RestartAppIfNecessary(uint unOwnAppID) {
			return NativeMethods.SteamAPI_RestartAppIfNecessary(unOwnAppID);
		}

		public static bool InitSafe() {
			return NativeMethods.SteamAPI_InitSafe();
		}

		public static void Shutdown() {
			NativeMethods.SteamAPI_Shutdown();
		}

		public static bool IsSteamRunning() {
			return NativeMethods.SteamAPI_IsSteamRunning();
		}

		public static int GetHSteamUserCurrent() {
			return NativeMethods.Steam_GetHSteamUserCurrent();
		}
		
		public static int GetHSteamPipe() {
			return NativeMethods.SteamAPI_GetHSteamPipe();
		}

		public static int GetHSteamUser() {
			return NativeMethods.SteamAPI_GetHSteamUser();
		}
	}
}