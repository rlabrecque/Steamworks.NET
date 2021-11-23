using UnityEngine;

namespace Steamworks.Samples
{
    /// <summary>
    /// This should only ever be created by the <see cref="ExampleSteamworksBehaviour"/>.
    /// When created the <see cref="ExampleSteamworksBehaviour"/> will set the target App ID and will have already insured no other system is running.
    /// </summary>
    [System.Obsolete("Example only, this is meant as a demonstration of code not for produciton use.\n\nThis should not be added to a game object manually, please use the ExampleSteamworksBehaviour which will perfrom checks before creating this object.")]
    [HideInInspector]
    public class ExampleSteamSystem : MonoBehaviour
    {
        /// <summary>
        /// Checks if the API is avialable and initalized
        /// </summary>
        public static bool Initalized
        {
            get
            {
                try
                {
#if UNITY_SERVER
                    InteropHelp.TestIfAvailableGameServer();
#else
                    InteropHelp.TestIfAvailableClient();
#endif
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static ExampleSteamSystem current = null;

        /// <summary>
        /// deligate for Steam API warning messages
        /// </summary>
        protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
        private uint appId = AppId_t.Invalid.m_AppId;

        /// <summary>
        /// Handler invoked by Steam API when teh SteamAPIWarningMessageHook callback is received
        /// </summary>
        /// <remarks>
        /// Note the invoke callback, this is nessisary if you want to support IL2CPP
        /// </remarks>
        /// <param name="nSeverity"></param>
        /// <param name="pchDebugText"></param>
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        /// <summary>
        /// Required to support Unity's editor feature "Disable Domain Reloading"
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
            current = null;
        }

        /// <summary>
        /// <list type="number">
        /// <item>Checks package size and reports error if incorrect.</item>
        /// <item>Checks DLLs and reports error if incorrect.</item>
        /// <item>Calls RestartAppIfNecessary which will restart the app from Steam Client unless steam_appid.txt is present in the working directory.</item>
        /// <item>Initalizes the Steam Client API</item>
        /// <item>Connects the SteamAPIWarningMessageHook callback to its handler</item>
        /// </list>
        /// </summary>
        private void Start()
        {
            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
                enabled = false;
                return;
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
                enabled = false;
                return;
            }

            if(!SteamAPI.IsSteamRunning())
            {
                Debug.LogError("[Steamworks.NET] Steam client must be running for the API to intialize.", this);
                enabled = false;
                return;
            }

            try
            {
                // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
                // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(appId)))
                {
                    Debug.LogError("[Steamworks.NET] Restart App If Necessary returned true sugesting you do not have the steam_appid.txt located in the working directory of this app.", this);
                    Application.Quit();
                }
            }
            catch (System.DllNotFoundException e)
            { // We catch this exception here, as it will be the first occurrence of it.
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
                enabled = false;
                Application.Quit();
                return;
            }

            if (!SteamAPI.Init())
            {
                // Initializes the Steamworks API.
                // If this returns false then this indicates one of the following conditions:
                // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
                // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
                // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
                // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
                // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
                // Valve's documentation for this is located here:
                // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                enabled = false;
                return;
            }

            if (m_SteamAPIWarningMessageHook == null)
            {
                // Set up our callback to receive warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
            }

            Debug.Log("Steam API initalized by " + SteamFriends.GetPersonaName() + " for app " + SteamUtils.GetAppID() + "\nIf this is not the expected AppID then please check the steam_appid.txt and insure its set with the proper value, then restart both Unity Editor and Visual Studio or any other tools which may have mounted the active process.");
        }

        /// <summary>
        /// On update runs the client API callbacks
        /// </summary>
        private void Update()
        {
            //It is not possible to do this unless SteamAPI is initalized
            SteamAPI.RunCallbacks();
        }

        /// <summary>
        /// On destroy shuts down the client API
        /// </summary>
        private void OnDestroy()
        {
            SteamAPI.Shutdown();
        }

        /// <summary>
        /// Called by the SteamworksBeahviour when it creates this object to set the target AppID
        /// </summary>
        /// <param name="appId"></param>
        public void SetAppId(uint appId)
        {
            this.appId = appId;
        }
    }
}
