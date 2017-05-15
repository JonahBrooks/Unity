using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit2D hit;
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicked!");
            hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                    Vector2.zero, 0f);
            if (hit)
            {
                Debug.Log("Clicked on " + hit.transform.name);
            }
        }
    }
}
