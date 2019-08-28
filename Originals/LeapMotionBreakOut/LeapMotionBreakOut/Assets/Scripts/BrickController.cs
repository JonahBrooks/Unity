using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickController : MonoBehaviour
{

    // Generates score from this brick then destroys it
    public void BreakBrick()
    {
        // Call the parent object's score increment function
        this.GetComponentInParent<BrickGridController>().IncrementScore();
        this.GetComponentInParent<BrickGridController>().numberOfBricksRemaining--;
        Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
