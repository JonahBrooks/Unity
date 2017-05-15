using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manual_animator : MonoBehaviour {

    public GameObject[] frames;
    public double frameTime = .01;

    private double timer;
    private int activeIndex = 0;

	// Use this for initialization
	void Start () {
        timer = frameTime;
	}
	
	// Update is called once per frame
	void Update () {
        // Count to see when the next frame of animation needs to be played
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            timer = frameTime;
            // Advance the frame of animation
            activeIndex++;
            activeIndex = activeIndex % frames.Length;
        }
        for (int i = 0; i < frames.Length; i++)
        {
            if (i != activeIndex)
            {
                frames[i].SetActive(false);
            }
            else
            {
                frames[i].SetActive(true);
            }
        }
    }
}
