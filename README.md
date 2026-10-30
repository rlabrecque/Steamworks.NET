# Steamworks.NET.AnyCPU
Steamworks.NET.AnyCPU is a .NET 8+ library that provides a C# wrapper for Valve's Steamworks API, develivered through [NuGet packages](https://www.nuget.org/packages/Steamworks.NET.AnyCPU/). It is designed to be used in most .NET 8+ application, including .NET scripted games, Steamworks supported .NET(Core) game servers, and other applications that require integration with Steamworks features. The library is compatible with Windows, macOS, and Linux platforms. Android support is not tested.

This library is forked from upstream [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) and is maintained by @Akarainnnn with the help of the community. It is licensed under the MIT License, which allows for free use, modification, and distribution of the software.

## Installation

### .NET 8+ Projects including Godot and MonoGame
Reference the [Steamworks.NET.AnyCPU NuGet package](https://www.nuget.org/packages/Steamworks.NET.AnyCPU/) for SDK style based projects.  
We version the packages in this scheme: `{PublishedYear}.{SteamworksNativeSdkVersion}.{Patch}(-{PreviewVersionSuffix})`. Version `2026.165.1` means this package version is published in 2026, targetting to Steamworks SDK v1.65, this version is the second v1.65 version with (potentially critical)fixes.

For multiple projects sharing the same Steamworks.NET.AnyCPU library, it is recommended to use a [Directory.Packages.props](https://learn.microsoft.com/nuget/consume-packages/central-package-management) file to manage the package version centrally(the *CPM* method).

### Unity Projects
This library is **not applicable to Unity** projects, which should use the [upstream Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET).

### .NET Framework 4.8 and Earlier including FNX and XNA
.NET Framework are **not supported** by this library, and users should compile the upstream Steamworks.NET library for those versions. Modify the `<TargetFrameworks>` property of `Standalone2.0/Steamworks.NET.Standard.csproj` should cover enough.

.NET Framework 4.5 made an ABI change that is incompatible with older .NET Framework versions, targetting to those versions should build against the upstream `Standalone\Steamworks.NET.csproj` instead.

## Technical Information

### Supported Platforms
`win-x64`, `win-x86`, `osx-arm64`, `linux-x64`, `linux-arm64` are tested platforms. `android-arm64` is not tested.

### Differences from Upstream
Top supported game engines is Godot, but the library can be used in any .NET 8+ application that requires integration with Steamworks features.

We made a big change to the Binding Generator to support AnyCPU, and the library is now built as a single assembly that can be used in AnyCPU projects. The library is also built against .NET 8+ and uses the C# 12 features.
Due to the python based Binding Generator is full of workarounds and hacks, I plan to explore ClangSharp as a replacement for the Binding Generator in future, which will make the library more maintainable and easier to support.

### Supported .NET Versions
Earlier supported LTS version of .NET, as 2026/10/10 we support .NET 8.0 and later versions. Once the supported .NET version we using is no longer supported by Microsoft, we will upgrade to the next LTS version of .NET. The library is currently built against `net8.0` and uses C# 12 features.

## Relationship between the published Steamworks.NET code based NuGet packages, and the NuGet packaging workflow history

### Initial Steamworks.NET.Standard.\<rid\> packages
I initially contributed to the NuGet packaging workflow of the upstream Steamworks.NET repository in the `Standalone` subfolder, which manually build different platform-specific assemblies into different packages. The initial NuGet packages are published under the `Steamworks.NET.Standard.<rid>` package names. They are all obsolete and not maintained anymore.

### The third party `Steamworks.NET`
Later I improved the workflow to pack all versions into a single package consists of multiple platform-specific assemblies, with a MSBuild targets file and a `nuspec` package definition. Such packaging process is too complex for others and have a hard dependency to Windows NuGet client.

At this time the package `Steamworks.NET` was not published by @rlabrecque yet due to some reasons, so a third party individual @thetestgame take that ID and published the package, which is not updated since 2024/2/29. I guess the packaging workflow is broken that the pacakge owner does not able to debug the MSBuild target file.

Currently `Steamworks.NET` package is targetting to *Steamworks SDK v1.60* and lack of some critical fix like threading issues in `CallbackDispatcher`. Also, this package does not contain Steamworks native binaries, you need to install them by referencing correct `Facepunch.Steamworks.Dll` version or deploy the natives yourself.

### MonoGame suffixed package
`Steamworks.NET.MonoGame` is recently(in 2026) updated `netstandard2.1` package, based on fixed `Standalone2.0` packaging workflow. Different from the `Steamworks.NET`, it places steam natives correctly inside the package, that making .NET SDK able to deploy natives for consumer. It's versioning scheme is `YYYY.MM.dd`. `2026.6.12` bind against Steamworks SDK *v1.64*.

### Standalone3.0, The `Steamwork.NET.AnyCPU`
`Steamworks.NET.AnyCPU` is based on new packaging workflow in `Standalone3.0` using standard `dotnet pack` experience, thus cross-platform complie-pack-publish became possible. Now we publish packages through CI/CD: GitHub Actions. If new Steamworks SDK is published or upstream merged some fixes, submit an issue in `Steamworks.NET.AnyCPU` repository to request update.

## Contributing
For more technical information about this library, please refer to the [AnyCPU Contributing Guidelines](./Standalone3.0/CONTRIBUTING.md). We need contributors to keep this library alive.

## Original readme from upstream:

---

## Steamworks.NET

_Steamworks.NET_ is a C# Wrapper for Valve's Steamworks API, it can be used either with Unity or your C# based Application.

_Steamworks.NET_ was designed to be as close as possible to the original C++ API, as such the documentation provided from Valve largely covers usage of _Steamworks.NET_.
Niceties and C# Idioms can be easily implemented on top of _Steamworks.NET_.

_Steamworks.NET_ fully supports Windows (32 and 64 bit), OSX, and Linux. Currently building against Steamworks SDK 1.65.

* Author: [Riley Labrecque](https://github.com/rlabrecque)
* License: [MIT](https://www.opensource.org/licenses/mit-license.php)
* [Documentation](https://steamworks.github.io/)
* [Discussion Thread](https://steamcommunity.com/groups/steamworks/discussions/0/666827974770212954/)
* [Reporting Issues](https://github.com/rlabrecque/Steamworks.NET/issues)
  Note that only Steamworks.NET specific issues should be reported, general API questions/issues should be asked on the [Steamworks discussion board](http://steamcommunity.com/groups/steamworks/discussions).

## Installation

You can find the installation instructions [here](https://steamworks.github.io/installation/).

## Samples

Check out these sample projects to get started:

* [Steamworks.NET Example](https://github.com/rlabrecque/Steamworks.NET-Example)
* [Steamworks.NET Test](https://github.com/rlabrecque/Steamworks.NET-Test)
* [Steamworks.NET ChatClient](https://github.com/rlabrecque/Steamworks.NET-ChatClient)
* [Steamworks.NET GameServerTest](https://github.com/rlabrecque/Steamworks.NET-GameServerTest)
