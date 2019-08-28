using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickGridController : MonoBehaviour
{
    public int numberOfBricksRemaining { get; set; }
    public int score { get; set; }

    // Clears all bricks from the play field; does not generate score
    public void ClearAllBricks()
    {
        // Iterate over all child objects (bricks) and destroy them
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        numberOfBricksRemaining = 0;
    }

    // Increments the score stored in this object; that is the total score for the game so far
    public void IncrementScore()
    {
        score++;
        // Tell the board controller that the score has changed
        this.transform.GetComponentInParent<BoardController>().UpdateUI();
    }

    // Resets the score stored in this object back to zero
    public void ResetScore()
    {
        score = 0;
        // Tell the board controller that the score has changed
        this.transform.GetComponentInParent<BoardController>().UpdateUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(numberOfBricksRemaining == 0)
        {
            // Tell the board controller that the grid has been cleared
            this.transform.GetComponentInParent<BoardController>().BoardCleared();
        }
    }
}
