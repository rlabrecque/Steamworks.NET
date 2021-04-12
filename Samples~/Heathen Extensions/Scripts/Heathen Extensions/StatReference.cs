#if !DISABLESTEAMWORKS
using UnityEngine;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> referencing a Steamworks Stat.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object is simply reference to a stat defined in the Steamworks developer portal.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements" />
    /// </para>
    /// </remarks>
    public abstract class StatReference : ScriptableObject
    {
#region internal types
        /// <summary>
        /// The availble type of stat data used in the Steamworks API
        /// </summary>
        public enum StatDataType
        {
            Int,
            Float
        }
#endregion

        /// <summary>
        /// The name of the stat as it appears in the Steamworks Portal
        /// </summary>
        public string statName;
        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public abstract StatDataType DataType { get; }
        /// <summary>
        /// This should only be called internally when the Steamworks client notifies the system of an updated value
        /// This does not call SetStat on the Steamworks backend
        /// </summary>
        /// <param name="value"></param>
        internal abstract void Update();
        /// <summary>
        /// Returns the value of this stat as an int.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public abstract int GetIntValue();
        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public abstract float GetFloatValue();
        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public abstract void SetValue(int value);
        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public abstract void SetValue(float value);
        /// <summary>
        /// This stores all stats to the Valve backend servers it is not possible to store only 1 stat at a time
        /// Note that this will cause a callback from Steamworks which will cause the stats to update
        /// </summary>
        public abstract void StoreStats();
        /// <summary>
        /// Occures when the Set Value methods are called.
        /// </summary>
        public SteamSystem.StatEvent eventValueChanged;

        

#if UNITY_SERVER || UNITY_EDITOR
        /// <summary>
        /// Get the int value of this stat for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// IMPORTANT: you must first call <see cref="SteamworksClientApiSettings.GameServer.RequestUserStats(CSteamID, Action{GSStatsReceived_t})"/> via SteamSettings.current.server.RequestUserStats(id, callbackMethod);
        /// </para>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public int GetUserIntStat(CSteamID user)
        {
            int buffer;
            SteamGameServerStats.GetUserStat(user, statName, out buffer);
            return buffer;
        }

        /// <summary>
        /// Get the float value of this stat for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// IMPORTANT: you must first call <see cref="SteamworksClientApiSettings.GameServer.RequestUserStats(CSteamID, Action{GSStatsReceived_t})"/> via SteamSettings.current.server.RequestUserStats(id, callbackMethod);
        /// </para>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public float GetUserFloatStat(CSteamID user)
        {
            float buffer;
            SteamGameServerStats.GetUserStat(user, statName, out buffer);
            return buffer;
        }

        /// <summary>
        /// Sets a integer value for the user on this stat
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="value"></param>
        public void SetUserStat(CSteamID user, int value)
        {
            SteamGameServerStats.SetUserStat(user, statName, value);
        }

        /// <summary>
        /// Sets a float value for the user on this stat
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="value"></param>
        public void SetUserStat(CSteamID user, float value)
        {
            SteamGameServerStats.SetUserStat(user, statName, value);
        }

        /// <summary>
        /// Updates the users average rate for this stat
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="countThisSession"></param>
        /// <param name="sessionLength"></param>
        public void UpdateUserAvgRateStat(CSteamID user, float countThisSession, double sessionLength)
        {
            SteamGameServerStats.UpdateUserAvgRateStat(user, statName, countThisSession, sessionLength);
        }
#endif
    }
}
#endif
