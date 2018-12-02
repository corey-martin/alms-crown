using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class End : MonoBehaviour {

	void Update () {
		if (Input.GetKeyDown("escape")) {
			Debug.Log("quit");
			Application.Quit();
		}
	}
}
