// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2021 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

using UnityEngine;
using UnityEditor;
using System.IO;

// This copys various files into their required locations when Unity is launched to make installation a breeze.
[InitializeOnLoad]
public class RedistInstall {
	static RedistInstall() {
		WriteSteamAppIdTxtFile();
	}

	static void WriteSteamAppIdTxtFile() {
		string strCWD = Directory.GetCurrentDirectory();
		string strDest = Path.Combine(strCWD, "steam_appid.txt");

		// If the steam_appid.txt file already exists, then we skip this!
		if (File.Exists(strDest)) {
			return;
		}

		Debug.Log("[Steamworks.NET] 'steam_appid.txt' is not present in the project root. Writing...");

		try
		{
			StreamWriter appIdFile = File.CreateText(strDest);
			appIdFile.Write("480");
			appIdFile.Close();

			Debug.Log("[Steamworks.NET] Successfully copied 'steam_appid.txt' into the project root. Please relaunch Unity.");
		}
		catch (System.Exception e) {
			Debug.LogWarning("[Steamworks.NET] Could not copy 'steam_appid.txt' into the project root. Please place 'steam_appid.txt' into the project root manually.");
			Debug.LogException(e);
		}
	}
}
