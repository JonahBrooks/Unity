using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class PaddleController : MonoBehaviour
{

    public GameObject rightPaddle;
    public GameObject leftPaddle;
    public PaddleBounds rightPaddleBounds;
    public PaddleBounds leftPaddleBounds;
    public PaddleBounds rightLeapBounds;
    public PaddleBounds leftLeapBounds;

    private Controller controller;

    [System.Serializable]
    public class PaddleBounds
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = new Controller();
    }

    // Update is called once per frame
    void Update()
    {
        // Get hands
        List<Hand> rawHands = controller.Frame().Hands;

        Debug.Log(rawHands.Count);

        foreach(Hand hand in rawHands)
        {
            // TODO: Find some way to ignore other people's hands
            if (hand.IsRight)
            {
                MovePaddle(rightPaddle, hand, rightLeapBounds, rightPaddleBounds);
            }
            else if (hand.IsLeft)
            {
                MovePaddle(leftPaddle, hand, leftLeapBounds, leftPaddleBounds);
            }
        }
    }

    // Moves the specified paddle in relation to the specified hand and boundaries
    private void MovePaddle(GameObject paddle, Hand hand, PaddleBounds leapBounds, PaddleBounds paddleBounds)
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

        Vector3 newPosition = new Vector3(newX, newY, this.transform.position.z);

        paddle.transform.position = newPosition;
        
    }
    
}
