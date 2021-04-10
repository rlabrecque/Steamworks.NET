#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// Handles Friend chat message events
    /// See <a href="https://partner.steamgames.com/doc/api/steam_api#EChatEntryType">https://partner.steamgames.com/doc/api/steam_api#EChatEntryType</a> for more details
    /// </summary>
    /// <remarks>
    /// will invoke a method that takes a <see cref="UserData"/>, string and <see cref="EChatEntryType"/> as a param e.g.
    /// <code>
    /// private void HandleFriendChatMessageEvent(SteamUserData user, string message, EChatEntryType entryType)
    /// {
    /// }
    /// </code>
    /// </remarks>
    [Serializable]
    public class FriendChatMessageEvent : UnityEvent<UserData, string, EChatEntryType> { }
}
#endif