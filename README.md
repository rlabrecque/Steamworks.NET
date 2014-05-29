Steamworks.NET
=======

_Steamworks.NET_ is a C# Wrapper for Valve's Steamworks API, it can be used either with Unity or your C# based Application. This project relies on dynamic libraries created by [CSteamworks](https://github.com/rlabrecque/CSteamworks). (Prebuilt and included for ease of use)

_Steamworks.NET_ was designed to be as close as possible to the original C++ API, as such the documentation provided from Valve largely covers usage of _Steamworks.NET_. 
Niceties and C# Idioms can be easily implemented on top of _Steamworks.NET_.

_Steamworks.NET_ currently supports Windows, OSX, and Linux in both 32 and 64bit varieties. Currently building against Steamworks SDK 1.29a.

* Author: [Riley Labrecque](https://github.com/rlabrecque)
* License: [MIT](http://www.opensource.org/licenses/mit-license.php)
* [Discussion](http://steamcommunity.com/groups/steamworks/discussions/0/666827974770212954/)
* [Reporting Issues](https://github.com/rlabrecque/Steamworks.NET/issues)
* 1-on-1 support is available by donating $40 USD or greater.
 * Support can be obtained via [Email](support@rileylabrecque.com), [Skype](skype:riley.labrecque?userinfo), or [Steam](http://steamcommunity.com/id/rlabrecque)
 * I can only help with Steamworks.NET specific issues, general API questions should be asked on the [Steamworks discussion board](http://steamcommunity.com/groups/steamworks/discussions).

[![Support via Gittip](https://rawgithub.com/twolfson/gittip-badge/0.1.0/dist/gittip.png)](https://www.gittip.com/rlabrecque/)
[![Support via Paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=YFZZER8VNXKRC)

[![Support via Flattr](https://api.flattr.com/button/flattr-badge-large.png)](https://flattr.com/submit/auto?user_id=rlabrecque&url=https%3A%2F%2Fgithub.com%2Frlabrecque%2FSteamworks.NET)


Usage
-----

To use _Steamworks.NET_ you must be a Steamworks developer. _Steamworks.NET_ requires Unity Pro for the plugin functionality.

* Clone or download a Zip Archive: [master - 3.0.0](https://github.com/rlabrecque/Steamworks.NET/archive/master.zip)
* Extract and copy Steamworks.NET's `Plugins/` and `Editor/` folders into your `Assets/` folder.
* Open `Plugins/Steamworks.NET/redist/steam_appid.txt` and replace `480` with your own AppId.
* Launch your Unity project. It should copy steam_appid.txt (and steam_api.dll if your on windows) into the root of your project.
* Close Unity and relaunch the project so that it loads the newly copied steam_appid.txt & steam_api.dll.

##### Samples
Check out these sample projects to get started:
* [Steamworks.NET Example](https://github.com/rlabrecque/Steamworks.NET-Example)
* [Steamworks.NET Test](https://github.com/rlabrecque/Steamworks.NET-Test)

Not using Unity?
----------------

If you are not using Unity then you have two available routes that you could take.
* A: Copy `Plugins/Steamworks.NET` into your C# project. In Visual Studio open your project properties, change to the Build tab and define `STEAMWORKS_WIN` or `STEAMWORKS_LIN_OSX` globally via `Conditional compilation symbols`.
 * This is only recommended if your binary is not portable across platforms already. If you ship on multiple platforms you must have multiple build targets for each platforms. Please prefer the second route.
* B: The peferable route is to build the standalone assemblies, with the project file located in `Standalone/`. Alternatively you can download the [prebuilt binaries (2.0.0)](https://github.com/rlabrecque/Steamworks.NET/releases/download/2.0.0/Steamworks.NET-Standalone_2.0.0.zip).
 * Further instructions are provided by the [README.md](https://github.com/rlabrecque/Steamworks.NET/blob/master/Standalone/README.md) in the `Standalone/` folder.

Using Steam Encrypted App Ticket?
---------------------------------

If you wish to use the functions from `sdkencryptedappticket.dll` then you will need to manually place the dll/so/dylib in the following location:
* Windows: Next to steam_api.dll
* OSX: In `/Contents/Frameworks/MonoEmbedRuntime/osx/`
* Linux: Next to CSteamworks.so

`sdkencryptedappticket.dll` can be found in the Steamworks SDK.

Limitations
-----------

* _Steamworks.NET_ does not currently support ISteamAppTicket or ISteamGameCoordinator.
* The following Interfaces are largely untested (but should be completely functional):
```
ISteamAppsList
ISteamMatchmaking (May require some work!)
ISteamMusic
ISteamNetworking
ISteamUGC
ISteamUnifiedMessages
ISteamGameServer
```

* The following are Unity specific and are out of our control, *ALL* Steamworks wrappers for Unity experience these issues.
 * When used from within the Unity Editor, Steam will think you are in game constantly. This is normal and does not effect usage.
 * The Overlay only works when launched from Steam directly. (A small number of Steamworks features (such as the Steam Controller) rely on the overlay being present. Prefer using a [Local Content Server](https://partner.steamgames.com/documentation/steampipeLCS) for testing.)
