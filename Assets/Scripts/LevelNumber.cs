using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelNumber : MonoBehaviour {

	void Start() {
		int levelNum = SceneManager.GetActiveScene().buildIndex;
		GetComponent<Text>().text = "LEVEL " + levelNum;
	}
}
