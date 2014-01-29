namespace Steamworks {
	public struct ClientUnifiedMessageHandle {
		public static readonly ClientUnifiedMessageHandle Invalid = new ClientUnifiedMessageHandle(0);
		public ulong m_ClientUnifiedMessageHandle;

		public ClientUnifiedMessageHandle(ulong value) {
			m_ClientUnifiedMessageHandle = value;
		}

		public override string ToString() {
			return m_ClientUnifiedMessageHandle.ToString();
		}
	}
}
