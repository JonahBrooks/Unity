using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour {

    public int gridwidth;
    public int gridheight;
    public GameObject Ground;
    public GameObject WestWall;
    public GameObject EastWall;
    public GameObject NorthWall;
    public GameObject SouthWall;

    // Matrix for storing pieces
    private bool[,] board = new bool[16,16];

    // Function for checking a single block on the grid for fullness
    // Input: An x,y coordinate from (-gridwidth/2,-gridheigh/2) to (gridwidh/2,gridheight/2)
    // Output: Whether this grid coordinate is empty (true) or not (false)
    public bool checkGrid(int x, int y)
    {
        if (x < 0 || y < 0 || x > gridwidth || y > gridheight)
        {
            return false;
        }
        return board[x, y];
    }

    // Function for checking an entire piece but not placing it
    // Input: 8 coordinates of 4 blocks in 1 brick
    // Output: True if all blocks can be placed, False is not
    public bool checkBrick(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        return checkGrid(x0, y0) & checkGrid(x1, y1) & checkGrid(x2, y2) & checkGrid(x3, y3);
    }

    // Use this for initialization
    void Start () {
		for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                board[i,j] = false; // Set board to empty
            }
        }
        // Resize board based on width and height
        Ground.transform.localScale = new Vector3(gridwidth / 10.0f, 1, gridheight / 10.0f);
        WestWall.transform.localScale = new Vector3(gridwidth, 1, 1);
        EastWall.transform.localScale = new Vector3(gridwidth, 1, 1);
        NorthWall.transform.localScale = new Vector3(1, 1, gridheight + 2);
        SouthWall.transform.localScale = new Vector3(1, 1, gridheight + 2);
        // Position the walls based on width and height of board
        WestWall.transform.localPosition = new Vector3(0,0,-gridwidth/2.0f - 0.5f);
        EastWall.transform.localPosition = new Vector3(0, 0, gridwidth / 2.0f + 0.5f);
        NorthWall.transform.localPosition = new Vector3(-gridwidth / 2.0f - 0.5f, 0, 0);
        SouthWall.transform.localPosition = new Vector3(gridwidth / 2.0f + 0.5f, 0, 0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
