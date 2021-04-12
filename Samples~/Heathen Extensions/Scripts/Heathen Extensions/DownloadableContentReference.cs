#if !DISABLESTEAMWORKS
using UnityEngine;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// A ScriptableObject referencing a Steamworks DLC app.
    /// </summary>
    /// <remarks>
    /// DLC App Id is defined on the Steamworks API in your Steamworks Portal.
    /// Please carfully read <a href="https://partner.steamgames.com/doc/store/application/dlc" /> before designing features around this concept.
    /// </remarks>
    public class DownloadableContentReference : ScriptableObject
    {
        /// <summary>
        /// The <see cref="AppId_t"/> assoceated with this DLC
        /// </summary>
        public AppId_t AppId;

#if !UNITY_SERVER
        /// <summary>
        /// Is the current user 'subscribed' to this DLC.
        /// This indicates that the current user has right/license this DLC or not.
        /// </summary>
        public bool IsSubscribed => SteamApps.BIsSubscribedApp(AppId);
        /// <summary>
        /// Is this DLC currently installed.
        /// </summary>
        public bool IsDlcInstalled => SteamApps.BIsDlcInstalled(AppId);
#endif
    }
}
#endif
