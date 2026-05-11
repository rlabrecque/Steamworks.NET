namespace Steamworks
{
	//-----------------------------------------------------------------------------
	// Purpose: when callbacks are enabled this fires each time a controller action
	// state changes
	//-----------------------------------------------------------------------------
	[System.Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public struct SteamInputActionEvent_t
	{
		[FieldOffset(0)]
		public InputHandle_t controllerHandle;

		[FieldOffset(8)]
		public ESteamInputActionEventType eEventType;

		/// Option value
		[FieldOffset(12)]
		public OptionValue m_val;

		[System.Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct AnalogAction_t
		{
				public InputAnalogActionHandle_t actionHandle;

				public InputAnalogActionData_t analogActionData;
		}

		[System.Serializable]
		[StructLayout(LayoutKind.Sequential)]
		public struct DigitalAction_t
		{
				public InputDigitalActionHandle_t actionHandle;

				public InputDigitalActionData_t digitalActionData;
		}

		[System.Serializable]
		[StructLayout(LayoutKind.Explicit)]
		public struct OptionValue
		{
			[FieldOffset(0)]
			public AnalogAction_t analogAction;

			[FieldOffset(0)]
			public DigitalAction_t digitalAction;
		}
	}
}

#endif // !DISABLESTEAMWORKS