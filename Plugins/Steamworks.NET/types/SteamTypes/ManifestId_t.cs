// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct ManifestId_t : System.IEquatable<ManifestId_t>, System.IComparable<ManifestId_t> {
		public static readonly ManifestId_t Invalid = new ManifestId_t(0x0);
		public ulong m_SteamAPICall;

		public ManifestId_t(ulong value) {
			m_SteamAPICall = value;
		}

		public override string ToString() {
			return m_SteamAPICall.ToString();
		}

		public override bool Equals(object other) {
			return other is ManifestId_t && this == (ManifestId_t)other;
		}

		public override int GetHashCode() {
			return m_SteamAPICall.GetHashCode();
		}

		public static bool operator ==(ManifestId_t x, ManifestId_t y) {
			return x.m_SteamAPICall == y.m_SteamAPICall;
		}

		public static bool operator !=(ManifestId_t x, ManifestId_t y) {
			return !(x == y);
		}

		public static explicit operator ManifestId_t(ulong value) {
			return new ManifestId_t(value);
		}
		public static explicit operator ulong(ManifestId_t that) {
			return that.m_SteamAPICall;
		}

		public bool Equals(ManifestId_t other) {
			return m_SteamAPICall == other.m_SteamAPICall;
		}

		public int CompareTo(ManifestId_t other) {
			return m_SteamAPICall.CompareTo(other.m_SteamAPICall);
		}
	}
}
