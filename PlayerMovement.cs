using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public delegate void ResetMovementD();
    public static event ResetMovementD OnMoveInterrupt;

    public GameObject meshTextPrefab;


    public static PlayerMovement instance;

    public GameObject playerBody;

    public GameObject startingTile;
    public GameObject currentTile;
    public Transform targetLocation;
    
    public float distance;
    public int tileDist;
    public GameObject miniMapArrowPrefab;
    private SpriteRenderer miniMapArrowSpriteRend;
    private GameObject miniMapArrow;
    

    private bool isSetUp;
    private bool isMoving;
    public bool clicked;
    private bool notManuverable;
    private float fScore;

    //private GameObject[] cameFrom;
    //private GameObject[] openSet;
    //private List<GameObject> cameFrom;
    //private List<GameObject> openSet;
    public Dictionary<GameObject, GameObject> cameFrom = new Dictionary<GameObject, GameObject>();
    public Dictionary<GameObject, float> cumulativeG = new Dictionary<GameObject, float>();
    public GameObject[] path;

    private Dictionary<GameObject, float> openSet = new Dictionary<GameObject, float>();
    private List<GameObject> closedSet = new List<GameObject>();
    private GameObject[] neighbors = new GameObject[8];

    private bool isRunning;
    //private const float walkDelay = 30;
    //private const float runDelay = 15;
    private const float walkDelay = .3f;
    private const float runDelay = .15f;
    private float currDelay;

    public float runEnergy = 100;

    public Transform playerNorth;
    //public Transform nextLocation;

    void Start() {
        instance = this;

        //playerBody = GetComponentInChildren<GameObject>();

        targetLocation = transform;

        currDelay = walkDelay;

        miniMapArrow = Instantiate(miniMapArrowPrefab);
        miniMapArrowSpriteRend = miniMapArrow.GetComponentInChildren<SpriteRenderer>();
    }

    void Update() {
        if (!isMoving && clicked) {
            isMoving = true;
            if (FindPath()) {
                MovePath();
                //StartCoroutine("MoveAlongcameFrom");
            } else {
                ResetMovement();
            }
        }
    }
    private void FixedUpdate() {
        currDelay--;
        if (runEnergy < 100) {
            runEnergy += .1f;
        }
        if (runEnergy <= 0) {
            isRunning = false;
        }

        if (transform.position != targetLocation.position) {
            if (!isSetUp) {
                SpawnMiniMapArrow();
                SetStartingTile();

                distance = Vector3.Distance(transform.position, targetLocation.position);
                isSetUp = true;
            }
            if (currDelay <= 0) {
                //FindPath();

                if(isRunning) {
                    currDelay = runDelay;
                } else {
                    currDelay = walkDelay;
                }
                
            }
        } else {
            isSetUp = false;
            if (miniMapArrowSpriteRend.enabled) {
                miniMapArrowSpriteRend.enabled = false;
            }
        }
        if (notManuverable) {
            isSetUp = false;
            if (miniMapArrowSpriteRend.enabled) {
                miniMapArrowSpriteRend.enabled = false;
            }
        }

        if (miniMapArrow) {
            miniMapArrow.transform.eulerAngles = new Vector3(90, playerNorth.eulerAngles.y, 0);
        }
    }

    private bool FindPath() {
        if (runEnergy < 10) {
            isRunning = false;
        }
        if (isRunning) {
            runEnergy -= 10;
        }
        SetStartingTile();
        float currentGScore = Vector3.Distance(startingTile.transform.position, targetLocation.position);
        // debug
        //Debug.Log("startingTile Gscore: " + currentGScore);
        // /debug
        openSet.Add(startingTile, currentGScore);
        cumulativeG.Add(startingTile, 0);

        float neighborGScore;
        float currDistAway;
        //debug
        float currDistAway1;
        while (openSet.Count > 0) {
            // debug
            //Debug.Log("looking for closest openSet member...");
            // /debug
            SetCurrentTile();

            // debug - visualize
            /*
            GameObject tempText1 = Instantiate(meshTextPrefab, currentTile.transform.position, Quaternion.identity);
            tempText1.GetComponent<TextMesh>().color = Color.red;
            cumulativeG.TryGetValue(currentTile, out currDistAway1);
            tempText1.GetComponent<TextMesh>().text = currDistAway1.ToString();
            */
            // /debug

            if (currentTile.transform.position == targetLocation.position) {
                //Debug.Log("found target");
                //cameFrom.Add(currentTile, null);
                //if (cumulativeG.ContainsKey(currentTile)) {
                //    cumulativeG.Remove(currentTile);
                //}
                //cumulativeG.Add(currentTile, currDistAway1);
                //cameFrom.Add(null, currentTile);
                return true;
            }
            openSet.TryGetValue(currentTile, out currentGScore);
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            //Debug.Log("looking at " + currentTile);
            SetNeighbors();
            cumulativeG.TryGetValue(currentTile, out currDistAway);
            foreach (GameObject neighbor in neighbors) {
                if (neighbor == null)
                    continue;
                if (closedSet.Contains(neighbor)) {
                    //Debug.Log("skipping " + neighbor.name + " for being in closedSet");
                    continue;
                }
                neighborGScore = Vector3.Distance(neighbor.transform.position, targetLocation.position);
                if (!openSet.ContainsKey(neighbor)) {
                    //neighborGScore = Vector3.Distance(neighbor.transform.position, targetLocation.position);
                    openSet.Add(neighbor, neighborGScore);
                    cumulativeG.Add(neighbor, currDistAway + 1);
                }
                //if (currentGScore >= neighborGScore) {}
                if (currentGScore <= neighborGScore) {
                    continue;
                }
                //Debug.Log("currentGScore <= neighborGScore");
                //Debug.Log(currentGScore + " <= " + neighborGScore);
                //Debug.Log("attempt to add " + neighbor);
                /*
                if (cameFrom.ContainsKey(neighbor)) {
                    // debug
                    GameObject tempFrom;
                    cameFrom.TryGetValue(neighbor, out tempFrom);
                    Debug.Log("removing " + neighbor.name + " from " + tempFrom.name);
                    // /debug
                    cameFrom.Remove(neighbor);
                }
                */
                //cameFrom.Add(neighbor, currentTile);

                // visualize
                /*
                GameObject tempText = Instantiate(meshTextPrefab, neighbor.transform.position, Quaternion.identity);
                tempText.GetComponent<TextMesh>().text = (currDistAway+1) + " " + neighbor.name + " came from " + currentTile.name;
                */
                // /visualize

                if (cameFrom.ContainsKey(currentTile)) {
                    cameFrom.Remove(currentTile);
                }
                cameFrom.Add(currentTile, neighbor);
                neighbor.GetComponent<WalkingBehavior>().cameFrom = currentTile;
                //if (cameFrom.ContainsKey(neighbor)) {
                //    cameFrom.Remove(neighbor);
                //}
                //cameFrom.Add(neighbor, currentTile);
            }
        }
        Debug.Log("findpath failed");
        return false;
    }
    private void SetCurrentTile() {
        float lowestSoFar = float.MaxValue;
        foreach (KeyValuePair<GameObject, float> entry in openSet) {
            // debug
            //Debug.Log(entry.Key.name + "has Gscore " + entry.Value);
            // /debug
            if (entry.Value < lowestSoFar) {
                lowestSoFar = entry.Value;
                currentTile = entry.Key;
                //return true here to skip extra neighbor searches?
                // debug
                //Debug.Log("new lowest key: " + entry.Key.name);
                // /debug
            }
        }
    }
    private void MovePath() {

        float tempFloat;
        cumulativeG.TryGetValue(currentTile, out tempFloat);
        tileDist = (int)tempFloat + 1;

        path = new GameObject[tileDist];
        //Debug.Log("tiledist: " + tileDist);


        for (int i = tileDist; i > 0; i--) {
            path[i-1] = currentTile;
            currentTile = currentTile.GetComponent<WalkingBehavior>().cameFrom;
        }
        //PrintPath();
        
        StartCoroutine("MoveAlongPath");
        //Transform nextLocation;
        /*
        foreach (KeyValuePair<GameObject, GameObject> kv in cameFrom) {
            while (currDelay > 0) {
                Debug.Log("waiting for currDelay, " + currDelay);
            }
            Move(FindDirection(kv.Key.transform));
        }
        isMoving = false;
        */
    }
    public void PrintPath() {
        for (int i = 0; i < tileDist; i++) {
            Debug.Log(path[i].name);
        }
    }
    
    public IEnumerator MoveAlongPath() {
        /*
        GameObject currentTile;
        cameFrom.TryGetValue(null, out currentTile);

        while (currentTile != startingTile) {

        }
        */
        
        //Debug.Log("coroutine: starting");
        bool setUp = false;
        foreach (GameObject tile in path) {
            if (setUp == false) {
                setUp = true;
                continue;
            }
            if (isRunning) {
                yield return new WaitForSeconds(runDelay);
            } else {
                yield return new WaitForSeconds(walkDelay);
            }
            Move(FindDirection(tile.transform));
        }
        /*  using cameFrom
        foreach (KeyValuePair<GameObject, GameObject> kv in cameFrom) {
            if (setUp == false) {
                //Debug.Log("skipping first...");
                setUp = true;
                continue;
            }
            //Debug.Log("waiting");
            if (isRunning) {
                yield return new WaitForSeconds(runDelay);
            } else {
                yield return new WaitForSeconds(walkDelay);
            }
            //Debug.Log("moving");
            Move(FindDirection(kv.Key.transform));

        }
        */
        ResetMovement();
    }

    private void Move(string direction) {
        switch (direction) {
            case "left":
                transform.position += new Vector3(-1, 0, 0);
                playerBody.transform.eulerAngles = new Vector3(0, -90, 0);
                return;
            case "up":
                transform.position += new Vector3(0, 0, 1);
                playerBody.transform.eulerAngles = new Vector3(0, 0, 0);
                return;
            case "right":
                transform.position += new Vector3(1, 0, 0);
                playerBody.transform.eulerAngles = new Vector3(0, 90, 0);
                return;
            case "down":
                transform.position += new Vector3(0, 0, -1);
                playerBody.transform.eulerAngles = new Vector3(0, 180, 0);
                return;
            case "UL":
                transform.position += new Vector3(-1, 0, 1);
                playerBody.transform.eulerAngles = new Vector3(0, -45, 0);
                return;
            case "UR":
                transform.position += new Vector3(1, 0, 1);
                playerBody.transform.eulerAngles = new Vector3(0, 45, 0);
                return;
            case "DL":
                transform.position += new Vector3(-1, 0, -1);
                playerBody.transform.eulerAngles = new Vector3(0, -135, 0);
                return;
            case "DR":
                transform.position += new Vector3(1, 0, -1);
                playerBody.transform.eulerAngles = new Vector3(0, 135, 0);
                return;
            case "": return;
        }
    }

    private string FindDirection(Transform nextLocation) {
        /*
        if (nextLocation.position.z == transform.position.z) {
            if (nextLocation.position.x < transform.position.x) {
                return "left";
            } else if (nextLocation.position.x > transform.position.x) {
                return "right";
            }
        } else if (nextLocation.position.x == transform.position.x) {
            if (nextLocation.position.z > transform.position.z) {
                return "up";
            } else if (nextLocation.position.z < transform.position.z) {
                return "down";
            }
        }
        return "";
        */
        if (nextLocation.transform.position.x == transform.position.x - 1) {
            if (nextLocation.transform.position.z == transform.position.z + 1) {
                return "UL";
            } else if (nextLocation.transform.position.z == transform.position.z) {
                return "left";
            } else if (nextLocation.transform.position.z == transform.position.z - 1) {
                return "DL";
            }
        } else if (nextLocation.transform.position.x == transform.position.x) {
            if (nextLocation.transform.position.z == transform.position.z + 1) {
                return "up";
            } else if (nextLocation.transform.position.z == transform.position.z - 1) {
                return "down";
            }
        } else if (nextLocation.transform.position.x == transform.position.x + 1) {
            if (nextLocation.transform.position.z == transform.position.z + 1) {
                return "UR";
            } else if (nextLocation.transform.position.z == transform.position.z) {
                return "right";
            } else if (nextLocation.transform.position.z == transform.position.z - 1) {
                return "DR";
            }
        }
        return "";
    }
    private void ResetMovement() {
        clicked = false;
        isMoving = false;

        openSet.Clear();
        closedSet.Clear();
        cameFrom.Clear();
        cumulativeG.Clear();

        if (OnMoveInterrupt != null) {
            OnMoveInterrupt();
        }
        //miniMapArrowSpriteRend.enabled = false;
    }

    public void RunButton() {
        isRunning = !isRunning;
    }

    private void SpawnMiniMapArrow() {
        miniMapArrow.transform.position = targetLocation.position;
        miniMapArrow.transform.position += new Vector3(0, 5, 0);
        miniMapArrow.GetComponentInChildren<SpriteRenderer>().enabled = true;
    }
    private void SetStartingTile() {
        Collider[] hitTiles = Physics.OverlapSphere(transform.position, 0.2f);
        int i = 0;
        while (i < hitTiles.Length) {
            if (hitTiles[i].tag == "Walkable") {
                startingTile = hitTiles[i].gameObject;
                return;
            }
            i++;
        }
    }
    public void PrintCameFrom() {
        Debug.Log("-------cameFrom-------");
        foreach (KeyValuePair<GameObject, GameObject> kv in cameFrom)
            Debug.Log("Key: " + kv.Key + " Value: " + kv.Value);
    }
    public void PrintOpenSet() {
        Debug.Log("-------openSet-------");
        foreach (KeyValuePair<GameObject, float> kv in openSet)
            Debug.Log("Key: " + kv.Key + " Value: " + kv.Value);
    }
    public void PrintClosedSet() {
        Debug.Log("-------closedSet------");
        foreach (GameObject go in closedSet)
            Debug.Log(go.name);
    }
    private void SetNeighbors() {
        if (currentTile.GetComponent<WalkingBehavior>().adjLeft)
            neighbors[0] = currentTile.GetComponent<WalkingBehavior>().adjLeft;
        if (currentTile.GetComponent<WalkingBehavior>().adjUL)
            neighbors[1] = currentTile.GetComponent<WalkingBehavior>().adjUL;
        if (currentTile.GetComponent<WalkingBehavior>().adjUp)
            neighbors[2] = currentTile.GetComponent<WalkingBehavior>().adjUp;
        if (currentTile.GetComponent<WalkingBehavior>().adjUR)
            neighbors[3] = currentTile.GetComponent<WalkingBehavior>().adjUR;
        if (currentTile.GetComponent<WalkingBehavior>().adjRight)
            neighbors[4] = currentTile.GetComponent<WalkingBehavior>().adjRight;
        if (currentTile.GetComponent<WalkingBehavior>().adjDR)
            neighbors[5] = currentTile.GetComponent<WalkingBehavior>().adjDR;
        if (currentTile.GetComponent<WalkingBehavior>().adjDown)
            neighbors[6] = currentTile.GetComponent<WalkingBehavior>().adjDown;
        if (currentTile.GetComponent<WalkingBehavior>().adjDL)
            neighbors[7] = currentTile.GetComponent<WalkingBehavior>().adjDL;
    }
}
