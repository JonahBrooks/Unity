using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    public float speed;
    public Text countText;
    public Text winText;
    public Camera camera;
    public Vector3 groundXYZ;
    public float DuckRotationSpeed;

    private Rigidbody rb;
    private SpringJoint sj;
    private int count;
    private Vector3 last;

    private bool vr;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sj = GetComponent<SpringJoint>();
        count = 0;
        setCountText();
        winText.text = "";
        vr = UnityEngine.VR.VRSettings.enabled;
        last = transform.position;
    }

    private void Update()
    {
        Vector3 vect = transform.position - last;
        if (vect != Vector3.zero)
        {
            Quaternion.Slerp(
                transform.rotation, 
                Quaternion.LookRotation(vect), 
                Time.deltaTime * DuckRotationSpeed
                );
        }
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        last = transform.position;


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
        } else if (Input.mousePresent) // Otherwise use the mouse or touch screen location
        {
            // Get mouse platform intercept
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
                // Set intercept to a sentinal indicating no target exists
                intercept = new Vector3(0, 99, 0);
            }
                
        } else if (Input.touchCount > 0)// Touchscreen
        {
            // Get mouse platform intercept
            winText.text = "No Intercept";
            center = camera.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit moo;
            Physics.Raycast(center, out moo, 1000f);
            if (moo.transform != null)
            {
                winText.text = moo.point.ToString();
                intercept = moo.point;
            }
            else
            {
                winText.text = "No moo";
                // Set intercept to a sentinal indicating no target exists
                intercept = new Vector3(0, 99, 0);
            }
        }

        Vector3 movement = intercept;
        movement.y = 0;

        // Set connectedAnchor to the target position if it exists
        if (intercept.y < 2)
        {
            sj.connectedAnchor = movement;
        }
        //rb.AddForce(movement * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        float randomx = Random.value * groundXYZ.x - groundXYZ.x / 2.0f;
        float randomz = Random.value * groundXYZ.z - groundXYZ.z / 2.0f;
        float gemy = groundXYZ.y;

        

        if(other.gameObject.CompareTag("Pickup"))
        {

            //other.gameObject.SetActive(false);
            // TODO: Add gem to timed queue of some sort
            other.gameObject.transform.position = new Vector3(randomx, gemy, randomz); 
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
