namespace Steamworks {
	/// Setup callback for debug output, and the desired verbosity you want.
	[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
	public delegate void FSteamNetworkingSocketsDebugOutput(ESteamNetworkingSocketsDebugOutputType nType, System.Text.StringBuilder pszMsg);
}

#endif // !DISABLESTEAMWORKS
