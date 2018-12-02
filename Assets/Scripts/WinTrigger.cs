using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		if (other.transform.name == "Possessions") {
			StartCoroutine(GameObject.Find("Game").GetComponent<Game>().WinLevel());
		}
 	}
}
