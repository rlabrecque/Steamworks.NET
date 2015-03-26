// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct FriendsGroupID_t : System.IEquatable<FriendsGroupID_t>, System.IComparable<FriendsGroupID_t> {
		public static readonly FriendsGroupID_t Invalid = new FriendsGroupID_t(-1);
		public short m_FriendsGroupID_t;

		public FriendsGroupID_t(short value) {
			m_FriendsGroupID_t = value;
		}

		public override string ToString() {
			return m_FriendsGroupID_t.ToString();
		}

		public override bool Equals(object other) {
			return other is FriendsGroupID_t && this == (FriendsGroupID_t)other;
		}

		public override int GetHashCode() {
			return m_FriendsGroupID_t.GetHashCode();
		}

		public static bool operator ==(FriendsGroupID_t x, FriendsGroupID_t y) {
			return x.m_FriendsGroupID_t == y.m_FriendsGroupID_t;
		}

		public static bool operator !=(FriendsGroupID_t x, FriendsGroupID_t y) {
			return !(x == y);
		}

		public static explicit operator FriendsGroupID_t(short value) {
			return new FriendsGroupID_t(value);
		}

		public static explicit operator short(FriendsGroupID_t that) {
			return that.m_FriendsGroupID_t;
		}

		public bool Equals(FriendsGroupID_t other) {
			return m_FriendsGroupID_t == other.m_FriendsGroupID_t;
		}

		public int CompareTo(FriendsGroupID_t other) {
			return m_FriendsGroupID_t.CompareTo(other.m_FriendsGroupID_t);
		}
	}
}
