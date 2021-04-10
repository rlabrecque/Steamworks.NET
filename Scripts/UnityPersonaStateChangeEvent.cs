#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="PersonaStateChange_t"/> data.
    /// </summary>
    [Serializable]
    public class UnityPersonaStateChangeEvent : UnityEvent<PersonaStateChange_t>
    { }
}
#endif
