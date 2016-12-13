using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public float speed;
    public Text countText;
    public Text winText;
    public Camera camera;
    public Plane ground;

    private Rigidbody rb;

    private int count;

    private bool vr;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        setCountText();
        winText.text = "";
        vr = UnityEditorInternal.VR.VREditor.GetVREnabled(UnityEditor.BuildTargetGroup.Android);
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 intercept = new Vector3(0f,0f,0f);
        float interceptDist;
        Ray center;
        // If VR is enabled, use tha center of the camera for movement
        if (vr)
        {
            moveHorizontal = camera.transform.rotation.y;
            moveVertical = -(camera.transform.rotation.x - ((float)Mathf.PI) / 4.0f + 0.5f);
        } else // Otherwise use the mouse or touch screen location
        {
            // Get mouse platform intercept
            winText.text = "No Mouse";
            if (Input.mousePresent)
            {
                winText.text = "No Intercept";
                center = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit moo;
                Physics.Raycast(center, out moo, 1000f);
                if (moo.transform != null)
                {
                    winText.text = moo.point.ToString();
                    intercept = moo.point;
                } else
                {
                    winText.text = "No moo";
                }
                
            }
            // Get touch platform intercept
        }

        Vector3 movement = intercept - transform.position;
        movement.y = 0;

        rb.AddForce(movement * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false);
            count++;
            setCountText();
        }
        
    }

    private void setCountText()
    {
        countText.text = "Count: " + count.ToString();
        if (count >= 12)
        {
            winText.text = "You Win!";
        }
    }
}
