using Steamworks.AnyCPU;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if !STEAMWORKS_ANYCPU
#error This file is Steamworks.NET.AnyCPU specific, not applicable to other vairant
#endif

namespace Steamworks
{
	internal partial class NativeMethods
	{
		static NativeMethods() {
			// ensure at least one dll resolver hook is applied to our assembly.
			try {
				NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, SteamNativeLibraryNameHelper.DefaultDllImportResolver);
			} catch (InvalidOperationException) {
				// consumer already set a DllImportResolver for this assembly, so we can't set ours.
				// This is fine, as long as they set a resolver which using SteamNativeLibraryNameHelper to resolve the Steamworks native library names.
				Debug.WriteLine("Steamworks.NET.AnyCPU: consumer defined DllImportResolver already set for this assembly. "
					+ "Ensure that the resolver uses SteamNativeLibraryNameHelper to resolve Steamworks native library names correctly.");
			}
		}
	}
}
