using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour {

    public int gridwidth;
    public int gridheight;
    public int groundY;
    public GameObject Ground;
    public GameObject WestWall;
    public GameObject EastWall;
    public GameObject NorthWall;
    public GameObject SouthWall;
    // Shadows for indicating whether a piece can be placed or not
    public GameObject RSh0, RSh1, RSh2, RSh3; // Red shadows gameobjects
    public GameObject BSh0, BSh1, BSh2, BSh3; // Blue shadow game objects

    // Matrix for storing pieces
    private bool[,] board;
    // Grouping shadows
    private GameObject[] RSh;
    private GameObject[] BSh;

    // Function for checking a single block on the grid for fullness
    // Input: An x,y coordinate from (-gridwidth/2,-gridheigh/2) to (gridwidh/2,gridheight/2)
    // Output: Whether this grid coordinate is empty (true) or not (false)
    public bool checkGrid(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridwidth || y >= gridheight)
        {
            return false;
        }
        return !board[x, y];
    }

    // Function for checking an entire piece but not placing it
    // Input: 8 coordinates of 4 blocks in 1 brick
    // Output: True if all blocks can be placed, False is not
    public bool checkBrick(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        bool isValid = checkGrid(x0, y0) & checkGrid(x1, y1) & checkGrid(x2, y2) & checkGrid(x3, y3);

        if (isValid)
        {
            // TODO: Blue shadow in all locations
            BSh0.transform.position = new Vector3(x0 - gridwidth/2, groundY, y0 - gridheight/2);
            BSh1.transform.position = new Vector3(x1 - gridwidth/2, groundY, y1 - gridheight/2);
            BSh2.transform.position = new Vector3(x2 - gridwidth/2, groundY, y2 - gridheight/2);
            BSh3.transform.position = new Vector3(x3 - gridwidth/2, groundY, y3 - gridheight/2);
        } else
        {
            // TODO: Red shadow in any valid locations
        }

        return isValid;
    }

    // Use this for initialization
    void Start () {
        board = new bool[gridwidth, gridheight];

        for (int i = 0; i < gridwidth; i++)
        {
            for (int j = 0; j < gridheight; j++)
            {
                board[i,j] = false; // Set board to empty
            }
        }
        // Set up array for accessing shadow objects
        BSh = new GameObject[4];
        BSh[0] = BSh0; BSh[1] = BSh1; BSh[2] = BSh2; BSh[3] = BSh3;
        RSh = new GameObject[4];
        RSh[0] = RSh0; RSh[1] = RSh1; RSh[2] = RSh2; RSh[3] = RSh3;
        // Resize board based on width and height
        Ground.transform.localScale = new Vector3(gridwidth / 10.0f, groundY, gridheight / 10.0f);
        WestWall.transform.localScale = new Vector3(gridwidth, groundY, 1);
        EastWall.transform.localScale = new Vector3(gridwidth, groundY, 1);
        NorthWall.transform.localScale = new Vector3(1, groundY, gridheight + 2);
        SouthWall.transform.localScale = new Vector3(1, groundY, gridheight + 2);
        // Position the walls based on width and height of board
        WestWall.transform.localPosition = new Vector3(0, groundY -1, -gridwidth/2.0f - 0.5f);
        EastWall.transform.localPosition = new Vector3(0, groundY -1, gridwidth / 2.0f + 0.5f);
        NorthWall.transform.localPosition = new Vector3(-gridwidth / 2.0f - 0.5f, groundY - 1, 0);
        SouthWall.transform.localPosition = new Vector3(gridwidth / 2.0f + 0.5f, groundY - 1, 0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
