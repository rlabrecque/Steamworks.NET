#if !DISABLESTEAMWORKS
using UnityEngine;
using UnityEngine.Events;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> referencing a Steamworks Achievement.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply references an achievement that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements" />
    /// </para>
    /// </remarks>
    public class AchievementReference : ScriptableObject
    {
        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        public string achievementId;

        /// <summary>
        /// Called internally when Valve callsback with stats and achievement data
        /// </summary>
        /// <remarks>
        /// You should never need to call this manually
        /// </remarks>
        public void Update()
        {
            bool achievedBuffer;
            bool ret = SteamUserStats.GetAchievement(achievementId, out achievedBuffer);
            if (ret)
            {
                Name = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "name");
                Description = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "desc");
                Hidden = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "hidden") == "1";
            }
            else
            {
                Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + achievementId + "\nConfrim Is it registered in the Steamworks Partner site and that changes have been published.");
            }
        }

#region Client Only
#if !UNITY_SERVER
        /// <summary>
        /// Indicates that this achievment has been unlocked by this user.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public bool IsAchieved { get; private set; }
        /// <summary>
        /// The display name for this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public string Name { get; private set; }
        /// <summary>
        /// The display description for this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public string Description { get; private set; }
        /// <summary>
        /// Is this achievement a hidden achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public bool Hidden { get; private set; }

        /// <summary>
        /// Occures when this achivement has been unlocked.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public UnityEvent eventUnlock;

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement" />
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public void Unlock()
        {
            if (SteamUserStats.SetAchievement(achievementId))
            {
                IsAchieved = true;
                eventUnlock?.Invoke();
            }
        }

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement" />
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public void ClearAchievement()
        {
            if(SteamUserStats.ClearAchievement(achievementId))
            {
                IsAchieved = false;
            }
        }
#endif
#endregion

#region Server Only
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
#endregion
    }
}
#endif
