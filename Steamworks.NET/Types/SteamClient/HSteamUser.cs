namespace Steamworks {
	public struct HSteamUser {
		public int m_HSteamUser;

		public HSteamUser(int value) {
			m_HSteamUser = value;
		}

		public override string ToString() {
			return m_HSteamUser.ToString();
		}
	}
}
