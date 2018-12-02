using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowGenerator : MonoBehaviour {

	public Transform snowPrefab;

	void Awake () {
		for (int i = 0; i < 100; i++) {
			float r1 = Random.Range(-10,10);
			float r2 = Random.Range(-10,10);

            Transform snow = (Transform)Instantiate(snowPrefab, new Vector3(r1, r2, 0.45f), Quaternion.identity);

			float r3 = Random.Range(0.2f,3);
			snow.localScale = new Vector3(r3,r3,0.01f);
		}
	}
}
