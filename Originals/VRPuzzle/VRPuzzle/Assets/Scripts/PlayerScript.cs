using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//**********************************************
//
//TODO:
//  Add offscreen piece indicators
//  Add intro and outro screens
//  Add optional play modes based on time vs survival
//  Add special effects when removing gold bricks
//  Add special effects when placing a brick/generating gold bricks?
//  Add sound
//  Add board variety
//      Allow for even sized boards
//      Allow for boards larger than 11 by 11 (currently can't reach the top edge on larger boards)
//
//**********************************************


public class PlayerScript : MonoBehaviour {

    // To hold stats for the brick currently being moved, if any.
    public float mouseSpeed;
    public float rotationSpeed;
    public float androidRotationSpeed;
    // To hold nearest game board (Currently only game board)
    public GameObject board;
    // Distance at which a piece will snap into place if dropped
    public float snapDistance;
    public float maxSnapDistance;
    // Min and max angles for spawning pieces
    public float minTheta;
    public float maxTheta;
    public float minPhi;
    public float maxPhi;
    // Number of pieces to keep in play
    public int numPieces;
    // Speed of camera when using mouse rotation
    public float cameraSpeed;
    // Score display
    public Text scoreTxt;
    // GameOver display
    public Text gameOverTxt;
    // Prefabs for each brick type
    public GameObject Ibrick;
    public GameObject Jbrick;
    public GameObject Lbrick;
    public GameObject Obrick;
    public GameObject Sbrick;
    public GameObject Tbrick;
    public GameObject Zbrick;
    // Sphere to represent "hands"
    public GameObject hand;
    // For sound and music
    public AudioSource placeEffect;
    public AudioSource grabEffect;
    public AudioSource music;
    // Distance of piece from camera when carried
    private float pieceDistance;
    private float tempPieceDistance;
    // Rotation of camera
    private float yaw;
    private float pitch;
    // Player score
    private int score;

    // To hold the brick currently being moved, if any.
    private GameObject current;

    // To hold the current rotation (in degrees) of the current brick.
    private float rotation;

    // List of bricks currently in play
    private List<GameObject> brickList;
    
    // Use this for initialization
    void Start () {

        current = null;
        rotation = 0.0f;
        pitch = 45;
        yaw = 0;
        score = 0;
        gameOverTxt.text = "";
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Cursor.lockState = CursorLockMode.Locked;
        pieceDistance = Vector3.Distance(board.transform.position, Camera.main.transform.position);
        hand.transform.localPosition = new Vector3 (0,0,pieceDistance);
        brickList = new List<GameObject>();
        for(int i = 0; i < numPieces; i++)
        {
            spawnPiece();
        }
    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        Ray ray;
        // Positions
        Vector3 boardXYZ;
        Vector3 brickXYZ;
        // Height, width, length
        Vector3 boardHWL;
        Vector3 brickHWL;
        // Camera center vector
        Vector3 screenCenter = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, pieceDistance);
        // Holds the rotation around the z axis in -180 to 180 degrees
        float adjustedAngle;
        // Board script
        BoardScript bs = (board.GetComponent("BoardScript") as BoardScript);

        // Detect proximity to board.
        if (current != null)
        {
            // This first line is just a contingency in case Pivot doesn't exist
            brickXYZ = current.GetComponentInChildren<Renderer>().bounds.center;
            boardXYZ = board.GetComponentInChildren<Renderer>().bounds.center;
            boardHWL = board.GetComponentInChildren<Renderer>().bounds.size;
            brickHWL = current.GetComponentInChildren<Renderer>().bounds.size;
            foreach (Transform child in current.transform)
            {
                if (child.tag == "Pivot")
                {
                    brickXYZ = child.GetComponentInChildren<Renderer>().bounds.center;
                }
            }

            // Check to see if brick is above and close enough to board
            if (brickXYZ.y - boardXYZ.y < snapDistance &&
                brickXYZ.y - boardXYZ.y > 0 &&
                brickXYZ.x + brickHWL.x / 2.0f > boardXYZ.x - boardHWL.x / 2.0f &&
                brickXYZ.x - brickHWL.x / 2.0f < boardXYZ.x + boardHWL.x / 2.0f &&
                brickXYZ.z + brickHWL.z / 2.0f > boardXYZ.z - boardHWL.z / 2.0f &&
                brickXYZ.z - brickHWL.z / 2.0f < boardXYZ.z + boardHWL.z / 2.0f
              )
            {
                // Debug: current.transform.Rotate(new Vector3(1, 0, 0), 90);
                castShadow();
            }

            // Keep current block in center of screen
            current.transform.position = Camera.main.ScreenToWorldPoint(screenCenter);
            // See if piece is above or below board
            if (brickXYZ.y - boardXYZ.y < maxSnapDistance &&
                brickXYZ.x + brickHWL.x / 2.0f > boardXYZ.x - boardHWL.x / 2.0f &&
                brickXYZ.x - brickHWL.x / 2.0f < boardXYZ.x + boardHWL.x / 2.0f &&
                brickXYZ.z + brickHWL.z / 2.0f > boardXYZ.z - boardHWL.z / 2.0f &&
                brickXYZ.z - brickHWL.z / 2.0f < boardXYZ.z + boardHWL.z / 2.0f
                )
            {
                current.transform.position = new Vector3(current.transform.position.x, snapDistance, current.transform.position.z);
            }
        }

        // Clear board on escape key / back button press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene(0);
            //bs.clearBoard();
            //score = 0;
            //gameOverTxt.text = "";
        }

        // Get left click information.
        if (Input.GetMouseButtonDown(0))
        {
            if (current != null)
            {   // Let go of piece
                // Check for board status before letting go.
                if (castShadow())
                {
                    // Piece can be placed
                    // Set piece in board
                    castShadow(true);
                    // Play sound effect
                    placeEffect.Play();
                    // Remove piece from game
                    Destroy(current);
                    // Generate new piece
                    spawnPiece();
                    // Check remaining moves
                    if (availableMoves() == false)
                    {
                        gameOverTxt.text = "Game Over!";
                    }
                }
                
                current = null;
                rotation = 0;
                bs.clearShadows();
            }
            else
            {   // Pick up piece, if one is targetted
                // Raycast to find object to pick up if no object currently, drop if so.
                // Start by looking where the mouse was clicked
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(!(Physics.Raycast(ray, out hit) && hit.collider.transform.tag == "Brick"))
                {
                    // Then check under the crosshairs
                    ray = Camera.main.ScreenPointToRay(screenCenter);
                }
                if (Physics.Raycast(ray,out hit))
                {
                    if(hit.collider.transform.tag == "Brick")
                    {
                        // Hold that object into current.
                        current = hit.collider.transform.parent.gameObject;
                        // Reset current statistics
                        rotation = current.transform.eulerAngles.y;
                        if (rotation < 0)
                        {
                            rotation += 360;
                        }
                        rotation = rotation % 360;
                        // Play sound effect
                        grabEffect.Play();
                        //last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouseSpeed;
                        castShadow();
                    }
                    //Debug.Log(hit.collider.transform.tag);
                    // If clicked on piece is already set
                    if(hit.collider.transform.tag == "Set"
                       || hit.collider.transform.tag == "Board")
                    {
                        // Clear row and/or column if they are full
                        score += bs.clearGold();
                    }
                }
                
            }
        }
 
        // General camera movement with mouse movement
        yaw += cameraSpeed * Input.GetAxis("Mouse X");
        pitch -= cameraSpeed * Input.GetAxis("Mouse Y");
        Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, 0);
        
        // Get middle click, rotate brick by 90 degree chunks.
        if (Input.GetMouseButtonDown(2))
        {
            if (current != null)
            {
                current.transform.Rotate(new Vector3(0, 1, 0), 90);
                rotation += 90;
                rotation = rotation % 360;
                castShadow();
            }
        }
        if (current != null)
        {
            // Rotate with mouse wheel
            rotation += Input.GetAxis("Mouse ScrollWheel") * rotationSpeed;
            if (rotation < 0)
            {
                rotation += 360;
            }
            rotation = rotation % 360;

            current.transform.Rotate(new Vector3(0, 1, 0), Input.GetAxis("Mouse ScrollWheel") * rotationSpeed);

            // Move with mouse
            //delta = new Vector3(Input.mousePosition.x,0, Input.mousePosition.y)*mouseSpeed - last;
            //last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouseSpeed;

            // TODO: Use Time.deltaTime
            castShadow();
        }

        // For Android:

        if (Application.platform == RuntimePlatform.Android && !UnityEngine.XR.XRSettings.enabled)
        {
            Input.gyro.enabled = true;
            Camera.main.transform.rotation = Input.gyro.attitude;
            Camera.main.transform.Rotate(0f, 0f, 180f, Space.Self);
            Camera.main.transform.Rotate(90f, 180f, 0f, Space.World);
            adjustedAngle = Camera.main.transform.rotation.z;
            if (current != null && Mathf.Abs(adjustedAngle) > 0.1)
            {
                if (adjustedAngle > 0)
                {
                    rotation += androidRotationSpeed;
                    current.transform.Rotate(new Vector3(0, 1, 0), androidRotationSpeed);
                }
                if (adjustedAngle < 0)
                {
                    rotation -= androidRotationSpeed;
                    current.transform.Rotate(new Vector3(0, 1, 0), -androidRotationSpeed);
                }
                if (rotation < 0)
                {
                    rotation += 360;
                }
                rotation = rotation % 360;

            }
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            adjustedAngle = -Camera.main.transform.rotation.z;
            if (current != null && Mathf.Abs(adjustedAngle) > 0.1)
            {
                if (adjustedAngle > 0)
                {
                    rotation += androidRotationSpeed;
                    current.transform.Rotate(new Vector3(0, 1, 0), androidRotationSpeed);
                }
                if (adjustedAngle < 0)
                {
                    rotation -= androidRotationSpeed;
                    current.transform.Rotate(new Vector3(0, 1, 0), -androidRotationSpeed);
                }
                if (rotation < 0)
                {
                    rotation += 360;
                }
                rotation = rotation % 360;
            }
        }
        

        scoreTxt.text = "Score: " + score.ToString();
    }

    // Checks to see if there are any available moves given the pieces and board state
    // Input: None
    // Output: True if there is at least one move possible
    //         False if no moves are available
    private bool availableMoves()
    {
        BoardScript bs = (board.GetComponent("BoardScript") as BoardScript);
        // Step through all pieces available
        // Check if that piece can be placed with any rotation at any position
        foreach ( GameObject piece in brickList)
        {
            for (int i = 0; i < bs.gridheight; i++)
            {
                for(int j = 0; j < bs.gridwidth; j++)
                {
                    for(int rot = 0; rot < 360; rot += 90)
                    {
                        if (checkPiece(piece, rot, j, i, false, false, true) == true)
                            return true;
                    }
                }
            }

        }

        return false;
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Spawns a piece into the world at a random location
    // Input: None
    // Output: None
    private void spawnPiece()
    {
        Vector3 pos;
        int rng = (int)(Random.value * 7);
        // Make vector in camera space first
        pos = new Vector3(0, 0, pieceDistance);

        // Rotate about the origin to position randomly
        pos = Quaternion.AngleAxis(Random.Range(minTheta, maxTheta),Vector3.up) * pos;
        pos = Quaternion.AngleAxis(Random.Range(minPhi, maxPhi), Vector3.left) * pos;
        // Move into world space
        pos += Camera.main.transform.position;
        // TODO: Make sure it is not hiden by the board
        
        //Debug.Log("Making piece at " + pos.ToString());
        switch(rng)
        {
            case 0: // I-brick
                brickList.Add(Object.Instantiate(Ibrick, pos, Quaternion.identity));
                break;
            case 1: // J-brick
                brickList.Add(Object.Instantiate(Jbrick, pos, Quaternion.identity));
                break;
            case 2: // L-brick
                brickList.Add(Object.Instantiate(Lbrick, pos, Quaternion.identity));
                break;
            case 3: // O-brick
                brickList.Add(Object.Instantiate(Obrick, pos, Quaternion.identity));
                break;
            case 4: // S-brick
                brickList.Add(Object.Instantiate(Sbrick, pos, Quaternion.identity));
                break;
            case 5: // T-brick
                brickList.Add(Object.Instantiate(Tbrick, pos, Quaternion.identity));
                break;
            case 6: // Z-brick
                brickList.Add(Object.Instantiate(Zbrick, pos, Quaternion.identity));
                break;
            case 7: // quirk of C# random generation
                brickList.Add(Object.Instantiate(Lbrick, pos, Quaternion.identity));
                break; // Should never get here, but maybe since Random.value is inclusive
            default:
                brickList.Add(Object.Instantiate(Lbrick, pos, Quaternion.identity));
                break;
        }

    }


    // Check if a piece brick with rotation rot can be set on the board
    // Input:   brick is the piece to check
    //          rot is the current rotation of the piece to check
    //          x is the position on the game board of the center brick in x
    //          y is the position on the game board of the center brick in y
    //          set determines whether the piece should be placed if possible
    //          shadow determines whether the shadow should be displayed for the piece
    //          ignoreGold determines if gold blocks should be considered empty
    // Returns: true if brick could be placed (possibly if gold blocks are cleared, if ignoreGold is true)
    private bool checkPiece(GameObject brick, float rot, int x, int y, bool set, bool shadow, bool ignoreGold)
    {
        BoardScript bs = (board.GetComponent("BoardScript") as BoardScript);
        switch (brick.tag)
        {
            case "I-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //  [][*][][]
                    //Debug.Log("Casting shadow " + hit.point.x.ToString() + " " + hit.point.z.ToString());
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x + 1, y, x + 2, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x + 1, y, x + 2, y, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    // []
                    // [*]
                    // []
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x, y, x, y - 1, x, y - 2))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x, y, x, y - 1, x, y - 2, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    // [][][*][]
                    if (set)
                    {
                        if (bs.setBrick(x - 2, y, x - 1, y, x, y, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 2, y, x - 1, y, x, y, x + 1, y, shadow, ignoreGold);
                }
                else
                {
                    // []
                    // []
                    // [*]
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x, y + 2, x, y + 1, x, y, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 2, x, y + 1, x, y, x, y - 1, shadow, ignoreGold);
                }
            //break;
            case "J-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //  []
                    //  []
                    //[][*]
                    if (set)
                    {
                        if (bs.setBrick(x, y + 2, x, y + 1, x, y, x - 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 2, x, y + 1, x, y, x - 1, y, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    // []
                    // [*][][]
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x, y, x + 1, y, x + 2, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x, y, x + 1, y, x + 2, y, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    // [*][]
                    // []
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x + 1, y, x, y, x, y - 1, x, y - 2))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x + 1, y, x, y, x, y - 1, x, y - 2, shadow, ignoreGold);
                }
                else
                {
                    // [][][*]
                    //     []
                    if (set)
                    {
                        if (bs.setBrick(x - 2, y, x - 1, y, x, y, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 2, y, x - 1, y, x, y, x, y - 1, shadow, ignoreGold);
                }
            //break;
            case "L-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //  []
                    //  []
                    //  [*][]
                    if (set)
                    {
                        if (bs.setBrick(x, y + 2, x, y + 1, x, y, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 2, x, y + 1, x, y, x + 1, y, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    // [*][][]
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x, y - 1, x, y, x + 1, y, x + 2, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y - 1, x, y, x + 1, y, x + 2, y, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    // [][*]
                    //   []
                    //   []
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x, y - 1, x, y - 2))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x, y - 1, x, y - 2, shadow, ignoreGold);
                }
                else
                {
                    //    []
                    //[][][*]
                    if (set)
                    {
                        if (bs.setBrick(x - 2, y, x - 1, y, x, y, x, y + 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 2, y, x - 1, y, x, y, x, y + 1, shadow, ignoreGold);
                }
            //break;
            case "O-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //  [][]
                    //  [*][]
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x + 1, y + 1, x, y, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x + 1, y + 1, x, y, x + 1, y, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    //  [*][]
                    //  [][]
                    if (set)
                    {
                        if (bs.setBrick(x, y, x + 1, y, x, y - 1, x + 1, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y, x + 1, y, x, y - 1, x + 1, y - 1, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    //  [][*]
                    //  [][]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x - 1, y - 1, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x - 1, y - 1, x, y - 1, shadow, ignoreGold);
                }
                else
                {
                    //  [][]
                    //  [][*]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y + 1, x, y + 1, x - 1, y, x, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y + 1, x, y + 1, x - 1, y, x, y, shadow, ignoreGold);
                }
            //break;
            case "S-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //  [][]
                    //[][*]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x, y + 1, x + 1, y + 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x, y + 1, x + 1, y + 1, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    // []
                    // [*][]
                    //    []
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x, y, x + 1, y, x + 1, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x, y, x + 1, y, x + 1, y - 1, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    //  [*][]
                    //[][]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y - 1, x, y - 1, x, y, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y - 1, x, y - 1, x, y, x + 1, y, shadow, ignoreGold);
                }
                else
                {
                    // []
                    // [][*]
                    //   []
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y + 1, x - 1, y, x, y, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y + 1, x - 1, y, x, y, x, y - 1, shadow, ignoreGold);
                }
            //break;
            case "T-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //    []
                    //  [][*][]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x, y + 1, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x, y + 1, x + 1, y, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    // []
                    // [*][]
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x, y, x + 1, y, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x, y, x + 1, y, x, y - 1, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    // [][*][]
                    //   []
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x, y - 1, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x, y - 1, x + 1, y, shadow, ignoreGold);
                }
                else
                {
                    //   []
                    // [][*]
                    //   []
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x - 1, y, x, y, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x - 1, y, x, y, x, y - 1, shadow, ignoreGold);
                }
            //break;
            case "Z-brick":
                if ((rot >= 0 && rot < 45) || (rot >= 315 && rot < 360))
                {
                    // Brick shape:
                    //  [][*]
                    //    [][]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y, x, y, x, y - 1, x + 1, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y, x, y, x, y - 1, x + 1, y - 1, shadow, ignoreGold);
                }
                else if (rot >= 45 && rot < 135)
                {
                    //   []
                    // [][*]
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x, y + 1, x, y, x - 1, y, x - 1, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x, y + 1, x, y, x - 1, y, x - 1, y - 1, shadow, ignoreGold);
                }
                else if (rot >= 135 && rot < 225)
                {
                    // [][]
                    //   [*][]
                    if (set)
                    {
                        if (bs.setBrick(x - 1, y + 1, x, y + 1, x, y, x + 1, y))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x - 1, y + 1, x, y + 1, x, y, x + 1, y, shadow, ignoreGold);
                }
                else
                {
                    //    []
                    // [*][]
                    // []
                    if (set)
                    {
                        if (bs.setBrick(x + 1, y + 1, x + 1, y, x, y, x, y - 1))
                        {
                            brickList.Remove(brick);
                        }
                    }
                    return bs.checkBrick(x + 1, y + 1, x + 1, y, x, y, x, y - 1, shadow, ignoreGold);
                }
                //break;
        }

        return false; // If the piece is not valid, return false
    }

    // Casts a shadow of the piece on the board, indicating whether it can be played or not
    // Input: None, but it uses current
    // Output: True if the piece can be placed, false if not
    private bool castShadow(bool set = false)
    {
        RaycastHit hit;
        // Position
        Vector3 brickXYZ;

        // Position in the board matrix of the center piece
        int x;
        int y;
        // Board script
        BoardScript bs = (board.GetComponent("BoardScript") as BoardScript);

        brickXYZ = current.GetComponentInChildren<Renderer>().bounds.center;
        // TODO: Display shadow of where piece will place
        x = (int)-10;
        y = (int)-10;
        if (Physics.Raycast(brickXYZ, new Vector3(0, -1, 0), out hit) &&
              hit.collider.transform.parent != null &&
              hit.collider.transform.parent.tag == "Board")
        {
            // Calculate the board position on which this block lies.
            // If board can be somewhere other than origin, transform hit.point first
            x = Mathf.FloorToInt(hit.point.x) + bs.gridwidth / 2;
            y = Mathf.FloorToInt(hit.point.z) + bs.gridheight / 2;
            //debug.text = x + " " + y;
            //Debug.Log("Hit board");
        }

        //debug.text = rotation.ToString();
        // Switch statement on current tag to get piece shape
        //debug.text = "Trying to cast shadow " + hit.point.x.ToString() + " " + hit.point.z.ToString();
        return checkPiece(current, rotation, x, y, set, true, false);
    }
}
