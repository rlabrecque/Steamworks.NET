// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2021 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using Steamworks;

public class RedistCopy {
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		string baseDir;

		switch(target)
		{
			case BuildTarget.StandaloneWindows:
			{
				baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
				break;
			}
			case BuildTarget.StandaloneWindows64:
			{
				baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
				break;
			}
			case BuildTarget.StandaloneLinux64:
			{
				baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
				break;
			}
			case BuildTarget.StandaloneOSX:
			default:
			{
				return;
			}
		}

		string pluginsDir = Path.Combine(baseDir, "Plugins");

		// Create if it doesn't exist yet
		Directory.CreateDirectory(pluginsDir);

		string[] DebugInfo = {
			"Steamworks.NET created by Riley Labrecque",
			"http://steamworks.github.io",
			"",
			"Steamworks.NET Version: " + Steamworks.Version.SteamworksNETVersion,
			"Steamworks SDK Version: " + Steamworks.Version.SteamworksSDKVersion,
			"Steam API DLL Version:  " + Steamworks.Version.SteamAPIDLLVersion,
			"Steam API DLL Size:     " + Steamworks.Version.SteamAPIDLLSize,
			"Steam API64 DLL Size:   " + Steamworks.Version.SteamAPI64DLLSize,
			""
		};
		File.WriteAllLines(Path.Combine(pluginsDir, "Steamworks.NET.txt"), DebugInfo);
	}
}

#endif // !DISABLESTEAMWORKS
