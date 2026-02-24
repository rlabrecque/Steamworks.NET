using System;
using System.IO;
using System.Runtime.InteropServices;

#if STEAMWORKS_ANYCPU

namespace Steamworks
{
	internal partial class NativeMethods
	{
		static NativeMethods() {
			NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
		}

		private static IntPtr DllImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath) {
			// check is requesting library name matches steam native
			// we don't check requester here because we want to ensure we are the first loader of steam native
			// otherwise other libraries may have already loaded steam native with wrong architecture
			if (libraryName == NativeLibraryName || libraryName == NativeLibrary_SDKEncryptedAppTicket) {
				// check are we on win64, the special case we are going to handle
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.Is64BitProcess) {
					// check who is requesting steam native
					if (assembly.GetName().Name != "Steamworks.NET") {
						// Unmanaged libraries(steam native dll) will be cached(probably by name),
						// we warn developers here the steam native will be cached for any later loads, this is a potentional pollution.
						System.Diagnostics.Debug.WriteLine(
							$"[Warning] Assembly {assembly.GetName().Name} is requesting Steam native by it's original name, " +
							$"but Steamworks.NET.AnyCPU want to load x64 version of steam library \"{libraryName}\". The loaded Steam native will be cached " +
							$"and may potentially break it.\n" +
							$"Affected assembly's full name: {assembly.FullName}");

						return 0;
					}

					// platform specific suffix is not needed, to reuse default unmanaged dependencies resolve logic on win64
					string x64LibName = $"{libraryName}64";
					NativeLibrary.TryLoad(x64LibName, assembly, searchPath, out nint lib);

					return lib;
				} else {
					// first chance search, if failed or specified, check the assembly directory for steam natives
					if (searchPath is DllImportSearchPath.AssemblyDirectory || !NativeLibrary.TryLoad(libraryName, assembly, searchPath, out nint lib)) {
						// in case of first chance search failed, build the full path of steam native, include extension name,
						// and try load again, this is for the case when steam native is not in default `dlopen()` search path
						// but in the same directory as the assembly.
						string extension;
						string nixPrefix = "lib";
						if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
							extension = ".dylib";
						else
							extension = ".so"; // I can't imagine what else platforms other than linux that
											   // Steamworks.NET.AnyCPU will run on, but let's be future proof

						string assemblyLocation = assembly.Location;

						if (assemblyLocation == "") {
							System.Diagnostics.Debug.WriteLine("It seems you are loading Steamworks.NET.AnyCPU from memory," +
								" auto-detect steam native location is not possible," +
								" please manually load steam natives into correct ALC.");
						}

						string path = Path.Combine(assemblyLocation, Path.ChangeExtension(nixPrefix + libraryName, extension));

						// second chance or user specified behavior search, not caring failures anymore
						NativeLibrary.TryLoad(path, assembly, searchPath, out lib);
					}

					return lib;
				}
			}
			return 0;
		}
	}
}
#else
#error This file is Steamworks.NET.AnyCPU specific, not applicable to other vairant
#endif
