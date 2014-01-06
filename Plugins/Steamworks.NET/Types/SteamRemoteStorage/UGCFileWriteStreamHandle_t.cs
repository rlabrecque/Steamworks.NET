namespace Steamworks {
	public struct UGCFileWriteStreamHandle_t {
		public static readonly UGCFileWriteStreamHandle_t Invalid = new UGCFileWriteStreamHandle_t(0xffffffffffffffff);
		public ulong m_UGCFileWriteStreamHandle;

		public UGCFileWriteStreamHandle_t(ulong value) {
			m_UGCFileWriteStreamHandle = value;
		}

		public override string ToString() {
			return m_UGCFileWriteStreamHandle.ToString();
		}
	}
}