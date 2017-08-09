using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSavesButton : MonoBehaviour {

    public void OnClick()
    {
        MetalDeer.saveData = new SaveData();
        SaveData.Save(MetalDeer.saveData);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
