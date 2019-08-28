using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardController : MonoBehaviour
{

    public Text scoreText;
    public Text highScoreText;
    public Text clearText;
    public Text outText;
    public Text countdownText;
    public float textDisplayTime;
    public float brickLayoutDelay;
    public GameObject ball;
    public GameObject brickPrefab;
    public GameObject brickParentObject;
    public BrickGridBounds brickGridBounds;
    public float xBuffer;
    public float yBuffer;
    public int initialNumberOfBricks;
    public int bonusScoreForClears;

    private BrickGridController brickGridController;
    private BallController ballController;
    private int currentScore;
    private int highScore;
    private bool layingOutBricks;

    [System.Serializable]
    public class BrickGridBounds
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Don't show clear, out, or countdown text until the right time
        clearText.enabled = false;
        outText.enabled = false;
        countdownText.enabled = false;

        brickGridController = brickParentObject.GetComponent<BrickGridController>();
        ballController = ball.GetComponent<BallController>();
        currentScore = 0;
        highScore = 0;
        layingOutBricks = false;

       // Instantiate the requested number of bricks
        StartCoroutine(LayoutBricks(initialNumberOfBricks));
    
        // Countdown before the ball goes into play
        StartCoroutine(InitialCountdownCoroutine());

    }

    private IEnumerator InitialCountdownCoroutine()
    {
        ballController.ResetBall();
        ballController.PauseBall();
        while(layingOutBricks)
        {
            yield return new WaitForSeconds(0.1f);
        }
        // Count down from 3
        countdownText.enabled = true;
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.enabled = false;
        ballController.UnpauseBall();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

    }

    // Update the UI; is triggered from BrickGridController when the score changes
    public void UpdateUI()
    {
        currentScore = brickGridController.score;
        scoreText.text = "Score: " + currentScore.ToString();
        if (highScore < currentScore)
        {
            highScore = currentScore;
            highScoreText.text = "Session High Score: " + highScore.ToString();
        }
    }


    // Triggered when numberOfBricksRemaining hits 0
    public void BoardCleared()
    {
        // Don't trigger this on the ClearAllBricks call that LayoutBricks makes
        if(!layingOutBricks)
        {
            StartCoroutine(BoardClearedCoroutine());
            StartCoroutine(LayoutBricks(initialNumberOfBricks));
        }
    }

    // A coroutine for displaying "CLEAR!", then reseting the ball, then counting down, then unpausing the ball
    private IEnumerator BoardClearedCoroutine()
    {
        ballController.ResetBall();
        ballController.PauseBall();
        brickGridController.score += bonusScoreForClears;
        UpdateUI();
        
        // Display "CLEAR!" on the screen
        clearText.enabled = true;
        yield return new WaitForSeconds(textDisplayTime);
        clearText.enabled = false;

        // Wait for LayoutBricks to end before counting down
        while(layingOutBricks)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Count down from 3
        countdownText.enabled = true;
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.enabled = false;
        ballController.UnpauseBall();
    }

    // Triggered when the ball hits the bottom of the screen; resets the score, the ball, and the bricks
    public void BallOutOfBounds()
    {
        StartCoroutine(BallOutOfBoundsCoroutine());
        StartCoroutine(LayoutBricks(initialNumberOfBricks));
     
    }

    // A coroutine for displaying "OUT!", then reseting the ball, then counting down, then unpausing the ball
    private IEnumerator BallOutOfBoundsCoroutine()
    {
        ballController.ResetBall();
        ballController.PauseBall();
        brickGridController.ResetScore();
        UpdateUI();

        // Display "OUT!" on the screen
        outText.enabled = true;
        yield return new WaitForSeconds(textDisplayTime);
        outText.enabled = false;

        // Wait for LayoutBricks to end before counting down
        while (layingOutBricks)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Count down from 3
        countdownText.enabled = true;
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.enabled = false;
        ballController.UnpauseBall();
    }

    // Uses the Fisher-Yates shuffle algorithm to shuffle the list
    private static void Shuffle(ref List<int> list)
    {
        for (int i = list.Count; i > 0; i--)
        {
            int k = Random.Range(0, i);
            int temp = list[k];
            list[k] = list[i - 1];
            list[i - 1] = temp;
        }
    }

    // Instantiates brick prefabs to populate the brick grid; clears any existing bricks prior to generating new ones
    private IEnumerator LayoutBricks(int numberOfBricks)
    {
        layingOutBricks = true; // Make other coroutines wait before countdown

        if(numberOfBricks <= 0)
        {
            numberOfBricks = int.MaxValue;
        }

        // Get rid of any bricks currently on the field
        brickGridController.ClearAllBricks();

        // Calculate statistics for this brick grid
        int maxNumberOfBricksInX = Mathf.FloorToInt((brickGridBounds.Right - brickGridBounds.Left) / (brickPrefab.transform.localScale.x + xBuffer));
        int maxNumberOfBricksInY = Mathf.FloorToInt((brickGridBounds.Top - brickGridBounds.Bottom) / (brickPrefab.transform.localScale.y + yBuffer));
        int maxNumberOfBricks = maxNumberOfBricksInX * maxNumberOfBricksInY;

        // Prepare the list of indexes
        List<int> listOfIndexes = Enumerable.Range(0, maxNumberOfBricks).ToList();
        // Randomize the order of the indexes so the bricks appear in random locations within the grid
        Shuffle(ref listOfIndexes);

        // Generate bricks in the order specified in listOfIndexes
        int numberOfBricksLayedOut = 0;
        for (int i = 0; i < numberOfBricks && i < maxNumberOfBricks; i++)
        {
            // Get the next index in line
            int index = listOfIndexes[i];

            // Calculate the x,y coordinate of this index in the grid
            int x = index % maxNumberOfBricksInX;
            int y = index / maxNumberOfBricksInX;

            // Find the unity x,y coordinates coresponding to this x,y grid position
            float xPositionOfBrick = brickGridBounds.Left + (x + 0.5f) * brickPrefab.transform.localScale.x + x * xBuffer;
            float yPositionOfBrick = brickGridBounds.Top - (y + 0.5f) * brickPrefab.transform.localScale.y - y * yBuffer;

            // Instantiate the object and parent it to the brickParentObject
            GameObject brick = Instantiate(brickPrefab, new Vector3(xPositionOfBrick, yPositionOfBrick, this.transform.position.z), Quaternion.identity);
            brick.transform.parent = brickParentObject.transform;

            numberOfBricksLayedOut++;

            yield return new WaitForSeconds(brickLayoutDelay);
        }
        brickGridController.numberOfBricksRemaining = numberOfBricksLayedOut;

        layingOutBricks = false; // Let the other coroutines finish
    }

}
