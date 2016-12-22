using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    // To hold the brick currently currently being moved, if any.
    public GameObject current;
    public float mouse_speed;
    // To hold nearest game board (Currently only game board)
    public GameObject board;
    // For debugging
    public Text debug;

    // To hold the current rotation (in degrees) of the current brick.
    private float rotation;
    // To hold the last none position of the mouse
    private Vector3 last;
    // To hold the delta movement of the mouse since last frame
    private Vector3 delta;

    // For use in changing camera angles
    // TODO: Add camera movement. Right drag for camera move.

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
		// Get left click information.
        if (Input.GetMouseButtonDown(0))
        {
            if (current != null)
            {
                // TODO: Check for board status before letting go.
                current = null;
            }
            else
            {
                // Raycast to find object to pick up if no object currently, drop if so.
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray,out hit))
                {
                    if(hit.collider.transform.parent.tag == "Brick")
                    {
                        // Hold that object into current.
                        current = hit.collider.transform.parent.gameObject;
                        // Reset current statistics
                        rotation = 0;
                        last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouse_speed;
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            // TODO: Handle camera
        }
        // Get middle click, rotate brick by 90 degree chunks.
        if (Input.GetMouseButtonDown(3))
        {
            if (current != null)
            {
                current.transform.Rotate(new Vector3(0, 1, 0), 90);
                rotation += 90;
                rotation = rotation % 360;
            }
        }
        // TODO: Get middle mouse wheel, rotate brick.

        // Detect motion of mouse and update current, if any.
        if (current != null)
        {
            delta = new Vector3(Input.mousePosition.x,0, Input.mousePosition.y)*mouse_speed - last;
            last = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y)*mouse_speed;

            current.transform.Translate(delta);
            debug.text = delta.ToString();
        }
        // TODO: Detect proximity to board.
        // TODO: Left click becomes place at 90 degree snaps if possible, buzz if not.
        // TODO: Call board functions to place piece.
    }
}
