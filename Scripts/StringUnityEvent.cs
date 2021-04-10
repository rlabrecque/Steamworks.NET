#if !DISABLESTEAMWORKS
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    [Serializable]
    public class StringUnityEvent : UnityEvent<string>
    { }
}
#endif