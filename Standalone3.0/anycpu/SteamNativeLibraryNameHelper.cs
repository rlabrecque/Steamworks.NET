using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

#if !STEAMWORKS_ANYCPU
#error This file is Steamworks.NET.AnyCPU specific, not applicable to other vairant
#endif

namespace Steamworks.AnyCPU
{
	/// <summary>
	/// Advanced helper class to resolve the correct platform-specific binary filename for Steamworks native libraries
	/// from Steamworks.NET.AnyCPU internally defined library names on <see cref="DllImportAttribute"/>.
	/// </summary>
	/// <remarks>
	/// This class provided a sample in XMLDoc to help you <b>correctly</b> implement your own <see cref="DllImportResolver"/>
	/// for Steamworks native libraries against Steamworks.NET.AnyCPU's P/Invoke usages.<br/>
	/// Navigate to <see cref="SteamNativeLibraryNameHelper"/> in your IDE to read the sample code in XMLDoc, or check the source code of this class for more details.
	/// <para/>
	/// As a consumer you must set your resolver before any access to <see cref="Steamworks"/> other than class <see cref="SteamNativeLibraryNameHelper"/>,
	/// or <see cref="NativeLibrary"/> will lock the resolver table preventing your resolver from applying.<para/>
	/// For best practices, your resolver should be set as early as you can. Main() method is a good place to set your resolver, or early initialization code of your Steamworks manager class.<br/>	
	/// </remarks>
	/// <example>
	/// English explaination:
	/// <code>
	/// // Example resolver
	/// // In your own <see cref="DllImportResolver"/> method like this, you can use this helper to resolve the correct platform-specific binary filename for Steamworks native libraries.
	/// // such resolvers can be a static method or even a lambda, as you retrieve enough information to locate the binaries.
	/// IntPtr YourSteamworksDllImportResolver(string libraryName, System.Reflection.Assembly assembly, System.Runtime.InteropServices.DllImportSearchPath? searchPath)
	/// {
	///		// In your resolver, you can use this helper class to resolve the correct platform-specific binary filename for Steamworks native libraries.
	///		// To prevent misuses like resolve arbitrary native library, the helper will throw <see cref="ArgumentException"/>
	///		// if the library name is not from Steamworks.NET.AnyCPU.
	///		
	///		// DO NOT prepend or append any directory or extension to the library name,
	///		// just pass the P/Invoke library name as is from the <see cref="DllImportResolver"/> parameters.
	///		string platformSpecificBinaryFileName = SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(libraryName);
	///		
	///		string fullPathToYourLibrary = Path.Combine(GetYourNativeLibraryFolder(), platformSpecificBinaryFileName);
	///		
	///		if (!NativeLibrary.TryLoad(fullPathToYourLibrary, assembly, searchPath, out lib))
	///		{
	///			// throw desired exception or handle the error as you see fit
	///			// .NET Runtime provided resolution logic can't work in general, so you need to provide your own fallback resolution logic here.
	///		}
	///		
	///		return lib;
	///	}
	///	
	/// // Once you gather enough information to locate the binaries, you can set your resolver like this:
	/// NativeLibrary.SetDllImportResolver(typeof(SteamNativeLibraryNameHelper).Assembly, YourDllImportResolver);
	/// </code>
	/// <para/>
	/// For NativeAOT compiled Godot 4.7+ games with "Embed Build Outputs" unchecked,
	/// you can use the following resolver code to load Steamworks native libraries from the data directory:
	/// <code>
	/// // add this snippet into your Steamwork manager node's _Ready()
	/// if (!RuntimeFeature.IsDynamicCodeSupported)
	/// {
	///		NativeLibrary.SetDllImportResolver(typeof(SteamNativeLibraryNameHelper).Assembly, static (lib, asm, sp) => {
	///			const string dataDirectory = "data_Darmok_windows_x86_64"; // replace it with your own data directory
	///			NativeLibrary.TryLoad(Path.Combine(dataDirectory, SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(lib)), asm, sp, out IntPtr handle);
	///			return handle;
	///		});
	/// }
	/// // you are able to call SteamAPI.InitEx() now
	/// </code>
	/// <para/>
	/// <para>
	/// 中文解释：
	/// <code>
	/// // 示例Resolver的伪代码，需要在访问Steamworks.NET.AnyCPU任何类之前设置（本类除外），否则<see cref="NativeLibrary"/>会锁定Resolver表，
	/// // 然后在挂你的Resolver上去的时候抛出<see cref="InvalidOperationException"/>。
	/// // 必须调用<see cref="SteamNativeLibraryNameHelper"/>里面的辅助方法获取库的文件名。
	/// IntPtr YourSteamworksDllImportResolver(string libraryName, System.Reflection.Assembly assembly, System.Runtime.InteropServices.DllImportSearchPath? searchPath)
	/// {
	///		// 获取当前平台的文件名，用于拼接完整路径
	///		// 不能在这个时候对libraryName做任何修改，不能添加路径和扩展名，也不能传入其他库的路径。
	///		string platformSpecificBinaryFileName = SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(libraryName);
	///		// 拼接相对于主模块的路径，或完整路径
	///		string fullPathToYourLibrary = Path.Combine(Get原生库所在目录(), platformSpecificBinaryFileName);
	///		
	///		// 尝试用你提供的路径加载
	///		if (!NativeLibrary.TryLoad(fullPathToYourLibrary, assembly, searchPath, out lib))
	///		{
	///			// 抛出异常或进行其他错误处理
	///			// .NET 运行时无法为你兜底，因为我改了P/Invoke导入名，在前面加了"AnyCPU``"前缀。
	///		}
	///		
	///		return lib;
	///	}
	///	
	/// // 在获取足够用于拼接Steamworks库文件的信息之后，调用这个方法挂上你的Resolver：
	/// NativeLibrary.SetDllImportResolver(typeof(SteamNativeLibraryNameHelper).Assembly, YourDllImportResolver);
	/// </code>
	/// <para/>
	/// Godot 4.7+ NativeAOT编译的游戏，如果在导出时没有勾选"Embed Build Outputs"选项，建议参考以下代码：
	/// <code>
	/// // 在Steamworks API管理器节点的_Ready()中添加以下代码：
	/// if (!RuntimeFeature.IsDynamicCodeSupported)
	/// {
	///		NativeLibrary.SetDllImportResolver(typeof(SteamNativeLibraryNameHelper).Assembly, static (lib, asm, sp) => {
	///			const string dataDirectory = "data_新建游戏项目_windows_x86_64"; // 改成你的数据目录
	///			NativeLibrary.TryLoad(Path.Combine(dataDirectory, SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(lib)), asm, sp, out IntPtr handle);
	///			return handle;
	///		});
	/// }
	/// // 现在可以调用SteamAPI.InitEx()了
	/// </code>
	/// </para>
	/// </example>
	public static class SteamNativeLibraryNameHelper
	{
		/// <summary>
		/// Resolves the platform-specific binary name for the requested Steamworks native library for path concatenation, directly from the first parameter of DllImportResolver.
		/// </summary>
		/// <remarks>
		/// This method is designed to be used as a DllImportResolver for resolving the platform-specific binary file names for Steamworks native libraries.
		/// No path or directory information is included in the return value, nor pass any path or directory information to this method.
		/// The return value is intended for concatenation with a directory path to form the full path to the native library.
		/// </remarks>
		/// <param name="requestedSteamBinaryPInvokeName">The name of the Steamworks native library to resolve from this library. Must be obtained from the first parameter of <see cref="DllImportResolver"/>.</param>
		/// <returns>Simple module name that P/Invoke can use for first-chance search.</returns>
		/// <exception cref="ArgumentException">Attempt to request arbitrary native library.</exception>
		public static string ResolvePlatformBinarySimpleFileName(string requestedSteamBinaryPInvokeName) {
			if (requestedSteamBinaryPInvokeName != NativeMethods.NativeLibraryName && requestedSteamBinaryPInvokeName != NativeMethods.NativeLibrary_SDKEncryptedAppTicket) {
				throw new ArgumentException($"Attempt to request arbitrary native library or directory name was included. " +
					$"Only P/Invoke library names from {nameof(Steamworks)}.{nameof(NativeMethods)} are allowed.", nameof(requestedSteamBinaryPInvokeName));
			}

			// strip the synthetic prefix on every platform
			string simpleName = requestedSteamBinaryPInvokeName.Substring(NativeMethods.AnyCPUPrefix.Length);

			// only win64 binaries carry the "64" suffix
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.Is64BitProcess)
				simpleName += "64";

			return simpleName;
		}

		/// <summary>
		/// Resolves the platform-specific binary file name for the requested Steamworks native library for path concatenation, directly from the first parameter of DllImportResolver.
		/// The return value include the appropriate file extension and prefix based on the operating system.
		/// </summary>
		/// <remarks>
		/// This method is designed to be used as a DllImportResolver for resolving the platform-specific binary file names for Steamworks native libraries.
		/// No path or directory information is included in the return value, nor pass any path or directory information to this method.
		/// The return value is intended for concatenation with a directory path to form the full path to the native library.
		/// </remarks>
		/// <param name="requestedSteamBinaryPInvokeName">The name of the Steamworks native library to resolve from this library. Must be obtained from the first parameter of <see cref="DllImportResolver"/>.</param>
		/// <returns>The platform-specific binary file name.</returns>
		/// <exception cref="ArgumentException">Attempt to request arbitrary native library.</exception>
		public static string ResolvePlatformBinaryFileName(string requestedSteamBinaryPInvokeName) {
			string simpleName = ResolvePlatformBinarySimpleFileName(requestedSteamBinaryPInvokeName);
			string extension;
			string libFilenamePrefix = "lib";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				extension = ".dylib";
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				extension = ".dll";
				libFilenamePrefix = ""; // no prefix is applied on Windows
			} else
				extension = ".so"; // I can't imagine what else platforms other than linux that
								   // Steamworks.NET.AnyCPU will run on, but let's be future proof
			return Path.ChangeExtension(libFilenamePrefix + simpleName, extension);
		}

		internal static IntPtr DefaultDllImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath) {
			// check is requesting library name matches steam native
			// we don't check requester here because we want to ensure we are the first loader of steam native
			// otherwise other libraries may have already loaded steam native with wrong architecture
			if (libraryName == NativeMethods.NativeLibraryName || libraryName == NativeMethods.NativeLibrary_SDKEncryptedAppTicket) {
				// check are we on win64, the special case we are going to handle
				string librarySimpleName = ResolvePlatformBinarySimpleFileName(libraryName);

				if (!NativeLibrary.TryLoad(librarySimpleName, assembly, searchPath, out nint lib)) {
					// godot non-NAOT specific search
					// in case of first chance search failed, build the full path of steam native, include extension name,
					// and try load again, this is for the case when steam native is not in default `dlopen()` search path
					// but in the same directory as the assembly.
#pragma warning disable IL3000 // suppress warning about using Assembly.Location, we have fallback to AppBaseDirectory.
					string searchDirectory = Path.GetDirectoryName(assembly.Location);
#pragma warning restore IL3000
					if (string.IsNullOrEmpty(searchDirectory)) {
						Debug.WriteLine("It seems you are loading Steamworks.NET.AnyCPU from memory" +
							" or compiled into machine code by NativeAOT," +
							" auto-detect steam native location through assembly location is not possible," +
							" now trying to load from AppDomain.BaseDirectory.\n" +
							"If still fails, please edit your program to manually set your P/Invoke resolver at early stage by" +
							" calling `NativeLibrary.SetDllImportResolver(typeof(Steamworks.AnyCPU.SteamNativeLibraryNameHelper).Assembly, YourResolver)`." +
							" Use `class Steamworks.AnyCPU.SteamNativeLibraryNameHelper` to implement it appropriately.");

						searchDirectory = AppContext.BaseDirectory;
					}

					libraryName = ResolvePlatformBinaryFileName(libraryName);
					string path = Path.Combine(searchDirectory, libraryName);

					if (!NativeLibrary.TryLoad(path, assembly, null, out lib)) {
						throw new DllNotFoundException($"Failed to load native library: {libraryName}. Refer to debug output or `SteamNativeLibraryNameHelper.cs` source code for instructions.");
					}
				}

				return lib;
			}

			return 0;
		}
	}
}