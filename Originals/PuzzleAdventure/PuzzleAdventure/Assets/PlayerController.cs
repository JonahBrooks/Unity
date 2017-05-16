using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // Stores the grid of gameobjects
    public GameObject[][] board = new GameObject[8][];

    // Stores for each element in board if it should be cleared
    private bool[][] toClear = new bool[8][];
    // For initializing slimes
    private GridLayout gl;
    private Dictionary<string,int> slimeDict = new Dictionary<string,int>();

	// Use this for initialization
	void Start () {
        gl = gameObject.GetComponent<GridLayout>();
        slimeDict.Add("Blue(Clone)", 0);
        slimeDict.Add("Gray(Clone)", 1);
        slimeDict.Add("Green(Clone)", 2);
        slimeDict.Add("Purple(Clone)", 3);
        slimeDict.Add("Red(Clone)", 4);
        slimeDict.Add("Yellow(Clone)", 5);
        // Initialize the board
        for (int i = 0; i < 8; i++)
        {
            board[i] = new GameObject[8];
            toClear[i] = new bool[8];
            for(int j = 0; j < 8; j++)
            {
                toClear[i][j] = false;
            }
        }
        // Generate new board
        gl.NewBoard();
        // Start out with no sets of 3 on the board
        while(Clear3s() > 0)
        {
            //pass
        }
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit2D hit;
        int x;
        int y;
        if (Input.GetMouseButtonDown(0))
        {
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                    Vector2.zero, 0f);
            if (hit)
            {
                x = hit.transform.GetComponentInParent<Coordinates>().x;
                y = hit.transform.GetComponentInParent<Coordinates>().y;

                Debug.Log("Clicked on " + x + ", " + y + " at " + hit.transform.position);
            }
        }
    }

 
    // Find and clear matches of 3, return score generated
    private int Clear3s()
    {
        int score = 0;
        int numInARow = 1;
        string lastName = "";

        //Check horizontal
        for(int i = 0; i < 8; i++)
        {
            for(int j =0; j < 8; j++)
            {
                if (lastName == board[i][j].transform.name)
                {
                    numInARow++;
                }
                else
                {
                    numInARow = 1;
                }
                lastName = board[i][j].transform.name;
                if(numInARow >= 3)
                {
                    toClear[i][j - 2] = true;
                    toClear[i][j - 1] = true;
                    toClear[i][j] = true;
                }
            }
            lastName = "";
            numInARow = 1;
        }
        //Check vertical
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (lastName == board[j][i].transform.name)
                {
                    numInARow++;
                }
                else
                {
                    numInARow = 1;
                }
                lastName = board[j][i].transform.name;
                if (numInARow >= 3)
                {
                    toClear[j-2][i] = true;
                    toClear[j-1][i] = true;
                    toClear[j][i] = true;
                }
            }
            lastName = "";
            numInARow = 1;
        }

        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                if(toClear[i][j])
                {
                    // Award more score the more slimes were matched this move
                    score+=score;
                }
            }
        }

        //  Call ClearAndDrop to clear all toClear slimes and drop new ones
        ClearAndDrop();

        return score;
    }


    //Clears all slimes flagged in toClear and replaces them
    private void ClearAndDrop()
    {
        bool loopAgain = true;
        Vector2 posToReplace;

        while(loopAgain)
        {
            loopAgain = false;
            for(int i = 7; i >=0; i--)
            {
                for(int j = 7; j >=0; j--)
                {
                    // Clear this slime
                    if(toClear[i][j])
                    {
                        //If this is not the top row
                        if(i > 0)
                        {
                            // Drop down the next slime up
                            toClear[i][j] = toClear[i - 1][j];
                            //board[i - 1][j].transform.position = board[i][j].transform.position;
                            posToReplace = board[i][j].transform.position;
                            // Check to see if the next slime up already needed clearing
                            if (toClear[i - 1][j])
                            {
                                loopAgain = true;
                            }
                            // Delete this slime
                            Destroy(board[i][j]);
                            // And replace it with a copy of the one above
                            board[i][j] = Instantiate(gl.slimes[slimeDict[board[i - 1][j].transform.name]],
                                                        posToReplace, Quaternion.identity);
                            board[i][j].GetComponent<Coordinates>().x = i;
                            board[i][j].GetComponent<Coordinates>().y = j;
                            // And set the now empty slime to clear
                            toClear[i - 1][j] = true;
                        }
                        else // If this is the top row
                        {
                            // Randomly generate a new slime here
                            posToReplace = board[i][j].transform.position;
                            Destroy(board[i][j]);
                            board[i][j] = Instantiate(gl.slimes[Random.Range(0, gl.slimes.Length)],
                                                        posToReplace, Quaternion.identity);
                            board[i][j].GetComponent<Coordinates>().x = i;
                            board[i][j].GetComponent<Coordinates>().y = j;
                            toClear[i][j] = false;
                        }
                    }
                }
            }
        }

    }

}
