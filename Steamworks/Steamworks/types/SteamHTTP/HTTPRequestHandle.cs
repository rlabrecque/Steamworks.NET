namespace Steamworks {
	public struct HTTPRequestHandle {
		public static readonly HTTPRequestHandle Invalid = new HTTPRequestHandle(0);
		public uint m_HTTPRequestHandle;

		public HTTPRequestHandle(uint value) {
			m_HTTPRequestHandle = value;
		}

		public override string ToString() {
			return m_HTTPRequestHandle.ToString();
		}
	}
}
