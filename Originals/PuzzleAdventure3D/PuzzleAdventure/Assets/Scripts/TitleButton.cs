using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TitleButton : MonoBehaviour {

    public void OnClick()
    {
        PuzzleController.firstRun = true;
        AdventureController.firstRun = true;
        SceneManager.LoadScene("Adventure");
    }
	
}
