using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class PuzzleController : MonoBehaviour {

    // Stores the grid of gameobjects
    public GameObject[][] board = new GameObject[8][];
    public bool animationDelay = true;

    // Stores for each element in board if it should be cleared
    private bool[][] toClear = new bool[8][];
    // For initializing slimes
    private GridLayout gl;
    private Dictionary<string,int> slimeDict = new Dictionary<string,int>();
    // For synchronizing the two different coroutines
    private bool swapping;
    private bool dropping;
    // For keeping track of player score
    private int score;

    Transform current = null;


    // Use this for initialization
    void Start () {
        score = 0;
        swapping = false;
        dropping = false;
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
        // Destroy any sets of three on the board at the start
        Clear3s(false, false);
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit2D hit;
        int x;
        int y;
        int cx;
        int cy;
        Vector2 currentPos;
        Vector2 hitPos;

        Debug.Log("Score: " + score);
        // Only process main loop if no slimes are in motion
        if(!swapping && !dropping)
        {
            if (Input.GetMouseButtonDown(0))
            {
                hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                        Vector2.zero, 0f);
                if (hit && hit.transform.tag == "Slime")
                {
                    x = hit.transform.GetComponentInParent<Coordinates>().x;
                    y = hit.transform.GetComponentInParent<Coordinates>().y;
                    if(current == null)
                    {
                        current = hit.transform;
                        current.localScale = new Vector2(current.localScale.x * 1.1f, current.localScale.y * 1.1f);
                    }
                    else if (current != null)
                    {
                        cx = current.GetComponentInParent<Coordinates>().x;
                        cy = current.GetComponentInParent<Coordinates>().y;

                        if (x == cx - 1 || x == cx + 1 || y == cy - 1 || y == cy + 1)
                        {
                            // Clicked on a slime adjacent to current slime
                            currentPos = current.position;
                            hitPos = hit.transform.position;
                            StartCoroutine(SlideSlimeTowards(current.gameObject, hitPos, animationDelay));
                            StartCoroutine(SlideSlimeTowards(hit.transform.gameObject, currentPos, animationDelay));
                            current.GetComponentInParent<Coordinates>().x = x;
                            current.GetComponentInParent<Coordinates>().y = y;
                            board[x][y] = current.gameObject;
                            hit.transform.GetComponentInParent<Coordinates>().x = cx;
                            hit.transform.GetComponentInParent<Coordinates>().y = cy;
                            board[cx][cy] = hit.transform.gameObject;
                            
                        }
                        // Either way, deselect current
                        current.localScale = new Vector2(current.localScale.x / 1.1f, current.localScale.y / 1.1f);
                        current = null;
                    
                    }
                } else
                {
                    // Clicked on something other than a slime
                    // Deselect current
                    if(current != null)
                    {
                        current.localScale = new Vector2(current.localScale.x / 1.1f, current.localScale.y / 1.1f);
                        current = null;
                    }
                }
            }
        }
    }

 
    // Find and clear matches of 3, return score generated
    // Returns -1 if slimes are in motion
    private int Clear3s(bool delay = true, bool tallyScore = true)
    {
        int newScore = 0;
        int numInARow = 1;
        string lastName = "";

        // Don't match while slimes are still in motion
        if(!swapping && !dropping)
        {
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
                        if(newScore == 0)
                        {
                            newScore = 1;
                        }
                        newScore += newScore;
                    }
                }
            }

            // Add newScore to player score
            if(tallyScore)
            {
                score += newScore;
            }

            //  Call ClearAndDrop to clear all toClear slimes and drop new ones
            if(newScore > 0)
            {
                if (delay)
                {
                    StartCoroutine(DelayedClearAndDrop(tallyScore));
                }
                else
                {
                    StartCoroutine(ClearAndDrop(false,tallyScore));
                }
            }
        
        
            return newScore;
        }
        else
        {
            return -1;
        }
    }

    private IEnumerator SlideSlimeTowards(GameObject slime, Vector2 pos, bool delay = true)
    {
        swapping = true;
        if(delay)
        {
            // Move the slime a little bit each frame until it's in the right position
            while (new Vector2(slime.transform.position.x, slime.transform.position.y) != pos)
            {
                slime.transform.position = Vector2.MoveTowards(new Vector2(slime.transform.position.x,
                                                                            slime.transform.position.y), 
                                                                pos, 3 * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            // No delay, just move the slime
            slime.transform.position = pos;
        }
        swapping = false;
        Clear3s(animationDelay);
    }

    private IEnumerator DelayedClearAndDrop(bool tallyScore = true)
    {
        // Start ClearAndDrop
        yield return StartCoroutine(ClearAndDrop(true, tallyScore));
        
    }

    //Clears all slimes flagged in toClear and replaces them
    private IEnumerator ClearAndDrop(bool delay = true, bool tallyScore = true)
    {
        
        Vector2 posToReplace;
        Vector2 posToSlide;
        bool loopAgain = true;

        dropping = true;
        // Don't drop slimes while slimes are already in motion
        if(!swapping)
        {
            while (loopAgain)
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
                                posToReplace = board[i-1][j].transform.position;
                                posToSlide = board[i][j].transform.position;
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
                                StartCoroutine(SlideSlimeTowards(board[i][j], posToSlide, delay));
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
                if(delay)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return null;
                }
            }
        }
        dropping = false;
        Clear3s(delay,tallyScore);
    }

}
