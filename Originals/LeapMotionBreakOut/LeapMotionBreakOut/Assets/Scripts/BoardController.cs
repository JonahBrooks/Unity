using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{

    public GameObject brickPrefab;
    public BrickGridBounds brickGridBounds;
    public float xBuffer;
    public float yBuffer;
    public int initialNumberOfBricks;

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
        if(initialNumberOfBricks <= 0)
        {
            // Instantiate as many bricks as possible
            LayoutBricks(int.MaxValue);
        }
        else
        {
            // Instantiate as many bricks as was requested
            LayoutBricks(initialNumberOfBricks);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Instantiates brick prefabs to populate the brick grid
    private void LayoutBricks(int numberOfBricks)
    {
        for (int i = 0; i < numberOfBricks; i++)
        {
            int numberOfBricksInX = Mathf.FloorToInt((brickGridBounds.Right - brickGridBounds.Left) / (brickPrefab.transform.localScale.x+xBuffer));
            int numberOfBricksInY = Mathf.FloorToInt((brickGridBounds.Top - brickGridBounds.Bottom) / (brickPrefab.transform.localScale.y+yBuffer));
                
            int x = i % numberOfBricksInX;
            int y = i / numberOfBricksInX;

            if (y > numberOfBricksInY)
            {
                break; // Stop adding bricks if this is starting a new row beyond the height limit
            }

            float xPositionOfBrick = brickGridBounds.Left + (x+0.5f) * brickPrefab.transform.localScale.x + x*xBuffer;
            float yPositionOfBrick = brickGridBounds.Top - (y+0.5f) * brickPrefab.transform.localScale.y - y*yBuffer;

            Instantiate(brickPrefab, new Vector3(xPositionOfBrick, yPositionOfBrick, this.transform.position.z), Quaternion.identity);
        }
    }

}
