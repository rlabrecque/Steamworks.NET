// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Changes to this file will be reverted when you update Steamworks.NET

#if !DISABLESTEAMWORKS

namespace Steamworks {
	[System.Runtime.InteropServices.UnmanagedFunctionPointer(System.Runtime.InteropServices.CallingConvention.StdCall)] // TODO: This is probably wrong, will likely crash on some platform.
	public delegate void SteamAPI_CheckCallbackRegistered_t(int iCallbackNum);
}

#endif // !DISABLESTEAMWORKS
