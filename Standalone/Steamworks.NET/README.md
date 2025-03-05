Steamworks.NET Standalone
==============

This is the project file for the standalone version of _Steamworks.NET_. This is designed to be used with non Unity applications.

* A: Install community built NuGet package `Steamworks.NET` into your project, .NET SDK will ship proper binaries for you(both .NET assembly and steam native library).
  * Start coding! Call SteamAPI.Init() before initializing your renderer.
* B: Another approach is to build the standalone assemblies with the project file located in `Standalone/` or download the prebuilt binaries which are available on the [Releases](https://github.com/rlabrecque/Steamworks.NET/releases) page.
  * Open the Visual Studio solution (.sln) file, build both targets one for Windows and one for OSX & Linux. (Optional if you downloaded a prebuilt version)
  * Reference the built assembly (Steamworks.NET.dll) in your project.
* C: Otherwise, copy `Plugins/Steamworks.NET` directly into your C# project. In Visual Studio open your project properties, change to the Build tab and define `STEAMWORKS_WIN` or `STEAMWORKS_LIN_OSX` globally via `Conditional compilation symbols`.
  * This is only recommended if your binary is not portable across platforms already. If you ship on multiple platforms you must have multiple build targets for each platforms. Please prefer the first route.

* When you build your application the following files must be copied into the output dir:
  * All Platforms:
    * **steam_appid.txt** - Required for testing your application outside of steam. *Do not ship this to customers!*
    * **Steamworks.NET.dll** - By default this binary matches build computer's operating system. 
      To cross compile, Specify `SNetOperatingSystem` MSBuild property on command line. Possible values are `win` `linux` `osx`.
      For example, `dotnet build -p:SNetOperatingSystem=linux`.
  * Windows:
    * **steam_api.dll**
  * OSX:
    * **steam_api.bundle**
    * **Steamworks.NET.dll.config** - This lets Mono know where to find CSteamworks.
  * Linux:
    * **libsteam_api.so**

Have a look at this [example project](https://github.com/rlabrecque/Steamworks.NET-StandaloneTest) to get started.
