using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Leap;
using Leap.Unity;

public class MenuController : MonoBehaviour
{
    public Text errorText;
    public Button leapMotionButton;
    public Button kinectButton;

    private Controller leapMotionController;
    private bool lmcConnected;
    private bool kConnected;


    // Start is called before the first frame update
    void Start()
    {
        // Disable the error message on start
        errorText.gameObject.SetActive(false);

        Button lmb = leapMotionButton.GetComponent<Button>(); 
        lmb.onClick.AddListener(LeapMotionButtonOnClick);

        Button kb = kinectButton.GetComponent<Button>();
        kb.onClick.AddListener(KinectButtonOnClick);

        // Check if a leap motion device is ready
        leapMotionController = new Controller();
        lmcConnected = leapMotionController.IsConnected;

        // Check if the kinect is ready
        kConnected = KinectManager.Instance && KinectManager.Instance.IsInitialized();
    }

    private void Update()
    {
        // Check the status of the devices
        lmcConnected = leapMotionController.IsConnected;
        kConnected = KinectManager.Instance && KinectManager.Instance.IsInitialized();

        // Check if no motion controllers are ready
        if (lmcConnected == false && kConnected == false)
        {
            // Hide both buttons and display error message
            leapMotionButton.gameObject.SetActive(false);
            kinectButton.gameObject.SetActive(false);
            // Show error message
            errorText.gameObject.SetActive(true);
        }
        else if(lmcConnected == false && kConnected == true)
        {
            // If only a kinect is connected
            leapMotionButton.gameObject.SetActive(false);
            kinectButton.gameObject.SetActive(true);
            // Center the kinect button
            kinectButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);
            // Hide error message
            errorText.gameObject.SetActive(false);
        }
        else if(lmcConnected == true && kConnected == false)
        {
            // If only the leap motion is connected
            leapMotionButton.gameObject.SetActive(true);
            kinectButton.gameObject.SetActive(false);
            // Center the leap motion button
            leapMotionButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            // Hide error message
            errorText.gameObject.SetActive(false);
        }
        else if(lmcConnected == true && kConnected == true)
        {
            // If both sensors are connected
            leapMotionButton.gameObject.SetActive(true);
            kinectButton.gameObject.SetActive(true);
            // Offset both buttons
            leapMotionButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(-100, 0, 0);
            kinectButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(100, 0, 0);
            // Hide error message
            errorText.gameObject.SetActive(false);
        }
    }

    private void LeapMotionButtonOnClick()
    {
        PaddleController.movementStyle = PaddleController.MovementStyleEnum.Controller;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    private void KinectButtonOnClick()
    {
        PaddleController.movementStyle = PaddleController.MovementStyleEnum.Kinect;
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}
