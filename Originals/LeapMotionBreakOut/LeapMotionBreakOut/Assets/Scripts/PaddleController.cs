using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class PaddleController : MonoBehaviour
{

    public GameObject rightPaddle;
    public GameObject leftPaddle;
    public bool rightPaddleTilt;
    public bool leftPaddleTilt;
    public PaddleBounds rightPaddleBounds;
    public PaddleBounds leftPaddleBounds;
    public PaddleBounds rightLeapBounds;
    public PaddleBounds leftLeapBounds;

    public float movementSpeed;

    private Controller controller;
    private Rigidbody rightRb;
    private Rigidbody leftRb;
    private Vector3 rightNewPosition;
    private Vector3 leftNewPosition;

    [System.Serializable]
    public class PaddleBounds
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
    }

    private void Awake()
    {
        controller = new Controller();
    }

    // Start is called before the first frame update
    void Start()
    {
        rightRb = rightPaddle.GetComponent<Rigidbody>();
        leftRb = leftPaddle.GetComponent<Rigidbody>();
        rightNewPosition = rightPaddle.transform.position;
        leftNewPosition = leftPaddle.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Get hands
        List<Hand> rawHands = controller.Frame().Hands;

        //Debug.Log(rawHands.Count);

        foreach(Hand hand in rawHands)
        {
            // TODO: Find some way to ignore other people's hands
            if (hand.IsRight)
            {
                UpdatePaddleNewPosition(rightPaddle, hand, rightLeapBounds, rightPaddleBounds, out rightNewPosition);
                // If the right paddle is allowed to tilt
                if (rightPaddleTilt)
                {
                    TiltPaddle(rightPaddle, hand);
                }
            }
            else if (hand.IsLeft)
            {
                UpdatePaddleNewPosition(leftPaddle, hand, leftLeapBounds, leftPaddleBounds, out leftNewPosition);
                // If the left paddle is allowed to tilt
                if (leftPaddleTilt)
                {
                    TiltPaddle(leftPaddle, hand);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if(rightRb.position != rightNewPosition)
        {
            rightRb.MovePosition(Vector3.MoveTowards(rightRb.position, rightNewPosition, movementSpeed * Time.deltaTime));
        }
        if (leftRb.position != leftNewPosition)
        {
            leftRb.MovePosition(Vector3.MoveTowards(leftRb.position, leftNewPosition, movementSpeed * Time.deltaTime));
        }
    }

    private void TiltPaddle(GameObject paddle, Hand hand)
    {
        Vector3 newNormal = new Vector3(-hand.PalmNormal.x, -hand.PalmNormal.y, 0); // TODO: Did I do z right?

        paddle.transform.rotation = Quaternion.LookRotation(Vector3.forward, newNormal);
    }

    // Updates the newX and newY for moving the specified paddle in relation to the specified hand and boundaries
    private void UpdatePaddleNewPosition(GameObject paddle, Hand hand, PaddleBounds leapBounds, PaddleBounds paddleBounds, out Vector3 newPosition)
    {
        float newX = hand.PalmPosition.x;
        float newY = hand.PalmPosition.y;

        // Clamp newX and newY to be within the leap bounds
        if (newY > leapBounds.Top)
        {
            newY = leapBounds.Top;
        }
        else if (newY < leapBounds.Bottom)
        {
            newY = leapBounds.Bottom;
        }
        if (newX > leapBounds.Right)
        {
            newX = leapBounds.Right;
        }
        else if (newX < leapBounds.Left)
        {
            newX = leapBounds.Left;
        }

        // Scale newX and newY to be within the paddle bounds
        newX = ((newX - leapBounds.Left) * (paddleBounds.Right - paddleBounds.Left)) / (leapBounds.Right - leapBounds.Left) + paddleBounds.Left;
        newY = ((newY - leapBounds.Bottom) * (paddleBounds.Top - paddleBounds.Bottom)) / (leapBounds.Top - leapBounds.Bottom) + paddleBounds.Bottom;

        newPosition = new Vector3(newX, newY, this.transform.position.z);
    }
    
}
