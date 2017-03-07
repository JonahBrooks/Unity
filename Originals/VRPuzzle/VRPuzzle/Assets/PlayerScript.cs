using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//**********************************************
//
//TODO:
//  Add offscreen piece indicators
//  Add intro and outro screens
//  Add optional play modes based on time vs survival
//  Detect when no moves are possible
//      End game in one mode, reset board in time mode
//  Add special effects when removing gold bricks
//  Add special effects when placing a brick/generating gold bricks?
//  Add sound
//  Create method for rotating pieces in VR
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
    // Speed of camera when using mouse rotation
    public float cameraSpeed;
    // Canvas used to hold crosshairs
    public Canvas CHCanvas;
    // Score display
    public Text scoreTxt;
    // Prefabs for each brick type
    public GameObject Ibrick;
    public GameObject Jbrick;
    public GameObject Lbrick;
    public GameObject Obrick;
    public GameObject Sbrick;
    public GameObject Tbrick;
    public GameObject Zbrick;
    // Distance of piece from camera when carried
    private float pieceDistance;
    private float tempPieceDistance;
    // Rotation of camera
    private float yaw;
    private float pitch;
    private bool mouse1Down;
    // Player score
    private int score;

    // To hold the brick currently being moved, if any.
    private GameObject current;

    // To hold the current rotation (in degrees) of the current brick.
    private float rotation;

    // For use in changing camera angles

    // Use this for initialization
    void Start () {

        current = null;
        rotation = 0.0f;
        pitch = 45;
        yaw = 0;
        mouse1Down = false;
        score = 0;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        pieceDistance = Vector3.Distance(board.transform.position, Camera.main.transform.position);
        CHCanvas.planeDistance = pieceDistance;
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
        // Position in the board matrix of the center piece
        int x;
        int y;
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
                    // Remove piece from game
                    Destroy(current);
                    // Generate new piece
                    spawnPiece();
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
                        //last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouseSpeed;
                        castShadow();
                    }
                    // If clicked on piece is already set
                    if(hit.collider.transform.tag == "Set")
                    {
                        // Calculate the board position on which this block lies.
                        // If board can be somewhere other than origin, transform hit.point first
                        x = Mathf.FloorToInt(hit.point.x) + bs.gridwidth / 2;
                        y = Mathf.FloorToInt(hit.point.z) + bs.gridheight / 2;
                        // Clear row and/or column if they are full
                        score += bs.clearIfGold(x,y);
                    }
                }
                
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            mouse1Down = true;
        }
        if(Input.GetMouseButtonUp(1))
        {
            mouse1Down = false;
        }
        if(mouse1Down)
        {
            yaw += cameraSpeed * Input.GetAxis("Mouse X");
            pitch -= cameraSpeed * Input.GetAxis("Mouse Y");
            Camera.main.transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }
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
        
        if (Application.platform == RuntimePlatform.Android && !UnityEngine.VR.VRSettings.enabled)
        {
            Input.gyro.enabled = true;
            Camera.main.transform.rotation = Input.gyro.attitude;
            Camera.main.transform.Rotate(0f, 0f, 180f, Space.Self);
            Camera.main.transform.Rotate(90f, 180f, 0f, Space.World);
            adjustedAngle = Camera.main.transform.rotation.z;
            if (current != null && Mathf.Abs(adjustedAngle) > 0.1)
            {
                if(adjustedAngle > 0)
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
        // Keep current block in center of screen
        if (current != null)
        {
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

        scoreTxt.text = "Score: " + score.ToString();
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
        pos = Quaternion.AngleAxis(Random.Range(0,360),Vector3.up) * pos;
        pos = Quaternion.AngleAxis(Random.Range(0, 180), Vector3.left) * pos;
        // Move into world space
        pos += Camera.main.transform.position;
        // TODO: Make sure it is not hiden by the board
        
        Debug.Log("Making piece at " + pos.ToString());
        switch(rng)
        {
            case 0: // I-brick
                Object.Instantiate(Ibrick, pos, Quaternion.identity);
                break;
            case 1: // J-brick
                Object.Instantiate(Jbrick, pos, Quaternion.identity);
                break;
            case 2: // L-brick
                Object.Instantiate(Lbrick, pos, Quaternion.identity);
                break;
            case 3: // O-brick
                Object.Instantiate(Obrick, pos, Quaternion.identity);
                break;
            case 4: // S-brick
                Object.Instantiate(Sbrick, pos, Quaternion.identity);
                break;
            case 5: // T-brick
                Object.Instantiate(Tbrick, pos, Quaternion.identity);
                break;
            case 6: // Z-brick
                Object.Instantiate(Zbrick, pos, Quaternion.identity);
                break;
            case 7: // quirk of C# random generation
                Object.Instantiate(Lbrick, pos, Quaternion.identity);
                break; // Should never get here, but maybe since Random.value is inclusive
            default:
                Object.Instantiate(Lbrick, pos, Quaternion.identity);
                break;
        }
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
        x = (int)-1;
        y = (int)-1;
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
        switch (current.tag)
        {
            case "I-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //  [][*][][]
                    //Debug.Log("Casting shadow " + hit.point.x.ToString() + " " + hit.point.z.ToString());
                    if (set) bs.setBrick(current, x - 1, y, x, y, x + 1, y, x + 2, y);
                    return bs.checkBrick(x - 1, y, x, y, x + 1, y, x + 2, y);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    // []
                    // [*]
                    // []
                    // []
                    if (set) bs.setBrick(current, x, y + 1, x, y, x, y - 1, x, y - 2);
                    return bs.checkBrick(x, y+1, x, y, x, y-1, x, y-2);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    // [][][*][]
                    if (set) bs.setBrick(current, x - 2, y, x - 1, y, x, y, x + 1, y);
                    return bs.checkBrick(x-2, y, x-1, y, x, y, x+1, y);
                }
                else
                {
                    // []
                    // []
                    // [*]
                    // []
                    if (set) bs.setBrick(current, x, y + 2, x, y + 1, x, y, x, y - 1);
                    return bs.checkBrick(x, y+2, x, y+1, x, y, x, y-1);
                }
                //break;
            case "J-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //  []
                    //  []
                    //[][*]
                    if (set) bs.setBrick(current, x, y + 2, x, y + 1, x, y, x - 1, y);
                    return bs.checkBrick(x, y+2, x, y+1, x, y, x-1, y);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    // []
                    // [*][][]
                    if (set) bs.setBrick(current, x, y + 1, x, y, x + 1, y, x + 2, y);
                    return bs.checkBrick(x, y+1, x, y, x+1, y, x+2, y);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    // [*][]
                    // []
                    // []
                    if (set) bs.setBrick(current, x + 1, y, x, y, x, y - 1, x, y - 2);
                    return bs.checkBrick(x+1, y, x, y, x, y-1, x, y-2);
                }
                else
                {
                    // [][][*]
                    //     []
                    if (set) bs.setBrick(current, x - 2, y, x - 1, y, x, y, x, y - 1);
                    return bs.checkBrick(x-2, y, x-1, y, x, y, x, y-1);
                }
                //break;
            case "L-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //  []
                    //  []
                    //  [*][]
                    if (set) bs.setBrick(current, x, y + 2, x, y + 1, x, y, x + 1, y);
                    return bs.checkBrick(x, y+2, x, y+1, x, y, x+1, y);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    // [*][][]
                    // []
                    if (set) bs.setBrick(current, x, y - 1, x, y, x + 1, y, x + 2, y);
                    return bs.checkBrick(x, y-1, x, y, x+1, y, x+2, y);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    // [][*]
                    //   []
                    //   []
                    if (set) bs.setBrick(current, x - 1, y, x, y, x, y - 1, x, y - 2);
                    return bs.checkBrick(x-1, y, x, y, x, y-1, x, y-2);
                }
                else
                {
                    //    []
                    //[][][*]
                    if (set) bs.setBrick(current, x - 2, y, x - 1, y, x, y, x, y + 1);
                    return bs.checkBrick(x-2, y, x-1, y, x, y, x, y+1);
                }
                //break;
            case "O-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //  [][]
                    //  [*][]
                    if (set) bs.setBrick(current, x, y + 1, x + 1, y + 1, x, y, x + 1, y);
                    return bs.checkBrick(x, y+1, x+1, y+1, x, y, x+1, y);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    //  [*][]
                    //  [][]
                    if (set) bs.setBrick(current, x, y, x + 1, y, x, y - 1, x + 1, y - 1);
                    return bs.checkBrick(x, y, x+1, y, x, y-1, x+1, y-1);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    //  [][*]
                    //  [][]
                    if (set) bs.setBrick(current, x - 1, y, x, y, x - 1, y - 1, x, y - 1);
                    return bs.checkBrick(x-1, y, x, y, x-1, y-1, x, y-1);
                }
                else
                {
                    //  [][]
                    //  [][*]
                    if (set) bs.setBrick(current, x - 1, y + 1, x, y + 1, x - 1, y, x, y);
                    return bs.checkBrick(x-1, y+1, x, y+1, x-1, y, x, y);
                }
                //break;
            case "S-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //  [][]
                    //[][*]
                    if (set) bs.setBrick(current, x - 1, y, x, y, x, y + 1, x + 1, y + 1);
                    return bs.checkBrick(x-1, y, x, y, x, y+1, x+1, y+1);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    // []
                    // [*][]
                    //    []
                    if (set) bs.setBrick(current, x, y + 1, x, y, x + 1, y, x + 1, y - 1);
                    return bs.checkBrick(x, y+1, x, y, x+1, y, x+1, y-1);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    //  [*][]
                    //[][]
                    if (set) bs.setBrick(current, x - 1, y - 1, x, y - 1, x, y, x + 1, y);
                    return bs.checkBrick(x-1, y-1, x, y-1, x, y, x+1, y);
                }
                else
                {
                    // []
                    // [][*]
                    //   []
                    if (set) bs.setBrick(current, x - 1, y + 1, x - 1, y, x, y, x, y - 1);
                    return bs.checkBrick(x-1, y+1, x-1, y, x, y, x, y-1);
                }
                //break;
            case "T-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //    []
                    //  [][*][]
                    if (set) bs.setBrick(current, x - 1, y, x, y, x, y + 1, x + 1, y);
                    return bs.checkBrick(x-1, y, x, y, x, y+1, x+1, y);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    // []
                    // [*][]
                    // []
                    if (set) bs.setBrick(current, x, y + 1, x, y, x + 1, y, x, y - 1);
                    return bs.checkBrick(x, y+1, x, y, x+1, y, x, y-1);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    // [][*][]
                    //   []
                    if (set) bs.setBrick(current, x - 1, y, x, y, x, y - 1, x + 1, y);
                    return bs.checkBrick(x-1, y, x, y, x, y-1, x+1, y);
                }
                else
                {
                    //   []
                    // [][*]
                    //   []
                    if (set) bs.setBrick(current, x, y + 1, x - 1, y, x, y, x, y - 1);
                    return bs.checkBrick(x, y+1, x-1, y, x, y, x, y-1);
                }
                //break;
            case "Z-brick":
                if ((rotation >= 0 && rotation < 45) || (rotation >= 315 && rotation < 360))
                {
                    // Brick shape:
                    //  [][*]
                    //    [][]
                    if (set) bs.setBrick(current, x - 1, y, x, y, x, y - 1, x + 1, y - 1);
                    return bs.checkBrick(x-1, y, x, y, x, y-1, x+1, y-1);
                }
                else if (rotation >= 45 && rotation < 135)
                {
                    //   []
                    // [][*]
                    // []
                    if (set) bs.setBrick(current, x, y + 1, x, y, x - 1, y, x - 1, y - 1);
                    return bs.checkBrick(x, y+1, x, y, x-1, y, x-1, y-1);
                }
                else if (rotation >= 135 && rotation < 225)
                {
                    // [][]
                    //   [*][]
                    if (set) bs.setBrick(current, x - 1, y + 1, x, y + 1, x, y, x + 1, y);
                    return bs.checkBrick(x-1, y+1, x, y+1, x, y, x+1, y);
                }
                else
                {
                    //    []
                    // [*][]
                    // []
                    if (set) bs.setBrick(current, x + 1, y + 1, x + 1, y, x, y, x, y - 1);
                    return bs.checkBrick(x+1, y+1, x+1, y, x, y, x, y-1);
                }
                //break;

        }
           
        // TODO: For each case, find the matrix spots that will be filled
        // TODO: Display shadow
        // TODO: Update variable indicating whether it can be placed or not; reference that above
        // TODO: Left click becomes place at 90 degree snaps if possible, buzz if not.

        return true;
    }
}
