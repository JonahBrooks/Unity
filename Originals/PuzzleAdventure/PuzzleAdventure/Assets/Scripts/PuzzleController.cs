using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class PuzzleController : MonoBehaviour {

    // Stores the grid of gameobjects
    public GameObject[][] board = new GameObject[8][];
    public bool animationDelay = true;
    public Text scoreText;
    public int playerMaxHealth;
    public int slimeMaxHealth;

    // Stores for each element in board if it should be cleared
    private bool[][] toClear = new bool[8][];
    // For initializing slimes
    private GridLayout gl;
    private Dictionary<string,int> slimeDict = new Dictionary<string,int>();
    // For synchronizing the different coroutines
    private int swapping;
    private int dropping;
    private bool playersTurn;
    private bool cpuThinking;
    // For keeping track of player score
    private int score;
    private int cpuScore;
    private int slimeHealth;
    public static int maxHealth = 100;
    public static int playerHealth;
    public static bool firstRun = true;

    Transform current = null;


    // Use this for initialization
    void Start () {

        if(PuzzleController.firstRun)
        {
            PuzzleController.firstRun = false;
            PuzzleController.maxHealth = playerMaxHealth; // Beds now restore to playerMaxHealth
            PuzzleController.playerHealth = PuzzleController.maxHealth;
        }

        score = 0;
        cpuScore = 0;
        slimeHealth = slimeMaxHealth;
        swapping = 0;
        dropping = 0;
        playersTurn = true;
        cpuThinking = false;
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
        

        // Only process main loop if no slimes are in motion
        if(swapping == 0 && dropping == 0)
        {

            // Process CPU's turn
            if(!playersTurn)
            {
                // Don't enter this coroutine if it is already running
                if(!cpuThinking)
                {
                    StartCoroutine(CPUTurn());
                }
            }
            else // Process player's turn
            {
                if (Input.GetMouseButtonDown(0))
                {
                    hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                            Vector2.zero, 0f);
                    if (hit && hit.transform.tag == "Slime")
                    {
                        if(current == null)
                        {
                            current = hit.transform;
                            current.localScale = new Vector2(current.localScale.x * 1.1f, current.localScale.y * 1.1f);
                        }
                        else if (current != null)
                        {
                            if(SwapIfValid(current, hit.transform))
                            {
                                // Player's turn ends on a valid swap
                                playersTurn = false;
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
        scoreText.text = "Player Health: " + PuzzleController.playerHealth + "\t\tEnemy Health: " + slimeHealth;
        if(slimeHealth <= 0)
        {
            SceneManager.LoadScene("Adventure");
        }
        else if (PuzzleController.playerHealth <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }


    // Coroutine for processing the CPU's turn
    private IEnumerator CPUTurn()
    {
        int numMoves = 8; // Number of possible moves from each location

        int tmpScore = 0;
        int highScore = 0;

        int[] swap1 = new int[2];
        int[] swap2 = new int[2];
        int[] tmpSwap1 = new int[2];
        int[] tmpSwap2 = new int[2];

        GameObject tmp;

        cpuThinking = true;
        // Pause before making move
        yield return new WaitForSeconds(1f);
        // Find best move to make
        // Make swap in board, simulate Clear3s, swap again to undo
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                // Set tmpSwap1 to current slime at i,j
                tmpSwap1[0] = i;
                tmpSwap1[1] = j;
                for(int k = 0; k < numMoves; k++)
                {
                    // Set tmpSwap2 to slime corresponding to move number k
                    switch (k)
                    {
                        case 0:
                            // [*][ ][ ]
                            // [ ][ ][ ]
                            // [ ][ ][ ]
                            tmpSwap2[0] = i - 1;
                            tmpSwap2[1] = j - 1;
                            break;
                        case 1:
                            // [ ][*][ ]
                            // [ ][ ][ ]
                            // [ ][ ][ ]
                            tmpSwap2[0] = i - 1;
                            tmpSwap2[1] = j;
                            break;
                        case 2:
                            // [ ][ ][*]
                            // [ ][ ][ ]
                            // [ ][ ][ ]
                            tmpSwap2[0] = i - 1;
                            tmpSwap2[1] = j + 1;
                            break;
                        case 3:
                            // [ ][ ][ ]
                            // [*][ ][ ]
                            // [ ][ ][ ]
                            tmpSwap2[0] = i;
                            tmpSwap2[1] = j - 1;
                            break;
                        case 4:
                            // [ ][ ][ ]
                            // [ ][ ][*]
                            // [ ][ ][ ]
                            tmpSwap2[0] = i;
                            tmpSwap2[1] = j + 1;
                            break;
                        case 5:
                            // [ ][ ][ ]
                            // [ ][ ][ ]
                            // [*][ ][ ]
                            tmpSwap2[0] = i + 1;
                            tmpSwap2[1] = j - 1;
                            break;
                        case 6:
                            // [ ][ ][ ]
                            // [ ][ ][ ]
                            // [ ][*][ ]
                            tmpSwap2[0] = i + 1;
                            tmpSwap2[1] = j;
                            break;
                        case 7:
                            // [ ][ ][ ]
                            // [ ][ ][ ]
                            // [ ][ ][*]
                            tmpSwap2[0] = i + 1;
                            tmpSwap2[1] = j + 1;
                            break;
                    }

                    // If move is valid
                    if(tmpSwap2[0] >= 0 && tmpSwap2[0] < 8 && tmpSwap2[1] >= 0 && tmpSwap2[1] < 8)
                    {
                        // Make swap in board
                        tmp = board[i][j];
                        board[i][j] = board[tmpSwap2[0]][tmpSwap2[1]];
                        board[tmpSwap2[0]][tmpSwap2[1]] = tmp;

                        // Get score that would be generated
                        tmpScore = Clear3s(false, false, true);
                        // Store score and swaps if this is the best so far
                        if (tmpScore > highScore)
                        {
                            highScore = tmpScore;
                            swap1[0] = tmpSwap1[0];
                            swap1[1] = tmpSwap1[1];
                            swap2[0] = tmpSwap2[0];
                            swap2[1] = tmpSwap2[1];
                        }

                        // Undo swap in board
                        tmp = board[i][j];
                        board[i][j] = board[tmpSwap2[0]][tmpSwap2[1]];
                        board[tmpSwap2[0]][tmpSwap2[1]] = tmp;
                    }
                }
            }
        }
        // Make move (which calls Clear3s and tallies score for CPU
        SwapIfValid(board[swap1[0]][swap1[1]].transform, board[swap2[0]][swap2[1]].transform);
        playersTurn = true;
        cpuThinking = false;
        yield return null;
    }

 
    // Find and clear matches of 3, return score generated
    // delay indicates whether there should be a time delay during the dropping after matching
    // tallyScore indicates whether the new score generated should be added to score
    // simulate indicates whether this should impact toClear or not
    // Returns -1 if slimes are in motion and thus can't be matched right now
    private int Clear3s(bool delay = true, bool tallyScore = true, bool simulate = false)
    {
        int newScore = 0;
        int numInARow = 1;
        string lastName = "";

        // Don't match while slimes are still in motion
        if(swapping == 0 && dropping == 0)
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
                        // Award 1 point for each slime matched this turn
                        newScore++; ;
                    }
                }
            }

            // Add newScore to player score
            if(tallyScore)
            {
                // Score is tallied at the end of a turn, so the logic is reveresed here due to coroutine times
                if (!playersTurn)
                {
                    score += newScore;
                    slimeHealth -= newScore;
                }
                else
                {
                    cpuScore += newScore;
                    PuzzleController.playerHealth -= newScore;
                }
            }

            //  Call ClearAndDrop to clear all toClear slimes and drop new ones
            if(newScore > 0 && !simulate)
            {
                StartCoroutine(ClearAndDrop(delay,tallyScore));
            }
        
            if(simulate)
            {
                for( int i = 0; i < 8; i++)
                {
                    for( int j = 0; j < 8; j++)
                    {
                        toClear[i][j] = false;
                    }
                }
            }

        
            return newScore;
        }
        else
        {
            return -1;
        }
    }

    // Swaps the position of slime One and slime Two if that is a valid swap
    // Returns true if the swap was valid, false if not
    bool SwapIfValid(Transform one, Transform two)
    {
        int x = two.GetComponentInParent<Coordinates>().x;
        int y = two.GetComponentInParent<Coordinates>().y;
        int cx = one.GetComponentInParent<Coordinates>().x;
        int cy = one.GetComponentInParent<Coordinates>().y;
        Vector2 pos1;
        Vector2 pos2;
        if (x == cx - 1 || x == cx + 1 || y == cy - 1 || y == cy + 1)
        {
            // Clicked on a slime adjacent to current slime
            pos1 = one.position;
            pos2 = two.position;
            StartCoroutine(SlideSlimeTowards(one.gameObject, pos2, animationDelay));
            StartCoroutine(SlideSlimeTowards(two.gameObject, pos1, animationDelay));
            one.GetComponentInParent<Coordinates>().x = x;
            one.GetComponentInParent<Coordinates>().y = y;
            board[x][y] = one.gameObject;
            two.GetComponentInParent<Coordinates>().x = cx;
            two.GetComponentInParent<Coordinates>().y = cy;
            board[cx][cy] = two.gameObject;
            // The swap was valid
            return true;
        }
        else
        {
            // The swap was invalid
            return false;
        }
    }

    // Slowly move one slime toward a given location (if delay = true, otherwise move it immediately)
    private IEnumerator SlideSlimeTowards(GameObject slime, Vector2 pos, bool delay = true)
    {
        swapping++;
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
        swapping--;
        Clear3s(delay);
    }

    //Clears all slimes flagged in toClear and replaces them
    private IEnumerator ClearAndDrop(bool delay = true, bool tallyScore = true)
    {
        
        Vector2 posToReplace;
        Vector2 posToSlide;
        bool loopAgain = true;

        dropping++;
        // Don't drop slimes while slimes are already in motion
        if(swapping == 0)
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
                                                            new Vector2(posToReplace.x, posToReplace.y + gl.slimeHeight + gl.ypadding), 
                                                            Quaternion.identity);
                                StartCoroutine(SlideSlimeTowards(board[i][j], posToReplace, delay));
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
        dropping--;
        Clear3s(delay,tallyScore);
    }

}
