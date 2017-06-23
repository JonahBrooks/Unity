using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TitleButton : MonoBehaviour {

    public void OnClick()
    {
        PuzzleController.firstRun = true;
        PlayerMover.firstRun = true;
        SceneManager.LoadScene("Adventure");
    }
	
}
