using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Steamworks.HeathenExtensions.Editor
{
    [CustomEditor(typeof(SteamSystem))]
    public class SteamSystemEditor : UnityEditor.Editor
    {
        public Texture2D dropBoxTexture;
        public static Color SteamBlue = new Color(0.2f, 0.60f, 0.93f, 1f);
        public static Color SteamGreen = new Color(0.2f, 0.42f, 0.2f, 1f);
        public static Color BrightGreen = new Color(0.4f, 0.84f, 0.4f, 1f);
        public static Color HalfAlpha = new Color(1f, 1f, 1f, 0.5f);
        public static Color ErrorRed = new Color(1, 0.5f, 0.5f, 1);

        private SteamSystem settings;
        bool needRefresh = false;

        public override void OnInspectorGUI()
        {
            settings = target as SteamSystem;

            if (needRefresh)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings), ImportAssetOptions.ForceUpdate);
                needRefresh = false;
            }

            ValidationChecks();
            DrawCommonSettings();

            DrawServerSettings();
        }

        private void ValidationChecks()
        {
            if (settings.server == null)
            {
                settings.server = new SteamSystem.GameServer();
                EditorUtility.SetDirty(settings);
            }

            if (settings.client == null)
            {
                settings.client = new SteamSystem.GameClient();
                EditorUtility.SetDirty(settings);
            }

            List<StatReference> toMove = new List<StatReference>();

            foreach (var stat in settings.stats)
            {
                if (AssetDatabase.GetAssetPath(stat) != AssetDatabase.GetAssetPath(settings))
                {
                    toMove.Add(stat);
                    Debug.Log("Moving " + AssetDatabase.GetAssetPath(stat) + " to " + AssetDatabase.GetAssetPath(settings));
                }
            }

            foreach (var stat in toMove)
            {
                settings.stats.Remove(stat);
                if (stat.GetType() == typeof(StatReferenceInt))
                {
                    StatReferenceInt nStat = new StatReferenceInt();
                    nStat.name = stat.statName;
                    nStat.statName = stat.statName;
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);

                    stat.name = "[REPLACED] " + stat.name;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stat));
                }
                else
                {
                    StatReferenceFloat nStat = new StatReferenceFloat();
                    nStat.name = stat.statName;
                    nStat.statName = stat.statName;
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);

                    stat.name = "[REPLACED] " + stat.name;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stat));
                }
            }

            List<AchievementReference> toMove2 = new List<AchievementReference>();

            foreach (var achievement in settings.achievements)
            {
                if (AssetDatabase.GetAssetPath(achievement) != AssetDatabase.GetAssetPath(settings))
                {
                    toMove2.Add(achievement);
                    Debug.Log("Moving " + AssetDatabase.GetAssetPath(achievement) + " to " + AssetDatabase.GetAssetPath(settings));
                }
            }

            foreach (var achievement in toMove2)
            {
                settings.achievements.Remove(achievement);

                AchievementReference nStat = new AchievementReference();
                nStat.name = achievement.achievementId;
                nStat.achievementId = achievement.achievementId;
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.achievements.Add(nStat);

                achievement.name = "[REPLACED] " + achievement.name;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(achievement));
            }
        }

        private void DrawCommonSettings()
        {
            var id = EditorGUILayout.TextField("Application Id", settings.applicationId.m_AppId.ToString());
            uint buffer = 0;
            if (uint.TryParse(id, out buffer))
            {
                if (buffer != settings.applicationId.m_AppId)
                {
                    var appIdPath = Application.dataPath.Replace("/Assets", "") + "/steam_appid.txt";
                    File.WriteAllText(appIdPath, buffer.ToString());
                    Undo.RecordObject(settings, "editor");
                    settings.applicationId = new AppId_t(buffer);
                    EditorUtility.SetDirty(settings);
                    Debug.LogWarning("When updating the App ID we also update the steam_appid.txt for you. You must restart Unity and Visual Studio for this change to take full effect as seen from the Steamworks Client.");
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawStatsList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawAchievementList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawLeaderboardList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawDLCList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Game Server Configuration", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawServerSettings()
        {
            EditorGUILayout.Space();
            DrawServerToggleSettings();
            EditorGUILayout.Space();
            DrawConnectionSettings();
            EditorGUILayout.Space();
            DrawServerGeneralSettings();
        }

        private void DrawServerGeneralSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("General", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            if (!settings.server.anonymousServerLogin)
            {
                EditorGUILayout.HelpBox("If anonymous server login is not enabled then you must provide a game server token.", MessageType.Info);

                var token = EditorGUILayout.TextField("Token", settings.server.gameServerToken);

                if (token != settings.server.gameServerToken)
                {
                    Undo.RecordObject(settings, "editor");
                    settings.server.gameServerToken = token;
                }
            }

            var serverName = EditorGUILayout.TextField("Server Name", settings.server.serverName);

            if (serverName != settings.server.serverName)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.serverName = serverName;
            }

            if (settings.server.supportSpectators)
            {
                serverName = EditorGUILayout.TextField("Spectator Name", settings.server.spectatorServerName);

                if (serverName != settings.server.spectatorServerName)
                {
                    Undo.RecordObject(settings, "editor");
                    settings.server.spectatorServerName = serverName;
                }
            }

            serverName = EditorGUILayout.TextField("Description", settings.server.gameDescription);

            if (serverName != settings.server.gameDescription)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.gameDescription = serverName;
            }

            serverName = EditorGUILayout.TextField("Directory", settings.server.gameDirectory);

            if (serverName != settings.server.gameDirectory)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.gameDirectory = serverName;
            }

            serverName = EditorGUILayout.TextField("Map Name", settings.server.mapName);

            if (serverName != settings.server.mapName)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.mapName = serverName;
            }

            serverName = EditorGUILayout.TextField("Game Metadata", settings.server.gameData);

            if (serverName != settings.server.gameData)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.gameData = serverName;
            }

            var count = EditorGUILayout.TextField("Max Player Count", settings.server.maxPlayerCount.ToString());
            int buffer;
            if (int.TryParse(count, out buffer) && buffer != settings.server.maxPlayerCount)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.maxPlayerCount = buffer;
            }

            count = EditorGUILayout.TextField("Bot Player Count", settings.server.botPlayerCount.ToString());

            if (int.TryParse(count, out buffer) && buffer != settings.server.botPlayerCount)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.botPlayerCount = buffer;
            }
        }

        private void DrawConnectionSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Connection", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var address = SteamSystem.IPUintToString(settings.server.ip);
            var nAddress = EditorGUILayout.TextField("IP Address", address);

            if (address != nAddress)
            {
                try
                {
                    var nip = SteamSystem.IPStringToUint(nAddress);
                    Undo.RecordObject(settings, "editor");
                    settings.server.ip = nip;
                }
                catch { }
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Ports ");
            EditorGUILayout.EndHorizontal();

            var port = EditorGUILayout.TextField(new GUIContent("Game", "The port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port."), settings.server.gamePort.ToString());
            ushort nPort;

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.gamePort)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.gamePort = nPort;
            }

            port = EditorGUILayout.TextField(new GUIContent("Query", "The port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE (65535) for QueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master server updater. See references to GameSocketShare in isteamgameserver.hn"), settings.server.queryPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.queryPort)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.queryPort = nPort;
            }

            if (settings.server.supportSpectators)
            {
                port = EditorGUILayout.TextField("Spectator", settings.server.spectatorPort.ToString());

                if (ushort.TryParse(port, out nPort) && nPort != settings.server.spectatorPort)
                {
                    Undo.RecordObject(settings, "editor");
                    settings.server.spectatorPort = nPort;
                }
            }
        }

        private void DrawServerToggleSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Features", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var autoInt = GUILayout.Toggle(settings.server.autoInitalize, (settings.server.autoInitalize ? "Disable" : "Enable") + " Auto-Initalize", EditorStyles.toolbarButton);
            var heart = GUILayout.Toggle(settings.server.enableHeartbeats, (settings.server.enableHeartbeats ? "Disable" : "Enable") + " Server Heartbeat", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var spec = GUILayout.Toggle(settings.server.supportSpectators, (settings.server.supportSpectators ? "Disable" : "Enable") + " Spectator Support", EditorStyles.toolbarButton);
            var anon = GUILayout.Toggle(settings.server.anonymousServerLogin, (settings.server.anonymousServerLogin ? "Disable" : "Enable") + " Anonymous Server Login", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var gsAuth = GUILayout.Toggle(settings.server.usingGameServerAuthApi, (settings.server.usingGameServerAuthApi ? "Disable" : "Enable") + " Game Server Auth API", EditorStyles.toolbarButton);
            var pass = GUILayout.Toggle(settings.server.isPasswordProtected, (settings.server.isPasswordProtected ? "Disable" : "Enable") + " Password Protected", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var dedicated = GUILayout.Toggle(settings.server.isDedicated, (settings.server.isDedicated ? "Disable" : "Enable") + " Dedicated Server", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            if (autoInt != settings.server.autoInitalize)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.autoInitalize = autoInt;
            }

            if (heart != settings.server.enableHeartbeats)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.enableHeartbeats = heart;
            }

            if (spec != settings.server.supportSpectators)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.supportSpectators = spec;
            }

            if (anon != settings.server.anonymousServerLogin)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.anonymousServerLogin = anon;
            }

            if (gsAuth != settings.server.usingGameServerAuthApi)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.usingGameServerAuthApi = gsAuth;
            }

            if (pass != settings.server.isPasswordProtected)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.isPasswordProtected = pass;
            }

            if (dedicated != settings.server.isDedicated)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.isDedicated = dedicated;
            }
        }

        private void DrawStatsList()
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = BrightGreen;
            if (GUILayout.Button("+ Int", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                StatReferenceInt nStat = new StatReferenceInt();
                nStat.name = "New Int Stat";
                nStat.statName = "New Int Stat";
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.stats.Add(nStat);
            }
            if (GUILayout.Button("+ Float", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                StatReferenceFloat nStat = new StatReferenceFloat();
                nStat.name = "New Float Stat";
                nStat.statName = "New Float Stat";
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.stats.Add(nStat);
            }
            GUI.contentColor = color;
            EditorGUILayout.LabelField("Stats", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            for (int i = 0; i < settings.stats.Count; i++)
            {
                var target = settings.stats[i];
                if (target == null)
                    continue;

                Color sC = GUI.backgroundColor;

                GUI.backgroundColor = sC;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                }

                var newName = EditorGUILayout.TextField(target.statName);
                if (!string.IsNullOrEmpty(newName) && newName != target.statName)
                {
                    Undo.RecordObject(target, "name change");
                    target.statName = newName;
                    target.name = newName;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }


                var terminate = false;
                GUI.contentColor = ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    if (AssetDatabase.GetAssetPath(settings.stats[i]) == AssetDatabase.GetAssetPath(settings))
                    {
                        AssetDatabase.RemoveObjectFromAsset(settings.stats[i]);
                        needRefresh = true;
                    }

                    settings.stats.RemoveAt(i);
                    terminate = true;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (terminate)
                    break;
            }
            EditorGUI.indentLevel = il;
        }

        private void DrawAchievementList()
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = BrightGreen;
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                AchievementReference nStat = new AchievementReference();
                nStat.name = "New Achievement";
                nStat.achievementId = "New Achievement";
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.achievements.Add(nStat);
            }
            GUI.contentColor = color;
            EditorGUILayout.LabelField("Achievements", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            for (int i = 0; i < settings.achievements.Count; i++)
            {
                var target = settings.achievements[i];

                if (target == null)
                    continue;

                Color sC = GUI.backgroundColor;

                GUI.backgroundColor = sC;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(target);
                }

                var newName = EditorGUILayout.TextField(target.achievementId);
                if (!string.IsNullOrEmpty(newName) && newName != target.achievementId)
                {
                    Undo.RecordObject(target, "name change");
                    target.achievementId = newName;
                    target.name = newName;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                var terminate = false;
                GUI.contentColor = ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    if (AssetDatabase.GetAssetPath(settings.achievements[i]) == AssetDatabase.GetAssetPath(settings))
                    {
                        AssetDatabase.RemoveObjectFromAsset(settings.achievements[i]);
                        needRefresh = true;
                    }
                    settings.achievements.RemoveAt(i);
                    terminate = true;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (terminate)
                    break;
            }
            EditorGUI.indentLevel = il;
        }

        private void DrawLeaderboardList()
        {
            if (settings.leaderboards == null)
                settings.leaderboards = new List<LeaderboardReference>();

            settings.leaderboards.RemoveAll(p => p == null);
            if (settings.leaderboards == null)
                settings.leaderboards = new List<LeaderboardReference>();

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = BrightGreen;
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                LeaderboardReference nStat = new LeaderboardReference();
                nStat.name = "New Leaderboard";
                nStat.leaderboardName = "New Leaderboard";
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.leaderboards.Add(nStat);
            }
            GUI.contentColor = color;
            EditorGUILayout.LabelField("Leaderboards", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var bgColor = GUI.backgroundColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.leaderboards.Count; i++)
            {
                var item = settings.leaderboards[i];

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                var nVal = EditorGUILayout.TextField(item.leaderboardName);
                if (nVal != item.leaderboardName)
                {
                    item.leaderboardName = nVal;
                    item.name = nVal;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                GUIContent detailsContent = new GUIContent("Details:", "This is the number of detail values that will be loaded for entries when querying the leaderboard. Details are an int array and can be used to assoceate general data with a given entry such as class, rank, level, map, etc.");
                EditorGUILayout.LabelField(detailsContent, GUILayout.Width(65));
                var nCount = EditorGUILayout.TextField(item.maxDetailEntries.ToString(), GUILayout.Width(75));
                int nCountBuffer = 0;
                if (int.TryParse(nCount, out nCountBuffer))
                {
                    item.maxDetailEntries = nCountBuffer;
                }

                GUI.contentColor = ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    if (AssetDatabase.GetAssetPath(settings.leaderboards[i]) == AssetDatabase.GetAssetPath(settings))
                    {
                        AssetDatabase.RemoveObjectFromAsset(settings.leaderboards[i]);
                        needRefresh = true;
                    }
                    settings.leaderboards.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
            GUI.backgroundColor = bgColor;


        }

        private void DrawDLCList()
        {
            if (settings.dlc == null)
                settings.dlc = new List<DownloadableContentReference>();

            settings.dlc.RemoveAll(p => p == null);
            if (settings.dlc == null)
                settings.dlc = new List<DownloadableContentReference>();

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = BrightGreen;
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                DownloadableContentReference nStat = new DownloadableContentReference();
                nStat.name = "New DLC";
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.dlc.Add(nStat);
            }
            GUI.contentColor = color;
            EditorGUILayout.LabelField("Downloadable Content", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var bgColor = GUI.backgroundColor;
            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < settings.dlc.Count; i++)
            {
                var item = settings.dlc[i];

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                var nVal = EditorGUILayout.TextField(item.name);
                if (nVal != item.name)
                {
                    item.name = nVal;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                GUIContent detailsContent = new GUIContent("App ID:", "The unique app id of the DLC.");
                EditorGUILayout.LabelField(detailsContent, GUILayout.Width(65));
                var nCount = EditorGUILayout.TextField(item.AppId.ToString(), GUILayout.Width(75));
                uint nCountBuffer = 0;
                if (uint.TryParse(nCount, out nCountBuffer))
                {
                    item.AppId = new AppId_t(nCountBuffer);
                }

                GUI.contentColor = ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    if (AssetDatabase.GetAssetPath(settings.dlc[i]) == AssetDatabase.GetAssetPath(settings))
                    {
                        AssetDatabase.RemoveObjectFromAsset(settings.dlc[i]);
                        needRefresh = true;
                    }
                    settings.dlc.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
            GUI.backgroundColor = bgColor;
        }

    }
}

