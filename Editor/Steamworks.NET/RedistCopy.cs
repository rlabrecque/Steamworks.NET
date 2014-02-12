// Uncomment this out to disable copying
//#define DISABLEREDISTCOPY

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class RedistCopy {
	const string SteamAPIRelativeLoc = "Assets/Plugins/Steamworks.NET/redist";

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
#if !DISABLEREDISTCOPY
		string strProjectName = Path.GetFileNameWithoutExtension(pathToBuiltProject);

		switch(target) {
			case BuildTarget.StandaloneWindows:
				CopyFile("steam_api.dll", "steam_api.dll", pathToBuiltProject);
				break;
			case BuildTarget.StandaloneWindows64:
				CopyFile("steam_api64.dll", "steam_api64.dll", pathToBuiltProject);
				break;
			case BuildTarget.StandaloneLinux:
				CopyFile("linux/launchscript", strProjectName, pathToBuiltProject);
				break;
			case BuildTarget.StandaloneLinux64:
				CopyFile("linux/launchscript64", strProjectName, pathToBuiltProject);
				break;
			case BuildTarget.StandaloneLinuxUniversal:
				CopyFile("linux/launchscriptuniversal", strProjectName, pathToBuiltProject);
				break;
			case BuildTarget.StandaloneOSXIntel:
				break;
			default:
				Debug.Log(string.Format("[Steamworks.NET] {0} Is not a supported platform.", target));
				return;
		}
				
		string controllerCfg = Path.Combine(Application.dataPath, "controller.vdf");
		if (File.Exists(controllerCfg)) {
			string dir = "_Data";
			if (target == BuildTarget.StandaloneOSXIntel) {
				dir = ".app/Contents";
			}

			string strFileDest = Path.Combine(Path.Combine(Path.GetDirectoryName(pathToBuiltProject), strProjectName + dir), "controller.vdf");

			File.Copy(controllerCfg, strFileDest);

			if (!File.Exists(strFileDest)) {
				Debug.LogWarning("[Steamworks.NET] Could not copy controller.vdf into the built project. File.Copy() Failed. Place controller.vdf from the Steamworks SDK in the output dir manually.");
			}
		}
#endif
	}

	static void CopyFile(string filename, string outputfilename, string pathToBuiltProject) {
		string strCWD = Directory.GetCurrentDirectory();
		string strSource = Path.Combine(Path.Combine(strCWD, SteamAPIRelativeLoc), filename);
		string strFileDest = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), outputfilename);

		if (File.Exists(strFileDest)) {
			return;
		}

		if (!File.Exists(strSource)) {
			Debug.LogWarning(string.Format("[Steamworks.NET] Could not copy {0} into the project root. {0} could not be found in '{1}'. Place {0} from the redist into the project root manually.", filename, SteamAPIRelativeLoc));
			return;
		}

		File.Copy(strSource, strFileDest);

		if (!File.Exists(strFileDest)) {
			Debug.LogWarning(string.Format("[Steamworks.NET] Could not copy {0} into the built project. File.Copy() Failed. Place {0} from the redist folder into the output dir manually.", filename));
		}
	}
}
