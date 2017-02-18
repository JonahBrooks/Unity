using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    // To hold stats for the brick currently being moved, if any.
    public float mouseSpeed;
    public float rotationSpeed;
    // To hold nearest game board (Currently only game board)
    public GameObject board;
    // Distance at which a piece will snap into place if dropped
    public float snapDistance;

    // To hold the brick currently being moved, if any.
    private GameObject current;

    // To hold the current rotation (in degrees) of the current brick.
    private float rotation;
    // To hold the last known position of the mouse
    private Vector3 last;
    // To hold the delta movement of the mouse since last frame
    private Vector3 delta;
    // Debug text for android debugging
    public Text debug;
    // For use in changing camera angles
    // TODO: Add movement. Right drag for camera move.

    // Use this for initialization
    void Start () {

        current = null;
        rotation = 0.0f;
        last = new Vector3(0, 0, 0);

    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        Ray ray;
        // Acceleration vector
        Vector3 accelVector;
        Quaternion accelQuat;
        // Positions
        Vector3 boardXYZ;
        Vector3 brickXYZ;
        // Height, width, length
        Vector3 boardHWL;
        Vector3 brickHWL;
        // Position in the board matrix of the center piece
        int x;
        int y;
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
                }
                
                current = null;
                rotation = 0;
                bs.clearShadows();
            }
            else
            {   // Pick up piece, if one is targetted
                // Raycast to find object to pick up if no object currently, drop if so.
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
                        last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouseSpeed;
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
                        bs.clearIfGold(x,y);
                    }
                }
                
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            // TODO: Handle camera
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
            delta = new Vector3(Input.mousePosition.x,0, Input.mousePosition.y)*mouseSpeed - last;
            last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouseSpeed;

            // TODO: Use Time.deltaTime
            current.transform.Translate(delta, Space.World);
            castShadow();
        }

        // For Android:
        accelVector = Input.acceleration;
        Input.gyro.enabled = true;
        accelQuat = Quaternion.Inverse(Input.gyro.attitude);
        accelQuat.SetLookRotation(new Vector3(0, -Mathf.PI/4, 1));
        accelQuat *= Quaternion.Inverse(Input.gyro.attitude);

        if (accelVector.sqrMagnitude > 1)
        {
            accelVector.Normalize();
        }
        accelVector *= Time.deltaTime * 1000;

        //Camera.main.transform.Rotate(accelVector);
        Camera.main.transform.rotation = accelQuat;
        
        debug.text = accelQuat.ToString();


        
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
