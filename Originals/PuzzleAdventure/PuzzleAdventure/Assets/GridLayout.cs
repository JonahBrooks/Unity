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

    private float slimeWidth;
    private float slimeHeight;

    public void NewBoard()
    {
        int randomIndex;
        float slimex = topLeftPos[0];
        float slimey = topLeftPos[1];
        Vector3 slimepos;

        slimeWidth = slimes[0].GetComponent<SpriteRenderer>().bounds.size.x;
        slimeHeight = slimes[0].GetComponent<SpriteRenderer>().bounds.size.y;

        // Instantiate 64 slimes in a grid of 8 by 8
        for (int i = 0; i < 8; i++)
        {
            slimex = topLeftPos[0];
            slimey = topLeftPos[1] - i * slimeHeight - i * ypadding;
            for(int j = 0; j < 8; j++)
            {
                randomIndex = Random.Range(0, slimes.Length);
                slimex = topLeftPos[0] + j * slimeWidth + j * xpadding;
                slimepos = new Vector3(slimex, slimey, 0);
                Instantiate(slimes[randomIndex], slimepos, Quaternion.identity);
            }
        }

    }

	// Use this for initialization
	void Start () {
        NewBoard();
	}
	
	// Update is called once per frame
	void Update () {
 

	}
}
