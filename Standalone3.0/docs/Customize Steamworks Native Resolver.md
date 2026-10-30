# Customizing the Steamworks Native Library Resolver

Steamworks.NET.AnyCPU loads its native Steamworks binaries through a `DllImportResolver` it installs on its own assembly.
This guide explains how that works, when you need to replace the default resolver, and how to implement your own
correctly with `Steamworks.AnyCPU.SteamNativeLibraryNameHelper`.

## Table of Contents

- [Overview](#overview)
- [Why the AnyCPU build needs a resolver](#why-the-anycpu-build-needs-a-resolver)
- [When you need a custom resolver](#when-you-need-a-custom-resolver)
- [Rules you must not break](#rules-you-must-not-break)
- [Step-by-step: implementing your own resolver](#step-by-step-implementing-your-own-resolver)
- [API reference](#api-reference)
- [What the default resolver does](#what-the-default-resolver-does)
- [Recipes](#recipes)
  - [A. Flat `native` folder](#a-flat-native-folder)
  - [B. Godot 4.7+ NativeAOT with unembedded build outputs](#b-godot-47-nativeaot-with-unembedded-build-outputs)
  - [C. RID subfolders next to the executable](#c-rid-subfolders-next-to-the-executable)
  - [D. Probing several directories with fallback](#d-probing-several-directories-with-fallback)
- [Troubleshooting](#troubleshooting)
- [See also](#see-also)

## Overview

Steamworks.NET.AnyCPU is an **any-CPU** managed assembly, but the Steamworks native libraries it P/Invokes into are
per-platform and per-architecture binaries:

| Platform / process | Native library on disk |
| --- | --- |
| Windows x86 | `steam_api.dll` |
| Windows x64 | `steam_api64.dll` |
| Linux x64 / arm64 | `libsteam_api.so` |
| macOS x64 / arm64 | `libsteam_api.dylib` |

The managed assembly cannot know at compile time which file to bind to, so it imports its P/Invokes under **synthetic
library names** and translates those names into real file names at runtime, right before the first P/Invoke, through a
`DllImportResolver` registered on the Steamworks.NET.AnyCPU assembly.

A default resolver is installed automatically and covers the common cases (.NET SDK produced `runtimes/<rid>/` layout, files
next to the managed assembly, or files in the application base directory). You only need this guide if that default
does not fit your deployment.

## Why the AnyCPU build needs a resolver

### The P/Invoke library names are synthetic

In the regular (non-AnyCPU) builds the `DllImport` library names are already real file names, so .NET's built-in
probing can find the binaries by itself. The AnyCPU build instead prefixes every Steamworks P/Invoke name with
`AnyCPU` followed by two backticks:

```csharp
// NativeMethods (STEAMWORKS_ANYCPU)
internal const string AnyCPUPrefix = "AnyCPU``";
internal const string NativeLibraryName = AnyCPUPrefix + "steam_api";                       // AnyCPU``steam_api
internal const string NativeLibrary_SDKEncryptedAppTicket = AnyCPUPrefix + "sdkencryptedappticket";
```

| Build configuration | `DllImport` library name used by `SteamAPI` |
| --- | --- |
| `Steamworks.NET` with `STEAMWORKS_WIN` + `STEAMWORKS_X64` | `steam_api64` |
| `Steamworks.NET` (all other platforms) | `steam_api` |
| `Steamworks.NET.AnyCPU` | ``` AnyCPU``steam_api ``` |

Because no file is ever named ``` AnyCPU``steam_api ```, .NET's default resolution logic can never succeed on its own.
The prefix exists on purpose:

- it guarantees the AnyCPU wrapper is the **only** decision point for which Steamworks binary gets loaded, so the
  runtime can never silently bind a P/Invoke to an unrelated `steam_api64` (wrong architecture or wrong wrapper
  version) that happens to be on the OS search path;
- it makes the AnyCPU wrapper the **first** loader of the Steamworks native library, which matters because loading a
  Steamworks binary of the wrong architecture first is not recoverable in-process.

That design is precisely why .NET cannot provide a fallback for you: **your resolver must translate the name itself.**

### The helper does the translation

`SteamNativeLibraryNameHelper` exposes two pure functions that turn the resolver's `libraryName` argument into a
platform-specific file name. It never touches the file system and never includes directory information in its result.

| `libraryName` input | `ResolvePlatformBinarySimpleFileName` | `ResolvePlatformBinaryFileName` |
| --- | --- | --- |
| ``` AnyCPU``steam_api ``` on Windows, 64-bit process | `steam_api64` | `steam_api64.dll` |
| ``` AnyCPU``steam_api ``` on Windows, 32-bit process | `steam_api` | `steam_api.dll` |
| ``` AnyCPU``steam_api ``` on Linux (x64/arm64) | `steam_api` | `libsteam_api.so` |
| ``` AnyCPU``steam_api ``` on macOS (x64/arm64) | `steam_api` | `libsteam_api.dylib` |
| ``` AnyCPU``sdkencryptedappticket ``` on Windows, 64-bit process | `sdkencryptedappticket64` | `sdkencryptedappticket64.dll` |
| anything else | throws `ArgumentException` | throws `ArgumentException` |

Use `ResolvePlatformBinarySimpleFileName` when you want .NET's own search path (deps.json, `runtimes/<rid>/`, OS
loader path) to do the loading, and `ResolvePlatformBinaryFileName` when you build an explicit path to a file yourself.

**Please note the internals of `SteamNativeLibraryNameHelper` is implementation detail and is subject to change. Do not rely on these behavior.**

## When you need a custom resolver

You need a custom resolver when the default one cannot find your binaries or cannot run:

- Experimental Godot 4.7+ **NativeAOT** builds, especially **exported with "Embed Build Outputs" unchecked**.
  In this case `Assembly.Location` is empty but `AppContext.BaseDirectory` are also incorrect. Non-NativeAOT published games works, the Godot
  sets `data_<project>_<platform>` directory as `AppContext.BaseDirectory` which in a fallback path of our default resolver. 
- **Single-file / self-extracting publish**, ILRepack/ILMerge, or `Assembly.Load(byte[])` style loading.
- Your native binaries are **not** in a location .NET probes: a custom `native/` folder, a plug-in directory, 
  an asset bundle you unpack at startup, and so on.
- Your application is itself a **plug-in or host** that must control exactly *when* the Steamworks native library is
  loaded (for example, to guarantee the correct architecture is loaded first).
- You hit the default resolver's `DllNotFoundException` message, or you want a clearer failure than a debug trace.

You do **not** need a custom resolver for an ordinary .NET app that references the NuGet package and leaves the native
binaries in the standard `runtimes/<rid>/native` layout — that is already handled.

## Rules you must not break

1. **Register your resolver before any Steamworks access**, other than `SteamNativeLibraryNameHelper` itself. The
   default resolver is registered from `NativeMethods`' static constructor on the first Steamworks P/Invoke, and
   `NativeLibrary.SetDllImportResolver` throws `InvalidOperationException` when a resolver is already registered for
   the assembly. Registering in `Main()` or in your Steamworks manager's initialization is early enough.
2. **Register on the Steamworks assembly, not on yours.** The resolver is keyed by assembly, and it is only consulted
   for P/Invokes declared in that assembly. Always pass `typeof(SteamNativeLibraryNameHelper).Assembly`.
3. **Never modify the `libraryName` before handing it to the helper.** Do not prepend a directory, do not append an
   extension, and do not translate it to a file name yourself. Feed the resolver's first parameter through unchanged.
4. **Resolve both native libraries.** `steam_api` *and* `sdkencryptedappticket` are requested by the assembly. Using
   the helper covers both automatically; hard-coding one file name does not.
5. **Return a valid `IntPtr`, or return `IntPtr.Zero` to delegate.** Returning zero hands the name back to .NET's
   default resolution logic, which will fail for the synthetic names — so only do it deliberately, for example as the
   last branch of a multi-directory fallback. If you want a deterministic error, throw `DllNotFoundException` yourself.
6. **Never call the helper on a name that is not a Steamworks P/Invoke name.** The helper throws `ArgumentException`
   for arbitrary names to prevent misuse; that is a bug in your resolver, not a runtime error to swallow.

## Step-by-step: implementing your own resolver

### 1. Choose the registration time

Pick the earliest point in your program where you know where the native binaries are: `Main()`, a module initializer, or
the constructor / `_Ready()` of your Steamworks manager. It must run **before** the first call to any Steamworks API.

### 2. Implement the resolver

```csharp
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Steamworks.AnyCPU;

internal static class SteamworksNativeBootstrap
{
	// Wherever you ship the native libraries, relative to the executable.
	private const string NativeSubdirectory = "native";

	public static void RegisterResolver()
	{
		NativeLibrary.SetDllImportResolver(
			typeof(SteamNativeLibraryNameHelper).Assembly,
			Resolve);
	}

	private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		// 1. Translate the synthetic P/Invoke name into a real, platform-specific file name.
		//    Do not touch libraryName first: pass the resolver parameter through as-is.
		string platformSpecificFileName = SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(libraryName);

		// 2. Combine it with the directory you ship the binaries in.
		string fullPath = Path.Combine(AppContext.BaseDirectory, NativeSubdirectory, platformSpecificFileName);

		// 3. Let the runtime perform the actual load, forwarding the original searchPath.
		if (NativeLibrary.TryLoad(fullPath, assembly, searchPath, out IntPtr handle))
		{
			return handle;
		}

		// 4. Return zero to delegate to .NET's default resolution logic, or throw your own
		//    DllNotFoundException to fail fast with a useful message.
		return IntPtr.Zero;
	}
}
```

### 3. Register it before touching Steamworks

```csharp
internal static class Program
{
	private static void Main()
	{
		SteamworksNativeBootstrap.RegisterResolver();

		if (SteamAPI.InitEx(out string errorMessage) != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
		{
			Console.Error.WriteLine($"SteamAPI init failed: {errorMessage}");
			return;
		}

		// ... run your application ...
	}
}
```

### 4. Verify

- The first Steamworks call must not throw `DllNotFoundException` or `InvalidOperationException`.
- With a debug listener attached you **should** see
  `Steamworks.NET.AnyCPU: consumer defined DllImportResolver already set for this assembly.` — that message means you correctly registered
   before the default one.
- Confirm the handle you returned belongs to the right architecture. Loading a 32-bit `steam_api.dll` into a 64-bit
  process can succeed at load time and fail later during initialization, so always point your resolver at a directory
  that matches the process architecture.

## API reference

All members live on `Steamworks.AnyCPU.SteamNativeLibraryNameHelper` (a `public static` class in the
Steamworks.NET.AnyCPU assembly).

### `ResolvePlatformBinarySimpleFileName(string requestedSteamBinaryPInvokeName)`

Returns the bare module name to hand to a first-chance search — for example `steam_api64` on Windows x64, `steam_api`
elsewhere. No directory and no file extension are included, and none may be supplied as input.

- **`requestedSteamBinaryPInvokeName`** — must be the resolver's `libraryName` parameter, unmodified.
- **Returns** — a simple module name such as `steam_api64` or `sdkencryptedappticket64`.
- **Throws `ArgumentException`** — when the name does not come from Steamworks.NET.AnyCPU, when a directory or extension
  was introduced, or when any other native library is requested. This is the misuse guard described above.

Use this overload when you want the runtime to search for the library itself (for example with
`NativeLibrary.TryLoad(simpleName, assembly, searchPath, out handle)`), relying on deps.json, the RID-specific
`runtimes/<rid>/` folders, and the OS loader search path.

### `ResolvePlatformBinaryFileName(string requestedSteamBinaryPInvokeName)`

Returns the platform-specific file name, including the `lib` prefix on Linux/macOS and the correct extension
(`.dll`, `.so`, or `.dylib`). Still no directory information.

- **`requestedSteamBinaryPInvokeName`** — must be the resolver's `libraryName` parameter, unmodified.
- **Returns** — a file name such as `steam_api64.dll`, `libsteam_api.so`, or `libsteam_api.dylib`.
- **Throws `ArgumentException`** — as above.

Use this overload when you build an explicit path with `Path.Combine` and load it directly. This is the correct choice
whenever the native binaries are outside any path .NET probes by default.

## What the default resolver does

The internal `SteamNativeLibraryNameHelper.DefaultDllImportResolver` is installed automatically from `NativeMethods`'
static constructor. If you register your own resolver first, the automatic registration fails with
`InvalidOperationException`, which the library catches and reports through `Debug.WriteLine`:

```text
Steamworks.NET.AnyCPU: consumer defined DllImportResolver already set for this assembly. Ensure that the resolver uses
SteamNativeLibraryNameHelper to resolve Steamworks native library names.
```

The default resolver is fine to replace, but it is useful to know what it reproduces, because a custom resolver that
forgets a step behaves worse than the default in that step:

1. Translate `libraryName` with `ResolvePlatformBinarySimpleFileName` (which also applies the `64` suffix on Windows
   64-bit processes).
2. First-chance load: `NativeLibrary.TryLoad(simpleName, assembly, searchPath, out handle)`. This lets deps.json's
   `runtimes/<rid>/native` resolution and the OS loader search path find the file.
3. On failure, compute a search directory:
   - `Path.GetDirectoryName(assembly.Location)` — the managed assembly's own directory; or
   - `AppContext.BaseDirectory` when `Assembly.Location` is empty (assembly loaded from memory, or
     NativeAOT compiled to machine code), after writing an explanatory debug message.
4. Build the full path with `ResolvePlatformBinaryFileName` and retry with
   `NativeLibrary.TryLoad(path, assembly, null, out handle)`.
5. If that fails too, throw `DllNotFoundException` whose message points back at `SteamNativeLibraryNameHelper.cs`:

```text
Failed to load native library: steam_api64.dll. Refer to debug output or `SteamNativeLibraryNameHelper.cs` source code for instructions.
```

Non-Steamworks library names are ignored and return `0` so other resolvers (or the default logic) can handle them.

## Recipes

### A. Flat `native` folder

See [Step-by-step: implementing your own resolver](#step-by-step-implementing-your-own-resolver). Combine the file name
with `AppContext.BaseDirectory` so the result does not depend on the process working directory.

### B. Godot 4.7+ NativeAOT with unembedded build outputs

When a Godot game is exported with NativeAOT and **"Embed Build Outputs" is unchecked**, the native libraries live in
the Godot data directory (`data_<project>_<platform>`) rather than next to the executable, and `Assembly.Location` is
empty. Register the resolver from your Steamworks manager node's `_Ready()` before any Steamworks call:

```csharp
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using Steamworks.AnyCPU;

public partial class SteamManager : Node
{
	public override void _Ready()
	{
		if (!RuntimeFeature.IsDynamicCodeSupported) // true only when running under NativeAOT
		{
			const string dataDirectory = "data_Darmok_windows_x86_64"; // replace with your own data directory

			NativeLibrary.SetDllImportResolver(
				typeof(SteamNativeLibraryNameHelper).Assembly,
				(libraryName, assembly, searchPath) =>
				{
					string path = Path.Combine(
						AppContext.BaseDirectory,
						dataDirectory,
						SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(libraryName));

					return NativeLibrary.TryLoad(path, assembly, searchPath, out IntPtr handle) ? handle : 0;
				});
		}

		// You are now able to initialize Steamworks.
		if (SteamAPI.InitEx(out string errorMessage) != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
		{
			GD.PrintErr($"SteamAPI init failed: {errorMessage}");
		}
	}
}
```

`RuntimeFeature.IsDynamicCodeSupported` is the reliable runtime check for "compiled by NativeAOT"; do not test it at
compile time with `#if`, because the same source can be compiled both with and without NativeAOT.

Note that `Path.Combine(AppContext.BaseDirectory, ...)` is used instead of a bare relative path: Godot may change the
process working directory, and a relative path handed to `NativeLibrary.TryLoad` is resolved against the working
directory, not the executable.

### C. RID subfolders next to the executable

If you ship one directory per runtime identifier (`native/win-x64`, `native/linux-x64`, `native/osx-arm64`, ...), pick
the subfolder from the current platform instead of guessing:

```csharp
private static string GetRuntimeIdentifier()
{
	if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		return Environment.Is64BitProcess ? "win-x64" : "win-x86";

	if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";

	return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
}

private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
{
	string path = Path.Combine(
		AppContext.BaseDirectory,
		"native",
		GetRuntimeIdentifier(),
		SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(libraryName));

	return NativeLibrary.TryLoad(path, assembly, searchPath, out IntPtr handle) ? handle : 0;
}
```

### D. Probing several directories with fallback

When the same application can be deployed in more than one layout (for example, an installed game and a developer
checkout), try each location in order and fail with a message that lists what was tried:

```csharp
private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
{
	string fileName = SteamNativeLibraryNameHelper.ResolvePlatformBinaryFileName(libraryName);

	string[] candidateDirectories =
	{
		Path.Combine(AppContext.BaseDirectory, "native"),
		Path.Combine(AppContext.BaseDirectory, "runtimes", GetRuntimeIdentifier(), "native"), // see Recipe C
		AppContext.BaseDirectory,
	};

	foreach (string directory in candidateDirectories)
	{
		if (NativeLibrary.TryLoad(Path.Combine(directory, fileName), assembly, searchPath, out IntPtr handle))
		{
			return handle;
		}
	}

	throw new DllNotFoundException(
		$"Could not locate '{fileName}' in any of: {string.Join(", ", candidateDirectories)}.");
}
```

Because this resolver always throws instead of returning `0`, a misconfigured deployment fails immediately with the list
of directories that were tried, which is usually more useful than the runtime's own error.

## Troubleshooting

| Symptom | Likely cause | Fix |
| --- | --- | --- |
| `InvalidOperationException`: a resolver is already registered for the assembly | You registered after a Steamworks P/Invoke had already run, so the default resolver was installed first | Move `SetDllImportResolver` into `Main()` or your earliest initialization code |
| `DllNotFoundException` from the default resolver's message | The binaries are not in a probed location, and your resolver was never registered | Register a custom resolver, or place the binaries next to the managed assembly / in `AppContext.BaseDirectory` / in `$"{AppContext.BaseDirectory}/runtimes/{rid}/"` |
| `DllNotFoundException` from your own path | Wrong directory, wrong file name, or the file is missing for the current platform | Log the full path you computed and confirm the exact spelling, including the `lib` prefix and extension |
| `ArgumentException`: "Only P/Invoke library names from `Steamworks.NativeMethods` are allowed" | You passed a modified name into the helper (added a directory or extension), or you fed it a non-Steamworks name | Pass the resolver's `libraryName` parameter through untouched, and do not use the helper for your own libraries |
| Libraries resolve, but Steamworks initialization fails or crashes | A binary of the wrong architecture was loaded (for example, a 32-bit `steam_api.dll` in a 64-bit process) | Point the resolver at an architecture-matched directory; check `Environment.Is64BitProcess` and `RuntimeInformation.ProcessArchitecture` |
| Debug output: "DllImportResolver already set for this assembly." | The library detected an existing resolver — either yours (expected) or one installed by other code | Verify the registered resolver uses `SteamNativeLibraryNameHelper`, and keep only one resolver per assembly |
| Steamworks APIs are called but nothing appears to load | The resolver was registered against your own assembly instead of the Steamworks assembly | Use `typeof(SteamNativeLibraryNameHelper).Assembly` |
| `Assembly.Location` is an empty string | The assembly was loaded from memory or compiled by NativeAOT | Do not rely on `Assembly.Location`; resolve against `AppContext.BaseDirectory` or a data directory (see [Recipe B](#b-godot-47-nativeaot-with-unembedded-build-outputs)) |

## See also

- [`SteamNativeLibraryNameHelper.cs`](../anycpu/SteamNativeLibraryNameHelper.cs) — the helper and its XMLDoc sample,
  including the Chinese-language explanation.
- [`NativeMethods.AnyCPU.cs`](../anycpu/autogen/NativeMethods.AnyCPU.cs) — where the default resolver is registered and
  where the synthetic library names come from.
- [`CONTRIBUTING.md`](../CONTRIBUTING.md) — project structure, build instructions, and coding standards.
- [NativeLibrary.SetDllImportResolver](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.nativelibrary.setdllimportresolver)
- [DllImportResolver delegate](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.dllimportresolver)
- [NativeLibrary.TryLoad](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.nativelibrary.tryload)

<!-- 
Heart from Cyberstan, no AI was hurt during the writing process🤖.
-->