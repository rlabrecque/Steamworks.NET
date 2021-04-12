#if !DISABLESTEAMWORKS
using UnityEngine;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// Handles the initalization, update and disposal of the Steam APIs
    /// </summary>
    public class SteamController : MonoBehaviour
    {
        [Tooltip("This will mark this GameObject as DontDestroyOnLoad causing Unity to move it to a hidden scene.\nUse with caution.")]
        public bool DoNotDestroyOnLoad = false;
        public SteamSystem system;

        private void Start()
        {
            if (DoNotDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (system != null)
                system.Initialize();
        }

        private void OnDisable()
        {
#if UNITY_SERVER
            if (system.server.usingGameServerAuthApi)
                SteamGameServer.EnableHeartbeats(false);

            SteamGameServer.LogOff();
#endif
        }

        private void OnDestroy()
        {
            if (!SteamSystem.Initialized)
                return;

#if !UNITY_SERVER
            SteamAPI.Shutdown();
#else
            GameServer.Shutdown();
#endif
        }

        private void Update()
        {
            if (!SteamSystem.Initialized)
            {
                return;
            }

#if UNITY_SERVER
            GameServer.RunCallbacks();

#else
            SteamAPI.RunCallbacks();
#endif
        }
    }
}
#endif
