using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{
	public GameObject parentCamera;	// parent of the prefab clones
	public GameObject prefab;	
	
	// Grid in fo
	public int columnLength;
	public int rowLength;

	public float x_Start, y_Start, x_Space, y_Space;	// positioning
	
	private GameObject[] objects;	// For manipulating the objects in the view
	private Material[] materials;	// For choosing materials at random for the prefabs
	
	// Reference to the materials
	public Material material0, material1, material2, material3;

	// This queue is important for deletion of the prefabs
	private Queue<GameObject>[] qObjects;
    private HashSet<GameObject> disposables;

	// Count of how many prefabs are deleted in the i-th columns
	private int[] newInColumn;

	// speed of dropping of the prefabs
	public float speed;

	[SerializeField]
	private int minimumAdjacent;
	private int currentAdjacentCount;
    private bool sentScore;

	private float height, width; // Camera

    private bool isInputLocked;
    private bool[] visited;
    private int taskStatus;
    Dictionary<GameObject, Vector3> objectCurrentPosition;
    Dictionary<GameObject, Vector3> objectNewPosition;

    private Vector3[] spawnLocation;

    private FirebaseManager firebaseManager;

    void Start()
    {
    	// ---------Initialization of the variables --------------
    	objects = new GameObject[columnLength * rowLength];
    	qObjects = new Queue<GameObject>[columnLength];
    	newInColumn = new int[columnLength];
        visited = new bool[columnLength * rowLength];
        spawnLocation = new Vector3[columnLength];

        objectCurrentPosition = new Dictionary<GameObject, Vector3>();
        objectNewPosition = new Dictionary<GameObject, Vector3>();


    	height = Camera.main.orthographicSize * 2;
 		width = height * Screen.width/ Screen.height; // basically height * screen aspect ratio

    	for(int i = 0; i < qObjects.Length; ++i) {
    		qObjects[i] = new Queue<GameObject>();
    	}

        firebaseManager = FindObjectOfType<FirebaseManager>();

    	// ----- End of Initialization --------

    	SetUpMaterialsArray();

    	// Initial cloning of the prefabs.
        Init();
    }

    /*
     * @Usage:
	 * 	Instantiates the inital prefab clones to the screen
	 * 	Sets up properties of the newly created clones
	 *
     */
    private void Init() {

    	float height = Camera.main.orthographicSize * 2;
 		float width = height * Screen.width/ Screen.height; // basically height * screen aspect ratio

        for(int i = 0; i < columnLength; ++i) {
            float xPos = x_Start + (x_Space * (i%columnLength));
            float yPos = y_Start + (-y_Space * (i / columnLength));
            yPos += y_Space;

            spawnLocation[i] = new Vector3(xPos, yPos);
        }

    	for(int i = 0; i < columnLength * rowLength; ++i) {
    		float xPos = x_Start + (x_Space * (i%columnLength));
    		float yPos = y_Start + (-y_Space * (i / columnLength));

    		objects[i] = Instantiate(prefab) as GameObject;

    		/*
    		 * Properties
    		 */

    	//	objects[i].transform.localScale = Vector3.one * height / 15f;
    //		objects[i].transform.parent = parentCamera.transform;				// parent of the clone
    		objects[i].transform.localPosition = new Vector3(xPos, yPos);		// localPosition of the clone

    		int materialIndex = UnityEngine.Random.Range(0,4);					

    		objects[i].GetComponent<Renderer>().material = materials[materialIndex];	// Randomly chosen material for the clone

    		objects[i].gameObject.tag = materialIndex.ToString();						// Give a tag to the clone for determining its material

    		qObjects[i%columnLength].Enqueue(objects[i]);								// push a reference to the gameObject to the corresponding queue

            objectNewPosition.Add(objects[i] as GameObject, objects[i].transform.localPosition);
            objectCurrentPosition.Add(objects[i] as GameObject, objects[i].transform.localPosition);
    	}
    }

    /*
     *
     *	In every frame check
     *		- if any prefab is clicked or not.
     *		- if clicked, do the required operation.
     *
     **/

    void Update()
    {
        if(firebaseManager.updated) {
            if( !isInputLocked) {
                sentScore = false;                                                                    
                currentAdjacentCount = 0;

                for(int i = 0; i < columnLength * rowLength; ++i) {
                    PrefabManager prefabManager = objects[i].GetComponent<PrefabManager>();

                    if(prefabManager.isClicked) {

                        visited = new bool[columnLength * rowLength];
                        disposables = new HashSet<GameObject>();
                        for(int j = 0; j < columnLength; ++j)
                            newInColumn[j] = 0;

                        Propagate(i);
                    }
                }
            } else {                // Need this frame to update the UI. can't take input
                if(taskStatus == 0) {
                    print("Disposing objects");
                    DisposeObjects();
                    AudioManagerScript.PlayScoreSound();
                    
                } else if(taskStatus == 1) {

                    if(taskOneClear()) {
                        taskStatus++;
                    } else {
                        for(int i = 0; i < columnLength * rowLength; ++i) {

                            if(objectCurrentPosition[objects[i]] != objectNewPosition[objects[i]]) {
                                MoveObject(objects[i] , objectNewPosition[objects[i]]);
                            }
                        }
                    }
                } else {
                    isInputLocked = false;
                }
            }
        }
    }

    private bool taskOneClear() {
        bool ret = true;

        foreach(var d in objectNewPosition) { 
            GameObject key = d.Key as GameObject;
            ret &= (d.Value == objectCurrentPosition[key]);

        //    if(ret == false) {
        //        print(d.Value);
        //        print(objectCurrentPosition[key]);
        //        print(d.Value == objectCurrentPosition[key]);
        //    }
        } 

        return ret;
    }

    private void DisposeObjects() {

        foreach(GameObject d in disposables) {
            objectNewPosition.Remove(d);
            objectCurrentPosition.Remove(d);
            Destroy(d);
        }

        disposables.Clear();

        taskStatus++;  
    }

    /*
     *
     *	Checks if y is adjacent to x
     * 		- Retrieves row and column number from the indices
     *		- Check if they are adjacent
     *
     **/

    private bool IsAdjacent(int x, int y) {
    	int lim = columnLength * rowLength;

    	if(y < 0 || y >= lim)	return false;

    	int r1,c1,r2,c2;

    	c1 = x%columnLength ; c2 = y%columnLength;
    	r1 = x/columnLength ; r2 = y/columnLength;

    	if(c1 == c2 && Math.Abs(r1-r2) == 1)
    		return true;
    	else if(r1 == r2 && Math.Abs(c1-c2) == 1)
    		return true;

    	return false;
    }

    private int VisitedCount(bool[] visited) {
    	int cnt = 0;
    	for(int i = 0;i < columnLength*rowLength; ++i) {
    		cnt += (visited[i] == true ? 1 : 0);
    	}

    	return cnt;
    }

    private void Propagate(int clickedIndex) {
    	
    	visited = GetCommonAdjacent(clickedIndex);

    	int adjacentCount = VisitedCount(visited);

    	if(adjacentCount >= minimumAdjacent) {
            isInputLocked = true;
            taskStatus = 0;
    		currentAdjacentCount = adjacentCount;
            print("Adjacent detected...");
            UpdateObjectList(visited);
    	}
    }

    /*
	 * @Function Description:
	 * 	• A BFS algorithm that iterates over the current grid and advances if adjacent prefab has same material
	 * 	• materials of two prefab are compared by comparing their tags
	 *
     */

    private bool[] GetCommonAdjacent(int clickedIndex) {

    	Queue < int > q = new Queue<int>();
    	int[] dx = new int[] {-1,+1,-columnLength,+columnLength};
    	bool[] visited = new bool[columnLength * rowLength];

    	q.Enqueue(clickedIndex);
    	visited[clickedIndex] = true;

    	while(q.Count > 0) {
    		int currentPosition = q.Peek();	q.Dequeue();

    		for(int i = 0; i < 4; ++i){
    			int nextPosition = currentPosition + dx[i];

    			if(!IsAdjacent(currentPosition, nextPosition))	continue;

    			if(visited[nextPosition])	continue;

    			string currentMaterial = objects[currentPosition].gameObject.tag;
    			string nextMaterial = objects[nextPosition].gameObject.tag;
    			

    			if(currentMaterial == nextMaterial) {
    				q.Enqueue(nextPosition);
    				visited[nextPosition] = true;
    			}
    		}
    	}

    	return visited;
    }

    /*
     *
     * Procedure by which 
     * 		Firstly, visited clones are destroyed.
     *		Then, new clones are added
     *		At last, their positions are organized 
     *		Updates the array for future reference.
     *
     **/
    private void UpdateObjectList(bool[] visited) {

        RemoveClickedObjects(visited);

    	PushNewObjects();

    	RotateQueue();			// put the newly created prefabs in the front accordingly, since new clones are always on the top of the grid

    	UpdatePositions();		// get the updated positions for the new clones as well as the clones that have changed position

    	UpdateArray();			// get the updated information from the queue to the array

    }

    private void RemoveClickedObjects(bool[] visited) {
    	
    	for(int i = 0; i < columnLength; ++i) {
    		for(int j = 0; j < rowLength; ++j) {
    			int idx = j*columnLength + i;

    			if(visited[idx]) {
    				GameObject dObject = qObjects[i].Peek();
	    			qObjects[i].Dequeue();
	    	//		print("Destroying...");
	    	//		Destroy(dObject);
                    print("Adding to the disposable set: " + dObject);
                    disposables.Add(dObject as GameObject);
	    			continue;
    			}
    			GameObject topObject = qObjects[i].Peek();	qObjects[i].Dequeue();
    			qObjects[i].Enqueue(topObject);
    		}
    	}    	

        print("Disposable objects: " + disposables.Count);
    }

    private void PushNewObjects() {

    	for(int i = 0; i < columnLength; ++i) {
    		
    		while(qObjects[i].Count < rowLength) {
    			GameObject _newObject = Instantiate(prefab) as GameObject;
		    	
		    	_newObject.transform.parent = parentCamera.transform;
                _newObject.transform.localPosition = spawnLocation[i%columnLength];
		    //	_newObject.transform.localScale = Vector3.one * height / 15f;
		    	int materialIndex = UnityEngine.Random.Range(0,4);
		    	_newObject.GetComponent<Renderer>().material = materials[materialIndex];
		    	_newObject.gameObject.tag = materialIndex.ToString();
		    	qObjects[i].Enqueue(_newObject);
		    	newInColumn[i]++;
    		}
    	}
    }

    private void RotateQueue() {
    	for(int c = 0; c < columnLength; ++c) {
    		for(int i = 0; i < rowLength-newInColumn[c]; ++i) {
	    		GameObject topObject = qObjects[c].Peek();	qObjects[c].Dequeue();
	    		qObjects[c].Enqueue(topObject);
	    	}
    	}
    }

    private void UpdatePositions() {

    	for(int c = 0; c < columnLength; ++c) {
    		for(int i = 0; i < rowLength; ++i) {

	    		GameObject topObject = qObjects[c].Peek();	qObjects[c].Dequeue();

	    		// move position
	    		Vector3 _from = topObject.transform.localPosition;
	    		Vector3 to = objects[i * columnLength + c].transform.localPosition;
	    		
                if(objectNewPosition.ContainsKey(topObject)) {
                    objectNewPosition[topObject] = to;
                } else {
                    objectNewPosition.Add(topObject, to);
                    objectCurrentPosition.Add(topObject, topObject.transform.localPosition);
                }

	    		// push it to the queue after positioning it correctly
	    		qObjects[c].Enqueue(topObject);
	    	}
    	}
    }

    private void MoveObject(GameObject _object, Vector3 to) {

        Vector3 epsVector = new Vector3(0,1,0);
        Vector3 _from = _object.transform.localPosition;
        Vector3 displacement = to - _from;
        Vector3 direction = displacement.normalized;
        print(displacement.x+" "+displacement.y+" "+displacement.z);
        if(direction != Vector3.down) {
            _object.transform.localPosition = to;
            objectCurrentPosition[_object] = _object.transform.localPosition;
            return;
        }
        Vector3 velocity = direction * speed;

        _object.transform.Translate(velocity * Time.deltaTime);

        objectCurrentPosition[_object] = _object.transform.localPosition;

     //   yield return null;
    }

    /*
     *
     * Copies the queue information to the array
     *
     **/
    private void UpdateArray() {
    	
    	for(int c = 0; c < columnLength; ++c) {
    		for(int i = 0; i < rowLength; ++i) {
	    		GameObject topObject = qObjects[c].Peek();	qObjects[c].Dequeue();

	    		objects[i * columnLength + c] = topObject;

	    		qObjects[c].Enqueue(topObject);
	    	}
    	}
    }

    void SetUpMaterialsArray() {
    	materials = new Material[4];
    	materials[0] = material0;
    	materials[1] = material1;
    	materials[2] = material2;
    	materials[3] = material3;
    }

    // getter
    public int GetCurrentAdjacentCount() {
        if(!sentScore) {
            sentScore = true;
        	return currentAdjacentCount;
        }
        return 0;
    }
}
