using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CameraScript : MonoBehaviour {

    public GameObject player;

    //private Vector3 offset;
    //private Vector3 rotate;
	
    // Use this for initialization
	void Start () {
        //offset = transform.position - player.transform.position;
    }


    private void FixedUpdate()
    {
        //float moveHorizontal = Input.GetAxis("Horizontal");
        //float moveVertical = Input.GetAxis("Vertical");
        
    }

    // LateUpdate is called after every other update
    void LateUpdate () {
        //transform.position = player.transform.position + offset;
	}
}
