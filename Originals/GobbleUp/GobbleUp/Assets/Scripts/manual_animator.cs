using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manual_animator : MonoBehaviour {

    public GameObject[] frames;
    public double animationModifier = 1.0;

    private double frameTime = .01;
    private double timer;
    private double relativeTimer;
    private Rigidbody rb;
    private int activeIndex = 0;

	// Use this for initialization
	void Start () {
        relativeTimer = frameTime;
        timer = frameTime;
        rb = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        if(rb.velocity.sqrMagnitude > 0)
        {
            relativeTimer = animationModifier / rb.velocity.sqrMagnitude;
        }
        timer += relativeTimer - frameTime;
        frameTime = relativeTimer;
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
