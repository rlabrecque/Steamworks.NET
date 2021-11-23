using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steamworks.Samples
{
    /// <summary>
    /// This will test for an existing <see cref="ExampleSteamSystem"/> if its not found it will create a new one and optionally mark it as DoNotDestroyOnLoad.
    /// If an ExampleSteamSystem is found this will do nothing.
    /// It is safe to add this to every scene in your game though optimally you would use a bootstrap scene structure and handle Steam intialization there.
    /// A boostrap scene in general wouldn't load the rest of the game unless Steam API was ready to go so it wouldn't be useful to have every scene perform this check.
    /// </summary>
    [System.Obsolete("Example only, this is meant as a demonstration of code not for produciton use.")]
    public class ExampleSteamworksBehaviour : MonoBehaviour
    {
        public uint appId;
        public bool markDoNotDestroyOnLoad;

        protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

        private void Start()
        {
            //Here we simply check if there is already an existing ExampleSteamSystem if so we do nothing, if not we create a new one

            if(!ExampleSteamSystem.Initalized)
            {
                var GO = new GameObject("Steam System");
                var system = GO.AddComponent<ExampleSteamSystem>();
                system.SetAppId(appId);

                if (markDoNotDestroyOnLoad)
                    DontDestroyOnLoad(GO);
            }
        }
    }
}
