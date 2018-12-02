using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour {

	[HideInInspector] public Moveable[] moveables;

	public Player player;
	public GameObject successText;

	public static bool freeze = false;
	public AudioSource undoSound;
	bool holdingUndo = false;

	public static int sceneIndex;
	public static string roomNumber;

	public static Transform blackScreen;

	void Start() {
		if (player == null) {
			player = transform.root.GetComponent<Player>();
		}
		moveables = FindObjectsOfType<Moveable>();

		sceneIndex = SceneManager.GetActiveScene().buildIndex;
		PlayerPrefs.SetInt("saveIndex", sceneIndex);
		freeze = false;

        Application.targetFrameRate = 60;
        Cursor.visible = false;
	}

	public IEnumerator WinLevel() {
		freeze = true;
		StartCoroutine(ZoomOut());
		Player.isHoldingPossessions = false;
		yield return new WaitForSeconds(5);
		freeze = false;
		SceneManager.LoadScene(sceneIndex + 1, LoadSceneMode.Single);
	}

	IEnumerator ZoomOut() {
		Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		float size = cam.orthographicSize;
		while (size < 15) {
			size += Time.deltaTime * 0.1f;
			cam.orthographicSize = size;
			yield return null;
		}
	}

	void Update() {
		if (Input.GetKeyDown("escape")) {
			Debug.Log("quit");
			Application.Quit();
		}

		if (!Moveable.isMoving && !freeze) {
			if (Input.GetButtonDown("Undo")) {
				DoUndo();
				StartCoroutine(UndoRepeat());
			}

			if (Input.GetButtonDown("Restart")) {
				if (undoSound != null) {
					undoSound.pitch = Random.Range(.5f, .5f);
					undoSound.Play();
				}
				foreach (Moveable m in moveables) {
					m.DoRestart();
				}
				player.UpdateFacing();
			}
		}

		if (Input.GetButtonUp("Undo")) {
			holdingUndo = false;
		}

		if (Input.GetKeyDown("+") || Input.GetKeyDown("=")) {
			freeze = false;
			Player.isHoldingPossessions = false;
			SceneManager.LoadScene(sceneIndex + 1, LoadSceneMode.Single);
		}
		if (Input.GetKeyDown("-")) {
			freeze = false;
			Player.isHoldingPossessions = false;
			SceneManager.LoadScene(sceneIndex - 1, LoadSceneMode.Single);
		}
	}

	public void AddToUndoStack() {
		foreach (Moveable m in moveables) {
			m.AddToUndoStack();
		}
	}

	void DoUndo() {
		if (player.undoPositions.Count > 1) {
			if (undoSound != null) {
				undoSound.pitch = Random.Range(.9f, 1f);
				undoSound.Play();
			}
			foreach (Moveable m in moveables) {
				m.DoUndo();
			}
			player.UpdateFacing();
		}
	}

	IEnumerator UndoRepeat() {
		holdingUndo = true;
		float t = 0;
		while (t < 0.75f && holdingUndo) {
			t += Time.deltaTime;
			yield return null;
		}
		while (Input.GetButton("Undo") && holdingUndo && !Moveable.isMoving && player.undoPositions.Count > 0) {
			DoUndo();
			yield return new WaitForSeconds(0.075f);
		}
	}

	public void PreviousLevel() {
		freeze = false;
		SceneManager.LoadScene(sceneIndex - 1, LoadSceneMode.Single);
	}
	public void NextLevelNow() {
		freeze = false;
		SceneManager.LoadScene(sceneIndex + 1, LoadSceneMode.Single);
	}
}
