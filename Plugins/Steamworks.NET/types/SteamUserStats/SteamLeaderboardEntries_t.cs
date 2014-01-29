namespace Steamworks {
	public struct SteamLeaderboardEntries_t {
		public ulong m_SteamLeaderboardEntries;

		public SteamLeaderboardEntries_t(ulong value) {
			m_SteamLeaderboardEntries = value;
		}

		public override string ToString() {
			return m_SteamLeaderboardEntries.ToString();
		}
	}
}
