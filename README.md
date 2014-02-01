Steamworks.NET
=======

_Steamworks.NET_ is a C# Wrapper for Valve's Steamworks API, it is intended for use with Unity. This project relies on dynamic libraries created by [CSteamworks](https://github.com/rlabrecque/CSteamworks). (Prebuilt and included for ease of use)

_Steamworks.NET_ was designed to be as close as possible to the original C++ API, as such the documentation provided from Valve largely covers usage of _Steamworks.NET_. 
Niceties and C# Idioms can be easily implemented on top of _Steamworks.NET_.

The included CSteamworks.dll was built with VS2010, and as such requires [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=5555). You must also ship this to customers via Installation -> Installers on the Steam Partner Website.

_Steamworks.NET_ currently supports Steamworks SDK 1.28.

* Author: [Riley Labrecque](https://github.com/rlabrecque)
* License: [MIT](http://www.opensource.org/licenses/mit-license.php)
* [Discussion](http://steamcommunity.com/groups/steamworks/discussions/0/666827974770212954/)
* [Reporting Issues](https://github.com/rlabrecque/Steamworks.NET/issues)
* 1-on-1 support is available by donating $40 USD or greater.
 * Support can be obtained via [Email](support@rileylabrecque.com) or [Steam](http://steamcommunity.com/id/rlabrecque)
 * I can only help with Steamworks.NET specific issues, general API questions should be asked on the discussion board.

[![Support via Gittip](https://rawgithub.com/twolfson/gittip-badge/0.1.0/dist/gittip.png)](https://www.gittip.com/rlabrecque/)
[![Support via Paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QHK4A3CWJDK3N)

[![Support via Flattr](https://api.flattr.com/button/flattr-badge-large.png)](https://flattr.com/submit/auto?user_id=rlabrecque&url=https%3A%2F%2Fgithub.com%2Frlabrecque%2FSteamworks.NET)


Usage
--------

To use _Steamworks.NET_ you must be a Steamworks developer. _Steamworks.NET_ Requires Unity Pro for the plugin functionality.

* Clone or download a Zip Archive: [Stable (1.1.0)](https://github.com/rlabrecque/Steamworks.NET/archive/1.1.0.zip) -- [Master (Cutting-edge Development)](https://github.com/rlabrecque/Steamworks.NET/archive/master.zip)
* Extract and copy Steamworks.NET's `Plugins` and `Editor` folders into your `Assets` folder.
* Open `Plugins/Steamworks.NET/redist/steam_appid.txt` and replace '480' with your own AppId.
* Launch your Unity project. It should copy steam_appid.txt (and steam_api.dll if your on windows) into the root of your project.
* Close Unity and relaunch the project so that it loads the newly copied steam_appid.txt & steam_api.dll.

If you plan on shipping a Linux build (do it!) then you must edit `Plugins/Steamworks.NET/redist/linux/launchscript` and replace `REPLACEWITHYOURGAMENAME` with your games name.


If you wish to use the functions from `sdkencryptedappticket.dll` then you will need to manually place the dll/so/dylib in the following location:
* Windows: Next to steam_api.dll
* OSX: In `/Contents/Frameworks/MonoEmbedRuntime/osx/`
* Linux: Next to CSteamworks.so

`sdkencryptedappticket.dll` can be found in the Steamworks SDK.

Check out these example projects to get started:
* [Steamworks.NET Example](https://github.com/rlabrecque/Steamworks.NET-Example)
* [Steamworks.NET Test](https://github.com/rlabrecque/Steamworks.NET-Test)


Limitations
-----------

* Only x86 builds are currently supported
* The Overlay and Steam Controller only work when launched from Steam directly.
* When used from within the Unity Editor, Steam will think you are in game constantly. This is normal and does not effect usage.
* _Steamworks.NET_ does not currently support ISteamAppTicket or ISteamGameCoordinator.
* The following Interfaces are largely untested (but should be completely functional):
```
ISteamHTTP
ISteamMatchmaking (May require some work!)
ISteamNetworking
ISteamScreenshots
ISteamUGC
ISteamUnifiedMessages
ISteamGameServer
ISteamGameServerHttp
ISteamGameServerNetworking
ISteamGameServerStats
ISteamGameServerUtils
