#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class Level : EditorWindow {

   	int selGridInt = 0;
    string[] selectStrings = new string[] {
    	"None", "Empty", "Player", "Possessions", "Block A", "Block B"
	};

	int rotateInt = 0;
    string[] rotateStrings = new string[] {
    	"0", "90", "180", "270"
	};

	bool foldoutPrefabs = false;

	public GameObject playerPrefab;
	public GameObject possessionsPrefab;
	public GameObject blockAPrefab;
	public GameObject blockBPrefab;

    bool isHoldingAlt = false;

	[MenuItem("Window/Level Builder")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(Level));
	}

	void OnGUI() {
		GUILayout.Label ("Selected GameObject:", EditorStyles.boldLabel);
        selGridInt = GUILayout.SelectionGrid(selGridInt, selectStrings, 4, GUILayout.Width(330));

		GUILayout.Label ("GameObject Rotation (Z):", EditorStyles.boldLabel);
        rotateInt = GUILayout.SelectionGrid(rotateInt, rotateStrings, 4, GUILayout.Width(330));

		EditorGUILayout.Space();

		///////////////// ROTATION //////////////////

		GUILayout.Label ("Rotate Level:", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("90° CW", GUILayout.Width(80))) {
        	RotateLevel(90);
        }
        if (GUILayout.Button("90° CCW", GUILayout.Width(80))) {
        	RotateLevel(-90);
        }
        if (GUILayout.Button("180°", GUILayout.Width(80))) {
        	RotateLevel(180);
        }
        EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		///////////////// INVERSION //////////////////

		GUILayout.Label ("Invert Level:", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("X axis", GUILayout.Width(80))) {
        	InvertLevel("x");
        }
        if (GUILayout.Button("Y axis", GUILayout.Width(80))) {
        	InvertLevel("y");
        }
        EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		foldoutPrefabs = EditorGUILayout.Foldout(foldoutPrefabs, "Prefabs");
		if (foldoutPrefabs) {
			playerPrefab = EditorGUILayout.ObjectField("Player", playerPrefab, typeof(GameObject), false) as GameObject;
			possessionsPrefab = EditorGUILayout.ObjectField("Possessions", possessionsPrefab, typeof(GameObject), false) as GameObject;
			blockAPrefab = EditorGUILayout.ObjectField("Block A", blockAPrefab, typeof(GameObject), false) as GameObject;
			blockBPrefab = EditorGUILayout.ObjectField("Block B", blockBPrefab, typeof(GameObject), false) as GameObject;
		}
	}

	void OnEnable() {
		SceneView.onSceneGUIDelegate += SceneGUI;
	}

	public void SceneGUI(SceneView sceneView) {
		Event e = Event.current;

		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var controlID = GUIUtility.GetControlID(FocusType.Passive);
		var eventType = e.GetTypeForControl(controlID);

		//Debug.Log(selGridInt);

    	if (e.isKey && e.keyCode == KeyCode.P) {
    		EditorApplication.ExecuteMenuItem("Edit/Play");
    	}

    	if (eventType == EventType.KeyDown && e.keyCode == KeyCode.LeftAlt) {
    		isHoldingAlt = true;
    	}

    	if (eventType == EventType.KeyUp && e.keyCode == KeyCode.LeftAlt) {
    		isHoldingAlt = false;
    	}
 
		if (eventType == EventType.MouseUp && e.button == 0 && selGridInt != 0 && sceneView.in2DMode) {
			e.Use();
		}
 
		if (eventType == EventType.MouseDown && e.button == 0 && selGridInt != 0 && sceneView.in2DMode) {
			
			e.Use();  //Eat the event so it doesn't propagate through the editor.

			GameObject prefab = playerPrefab;
			string parentName = "Level";
			bool clearAtPosition = false;

			switch (selGridInt) {
				case 1:
					// "Empty", so it should clear any objects at this position
					break;
				case 2:
					prefab = playerPrefab;
					break;
				case 3:
					prefab = possessionsPrefab;
					break;
				case 4:
					prefab = blockAPrefab;
					parentName = "Blocks";
					break;
				case 5:
					prefab = blockBPrefab;
					parentName = "Blocks";
					break;
				default:
					//
					break;
			}

			Vector3 screenPosition = e.mousePosition;
			screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
			Ray ray = Camera.current.ScreenPointToRay (screenPosition);
			Vector3 point = ray.origin + ray.direction;
			point = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), 0);

			if (selGridInt == 1 || clearAtPosition) {
				ClearObjectsAtPosition(point);
			} 

			if (selGridInt != 1) {
				GameObject go  = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
				go.transform.position = point;
				go.transform.parent = GameObject.Find(parentName).transform;

				int z = 0;
				switch (rotateInt) {
					case 0:
						z = 0;
						break;
					case 1:
						z = 90;
						break;
					case 2:
						z = 180;
						break;
					case 3:
						z = 270;
						break;
				}

				go.transform.eulerAngles = new Vector3(0,0,z);
				AvoidIntersect(go.transform);

	        	Undo.RegisterCreatedObjectUndo (go, "Create object");
			}
		}
    }

    void RotateLevel(int degrees) {
    	GameObject.Find("Level").transform.eulerAngles += new Vector3 (0,0,degrees);
    	GameObject.FindWithTag("Player").transform.eulerAngles += new Vector3 (0,0,-degrees);
    }

    void InvertLevel(string axis) {
		Transform level = GameObject.Find("Level").transform;
    	foreach (Transform child in level) {
    		if (child.tag == "Player") {
    			Vector3 p = child.position;
    			if (axis == "x") {
    				child.position = new Vector3(-p.x, p.y, p.z);
				} else {
    				child.position = new Vector3(p.x, -p.y, p.z);
				}
    		} else {
    			foreach (Transform gchild in child) {
	    			Vector3 p = gchild.position;
	    			Vector3 s = gchild.localScale;
	    			if (axis == "x") {
	    				gchild.position = new Vector3(-p.x, p.y, p.z);
	    				gchild.localScale = new Vector3(-s.x, s.y, s.z);
					} else {
	    				gchild.position = new Vector3(p.x, -p.y, p.z);
	    				gchild.localScale = new Vector3(s.x, -s.y, s.z);
					}
    			}
    		}
    	}
    }

    void ClearObjectsAtPosition(Vector3 point) {
    	Debug.Log("clearing objects at position " + point);
		Transform level = GameObject.Find("Level").transform;
		foreach (Transform child in level) {
			if ((child.tag == "Player" || child.name == "Possessions") && child.position == point) {
				Undo.DestroyObjectImmediate(child.gameObject);
			} else {
				foreach (Transform gchild in child) {
					Vector3 p = new Vector3(point.x, point.y, gchild.position.z);
					if (gchild.position == p) {
						Undo.DestroyObjectImmediate(gchild.gameObject);
					}
				}
			}
		}
    }

    void AvoidIntersect(Transform obj) {
    	bool intersecting = true;
    	while (intersecting) {
    		intersecting = false;
			Collider[] hits = Physics.OverlapSphere(obj.position, 0.25f);
			foreach (Collider col in hits) {
				if ((col.tag == "Tile" || col.tag == "Wall") && col.transform != obj) {
					obj.position += new Vector3(0,0,-1);
					intersecting = true;
				}
			}
    	}
    	RoundEverything(obj);
    }

    void RoundEverything(Transform obj) {
    	Vector3 p = obj.position;
    	obj.position = new Vector3(Mathf.RoundToInt(p.x),Mathf.RoundToInt(p.y),Mathf.RoundToInt(p.z));
    	Vector3 r = obj.eulerAngles;
    	obj.eulerAngles = new Vector3(Mathf.RoundToInt(r.x),Mathf.RoundToInt(r.y),Mathf.RoundToInt(r.z));
    	Vector3 s = obj.localScale;
    	obj.localScale = new Vector3(Mathf.RoundToInt(s.x),Mathf.RoundToInt(s.y),Mathf.RoundToInt(s.z));
    }
}

#endif