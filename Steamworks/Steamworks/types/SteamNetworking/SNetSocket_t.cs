namespace Steamworks {
	public struct SNetSocket_t {
		public uint m_SNetSocket;

		public SNetSocket_t(uint value) {
			m_SNetSocket = value;
		}

		public override string ToString() {
			return m_SNetSocket.ToString();
		}
	}
}
