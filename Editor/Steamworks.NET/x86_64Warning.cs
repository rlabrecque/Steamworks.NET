// Uncomment this out to disable warning
//#define DISABLEX86_64WARNING

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class x86_64Warning {
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
#if !DISABLEX86_64WARNING
		if (target != BuildTarget.StandaloneOSXIntel && target != BuildTarget.StandaloneWindows && target != BuildTarget.StandaloneLinux) {
			Debug.LogWarning("[Steamworks.NET] Currently only 32bit builds are supported.");
		}
#endif
	}
}
