// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct HServerListRequest : System.IEquatable<HServerListRequest> {
		public static readonly HServerListRequest Invalid = new HServerListRequest(System.IntPtr.Zero);
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
