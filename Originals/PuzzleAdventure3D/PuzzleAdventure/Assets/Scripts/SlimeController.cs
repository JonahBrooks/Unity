using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [HideInInspector]
    public bool inHouseTrigger = false;
    [HideInInspector]
    public int index;

    public float minDelayBetweenAnimations;
    public float maxDelayBetweenAnimations;

    private float delayUntilAnimation; 
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        delayUntilAnimation = Random.Range(minDelayBetweenAnimations, maxDelayBetweenAnimations);
    }
    
    // Update is called once per frame
    void Update()
    {
        // Look at the player character
        Vector3 target = playerTransform.position;
        target = new Vector3(target.x, 0, target.z);
        transform.LookAt(target);

        // Trigger animations
        Animator animator = transform.gameObject.GetComponent<Animator>();
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            // Count down until the next animation
            delayUntilAnimation -= Time.deltaTime;
            if (delayUntilAnimation <= 0)
            {
                // Randomly pick an animation to play
                float randomSelection = Random.Range(0f, 1f);
                if(randomSelection <= 0.5f)
                {
                    animator.SetTrigger("WiggleTrigger");
                }
                else
                {
                    animator.SetTrigger("JumpTrigger");
                }
                // Reset the clock between animations
                delayUntilAnimation = Random.Range(minDelayBetweenAnimations, maxDelayBetweenAnimations);
            }
        }
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
