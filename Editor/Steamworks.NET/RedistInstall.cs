// Uncomment this out to disable copying
//#define DISABLEPLATFORMSETTINGS

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

// This copys various files into their required locations when Unity is launched to make installation a breeze.
[InitializeOnLoad]
public class RedistInstall {
	static RedistInstall() {
		CopyFile("Assets/Plugins/Steamworks.NET/redist", "steam_appid.txt", false);

		// We only need to copy the dll into the project root on <= Unity 5.0
#if UNITY_EDITOR_WIN && (!UNITY_5 || UNITY_5_0)
	#if UNITY_EDITOR_64
		CopyFile("Assets/Plugins/x86_64", "steam_api64.dll", true);
	#else
		CopyFile("Assets/Plugins/x86", "steam_api.dll", true);
	#endif
#endif

#if UNITY_5
	#if !DISABLEPLATFORMSETTINGS
		SetPlatformSettings();
	#endif
#endif
	}

	static void CopyFile(string path, string filename, bool bCheckDifference) {
		string strCWD = Directory.GetCurrentDirectory();
		string strSource = Path.Combine(Path.Combine(strCWD, path), filename);
		string strDest = Path.Combine(strCWD, filename);

		if (!File.Exists(strSource)) {
			Debug.LogWarning(string.Format("[Steamworks.NET] Could not copy {0} into the project root. {0} could not be found in '{1}'. Place {0} from the Steamworks SDK in the project root manually.", filename, Path.Combine(strCWD, path)));
			return;
		}

		if (File.Exists(strDest)) {
			if (!bCheckDifference)
				return;

			if (File.GetLastWriteTime(strSource) == File.GetLastWriteTime(strDest)) {
				FileInfo fInfo = new FileInfo(strSource);
				FileInfo fInfo2 = new FileInfo(strDest);
				if (fInfo.Length == fInfo2.Length) {
					return;
				}
			}

			Debug.Log(string.Format("[Steamworks.NET] {0} in the project root differs from the Steamworks.NET redistributable. Updating.... Please relaunch Unity.", filename));
		}
		else {
			Debug.Log(string.Format("[Steamworks.NET] {0} is not present in the project root. Copying...", filename));
		}

		File.Copy(strSource, strDest, true);
		File.SetAttributes(strDest, File.GetAttributes(strDest) & ~FileAttributes.ReadOnly);

		if (File.Exists(strDest)) {
			Debug.Log(string.Format("[Steamworks.NET] Successfully copied {0} into the project root. Please relaunch Unity.", filename));
		}
		else {
			Debug.LogWarning(string.Format("[Steamworks.NET] Could not copy {0} into the project root. File.Copy() Failed. Place {0} from the Steamworks SDK in the project root manually.", filename));
		}
	}

#if UNITY_5
	static void SetPlatformSettings() {
		foreach(var plugin in PluginImporter.GetAllImporters()) {
			// Skip any absolute paths, as they are only built in plugins.
			if(Path.IsPathRooted(plugin.assetPath)) {
				continue;
			}

			string filename = Path.GetFileName(plugin.assetPath);
			switch(filename) {
				case "CSteamworks.bundle":
					ResetPluginSettings(plugin, "AnyCPU", "OSX");
					SetCompatibleWithOSX(plugin);
					break;
				case "libCSteamworks.so":
				case "libsteam_api.so":
					if(plugin.assetPath.Contains("x86_64")) {
						ResetPluginSettings(plugin, "x86_64", "Linux");
						SetCompatibleWithLinux(plugin, BuildTarget.StandaloneLinux64);
					}
					else {
						ResetPluginSettings(plugin, "x86", "Linux");
						SetCompatibleWithLinux(plugin, BuildTarget.StandaloneLinux);
					}
					break;
				case "CSteamworks.dll":
					if (plugin.assetPath.Contains("x86_64")) {
						ResetPluginSettings(plugin, "x86_64", "Windows");
						SetCompatibleWithWindows(plugin, BuildTarget.StandaloneWindows64);
					}
					else {
						ResetPluginSettings(plugin, "x86", "Windows");
						SetCompatibleWithWindows(plugin, BuildTarget.StandaloneWindows);
					}
					break;
				case "steam_api.dll":
				case "steam_api64.dll":
					if (plugin.assetPath.Contains("x86_64")) {
						ResetPluginSettings(plugin, "x86_64", "Windows");
					}
					else {
						ResetPluginSettings(plugin, "x86", "Windows");
					}
					SetCompatibleWithEditor(plugin);
					break;
			}
		}
	}

	static void ResetPluginSettings(PluginImporter plugin, string CPU, string OS)
	{
	    bool didChange = false;

        if (plugin.GetCompatibleWithAnyPlatform())
	    {
	        plugin.SetCompatibleWithAnyPlatform(false);
	        didChange = true;
	    }
	    if (plugin.GetCompatibleWithEditor() != true)
	    {
	        plugin.SetCompatibleWithEditor(true);
            didChange = true;
        }
	    if (plugin.GetEditorData("CPU") != CPU)
	    {
	        plugin.SetEditorData("CPU", CPU);
            didChange = true;
        }
	    if (plugin.GetEditorData("OS") != OS)
	    {
	        plugin.SetEditorData("OS", OS);
            didChange = true;
        }

        if(didChange) plugin.SaveAndReimport();
	}

	static void SetCompatibleWithOSX(PluginImporter plugin) {
        SetCompatibilities(plugin, new[] {
            BuildTarget.StandaloneLinux,
            BuildTarget.StandaloneLinux64,
            BuildTarget.StandaloneLinuxUniversal,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64
            },
        new[] { BuildTarget.StandaloneOSXIntel, BuildTarget.StandaloneOSXIntel64, BuildTarget.StandaloneOSXUniversal }
        );
    }

	static void SetCompatibleWithLinux(PluginImporter plugin, BuildTarget platform) {
        SetCompatibilities(plugin, new[] {
            BuildTarget.StandaloneOSXIntel,
            BuildTarget.StandaloneOSXIntel64,
            BuildTarget.StandaloneOSXUniversal,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64
            },
        new[] { platform, BuildTarget.StandaloneLinuxUniversal }
        );
    }

	static void SetCompatibleWithWindows(PluginImporter plugin, BuildTarget platform) {
        SetCompatibilities(plugin, new[] {
            BuildTarget.StandaloneLinux64,
            BuildTarget.StandaloneLinux,
            BuildTarget.StandaloneLinuxUniversal,
            BuildTarget.StandaloneOSXIntel,
            BuildTarget.StandaloneOSXIntel64,
            BuildTarget.StandaloneOSXUniversal,
            }, 
        new[] { platform }
        );
    }

	static void SetCompatibleWithEditor(PluginImporter plugin) {
        SetCompatibilities(plugin, new [] {
            BuildTarget.StandaloneLinux64,
            BuildTarget.StandaloneLinux,
            BuildTarget.StandaloneLinuxUniversal,
            BuildTarget.StandaloneOSXIntel,
            BuildTarget.StandaloneOSXIntel64,
            BuildTarget.StandaloneOSXUniversal,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
        }, new BuildTarget[] {});
	}

    private static void SetCompatibilities(PluginImporter plugin, BuildTarget[] notCompatible, BuildTarget[] compatible)
    {
        var targetMapping = new Dictionary<BuildTarget, bool>();

        foreach(var n in notCompatible) targetMapping.Add(n, false);
        foreach(var c in compatible) targetMapping.Add(c, true);

        bool didUpdate = false;

        foreach (var t in targetMapping)
        {
            if (plugin.GetCompatibleWithPlatform(t.Key) == t.Value) continue;

            plugin.SetCompatibleWithPlatform(t.Key, t.Value);

            didUpdate = true;
        }

        if (didUpdate) plugin.SaveAndReimport();
    }
#endif // UNITY_5
}
