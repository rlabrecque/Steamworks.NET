// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2019 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

// Add 'DISABLEREDISTCOPY' to your custom platform defines to disable automatic copying!
#if UNITY_5_3_OR_NEWER
	#define DISABLEREDISTCOPY
#endif // UNITY_5_3_OR_NEWER

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Steamworks;
using System.IO;

public class RedistCopy {
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		string baseDir;

		switch(target)
		{
			case BuildTarget.StandaloneWindows:
			{
				baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
				CopyFile("steam_api.dll", "steam_api.dll", "Assets/Plugins/x86", pathToBuiltProject);
				break;
			}
			case BuildTarget.StandaloneWindows64:
			{
				baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
				CopyFile("steam_api64.dll", "steam_api64.dll", "Assets/Plugins/x86_64", pathToBuiltProject);
				break;
			}
#if !UNITY_2019_2_OR_NEWER
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinuxUniversal:
#endif // !UNITY_2019_2_OR_NEWER
			case BuildTarget.StandaloneLinux64:
			{
				baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
				break;
			}
#if UNITY_2017_3_OR_NEWER
			case BuildTarget.StandaloneOSX:
#else
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSXUniversal:
#endif // UNITY_2017_3_OR_NEWER
			{
				baseDir = Path.Combine(Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + ".app"), "Contents");
				break;
			}
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

	static void CopyFile(string filename, string outputfilename, string pathToFile, string pathToBuiltProject) {
#if !DISABLEREDISTCOPY
		string strCWD = Directory.GetCurrentDirectory();
		string strSource = Path.Combine(Path.Combine(strCWD, pathToFile), filename);
		string strFileDest = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), outputfilename);

		if (!File.Exists(strSource)) {
			Debug.LogWarning(string.Format("[Steamworks.NET] Could not copy {0} into the project root. {0} could not be found in '{1}'. Place {0} from the redist into the project root manually.", filename, pathToFile));
			return;
		}

		if (File.Exists(strFileDest)) {
			if (File.GetLastWriteTime(strSource) == File.GetLastWriteTime(strFileDest)) {
				FileInfo fInfo = new FileInfo(strSource);
				FileInfo fInfo2 = new FileInfo(strFileDest);
				if (fInfo.Length == fInfo2.Length) {
					return;
				}
			}
		}

		File.Copy(strSource, strFileDest, true);
		File.SetAttributes(strFileDest, File.GetAttributes(strFileDest) & ~FileAttributes.ReadOnly);

		if (!File.Exists(strFileDest)) {
			Debug.LogWarning(string.Format("[Steamworks.NET] Could not copy {0} into the built project. File.Copy() Failed. Place {0} from the redist folder into the output dir manually.", filename));
		}
#endif // !DISABLEREDISTCOPY
	}
}

#endif // !DISABLESTEAMWORKS
