#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steamworks Achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of an achievement that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Steamworks/Achievement Object")]
    public class AchievementObject : ScriptableObject
    {
        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        public string achievementId;

#if !CONDITIONAL_COMPILE || !UNITY_SERVER
        /// <summary>
        /// Indicates that this achievment has been unlocked by this user.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [NonSerialized]
        public bool isAchieved;
        /// <summary>
        /// The display name for this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [NonSerialized]
        public string displayName;
        /// <summary>
        /// The display description for this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [NonSerialized]
        public string displayDescription;
        /// <summary>
        /// Is this achievement a hidden achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        [NonSerialized]
        public bool hidden;
        /// <summary>
        /// Occures when this achivement has been unlocked.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public UnityEvent OnUnlock;

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public void Unlock()
        { 
            if (!isAchieved)
            {
                isAchieved = true;
                SteamUserStats.SetAchievement(achievementId);
                OnUnlock.Invoke();
            }
        }

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public void ClearAchievement()
        {
            isAchieved = false;
            SteamUserStats.ClearAchievement(achievementId);
        }
#endif
#if UNITY_SERVER || UNITY_EDITOR
        /// <summary>
        /// Unlock the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void Unlock(CSteamID user)
        {
            SteamGameServerStats.SetUserAchievement(user, achievementId);
        }

        /// <summary>
        /// Clears the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void ClearAchievement(CSteamID user)
        {
            SteamGameServerStats.ClearUserAchievement(user, achievementId);
        }

        /// <summary>
        /// Gets the achievement status for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool GetAchievementStatus(CSteamID user)
        {
            bool achieved;
            SteamGameServerStats.GetUserAchievement(user, achievementId, out achieved);
            return achieved;
        }
#endif
    }
}
#endif