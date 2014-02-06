1. Simply reference "(Win32) Steamworks" in your visual studio project as usual.
2. Add an ANSI encoded text file called "steam_appid.txt" to your project, put the steam ID of your application in here
3. In the properties for steam_appid.txt set Build Action: None and Copy To Output Directory: True
4. When your game starts up, call SteamAPI.Init(), from this point on everything is very close to the original Steamworks SDK.