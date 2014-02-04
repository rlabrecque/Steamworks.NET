﻿namespace Steamworks {
	public struct DepotId_t : System.IEquatable<DepotId_t>, System.IComparable<DepotId_t> {
		public static readonly DepotId_t Invalid = new DepotId_t(0x0);
		public uint m_DepotId;

		public DepotId_t(uint value) {
			m_DepotId = value;
		}

		public override string ToString() {
			return m_DepotId.ToString();
		}

		public override bool Equals(object other) {
			return other is DepotId_t && this == (DepotId_t)other;
		}

		public override int GetHashCode() {
			return m_DepotId.GetHashCode();
		}

		public static bool operator ==(DepotId_t x, DepotId_t y) {
			return x.m_DepotId == y.m_DepotId;
		}

		public static bool operator !=(DepotId_t x, DepotId_t y) {
			return !(x == y);
		}

		public static explicit operator DepotId_t(uint value) {
			return new DepotId_t(value);
		}
		public static explicit operator uint(DepotId_t that) {
			return that.m_DepotId;
		}

		public bool Equals(DepotId_t other) {
			return m_DepotId == other.m_DepotId;
		}

		public int CompareTo(DepotId_t other) {
			return m_DepotId.CompareTo(other.m_DepotId);
		}
	}
}
