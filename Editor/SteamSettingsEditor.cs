#if !DISABLESTEAMWORKS
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamAPI.Editors
{
    [CustomEditor(typeof(SteamSettings))]
    public class SteamSettingsEditor : Editor
    {
        public Texture2D dropBoxTexture;

        private SteamSettings settings;
        private static bool showClient = true;

        public override void OnInspectorGUI()
        {
            settings = target as SteamSettings;
            ValidationChecks();
            DrawCommonSettings();
            DrawClientServerToggle();

            if (showClient)
                DrawClientSettings();
            else
                DrawServerSettings();
        }

        private void ValidationChecks()
        {
            if (settings.server == null)
            {
                settings.server = new SteamSettings.GameServer();
                EditorUtility.SetDirty(settings);
            }

            if (settings.client == null)
            {
                settings.client = new SteamSettings.GameClient();
                EditorUtility.SetDirty(settings);
            }

            if (settings.client.user == null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(settings)))
            {
                try
                {
                    UserData newData = new UserData();
                    newData.name = "Local User Data";
                    AssetDatabase.AddObjectToAsset(newData, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    settings.client.user = newData;
                }
                catch
                { 
                }
            }

            List<StatObject> toMove = new List<StatObject>();

            foreach(var stat in settings.stats)
            {
                if(AssetDatabase.GetAssetPath(stat) != AssetDatabase.GetAssetPath(settings))
                {
                    toMove.Add(stat);
                    Debug.Log("Moving " + AssetDatabase.GetAssetPath(stat) + " to " + AssetDatabase.GetAssetPath(settings));
                }
            }

            foreach(var stat in toMove)
            {
                settings.stats.Remove(stat);
                if(stat.GetType() == typeof(IntStatObject))
                {
                    IntStatObject nStat = new IntStatObject();
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
                    FloatStatObject nStat = new FloatStatObject();
                    nStat.name = stat.statName;
                    nStat.statName = stat.statName;
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);

                    stat.name = "[REPLACED] " + stat.name;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stat));
                }
            }

            List<AchievementObject> toMove2 = new List<AchievementObject>();

            foreach(var achievement in settings.achievements)
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

                AchievementObject nStat = new AchievementObject();
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
            var debug = GUILayout.Toggle(settings.isDebugging, "Enable Debug Messages", EditorStyles.toolbarButton);
            if(settings.isDebugging != debug)
            {
                Undo.RecordObject(settings, "editor");
                settings.isDebugging = debug;
            }

            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            HashSet<string> defines = new HashSet<string>(currentDefines.Split(';'));

            GUIContent ccContent = new GUIContent("Enable Conditional Compile", "When enabled script defines will be configured such that on compile code restricted to use on Client builds will be striped from Server builds and code restricted to use on Server builds will be stripped from Client builds.\n\nNote that this will force a recompile of the project which may take time.");

            if(defines.Contains("CONDITIONAL_COMPILE"))
            {
                if(!GUILayout.Toggle(true, ccContent, EditorStyles.toolbarButton))
                {
                    defines.Remove("CONDITIONAL_COMPILE");

                    string newDefines = string.Join(";", defines);
                    if (newDefines != currentDefines)
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
                    }
                }
            }
            else
            {
                if (GUILayout.Toggle(false, ccContent, EditorStyles.toolbarButton))
                {
                    defines.Add("CONDITIONAL_COMPILE");

                    string newDefines = string.Join(";", defines);
                    if (newDefines != currentDefines)
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
                    }
                }
            }

#if HE_STEAMPLAYERSERVICES
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GeneralDropAreaGUI("Drop inventory and data models here.");
            RemoteStorageHeader();
#endif
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //GeneralDropAreaGUI("... Drop Legacy Objects Here ...");
            DrawStatsList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawAchievementList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
#if HE_STEAMPLAYERSERVICES
            DrawLeaderboardList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawDLCList();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
#endif
#if HE_STEAMCOMPLETE
            DrawInventoryArea();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
#endif

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Build", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

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

                    Debug.LogWarning("When updating the App ID we also update the steam_appid.txt for you. You must restart Unity and Visual Studio for this change to take full effect as seen from the Steamworks Client.");
                }
            }
        }

        private void DrawClientServerToggle()
        {
            EditorGUILayout.BeginHorizontal();
            showClient = GUILayout.Toggle(showClient, "Client", EditorStyles.toolbarButton);
            showClient = !GUILayout.Toggle(!showClient, "Server", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawClientSettings()
        {
            if (needRefresh)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings), ImportAssetOptions.ForceUpdate);
                needRefresh = false;
            }

            DrawAppOverlayData();
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

            var address = SteamSettings.IPUintToString(settings.server.ip);
            var nAddress = EditorGUILayout.TextField("IP Address", address);
            
            if(address != nAddress)
            {
                try
                {
                    var nip = SteamSettings.IPStringToUint(nAddress);
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
            
            if(ushort.TryParse(port, out nPort) && nPort != settings.server.queryPort)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.queryPort = nPort;
            }

            port = EditorGUILayout.TextField("Spectator", settings.server.spectatorPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.spectatorPort)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.spectatorPort = nPort;
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
            var mirror = GUILayout.Toggle(settings.server.enableMirror, (settings.server.enableMirror ? "Disable" : "Enable") + " Mirror Support", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            if (autoInt != settings.server.autoInitalize)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.autoInitalize = autoInt;
            }

            if (mirror != settings.server.enableMirror)
            {
                Undo.RecordObject(settings, "editor");
                settings.server.enableMirror = mirror;
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

        private void DrawAppOverlayData()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Notification Popup", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Popup Position");

            int cSelected = (int)settings.client.overlay.notificationPosition;

            EditorGUILayout.BeginVertical();
            cSelected = EditorGUILayout.Popup(cSelected, new string[] { "Top Left", "Top Right", "Bottom Left", "Bottom Right" });

            var v = EditorGUILayout.Vector2IntField(GUIContent.none, settings.client.overlay.notificationInset);
            if (settings.client.overlay.notificationInset != v)
            {
                settings.client.overlay.notificationInset = v;
                EditorUtility.SetDirty(settings);
            }
            EditorGUILayout.EndVertical();

            if (settings.client.overlay.notificationPosition != (ENotificationPosition)cSelected)
            {
                settings.client.overlay.notificationPosition = (ENotificationPosition)cSelected;
                EditorUtility.SetDirty(settings);
            }

            EditorGUILayout.EndHorizontal();
        }

        bool needRefresh = false;

        private void DrawStatsList()
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = SteamSettings.Colors.BrightGreen;
            if (GUILayout.Button("+ Int", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                IntStatObject nStat = new IntStatObject();
                nStat.name = "New Int Stat";
                nStat.statName = "New Int Stat";
                AssetDatabase.AddObjectToAsset(nStat, settings);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                settings.stats.Add(nStat);
            }
            if (GUILayout.Button("+ Float", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                FloatStatObject nStat = new FloatStatObject();
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
                if(!string.IsNullOrEmpty(newName) && newName != target.statName)
                {
                    Undo.RecordObject(target, "name change");
                    target.statName = newName;
                    target.name = newName;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                }

                
                var terminate = false;
                GUI.contentColor = SteamSettings.Colors.ErrorRed;
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
            GUI.contentColor = SteamSettings.Colors.BrightGreen;
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                AchievementObject nStat = new AchievementObject();
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
                GUI.contentColor = SteamSettings.Colors.ErrorRed;
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

#if HE_STEAMPLAYERSERVICES
        private bool GeneralDropAreaGUI(string message)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 70.0f, GUILayout.ExpandWidth(true));

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = dropBoxTexture;
            style.normal.textColor = Color.white;
            style.border = new RectOffset(5, 5, 5, 5);
            var color = GUI.backgroundColor;
            var fontColor = GUI.contentColor;
            GUI.backgroundColor = SteamSettings.Colors.SteamGreen * SteamSettings.Colors.HalfAlpha;
            GUI.contentColor = SteamSettings.Colors.BrightGreen;
            GUI.Box(drop_area, "\n\n" + message, style);
            GUI.backgroundColor = color;
            GUI.contentColor = fontColor;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    bool retVal = false;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            if (dragged_object.GetType().IsAssignableFrom(typeof(InventoryItemBundleDefinition)))
                            {
                                InventoryItemBundleDefinition go = dragged_object as InventoryItemBundleDefinition;
                                if (go != null)
                                {
                                    if (!settings.client.inventory.itemBundles.Exists(p => p == go))
                                    {
                                        Undo.RecordObject(settings, "drop");
                                        settings.client.inventory.itemBundles.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsAssignableFrom(typeof(TagGeneratorDefinition)))
                            {
                                TagGeneratorDefinition go = dragged_object as TagGeneratorDefinition;
                                if (go != null)
                                {
                                    if (!settings.client.inventory.tagGenerators.Exists(p => p == go))
                                    {
                                        Undo.RecordObject(settings, "drop");
                                        settings.client.inventory.tagGenerators.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsAssignableFrom(typeof(ItemGeneratorDefinition)))
                            {
                                ItemGeneratorDefinition go = dragged_object as ItemGeneratorDefinition;
                                if (go != null)
                                {
                                    if (!settings.client.inventory.itemGenerators.Exists(p => p == go))
                                    {
                                        Undo.RecordObject(settings, "drop");
                                        settings.client.inventory.itemGenerators.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsSubclassOf(typeof(InventoryItemDefinition)))
                            {
                                InventoryItemDefinition go = dragged_object as InventoryItemDefinition;
                                if (go != null)
                                {
                                    if (!settings.client.inventory.itemDefinitions.Exists(p => p == go))
                                    {
                                        Undo.RecordObject(settings, "drop");
                                        settings.client.inventory.itemDefinitions.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsSubclassOf(typeof(DataModel)))
                            {
                                DataModel go = dragged_object as DataModel;
                                if (go != null)
                                {
                                    if (!settings.client.remoteStorage.DataModels.Exists(p => p == go))
                                    {
                                        Undo.RecordObject(settings, "drop");
                                        settings.client.remoteStorage.DataModels.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                            else if (dragged_object.GetType().IsAssignableFrom(typeof(FileDataModel)))
                            {
                                FileDataModel go = dragged_object as FileDataModel;
                                if (go != null)
                                {
                                    if (!settings.client.remoteStorage.LegacyDataModels.Exists(p => p == go))
                                    {
                                        Undo.RecordObject(settings, "drop"); 

                                        settings.client.remoteStorage.LegacyDataModels.Add(go);
                                        EditorUtility.SetDirty(settings);
                                        retVal = true;
                                    }
                                }
                            }
                        }
                    }

                    return retVal;
            }

            return false;
        }

        private void DrawLeaderboardList()
        {
            if (settings.leaderboards == null)
                settings.leaderboards = new List<LeaderboardObject>();

            settings.leaderboards.RemoveAll(p => p == null);
            if (settings.leaderboards == null)
                settings.leaderboards = new List<LeaderboardObject>();

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = SteamSettings.Colors.BrightGreen;
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                LeaderboardObject nStat = new LeaderboardObject();
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
                if(int.TryParse(nCount, out nCountBuffer))
                {
                    item.maxDetailEntries = nCountBuffer;
                }

                GUI.contentColor = SteamSettings.Colors.ErrorRed;
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
                settings.dlc = new List<DownloadableContentObject>();

            settings.dlc.RemoveAll(p => p == null);
            if (settings.dlc == null)
                settings.dlc = new List<DownloadableContentObject>();

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = SteamSettings.Colors.BrightGreen;
            if (GUILayout.Button("+ New", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);

                DownloadableContentObject nStat = new DownloadableContentObject();
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

                GUI.contentColor = SteamSettings.Colors.ErrorRed;
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

        private void RemoteStorageHeader()
        {
            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Remote Storage Data Models", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            for (int i = 0; i < settings.client.remoteStorage.DataModels.Count; i++)
            {
                var target = settings.client.remoteStorage.DataModels[i];

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

                EditorGUILayout.LabelField(target.name);

                var newName = EditorGUILayout.TextField(target.extension);
                if (!string.IsNullOrEmpty(newName) && newName != target.extension)
                {
                    Undo.RecordObject(target, "name change");
                    target.extension = newName;
                }

                var terminate = false;
                GUI.contentColor = SteamSettings.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.client.remoteStorage.DataModels.RemoveAt(i);
                    terminate = true;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (terminate)
                    break;
            }

            for (int i = 0; i < settings.client.remoteStorage.LegacyDataModels.Count; i++)
            {
                var target = settings.client.remoteStorage.LegacyDataModels[i];

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

                EditorGUILayout.LabelField(target.name);

                var newName = EditorGUILayout.TextField(target.filePrefix);
                if (!string.IsNullOrEmpty(newName) && newName != target.filePrefix)
                {
                    Undo.RecordObject(target, "name change");
                    target.filePrefix = newName;
                }

                var terminate = false;
                GUI.contentColor = SteamSettings.Colors.ErrorRed;
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    settings.client.remoteStorage.LegacyDataModels.RemoveAt(i);
                    terminate = true;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (terminate)
                    break;
            }

            EditorGUI.indentLevel = il;
        }
#endif

#if HE_STEAMCOMPLETE
        private void DrawInventoryArea()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            var color = GUI.contentColor;
            GUI.contentColor = SteamSettings.Colors.BrightGreen;
            if (GUILayout.Button("Editor", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                GUI.FocusControl(null);
                InventoryIMGUITool.steamSettings = settings;
                SteamInspector_Code.ShowExample();
                SteamInspector_Code.instance.inventoryToggle.value = true;
            }
            GUI.contentColor = color;
            EditorGUILayout.LabelField("Inventory", EditorStyles.whiteLabel);

            EditorGUILayout.EndHorizontal();



            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            List<CraftingRecipe> recipie = new List<CraftingRecipe>();

            foreach (var item in settings.client.inventory.itemDefinitions)
            {
                if (item.Recipes != null)
                {
                    foreach (var rec in item.Recipes)
                    {
                        if (!recipie.Contains(rec))
                            recipie.Add(rec);
                    }
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                EditorGUILayout.LabelField(item.name);
                EditorGUILayout.EndHorizontal();
            }

            foreach (var item in settings.client.inventory.itemBundles)
            {
                if (item.Recipes != null)
                {
                    foreach (var rec in item.Recipes)
                    {
                        if (!recipie.Contains(rec))
                            recipie.Add(rec);
                    }
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                EditorGUILayout.LabelField(item.name);
                EditorGUILayout.EndHorizontal();
            }

            foreach (var item in settings.client.inventory.itemGenerators)
            {
                if (item.Recipes != null)
                {
                    foreach (var rec in item.Recipes)
                    {
                        if (!recipie.Contains(rec))
                            recipie.Add(rec);
                    }
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                EditorGUILayout.LabelField(item.name);
                EditorGUILayout.EndHorizontal();
            }

            foreach (var item in settings.client.inventory.tagGenerators)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                EditorGUILayout.LabelField(item.name);
                EditorGUILayout.EndHorizontal();
            }

            foreach (var item in recipie)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(item);
                }

                EditorGUILayout.LabelField(item.name);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel = il;
        }
#endif
    }
}
#endif