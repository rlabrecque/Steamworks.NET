// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#if !DISABLESTEAMWORKS
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks {
	//-----------------------------------------------------------------------------
	// Purpose: Data describing a single server
	//-----------------------------------------------------------------------------
	[StructLayout(LayoutKind.Sequential, Size = 372, Pack = 4)]
	[System.Serializable]
	public class gameserveritem_t {
		public string GetGameDir() {
			return Encoding.UTF8.GetString(m_szGameDir, 0, System.Array.IndexOf<byte>(m_szGameDir, 0));
		}

		public void SetGameDir(string dir) {
			m_szGameDir = Encoding.UTF8.GetBytes(dir + '\0');
		}

		public string GetMap() {
			return Encoding.UTF8.GetString(m_szMap, 0, System.Array.IndexOf<byte>(m_szMap, 0));
		}

		public void SetMap(string map) {
			m_szMap = Encoding.UTF8.GetBytes(map + '\0');
		}

		public string GetGameDescription() {
			return Encoding.UTF8.GetString(m_szGameDescription, 0, System.Array.IndexOf<byte>(m_szGameDescription, 0));
		}

		public void SetGameDescription(string desc) {
			m_szGameDescription = Encoding.UTF8.GetBytes(desc + '\0');
		}

		public string GetServerName() {
			// Use the IP address as the name if nothing is set yet.
			if (m_szServerName[0] == 0)
				return m_NetAdr.GetConnectionAddressString();
			else
				return Encoding.UTF8.GetString(m_szServerName, 0, System.Array.IndexOf<byte>(m_szServerName, 0));
		}

		public void SetServerName(string name) {
			m_szServerName = Encoding.UTF8.GetBytes(name + '\0');
		}

		public string GetGameTags() {
			return Encoding.UTF8.GetString(m_szGameTags, 0, System.Array.IndexOf<byte>(m_szGameTags, 0));
		}

		public void SetGameTags(string tags) {
			m_szGameTags = Encoding.UTF8.GetBytes(tags + '\0');
		}

		public servernetadr_t m_NetAdr;										///< IP/Query Port/Connection Port for this server
		public int m_nPing;													///< current ping time in milliseconds
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bHadSuccessfulResponse;								///< server has responded successfully in the past
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bDoNotRefresh;										///< server is marked as not responding and should no longer be refreshed
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.k_cbMaxGameServerGameDir)]
		private byte[] m_szGameDir;											///< current game directory
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.k_cbMaxGameServerMapName)]
		private byte[] m_szMap;												///< current map
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.k_cbMaxGameServerGameDescription)]
		private byte[] m_szGameDescription;									///< game description
		public uint m_nAppID;												///< Steam App ID of this server
		public int m_nPlayers;												///< total number of players currently on the server.  INCLUDES BOTS!!
		public int m_nMaxPlayers;											///< Maximum players that can join this server
		public int m_nBotPlayers;											///< Number of bots (i.e simulated players) on this server
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bPassword;											///< true if this server needs a password to join
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bSecure;												///< Is this server protected by VAC
		public uint m_ulTimeLastPlayed;										///< time (in unix time) when this server was last played on (for favorite/history servers)
		public int	m_nServerVersion;										///< server version as reported to Steam

		// Game server name
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.k_cbMaxGameServerName)]
		private byte[] m_szServerName;

		// the tags this server exposes
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.k_cbMaxGameServerTags)]
		private byte[] m_szGameTags;

		// steamID of the game server - invalid if it's doesn't have one (old server, or not connected to Steam)
		public CSteamID m_steamID;
	}
}
#endif