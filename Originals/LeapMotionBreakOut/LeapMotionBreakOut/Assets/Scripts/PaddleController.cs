using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class PaddleController : MonoBehaviour
{

    public GameObject rightPaddle;
    public GameObject leftPaddle;
    public bool rightPaddleTilt;
    public bool leftPaddleTilt;
    public MovementStyleEnum movementStyle;
    public PaddleBounds rightPaddleBounds;
    public PaddleBounds leftPaddleBounds;
    public PaddleBounds rightLeapBounds;
    public PaddleBounds leftLeapBounds;
    public PaddleBounds rightLeapProviderBounds;
    public PaddleBounds leftLeapProviderBounds;

    public float movementSpeed;
    public float rotationSpeed;

    private Controller controller;
    private LeapProvider leapProvider;
    private Rigidbody rightRb;
    private Rigidbody leftRb;
    private Vector3 rightNewPosition;
    private Vector3 leftNewPosition;
    private Quaternion rightNewRotation;
    private Quaternion leftNewRotation;

    [System.Serializable]
    public class PaddleBounds
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
    }

    public enum MovementStyleEnum
    {
        LeapProvider,
        Controller,
        OnFixedFrame
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
        rightNewRotation = rightPaddle.transform.rotation;
        leftNewRotation = leftPaddle.transform.rotation;
        leapProvider = FindObjectOfType<LeapProvider>() as LeapProvider;
        if (movementStyle == MovementStyleEnum.OnFixedFrame)
        {
            leapProvider.OnUpdateFrame -= OnUpdateFrame;
            leapProvider.OnUpdateFrame += OnUpdateFrame;
            leapProvider.OnFixedFrame -= OnFixedFrame;
            leapProvider.OnFixedFrame += OnFixedFrame;
        }
    }

    private void OnEnable()
    {
        
    }

    protected virtual void OnUpdateFrame(Frame frame)
    {

    }

    protected virtual void OnFixedFrame(Frame frame)
    {
        foreach(Hand hand in frame.Hands)
        {
            if(hand.IsRight)
            {
                UpdatePaddleNewPosition(rightPaddle, hand, rightLeapProviderBounds, rightPaddleBounds, out rightNewPosition);
                if (rightPaddleTilt)
                {
                    UpdatePaddleNewRotation(rightPaddle, hand, out rightNewRotation);
                }
            }
            else if (hand.IsLeft)
            {
                UpdatePaddleNewPosition(leftPaddle, hand, leftLeapProviderBounds, leftPaddleBounds, out leftNewPosition);
                if (leftPaddleTilt)
                {
                    UpdatePaddleNewRotation(leftPaddle, hand, out leftNewRotation);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get hands

        // Old style
        if (movementStyle == MovementStyleEnum.Controller)
        {
            List<Hand> rawHands = controller.Frame().Hands;
            foreach (Hand hand in rawHands)
            {
                if (hand.IsRight)
                {
                    UpdatePaddleNewPosition(rightPaddle, hand, rightLeapBounds, rightPaddleBounds, out rightNewPosition);
                    // If the right paddle is allowed to tilt
                    if (rightPaddleTilt)
                    {
                        UpdatePaddleNewRotation(rightPaddle, hand, out rightNewRotation);
                    }
                }
                else if (hand.IsLeft)
                {
                    UpdatePaddleNewPosition(leftPaddle, hand, leftLeapBounds, leftPaddleBounds, out leftNewPosition);
                    // If the left paddle is allowed to tilt
                    if (leftPaddleTilt)
                    {
                        UpdatePaddleNewRotation(leftPaddle, hand, out leftNewRotation);
                    }
                }
            }
        }

        // New style
        if (movementStyle == MovementStyleEnum.LeapProvider)
        {
            Frame frame = leapProvider.CurrentFrame;
            List<Hand> rawHands = frame.Hands;

            foreach (Hand hand in rawHands)
            {
                if (hand.IsRight)
                {
                    UpdatePaddleNewPosition(rightPaddle, hand, rightLeapProviderBounds, rightPaddleBounds, out rightNewPosition);
                    // If the right paddle is allowed to tilt
                    if (rightPaddleTilt)
                    {
                        UpdatePaddleNewRotation(rightPaddle, hand, out rightNewRotation);
                    }
                }
                else if (hand.IsLeft)
                {
                    UpdatePaddleNewPosition(leftPaddle, hand, leftLeapProviderBounds, leftPaddleBounds, out leftNewPosition);
                    // If the left paddle is allowed to tilt
                    if (leftPaddleTilt)
                    {
                        UpdatePaddleNewRotation(leftPaddle, hand, out leftNewRotation);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // Move the paddles into position if the players hands have moved
        if(rightRb.position != rightNewPosition || rightRb.rotation != rightNewRotation)
        {
            rightRb.MovePosition(Vector3.MoveTowards(rightRb.position, rightNewPosition, movementSpeed * Time.deltaTime));
            rightRb.MoveRotation(Quaternion.RotateTowards(rightRb.rotation, rightNewRotation, rotationSpeed * Time.deltaTime));
        }
        if (leftRb.position != leftNewPosition || leftRb.rotation != leftNewRotation)
        {
            leftRb.MovePosition(Vector3.MoveTowards(leftRb.position, leftNewPosition, movementSpeed * Time.deltaTime));
            leftRb.MoveRotation(Quaternion.RotateTowards(leftRb.rotation, leftNewRotation, rotationSpeed * Time.deltaTime));
        }
    }

    // Adjusts the tilt of the paddles based on hand tilt
    private void UpdatePaddleNewRotation(GameObject paddle, Hand hand, out Quaternion paddleNewRotation)
    {
        Vector3 newNormal = new Vector3(-hand.PalmNormal.x, -hand.PalmNormal.y, 0);

        paddleNewRotation = Quaternion.LookRotation(Vector3.forward, newNormal);
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
