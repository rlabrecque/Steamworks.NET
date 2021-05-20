using System;

namespace Steamworks
{
	[Flags]
	public enum FailureReason
	{
		None = 0,
		SteamAPI_Init = 1,
		hSteamPipe = 2,
		m_pSteamClient = 4,
		m_pSteamUser = 8,
		m_pSteamFriends = 16,
		m_pSteamUtils = 32,
		m_pSteamMatchmaking = 64,
		m_pSteamMatchmakingServers = 128,
		m_pSteamUserStats = 256,
		m_pSteamApps = 512,
		m_pSteamNetworking = 1024,
		m_pSteamRemoteStorage = 2048,
		m_pSteamScreenshots = 4096,
		m_pSteamGameSearch = 8192,
		m_pSteamHTTP = 16384,
		m_pSteamUGC = 32768,
		m_pSteamAppList = 65536,
		m_pSteamMusic = 131072,
		m_pSteamMusicRemote = 262144,
		m_pSteamHTMLSurface = 524288,
		m_pSteamInventory = 1048576,
		m_pSteamVideo = 2097152,
		m_pSteamParentalSettings = 4194304,
		m_pSteamInput = 8388608,
		m_pSteamParties = 16777216,
		m_pSteamRemotePlay = 33554432,
		m_pSteamNetworkingUtils = 67108864,
		m_pSteamNetworkingSockets = 134217728,
		m_pSteamNetworkingMessages = 268435456,
	}
}