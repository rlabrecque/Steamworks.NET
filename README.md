# Steamworks.NET

_Steamworks.NET_ is a C# Wrapper for Valve's Steamworks API, it can be used either with Unity or your C# based Application.

_Steamworks.NET_ was designed to be as close as possible to the original C++ API, as such the documentation provided from Valve largely covers usage of _Steamworks.NET_.
Niceties and C# Idioms can be easily implemented on top of _Steamworks.NET_.

_Steamworks.NET_ fully supports Windows (32 and 64 bit), OSX, and Linux. Currently building against Steamworks SDK 1.52.

* Author: [Riley Labrecque](https://github.com/rlabrecque)
* License: [MIT](http://www.opensource.org/licenses/mit-license.php)
* [Documentation](https://steamworks.github.io/)
* [Discussion Thread](http://steamcommunity.com/groups/steamworks/discussions/0/666827974770212954/)
* [Reporting Issues](https://github.com/rlabrecque/Steamworks.NET/issues)
  Note that only Steamworks.NET specific issues should be reported, general API questions/issues should be asked on the [Steamworks discussion board](http://steamcommunity.com/groups/steamworks/discussions).

[![Support via Paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=YFZZER8VNXKRC)

## Installation

You can find the installation instructions [here](http://steamworks.github.io/installation/).

### Unity Package Manager

Unity Package Manager support is still fairly new but you can use it like so:

1. Open the Package Manager
2. Click the "+" (plus) button located in the upper left of the window
3. Select the "Add package from git URL..." option
4. Enter the following URL:
    `https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net`
5. Click the "Add" button and wait several seconds for the system to download and install the Steamworks.NET package from GitHub.
6. Optionally: Manually download and import the [SteamManager](https://github.com/rlabrecque/Steamworks.NET-SteamManager/blob/master/SteamManager.cs) MonoBehavior which contains a starting point for using the Steamworks API in your project.

## Samples

Check out these sample projects to get started:

* [Steamworks.NET Example](https://github.com/rlabrecque/Steamworks.NET-Example)
* [Steamworks.NET Test](https://github.com/rlabrecque/Steamworks.NET-Test)
* [Steamworks.NET ChatClient](https://github.com/rlabrecque/Steamworks.NET-ChatClient)
* [Steamworks.NET GameServerTest](https://github.com/rlabrecque/Steamworks.NET-GameServerTest)
