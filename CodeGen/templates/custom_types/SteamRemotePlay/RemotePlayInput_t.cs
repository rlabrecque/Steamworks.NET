namespace Steamworks
{
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = Packsize.value)]
	public struct RemotePlayInput_t
	{
		public RemotePlaySessionID_t m_unSessionID;

		public ERemotePlayInputType m_eType;

		/// Option value
		public OptionValue m_val;

		[StructLayout(LayoutKind.Explicit, Size = 56)]
		public struct OptionValue
		{
			// Mouse motion event data, valid when m_eType is k_ERemotePlayInputMouseMotion
			[FieldOffset(0)]
			public RemotePlayInputMouseMotion_t m_MouseMotion;

			// Mouse button event data, valid when m_eType is k_ERemotePlayInputMouseButtonDown or k_ERemotePlayInputMouseButtonUp
			[FieldOffset(0)]
			public ERemotePlayMouseButton m_eMouseButton;

			// Mouse wheel event data, valid when m_eType is k_ERemotePlayInputMouseWheel
			[FieldOffset(0)]
			public RemotePlayInputMouseWheel_t m_MouseWheel;

			// Key event data, valid when m_eType is k_ERemotePlayInputKeyDown or k_ERemotePlayInputKeyUp
			[FieldOffset(0)]
			public RemotePlayInputKey_t m_Key;

			// Unused space for future use
			//char padding[ 64 - ( sizeof( m_unSessionID ) + sizeof( m_eType ) ) ];
		}
	}
}

#endif // !DISABLESTEAMWORKS