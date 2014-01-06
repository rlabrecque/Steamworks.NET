namespace Steamworks {
	public struct SteamLeaderboard_t {
		public ulong m_SteamLeaderboard;

		public SteamLeaderboard_t(ulong value) {
			m_SteamLeaderboard = value;
		}

		public override string ToString() {
			return m_SteamLeaderboard.ToString();
		}
	}
}
