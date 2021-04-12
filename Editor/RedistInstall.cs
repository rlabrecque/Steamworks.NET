// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2019 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Uncomment this out or add it to your custom platform defines to disable checking the plugin platform settings.
//#define DISABLEPLATFORMSETTINGS

using UnityEngine;
using UnityEditor;
using System.IO;

// This copys various files into their required locations when Unity is launched to make installation a breeze.
[InitializeOnLoad]
public class RedistInstall
{
	static RedistInstall()
	{
		var appIdPath = Application.dataPath.Replace("/Assets", "") + "/steam_appid.txt";
		if (!File.Exists(appIdPath))
		{
			File.WriteAllText(appIdPath, "480");
		}
	}

#if UNITY_5 || UNITY_2017 || UNITY_2017_1_OR_NEWER
	static void SetPlatformSettings()
	{
		foreach (var plugin in PluginImporter.GetAllImporters())
		{
			// Skip any null plugins, why is this a thing?!
			if (plugin == null)
			{
				continue;
			}

			// Skip any absolute paths, as they are only built in plugins.
			if (Path.IsPathRooted(plugin.assetPath))
			{
				continue;
			}

			bool didUpdate = false;
			string filename = Path.GetFileName(plugin.assetPath);

			switch (filename)
			{
				case "steam_api.bundle":
					didUpdate |= ResetPluginSettings(plugin, "AnyCPU", "OSX");
					didUpdate |= SetCompatibleWithOSX(plugin);
					break;
				case "libsteam_api.so":
					if (plugin.assetPath.Contains("x86_64"))
					{
						didUpdate |= ResetPluginSettings(plugin, "x86_64", "Linux");
						didUpdate |= SetCompatibleWithLinux(plugin, BuildTarget.StandaloneLinux64);
					}
#if !UNITY_2019_2_OR_NEWER
					else {
						didUpdate |= ResetPluginSettings(plugin, "x86", "Linux");
						didUpdate |= SetCompatibleWithLinux(plugin, BuildTarget.StandaloneLinux);
					}
#endif
					break;
				case "steam_api.dll":
				case "steam_api64.dll":
					if (plugin.assetPath.Contains("x86_64"))
					{
						didUpdate |= ResetPluginSettings(plugin, "x86_64", "Windows");
#if UNITY_5_3_OR_NEWER
						didUpdate |= SetCompatibleWithWindows(plugin, BuildTarget.StandaloneWindows64);
#endif
					}
					else
					{
						didUpdate |= ResetPluginSettings(plugin, "x86", "Windows");
#if UNITY_5_3_OR_NEWER
						didUpdate |= SetCompatibleWithWindows(plugin, BuildTarget.StandaloneWindows);
#endif
					}

#if !UNITY_5_3_OR_NEWER
					// We do this because Unity had a bug where dependent dll's didn't get loaded from the Plugins
					// folder in actual builds. But they do in the editor now! So close... Unity bug number: 728945
					// So ultimately we must keep using RedistCopy to copy steam_api[64].dll next to the .exe on builds, and
					// we don't want a useless duplicate version of the dll ending up in the Plugins folder.
					// This was fixed in Unity 5.3!
					didUpdate |= SetCompatibleWithEditor(plugin);
#endif
					break;
			}

			if (didUpdate)
			{
				plugin.SaveAndReimport();
			}
		}
	}

	static bool ResetPluginSettings(PluginImporter plugin, string CPU, string OS)
	{
		bool didUpdate = false;

		if (plugin.GetCompatibleWithAnyPlatform() != false)
		{
			plugin.SetCompatibleWithAnyPlatform(false);
			didUpdate = true;
		}

		if (plugin.GetCompatibleWithEditor() != true)
		{
			plugin.SetCompatibleWithEditor(true);
			didUpdate = true;
		}

		if (plugin.GetEditorData("CPU") != CPU)
		{
			plugin.SetEditorData("CPU", CPU);
			didUpdate = true;
		}

		if (plugin.GetEditorData("OS") != OS)
		{
			plugin.SetEditorData("OS", OS);
			didUpdate = true;
		}

		return didUpdate;
	}

	static bool SetCompatibleWithOSX(PluginImporter plugin)
	{
		bool didUpdate = false;

#if UNITY_2017_3_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSX, true);
#else
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel, true);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel64, true);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXUniversal, true);
#endif

#if !UNITY_2019_2_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinuxUniversal, false);
#endif
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux64, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows64, false);

		return didUpdate;
	}

	static bool SetCompatibleWithLinux(PluginImporter plugin, BuildTarget platform)
	{
		bool didUpdate = false;

#if !UNITY_2019_2_OR_NEWER
		if (platform == BuildTarget.StandaloneLinux) {
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux, true);
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux64, false);
		}
		else {
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux, false);
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux64, true);
		}
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinuxUniversal, true);
#else
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux64, true);
#endif

#if UNITY_2017_3_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSX, false);
#else
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel64, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXUniversal, false);
#endif
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows64, false);

		return didUpdate;
	}

	static bool SetCompatibleWithWindows(PluginImporter plugin, BuildTarget platform)
	{
		bool didUpdate = false;

		if (platform == BuildTarget.StandaloneWindows)
		{
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows, true);
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows64, false);
		}
		else
		{
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows, false);
			didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows64, true);
		}

#if !UNITY_2019_2_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinuxUniversal, false);
#endif
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux64, false);
#if UNITY_2017_3_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSX, false);
#else
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel64, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXUniversal, false);
#endif

		return didUpdate;
	}

	static bool SetCompatibleWithEditor(PluginImporter plugin)
	{
		bool didUpdate = false;

#if !UNITY_2019_2_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinuxUniversal, false);
#endif
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneLinux64, false);
#if UNITY_2017_3_OR_NEWER
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSX, false);
#else
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXIntel64, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneOSXUniversal, false);
#endif
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows, false);
		didUpdate |= SetCompatibleWithPlatform(plugin, BuildTarget.StandaloneWindows64, false);

		return didUpdate;
	}

	static bool SetCompatibleWithPlatform(PluginImporter plugin, BuildTarget platform, bool enable)
	{
		if (plugin.GetCompatibleWithPlatform(platform) == enable)
		{
			return false;
		}

		plugin.SetCompatibleWithPlatform(platform, enable);
		return true;
	}
#endif // UNITY_5 || UNITY_2017 || UNITY_2017_1_OR_NEWER
}
