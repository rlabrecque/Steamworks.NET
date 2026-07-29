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
					// modify library name to x64 version
					libraryName = $"{libraryName}64";
				}

				if (!NativeLibrary.TryLoad(libraryName, assembly, searchPath, out nint lib)) {
					// godot specific search
					// in case of first chance search failed, build the full path of steam native, include extension name,
					// and try load again, this is for the case when steam native is not in default `dlopen()` search path
					// but in the same directory as the assembly.
					string extension;
					string nixPrefix = "lib";
					if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
						extension = ".dylib";
					else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						extension = ".dll";
					else
						extension = ".so"; // I can't imagine what else platforms other than linux that
										   // Steamworks.NET.AnyCPU will run on, but let's be future proof

					string searchDirectory = Path.GetDirectoryName(assembly.Location);

					if (string.IsNullOrEmpty(searchDirectory)) {
						System.Diagnostics.Debug.WriteLine("It seems you are loading Steamworks.NET.AnyCPU from memory," +
							" auto-detect steam native location is not possible," +
							" now trying to load from AppDomain.BaseDirectory." +
							" If still fails, please call" +
							" NativeLibrary.SetDllImporterResplver(typeof(Steamworks.SteamAPI).Assembly, YourResolver) manually.");

						searchDirectory = AppDomain.CurrentDomain.BaseDirectory;
					}

					string path = Path.Combine(searchDirectory, Path.ChangeExtension(nixPrefix + libraryName, extension));

					// second chance search, not caring failures anymore
					NativeLibrary.TryLoad(path, assembly, null, out lib);
				}

				return lib;
			}

			return 0;
		}
	}
}
#else
#error This file is Steamworks.NET.AnyCPU specific, not applicable to other vairant
#endif
