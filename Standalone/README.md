Steamworks.NET
=======

This is the project file for the standalone version of _Steamworks.NET_. This is designed to be used with non Unity applications.

Usage instructions:
 * Open the Visual Studio solution (.sln) file, build both targets one for Windows and one for OSX & Linux. (Optional if you downloaded a prebuilt version)
 * Reference the built assembly (Steamworks.NET.dll) in your project.
 * Start coding! Call SteamAPI.Init() before starting up your renderer.
 * When you build your application the following files must be copied in the output dir:
  * - **steam_appid.txt** - Required for testing your application outside of steam. *Do not ship this to customers!*
  * - **Steamworks.NET.dll** - Make sure it's the correct version for the platform that you plan on shipping for! - When building for OSX or Linux the wrong Steamworks.NET.dll will be copied over by default, it is recommended that you create a post build script to copy the correct version.
  * Win:
  *  - **CSteamworks.dll**
  *  - **steam_api.dll**
  * OSX:
  *  - **CSteamworks.bundle**
  *  - **Steamworks.NET.dll.config** - Lets mono know where to find CSteamworks
  * Linux:
  *  - **libCSteamworks.so**
  *  - **libsteam_api.so**



Have a look at this [example project](https://github.com/rlabrecque/Steamworks.NET-StandaloneTest).
