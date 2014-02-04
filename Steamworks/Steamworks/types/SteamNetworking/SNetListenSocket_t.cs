namespace Steamworks {
	public struct SNetListenSocket_t {
		public uint m_SNetListenSocket;

		public SNetListenSocket_t(uint value) {
			m_SNetListenSocket = value;
		}

		public override string ToString() {
			return m_SNetListenSocket.ToString();
		}
	}
}
