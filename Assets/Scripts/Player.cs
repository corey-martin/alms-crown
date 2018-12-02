using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Moveable {

	[HideInInspector] public Vector3 facing = Vector3.down;
	Vector3 direction = Vector3.right;

	public GameObject modelStand;
	public GameObject modelStandPull;
	bool waitedAFrame = false;

	AudioSource audioSource;
	public AudioClip moveSound;

	public static bool isHoldingPossessions = false;

	bool canMove = true;

	void Awake() {
		audioSource = GetComponent<AudioSource>();
	}

	new void Start() {
		base.Start();
		StartCoroutine(WaitAFrame());
	}

	IEnumerator WaitAFrame() {
		yield return new WaitForEndOfFrame();
		waitedAFrame = true;
		modelStand.SetActive(true);
		modelStandPull.SetActive(false);
	}
	
	void Update () {

		//Debug.Log("isHoldingPossessions " + isHoldingPossessions);

		if (!isMoving && !Game.freeze && !isFalling && canMove) {

			// PICK UP / PUT DOWN POSSESSIONS

        	if (Input.GetKeyDown("space")) {
				
				Vector3 pos = transform.position + facing;
				bool foundPossessions = false;
        		for (int i = 0; i < 2; i++) {
					Collider[] hits = Physics.OverlapSphere(pos, 0.01f, layerMask);

					foreach (Collider col in hits) {
						if (col.tag == "Possessions") {
							if (isHoldingPossessions) {
								isHoldingPossessions = false;
								col.transform.SetParent(level);
								StartCoroutine(col.transform.GetComponent<Moveable>().Fall());
							} else {
								isHoldingPossessions = true;
								col.transform.SetParent(modelParent);
								if (i == 1) {
									col.transform.position = transform.position + facing;
								}
							}
							foundPossessions = true;
							StartCoroutine(StackAfterFall());
						}
					}

					if (foundPossessions) {
						break;
					}

					pos += Vector3.forward;
        		}
            } else {

				// MOVEMENT

				float hor = Input.GetAxisRaw("Horizontal");
				float ver = Input.GetAxisRaw("Vertical");

				if (hor == 1) {
					direction = Vector3.right;
				} else if (hor == -1) { 
					direction = Vector3.left;
				} else if (ver == -1) {
					direction = Vector3.down;
				} else if (ver == 1) {
					direction = Vector3.up;
				} else {
					direction = Vector3.zero;
				}

				if (direction != Vector3.zero) {

					Vector3 originalFacing = facing;
					Vector3 originalAngles = modelParent.localEulerAngles;
					
					if (hor == 1) {
						facing = Vector3.right;
						modelParent.localEulerAngles = new Vector3 (0,0,90); 
					} else if (hor == -1) {
						facing = Vector3.left;
						modelParent.localEulerAngles = new Vector3 (0,0,-90); 
					} else if (ver == -1) {
						facing = Vector3.down;
						modelParent.localEulerAngles = new Vector3 (0,0,0); 
					} else if (ver == 1) {
						facing = Vector3.up;
						modelParent.localEulerAngles = new Vector3 (0,0,180); 
					}

					// check if turning would lead to a tile clipping a wall and revert if so
					bool canTurn = true;
					foreach (Transform child in modelParent.transform) {
						if (child.gameObject.tag == "Tile" || child.gameObject.tag == "Possessions") {
							Vector3 pos = child.position;
							Collider[] hits = Physics.OverlapSphere(pos, 0.01f, LayerMask.GetMask("Default"));
							foreach (Collider col in hits) {
								if (col.tag == "Wall") {
									facing = originalFacing;
									modelParent.localEulerAngles = originalAngles;
									canTurn = false;
								}
							}
						}
					}

					if (canTurn && CanMove(direction)) {
						AddIt(direction);
						if (moveSound != null) {
							audioSource.pitch = Random.Range(.8f, 1f);
							audioSource.clip = moveSound;
							audioSource.Play();
						}

						if (objsToMove.Count > 0) {
							StartCoroutine(MoveIt(direction));
						}
					} else {
						objsToMove.Clear();
						StartCoroutine(CantMove(direction));
					}
				}
            }
		}

		// GRAPHICS

		if (waitedAFrame) {
			if (isHoldingPossessions) {
				modelStand.SetActive(false);
				modelStandPull.SetActive(true);
			} else {
				modelStand.SetActive(true);
				modelStandPull.SetActive(false);
			}
		}
	}

	IEnumerator StackAfterFall() {
		canMove = false;
		foreach (Moveable m in game.moveables) {
			while (m.isFalling) {
				yield return null;
			}
		}
		//Debug.Log("added to stack in StackAfterFall()");
		game.AddToUndoStack();
		canMove = true;
	}

	IEnumerator CantMove(Vector3 dir) {
		canMove = false;
		if (dir == Vector3.up) {
			dir = Vector3.right;
		} else if (dir == Vector3.down) {
			dir = Vector3.left;
		} else if (dir == Vector3.left) {
			dir = Vector3.up;
		} else if (dir == Vector3.right) {
			dir = Vector3.down;
		}
		Vector3 playerRotation = modelParent.eulerAngles;
		modelParent.eulerAngles = playerRotation + (dir * Random.Range(3f, 6f)) + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-2f, 2f));

		float t = 0;
		bool interrupted = false;
		while (t < 0.2f && !interrupted) {
			t += Time.deltaTime;
			interrupted = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
			yield return null;
		}

		modelParent.eulerAngles = playerRotation;
		undoRotations[undoRotations.Count - 1] = playerRotation;
		canMove = true;
	}

	public void UpdateFacing() {
		if (Roughly(modelParent.localEulerAngles.z, 90)) {
			facing = Vector3.right;
		} else if (Roughly(modelParent.localEulerAngles.z, -90) || Roughly(modelParent.localEulerAngles.z, 270)) {
			facing = Vector3.left;
		} else if (Roughly(modelParent.localEulerAngles.z, 0)) {
			facing = Vector3.down;
		} else if (Roughly(modelParent.localEulerAngles.z, 180)) {
			facing = Vector3.up;
		} else {
			Debug.Log("uh oh not facing anywhere? modelParent.localEulerAngles.z is " + modelParent.localEulerAngles.z);
		}
	}

	bool Roughly(float one, float two) {
		return (one > (two - 1) && one < (two + 1));
	}
}
