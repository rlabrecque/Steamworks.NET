namespace Steamworks {
	public struct PublishedFileUpdateHandle_t {
		public static readonly PublishedFileUpdateHandle_t Invalid = new PublishedFileUpdateHandle_t(0xffffffffffffffff);
		public ulong m_PublishedFileUpdateHandle;

		public PublishedFileUpdateHandle_t(ulong value) {
			m_PublishedFileUpdateHandle = value;
		}

		public override string ToString() {
			return m_PublishedFileUpdateHandle.ToString();
		}
	}
}