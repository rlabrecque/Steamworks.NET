#if !DISABLESTEAMWORKS
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamAPI.Editors
{

    [CustomEditor(typeof(SteamSystem))]
    public class SteamworksClientApiSystemEditor : Editor
    {
        private SteamSystem pManager;
        private SerializedProperty Settings;

        //General
        private SerializedProperty DoNotDestroyOnLoad;
        private SerializedProperty OnSteamInitalized;
        private SerializedProperty OnSteamInitalizationError;
        
        //Client
        private SerializedProperty OnAvatarLoaded;
        private SerializedProperty OnPersonaStateChanged;
        private SerializedProperty OnUserStatsRecieved;
        private SerializedProperty OnUserStatsStored;
        private SerializedProperty OnOverlayActivated;
        private SerializedProperty OnAchievementStored;
        private SerializedProperty OnRecievedFriendChatMessage;
        private SerializedProperty OnNumberOfCurrentPlayersResult;

#if HE_STEAMPLAYERSERVICES
        private SerializedProperty leaderboardRankChanged;
        private SerializedProperty leaderboardRankLoaded;
        private SerializedProperty leaderboardNewHighRank;
        private SerializedProperty fileReadAsyncComplete;
        private SerializedProperty fileWriteAsyncComplete;
        private SerializedProperty fileShareResult;
#endif
#if HE_STEAMCOMPLETE
        public SerializedProperty itemInstancesUpdated;
        public SerializedProperty itemsGranted;
        public SerializedProperty itemsConsumed;
        public SerializedProperty itemsExchanged;
        public SerializedProperty itemsDroped;

        public SerializedProperty OnGameLobbyJoinRequest;
        public SerializedProperty OnLobbyMatchList;
        public SerializedProperty OnLobbyCreated;
        public SerializedProperty OnLobbyEnter;
        public SerializedProperty OnLobbyExit;
        public SerializedProperty OnGameServerSet;
        public SerializedProperty OnLobbyChatUpdate;
        public SerializedProperty QuickMatchFailed;
        public SerializedProperty SearchStarted;
        public SerializedProperty OnChatMessageReceived;
#endif

        //Server
        private SerializedProperty disconnected;
        private SerializedProperty connected;
        private SerializedProperty failure;

        public Texture2D achievementIcon;
        public Texture2D statIcon;
        public Texture2D leaderboardIcon;
        public Texture2D dlcIcon;
        public Texture itemIcon;
        public Texture generatorIcon;
        public Texture tagIcon;
        public Texture bundleIcon;
        public Texture recipeIcon;
        public Texture pointerIcon;
        public Texture2D dropBoxTexture;

        private int tabPage = 0;

        private void OnEnable()
        {
            Settings = serializedObject.FindProperty(nameof(SteamSystem.settings));

            OnSteamInitalized = serializedObject.FindProperty(nameof(SteamSystem.evtSteamInitalized));
            OnSteamInitalizationError = serializedObject.FindProperty(nameof(SteamSystem.evtSteamInitalizationError));
            OnOverlayActivated = serializedObject.FindProperty(nameof(SteamSystem.evtOverlayActivated));

#if HE_STEAMPLAYERSERVICES
            leaderboardRankChanged = serializedObject.FindProperty(nameof(ClientApiSystem.evtLeaderboardRankChanged));
            leaderboardRankLoaded = serializedObject.FindProperty(nameof(ClientApiSystem.evtLeaderboardRankLoaded));
            leaderboardNewHighRank = serializedObject.FindProperty(nameof(ClientApiSystem.evtLeaderboardNewHighRank));

            fileReadAsyncComplete = serializedObject.FindProperty(nameof(ClientApiSystem.evtFileReadAsyncComplete));
            fileWriteAsyncComplete = serializedObject.FindProperty(nameof(ClientApiSystem.evtFileWriteAsyncComplete));
            fileShareResult = serializedObject.FindProperty(nameof(ClientApiSystem.evtFileShareResult));
#endif

#if HE_STEAMCOMPLETE
            itemInstancesUpdated = serializedObject.FindProperty(nameof(ClientApiSystem.evtItemInstancesUpdated));
            itemsGranted = serializedObject.FindProperty(nameof(ClientApiSystem.evtItemsGranted));
            itemsConsumed = serializedObject.FindProperty(nameof(ClientApiSystem.evtItemsConsumed));
            itemsExchanged = serializedObject.FindProperty(nameof(ClientApiSystem.evtItemsExchanged));
            itemsDroped = serializedObject.FindProperty(nameof(ClientApiSystem.evtItemsDroped));

            OnGameLobbyJoinRequest = serializedObject.FindProperty(nameof(ClientApiSystem.evtGameLobbyJoinRequest));
            OnLobbyMatchList = serializedObject.FindProperty(nameof(ClientApiSystem.evtLobbyMatchList));
            OnLobbyCreated = serializedObject.FindProperty(nameof(ClientApiSystem.evtLobbyCreated));
            OnLobbyEnter = serializedObject.FindProperty(nameof(ClientApiSystem.evtLobbyEnter));
            OnLobbyExit = serializedObject.FindProperty(nameof(ClientApiSystem.evtLobbyExit));
            OnGameServerSet = serializedObject.FindProperty(nameof(ClientApiSystem.evtGameServerSet));
            OnLobbyChatUpdate = serializedObject.FindProperty(nameof(ClientApiSystem.evtLobbyChatUpdate));
            QuickMatchFailed = serializedObject.FindProperty(nameof(ClientApiSystem.evtQuickMatchFailed));
            SearchStarted = serializedObject.FindProperty(nameof(ClientApiSystem.evtSearchStarted));
            OnChatMessageReceived = serializedObject.FindProperty(nameof(ClientApiSystem.evtOnChatMessageReceived));
#endif

            OnUserStatsRecieved = serializedObject.FindProperty(nameof(SteamSystem.evtUserStatsRecieved));
            OnUserStatsStored = serializedObject.FindProperty(nameof(SteamSystem.evtUserStatsStored));
            OnAchievementStored = serializedObject.FindProperty(nameof(SteamSystem.evtAchievementStored));
            OnAvatarLoaded = serializedObject.FindProperty(nameof(SteamSystem.evtAvatarLoaded));
            OnPersonaStateChanged = serializedObject.FindProperty(nameof(SteamSystem.evtPersonaStateChanged));
            OnRecievedFriendChatMessage = serializedObject.FindProperty(nameof(SteamSystem.evtRecievedFriendChatMessage));

            disconnected = serializedObject.FindProperty(nameof(SteamSystem.evtDisconnected));
            connected = serializedObject.FindProperty(nameof(SteamSystem.evtConnected));
            failure = serializedObject.FindProperty(nameof(SteamSystem.evtFailure));

            OnNumberOfCurrentPlayersResult = serializedObject.FindProperty(nameof(SteamSystem.evtNumberOfCurrentPlayersResult));
        }

        public override void OnInspectorGUI()
        {
            pManager = target as SteamSystem;

            if (pManager != null)
            {
                if (pManager.settings != null)
                {
                    if (pManager.settings.client == null)
                        pManager.settings.client = new SteamSettings.GameClient();

                    if (pManager.settings.server == null)
                        pManager.settings.server = new SteamSettings.GameServer();

                    if (pManager.settings.achievements == null)
                        pManager.settings.achievements = new System.Collections.Generic.List<AchievementObject>();

                    if (pManager.settings.stats == null)
                        pManager.settings.stats = new System.Collections.Generic.List<StatObject>();

                    pManager.settings.stats.RemoveAll(p => p == null);
                    pManager.settings.achievements.RemoveAll(p => p == null);
                }
            }
            
            EditorGUILayout.PropertyField(Settings, GUIContent.none, true);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events");
            Rect hRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");

            Rect bRect = new Rect(hRect);
            bRect.width = (hRect.width - 25) / 3f;
            var tWidth = bRect.width;
            bRect.width = 25;
            tabPage = GUI.Toggle(bRect, tabPage == 0, "X", EditorStyles.toolbarButton) ? 0 : tabPage;
            bRect.x += 25;
            bRect.width = tWidth;
            tabPage = GUI.Toggle(bRect, tabPage == 1, "Common", EditorStyles.toolbarButton) ? 1 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 2, "Stat & Achievements", EditorStyles.toolbarButton) ? 2 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 3, "Game Server", EditorStyles.toolbarButton) ? 3 : tabPage;
            EditorGUILayout.EndHorizontal();

            Rect nhRect = EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");

#if HE_STEAMCOMPLETE
            bRect = new Rect(nhRect);
            bRect.width = nhRect.width / 4f;
            tabPage = GUI.Toggle(bRect, tabPage == 4, "Leaderboard", EditorStyles.toolbarButton) ? 4 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 5, "Remote Storage", EditorStyles.toolbarButton) ? 5 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 6, "Inventory", EditorStyles.toolbarButton) ? 6 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 7, "Lobby", EditorStyles.toolbarButton) ? 7 : tabPage;
#elif HE_STEAMPLAYERSERVICES
            bRect = new Rect(nhRect);
            bRect.width = nhRect.width / 2f;
            tabPage = GUI.Toggle(bRect, tabPage == 4, "Leaderboard", EditorStyles.toolbarButton) ? 4 : tabPage;
            bRect.x += bRect.width;
            tabPage = GUI.Toggle(bRect, tabPage == 5, "Remote Storage", EditorStyles.toolbarButton) ? 5 : tabPage;
#endif
            EditorGUILayout.EndHorizontal();

            switch(tabPage)
            {
                case 1:
                    EditorGUILayout.PropertyField(OnSteamInitalized);
                    EditorGUILayout.PropertyField(OnSteamInitalizationError);
                    EditorGUILayout.PropertyField(OnOverlayActivated);
                    EditorGUILayout.PropertyField(OnPersonaStateChanged);
                    EditorGUILayout.PropertyField(OnNumberOfCurrentPlayersResult);
                    EditorGUILayout.PropertyField(OnRecievedFriendChatMessage);
                    EditorGUILayout.PropertyField(OnAvatarLoaded);
                    break;
                case 2:
                    EditorGUILayout.PropertyField(OnUserStatsRecieved);
                    EditorGUILayout.PropertyField(OnUserStatsStored);
                    EditorGUILayout.PropertyField(OnAchievementStored);
                    break;
                case 3:
                    EditorGUILayout.PropertyField(disconnected);
                    EditorGUILayout.PropertyField(connected);
                    EditorGUILayout.PropertyField(failure);
                    break;
#if HE_STEAMPLAYERSERVICES || HE_STEAMCOMPLETE
                case 4:
                    EditorGUILayout.PropertyField(leaderboardRankChanged);
                    EditorGUILayout.PropertyField(leaderboardRankLoaded);
                    EditorGUILayout.PropertyField(leaderboardNewHighRank);
                    break;
                case 5:
                    EditorGUILayout.PropertyField(fileReadAsyncComplete);
                    EditorGUILayout.PropertyField(fileWriteAsyncComplete);
                    EditorGUILayout.PropertyField(fileShareResult);
                    break;
#endif
#if HE_STEAMCOMPLETE
                case 6:
                    EditorGUILayout.PropertyField(itemInstancesUpdated);
                    EditorGUILayout.PropertyField(itemsGranted);
                    EditorGUILayout.PropertyField(itemsConsumed);
                    EditorGUILayout.PropertyField(itemsExchanged);
                    EditorGUILayout.PropertyField(itemsDroped);
                    break;
                case 7:
                    EditorGUILayout.PropertyField(OnGameLobbyJoinRequest);
                    EditorGUILayout.PropertyField(OnLobbyMatchList);
                    EditorGUILayout.PropertyField(OnLobbyCreated);
                    EditorGUILayout.PropertyField(OnLobbyEnter);
                    EditorGUILayout.PropertyField(OnLobbyExit);
                    EditorGUILayout.PropertyField(OnGameServerSet);
                    EditorGUILayout.PropertyField(OnLobbyChatUpdate);
                    EditorGUILayout.PropertyField(QuickMatchFailed);
                    EditorGUILayout.PropertyField(SearchStarted);
                    EditorGUILayout.PropertyField(OnChatMessageReceived);
                    break;
#endif
                default:
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif