namespace Steamworks
{
	/// Network-routable identifier for a service.  This is an intentionally
	/// opaque byte blob.  The relays know how to use this to forward it on
	/// to the intended destination, but otherwise clients really should not
	/// need to know what's inside.  (Indeed, we don't really want them to
	/// know, as it could reveal information useful to an attacker.)
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
	public struct SteamDatagramHostedAddress
	{
		// Size of data blob.
		public int m_cbSize;

		// Opaque
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
		public byte[] m_data;

		// Reset to empty state
		public void Clear()
		{
			m_cbSize = 0;
			m_data = new byte[128];
		}
	}
}

#endif // !DISABLESTEAMWORKS