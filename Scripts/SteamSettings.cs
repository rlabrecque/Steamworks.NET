#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamAPI
{

    /// <summary>
    /// <para>The root of Heathen Engieering's Steamworks system. <see cref="SteamworksClientApiSettings"/> provides access to all core funcitonality including stats, achievements, the friend system and the overlay system.</para>
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SteamworksClientApiSettings"/> object is the root of Heathen Engineering's Steamworks kit.
    /// <see cref="SteamworksClientApiSettings"/> contains the configuration for the fundamental systems of the Steamworks API and provides access to all core funcitonality.
    /// You can easily access the active <see cref="SteamworksClientApiSettings"/> object any time via <see cref="current"/> a static member that is populated on initalization of the Steamworks API with the settings that are being used to configure it.</para>
    /// <para><see cref="SteamworksClientApiSettings"/> is divided into 2 major areas being <see cref="client"/> and <see cref="server"/>.
    /// The <see cref="client"/> member provides easy access to features and systems relivent for your "client" that is the applicaiton the end user is actually playing e.g. your game.
    /// This would include features such as overlay, friends, clans, stats, achievements, etc.
    /// <see cref="server"/> in contrast deals with tthe configuraiton of Steamworks Game Server features and only comes into play for server builds.
    /// Note that the <see cref="server"/> member and its funcitonality are stripped out of client builds, that is it is only accessable in a server build and in the Unity Editor</para>
    /// </remarks>
    [HelpURL("https://heathen-engineering.github.io/steamworks-v2-documentation/class_heathen_engineering_1_1_steam_a_p_i_1_1_steam_settings.html")]
    [CreateAssetMenu(menuName = "Steamworks/Settings")]
    public class SteamSettings : ScriptableObject
    {
        /// <summary>
        /// A reference to the initalized <see cref="SteamworksClientApiSettings"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value gets set when the <see cref="Init"/> method is called which should be done by the <see cref="SteamworksClientApiSystem"/>. Note that your app should have 1 <see cref="SteamworksClientApiSystem"/> defined in a scene that is loaded once and is never reloaded, that is you should not put the <see cref="SteamworksClientApiSystem"/> on a menu scene that will be reloaded multiple times during your games session life as this will break events and other features of the Steamworks API.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void SayMyName()
        ///     {
        ///         Debug.Log("This user's name is " + SystemSettings.Client.user.DisplayName);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static SteamSettings current;

        /// <summary>
        /// The AppId_t value configured and initalized for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the app id value the developer would have typed in to the Unity Editor when setting up the project.
        /// Note that hackers can easily modify this value to cause the Steamworks API to initalize as a different game or can use the steam_appid.txt to force the Steamworks API to register as a different ID.
        /// You can confirm what ID Valve sees this program as running as by calling <see cref="Utilities.GetAppId"/> you can then compare this fixed value to insure your user is not attempting to manipulate your program.
        /// In addition if you are integrating deeply with the Steamworks API such as using stats, achievements, leaderboards and other features with a configuration specific to your app ID ... this will further insure that if a user manages to initalize as an app other than your App ID ... such as an attempt to pirate your game that these features will break insuring a degraded experance for pirates.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void AppIdTests()
        ///     {
        ///         Debug.Log("Configred to run as " + SystemSettings.ApplicationId + ", actually running as " + SystemSettings.GetAppId() );
        ///     }
        /// }
        /// </code>
        /// </example>
        public static AppId_t ApplicationId => current.applicationId;

        /// <summary>
        /// The list of stats registered for this applicaiton.
        /// </summary>
        /// <remarks>
        /// See <see cref="stats"/> for more information. This field simply access the <see cref="stats"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<StatObject> Stats => current.stats;

        /// <summary>
        /// The list of achievements registered for this applicaiton.
        /// </summary>
        /// <remarks>
        /// See <see cref="achievements"/> for more information. This field simply access the <see cref="achievements"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<AchievementObject> Achievements => current.achievements;

        /// <summary>
        /// Indicates an error with API intializaiton
        /// </summary>
        /// <remarks>
        /// If true than an error occured during the initalization of the Steamworks API and normal funcitonality will not be possible.
        /// </remarks>
        public static bool HasInitalizationError { get; private set; }

        /// <summary>
        /// Initalization error message if any
        /// </summary>
        /// <remarks>
        /// See <see cref="HasInitalizationError"/> to determin if an error occured, if so this message will discribe possible causes.
        /// </remarks>
        public static string InitalizationErrorMessage { get; private set; }

#if !CONDITIONAL_COMPILE || (UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// Static access to the active <see cref="GameServer"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that when using CONDITIONAL_COMPILE script define this will be striped out of client builds.
        /// </para>
        /// <para>
        /// The <see cref="GameServer"/> object provides easy access to Steamworks Game Server configuraiton and server only features.
        /// Note that your server can be a Steamworks Game Server and not have to use the Steamworks Networking transports ... e.g. you can use any transport you like and host anywhere you like.
        /// Being a Steamworks Game Server simply means that your server has initalized the Steamworks API and registered its self against Valve's backend ... in addition if this server has an  IP address of a trusted server as defined in your app configuration on the Steamworks Portal,
        /// then it may perform GS only actions such as setting stats and achievments that are marked as GS only.
        /// </para>
        /// </remarks>
        public static GameServer Server => current.server;
#endif

#if !CONDITIONAL_COMPILE || (!UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// Static access to the active <see cref="GameClient"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that when using CONDITIONAL_COMPILE script define this will be striped out of server builds.
        /// </para>
        /// <para>
        /// The <see cref="GameClient"/> object provides easy access to client specifc functions such as the <see cref="Steam.UserData"/> for the local user ... you can access this via.
        /// <code>
        /// SteamSettings.Client.user
        /// </code>
        /// or you can fetch the <see cref="Steam.UserData"/> for any given user via code such as
        /// <code>
        /// SteamSettings.Client.GetUserData(ulong userId)
        /// </code>
        /// For more information please see the documentaiton on the <see cref="GameClient"/> object.
        /// </para>
        /// </remarks>
        public static GameClient Client => current.client;
#endif

        /// <summary>
        /// The current applicaiton ID
        /// </summary>
        /// <remarks>
        /// <para>It is importnat that this is set to your game's AppId.
        /// Note that when working in Unity Editor you need to change this value in the <see cref="SteamworksClientApiSettings"/> object your using but also in the steam_appid.txt file located in the root of your project.
        /// You can read more about the steam_appid.txt file here <a href="https://heathen-engineering.mn.co/posts/steam_appidtxt"/></para>
        /// </remarks>
        [FormerlySerializedAs("ApplicationId")]
        public AppId_t applicationId = new AppId_t(0x0);

        /// <summary>
        /// Indicates rather or not the Steamworks API is initalized
        /// </summary>
        /// <remarks>
        /// <para>This value gets set to true when <see cref="Init"/> is called by the <see cref="SteamworksClientApiSystem"/>.
        /// Note that if Steamworks API fails to initalize such as if the Steamworks client is not installed, running and logged in with a valid Steamworks user then the call to Init will fail and the <see cref="Initialized"/> value will remain false.</para>
        /// </remarks>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Used in various processes to determin the level of detail to log
        /// </summary>
        public bool isDebugging = false;

        /// <summary>
        /// The registered stats assoceated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the approprete stat.
        /// Put more simply if a stat is listed here then the system will update that <see cref="StatObject"/> object with score changes as that information comes in from the Valve backend insuring that these <see cref="StatObject"/> objects are an up to date snap shot of the local user's stat value.
        /// For servers these objects simplify fetching and settting stat values for targeted users but of course dosn't cashe values for a local user since server's have no local user.
        /// </remarks>
        public List<StatObject> stats = new List<StatObject>();

        /// <summary>
        /// The registered achievements assoceated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the approprete achievement.
        /// Put more simply if a stat is listed here then the system will update that <see cref="SteamAchievementData"/> object with state changes as that information comes in from the Valve backend insuring that these <see cref="AchievementObject"/> objects are an up to date snap shot of the local user's achievement value.
        /// For servers these objects simplify fetching and settting stat values for targeted users but of course dosn't cashe values for a local user since server's have no local user.
        /// </remarks>
        public List<AchievementObject> achievements = new List<AchievementObject>();

#if HE_STEAMPLAYERSERVICES
        #region DLC System
        public List<DownloadableContentObject> dlc = new List<DownloadableContentObject>();
        /// <summary>
        /// The list of dlc registered for this applicaiton.
        /// </summary>
        /// <remarks>
        /// See <see cref="dlc"/> for more information. This field simply access the <see cref="dlc"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<DownloadableContentObject> DLC => current.dlc;

#pragma warning disable IDE0052 // Remove unread private members
        private Callback<DlcInstalled_t> m_DlcInstalled;
#pragma warning restore IDE0052 // Remove unread private members

        private void RegisterDLC()
        {
            m_DlcInstalled = Callback<DlcInstalled_t>.Create(HandleDlcInstalled);
        }

        private void HandleDlcInstalled(DlcInstalled_t param)
        {
            var target = dlc.FirstOrDefault(p => p.AppId == param.m_nAppID);
            if (target != null)
            {
                target.UpdateStatus();
            }
        }
        #endregion

        #region Leaderboard System
        /// <summary>
        /// The list of leaderboards registered for this applicaiton.
        /// </summary>
        /// <remarks>
        /// See <see cref="leaderboards"/> for more information. This field simply access the <see cref="leaderboards"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<LeaderboardObject> Leaderboards => current.leaderboards;

        /// <summary>
        /// The list of leaderboards registered for this application.
        /// </summary>
        /// <remarks>
        /// Leaderboards are registered by name and on initialization are resolved to there respective leaderboard IDs.
        /// These IDs can be used in later operations, consaquently this means that adding a leaderboard to this list after initalization will require that you register the board manually by calling its <see cref="LeaderboardObject.Register"/> method.
        /// </remarks>
        public List<LeaderboardObject> leaderboards = new List<LeaderboardObject>();
        #endregion
#endif

#if HE_STEAMCOMPLETE
        public UserGeneratedContentTools ugc = new UserGeneratedContentTools();

        public static UserGeneratedContentTools UGC => current.ugc;
#endif



#if !CONDITIONAL_COMPILE || (UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// configuration settings and features unique to the Server API
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameServer
        {
            [Serializable]
            public class DisconnectedEvent : UnityEvent<SteamServersDisconnected_t> { }

            [Serializable]
            public class ConnectedEvent : UnityEvent<SteamServersConnected_t> { }

            [Serializable]
            public class FailureEvent : UnityEvent<SteamServerConnectFailure_t> { }

            public class UnloadedStatsEvent : UnityEvent<GSStatsUnloaded_t> { }

            [Header("System Configuraiton")]
            public bool autoInitalize = false;
            public bool enableMirror = true;

            [Header("Initalization Settings")]
            [Tooltip("will usually be zero.\nIf you are on a machine with multiple IP addresses, you can pass a non-zero value here and the relevant sockets will be bound to that IP.\nThis can be used to ensure that the IP you desire is the one used in the server browser.")]
            public uint ip = 0;
            [FormerlySerializedAs("masterServerUpdaterPort")]
            [Tooltip("is the port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE for usQueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master  server updater. See references to GameSocketShare in isteamgameserver.hn")]
            public ushort queryPort = 27016;
            [FormerlySerializedAs("serverPort")]
            [Tooltip("the port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port.")]
            public ushort gamePort = 27015;
            [Tooltip("The version string should be in the form x.x.x.x, and is used by the master server to detect when the server is out of date.  (Only servers with the latest version will be listed.)")]
            public string serverVersion = "1.0.0.0";
            [Tooltip("Only used if supporting spectators.")]
            public ushort spectatorPort = 27017;


            [Header("Server Settings")]
            [Tooltip("This will get set on logon and is how users will connect.")]
            public CSteamID serverId;
            [Tooltip("Should the system use the Game Server Authentication API.")]
            public bool usingGameServerAuthApi = false;
            [Tooltip("Heartbeats notify the master server of this servers details, if disabled your server will not list\nIf usingGameServerAuthApi is enabled heartbeats are always enabled..")]
            public bool enableHeartbeats = true;
            [Tooltip("If true the spectator port and server name will be used and configured on the server.")]
            public bool supportSpectators = false;
            [Tooltip("Only used if supporting spectators.")]
            public string spectatorServerName = "Usually GameDescription + Spectator";
            public bool anonymousServerLogin = false;
            [Tooltip("See https://steamcommunity.com/dev/managegameservers \nOr\nUse Anonymous Server Login")]
            public string gameServerToken = "See https://steamcommunity.com/dev/managegameservers";
            public bool isPasswordProtected = false;
            public string serverName = "My Server Name";
            [Tooltip("It is recomended to set this to the full name of your game.")]
            public string gameDescription = "Usually the name of your game";
            [Tooltip("Typically the same as the game's name e.g. its folder name.")]
            public string gameDirectory = "e.g. its folder name";
            public bool isDedicated = false;
            public int maxPlayerCount = 4;
            public int botPlayerCount = 0;
            public string mapName = "";
            [Tooltip("A delimited string used for Matchmaking Filtering e.g. CoolPeopleOnly,NoWagonsAllowed.\nThe above represents 2 data points matchmaking will then filter accordingly\n... see Heathen Game Server Browser for more informaiton.")]
            public string gameData;
            public List<StringKeyValuePair> rulePairs = new List<StringKeyValuePair>();

            [Header("Events")]
            public UnityEvent gameServerShuttingDown;
            public DisconnectedEvent disconnected;
            public ConnectedEvent connected;
            public FailureEvent failure;
            public UnloadedStatsEvent userStatsUnloaded;

            public Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
            public Callback<SteamServersConnected_t> steamServersConnected;
            public Callback<SteamServersDisconnected_t> steamServersDisconnected;
            public CallResult<GSStatsReceived_t> steamGameServerStatsReceived;
            public CallResult<GSStatsStored_t> steamGameServerStatsStored;
            public CallResult<GSStatsUnloaded_t> steamGameServerStatsUnloaded;



            private Dictionary<CSteamID, Action<GSStatsReceived_t>> requestStatsCallbacks = new Dictionary<CSteamID, Action<GSStatsReceived_t>>();
            private Dictionary<CSteamID, Action<GSStatsStored_t>> storeStatsCallbacks = new Dictionary<CSteamID, Action<GSStatsStored_t>>();

            private void OnSteamServersDisconnected(SteamServersDisconnected_t param)
            {
                disconnected.Invoke(param);
            }

            private void OnSteamServersConnected(SteamServersConnected_t param)
            {
                connected.Invoke(param);
            }

            private void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
            {
                failure.Invoke(param);
            }

            #region Achievements and Stas
            public void StoreUserStats(CSteamID user, Action<GSStatsStored_t> callback)
            {
                if (storeStatsCallbacks == null)
                    storeStatsCallbacks = new Dictionary<CSteamID, Action<GSStatsStored_t>>();

                if (storeStatsCallbacks.ContainsKey(user))
                    storeStatsCallbacks[user] = callback;
                else
                    storeStatsCallbacks.Add(user, callback);

                SteamGameServerStats.StoreUserStats(user);
            }

            /// <summary>
            /// Request stats for the <paramref name="user"/> and invoke the <paramref name="callback"/> when complete
            /// </summary>
            /// <remarks>
            /// <para>
            /// Note that each user can have 1 callback, if you call this on the same user before the previous operation is compelte only the last operation called will invoke its callback.
            /// </para>
            /// </remarks>
            /// <param name="user">The user to get stats for</param>
            /// <param name="callback">The action to be invoked when the process is complete</param>
            public void RequestUserStats(CSteamID user, Action<GSStatsReceived_t> callback)
            {
                if (requestStatsCallbacks == null)
                    requestStatsCallbacks = new Dictionary<CSteamID, Action<GSStatsReceived_t>>();

                if (requestStatsCallbacks.ContainsKey(user))
                    requestStatsCallbacks[user] = callback;
                else
                    requestStatsCallbacks.Add(user, callback);

                SteamGameServerStats.RequestUserStats(user);
            }

            private void OnUserStatsReceived(GSStatsReceived_t param, bool bIOFailure)
            {
                if (requestStatsCallbacks.ContainsKey(param.m_steamIDUser))
                {
                    requestStatsCallbacks[param.m_steamIDUser].Invoke(param);
                }
            }

            private void OnUserStatsStored(GSStatsStored_t param, bool bIOFailure)
            {
                if (storeStatsCallbacks.ContainsKey(param.m_steamIDUser))
                {
                    storeStatsCallbacks[param.m_steamIDUser].Invoke(param);
                }
            }

            private void OnUserStatsUnloaded(GSStatsUnloaded_t param, bool bIOFailure)
            {
                if (userStatsUnloaded != null)
                    userStatsUnloaded.Invoke(param);
            }

            public void RegisterCallbacks()
            {
                steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServerConnectFailure);
                steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
                steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
                steamGameServerStatsReceived = CallResult<GSStatsReceived_t>.Create(OnUserStatsReceived);
                steamGameServerStatsStored = CallResult<GSStatsStored_t>.Create(OnUserStatsStored);
                steamGameServerStatsUnloaded = CallResult<GSStatsUnloaded_t>.Create(OnUserStatsUnloaded);
            }
            #endregion
        }

        /// <summary>
        /// Contains server side funcitonality and is not available in client builds
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameServer server = new GameServer();
#endif

#if !CONDITIONAL_COMPILE || (!UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// configuration settings and features unique to the Client API
        /// </summary>
        /// <remarks>
        /// Note that if you have defined CONDITIONAL_COMPILE in your script defines then this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !CONDITIONAL_COMPILE || (!UNITY_SERVER || UNITY_EDITOR)
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameClient
        {
            /// <summary>
            /// <para>A wrapper around common SteamAPI Overlay funcitonlity. This class is used to provide access to Overlay funcitons and features.</para>
            /// </summary>
            /// <remarks>
            /// <para>
            /// The <see cref="Overlay"/> object handles the configuration of the overlay notificaiton panel and funcitons to activate the overlay on demand.
            /// </para>
            /// </remarks>
            [Serializable]
            public class Overlay
            {
                /// <summary>
                /// The offset of the Steamworks notification panel relative to its <see cref="notificationPosition"/>
                /// </summary>
                public Vector2Int notificationInset;
                /// <summary>
                /// The position the notification pannel of the Steamworks overlay system will anchor
                /// </summary>
                public ENotificationPosition notificationPosition = ENotificationPosition.k_EPositionBottomRight;

                /// <summary>
                /// <para>A wrap around <see cref="global::Steamworks.SteamUtils.IsOverlayEnabled()"/></para>   
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsOverlayEnabled">https://partner.steamgames.com/doc/api/ISteamUtils#IsOverlayEnabled</a> for more information.
                /// </summary>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Checks if the Steamworks Overlay is running & the user can access it. The overlay process could take a few seconds to start & hook the game process, so this function will initially return false while the overlay is loading.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings.</para></description>
                /// <code>
                /// if(settings.Overlay.IsEnable)
                ///      Debug.Log("The overlay is enabled and ready for use!");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public bool IsEnabled
                {
                    get
                    {
                        return global::Steamworks.SteamUtils.IsOverlayEnabled();
                    }
                }
                private static bool _OverlayOpen = false;

                /// <summary>
                /// <para>Indicates that the Steamworks Overlay is currently open.</para>   
                /// See <a href="https://partner.steamgames.com/doc/features/overlay">https://partner.steamgames.com/doc/features/overlay</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Indicates that the overlay is currently open.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings.</para></description>
                /// <code>
                /// if(settings.Overlay.IsOpen)
                ///      Debug.Log("The overlay is currently open.");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public bool IsOpen
                {
                    get
                    {
                        return _OverlayOpen;
                    }
                }

                /// <summary>
                /// For internal use only
                /// </summary>
                /// <param name="data"></param>
                public void HandleOnOverlayOpen(GameOverlayActivated_t data)
                {
                    _OverlayOpen = data.m_bActive == 1;
                }

                /// <summary>
                /// <para>A wrap around <see cref="global::Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(CSteamId lobbyId)"/>.</para>   
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayInviteDialog">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayInviteDialog</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the overlay with the invite dialog populated for the indicated lobby.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.Invite(myLobby);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void Invite(CSteamID lobbyId)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
                }

                /// <summary>
                /// <para>Opens the overlay to the current games store page.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenStore()
                {
                    OpenStore(SteamUtils.GetAppID(), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
                }

                /// <summary>
                /// <para>Opens the overlay to the store page of the provide app Id.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore(settings.ApplicationId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="appId">The application id of the game you wish to open the store to</param>
                public void OpenStore(uint appId)
                {
                    OpenStore(new AppId_t(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
                }

                /// <summary>
                /// <para>Opens the overlay to the store page of the provide app Id with the provided overlay store flag.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore(settings.ApplicationId.m_AppId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="appId">The application id of the game you wish to open the store to</param>
                /// <param name="flag">Modifies the behaviour of the store page when opened.</param>
                public void OpenStore(uint appId, EOverlayToStoreFlag flag)
                {
                    OpenStore(new AppId_t(appId), flag);
                }

                /// <summary>
                /// <para>Opens the overlay to the store page of the provide app Id with the provided overlay store flag.</para>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to the current apps store page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore(settings.ApplicationId.m_AppId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="appId">The application id of the game you wish to open the store to, See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#EOverlayToStoreFlag">https://partner.steamgames.com/doc/api/ISteamFriends#EOverlayToStoreFlag</a> for more details</param>
                public void OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToStore(appId, flag);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to the indicated dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStore("friends");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
                public void Open(string dialog)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay(dialog);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToWebPage">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToWebPage</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to the indicated web page.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenWebPage("http://www.google.com");
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
                public void OpenWebPage(string URL)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToWebPage(URL);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to friends dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriends();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenFriends()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("friends");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to community dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenCommunity();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenCommunity()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("community");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to players dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenPlayers();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenPlayers()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("players");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to settings dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenSettings();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenSettings()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("settings");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to offical game group dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenOfficialGameGroup();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenOfficialGameGroup()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("officialgamegroup");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to stats dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStats();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenStats()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("stats");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to achievements dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenArchievements();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                public void OpenAchievements()
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlay("achievements");
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to chat dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenChat(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user to open the chat dialog with</param>
                public void OpenChat(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("Chat", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to profile dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenProfile(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes profile you want to open</param>
                public void OpenProfile(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("steamid", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to a trade dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenTrade(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user who you want to trade with</param>
                public void OpenTrade(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("jointrade", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to stats dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenStats(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes stats you want to display</param>
                public void OpenStats(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("stats", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to achievements dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenAchievements(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The id of the user whoes achievements you want to display</param>
                public void OpenAchievements(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("achievements", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to friends add dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriendAdd(myFriendId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The Id of the user you want to add as a friend</param>
                public void OpenFriendAdd(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("friendadd", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to friend remove dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriendRemove(userId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user you want to remove from friends</param>
                public void OpenFriendRemove(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("friendremove", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to request accept dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenRequestAccept(userId);
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes request you want to accept</param>
                public void OpenRequestAccept(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestaccept", user);
                }

                /// <summary>
                /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
                /// </summary>
                /// <remarks>
                /// <para>Note that Steamworks Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
                /// </remarks>
                /// <example>
                /// <list type="bullet">
                /// <item>
                /// <description><para>Activates the Steamworks Overlay to request ignore dialog.</para>
                /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.Steam.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
                /// <code>
                /// settings.Overlay.OpenFriends();
                /// </code>
                /// </item>
                /// </list>
                /// </example>
                /// <param name="user">The user whoes request you want to ignore</param>
                public void OpenRequestIgnore(CSteamID user)
                {
                    global::Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestignore", user);
                }
            }

            /// <summary>
            /// cashe of the local users data
            /// </summary>
            /// <remarks>
            /// <para>
            /// This can be used to fetch the local users display name, persona state information, rich precense information, icon/avatar and to operate against the overlay for dialogs specific to this user such as the Invite dialog or Profile dialog.
            /// </para>
            /// </remarks>
            public UserData user;

            /// <summary>
            /// access the overlay configuration and functions.
            /// </summary>
            /// <remarks>
            /// Please see the documentation for <see cref="Overlay"/> for more details
            /// </remarks>
            public Overlay overlay = new Overlay();

            /// <summary>
            /// Count of players currently playing this game. This can be refreshed on demand by calling <see cref="RefreshPlayerCount"/>
            /// </summary>
            public int lastKnownPlayerCount;

            private readonly Dictionary<ulong, UserData> knownUsers = new Dictionary<ulong, UserData>();

#pragma warning disable IDE0052 // Remove unread private members
            private CGameID m_GameID;
            private Callback<AvatarImageLoaded_t> avatarLoadedCallback;
            private Callback<PersonaStateChange_t> personaStateChange;
            private Callback<UserStatsReceived_t> m_UserStatsReceived;
            private Callback<UserStatsStored_t> m_UserStatsStored;
            /// <summary>
            /// For internal user
            /// </summary>
            public Callback<GameOverlayActivated_t> m_GameOverlayActivated;
            private Callback<UserAchievementStored_t> m_UserAchievementStored;
            private Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFrinedChatMsg;
            private CallResult<NumberOfCurrentPlayers_t> m_OnNumberOfCurrentPlayersCallResult;
            private CallResult<FriendsGetFollowerCount_t> m_FriendsGetFollowerCount;
            private Dictionary<CSteamID, Action<UserData, int>> FollowCallbacks = new Dictionary<CSteamID, Action<UserData, int>>();
#pragma warning restore IDE0052 // Remove unread private members

            #region Utility
            /// <summary>
            /// <para>Checks if the Overlay needs a present. Only required if using event driven render updates.</para>
            /// <para>Typically this call is unneeded if your game has a constantly running frame loop that calls the D3D Present API, or OGL SwapBuffers API every frame as is the case in most games. However, if you have a game that only refreshes the screen on an event driven basis then that can break the overlay, as it uses your Present/SwapBuffers calls to drive it's internal frame loop and it may also need to Present() to the screen any time a notification happens or when the overlay is brought up over the game by a user. You can use this API to ask the overlay if it currently need a present in that case, and then you can check for this periodically (roughly 33hz is desirable) and make sure you refresh the screen with Present or SwapBuffers to allow the overlay to do it's work.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#BOverlayNeedsPresent">https://partner.steamgames.com/doc/api</a>
            /// </summary>
            /// <returns></returns>
            public bool OverlayNeedsPresent()
            {
                return SteamUtils.BOverlayNeedsPresent();
            }

            /// <summary>
            /// <para>Gets the current amount of battery power on the computer.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetCurrentBatteryPower">https://partner.steamgames.com/doc/api</a>
            /// </summary>
            /// <returns></returns>
            public float GetCurrentBatteryPower()
            {
                var steamPower = SteamUtils.GetCurrentBatteryPower();
                return Mathf.Clamp01(steamPower / 100f);
            }

            /// <summary>
            /// <para>Returns the language the steam client is running in.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetSteamUILanguage">https://partner.steamgames.com/doc/api</a>
            /// </summary>
            /// <returns></returns>
            public string GetSteamClientLanguage()
            {
                return SteamUtils.GetSteamUILanguage();
            }

            /// <summary>
            /// <para>Checks if Steamworks & the Steamworks Overlay are running in Big Picture mode.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsSteamInBigPictureMode">https://partner.steamgames.com/doc/api</a>
            /// </summary>
            public bool IsSteamInBigPictureMode
            {
                get { return SteamUtils.IsSteamInBigPictureMode(); }
            }

            /// <summary>
            /// <para>Checks if Steamworks is running in VR mode.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsSteamRunningInVR">https://partner.steamgames.com/doc/api</a>
            /// </summary>
            public bool IsSteamRunningInVR
            {
                get { return SteamUtils.IsSteamRunningInVR(); }
            }

            /// <summary>
            /// Flips an image buffer
            /// This is used when loading images from Steamworks as they tend to be inverted for what Unity wants
            /// </summary>
            /// <param name="width"></param>
            /// <param name="height"></param>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public byte[] FlipImageBufferVertical(int width, int height, byte[] buffer)
            {
                byte[] result = new byte[buffer.Length];

                int xWidth = width * 4;
                int yHeight = height;

                for (int y = 0; y < yHeight; y++)
                {
                    for (int x = 0; x < xWidth; x++)
                    {
                        result[x + ((yHeight - 1 - y) * xWidth)] = buffer[x + (xWidth * y)];
                    }
                }

                return result;
            }
            #endregion

            #region Events
            /// <summary>
            /// Occures on load of a Steamworks avatar
            /// </summary>
            [HideInInspector]
            public UnityAvatarImageLoadedEvent evtAvatarLoaded;
            /// <summary>
            /// Ocucres on change of Steamworks User Data persona information
            /// </summary>
            [HideInInspector]
            public UnityPersonaStateChangeEvent evtPersonaStateChanged;
            /// <summary>
            /// Occures when user stats and achievements are recieved from Valve
            /// </summary>
            [HideInInspector]
            public UnityUserStatsReceivedEvent evtUserStatsReceived;
            /// <summary>
            /// Occures when user stats are stored to Valve
            /// </summary>
            [HideInInspector]
            public UnityUserStatsStoredEvent evtUserStatsStored;
            /// <summary>
            /// Occures when the Steamworks overlay is activated / shown
            /// </summary>
            [HideInInspector]
            public BoolUnityEvent evtOverlayActivated;
            /// <summary>
            /// Occures when Achivements are stored to Valve
            /// </summary>
            [HideInInspector]
            public UnityUserAchievementStoredEvent evtAchievementStored;

            /// <summary>
            /// Occures when a chat message from a friend is recieved.
            /// </summary>
            [HideInInspector]
            public FriendChatMessageEvent evtRecievedFriendChatMessage;

            /// <summary>
            /// Occures as the result of a RefreshPlayerCount call
            /// </summary>
            [HideInInspector]
            public UnityNumberOfCurrentPlayersResultEvent evtNumberOfCurrentPlayersResult;
            #endregion

            #region Achievement System
            /// <summary>
            /// <para>Stores the stats and achievements to Valve</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats">https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats</a>
            /// </summary>
            /// <remarks>
            /// This must be called in order to store updated stats to the backend. Note that this will get called when the game closes.
            /// </remarks>
            public void StoreStatsAndAchievements()
            {
                SteamUserStats.StoreStats();
            }

            /// <summary>
            /// Registeres the achievement callbacks
            /// </summary>
            public void RegisterAchievementsSystem()
            {
                // Cache the GameID for use in the Callbacks
                m_GameID = new CGameID(SteamUtils.GetAppID());

                m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(HandleUserStatsReceived);
                m_UserStatsStored = Callback<UserStatsStored_t>.Create(HandleUserStatsStored);
                m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(HandleAchievementStored);
                m_FriendsGetFollowerCount = CallResult<FriendsGetFollowerCount_t>.Create(HandleGetFollowerCount);
                m_OnNumberOfCurrentPlayersCallResult = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
            }

            /// <summary>
            /// <para>Requests the current users stats from Valve servers</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats">https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats</a>
            /// </summary>
            /// <returns>Returns true if the server accepted the request.</returns>
            public bool RequestCurrentStats()
            {
                var handle = SteamUserStats.GetNumberOfCurrentPlayers();
                m_OnNumberOfCurrentPlayersCallResult.Set(handle);
                return SteamUserStats.RequestCurrentStats();
            }

            /// <summary>
            /// <para>
            /// Requests the count of current players from Steamworks for this application
            /// On return this will update the SteamSettings.LastKnownPlayerCount value
            /// and trigger the OnNumberOfCurrentPlayersResult event for the SteamSettings 
            /// object and the connected Foundation Manager
            /// </para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#GetNumberOfCurrentPlayers">https://partner.steamgames.com/doc/api/ISteamUserStats#GetNumberOfCurrentPlayers</a>
            /// </summary>
            public void RefreshPlayerCount()
            {
                var handle = SteamUserStats.GetNumberOfCurrentPlayers();
                m_OnNumberOfCurrentPlayersCallResult.Set(handle);
            }

            private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
            {
                if (!bIOFailure)
                {
                    if (pCallback.m_bSuccess == 1)
                        lastKnownPlayerCount = pCallback.m_cPlayers;

                    if (evtNumberOfCurrentPlayersResult != null)
                        evtNumberOfCurrentPlayersResult.Invoke(pCallback);
                }
            }

            private void HandleUserStatsReceived(UserStatsReceived_t pCallback)
            {
                // we may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (EResult.k_EResultOK == pCallback.m_eResult)
                    {
                        // load achievements
                        foreach (AchievementObject ach in Achievements)
                        {
                            bool ret = SteamUserStats.GetAchievement(ach.achievementId.ToString(), out ach.isAchieved);
                            if (ret)
                            {
                                ach.displayName = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "name");
                                ach.displayDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "desc");
                                ach.hidden = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "hidden") == "1";
                            }
                            else
                            {
                                Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.achievementId + "\nIs it registered in the Steamworks Partner site?");
                            }
                        }

                        foreach (var stat in Stats)
                        {
                            if (stat.DataType == StatObject.StatDataType.Float)
                            {
                                float rValue;
                                if (SteamUserStats.GetStat(stat.statName, out rValue))
                                    stat.InternalUpdateValue(rValue);
                                else
                                    Debug.LogWarning("SteamUserStats.GetAchievement failed for Stat " + stat.statName + "\nIs it registered in the Steamworks Partner site and the correct data type?");
                            }
                            else
                            {
                                int rValue;
                                if (SteamUserStats.GetStat(stat.statName, out rValue))
                                    stat.InternalUpdateValue(rValue);
                                else
                                    Debug.LogWarning("SteamUserStats.GetStat failed for Stat " + stat.statName + "\nIs it registered in the Steamworks Partner site and the correct data type?");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
                    }

                    evtUserStatsReceived.Invoke(pCallback);
                }
            }

            private void HandleUserStatsStored(UserStatsStored_t pCallback)
            {
                // we may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (EResult.k_EResultOK == pCallback.m_eResult)
                    {
                        evtUserStatsStored.Invoke(pCallback);
                    }
                    else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                    {
                        // One or more stats we set broke a constraint. They've been reverted,
                        // and we should re-iterate the values now to keep in sync.
                        Debug.Log("StoreStats - some failed to validate, re-syncing data now in an attempt to correct.");
                        // Fake up a callback here so that we re-load the values.
                        UserStatsReceived_t callback = new UserStatsReceived_t();
                        callback.m_eResult = EResult.k_EResultOK;
                        callback.m_nGameID = (ulong)m_GameID;
                        HandleUserStatsReceived(callback);
                    }
                    else
                    {
                        Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
                    }
                }
            }

            private void HandleAchievementStored(UserAchievementStored_t pCallback)
            {
                // We may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (0 == pCallback.m_nMaxProgress)
                    {
                        Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                    }
                    else
                    {
                        Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                    }

                    evtAchievementStored.Invoke(pCallback);
                }
            }

            private void HandleGetFollowerCount(FriendsGetFollowerCount_t param, bool bIOFailure)
            {
                if (FollowCallbacks.ContainsKey(param.m_steamID))
                {
                    var callback = FollowCallbacks[param.m_steamID];
                    if (param.m_eResult != EResult.k_EResultOK || bIOFailure)
                    {
                        if (callback != null)
                            callback.Invoke(GetUserData(param.m_steamID), -1);
                        FollowCallbacks.Remove(param.m_steamID);
                    }
                    else
                    {
                        if (callback != null)
                            callback.Invoke(GetUserData(param.m_steamID), param.m_nCount);
                        FollowCallbacks.Remove(param.m_steamID);
                    }
                }
            }

            /// <summary>
            /// <para>Unlocks the achievement.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
            /// </summary>
            public void UnlockAchievement(uint achievementIndex)
            {
                var target = Achievements[System.Convert.ToInt32(achievementIndex)];
                if (target != default(AchievementObject) && !target.isAchieved)
                    UnlockAchievementData(target);
            }

            /// <summary>
            /// <para>Unlocks the achievement.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
            /// </summary>
            public void UnlockAchievementData(AchievementObject data)
            {
                data.Unlock();
            }

            /// <summary>
            /// <para>Resets the unlock status of an achievmeent.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
            /// </summary>
            /// <param name="achievementIndex">The index of the registered achievment you wish to reset.</param>
            public void ClearAchievement(uint achievementIndex)
            {
                var target = Achievements[System.Convert.ToInt32(achievementIndex)];
                if (target != default(AchievementObject) && !target.isAchieved)
                    ClearAchievement(target);
            }

            /// <summary>
            /// <para>Resets the unlock status of an achievmeent.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
            /// </summary>
            /// <param name="data">The achievement you wish to reset.</param>
            public void ClearAchievement(AchievementObject data)
            {
                data.ClearAchievement();
            }
            #endregion

            #region Overlay System
            /// <summary>
            /// Called by the Heathen Steamworks Manager when the GameOverlayActivated callback is triggered
            /// </summary>
            /// <param name="data"></param>
            public void HandleOnOverlayOpen(GameOverlayActivated_t data)
            {
                overlay.HandleOnOverlayOpen(data);
                evtOverlayActivated.Invoke(overlay.IsOpen);
            }

            /// <summary>
            /// <para>Sets the overlay notification positon.</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#SetOverlayNotificationPosition">https://partner.steamgames.com/doc/api/ISteamUtils#SetOverlayNotificationPosition</a>
            /// </summary>
            /// <param name="position">The ENotificationPosition to set, see <a href="https://partner.steamgames.com/doc/api/steam_api#ENotificationPosition">https://partner.steamgames.com/doc/api/steam_api#ENotificationPosition</a> for details</param>
            public void SetNotificationPosition(ENotificationPosition position)
            {
                global::Steamworks.SteamUtils.SetOverlayNotificationPosition(overlay.notificationPosition);
                overlay.notificationPosition = position;
            }

            /// <summary>
            /// Updates the notification inset
            /// </summary>
            /// <param name="X"></param>
            /// <param name="Y"></param>
            public void SetNotificationInset(int X, int Y)
            {
                global::Steamworks.SteamUtils.SetOverlayNotificationInset(X, Y);
                overlay.notificationInset = new Vector2Int(X, Y);
            }

            /// <summary>
            /// Updates the notification inset
            /// </summary>
            /// <param name="inset"></param>
            public void SetNotificationInset(Vector2Int inset)
            {
                global::Steamworks.SteamUtils.SetOverlayNotificationInset(inset.x, inset.y);
                overlay.notificationInset = inset;
            }
            #endregion

            #region Friend System
            /// <summary>
            /// Gets a list of <see cref="Steam.UserData"/> representing the 'friends' that match the indicated friend flag.
            /// </summary>
            /// <remarks>
            /// For more details on what each flag option means please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#EFriendFlags"/> in Valve's documentation.
            /// </remarks>
            /// <param name="friendFlags">The category of friend list to return ... defaults to <see cref="EFriendFlags.k_EFriendFlagImmediate"/> aka the current user's "Regular" friends.</param>
            /// <returns>A list of the <see cref="Steam.UserData"/> objects for each friend in this category.</returns>
            public List<UserData> ListFriends(EFriendFlags friendFlags = EFriendFlags.k_EFriendFlagImmediate)
            {
                List<UserData> friendList = new List<UserData>();

                var friendCount = SteamFriends.GetFriendCount(friendFlags);
                for (int i = 0; i < friendCount; i++)
                {
                    friendList.Add(GetUserData(SteamFriends.GetFriendByIndex(i, friendFlags)));
                }

                return friendList;
            }

            

            /// <summary>
            /// Gets the number of users following the specified user.
            /// </summary>
            /// <remarks>
            /// For more details please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#GetFollowerCount"/> in Valve's documentation.
            /// </remarks>
            /// <param name="followingThisUser">The user ID of the Steamworks user you want to count the followers of</param>
            /// <param name="callback">The call back to invoke when the count completes. This will take the form of HandleCallback(SteamUserData user, int followers)</param>
            public void GetFollowerCount(CSteamID followingThisUser, Action<UserData, int> callback)
            {
                FollowCallbacks.Add(followingThisUser, callback);
                SteamFriends.GetFollowerCount(followingThisUser);
            }

            /// <summary>
            /// Sets a Rich Presence key/value for the current user that is automatically shared to all friends playing the same game. Each user can have up to 20 keys.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            /// <remarks>
            /// For details on Rich Presence or the usage of this method please read <see href="https://partner.steamgames.com/doc/api/ISteamFriends#SetRichPresence"/>
            /// </remarks>
            public bool SetRichPresence(string key, string value)
            {
                return SteamFriends.SetRichPresence(key, value);
            }

            /// <summary>
            /// Clears all of the current user's Rich Presence key/values.
            /// </summary>
            public void ClearRichPresence()
            {
                SteamFriends.ClearRichPresence();
            }

            /// <summary>
            /// For internal use, this regisers the Friend system and is called by the <see cref="HeathenEngineering.Steam.Foundation.SteamworksFoundationManager"/> as required.
            /// </summary>
            /// <param name="data"></param>
            public void RegisterFriendsSystem()
            {
                avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(HandleAvatarLoaded);
                personaStateChange = Callback<PersonaStateChange_t>.Create(HandlePersonaStatReceived);
                m_GameConnectedFrinedChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(HandleGameConnectedFriendMsg);

                if (evtRecievedFriendChatMessage == null)
                    evtRecievedFriendChatMessage = new FriendChatMessageEvent();

                if (evtAvatarLoaded == null)
                    evtAvatarLoaded = new UnityAvatarImageLoadedEvent();

                if (evtPersonaStateChanged == null)
                    evtPersonaStateChanged = new UnityPersonaStateChangeEvent();

                user.id = SteamUser.GetSteamID();

                knownUsers.Clear();
                knownUsers.Add(user.id.m_SteamID, user);

                int imageId = SteamFriends.GetLargeFriendAvatar(user.id);
                //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                if (imageId > 0)
                {
                    ApplyAvatarImage(user, imageId);
                }
            }

            private void HandleAvatarLoaded(AvatarImageLoaded_t data)
            {
                if (knownUsers.ContainsKey(data.m_steamID.m_SteamID))
                {
                    UserData u = knownUsers[data.m_steamID.m_SteamID];
                    ApplyAvatarImage(u, data.m_iImage);
                    if (u.evtAvatarLoaded == null)
                        u.evtAvatarLoaded = new UnityEngine.Events.UnityEvent();
                    u.evtAvatarLoaded.Invoke();
                }
                else
                {
                    var n = ScriptableObject.CreateInstance<UserData>();
                    n.id = data.m_steamID;
                    knownUsers.Add(n.id.m_SteamID, n);
                    ApplyAvatarImage(n, data.m_iImage);
                    n.evtAvatarLoaded.Invoke();
                }

                evtAvatarLoaded.Invoke(data);
            }

            private void HandleGameConnectedFriendMsg(GameConnectedFriendChatMsg_t callback)
            {
                string message;
                EChatEntryType chatType;
                SteamFriends.GetFriendMessage(callback.m_steamIDUser, callback.m_iMessageID, out message, 2048, out chatType);
                evtRecievedFriendChatMessage.Invoke(GetUserData(callback.m_steamIDUser), message, chatType);
            }

            private void HandlePersonaStatReceived(PersonaStateChange_t pCallback)
            {
                UserData target = null;
                if (knownUsers.ContainsKey(pCallback.m_ulSteamID))
                {
                    target = knownUsers[pCallback.m_ulSteamID];
                }
                else
                {
                    target = ScriptableObject.CreateInstance<UserData>();
                    target.id = new CSteamID(pCallback.m_ulSteamID);
                    knownUsers.Add(target.id.m_SteamID, target);
                }

                switch (pCallback.m_nChangeFlags)
                {
                    case EPersonaChange.k_EPersonaChangeAvatar:
                        try
                        {
                            int imageId = SteamFriends.GetLargeFriendAvatar(target.id);
                            if (imageId > 0)
                            {
                                target.iconLoaded = true;
                                uint imageWidth, imageHeight;
                                SteamUtils.GetImageSize(imageId, out imageWidth, out imageHeight);
                                byte[] imageBuffer = new byte[4 * imageWidth * imageHeight];
                                if (SteamUtils.GetImageRGBA(imageId, imageBuffer, imageBuffer.Length))
                                {
                                    target.avatar.LoadRawTextureData(FlipImageBufferVertical((int)imageWidth, (int)imageHeight, imageBuffer));
                                    target.avatar.Apply();
                                    target.evtAvatarChanged.Invoke();
                                }
                            }
                        }
                        catch { }
                        break;
                    case EPersonaChange.k_EPersonaChangeComeOnline:
                        if (target.evtComeOnline != null)
                            target.evtComeOnline.Invoke();
                        if (target.evtStateChange != null)
                            target.evtStateChange.Invoke();
                        break;
                    case EPersonaChange.k_EPersonaChangeGamePlayed:
                        if (target.evtGameChanged != null)
                            target.evtGameChanged.Invoke();
                        if (target.evtStateChange != null)
                            target.evtStateChange.Invoke();
                        break;
                    case EPersonaChange.k_EPersonaChangeGoneOffline:
                        if (target.evtGoneOffline != null)
                            target.evtGoneOffline.Invoke();
                        if (target.evtStateChange != null)
                            target.evtStateChange.Invoke();
                        break;
                    case EPersonaChange.k_EPersonaChangeName:
                        if (target.evtNameChanged != null)
                            target.evtNameChanged.Invoke();
                        break;
                }

                evtPersonaStateChanged.Invoke(pCallback);
            }

            private void ApplyAvatarImage(UserData user, int imageId)
            {
                uint width, height;
                SteamUtils.GetImageSize(imageId, out width, out height);
                user.avatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                int bufferSize = (int)(width * height * 4);
                byte[] imageBuffer = new byte[bufferSize];
                SteamUtils.GetImageRGBA(imageId, imageBuffer, bufferSize);
                user.avatar.LoadRawTextureData(FlipImageBufferVertical((int)width, (int)height, imageBuffer));
                user.avatar.Apply();
                user.iconLoaded = true;
            }

            /// <summary>
            /// <para>Set rather or not the system should listen for Steamworks Friend chat messages</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#SetListenForFriendsMessages">https://partner.steamgames.com/doc/api/ISteamFriends#SetListenForFriendsMessages</a>
            /// </summary>
            /// <param name="isOn">True if you want to turn this feature on, otherwise false</param>
            /// <returns>True if successfully enabled, otherwise false</returns>
            public bool ListenForFriendMessages(bool isOn)
            {
                return SteamFriends.SetListenForFriendsMessages(isOn);
            }

            /// <summary>
            /// Send a Steamworks Friend Chat message to the indicated user
            /// </summary>
            /// <param name="friend"></param>
            /// <param name="message"></param>
            /// <returns></returns>
            public bool SendFriendChatMessage(UserData friend, string message)
            {
                return friend.SendMessage(message);
            }

            /// <summary>
            /// <para>Send a Steamworks Friend Chat message to the indicated user</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage">https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage</a>
            /// </summary>
            /// <param name="friend">The friend you wish to send the message to</param>
            /// <param name="message">The message to be sent</param>
            /// <returns></returns>
            public bool SendFriendChatMessage(ulong friendId, string message)
            {
                return SendFriendChatMessage(new CSteamID(friendId), message);
            }

            /// <summary>
            /// <para>Send a Steamworks Friend Chat message to the indicated user</para>
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage">https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage</a>
            /// </summary>
            /// <param name="friend">The friend you wish to send the message to</param>
            /// <param name="message">The message to be sent</param>
            /// <returns></returns>
            public bool SendFriendChatMessage(CSteamID friend, string message)
            {
                return SteamFriends.ReplyToFriendMessage(friend, message);
            }

            /// <summary>
            /// <para>Requests the users avatar from Valve
            /// This is handled by the Friends subsystem but can be called manually to force a refresh</para>  
            /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#GetLargeFriendAvatar">https://partner.steamgames.com/doc/api/ISteamFriends#GetLargeFriendAvatar</a>
            /// </summary>
            /// <param name="userData">The user whoes avatar should be updated</param>
            public void RefreshAvatar(UserData userData)
            {
                int imageId = SteamFriends.GetLargeFriendAvatar(userData.id);
                //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                if (imageId > 0)
                {
                    ApplyAvatarImage(userData, imageId);
                }
            }

            /// <summary>
            /// <para>Locates the Steamworks User Data for the user provided 
            /// This will read from the friends subsystem if availabel or will create a new entery if none is found</para>
            /// </summary>
            /// <param name="steamID">THe user to find or load as required.</param>
            /// <returns>The <see cref="HeathenEngineering.Steam.Foundation.SteamUserData"/> for the indicated user.</returns>
            /// <example>
            /// <list type="bullet">
            /// <item>
            /// <description>Get the <see cref="HeathenEngineering.Steam.Foundation.SteamUserData"/> of a Steamworks user whose ID is stored in myFriendId</description>
            /// <code>
            /// var userData = settings.GetUserData(myFriendId);
            /// Debug.Log("Located the user data for " + userData.DisplayName);
            /// </code>
            /// </item>
            /// </list>
            /// </example>
            public UserData GetUserData(CSteamID steamID)
            {
                if (knownUsers.ContainsKey(steamID.m_SteamID))
                {
                    var n = knownUsers[steamID.m_SteamID];

                    int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                    //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                    if (imageId > 0)
                    {
                        ApplyAvatarImage(n, imageId);
                    }

                    return n;
                }
                else
                {
                    UserData n = CreateInstance<UserData>();
                    n.id = steamID;

                    knownUsers.Add(steamID.m_SteamID, n);

                    int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                    //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                    if (imageId > 0)
                    {
                        ApplyAvatarImage(n, imageId);
                    }

                    return n;
                }
            }

            public UserData GetUserData(ulong steamID)
            {
                return GetUserData(new CSteamID(steamID));
            }
            #endregion

#if HE_STEAMCOMPLETE
            /// <summary>
            /// Tools for fetching lists of Steam Game Servers and information relating to those servers including ping, player data and rule data
            /// </summary>
            public static GameServerBrowserTools ServerBrowser => current.client.serverBrowser;

            /// <summary>
            /// Tools for fetching lists of Steam Game Servers and information relating to those servers including ping, player data and rule data
            /// </summary>
            public GameServerBrowserTools serverBrowser = new GameServerBrowserTools();

            #region Inventory System
            /// <summary>
            /// The collection of inventory items registered for this applicaiton.
            /// </summary>
            /// <remarks>
            /// See <see cref="inventory"/> for more information. This field simply access the <see cref="inventory"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
            /// </remarks>
            public static SteamworksInventorySettings Inventory => current.client.inventory;

            /// <summary>
            /// The Steamworks Inventory settings assoceated with this applicaiton.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Steamworks Inventory is a powerful and flexable system which can enable player driven economies, microtransaction systems, in game shops using in game currency and or real currency, etc.
            /// These concepts are advanced and depend on well structured design. For security reasons there are various limitaitons on what can be done with inventory without the support of a trusted server.
            /// That said it is possible to build robust MTX, player economy, etc. systems without a dedicated server backend by using Steamworks Inventory.
            /// </para>
            /// <para>
            /// As noted this is a very complex topic, you should read and understand Valve's documenation on Steamworks Inventory before designing your game around this feature.
            /// </para>
            /// </remarks>
            public SteamworksInventorySettings inventory = new SteamworksInventorySettings();

            /// <summary>
            /// Clears and rebuilds the internal state for each item defined in the <see cref="inventory"/> object.
            /// </summary>
            /// <remarks>
            /// This can be used to refresh the current snap shot of the User's inventory, this process should be apply a minimal impact to your game processes however Valve's backend may throttle yoru requests if ran to frequently.
            /// In general you would do this when the player open's their inventory in game or performs some action that needs to confirm the state of the inventory. Please note that actions outside of your game can effect the state of a player's inventory
            /// as such you should be aware that the internal state as seen in the <see cref="inventory"/> members is a snapshot and estimate of the state based on operations ran from within this API ... it is not expected to be an assured accurate real time view.
            /// </remarks>
            public void RefreshInventory() => inventory.RefreshInventory();

            /// <summary>
            /// Test for and grants all availlable promotional items to the current user
            /// </summary>
            public void GrantAllPromotionalItems() => inventory.GrantAllPromotionalItems();

            /// <summary>
            /// <para>Grants the user a promotional item</para>
            /// <para>This will trigger the Item Instances Updated event after steam responds with the users inventory items and the items have been updated to reflect the correct instances.</para>
            /// <para> <para>
            /// <para>NOTE: this additivly updates the Instance list for effected items and is not a clean refresh!
            /// Consider a call to Refresh Inventory to resolve a complete and accurate refresh of all player items.</para>
            /// </summary>
            /// <paramref name="itemDefinition">The item type to grant to the user.</paramref>
            public void GrantPromotionalItem(InventoryItemDefinition itemDefinition) => GrantPromotionalItem(itemDefinition);
            #endregion

            #region User Generated Content System
            /// <summary>
            /// Returns a list of UGC file IDs for all items the local user is subscribed to.
            /// </summary>
            /// <remarks>
            /// Please see <see cref="GameServices.SteamworksUserGeneratedContent"/> to learn how to get speciific information from each item.
            /// Key methods are <see cref="GameServices.SteamworksUserGeneratedContent.GetItemInstallInfo(PublishedFileId_t, out ulong, out string, out DateTime)"/> which provides details about the items install location and
            /// <see cref="GameServices.SteamworksUserGeneratedContent.GetItemState(PublishedFileId_t)"/> which simply provides the status of the item.
            /// </remarks>
            public static List<PublishedFileId_t> SubscribedWorkshopItems => UGC.GetSubscribedItems();
            #endregion
#endif

#if HE_STEAMPLAYERSERVICES
            #region DLC System
            public void UpdateDlcStatus()
            {
                foreach (var dlc in DLC)
                {
                    dlc.UpdateStatus();
                }
            }
            #endregion

            #region Leaderboard System
            public UnityLeaderboardRankChangeEvent evtLeaderboardRankChanged;
            public UnityLeaderboardRankUpdateEvent evtLeaderboardRankLoaded;
            public UnityLeaderboardRankChangeEvent evtLeaderboardNewHighRank;
            #endregion

            #region Remote Storage System
            public RemoteStorageSystem remoteStorage = new RemoteStorageSystem();
            #endregion

            #region Clan System
            public ClanTools clanSystem = new ClanTools();
            #endregion
#endif
        }

        /// <summary>
        /// Contains client side funcitonality
        /// </summary>
        /// <remarks>
        /// Note that this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameClient client = new GameClient();
#endif

        public UnityEvent onInitalized;
        public StringUnityEvent onInitalizationError;
        
        public void Init()
        {
            if (Initialized)
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Tried to initalize the Steamworks API twice in one session!";
                onInitalizationError.Invoke("Tried to Initialize the SteamAPI twice in one session!");
                throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            if (!Packsize.Test())
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Packesize Test returned false, the wrong version of the Steamowrks.NET is being run in this platform.";
                onInitalizationError.Invoke("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
                throw new System.Exception("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }

            if (!DllCheck.Test())
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
                onInitalizationError.Invoke("Dll Check Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
                throw new System.Exception("Dll Check Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            current = this;
#if !UNITY_SERVER
            try
            {
                // If Steamworks is not running or the game wasn't started through Steamworks, SteamAPI_RestartAppIfNecessary starts the
                // Steamworks client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steamworks AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                //AppId = SteamAppId != null ? new AppId_t(SteamAppId.Value) : AppId_t.Invalid;
                if (Steamworks.SteamAPI.RestartAppIfNecessary(applicationId))
                {
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Steamworks.NET could not load, steam_api.dll/so/dylib. It's likely not in the correct location.";
                // We catch this exception here, as it will be the first occurence of it.
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
                onInitalizationError.Invoke("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                Application.Quit();
                return;
            }

            if (isDebugging)
                Debug.Log("Initalizing Steam API");
            Initialized = Steamworks.SteamAPI.Init();

#if HE_STEAMCOMPLETE
            client.inventory.Register();
            client.inventory.ClearItemCounts();
            client.inventory.RefreshInventory();
#endif
#if HE_STEAMPLAYERSERVICES
            RegisterDLC();
            client.UpdateDlcStatus();

            client.remoteStorage.Initalize();

            foreach (var board in leaderboards)
            {
                board.Register();
                board.evtUserRankChanged.AddListener(client.evtLeaderboardRankChanged.Invoke);
                board.evtUserRankLoaded.AddListener(client.evtLeaderboardRankLoaded.Invoke);
                board.evtUserNewHighRank.AddListener(client.evtLeaderboardNewHighRank.Invoke);
            }

            client.clanSystem.RegisterSystem();
#endif

#endif
#if UNITY_SERVER //|| UNITY_EDITOR
            EServerMode eMode = EServerMode.eServerModeNoAuthentication;
            if (server.usingGameServerAuthApi)
                eMode = EServerMode.eServerModeAuthenticationAndSecure;
            if (isDebugging)
                Debug.Log("Initalizing Steam API: ("+ server.ip + ", " + server.gamePort.ToString() + ", " + server.queryPort.ToString() + ", " + eMode.ToString() +", "+ server.serverVersion + ")");
            Initialized = Steamworks.GameServer.Init(server.ip, server.gamePort, server.queryPort, eMode, server.serverVersion);
#endif

            if (!Initialized)
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Steamworks initalization failed. refer to Valve's documenation or the comments in the log/console for more information.";
                onInitalizationError.Invoke("Steamworks Initialization failed. Refer to Valve's documentation or the comment above this line for more information.");
                return;
            }

            onInitalized.Invoke();


#if !UNITY_SERVER
            //Register the achievements system
            client.RegisterAchievementsSystem();
            client.RegisterFriendsSystem();
#endif
        }

        #region Utilities
        #region Colors
        public static class Colors
        {
            public static Color SteamBlue = new Color(0.2f, 0.60f, 0.93f, 1f);
            public static Color SteamGreen = new Color(0.2f, 0.42f, 0.2f, 1f);
            public static Color BrightGreen = new Color(0.4f, 0.84f, 0.4f, 1f);
            public static Color HalfAlpha = new Color(1f, 1f, 1f, 0.5f);
            public static Color ErrorRed = new Color(1, 0.5f, 0.5f, 1);
        }
        #endregion

        /// <summary>
        /// <para>Gets the App ID of the current process.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetAppID">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static AppId_t GetAppId()
        {
            return SteamUtils.GetAppID();
        }

        /// <summary>
        /// <para>Returns the Steamworks server time in Unix epoch format. (Number of seconds since Jan 1, 1970 UTC)</para>  
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetServerRealTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static uint GetServerRealUnixTime()
        {
            return SteamUtils.GetServerRealTime();
        }

        /// <summary>
        /// <para>Returns the Steamworks server time in Unix epoch format. (Number of seconds since Jan 1, 1970 UTC)</para>  
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#GetServerRealTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static DateTime GetServerRealDateTime()
        {
            return ConvertUnixDate(GetServerRealUnixTime());
        }

        /// <summary>
        /// <para>Gets the current language that the user has set.
        /// This falls back to the Steamworks UI language if the user hasn't explicitly picked a language for the title.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetCurrentGameLanguage">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentGameLanguage()
        {
            return SteamApps.GetCurrentGameLanguage();
        }

        /// <summary>
        /// <para>Gets the buildid of this app, may change at any time based on backend updates to the game.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetAppBuildId">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static int GetAppBuildId()
        {
            return SteamApps.GetAppBuildId();
        }

        /// <summary>
        /// <para>Gets the install folder for a specific AppID.
        /// This works even if the application is not installed, based on where the game would be installed with the default Steamworks library location.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetAppInstallDir">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static string GetAppInstallDir(AppId_t appId)
        {
            string results;
            SteamApps.GetAppInstallDir(appId, out results, 1024);
            return results;
        }

        /// <summary>
        /// <para>Gets the Steamworks ID of the original owner of the current app. If it's different from the current user then it is borrowed.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetAppOwner">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static CSteamID GetAppOwner()
        {
            return SteamApps.GetAppOwner();
        }

        /// <summary>
        /// <para>Gets the number of DLC pieces for the current app.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetDLCCount">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static int GetDLCCount()
        {
            return SteamApps.GetDLCCount();
        }

        /// <summary>
        /// <para>Returns a app DLC metadata object for the specified index.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#BGetDLCDataByIndex">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static AppDlcData GetDLCDataByIndex(int index)
        {
            var nData = new AppDlcData();
            if (SteamApps.BGetDLCDataByIndex(index, out nData.appId, out nData.available, out nData.name, 2048))
                return nData;
            else
            {
                nData.appId = AppId_t.Invalid;
                return nData;
            }
        }

        /// <summary>
        /// <para>Returns a collection of app DLC metadata for all available DLC.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#BGetDLCDataByIndex">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static List<AppDlcData> GetDLCData()
        {
            var results = new List<AppDlcData>();
            var count = GetDLCCount();
            for (int i = 0; i < count; i++)
            {
                var nData = new AppDlcData();
                if (SteamApps.BGetDLCDataByIndex(i, out nData.appId, out nData.available, out nData.name, 2048))
                    results.Add(nData);
            }
            return results;
        }

        /// <summary>
        /// <para>Gets the time of purchase of the specified app in Unix epoch format (time since Jan 1st, 1970).</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetEarliestPurchaseUnixTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static uint GetEarliestPurchaseUnixTime(AppId_t appId)
        {
            return SteamApps.GetEarliestPurchaseUnixTime(appId);
        }

        /// <summary>
        /// <para>Gets the time of purchase of the specified app.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetEarliestPurchaseUnixTime">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static DateTime GetEarliestPurchaseDateTime(AppId_t appId)
        {
            return ConvertUnixDate(GetEarliestPurchaseUnixTime(appId));
        }

        /// <summary>
        /// <para>Gets the command line if the game was launched via Steamworks URL, e.g. steam://run/<appid>//<command line>/. This method is preferable to launching with a command line via the operating system, which can be a security risk. In order for rich presence joins to go through this and not be placed on the OS command line, you must enable "Use launch command line" from the Installation > General page on your app.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamApps#GetLaunchCommandLine">https://partner.steamgames.com/doc/api</a>
        /// </summary>
        /// <returns></returns>
        public static string GetLaunchCommandLine()
        {
            string buffer;
            SteamApps.GetLaunchCommandLine(out buffer, 1024);
            return buffer;
        }

        /// <summary>
        /// Converts a Unix epoc style time stamp to a DateTime object
        /// </summary>
        /// <param name="nixTime"></param>
        /// <returns></returns>
        public static DateTime ConvertUnixDate(uint nixTime)
        {
            var timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return timeStamp.AddSeconds(nixTime);
        }

        /// <summary>
        /// Checks if the 'checkFlag' value is in the 'value'
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkflag"></param>
        /// <returns></returns>
        public static bool WorkshopItemStateHasFlag(EItemState value, EItemState checkflag)
        {
            return (value & checkflag) == checkflag;
        }

        /// <summary>
        /// Cheks if any of the 'checkflags' values are in the 'value'
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkflags"></param>
        /// <returns></returns>
        public static bool WorkshopItemStateHasAllFlags(EItemState value, params EItemState[] checkflags)
        {
            foreach (var checkflag in checkflags)
            {
                if ((value & checkflag) != checkflag)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Octet order from left to right as seen in string is index 0, 1, 2, 3
        /// </summary>
        /// <param name="address">And string which can be parsed by System.Net.IPAddress.Parse</param>
        /// <returns>Octet order from left to right as seen in string is index 0, 1, 2, 3</returns>
        public static byte[] IPStringToBytes(string address)
        {
            var ipAddress = IPAddress.Parse(address);
            return ipAddress.GetAddressBytes();
        }

        /// <summary>
        /// Expects octet order from index 0 to 3 for example string octet 1 as in the left most should be stored in index 0
        /// </summary>
        /// <param name="address">Expects octet order from index 0 to 3 for example string octet 1 as in the left most should be stored in index 0</param>
        /// <returns></returns>
        public static string IPBytesToString(byte[] address)
        {
            var ipAddress = new IPAddress(address);
            return ipAddress.ToString();
        }

        /// <summary>
        /// Octet order from left to right as seen in string e.g. byte 24, byte 16, byte 8, byte 0
        /// </summary>
        /// <param name="address">And string which can be parsed by System.Net.IPAddress.Parse</param>
        /// <returns>Octet order from left to right as seen in string e.g. byte 24, byte 16, byte 8, byte 0</returns>
        public static uint IPStringToUint(string address)
        {
            var ipBytes = IPStringToBytes(address);
            var ip = (uint)ipBytes[0] << 24;
            ip += (uint)ipBytes[1] << 16;
            ip += (uint)ipBytes[2] << 8;
            ip += (uint)ipBytes[3];
            return ip;
        }

        /// <summary>
        /// Returns a human friendly string version of the uint address
        /// </summary>
        /// <param name="address">Octet order from left to right as seen in string e.g. byte 24, byte 16, byte 8, byte 0</param>
        /// <returns></returns>
        public static string IPUintToString(uint address)
        {
            var ipBytes = BitConverter.GetBytes(address);
            var ipBytesRevert = new byte[4];
            ipBytesRevert[0] = ipBytes[3];
            ipBytesRevert[1] = ipBytes[2];
            ipBytesRevert[2] = ipBytes[1];
            ipBytesRevert[3] = ipBytes[0];
            return new IPAddress(ipBytesRevert).ToString();
        }
        #endregion
    }
}
#endif