#if !DISABLESTEAMWORKS
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// Used in the <see cref="HeathenEngineering.Steam.GameServices.SteamworksVoiceManager"/> class to return VoiceStream data
    /// </summary>
    /// <remarks>
    /// This is a simple wrap around UnityEvent&lt;byte[]&gt; to make it visible in Unity Editor windows.
    /// The event will invoke a method that takes a byte[] as a param such as
    /// <code>
    /// private void HandleByteArrayEvent(byte[] param) 
    /// {
    /// }
    /// </code>
    /// </remarks>
    [Serializable]
    public class ByteArrayEvent : UnityEvent<byte[]>
    { }
}
#endif