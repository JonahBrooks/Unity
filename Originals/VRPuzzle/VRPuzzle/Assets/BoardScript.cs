using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {

    public int gridwidth;
    public int gridheight;
    public int groundY;
    public GameObject Ground;
    public GameObject WestWall;
    public GameObject EastWall;
    public GameObject NorthWall;
    public GameObject SouthWall;
    // Block prefab for the game board
    public GameObject BlockTrans;
    // Materials for different shadows
    public Material BSh;
    public Material RSh;
    public Material WSh;

    // Matrix for storing pieces
    private GameObject[,] board;
    private GameObject Block;
    private Material[] materials;
    private int gridMin = 0; // Grid starts at 1
    private bool debug = false;

    // Function for checking a single block on the grid for fullness
    // Input: An x,y coordinate from (-gridwidth/2,-gridheigh/2) to (gridwidh/2,gridheight/2)
    // Output: Whether this grid coordinate is empty (true) or not (false)
    public bool checkGrid(int x, int y)
    {
        // NOTE: This excludes 0, which it shouldn't. But this makes it work for some reason.
        if (x < gridMin || y < gridMin || x >= gridwidth || y >= gridheight)
        {
            return false;
        }
        //Debug.Log(x.ToString() + " " + y.ToString());
        return board[x, y].tag != "Set";
    }

    // Function for checking if a row is entirely filled
    // Input: The row rumber to be checked
    // Output: True if the row is entirely filled, false if not
    public bool checkRow(int y)
    {
        bool toReturn = true;

        for (int x = gridMin; x < gridwidth; x++)
        {
            if (checkGrid(x,y) == true)
            {
                toReturn = false;
            }
        }

        return toReturn;
    }

    // Function for checking if a column is entirely filled
    // Input: The row rumber to be checked
    // Output: True if the row is entirely filled, false if not
    public bool checkCol(int x)
    {
        bool toReturn = true;

        for (int y = gridMin; y < gridheight; y++)
        {
            if (checkGrid(x, y) == false)
            {
                toReturn = false;
            }
        }

        return toReturn;
    }

    // Erase entire board except for set pieces
    public void clearShadows()
    {
        // Set material of entire game board to white
        foreach (GameObject block in board)
        {
            // Don't clear placed bricks
            if (block.tag != "Set")
            {
                block.GetComponent<Renderer>().material = WSh;
                block.GetComponent<Renderer>().enabled = false;
                if (debug)
                {   // Continue to show full grid in debug mode
                    block.GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }

    // Function for checking an entire piece but not placing it
    // Input: 8 coordinates of 4 blocks in 1 brick
    // Output: True if all blocks can be placed, False is not
    public bool checkBrick(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        bool isValid = checkGrid(x0, y0) & checkGrid(x1, y1) & checkGrid(x2, y2) & checkGrid(x3, y3);
        //Debug.Log("Casting Shadow!");

        if (isValid)
        {
            // Clear any active shadows before casting new ones
            clearShadows();
            // Enable all 4 blocks
            board[x0, y0].GetComponent<Renderer>().enabled = true;
            board[x1, y1].GetComponent<Renderer>().enabled = true;
            board[x2, y2].GetComponent<Renderer>().enabled = true;
            board[x3, y3].GetComponent<Renderer>().enabled = true;
            // Cast blue shadow
            board[x0, y0].GetComponent<Renderer>().material = BSh;
            board[x1, y1].GetComponent<Renderer>().material = BSh;
            board[x2, y2].GetComponent<Renderer>().material = BSh;
            board[x3, y3].GetComponent<Renderer>().material = BSh;
            //Debug.Log("Blue Shadow!");
        } else
        {
            // Clear any active shadows before casting new ones
            clearShadows();
            // Cast red shadow in valid positions if other positions are invalid
            if (checkGrid(x0,y0))
            {
                board[x0, y0].GetComponent<Renderer>().enabled = true;
                board[x0, y0].GetComponent<Renderer>().material = RSh;
            }
            if (checkGrid(x1, y1))
            {
                board[x1, y1].GetComponent<Renderer>().enabled = true;
                board[x1, y1].GetComponent<Renderer>().material = RSh;
            }
            if (checkGrid(x2, y2))
            {
                board[x2, y2].GetComponent<Renderer>().enabled = true;
                board[x2, y2].GetComponent<Renderer>().material = RSh;
            }
            if (checkGrid(x3, y3))
            {
                board[x3, y3].GetComponent<Renderer>().enabled = true;
                board[x3, y3].GetComponent<Renderer>().material = RSh;
            }
            //Debug.Log("Red Shadow!");
        }
        return isValid;
    }

    // Sets all blocks in a brick to true in the board matrix
    public void setBrick(GameObject parent, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        GameObject[] children = new GameObject[4];
        if (checkBrick(x0, y0, x1, y1, x2, y2, x3, y3))
        {
            // Turn on blocks if not already on
            board[x0, y0].GetComponent<Renderer>().enabled = true;
            board[x1, y1].GetComponent<Renderer>().enabled = true;
            board[x2, y2].GetComponent<Renderer>().enabled = true;
            board[x3, y3].GetComponent<Renderer>().enabled = true;
            // Set to white
            board[x0, y0].GetComponent<Renderer>().material = WSh;
            board[x1, y1].GetComponent<Renderer>().material = WSh;
            board[x2, y2].GetComponent<Renderer>().material = WSh;
            board[x3, y3].GetComponent<Renderer>().material = WSh;
            // Make Opaque
            board[x0, y0].GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            board[x1, y1].GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            board[x2, y2].GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            board[x3, y3].GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
            // Tag as Set
            board[x0, y0].tag = "Set";
            board[x1, y1].tag = "Set";
            board[x2, y2].tag = "Set";
            board[x3, y3].tag = "Set";

            // Check for row and column completions
            if (checkRow(y0) || checkRow(y1) || checkRow(y2) || checkRow(y3))
            {
                Debug.Log("Row Complete!");
            }
            if (checkCol(x0) || checkCol(x1) || checkCol(x2) || checkCol(x3))
            {
                Debug.Log("Column Complete!");
            }
        }
    }

    // Use this for initialization
    void Start () {
        board = new GameObject[gridwidth, gridheight];
        Block = BlockTrans.gameObject;

        for (int i = 0; i < gridwidth; i++)
        {
            for (int j = 0; j < gridheight; j++)
            {
                // Create blocks on the board
                board[i, j] = Instantiate(Block, new Vector3(i - gridwidth / 2, 
                    groundY - Block.transform.localScale.y, 
                    j - gridheight / 2), Quaternion.identity);
                board[i, j].layer = LayerMask.NameToLayer("Ignore Raycast");
                board[i, j].GetComponent<Renderer>().enabled = false;
                if (debug)
                {
                    board[i, j].GetComponent<Renderer>().enabled = true;
                }
                if (i < gridMin || j < gridMin)
                {
                    board[i, j].GetComponent<Renderer>().enabled = false;
                }
            }
        }
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
