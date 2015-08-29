// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2015 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

namespace Steamworks {
	public struct ClientUnifiedMessageHandle : System.IEquatable<ClientUnifiedMessageHandle>, System.IComparable<ClientUnifiedMessageHandle> {
		public static readonly ClientUnifiedMessageHandle Invalid = new ClientUnifiedMessageHandle(0);
		public ulong m_ClientUnifiedMessageHandle;

		public ClientUnifiedMessageHandle(ulong value) {
			m_ClientUnifiedMessageHandle = value;
		}

		public override string ToString() {
			return m_ClientUnifiedMessageHandle.ToString();
		}

		public override bool Equals(object other) {
			return other is ClientUnifiedMessageHandle && this == (ClientUnifiedMessageHandle)other;
		}

		public override int GetHashCode() {
			return m_ClientUnifiedMessageHandle.GetHashCode();
		}

		public static bool operator ==(ClientUnifiedMessageHandle x, ClientUnifiedMessageHandle y) {
			return x.m_ClientUnifiedMessageHandle == y.m_ClientUnifiedMessageHandle;
		}

		public static bool operator !=(ClientUnifiedMessageHandle x, ClientUnifiedMessageHandle y) {
			return !(x == y);
		}

		public static explicit operator ClientUnifiedMessageHandle(ulong value) {
			return new ClientUnifiedMessageHandle(value);
		}

		public static explicit operator ulong(ClientUnifiedMessageHandle that) {
			return that.m_ClientUnifiedMessageHandle;
		}

		public bool Equals(ClientUnifiedMessageHandle other) {
			return m_ClientUnifiedMessageHandle == other.m_ClientUnifiedMessageHandle;
		}

		public int CompareTo(ClientUnifiedMessageHandle other) {
			return m_ClientUnifiedMessageHandle.CompareTo(other.m_ClientUnifiedMessageHandle);
		}
	}
}
