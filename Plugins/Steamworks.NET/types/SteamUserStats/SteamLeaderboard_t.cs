// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct SteamLeaderboard_t : System.IEquatable<SteamLeaderboard_t>, System.IComparable<SteamLeaderboard_t> {
		public ulong m_SteamLeaderboard;

		public SteamLeaderboard_t(ulong value) {
			m_SteamLeaderboard = value;
		}

		public override string ToString() {
			return m_SteamLeaderboard.ToString();
		}

		public override bool Equals(object other) {
			return other is SteamLeaderboard_t && this == (SteamLeaderboard_t)other;
		}

		public override int GetHashCode() {
			return m_SteamLeaderboard.GetHashCode();
		}

		public static bool operator ==(SteamLeaderboard_t x, SteamLeaderboard_t y) {
			return x.m_SteamLeaderboard == y.m_SteamLeaderboard;
		}

		public static bool operator !=(SteamLeaderboard_t x, SteamLeaderboard_t y) {
			return !(x == y);
		}

		public static explicit operator SteamLeaderboard_t(ulong value) {
			return new SteamLeaderboard_t(value);
		}

		public static explicit operator ulong(SteamLeaderboard_t that) {
			return that.m_SteamLeaderboard;
		}

		public bool Equals(SteamLeaderboard_t other) {
			return m_SteamLeaderboard == other.m_SteamLeaderboard;
		}

		public int CompareTo(SteamLeaderboard_t other) {
			return m_SteamLeaderboard.CompareTo(other.m_SteamLeaderboard);
		}
	}
}
