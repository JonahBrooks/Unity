using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float ballSpeed;
    public Vector3 initialTrajectory;

    private Vector3 currentTrajectory;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        currentTrajectory = initialTrajectory.normalized;
        rb = GetComponent<Rigidbody>();
        rb.velocity = currentTrajectory * ballSpeed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {

    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Collision!");

        if(collision.transform.CompareTag("Brick"))
        {
            Destroy(collision.gameObject);
        }

        // Don't allow the ball to move faster or slower than its given ball speed
        rb.velocity = rb.velocity.normalized * ballSpeed;
    }
}

