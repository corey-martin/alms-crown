using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour {

	public static Game game;
	public static Transform level;
	public static float moveSpeed = 6f;
	public static bool isMoving = false;
	Vector3 startPos;
	[HideInInspector] public Vector3 endPos;

	Vector3 initialPos;
	Vector3 initialRot;
	[HideInInspector] public List<Vector3> undoPositions = new List<Vector3>();
	[HideInInspector] public List<Vector3> undoRotations = new List<Vector3>();
	[HideInInspector] public List<bool> parentedStack = new List<bool>();
	public Transform modelParent;

	public static List<Moveable> objsToMove = new List<Moveable>();

	[HideInInspector] public bool isFalling = false;
	bool step = false;

	public LayerMask layerMask;

	public void Start() {
		game = GameObject.Find("Game").GetComponent<Game>();
		if (level == null) {
			level = GameObject.Find("Level").transform;
		}
		initialPos = transform.localPosition;
		if (modelParent != null) {
			initialRot = modelParent.localEulerAngles;
		} else {
			initialRot = transform.localEulerAngles;
		}
		AddToUndoStack();
		isMoving = false;
	}

	public void Update() {
	//	Debug.Log(transform.name + " posCount " + undoPositions.Count + " rotCount " + undoRotations.Count + " parentedCount " + parentedStack.Count);
	}

	public void AddToUndoStack() {
		if (modelParent != null) {
			undoRotations.Add(modelParent.localEulerAngles);
		} else {
			undoRotations.Add(transform.localEulerAngles);
			parentedStack.Add(Player.isHoldingPossessions);
		}
		undoPositions.Add(transform.localPosition);
	}

	public void DoUndo() {
		if (undoPositions.Count > 1) {
			if (modelParent != null) {
				undoRotations.RemoveAt(undoRotations.Count - 1);
				modelParent.localEulerAngles = undoRotations[undoRotations.Count - 1];
			} else {
				undoRotations.RemoveAt(undoRotations.Count - 1);
				transform.localEulerAngles = undoRotations[undoRotations.Count - 1];

				parentedStack.RemoveAt(parentedStack.Count - 1);
				if (parentedStack[parentedStack.Count - 1]) {
					Player.isHoldingPossessions = true;
					transform.SetParent(GameObject.Find("Player").GetComponent<Player>().modelParent);
				} else {
					Player.isHoldingPossessions = false;
					transform.SetParent(level);
				}
			}
			undoPositions.RemoveAt(undoPositions.Count - 1);
			transform.localPosition = undoPositions[undoPositions.Count - 1];
		}
	}

	public void DoRestart() {
		if (modelParent != null) {
			modelParent.localEulerAngles = initialRot;
		} else {
			transform.localEulerAngles = initialRot;
			Player.isHoldingPossessions = false;
			transform.SetParent(level);
		}
		transform.localPosition = initialPos;
		AddToUndoStack();
	}

	public bool CanMove(Vector3 dir) {

		if (modelParent != null) {
			// check for step
			step = false;
			Vector3 pos = transform.position + dir + Vector3.forward;
			Collider[] hits = Physics.OverlapSphere(pos, 0.01f, layerMask);
			foreach (Collider col in hits) {
				if (col.tag == "Wall" || col.transform.name == "Possessions") {
					step = true;
				}
			}

			pos = transform.position + dir;

			if (step) {
				for (int i = 0; i < 3; i++) {
					if (!Player.isHoldingPossessions && i == 2) {
						continue;
					}
					if (i == 2) {
						hits = Physics.OverlapSphere(pos, 0.01f, LayerMask.GetMask("Default"));
					} else {
						hits = Physics.OverlapSphere(pos, 0.01f, layerMask);
					}
					foreach (Collider col in hits) {
						if (col.tag == "Wall") {
							return false;
						}
						if (i == 0 && !Player.isHoldingPossessions && col.transform.name == "Possessions") {
							return false;
						} 
					}
					if (i == 0) {
						pos += Vector3.back;
					} else {
						pos += dir;
					}
				}
			} else {
				for (int i = 0; i < 2; i++) {
					if (!Player.isHoldingPossessions && i == 1) {
						continue;
					}
					if (i == 1) {
						hits = Physics.OverlapSphere(pos, 0.01f, LayerMask.GetMask("Default"));
					} else {
						hits = Physics.OverlapSphere(pos, 0.01f, layerMask);
					}
					foreach (Collider col in hits) {
						if (col.tag == "Wall") {
							return false;
						}
					}
					pos += dir;
				}
			}
		} else {
			Vector3 pos = transform.position + dir;
			Collider[] hits = Physics.OverlapSphere(pos, 0.01f, layerMask);
			
			foreach (Collider col in hits) {
				if (col.tag == "Wall") {
					return false;
				}
			}
		}

		return true;
	}

	public void AddIt(Vector3 dir) {
		startPos = transform.position;
		endPos = startPos + dir;
		if (step) {
			endPos += Vector3.back;
		}
		if (!objsToMove.Contains(this)) {
			objsToMove.Add(this);
		}
	}

	public IEnumerator MoveIt(Vector3 dir) {
		isMoving = true;

		Vector3 playerRotation = Vector3.zero;
		Vector3 possessionRotation = Vector3.zero;

		if (gameObject.tag == "Player") {
			playerRotation = modelParent.localEulerAngles;
			modelParent.localEulerAngles = playerRotation + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
		} else {
			possessionRotation = transform.localEulerAngles;
		}

		float t = 0;

		while (t < 1f) {
			t += Time.deltaTime * moveSpeed;
			foreach (Moveable m in objsToMove) {
				m.transform.position = Vector3.Lerp(m.startPos, m.endPos, t);
			}
			yield return null;
		}

		// fall stuff
		bool falling = true;
		while (falling) {
			foreach (Moveable m in game.moveables) {
				StartCoroutine(m.Fall());
			}
			yield return new WaitForEndOfFrame();
			falling = false;
			foreach (Moveable m in game.moveables) {
				while (m.isFalling) {
					falling = true;
					yield return null;
				}
			}
		}
		// end fall stuff

		if (gameObject.tag == "Player") {
			modelParent.localEulerAngles = playerRotation;
		} else {
			transform.localEulerAngles = possessionRotation;
		}

		transform.position = new Vector3 (Mathf.Round(endPos.x), Mathf.Round(endPos.y), Mathf.Round(endPos.z));
		//Debug.Log("stacked in MoveIt()");
		game.AddToUndoStack();

		objsToMove.Clear();

		isMoving = false;
	}

	public IEnumerator Fall(bool animate = true) {

		bool shouldFall = true;
		isFalling = true;

		while (shouldFall && transform.position.z < 8) {

			bool possessionsAreGrounded = false;

			if (modelParent != null) {
				foreach (Transform child in modelParent.transform) {
					if (child.gameObject.tag == "Tile") {
						if (child.name == "Possessions" && GroundBelow(child)) {
							possessionsAreGrounded = true;
							continue;
						}
						if (GroundBelow(child)) {
							shouldFall = false;
						}
					}
				}
			} else {
				if (!Player.isHoldingPossessions) {
					if (GroundBelow(transform)) {
						shouldFall = false;
					}
				} else {
					shouldFall = false;
				}
			}

			if (shouldFall && possessionsAreGrounded) {
				Player.isHoldingPossessions = false;
				Transform p = GameObject.Find("Possessions").transform;
				p.SetParent(level);
				p.localEulerAngles = Vector3.zero;
			}

			if (shouldFall) {
				if (animate) {
					startPos = transform.position;
					endPos = startPos + Vector3.forward;
					float t = 0;

					while (t < 1f) {
						int multiplier = 20;
						t += Time.deltaTime * multiplier;
						transform.position = Vector3.Lerp(startPos, endPos, t);
						yield return null;
					}
				} else {
					transform.position = transform.position + Vector3.forward;
				}
			}
		}
		isFalling = false;
	}

	bool GroundBelow(Transform point) {
		Vector3 pos = point.position + Vector3.forward;
		Collider[] hits = Physics.OverlapSphere(pos, 0.01f, LayerMask.GetMask("Default"));
		foreach (Collider col in hits) {
			if (col.tag == "Tile" && col.transform.name == "Possessions" || col.tag == "Wall") {
				return true;
			}
		}
		return false;
	}
}
