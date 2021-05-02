namespace Steamworks {
	[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)] // TODO: This is probably wrong, will likely crash on some platform.
	public delegate void SteamAPI_CheckCallbackRegistered_t(int iCallbackNum);
}

#endif // !DISABLESTEAMWORKS
