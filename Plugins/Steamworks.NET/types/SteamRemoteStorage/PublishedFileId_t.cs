// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

using System.Runtime.InteropServices;

namespace Steamworks {
	public struct PublishedFileId_t : System.IEquatable<PublishedFileId_t>, System.IComparable<PublishedFileId_t> {
		public static readonly PublishedFileId_t Invalid = new PublishedFileId_t(0);
		public ulong m_PublishedFileId;

		public PublishedFileId_t(ulong value) {
			m_PublishedFileId = value;
		}

		public override string ToString() {
			return m_PublishedFileId.ToString();
		}

		public override bool Equals(object other) {
			return other is PublishedFileId_t && this == (PublishedFileId_t)other;
		}

		public override int GetHashCode() {
			return m_PublishedFileId.GetHashCode();
		}

		public static bool operator ==(PublishedFileId_t x, PublishedFileId_t y) {
			return x.m_PublishedFileId == y.m_PublishedFileId;
		}

		public static bool operator !=(PublishedFileId_t x, PublishedFileId_t y) {
			return !(x == y);
		}

		public static explicit operator PublishedFileId_t(ulong value) {
			return new PublishedFileId_t(value);
		}
		public static explicit operator ulong(PublishedFileId_t that) {
			return that.m_PublishedFileId;
		}

		public bool Equals(PublishedFileId_t other) {
			return m_PublishedFileId == other.m_PublishedFileId;
		}

		public int CompareTo(PublishedFileId_t other) {
			return m_PublishedFileId.CompareTo(other.m_PublishedFileId);
		}
	}
}
