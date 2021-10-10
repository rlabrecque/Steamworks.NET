namespace Steamworks {
	[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.Cdecl)]
	public delegate void SteamInputActionEventCallbackPointer(IntPtr /* SteamInputActionEvent_t* */ SteamInputActionEvent);
}

#endif // !DISABLESTEAMWORKS
