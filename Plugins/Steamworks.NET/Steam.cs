// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#define VERSION_SAFE_STEAM_API_INTERFACES

namespace Steamworks {
	public static class Version {
		public const string SteamworksNETVersion = "9.0.0";
		public const string SteamworksSDKVersion = "1.36";
		public const string SteamAPIDLLVersion = "03.27.76.74";
		public const int SteamAPIDLLSize = 217168;
		public const int SteamAPI64DLLSize = 239184;
	}

	public static class SteamAPI {
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	Steam API setup & shutdown
		//
		//	These functions manage loading, initializing and shutdown of the steamclient.dll
		//
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//

		// Detects if your executable was launched through the Steam client, and restarts your game through
		// the client if necessary. The Steam client will be started if it is not running.
		//
		// Returns: true if your executable was NOT launched through the Steam client. This function will
		//          then start your application through the client. Your current process should exit.
		//
		//          false if your executable was started through the Steam client or a steam_appid.txt file
		//          is present in your game's directory (for development). Your current process should continue.
		//
		// NOTE: This function should be used only if you are using CEG or not using Steam's DRM. Once applied
		//       to your executable, Steam's DRM will handle restarting through Steam if necessary.
		public static bool RestartAppIfNecessary(AppId_t unOwnAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_RestartAppIfNecessary(unOwnAppID);
		}

#if VERSION_SAFE_STEAM_API_INTERFACES
		public static bool InitSafe() {
			return Init();
		}

		// [Steamworks.NET] This is for Ease of use, since we don't need to care about the differences between them in C#.
		public static bool Init() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_InitSafe();
		}
#else
		public static bool Init() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_Init();
		}
#endif

		public static void Shutdown() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_Shutdown();
		}

		// Most Steam API functions allocate some amount of thread-local memory for
		// parameter storage. The SteamAPI_ReleaseCurrentThreadMemory() function
		// will free all API-related memory associated with the calling thread.
		// This memory is also released automatically by SteamAPI_RunCallbacks(), so
		// a single-threaded program does not need to explicitly call this function.
		public static void ReleaseCurrentThreadMemory() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_ReleaseCurrentThreadMemory();
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	steam callback and call-result helpers
		//
		//	The following macros and classes are used to register your application for
		//	callbacks and call-results, which are delivered in a predictable manner.
		//
		//	STEAM_CALLBACK macros are meant for use inside of a C++ class definition.
		//	They map a Steam notification callback directly to a class member function
		//	which is automatically prototyped as "void func( callback_type *pParam )".
		//
		//	CCallResult is used with specific Steam APIs that return "result handles".
		//	The handle can be passed to a CCallResult object's Set function, along with
		//	an object pointer and member-function pointer. The member function will
		//	be executed once the results of the Steam API call are available.
		//
		//	CCallback and CCallbackManual classes can be used instead of STEAM_CALLBACK
		//	macros if you require finer control over registration and unregistration.
		//
		//	Callbacks and call-results are queued automatically and are only
		//	delivered/executed when your application calls SteamAPI_RunCallbacks().
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//

		// SteamAPI_RunCallbacks is safe to call from multiple threads simultaneously,
		// but if you choose to do this, callback code may be executed on any thread.
		public static void RunCallbacks() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_RunCallbacks();
		}

		// checks if a local Steam client is running
		public static bool IsSteamRunning() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_IsSteamRunning();
		}

		// returns the HSteamUser of the last user to dispatch a callback
		public static HSteamUser GetHSteamUserCurrent() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamUser)NativeMethods.Steam_GetHSteamUserCurrent();
		}
		
		// returns the pipe we are communicating to Steam with
		public static HSteamPipe GetHSteamPipe() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamPipe)NativeMethods.SteamAPI_GetHSteamPipe();
		}

		public static HSteamUser GetHSteamUser() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamUser)NativeMethods.SteamAPI_GetHSteamUser();
		}
	}

	public static class GameServer {
		// Initialize ISteamGameServer interface object, and set server properties which may not be changed.
		//
		// After calling this function, you should set any additional server parameters, and then
		// call ISteamGameServer::LogOnAnonymous() or ISteamGameServer::LogOn()
		//
		// - usSteamPort is the local port used to communicate with the steam servers.
		// - usGamePort is the port that clients will connect to for gameplay.
		// - usQueryPort is the port that will manage server browser related duties and info
		//		pings from clients.  If you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE for usQueryPort, then it
		//		will use "GameSocketShare" mode, which means that the game is responsible for sending and receiving
		//		UDP packets for the master  server updater. See references to GameSocketShare in isteamgameserver.h.
		// - The version string is usually in the form x.x.x.x, and is used by the master server to detect when the
		//		server is out of date.  (Only servers with the latest version will be listed.)
#if VERSION_SAFE_STEAM_API_INTERFACES
		public static bool InitSafe(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			InteropHelp.TestIfPlatformSupported();
			using (var pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString)) {
				return NativeMethods.SteamGameServer_InitSafe(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString2);
			}
		}

		// [Steamworks.NET] This is for Ease of use, since we don't need to care about the differences between them in C#.
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			return InitSafe(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString);
		}
#else
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			InteropHelp.TestIfPlatformSupported();
			using (var pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString)) {
				return NativeMethods.SteamGameServer_Init(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString2);
		`	}
		}
#endif
		public static void Shutdown() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_Shutdown();
		}

		public static void RunCallbacks() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_RunCallbacks();
		}

		public static bool BSecure() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamGameServer_BSecure();
		}

		public static CSteamID GetSteamID() {
			InteropHelp.TestIfPlatformSupported();
			return (CSteamID)NativeMethods.SteamGameServer_GetSteamID();
		}

		public static HSteamPipe GetHSteamPipe() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamPipe)NativeMethods.SteamGameServer_GetHSteamPipe();
		}

		public static HSteamUser GetHSteamUser() {
			InteropHelp.TestIfPlatformSupported();
			return (HSteamUser)NativeMethods.SteamGameServer_GetHSteamUser();
		}
	}

	public static class SteamEncryptedAppTicket {
		public static bool BDecryptTicket(byte[] rgubTicketEncrypted, uint cubTicketEncrypted, byte[] rgubTicketDecrypted, ref uint pcubTicketDecrypted, byte[] rgubKey, int cubKey) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BDecryptTicket(rgubTicketEncrypted, cubTicketEncrypted, rgubTicketDecrypted, ref pcubTicketDecrypted, rgubKey, cubKey);
		}

		public static bool BIsTicketForApp(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BIsTicketForApp(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static uint GetTicketIssueTime(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.GetTicketIssueTime(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static void GetTicketSteamID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out CSteamID psteamID) {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.GetTicketSteamID(rgubTicketDecrypted, cubTicketDecrypted, out psteamID);
		}

		public static uint GetTicketAppID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.GetTicketAppID(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static bool BUserOwnsAppInTicket(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BUserOwnsAppInTicket(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static bool BUserIsVacBanned(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.BUserIsVacBanned(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static byte[] GetUserVariableData(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out uint pcubUserData) {
			InteropHelp.TestIfPlatformSupported();
			System.IntPtr punSecretData = NativeMethods.GetUserVariableData(rgubTicketDecrypted, cubTicketDecrypted, out pcubUserData);
			byte[] ret = new byte[pcubUserData];
			System.Runtime.InteropServices.Marshal.Copy(punSecretData, ret, 0, (int)pcubUserData);
			return ret;
		}
	}
}
