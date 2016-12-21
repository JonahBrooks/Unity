using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour {

    // Matrix for storing pieces
    private bool[,] board = new bool[16,16];

    // TODO: Function for trying to set a piece: Return success or fail. Place piece if success.

	// Use this for initialization
	void Start () {
		for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                board[i,j] = false; // Set board to empty
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
