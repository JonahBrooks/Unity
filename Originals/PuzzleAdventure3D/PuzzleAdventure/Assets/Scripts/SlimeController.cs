using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [HideInInspector]
    public bool inHouseTrigger = false;
    [HideInInspector]
    public int index;

    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector3 target = playerTransform.position;
        target = new Vector3(target.x, 0, target.z);
        transform.LookAt(target);
        transform.Rotate(new Vector3(-90, 0, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("House"))
        {
            inHouseTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("House"))
        {
            inHouseTrigger = false;
        }
    }
}
