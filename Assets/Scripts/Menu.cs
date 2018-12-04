using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	void Awake() {
        Screen.SetResolution (Screen.currentResolution.width, Screen.currentResolution.height, true);
	}

	void Update () {
		if (Input.GetKeyDown("escape")) {
			Debug.Log("quit");
			Application.Quit();
		}
		
        if (Input.anyKeyDown) {
			int sceneIndex = SceneManager.GetActiveScene().buildIndex;
			SceneManager.LoadScene(sceneIndex + 1, LoadSceneMode.Single);
        }
	}
}
