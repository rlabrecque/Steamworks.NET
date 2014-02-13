#define VERSION_SAFE_STEAM_API_INTERFACES

using System.Runtime.InteropServices;

namespace Steamworks {
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
			return NativeMethods.SteamAPI_RestartAppIfNecessary(unOwnAppID);
		}

#if VERSION_SAFE_STEAM_API_INTERFACES
		public static bool InitSafe() {
			return NativeMethods.SteamAPI_InitSafe();
		}

		// [Steamworks.NET] This is for Ease of use, since we don't need to care about the differences between them in C#.
		public static bool Init() {
			return NativeMethods.SteamAPI_InitSafe();
		}
#else
		public static bool Init() {
			return NativeMethods.SteamAPI_Init();
		}
#endif

		public static void Shutdown() {
			NativeMethods.SteamAPI_Shutdown();
		}

		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		//	steam callback helper functions
		//
		//	The following classes/macros are used to be able to easily multiplex callbacks 
		//	from the Steam API into various objects in the app in a thread-safe manner
		//
		//	These functors are triggered via the SteamAPI_RunCallbacks() function, mapping the callback
		//  to as many functions/objects as are registered to it
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		public static void RunCallbacks() {
			CallbackDispatcher.RunCallbacks();
		}

		// checks if a local Steam client is running
		public static bool IsSteamRunning() {
			return NativeMethods.SteamAPI_IsSteamRunning();
		}

		// returns the HSteamUser of the last user to dispatch a callback
		public static HSteamUser GetHSteamUserCurrent() {
			return (HSteamUser)NativeMethods.Steam_GetHSteamUserCurrent();
		}
		
		// returns the pipe we are communicating to Steam with
		public static HSteamPipe GetHSteamPipe() {
			return (HSteamPipe)NativeMethods.SteamAPI_GetHSteamPipe();
		}

		public static HSteamUser GetHSteamUser() {
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
			return NativeMethods.SteamGameServer_InitSafe(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, new InteropHelp.UTF8String(pchVersionString));
		}

		// This is for Ease of use, since we don't need to care about the differences between them in C#.
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			return NativeMethods.SteamGameServer_InitSafe(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, new InteropHelp.UTF8String(pchVersionString));
		}
#else
		public static bool Init(uint unIP, ushort usSteamPort, ushort usGamePort, ushort usQueryPort, EServerMode eServerMode, string pchVersionString) {
			return NativeMethods.SteamGameServer_Init(unIP, usSteamPort, usGamePort, usQueryPort, eServerMode, new InteropHelp.UTF8String(pchVersionString));
		}
#endif
		public static void Shutdown() {
			NativeMethods.SteamGameServer_Shutdown();
		}

		public static bool BSecure() {
			return NativeMethods.SteamGameServer_BSecure();
		}

		public static CSteamID GetSteamID() {
			return (CSteamID)NativeMethods.SteamGameServer_GetSteamID();
		}

		public static HSteamPipe GetHSteamPipe() {
			return (HSteamPipe)NativeMethods.SteamGameServer_GetHSteamPipe();
		}

		public static HSteamUser GetHSteamUser() {
			return (HSteamUser)NativeMethods.SteamGameServer_GetHSteamUser();
		}
	}

	public static class SteamEncryptedAppTicket {
		public static bool BDecryptTicket(byte[] rgubTicketEncrypted, uint cubTicketEncrypted, byte[] rgubTicketDecrypted, out uint pcubTicketDecrypted, byte[] rgubKey, int cubKey) {
			return NativeMethods.BDecryptTicket(rgubTicketEncrypted, cubTicketEncrypted, rgubTicketDecrypted, out pcubTicketDecrypted, rgubKey, cubKey);
		}

		public static bool BIsTicketForApp(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			return NativeMethods.BIsTicketForApp(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static uint GetTicketIssueTime(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			return NativeMethods.GetTicketIssueTime(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static void GetTicketSteamID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out CSteamID psteamID) {
			NativeMethods.GetTicketSteamID(rgubTicketDecrypted, cubTicketDecrypted, out psteamID);
		}

		public static uint GetTicketAppID(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			return NativeMethods.GetTicketAppID(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static bool BUserOwnsAppInTicket(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, AppId_t nAppID) {
			return NativeMethods.BUserOwnsAppInTicket(rgubTicketDecrypted, cubTicketDecrypted, nAppID);
		}

		public static bool BUserIsVacBanned(byte[] rgubTicketDecrypted, uint cubTicketDecrypted) {
			return NativeMethods.BUserIsVacBanned(rgubTicketDecrypted, cubTicketDecrypted);
		}

		public static byte[] GetUserVariableData(byte[] rgubTicketDecrypted, uint cubTicketDecrypted, out uint pcubUserData) {
			System.IntPtr punSecretData = NativeMethods.GetUserVariableData(rgubTicketDecrypted, cubTicketDecrypted, out pcubUserData);
			byte[] ret = new byte[pcubUserData];
			System.Runtime.InteropServices.Marshal.Copy(punSecretData, ret, 0, (int)pcubUserData);
			return ret;
		}
	}
}
