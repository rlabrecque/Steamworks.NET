// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2018 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

// Add 'DISABLEREDISTCOPY' to your custom platform defines to disable automatic copying!
#if UNITY_5_3_OR_NEWER
	#define DISABLEREDISTCOPY
#endif

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Steamworks;
using System.IO;

public class RedistCopy {
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		string baseDir;
		string pluginsDir;
		switch(target)
		{
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
			pluginsDir = Path.Combine(baseDir, "Plugins");
			break;
		case BuildTarget.StandaloneLinux:
			baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
			pluginsDir = Path.Combine(Path.Combine(baseDir, "Plugins"), "x86");
			break;
		case BuildTarget.StandaloneLinux64:
		case BuildTarget.StandaloneLinuxUniversal:
			baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
			pluginsDir = Path.Combine(Path.Combine(baseDir, "Plugins"), "x86_64");
			break;
#if UNITY_2017_3_OR_NEWER
		case BuildTarget.StandaloneOSX:
#else
		case BuildTarget.StandaloneOSXIntel:
		case BuildTarget.StandaloneOSXIntel64:
		case BuildTarget.StandaloneOSXUniversal:
#endif
			baseDir = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + ".app");
			baseDir = Path.Combine(baseDir, "Contents");
			pluginsDir = Path.Combine(baseDir, "Plugins");
			break;
		default:
			return;
		}

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

#if !DISABLEREDISTCOPY
		if (target == BuildTarget.StandaloneWindows64) {
			CopyFile("steam_api64.dll", "steam_api64.dll", "Assets/Plugins/x86_64", pathToBuiltProject);
		}
		else if (target == BuildTarget.StandaloneWindows) {
			CopyFile("steam_api.dll", "steam_api.dll", "Assets/Plugins/x86", pathToBuiltProject);
		}

		string controllerCfg = Path.Combine(Application.dataPath, "controller.vdf");
		if (File.Exists(controllerCfg)) {
			string strFileDest = Path.Combine(baseDir, "controller.vdf");

			File.Copy(controllerCfg, strFileDest);
			File.SetAttributes(strFileDest, File.GetAttributes(strFileDest) & ~FileAttributes.ReadOnly);

			if (!File.Exists(strFileDest)) {
				Debug.LogWarning("[Steamworks.NET] Could not copy controller.vdf into the built project. File.Copy() Failed. Place controller.vdf from the Steamworks SDK in the output dir manually.");
			}
		}
#endif
	}

	static void CopyFile(string filename, string outputfilename, string pathToFile, string pathToBuiltProject) {
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
	}
}

#endif // !DISABLESTEAMWORKS
