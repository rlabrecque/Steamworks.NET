#if !DISABLESTEAMWORKS
using System;
using UnityEngine;

namespace Steamworks.HeathenExtensions
{
    /// <summary>
    /// A ScriptableObject referencing a Steamworks leaderboard.
    /// </summary>
    public class LeaderboardReference : ScriptableObject
    {
        /// <summary>
        /// Should the board be created if missing on the target app
        /// </summary>
        public bool createIfMissing;
        /// <summary>
        /// If creating a board what sort method should be applied
        /// </summary>
        public ELeaderboardSortMethod sortMethod;
        /// <summary>
        /// If creating a board what display type is it
        /// </summary>
        public ELeaderboardDisplayType displayType;
        /// <summary>
        /// What is the name of the board ... if this is not to be created at run time then this must match the name as it appears in Steamworks
        /// </summary>
        public string leaderboardName;
        /// <summary>
        /// How many detail entries should be allowed on entries from this board
        /// </summary>
        public int maxDetailEntries = 0;
        /// <summary>
        /// What is the leaderboard ID ... this is nullable if null then no leaderboard has been connected
        /// </summary>
        [HideInInspector]
        public SteamLeaderboard_t? leaderboardId;

        /// <summary>
        /// Returns true when the leaderboard Id has been set.
        /// This assumes the id was set correctly.
        /// </summary>
        public bool IsReady => leaderboardId.HasValue;
        
        private CallResult<LeaderboardFindResult_t> OnLeaderboardFindResultCallResult;
        private CallResult<LeaderboardScoreUploaded_t> OnLeaderboardScoreUploadedCallResult;

        /// <summary>
        /// Registers the board on Steamworks creating if configured to do so or locating if not.
        /// </summary>
        /// <returns>True if the command was ran, false if the board has already been registerd.</returns>
        public bool Register()
        {
            //If this is null then reegister and update the board id, else we have already done this and shouldn't do it again.
            if (OnLeaderboardFindResultCallResult == null)
            {
                OnLeaderboardFindResultCallResult = CallResult<LeaderboardFindResult_t>.Create();
                OnLeaderboardScoreUploadedCallResult = CallResult<LeaderboardScoreUploaded_t>.Create();

                if (createIfMissing)
                    FindOrCreateLeaderboard(sortMethod, displayType);
                else
                    FindLeaderboard();

                return true;
            }
            else
                return false;
        }

        private void FindOrCreateLeaderboard(ELeaderboardSortMethod sortMethod, ELeaderboardDisplayType displayType)
        {
            var handle = SteamUserStats.FindOrCreateLeaderboard(leaderboardName, sortMethod, displayType);
            OnLeaderboardFindResultCallResult.Set(handle, OnLeaderboardFindResult);
        }

        private void FindLeaderboard()
        {
            var handle = SteamUserStats.FindLeaderboard(leaderboardName);
            OnLeaderboardFindResultCallResult.Set(handle, OnLeaderboardFindResult);
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        public void UploadScore(int score, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded_t, bool> callback = null)
        {
            if (!leaderboardId.HasValue)
            {
                Debug.LogError(name + " Leaderboard Data Object, cannot upload scores, the leaderboard has not been initalized and cannot upload scores.");
                return;
            }

            var handle = SteamUserStats.UploadLeaderboardScore(leaderboardId.Value, method, score, null, 0);

            if (callback == null)
                OnLeaderboardScoreUploadedCallResult.Set(handle, OnLeaderboardScoreUploaded);
            else
                OnLeaderboardScoreUploadedCallResult.Set(handle, callback.Invoke);
        }

        /// <summary>
        /// Uploads a score for the player to this board
        /// </summary>
        /// <param name="score"></param>
        /// <param name="method"></param>
        public void UploadScore(int score, int[] scoreDetails, ELeaderboardUploadScoreMethod method, Action<LeaderboardScoreUploaded_t, bool> callback = null)
        {
            if (!leaderboardId.HasValue)
            {
                Debug.LogError(name + " Leaderboard Data Object, cannot upload scores, the leaderboard has not been initalized and cannot upload scores.");
                return;
            }

            var handle = SteamUserStats.UploadLeaderboardScore(leaderboardId.Value, method, score, scoreDetails, scoreDetails.Length);

            if (callback == null)
                OnLeaderboardScoreUploadedCallResult.Set(handle, OnLeaderboardScoreUploaded);
            else
                OnLeaderboardScoreUploadedCallResult.Set(handle, callback.Invoke);
        }

        private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t param, bool bIOFailure)
        {
            if (param.m_bSuccess == 0 || bIOFailure)
                Debug.LogError(name + " Leaderboard Data Object, failed to upload score to Steamworks: Success code = " + param.m_bSuccess, this);
        }

        private void OnLeaderboardFindResult(LeaderboardFindResult_t param, bool bIOFailure)
        {
            if (param.m_bLeaderboardFound == 0 || bIOFailure)
            {
                leaderboardId = null;
                Debug.LogError("Failed to find leaderboard", this);
                return;
            }

            if (param.m_bLeaderboardFound != 0)
            {
                leaderboardId = param.m_hSteamLeaderboard;
            }
        }
    }
}
#endif
