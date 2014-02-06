At the moment there's only one project here - "(Win32) Steamworks" - which obviously only supports 32 bit windows. Supporting another platform should be quite simple.

1. First, copy the "(Win32) Steamworks" .csproj file, and rename it as appropriate for the platform you're supporting. e.g. "(Linux 32) Steamworks". Once you've done this, add the new project into the visual studio solution.
2. The windows project has the windows redistributable binaries for steamworks included in it, set to Copy to the build output folder. Remove these in your new project and add the redists for your platform
3. To set a file to copy _Right click File > Properties > "Build Action: None" and "Copy To Output Directory: Copy If Newer"_
4. There are three build symbols to be aware of, used within StructPack.cs:

	STEAMWORKS_WIN32
	STEAMWORKS_LINUX
	STEAMWORKS_OSX

5. Set your new project to define the appropriate build symbol. _Right click project > Properties > Build > Conditional Compilation Symbols"_
6. There's probably other stuff to do to. But I don't have any experience with this kind of thing. Once you have all the fun discovering what I missed out, edit it in here.
7. Submit a pull request for your changes to [Steamworks.Net](https://github.com/rlabrecque/Steamworks.NET) with your new project