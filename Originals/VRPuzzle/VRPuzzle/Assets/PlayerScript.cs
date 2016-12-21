using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    // To hold the brick currently currently being moved, if any.
    public GameObject current;
    // To hold nearest game board (Currently only game board)
    public GameObject board;

    // To hold the current rotation (in degrees) of the current brick.
    private float rotation;

    // For use in changing camera angles
    // TODO: Add camera movement. Right drag for camera move.

	// Use this for initialization
	void Start () {

        current = null;
        rotation = 0.0f;
		
	}
	
	// Update is called once per frame
	void Update () {
		// TODO: Get left click information.
        // TODO: Raycast to find object to pick up if no object currently, drop if so.
        // TODO: Hold that object into current.
        // TODO: Get middle mouse wheel, rotate brick.
        // TODO: Get middle click, rotate brick by 90 degree chunks.

        // TODO: Detect proximity to board.
        // TODO: Left click becomes place at 90 degree snaps if possible, buzz if not.
        // TODO: Call board functions to place piece.
	}
}
