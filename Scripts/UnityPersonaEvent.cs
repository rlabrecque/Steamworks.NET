#if !DISABLESTEAMWORKS
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="UserData"/> data.
    /// </summary>
    [Serializable]
    public class UnityPersonaEvent : UnityEvent<UserData>
    { }
}
#endif
