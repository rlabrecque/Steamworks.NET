using UnityEngine;
using UnityEditor;
using System.IO;

// This copys various files into their required locations when Unity is launched to make installation a breeze.
[InitializeOnLoad]
public class RedistInstall {
	static RedistInstall() {
		CopyFile("Assets/Plugins/Steamworks.NET/redist", "steam_appid.txt", false);
#if UNITY_EDITOR_WIN && !UNITY_5
	#if UNITY_EDITOR_64
		CopyFile("Assets/Plugins/x86_64", "steam_api64.dll", true);
	#else
		CopyFile("Assets/Plugins/x86", "steam_api.dll", true);
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
}
