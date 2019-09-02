using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float ballSpeed;
    public Vector3 initialTrajectory;
    public Vector3 initialPosition;

    private Rigidbody rb;
    private Vector3 pausedVelocity;
    private Vector3 lastPosition;

    public void ResetBall()
    {
        transform.position = initialPosition;
        rb.velocity = initialTrajectory.normalized * ballSpeed;
        lastPosition = initialPosition;
    }

    public void PauseBall()
    {
        pausedVelocity = rb.velocity;
        rb.isKinematic = true;
    }

    public void UnpauseBall()
    {
        rb.isKinematic = false;
        rb.velocity = pausedVelocity;
    }

    public Vector3 GetBallPosition()
    {
        return rb.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pausedVelocity = initialTrajectory.normalized * ballSpeed;
        ResetBall();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        // Don't allow the ball to move faster or slower than its given ball speed
        rb.velocity = rb.velocity.normalized * ballSpeed;

        // Ignore everything in the 2nd layer (Ignore Raycast)
        int layerMask = 1 << 2;
        layerMask = ~layerMask;

        // Calculate the vector of movement since last frame
        Vector3 movement = rb.position - lastPosition;

        RaycastHit hit;
        // If the ball passed through something it should have hit
        if(Physics.Raycast(lastPosition,movement, out hit, movement.magnitude, layerMask))
        {
            // Move the ball to the point where it would have collided with something
            rb.position = hit.point - (movement / movement.magnitude) * GetComponent<SphereCollider>().radius;
        }

        // Update lastPosition for use next frame
        lastPosition = rb.position;
    }

    private void OnCollisionExit(Collision collision)
    {
        // If it hit a brick
        if(collision.transform.CompareTag("Brick"))
        {
            // Tell this brick to generate score and become destroyed
            collision.gameObject.GetComponent<BrickController>().BreakBrick();
        }

        // If it hit the bottom of the screen
        if(collision.transform.CompareTag("Bottom"))
        {
            // Tell the board controller about the ball hitting the bottom of the screen
            this.transform.GetComponentInParent<BoardController>().BallOutOfBounds();
        }

    }
}

