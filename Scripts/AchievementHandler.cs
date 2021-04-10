#if !DISABLESTEAMWORKS
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// Exposes the 'On Unlock' Unity Event of an achievement to the Unity inspector. 
    /// </summary>
    /// <remarks>
    /// Use this componenet to connect methods within other mono behaviours to the On Unlock event of a specific <see cref="AchievementObject"/> object.
    /// </remarks>
    public class AchievementHandler : MonoBehaviour
    {
        /// <summary>
        /// A reference to the achievement this component should listen for
        /// </summary>
        public AchievementObject achievement;
#if !UNITY_SERVER
        public UnityEvent onUnlock;

        private void OnEnable()
        {
            achievement.OnUnlock.AddListener(handleUnlock);
        }

        private void OnDisable()
        {
            achievement.OnUnlock.RemoveListener(handleUnlock);
        }

        private void handleUnlock()
        {
            onUnlock.Invoke();
        }
#endif
    }
}
#endif