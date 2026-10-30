using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using static Steamworks.NativeMethods;
using static Steamworks.AnyCPU.SteamNativeLibraryNameHelper;

namespace Steamworks.AnyCPU.Tests
{
	public class NativeNameHelperTests
	{
		[Fact]
		[SupportedOSPlatform("windows")]
		public void Win_64Bit()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (Environment.Is64BitProcess)
				{
					Assert.Multiple([
						() => Assert.Equal("steam_api64", ResolvePlatformBinarySimpleFileName(NativeLibraryName)),
						() => Assert.Equal("sdkencryptedappticket64", ResolvePlatformBinarySimpleFileName(NativeLibrary_SDKEncryptedAppTicket)),
						() => Assert.Equal("steam_api64.dll", ResolvePlatformBinaryFileName(NativeLibraryName)),
						() => Assert.Equal("sdkencryptedappticket64.dll", ResolvePlatformBinaryFileName(NativeLibrary_SDKEncryptedAppTicket)),
						() => Assert.Throws<ArgumentException>(() => ResolvePlatformBinarySimpleFileName("KERNEL32"), (ex) => ex.Message.StartsWith(
							"Attempt to request arbitrary native library or directory name was included."
							) ? null : ex.ToString())
					]);
				}
				else
				{
					Assert.Skip("Skipped on win-x86");
				}
			}
			else
			{
				Assert.Skip("Skipped on non Windows.");
			}
		}

		[Fact]
		[SupportedOSPlatform("windows")]
		public void Win_x86()
		{

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (!Environment.Is64BitProcess)
				{
					Assert.Multiple([
						() => Assert.Equal("steam_api", ResolvePlatformBinarySimpleFileName(NativeLibraryName)),
						() => Assert.Equal("sdkencryptedappticket", ResolvePlatformBinarySimpleFileName(NativeLibrary_SDKEncryptedAppTicket)),
						() => Assert.Equal("steam_api.dll", ResolvePlatformBinaryFileName(NativeLibraryName)),
						() => Assert.Equal("sdkencryptedappticket.dll", ResolvePlatformBinaryFileName(NativeLibrary_SDKEncryptedAppTicket)),
						() => Assert.Throws<ArgumentException>(() => ResolvePlatformBinarySimpleFileName("KERNEL32"), (ex) => ex.Message.StartsWith(
							"Attempt to request arbitrary native library or directory name was included."
							) ? null : ex.ToString())
					]);
				}
				else
				{
					Assert.Skip("Skipped on win-x64");
				}
			}
			else
			{
				Assert.Skip("Skipped on non Windows.");
			}
		}

		[Fact]
		[SupportedOSPlatform("osx")]
		public void OSX()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				Assert.Multiple([
					() => Assert.Equal("steam_api", ResolvePlatformBinarySimpleFileName(NativeLibraryName)),
					() => Assert.Equal("sdkencryptedappticket", ResolvePlatformBinarySimpleFileName(NativeLibrary_SDKEncryptedAppTicket)),
					() => Assert.Equal("libsteam_api.dylib", ResolvePlatformBinaryFileName(NativeLibraryName)),
					() => Assert.Equal("libsdkencryptedappticket.dylib", ResolvePlatformBinaryFileName(NativeLibrary_SDKEncryptedAppTicket)),
					() => Assert.Throws<ArgumentException>(() => ResolvePlatformBinarySimpleFileName("libc"), (ex) => ex.Message.StartsWith(
						"Attempt to request arbitrary native library or directory name was included."
						) ? null : ex.ToString())
				]);
			}
			else
			{
				Assert.Skip("Skipped on non OSX.");
			}
		}

		[Fact]
		[SupportedOSPlatform("linux")]
		[SupportedOSPlatform("android")]
		public void Linux()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Assert.Multiple([
					() => Assert.Equal("steam_api", ResolvePlatformBinarySimpleFileName(NativeLibraryName)),
					() => Assert.Equal("sdkencryptedappticket", ResolvePlatformBinarySimpleFileName(NativeLibrary_SDKEncryptedAppTicket)),
					() => Assert.Equal("libsteam_api.so", ResolvePlatformBinaryFileName(NativeLibraryName)),
					() => Assert.Equal("libsdkencryptedappticket.so", ResolvePlatformBinaryFileName(NativeLibrary_SDKEncryptedAppTicket)),
					() => Assert.Throws<ArgumentException>(() => ResolvePlatformBinarySimpleFileName("libc"), (ex) => ex.Message.StartsWith(
						"Attempt to request arbitrary native library or directory name was included."
						) ? null : ex.ToString())
				]);
			}
			else
			{
				Assert.Skip("Skipped on non Linux.");
			}
		}
	}
}
