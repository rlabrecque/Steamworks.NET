#if !DISABLESTEAMWORKS
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamAPI.Editors
{
    static class SteamAppIdWriter
    {
        /// <summary>
        /// Creates the steam_appid.txt file if its missing
        /// </summary>
        [InitializeOnLoadMethod]
        public static void CreateAppIdTextFileIfMissing()
        {
            var appIdPath = Application.dataPath.Replace("/Assets", "") + "/steam_appid.txt";
            if (!File.Exists(appIdPath))
                File.WriteAllText(appIdPath, "480");
        }
    }
}
#endif