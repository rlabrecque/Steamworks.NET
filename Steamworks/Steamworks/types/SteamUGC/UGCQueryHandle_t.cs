namespace Steamworks {
	public struct UGCQueryHandle_t {
		public static readonly UGCQueryHandle_t Invalid = new UGCQueryHandle_t(0xffffffffffffffff);
		public ulong m_UGCQueryHandle;

		public UGCQueryHandle_t(ulong value) {
			m_UGCQueryHandle = value;
		}

		public override string ToString() {
			return m_UGCQueryHandle.ToString();
		}
	}
}