namespace Steamworks {
	public struct HServerListRequest : System.IEquatable<HServerListRequest> {
		public System.IntPtr m_HServerListRequest;

		public HServerListRequest(System.IntPtr value) {
			m_HServerListRequest = value;
		}

		public override string ToString() {
			return m_HServerListRequest.ToString();
		}

		public override bool Equals(object other) {
			return other is HServerListRequest && this == (HServerListRequest)other;
		}

		public override int GetHashCode() {
			return m_HServerListRequest.GetHashCode();
		}

		public static bool operator ==(HServerListRequest x, HServerListRequest y) {
			return x.m_HServerListRequest == y.m_HServerListRequest;
		}

		public static bool operator !=(HServerListRequest x, HServerListRequest y) {
			return !(x == y);
		}

		public static explicit operator HServerListRequest(System.IntPtr value) {
			return new HServerListRequest(value);
		}
		public static explicit operator System.IntPtr(HServerListRequest that) {
			return that.m_HServerListRequest;
		}

		public bool Equals(HServerListRequest other) {
			return m_HServerListRequest == other.m_HServerListRequest;
		}
	}
}
