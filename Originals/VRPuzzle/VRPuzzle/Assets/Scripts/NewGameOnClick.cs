using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameOnClick : MonoBehaviour {

    public void LaunchGame()
    {
        PlayerPrefs.SetInt("Hard", 0);
        SceneManager.LoadScene(1);
    }

    public void LaunchGameHard()
    {
        PlayerPrefs.SetInt("Hard", 1);
        SceneManager.LoadScene(1);
    }

}
