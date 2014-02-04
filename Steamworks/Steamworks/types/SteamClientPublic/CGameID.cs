﻿namespace Steamworks {
	public struct CGameID : System.IEquatable<CGameID>, System.IComparable<CGameID> {
		public ulong m_GameID;

		public enum EGameIDType {
			k_EGameIDTypeApp = 0,
			k_EGameIDTypeGameMod = 1,
			k_EGameIDTypeShortcut = 2,
			k_EGameIDTypeP2P = 3,
		};

		public CGameID(ulong GameID) {
			m_GameID = GameID;
		}

		public CGameID(AppId_t nAppID) {
			m_GameID = 0;
			SetAppID(nAppID);
		}

		public CGameID(AppId_t nAppID, uint nModID) {
			m_GameID = 0;
			SetAppID(nAppID);
			SetType(EGameIDType.k_EGameIDTypeGameMod);
			SetModID(nModID);
		}

		public bool IsSteamApp() {
			return Type() == EGameIDType.k_EGameIDTypeApp;
		}

		public bool IsMod() {
			return Type() == EGameIDType.k_EGameIDTypeGameMod;
		}

		public bool IsShortcut() {
			return Type() == EGameIDType.k_EGameIDTypeShortcut;
		}

		public bool IsP2PFile() {
			return Type() == EGameIDType.k_EGameIDTypeP2P;
		}

		public AppId_t AppID() {
			return new AppId_t((uint)(m_GameID & 0xFFFFFFul));
		}

		public EGameIDType Type() {
			return (EGameIDType)((m_GameID >> 24) & 0xFFul);
		}

		public uint ModID() {
			return (uint)((m_GameID >> 32) & 0xFFFFFFFFul);
		}
		
		public bool IsValid() {
			// Each type has it's own invalid fixed point:
			switch (Type()) {
				case EGameIDType.k_EGameIDTypeApp:
					return AppID() != AppId_t.Invalid;

				case EGameIDType.k_EGameIDTypeGameMod:
					return AppID() != AppId_t.Invalid && (ModID() & 0x80000000) != 0;

				case EGameIDType.k_EGameIDTypeShortcut:
					return (ModID() & 0x80000000) != 0;

				case EGameIDType.k_EGameIDTypeP2P:
					return AppID() == AppId_t.Invalid && (ModID() & 0x80000000) != 0;

				default:
					return false;
			}
		}

		public void Reset() {
			m_GameID = 0;
		}

		public void Set(ulong GameID) {
			m_GameID = GameID;
		}

		#region Private Setters for internal use
		private void SetAppID(AppId_t other) {
			m_GameID = (m_GameID & ~(0xFFFFFFul << (ushort)0)) | (((ulong)(other) & 0xFFFFFFul) << (ushort)0);
		}

		private void SetType(EGameIDType other) {
			m_GameID = (m_GameID & ~(0xFFul << (ushort)24)) | (((ulong)(other) & 0xFFul) << (ushort)24);
		}

		private void SetModID(uint other) {
			m_GameID = (m_GameID & ~(0xFFFFFFFFul << (ushort)32)) | (((ulong)(other) & 0xFFFFFFFFul) << (ushort)32);
		}
		#endregion

		#region Overrides
		public override string ToString() {
			return m_GameID.ToString();
		}

		public override bool Equals(object other) {
			return other is CGameID && this == (CGameID)other;
		}

		public override int GetHashCode() {
			return m_GameID.GetHashCode();
		}

		public static bool operator ==(CGameID x, CGameID y) {
			return x.m_GameID == y.m_GameID;
		}

		public static bool operator !=(CGameID x, CGameID y) {
			return !(x == y);
		}

		public static explicit operator CGameID(ulong value) {
			return new CGameID(value);
		}
		public static explicit operator ulong(CGameID that) {
			return that.m_GameID;
		}

		public bool Equals(CGameID other) {
			return m_GameID == other.m_GameID;
		}

		public int CompareTo(CGameID other) {
			return m_GameID.CompareTo(other.m_GameID);
		}
		#endregion
	}
}
