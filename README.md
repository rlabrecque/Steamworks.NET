Steamworks.NET
=======

_Steamworks.NET_ is a barebones C# Wrapper for Valve's Steamworks API intended for use with Unity. This project relys on dynamic libraries created by [CSteamworks](https://github.com/rlabrecque/CSteamworks). (Prebuilt and included for ease of use)

_Steamworks.NET_ was designed to be as close as possible to the original C++ API, as such the documentation provided from Valve largely covers usage of _Steamworks.NET_.
Niceties and C# Idioms can be implemented on top of _Steamworks.NET_ easily.

The included CSteamworks.dll was built with VS2010, and as such requires [Microsoft Visual C++ 2010 Redistributable Package (x86)](http://www.microsoft.com/en-us/download/details.aspx?id=5555). You must also ship this to customers via Installation -> Installers on the Steam Partner Website.

* Author: [Riley Labrecque](https://github.com/rlabrecque)
* License: [MIT](http://www.opensource.org/licenses/mit-license.php)
* [Github Project](https://github.com/rlabrecque/Steamworks.NET)
* [Reporting Issues](https://github.com/rlabrecque/Steamworks.NET/issues)
* Currently supports Steamworks SDK v1.26a


[![Support via Gittip](https://rawgithub.com/twolfson/gittip-badge/0.1.0/dist/gittip.png)](https://www.gittip.com/rlabrecque/)
[![Support via Paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2QWKYXPRH6GJW)
[![Support via Flattr](https://api.flattr.com/button/flattr-badge-large.png)](https://flattr.com/submit/auto?user_id=rlabrecque&url=https%3A%2F%2Fgithub.com%2Frlabrecque%2FSteamworks.NET)
Support via Bitcoin: 1svQRtNjCFthihzY5NN89aTan9WUua9GZ
Support via Litecoin: LPSyg4uJYSUdmFLEMLe85rKw6G7aQkmTak


Usage
--------

To use _Steamworks.NET_ you must be a Steamworks developer. _Steamworks.NET_ Requires Unity Pro for the plugin functionality.

* Clone _Steamworks.NET_ into your %UnityProject%/Assets/Plugins
* - Alternatively there is a [Zip](https://github.com/rlabrecque/Steamworks.NET/archive/master.zip) available.
* Create a file called `steam_appid.txt` within the root of your Unity Project, place your Steam AppID within this file.
* If you run the Unity Editor on Windows - copy `steam_api.dll` from the Steamworks SDK into the root of your Unity Project.


Check out these example projects to get started:
[Steamworks.NET Example](https://github.com/rlabrecque/Steamworks.NET-Example)
[Steamworks.NET Test](https://github.com/rlabrecque/Steamworks.NET-Test)


Realease Instructions
---------------------

**Only x86 builds are currently supported!**

* Windows: Copy `steam_api.dll` from the Steamworks SDK into the root of your built game, next to the .exe.

* Linux: You must launch the game with the following bash script.
    #!/bin/sh
    export LD_LIBRARY_PATH="./YOURGAME_Data/Plugins:$LD_LIBRARY_PATH"
    exec ./YOURGAME.x86
    
* OSX: No Additional Steps Required!

* NOTE: If you wish to test your game without launching through steam then you MUST place steam_appid.txt next to the game executable.


Limitations
-----------

* _Steamworks.NET_ does not currently support ISteamAppTicket or ISteamGameCoordinator.
* SteamGameServer is temporarily unsupported.
* When used from within the Unity Editor, Steam will think you are in game constantly. This is normal and does not effect usage.
* Does not currently support 64bit builds.
