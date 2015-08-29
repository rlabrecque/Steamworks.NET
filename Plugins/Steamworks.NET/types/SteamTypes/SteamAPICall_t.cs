// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct SteamAPICall_t : System.IEquatable<SteamAPICall_t>, System.IComparable<SteamAPICall_t> {
		public static readonly SteamAPICall_t Invalid = new SteamAPICall_t(0x0);
		public ulong m_SteamAPICall;

		public SteamAPICall_t(ulong value) {
			m_SteamAPICall = value;
		}

		public override string ToString() {
			return m_SteamAPICall.ToString();
		}

		public override bool Equals(object other) {
			return other is SteamAPICall_t && this == (SteamAPICall_t)other;
		}

		public override int GetHashCode() {
			return m_SteamAPICall.GetHashCode();
		}

		public static bool operator ==(SteamAPICall_t x, SteamAPICall_t y) {
			return x.m_SteamAPICall == y.m_SteamAPICall;
		}

		public static bool operator !=(SteamAPICall_t x, SteamAPICall_t y) {
			return !(x == y);
		}

		public static explicit operator SteamAPICall_t(ulong value) {
			return new SteamAPICall_t(value);
		}

		public static explicit operator ulong(SteamAPICall_t that) {
			return that.m_SteamAPICall;
		}

		public bool Equals(SteamAPICall_t other) {
			return m_SteamAPICall == other.m_SteamAPICall;
		}

		public int CompareTo(SteamAPICall_t other) {
			return m_SteamAPICall.CompareTo(other.m_SteamAPICall);
		}
	}
}
