// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2017 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// Add 'DISABLEREDISTCOPY' to your custom platform defines to disable automatic copying!
#if UNITY_5_3_OR_NEWER
	#define DISABLEREDISTCOPY
#endif

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class RedistCopy {
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
#if !DISABLEREDISTCOPY
		if (target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneWindows64 &&
			target != BuildTarget.StandaloneOSXIntel && target != BuildTarget.StandaloneOSXIntel64 && target != BuildTarget.StandaloneOSXUniversal &&
			target != BuildTarget.StandaloneLinux && target != BuildTarget.StandaloneLinux64 && target != BuildTarget.StandaloneLinuxUniversal) {
			return;
		}

		string strProjectName = Path.GetFileNameWithoutExtension(pathToBuiltProject);

		if (target == BuildTarget.StandaloneWindows64) {
			CopyFile("steam_api64.dll", "steam_api64.dll", "Assets/Plugins/x86_64", pathToBuiltProject);
		}
		else if (target == BuildTarget.StandaloneWindows) {
			CopyFile("steam_api.dll", "steam_api.dll", "Assets/Plugins/x86", pathToBuiltProject);
		}

		string controllerCfg = Path.Combine(Application.dataPath, "controller.vdf");
		if (File.Exists(controllerCfg)) {
			string dir = "_Data";
			if (target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXIntel64 || target == BuildTarget.StandaloneOSXUniversal) {
				dir = ".app/Contents";
			}

			string strFileDest = Path.Combine(Path.Combine(Path.GetDirectoryName(pathToBuiltProject), strProjectName + dir), "controller.vdf");

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
