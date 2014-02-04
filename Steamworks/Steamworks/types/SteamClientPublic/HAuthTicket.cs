namespace Steamworks {
	public struct HAuthTicket : System.IEquatable<HAuthTicket>, System.IComparable<HAuthTicket> {
		public static readonly HAuthTicket Invalid = new HAuthTicket(0);
		public uint m_HAuthTicket;

		public HAuthTicket(uint value) {
			m_HAuthTicket = value;
		}

		public override string ToString() {
			return m_HAuthTicket.ToString();
		}

		public override bool Equals(object other) {
			return other is HAuthTicket && this == (HAuthTicket)other;
		}

		public override int GetHashCode() {
			return m_HAuthTicket.GetHashCode();
		}

		public static bool operator ==(HAuthTicket x, HAuthTicket y) {
			return x.m_HAuthTicket == y.m_HAuthTicket;
		}

		public static bool operator !=(HAuthTicket x, HAuthTicket y) {
			return !(x == y);
		}

		public bool Equals(HAuthTicket other) {
			return m_HAuthTicket == other.m_HAuthTicket;
		}

		public int CompareTo(HAuthTicket other) {
			return m_HAuthTicket.CompareTo(other.m_HAuthTicket);
		}
	}
}
