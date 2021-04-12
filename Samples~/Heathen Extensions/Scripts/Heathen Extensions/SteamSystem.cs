#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// Primary access point for Heathen Engineering's Steamworks extensions
    /// </summary>
    [CreateAssetMenu(menuName = "Steamworks/System")]
    public class SteamSystem : ScriptableObject
    {
        /// <summary>
        /// Initalizes the Steam API
        /// </summary>
        /// <remarks>
        /// This will initalize the Client API on client builds and the Game Server API on server builds.
        /// Note that for server builds you should call <see cref="GameServer.LogOn"/> after you have initalized network layer.
        /// This can be done via <code>SteamSystem.Current.server.LogOn();</code>
        /// </remarks>
        /// <returns></returns>
        public bool Initialize()
        {
            if (Initialized)
            {
                throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            if (!Packsize.Test())
            {
                throw new System.Exception("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }

            if (!DllCheck.Test())
            {
                throw new System.Exception("Dll Check Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            Current = this;

#if UNITY_SERVER
            EServerMode eMode = EServerMode.eServerModeNoAuthentication;
            if (server.usingGameServerAuthApi)
                eMode = EServerMode.eServerModeAuthenticationAndSecure;

            Initialized = Steamworks.GameServer.Init(server.ip, server.gamePort, server.queryPort, eMode, server.serverVersion);

            if(!Initialized)
            {
                return false;
            }

            server.RegisterCallbacks();

            return true;
#else
            try
            {
                // If Steamworks is not running or the game wasn't started through Steamworks, SteamAPI_RestartAppIfNecessary starts the
                // Steamworks client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steamworks AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                // AppId = SteamAppId != null ? new AppId_t(SteamAppId.Value) : AppId_t.Invalid;
                if (Steamworks.SteamAPI.RestartAppIfNecessary(applicationId))
                {
                    Application.Quit();
                    return false;
                }
            }
            catch (System.DllNotFoundException e)
            {
                // We catch this exception here, as it will be the first occurence of it.
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
                Application.Quit();
                return false;
            }

            Initialized = SteamAPI.Init();

            if (!Initialized)
            {
                Debug.LogError("[Steamworks.NET] failed to initalize, please refer to the documentaiton listed at https://partner.steamgames.com/doc/api/steam_api#SteamAPI_Init");
                return false;
            }

            client.RegisterAchievementsSystem();
            SteamUserStats.RequestCurrentStats();

            foreach (var board in leaderboards)
            {
                board.Register();
            }

            return true;
#endif
        }

#region internal types
        [Serializable]
        public class StatEvent : UnityEvent<StatReference>
        { }
#endregion

        /// <summary>
        /// Reference to the current applied SteamSystem object
        /// </summary>
        public static SteamSystem Current { get; private set; }
        /// <summary>
        /// Indicates rather or not the Steam APIs have been initalized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// The AppId_t value configured and initalized for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the app id value the developer would have typed in to the Unity Editor when setting up the project.
        /// Note that players can easily modify this value to cause the Steamworks API to initalize as a different game or can use the steam_appid.txt to force the Steamworks API to register as a different ID.
        /// You can confirm what ID Valve sees this program as running as by calling <see cref="SteamUtils.GetAppId"/> you can then compare the values to insure your user is not attempting to manipulate your program.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void AppIdTests()
        ///     {
        ///         Debug.Log("Configred to run as " + SteamSystem.ApplicationId + ", actually running as " + SteamUtils.GetAppId() );
        ///     }
        /// }
        /// </code>
        /// </example>
        public AppId_t applicationId = new AppId_t(0x0);
        /// <summary>
        /// The list of stats registered for this applicaiton.
        /// </summary>
        public List<StatReference> stats = new List<StatReference>();
        /// <summary>
        /// The list of achievements registered for this applicaiton.
        /// </summary>
        public List<AchievementReference> achievements = new List<AchievementReference>();
        /// <summary>
        /// The list of DLC registered for this applicaiton.
        /// </summary>
        public List<DownloadableContentReference> dlc = new List<DownloadableContentReference>();
        /// <summary>
        /// The list of leaderboards registered for this applicaiton.
        /// </summary>
        public List<LeaderboardReference> leaderboards = new List<LeaderboardReference>();

        #region Client Only
#if !UNITY_SERVER
        /// <summary>
        /// configuration settings and features unique to the Client API
        /// </summary>
        /// <remarks>
        /// Note that if you have defined CONDITIONAL_COMPILE in your script defines then this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameClient
        {
            [Serializable]
            public class UserStatsReceivedEvent : UnityEvent<UserStatsReceived_t>
            { }

            [Serializable]
            public class UserStatsStoredEvent : UnityEvent<UserStatsStored_t>
            { }

            [Serializable]
            public class UserAchievementStoredEvent : UnityEvent<UserAchievementStored_t>
            { }

#pragma warning disable IDE0052 // Remove unread private members
            private CGameID m_GameID;
            private Callback<UserStatsReceived_t> m_UserStatsReceived;
            private Callback<UserStatsStored_t> m_UserStatsStored;
            private Callback<UserAchievementStored_t> m_UserAchievementStored;
#pragma warning restore IDE0052 // Remove unread private members

#region Events
            /// <summary>
            /// Occures when user stats and achievements are recieved from Valve
            /// </summary>
            [HideInInspector]
            public UserStatsReceivedEvent eventUserStatsReceived;
            /// <summary>
            /// Occures when user stats are stored to Valve
            /// </summary>
            [HideInInspector]
            public UserStatsStoredEvent evtUserStatsStored;
            /// <summary>
            /// Occures when Achivements are stored to Valve
            /// </summary>
            [HideInInspector]
            public UserAchievementStoredEvent evtAchievementStored;
#endregion

#region Stats & Achievement System

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
            }

            private void HandleUserStatsReceived(UserStatsReceived_t pCallback)
            {
                // we may get callbacks for other games' stats arriving, ignore them
                if ((ulong)m_GameID == pCallback.m_nGameID)
                {
                    if (EResult.k_EResultOK == pCallback.m_eResult)
                    {
                        // load achievements
                        foreach (var ach in Current.achievements)
                        {
                            ach.Update();
                        }

                        foreach (var stat in Current.stats)
                        {
                            stat.Update();
                        }
                    }
                    else
                    {
                        Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
                    }

                    eventUserStatsReceived.Invoke(pCallback);
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
#endregion

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
#endregion

        #region Server Only
#if UNITY_SERVER || UNITY_EDITOR
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
            public struct RuleValue
            {
                public string key;
                public string value;
            }

            [Serializable]
            public class DisconnectedEvent : UnityEvent<SteamServersDisconnected_t> { }

            [Serializable]
            public class ConnectedEvent : UnityEvent<SteamServersConnected_t> { }

            [Serializable]
            public class FailureEvent : UnityEvent<SteamServerConnectFailure_t> { }

            [Serializable]
            public class UnloadedStatsEvent : UnityEvent<GSStatsUnloaded_t> { }

            public bool autoInitalize = false;

            [Tooltip("will usually be zero.\nIf you are on a machine with multiple IP addresses, you can pass a non-zero value here and the relevant sockets will be bound to that IP.\nThis can be used to ensure that the IP you desire is the one used in the server browser.")]
            public uint ip = 0;
            
            [Tooltip("is the port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE for usQueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master  server updater. See references to GameSocketShare in isteamgameserver.hn")]
            public ushort queryPort = 27016;
            [Tooltip("the port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port.")]
            public ushort gamePort = 27015;
            [Tooltip("The version string should be in the form x.x.x.x, and is used by the master server to detect when the server is out of date.  (Only servers with the latest version will be listed.)")]
            public string serverVersion = "1.0.0.0";
            [Tooltip("Only used if supporting spectators.")]
            public ushort spectatorPort = 27017;

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
            public List<RuleValue> rulePairs = new List<RuleValue>();

            [Header("Events")]
            public UnityEvent gameServerShuttingDown;
            public DisconnectedEvent disconnected;
            public ConnectedEvent connected;
            public FailureEvent failure;
            public UnloadedStatsEvent userStatsUnloaded;

#pragma warning disable IDE0052 // Remove unread private members
            private Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
            private Callback<SteamServersConnected_t> steamServersConnected;
            private Callback<SteamServersDisconnected_t> steamServersDisconnected;
            private CallResult<GSStatsReceived_t> steamGameServerStatsReceived;
            private CallResult<GSStatsStored_t> steamGameServerStatsStored;
            private CallResult<GSStatsUnloaded_t> steamGameServerStatsUnloaded;
#pragma warning restore IDE0052 // Remove unread private members

            private void OnSteamServersDisconnected(SteamServersDisconnected_t param)
            {
                disconnected.Invoke(param);
            }

            private void OnSteamServersConnected(SteamServersConnected_t param)
            {
                serverId = SteamGameServer.GetSteamID();

                if (rulePairs != null && rulePairs.Count > 0)
                {
                    var pairString = "Seting the following rules:\n";

                    foreach (var pair in rulePairs)
                    {
                        SteamGameServer.SetKeyValue(pair.key, pair.value);
                        pairString += "\n\t[" + pair.key + "] = [" + pair.value + "]";
                    }
                    Debug.Log(pairString);
                }

                connected.Invoke(param);
            }

            private void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
            {
                if (!param.m_bStillRetrying)
                    Debug.LogError("SteamGameServer failed to log on: " + param.m_eResult);

                failure.Invoke(param);
            }

            public bool LogOn()
            {
                SteamGameServer.SetModDir(gameDirectory);
                SteamGameServer.SetProduct(Current.applicationId.m_AppId.ToString());
                SteamGameServer.SetGameDescription(gameDescription);
                SteamGameServer.SetMaxPlayerCount(maxPlayerCount);
                SteamGameServer.SetPasswordProtected(isPasswordProtected);
                SteamGameServer.SetServerName(serverName);
                SteamGameServer.SetBotPlayerCount(botPlayerCount);
                SteamGameServer.SetMapName(mapName);
                SteamGameServer.SetDedicatedServer(isDedicated);

                if (supportSpectators)
                {
                    SteamGameServer.SetSpectatorPort(spectatorPort);
                    SteamGameServer.SetSpectatorServerName(spectatorServerName);
                }

                Debug.Log("Configuring the SteamGameServer interface:\n\tServer Name: " + serverName +
                    "\n\tDescription: " + gameDescription +
                    "\n\tProduct: " + Current.applicationId +
                    "\n\tIs Dedicated Server: " + isDedicated +
                    "\n\tIs Password Protected: " + isPasswordProtected +
                    "\n\tMax Players: " + maxPlayerCount +
                    "\n\tBot Player Count: " + botPlayerCount +
                    "\n\tMod Dir: " + gameDirectory +
                    "\n\tMap Name: " + mapName);

                if (anonymousServerLogin)
                {
                    Debug.Log("Logging on with Anonymous");

                    SteamGameServer.LogOnAnonymous();
                }
                else
                {
                    if(string.IsNullOrEmpty(gameServerToken))
                    {
                        Debug.LogError("Attempted log on with game server token but the game server token value of the SteamSystem.server object is null or empty.");
                        return false;
                    }

                    Debug.Log("Logging on with token");

                    SteamGameServer.LogOn(gameServerToken);
                }

                if (usingGameServerAuthApi || enableHeartbeats)
                {
                    SteamGameServer.EnableHeartbeats(true);
                }

                return true;
            }

            #region Achievements and Stas
            /// <summary>
            /// Store user stats for <paramref name="user"/> and invoke the <paramref name="callback"/> when complete
            /// </summary>
            /// <param name="user">The user to store stats for</param>
            /// <param name="callback">THe action to be invoked when the process is complete</param>
            public void StoreUserStats(CSteamID user, Action<GSStatsStored_t, bool> callback = null)
            {
                var call = SteamGameServerStats.StoreUserStats(user);

                if (callback != null)
                    steamGameServerStatsStored.Set(call, callback.Invoke);
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
            public void RequestUserStats(CSteamID user, Action<GSStatsReceived_t, bool> callback = null)
            {
                var call = SteamGameServerStats.RequestUserStats(user);

                if (callback != null)
                    steamGameServerStatsReceived.Set(call, callback.Invoke);
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
                steamGameServerStatsReceived = CallResult<GSStatsReceived_t>.Create();
                steamGameServerStatsStored = CallResult<GSStatsStored_t>.Create();
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
        #endregion

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
    }
}
#endif
