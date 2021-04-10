#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamAPI
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steamworks Stat.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of a stat that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [Serializable]
    [CreateAssetMenu(menuName = "Steamworks/Stat Object (float)")]
    public class FloatStatObject : StatObject
    {
        [SerializeField]
        private float value;
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steamworks API
        /// </summary>
        public float Value 
        {
            get { return value; }
            set 
            {
                SetFloatStat(value);
            }
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public override StatDataType DataType { get { return StatDataType.Float; } }

        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
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
        public override void SetFloatStat(float value)
        {
            if (this.value != value)
            {
                this.value = value;
                SteamUserStats.SetStat(statName, value);
                ValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetIntStat(int value)
        {
            if (this.value != value)
            {
                this.value = value;
                SteamUserStats.SetStat(statName, value);
                ValueChanged.Invoke(this);
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
        internal override void InternalUpdateValue(int value)
        {
            if (value != Value)
            {
                Value = value;
                ValueChanged.Invoke(this);
            }
        }

        /// <summary>
        /// This should only be called internally when the Steamworks client notifies the system of an updated value
        /// This does not call SetStat on the Steamworks backend
        /// </summary>
        /// <param name="value"></param>
        internal override void InternalUpdateValue(float value)
        {
            if (value != Value)
            {
                Value = value;
                ValueChanged.Invoke(this);
            }
        }
    }
}
#endif