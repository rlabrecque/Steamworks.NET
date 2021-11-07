namespace Steamworks
{
	/// In a few places we need to set configuration options on listen sockets and connections, and
	/// have them take effect *before* the listen socket or connection really starts doing anything.
	/// Creating the object and then setting the options "immediately" after creation doesn't work
	/// completely, because network packets could be received between the time the object is created and
	/// when the options are applied.  To set options at creation time in a reliable way, they must be
	/// passed to the creation function.  This structure is used to pass those options.
	///
	/// For the meaning of these fields, see ISteamNetworkingUtils::SetConfigValue.  Basically
	/// when the object is created, we just iterate over the list of options and call
	/// ISteamNetworkingUtils::SetConfigValueStruct, where the scope arguments are supplied by the
	/// object being created.
	[System.Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct SteamNetworkingConfigValue_t
	{
		/// Which option is being set
		public ESteamNetworkingConfigValue m_eValue;

		/// Which field below did you fill in?
		public ESteamNetworkingConfigDataType m_eDataType;

		/// Option value
		public OptionValue m_val;

		[StructLayout(LayoutKind.Explicit)]
		public struct OptionValue
		{
			[FieldOffset(0)]
			public int m_int32;

			[FieldOffset(0)]
			public long m_int64;

			[FieldOffset(0)]
			public float m_float;

			[FieldOffset(0)]
			public IntPtr m_string; // Points to your '\0'-terminated buffer

			[FieldOffset(0)]
			public IntPtr m_functionPtr;
		}
	}
}

#endif // !DISABLESTEAMWORKS