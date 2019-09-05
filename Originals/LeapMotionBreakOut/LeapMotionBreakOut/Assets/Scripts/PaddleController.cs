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
    public PaddleBounds rightRelativeKinectBounds;
    public PaddleBounds leftRelativeKinectBounds;
    
    public float movementSpeed;
    public float rotationSpeed;

    private Controller controller;
    private LeapProvider leapProvider;
    private Rigidbody rightRb;
    private Rigidbody leftRb;
    private KinectWrapper.NuiSkeletonPositionIndex handRight = KinectWrapper.NuiSkeletonPositionIndex.HandRight;
    private KinectWrapper.NuiSkeletonPositionIndex handLeft = KinectWrapper.NuiSkeletonPositionIndex.HandLeft;
    private KinectWrapper.NuiSkeletonPositionIndex wristRight = KinectWrapper.NuiSkeletonPositionIndex.WristRight;
    private KinectWrapper.NuiSkeletonPositionIndex wristLeft = KinectWrapper.NuiSkeletonPositionIndex.WristLeft;
    private KinectWrapper.NuiSkeletonPositionIndex shoulderCenter = KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter;
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
        OnFixedFrame,
        Kinect
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

        if(movementStyle == MovementStyleEnum.Kinect)
        {
            KinectManager manager = KinectManager.Instance;

            // Make sure the manager is set up and a user is detected
            if(manager && manager.IsInitialized() && manager.IsUserDetected())
            {
                uint playerID = manager.GetPlayer1ID();

                // Handle right hand
                if(manager.IsJointTracked(playerID, (int)handRight))
                {
                    // Calculate the position of the right hand in respect to the shoulderCenter position
                    Vector3 relativePosition = manager.GetJointPosition(playerID, (int)handRight) - manager.GetJointPosition(playerID, (int)shoulderCenter);
                    UpdatePaddleNewPosition(rightPaddle, relativePosition, rightRelativeKinectBounds, rightPaddleBounds, out rightNewPosition);
                    // If the right paddle is allowed to tilt
                    if (rightPaddleTilt)
                    {
                        Vector3 handPos = manager.GetJointPosition(playerID, (int)handRight);
                        Vector3 wristPos = manager.GetJointPosition(playerID, (int)wristRight);

                        Vector3 handNormal = (handPos - wristPos).normalized;
                        UpdatePaddleNewRotation(rightPaddle, handNormal, out rightNewRotation);
                    }
                }

                // Handle left hand
                if (manager.IsJointTracked(playerID, (int)handLeft))
                {
                    // Calculate the position of the left hand in respect to the shoulderCenter position
                    Vector3 relativePosition = manager.GetJointPosition(playerID, (int)handLeft) - manager.GetJointPosition(playerID, (int)shoulderCenter);
                    Debug.Log(relativePosition);
                    UpdatePaddleNewPosition(leftPaddle, relativePosition, leftRelativeKinectBounds, leftPaddleBounds, out leftNewPosition);
                    // If the left paddle is allowed to tilt
                    if (leftPaddleTilt)
                    {
                        Vector3 handPos = manager.GetJointPosition(playerID, (int)handLeft);
                        Vector3 wristPos = manager.GetJointPosition(playerID, (int)wristLeft);

                        Vector3 handNormal = (handPos - wristPos).normalized;
                        UpdatePaddleNewRotation(leftPaddle, handNormal, out leftNewRotation);
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

    // Adjusts the tilt of the paddles based on hand tilt
    // Override for use with Kinect sensor
    private void UpdatePaddleNewRotation(GameObject paddle, Vector3 handNormal, out Quaternion paddleNewRotation)
    {
        Vector3 newNormal = new Vector3(-handNormal.x, -handNormal.y, 0);

        paddleNewRotation = Quaternion.LookRotation(Vector3.forward, newNormal);
    }

    // Updates the newX and newY for moving the specified paddle in relation to the specified hand and boundaries
    // Override for use with Kinect sensor
    private void UpdatePaddleNewPosition(GameObject paddle, Vector3 handPosition, PaddleBounds kinectBounds, PaddleBounds paddleBounds, out Vector3 newPosition)
    {
        float newX = handPosition.x;
        float newY = handPosition.y;

        // Clamp newX and newY to be within the kinect bounds
        if (newY > kinectBounds.Top)
        {
            newY = kinectBounds.Top;
        }
        else if (newY < kinectBounds.Bottom)
        {
            newY = kinectBounds.Bottom;
        }
        if (newX > kinectBounds.Right)
        {
            newX = kinectBounds.Right;
        }
        else if (newX < kinectBounds.Left)
        {
            newX = kinectBounds.Left;
        }

        // Scale newX and newY to be within the paddle bounds
        newX = ((newX - kinectBounds.Left) * (paddleBounds.Right - paddleBounds.Left)) / (kinectBounds.Right - kinectBounds.Left) + paddleBounds.Left;
        newY = ((newY - kinectBounds.Bottom) * (paddleBounds.Top - paddleBounds.Bottom)) / (kinectBounds.Top - kinectBounds.Bottom) + paddleBounds.Bottom;

        newPosition = new Vector3(newX, newY, this.transform.position.z);
    }

}
