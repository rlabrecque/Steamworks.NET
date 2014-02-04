using System.Runtime.InteropServices;

namespace Steamworks {
	public struct PublishedFileId_t {
		public ulong m_PublishedFileId;

		public PublishedFileId_t(ulong value) {
			m_PublishedFileId = value;
		}

		public override string ToString() {
			return m_PublishedFileId.ToString();
		}
	}
}