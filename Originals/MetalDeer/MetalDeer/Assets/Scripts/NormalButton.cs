using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NormalButton : MonoBehaviour {

    public void OnClick()
    {
        MetalDeer.difficulty = 50;
        SceneManager.LoadScene("MetalDeer");
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
