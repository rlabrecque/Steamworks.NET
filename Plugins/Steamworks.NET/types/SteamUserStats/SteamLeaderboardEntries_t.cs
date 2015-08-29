// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct SteamLeaderboardEntries_t : System.IEquatable<SteamLeaderboardEntries_t>, System.IComparable<SteamLeaderboardEntries_t> {
		public ulong m_SteamLeaderboardEntries;

		public SteamLeaderboardEntries_t(ulong value) {
			m_SteamLeaderboardEntries = value;
		}

		public override string ToString() {
			return m_SteamLeaderboardEntries.ToString();
		}

		public override bool Equals(object other) {
			return other is SteamLeaderboardEntries_t && this == (SteamLeaderboardEntries_t)other;
		}

		public override int GetHashCode() {
			return m_SteamLeaderboardEntries.GetHashCode();
		}

		public static bool operator ==(SteamLeaderboardEntries_t x, SteamLeaderboardEntries_t y) {
			return x.m_SteamLeaderboardEntries == y.m_SteamLeaderboardEntries;
		}

		public static bool operator !=(SteamLeaderboardEntries_t x, SteamLeaderboardEntries_t y) {
			return !(x == y);
		}

		public static explicit operator SteamLeaderboardEntries_t(ulong value) {
			return new SteamLeaderboardEntries_t(value);
		}

		public static explicit operator ulong(SteamLeaderboardEntries_t that) {
			return that.m_SteamLeaderboardEntries;
		}

		public bool Equals(SteamLeaderboardEntries_t other) {
			return m_SteamLeaderboardEntries == other.m_SteamLeaderboardEntries;
		}

		public int CompareTo(SteamLeaderboardEntries_t other) {
			return m_SteamLeaderboardEntries.CompareTo(other.m_SteamLeaderboardEntries);
		}
	}
}
