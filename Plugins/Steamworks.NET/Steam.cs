// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#if !DISABLESTEAMWORKS

namespace Steamworks {
	public static class Version {
		public const string SteamworksNETVersion = "10.0.0";
		public const string SteamworksSDKVersion = "1.40";
		public const string SteamAPIDLLVersion = "03.92.72.58";
		public const int SteamAPIDLLSize = 221984;
		public const int SteamAPI64DLLSize = 245536;
	}

	public static class SteamAPI {
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	Steam API setup & shutdown
		//
		//	These functions manage loading, initializing and shutdown of the steamclient.dll
		//
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//

		// SteamAPI_Init must be called before using any other API functions. If it fails, an
		// error message will be output to the debugger (or stderr) with further information.

		// [Steamworks.NET] Deprecated, just use Init(). This may be removed at a later date.
		public static bool InitSafe() {
			return Init();
		}

		public static bool Init() {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_Init();
		}

		public static void Shutdown() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_Shutdown();
		}

		// SteamAPI_RestartAppIfNecessary ensures that your executable was launched through Steam.
		//
		// Returns true if the current process should terminate. Steam is now re-launching your application.
		//
		// Returns false if no action needs to be taken. This means that your executable was started through
		// the Steam client, or a steam_appid.txt file is present in your game's directory (for development).
		// Your current process should continue if false is returned.
		//
		// NOTE: If you use the Steam DRM wrapper on your primary executable file, this check is unnecessary
		// since the DRM wrapper will ensure that your application was launched properly through Steam.
		public static bool RestartAppIfNecessary(AppId_t unOwnAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamAPI_RestartAppIfNecessary(unOwnAppID);
		}

		// Many Steam API functions allocate a small amount of thread-local memory for parameter storage.
		// SteamAPI_ReleaseCurrentThreadMemory() will free API memory associated with the calling thread.
		// This function is also called automatically by SteamAPI_RunCallbacks(), so a single-threaded
		// program never needs to explicitly call this function.
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
		// but if you choose to do this, callback code could be executed on any thread.
		// One alternative is to call SteamAPI_RunCallbacks from the main thread only,
		// and call SteamAPI_ReleaseCurrentThreadMemory regularly on other threads.
		public static void RunCallbacks() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamAPI_RunCallbacks();
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	steamclient.dll private wrapper functions
		//
		//	The following functions are part of abstracting API access to the steamclient.dll, but should only be used in very specific cases
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//

		// SteamAPI_IsSteamRunning() returns true if Steam is currently running
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
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			InteropHelp.TestIfPlatformSupported();
			using(var pchVersionString2 = new InteropHelp.UTF8StringHandle(pchVersionString)) {
				return NativeMethods.SteamGameServer_Init(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, pchVersionString2);
			}
		}

		public static void Shutdown() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_Shutdown();
		}

		public static void RunCallbacks() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_RunCallbacks();
		}

		// Most Steam API functions allocate some amount of thread-local memory for
		// parameter storage. Calling SteamGameServer_ReleaseCurrentThreadMemory()
		// will free all API-related memory associated with the calling thread.
		// This memory is released automatically by SteamGameServer_RunCallbacks(),
		// so single-threaded servers do not need to explicitly call this function.
		public static void ReleaseCurrentThreadMemory() {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamGameServer_ReleaseCurrentThreadMemory();
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
			return NativeMethods.SteamEncryptedAppTicket_BDecryptTicket(rgubTicketEncrypted, cubTicketEncrypted, rgubTicketDecrypted, ref pcubTicketDecrypted, rgubKey, cubKey);
		}

		public static bool BIsTicketForApp(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamEncryptedAppTicket_BIsTicketForApp(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static uint GetTicketIssueTime(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamEncryptedAppTicket_GetTicketIssueTime(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static void GetTicketSteamID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out CSteamID psteamID) {
			InteropHelp.TestIfPlatformSupported();
			NativeMethods.SteamEncryptedAppTicket_GetTicketSteamID(rgubTicketDecrypted, cubTicketDecrypted, out psteamID);
		}

		public static uint GetTicketAppID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamEncryptedAppTicket_GetTicketAppID(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static bool BUserOwnsAppInTicket(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamEncryptedAppTicket_BUserOwnsAppInTicket(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static bool BUserIsVacBanned(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			InteropHelp.TestIfPlatformSupported();
			return NativeMethods.SteamEncryptedAppTicket_BUserIsVacBanned(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static byte[] GetUserVariableData(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out uint pcubUserData) {
			InteropHelp.TestIfPlatformSupported();
			System.IntPtr punSecretData = NativeMethods.SteamEncryptedAppTicket_GetUserVariableData(rgubTicketDecrypted, cubTicketDecrypted, out pcubUserData);
			byte[] ret = new byte[pcubUserData];
			System.Runtime.InteropServices.Marshal.Copy(punSecretData, ret, 0, (int)pcubUserData);
			return ret;
		}
	}
}

#endif // !DISABLESTEAMWORKS
