using UnityEngine;
using System.Collections;
using Steamworks;

// Enum for possible game states on the client
enum EClientGameState {
	k_EClientGameActive,
	k_EClientGameWinner,
	k_EClientGameLoser,
};

class SpaceWarClient : MonoBehaviour {
	SteamStatsAndAchievements m_StatsAndAchievements;

	private void OnEnable() {
		m_StatsAndAchievements = GameObject.FindObjectOfType<SteamStatsAndAchievements>();

		m_StatsAndAchievements.OnGameStateChange(EClientGameState.k_EClientGameActive);
	}

	private void OnGUI() {
		m_StatsAndAchievements.Render();
		GUILayout.Space(10);

		if(GUILayout.Button("Set State to Active")) {
			m_StatsAndAchievements.OnGameStateChange(EClientGameState.k_EClientGameActive);
		}
		if (GUILayout.Button("Set State to Winner")) {
			m_StatsAndAchievements.OnGameStateChange(EClientGameState.k_EClientGameWinner);
		}
		if (GUILayout.Button("Set State to Loser")) {
			m_StatsAndAchievements.OnGameStateChange(EClientGameState.k_EClientGameLoser);
		}
		if (GUILayout.Button("Add Distance Traveled +100")) {
			m_StatsAndAchievements.AddDistanceTraveled(100.0f);
		}
	}
}
