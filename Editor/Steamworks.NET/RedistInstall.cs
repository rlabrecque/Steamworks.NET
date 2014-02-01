using UnityEngine;
using UnityEditor;
using System.IO;

// This code copys various files into their required locations to make installation a breeze.
[InitializeOnLoad]
public class RedistInstall {
	const string SteamAPIRelativeLoc = "Assets/Plugins/Steamworks.NET/redist";

	static RedistInstall() {
		CopyFile("steam_appid.txt", false);
#if UNITY_EDITOR_WIN
		CopyFile("steam_api.dll", true);
#endif
	}

	static void CopyFile(string filename, bool bCheckDifference) {
		string strCWD = Directory.GetCurrentDirectory();
		string strSource = Path.Combine(Path.Combine(strCWD, SteamAPIRelativeLoc), filename);
		string strDest = Path.Combine(strCWD, filename);

		if (File.Exists(strDest)) {
			if (!bCheckDifference)
				return;

			System.DateTime fTime = File.GetLastWriteTime(strSource);
			System.DateTime fTime2 = File.GetLastWriteTime(strDest);
			if (fTime == fTime2) {
				FileInfo fInfo = new FileInfo(strSource);
				FileInfo fInfo2 = new FileInfo(strDest);
				if (fInfo.Length == fInfo2.Length) {
					return;
				}
			}

			Debug.Log(System.String.Format("[Steamworks.NET] {0} in the project root differs from the Steamworks.NET redistributable. Updating...", filename));
		}
		else {
			Debug.Log(System.String.Format("[Steamworks.NET] {0} is not present in the project root. Copying...", filename));
		}

		if (!File.Exists(strSource)) {
			Debug.LogWarning(System.String.Format("[Steamworks.NET] Could not copy {0} into the project root. {0} could not be found in '{1}'. Place {0} from the Steamworks SDK in the project root manually.", filename, Path.Combine(strCWD, SteamAPIRelativeLoc)));
			return;
		}

		File.Copy(strSource, strDest, true);

		if (File.Exists(strDest)) {
			Debug.Log(System.String.Format("[Steamworks.NET] Successfully copied {0} into the project root. Please relaunch Unity.", filename));
		}
		else {
			Debug.LogWarning(System.String.Format("[Steamworks.NET] Could not copy {0} into the project root. File.Copy() Failed. Place {0} from the Steamworks SDK in the project root manually.", filename));
		}
	}
}
