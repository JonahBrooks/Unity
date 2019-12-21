using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayout : MonoBehaviour {

    // Array of slime prefabs
    public GameObject[] slimes;
    // Distance between slimes on the x axis
    public float xpadding = .1f;
    // Distance between slimes on the y axis
    public float ypadding = .1f;
    // Position of top left slime
    public float[] topLeftPos = { -4f, 3.5f };

    public float slimeWidthFactor;
    public float slimeHeightFactor;
    public float slimeDepthFactor;

    [HideInInspector]
    public float slimeWidth;
    [HideInInspector]
    public float slimeHeight;

    // Instantiates and returns a new slime of given color at board position x,y
    // A color index of -1 indicates the slime should be a random color
    public GameObject InstantiateNewSlime(int colorIndex, int x, int y)
    {
        // If the slime should be a random color
        if (colorIndex == -1)
        {
            // Generate the index at random
            colorIndex = Random.Range(0, slimes.Length);
        }

        GameObject toReturn = Instantiate(slimes[colorIndex],
                                          GetPositionFromBoardIndex(x, y),
                                          Quaternion.identity);
        toReturn.transform.Rotate(new Vector3(-90, 0, 180));
        toReturn.transform.localScale = new Vector3(toReturn.transform.localScale.x * slimeWidthFactor,
            toReturn.transform.localScale.y * slimeHeightFactor,
            toReturn.transform.localScale.z * slimeDepthFactor);
        toReturn.GetComponent<Coordinates>().x = x;
        toReturn.GetComponent<Coordinates>().y = y;
        toReturn.GetComponent<Coordinates>().z = 0;

        return toReturn;
    }

    // Calculates and returns the Unity position of the provided board index
    public Vector3 GetPositionFromBoardIndex(int x, int y)
    {
        return new Vector3(topLeftPos[0] + x * slimeWidth + x * xpadding,
            topLeftPos[1] - y * slimeHeight - y * ypadding,
            0);
    }

    public void NewBoard()
    {
        PuzzleController pc = gameObject.GetComponent<PuzzleController>();

        slimeWidth = slimes[0].GetComponent<Renderer>().bounds.size.x * slimeWidthFactor;
        slimeHeight= slimes[0].GetComponent<Renderer>().bounds.size.y * slimeHeightFactor;

        // Instantiate 64 slimes in a grid of 8 by 8
        for (int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                pc.board[i][j] = InstantiateNewSlime(-1, i,j);
            }
        }

    }

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
 

	}
}
