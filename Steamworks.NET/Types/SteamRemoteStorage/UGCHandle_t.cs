namespace Steamworks {
	public struct UGCHandle_t {
		public static readonly UGCHandle_t Invalid = new UGCHandle_t(0xffffffffffffffff);
		public ulong m_UGCHandle;
		
		public UGCHandle_t(ulong value) {
			m_UGCHandle = value;
		}

		public override string ToString() {
			return m_UGCHandle.ToString();
		}
	}
}