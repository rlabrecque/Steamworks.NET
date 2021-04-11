using UnityEngine;
using System.Collections;

public class QuitScript : MonoBehaviour {
	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
			return;
		}
	}
}
