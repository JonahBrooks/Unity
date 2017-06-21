using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TitleButton : MonoBehaviour {

    public void OnClick()
    {
        SceneManager.LoadScene("Adventure");
    }
	
}
