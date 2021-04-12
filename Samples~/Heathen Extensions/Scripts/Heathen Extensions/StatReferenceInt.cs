#if !DISABLESTEAMWORKS
using System;
using System.ComponentModel;
using UnityEngine;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> for referencing a Steamworks Stat.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of a stat that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements" />
    /// </para>
    /// </remarks>
    [Serializable]
    public class StatReferenceInt : StatReference
    {
        [SerializeField]
        private int value;
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steamworks API
        /// </summary>
        public int Value
        {
            get { return value; }
            set
            {
                SetValue(value);
            }
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public override StatDataType DataType { get { return StatDataType.Int; } }

        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override float GetFloatValue()
        {
            return Value;
        }

        /// <summary>
        /// Returns the value of this stat as an int.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public override int GetIntValue()
        {
            return (int)Value;
        }

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override void SetValue(float value)
        {
            if (this.value != value)
            {
                this.value = (int)value;
                if (SteamUserStats.SetStat(statName, value))
                    eventValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetValue(int value)
        {
            if (this.value != value)
            {
                this.value = value;
                if (SteamUserStats.SetStat(statName, value))
                    eventValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// This stores all stats to the Valve backend servers it is not possible to store only 1 stat at a time
        /// Note that this will cause a callback from Steamworks which will cause the stats to update
        /// </summary>
        public override void StoreStats()
        {
            SteamUserStats.StoreStats();
        }

        /// <summary>
        /// This should only be called internally when the Steamworks client notifies the system of an updated value
        /// This does not call SetStat on the Steamworks backend
        /// </summary>
        /// <param name="value"></param>
        internal override void Update()
        {
            if (!SteamUserStats.GetStat(statName, out value))
                Debug.LogWarning("SteamUserStats.GetAchievement failed for Stat " + statName + "\nIs it registered in the Steamworks Partner site and the correct data type?");
        }
    }
}
#endif
