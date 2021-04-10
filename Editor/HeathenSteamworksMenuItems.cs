#if !DISABLESTEAMWORKS
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamAPI.Editors
{
    public class HeathenSteamworksMenuItems
    {
        [MenuItem("Steamworks/Tools/Steamworks.NET Steam API (On GitHub)")]
        public static void SteamworksGetHub()
        {
            Application.OpenURL("https://github.com/rlabrecque/Steamworks.NET/releases");
            Debug.Log("Download the latest Steamworks.NET unity package from GetHub and import it to your project, you should also update the reference in your Install Settings.");
        }

        [MenuItem("Steamworks/Tools/Mirror Networking API (On Unity Asset Store)")]
        public static void MirrorAssetStore()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/network/mirror-129321");
            Debug.Log("Mirror is one of the integrated networking options support by Heathens Steamworks.\nIf you choose to use Mirror be sure to install Heathen Steamworks's Mirror Integration package after you install Mirror's base package.");
        }

        [MenuItem("Help/Steamworks/Valve's Steam API Documentation")]
        [MenuItem("Steamworks/Help/Valve's Steam API Documentation")]
        public static void ValvesDocuments()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/home");
        }

        [MenuItem("Help/Steamworks/Heathen's Steamworks User Documentation")]
        [MenuItem("Steamworks/Help/Heathen's Steamworks User Documentation")]
        public static void HeathenSteamworksDocumentation()
        {
            Application.OpenURL("https://heathen-engineering.mn.co/topics");
        }

        [MenuItem("Help/Steamworks/Heathen's Steamworks Class Documentation")]
        [MenuItem("Steamworks/Help/Heathen's Steamworks Class Documentation")]
        public static void HeathenSteamworksClassDocumentation()
        {
            Application.OpenURL("https://heathen-engineering.github.io/steamworks-v2-documentation/annotated.html");
        }

        [MenuItem("Help/Steamworks/Heathen Discord Community (Recomended)")]
        [MenuItem("Steamworks/Help/Heathen Discord Community (Recomended)")]
        public static void HeathenDiscord()
        {
            Application.OpenURL("https://discord.gg/RMGtDXV");
        }

        [MenuItem("Help/Steamworks/Valve's Developer Forums")]
        [MenuItem("Steamworks/Help/Valve's Developer Forums")]
        public static void ValvesForums()
        {
            Application.OpenURL("https://steamcommunity.com/groups/steamworks");
        }

        [MenuItem("Help/Steamworks/Valve's Developer Support")]
        [MenuItem("Steamworks/Help/Valve's Developer Support")]
        public static void ValvesSupport()
        {
            Application.OpenURL("https://partner.steamgames.com/home/contact");
        }
    }
}
#endif