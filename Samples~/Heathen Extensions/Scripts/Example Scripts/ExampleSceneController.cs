#if !DISABLESTEAMWORKS
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Steamworks.HeathenExtensions.Examples
{
    [HelpURL("https://www.heathenengineering.com/steamworks")]
    [Obsolete("This is an example script and is not fit for produciton use.")]
    public class ExampleSceneController : MonoBehaviour
    {
        [Header("UI Labels")]
        public Text NumGames;
        public Text NumWins;
        public Text NumLosses;
        public Text FeetTraveled;
        public Text MaxFeetTraveled;
        public Text Win1Game;
        public Text Win100Games;
        public Text TravelFarAccum;
        public Text TravelFarSingle;

        [Header("Stat References")]
        /// <summary>
        /// This demonstrates a direct stat reference;
        /// Direct reference is the most efficent method
        /// </summary>
        public StatReferenceInt NumGamesStat;
        public StatReferenceInt NumLossesStat;
        public StatReferenceFloat FeetTraveledStat;
        public StatReferenceFloat MaxFeetTraveledStat;

        /// <summary>
        /// This demonstrates a direct achievemnt reference
        /// Direct reference is the most efficent method
        /// </summary>
        public AchievementReference Win1Ach;
        public AchievementReference TravelFarAccumAch;
        public AchievementReference TravelFarSingleAch;

        /// <summary>
        /// This private stat is updated in the Start method and demonstrates look up of stats
        /// </summary>
        private StatReferenceInt NumWinsStat;

        private AchievementReference Win100Ach;

        /// <summary>
        /// Simple bool field used to sync between the initalizaiton coroutine and the update loop
        /// </summary>
        private bool ready = false;

        private void Start()
        {
            StartCoroutine(WaitForInitalization());
        }

        /// <summary>
        /// Demonstrates a method of waiting for Steam API initalizaiton
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForInitalization()
        {
            yield return new WaitUntil(() => { return SteamSystem.Initialized; });

            //This demonstrates a lookup; look up is less efficent than a direct reference due to the clock time required to find the stat vs the time required to resolve a reference
            NumWinsStat = SteamSystem.Current.stats.FirstOrDefault(p => p.statName == "NumWins") as StatReferenceInt;

            //This demonstrates a lookup; look up is less efficent than a direct reference due to the clock time required to find the achievement vs the time requried to resolve a reference
            Win100Ach = SteamSystem.Current.achievements.FirstOrDefault(p => p.achievementId == "ACH_WIN_100_GAMES");

            //This demonstrates the use of a Untiy Event to drive change of value responce for stats
            NumGamesStat.eventValueChanged.AddListener(HandleNumGamesStatUpdated);

            ready = true;
        }

        private void HandleNumGamesStatUpdated(StatReference arg0)
        {
            NumGames.text = "NumGames: " + arg0.GetIntValue();
        }

        private void Update()
        {
            if (!ready)
                return;

            //This demonstrates use of objects via reference
            //Stats
            NumGames.text = "NumGames: " + NumGamesStat.Value;
            NumWins.text = "NumWins: " + NumWinsStat.Value;
            NumLosses.text = "NumLosses: " + NumLossesStat.Value;
            FeetTraveled.text = "FeetTraveled: " + FeetTraveledStat.Value;
            MaxFeetTraveled.text = "MaxFeetTraveled: " + MaxFeetTraveledStat.Value;

            //Achievements
            Win1Game.text = Win1Ach.achievementId + "\n" + Win1Ach.Name + "\n" + Win1Ach.Description + "\nAchieved: " + Win1Ach.IsAchieved.ToString();
            Win100Games.text = Win100Ach.achievementId + "\n" + Win100Ach.Name + "\n" + Win100Ach.Description + "\nAchieved: " + Win100Ach.IsAchieved.ToString();
            TravelFarAccum.text = TravelFarAccumAch.achievementId + "\n" + TravelFarAccumAch.Name + "\n" + TravelFarAccumAch.Description + "\nAchieved: " + TravelFarAccumAch.IsAchieved.ToString();
            TravelFarSingle.text = TravelFarSingleAch.achievementId + "\n" + TravelFarSingleAch.Name + "\n" + TravelFarSingleAch.Description + "\nAchieved: " + TravelFarSingleAch.IsAchieved.ToString();
        }

        /// <summary>
        /// Simply adds 1 to the games and wins stats
        /// </summary>
        public void AddToWinStat()
        {
            var val = NumGamesStat.Value + 1;
            NumGamesStat.SetValue(val);
            val = NumWinsStat.Value + 1;
            NumWinsStat.SetValue(val);
        }

        /// <summary>
        /// Simply adds 1 to the games and loss stats
        /// </summary>
        public void AddToLossStat()
        {
            var val = NumGamesStat.Value + 1;
            NumGamesStat.SetValue(val);
            val = NumLossesStat.Value + 1;
            NumLossesStat.SetValue(val);
        }

        //Achievements are being unlocked and rest directly on the button click event.

        public void MoreFromHeathen()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/integration/steamworks-v2-complete-190316");
        }
    }
}
#endif
