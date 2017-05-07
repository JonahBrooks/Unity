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
    public Material BSh;    // Blue Shadow to show possible placement
    public Material RSh;    // Red Shadow to show impossible placement
    public Material WSh;    // White Shadow for final placed pieces
    public Material GSh;    // Gold Shadow to show complete row or column

    // Matrix for storing pieces
    private GameObject[,] board;
    private GameObject Block;
    private Material[] materials;
    private int gridMin = 0; // Grid starts at 1
    private bool debug = false;

    // Function for checking a single block on the grid for fullness
    // Input: An x,y coordinate from (-gridwidth/2,-gridheigh/2) to (gridwidh/2,gridheight/2)
    //        ignoreGold determines whether this should return true for gold blocks (used for checking remaining moves)
    // Output: Whether this grid coordinate is empty (true) or not (false)
    public bool checkGrid(int x, int y, bool ignoreGold = false)
    {
        // NOTE: This excludes 0, which it shouldn't. But this makes it work for some reason.
        if (x < gridMin || y < gridMin || x >= gridwidth || y >= gridheight)
        {
            return false;
        }

        if (ignoreGold)
        {
            return board[x, y].tag != "Set" || board[x, y].GetComponent<Renderer>().sharedMaterial == GSh;
        }
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
            if (checkGrid(x, y) == true)
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
    //          shadow determines if a shadow should be placed on the board
    //          ignoreGold determines if gold blocks should be considered empty (used for checking remaining moves)
    // Output: True if all blocks can be placed, False is not
    public bool checkBrick(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, bool shadow = false, bool ignoreGold = false)
    {
        bool isValid = checkGrid(x0, y0,ignoreGold) & checkGrid(x1, y1,ignoreGold) 
                        & checkGrid(x2, y2,ignoreGold) & checkGrid(x3, y3,ignoreGold);
        //Debug.Log("Casting Shadow!");
        if (shadow == false)
        {
            return isValid; // Don't cast shadow if shadow is false
        }
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
    // Returns: Whether the block was set or not
    public bool setBrick(GameObject parent, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {

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
            // Enable Raycast for these blocks
            board[x0, y0].layer = LayerMask.NameToLayer("Default");
            board[x1, y1].layer = LayerMask.NameToLayer("Default");
            board[x2, y2].layer = LayerMask.NameToLayer("Default");
            board[x3, y3].layer = LayerMask.NameToLayer("Default");

            // Check for row and column completions
            if (checkRow(y0) || checkRow(y1) || checkRow(y2) || checkRow(y3))
            {
                // Highlight row
                for (int i = 0; i < gridwidth; i++)
                {
                    if (checkRow(y0))
                    {
                        board[i, y0].GetComponent<Renderer>().material = GSh;
                    }
                    if (checkRow(y1))
                    {
                        board[i, y1].GetComponent<Renderer>().material = GSh;
                    }
                    if (checkRow(y2))
                    {
                        board[i, y2].GetComponent<Renderer>().material = GSh;
                    }
                    if (checkRow(y3))
                    {
                        board[i, y3].GetComponent<Renderer>().material = GSh;
                    }
                }
            }
            if (checkCol(x0) || checkCol(x1) || checkCol(x2) || checkCol(x3))
            {
                // Highlight column
                for (int i = 0; i < gridheight; i++)
                {
                    if (checkCol(x0))
                    {
                        board[x0,i].GetComponent<Renderer>().material = GSh;
                    }
                    if (checkCol(x1))
                    {
                        board[x1,i].GetComponent<Renderer>().material = GSh;
                    }
                    if (checkCol(x2))
                    {
                        board[x2,i].GetComponent<Renderer>().material = GSh;
                    }
                    if (checkCol(x3))
                    {
                        board[x3,i].GetComponent<Renderer>().material = GSh;
                    }
                }
            }
            return true;
        }
        return false;
    }

    // Clears all gold blocks
    // Input: none
    // Output: score generated, if any
    public int clearGold()
    {
        int score = 0;
        int combo = 1;
        // TODO: Add flashy effects
        AudioSource audio = GetComponent<AudioSource>();


        audio.Play();
        // Clear all gold blocks
        foreach (GameObject block in board)
        { 
            // If block is gold
            if (block.GetComponent<Renderer>().sharedMaterial == GSh)
            {
                // Clear block
                // Change each block to white
                block.GetComponent<Renderer>().material = WSh;
                // Make translucent
                block.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);
                // Disable
                block.GetComponent<Renderer>().enabled = false;
                // Unset
                block.tag = "Unset";
                // Disable Raycast from hitting this invisible block
                block.layer = LayerMask.NameToLayer("Ignore Raycast");
                // Increase score by 1 for each block removed before this one
                score += combo++;
            }
        }
        return score;
    }


    // Clears entire board
    // Input: None
    // Output: None
    public void clearBoard()
    {
        // Clear all blocks
        foreach (GameObject block in board)
        {
            // Clear block
            // Change each block to white
            block.GetComponent<Renderer>().material = WSh;
            // Make translucent
            block.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);
            // Disable
            block.GetComponent<Renderer>().enabled = false;
            // Unset
            block.tag = "Unset";
            // Disable Raycast from hitting this invisible block
            block.layer = LayerMask.NameToLayer("Ignore Raycast");
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
                board[i, j].tag = "Unset";
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
