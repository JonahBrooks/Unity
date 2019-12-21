using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    public bool thirdPerson;
    public Camera thirdPersonCamera;
    public Camera firstPersonCamera;

    public float moveForce;
    public float rotationTorque;
    public float maxSpeed;

    public Image fadeImage;
    public Text fadeText;

    private bool resting = false;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        thirdPersonCamera.enabled = thirdPerson;
        firstPersonCamera.enabled = !thirdPerson;

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the player is resting, don't allow the player to adjust camera
        if(resting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            thirdPerson = !thirdPerson;

            thirdPersonCamera.enabled = thirdPerson;
            firstPersonCamera.enabled = !thirdPerson;
        }
    }

    private void FixedUpdate()
    {
        // If the player is resting, don't allow the player to move
        if(resting)
        {
            return;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            rb.AddTorque(transform.up * -rotationTorque);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            rb.AddTorque(transform.up * rotationTorque);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            rb.AddForce(-transform.forward * moveForce);
            // Get rid of the sliding effect
            rb.velocity = Mathf.Clamp(rb.velocity.magnitude, 0, maxSpeed) * -transform.forward;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            rb.AddForce(transform.forward * moveForce);
            // Get rid of the sliding effect
            rb.velocity = Mathf.Clamp(rb.velocity.magnitude, 0, maxSpeed) * transform.forward;
        }

        

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) ||
            Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            rb.velocity = Vector3.zero;
        }

        if (!(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private IEnumerator Rest()
    {
        resting = true;

        // Stop any motion before resting
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        for (int i = 0; i < 64; i++)
        {
            fadeText.color = new Color(1.0f, 1.0f, 1.0f, i/255f);
            fadeImage.color = new Color(0, 0, 0, i/255f);
            yield return null;
        }

        PuzzleController.playerHealth = PuzzleController.maxHealth;

        for(int i = 64; i >= 0; i--)
        {
            fadeText.color = new Color(1.0f, 1.0f, 1.0f, i/255f);
            fadeImage.color = new Color(0, 0, 0, i/255f);
            yield return null;
        }

        resting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the character has entered the bed trigger
        if (other.CompareTag("Bed"))
        {
            StartCoroutine(Rest());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Slime"))
        {
            AdventureController.playerCoords = transform.position;
            AdventureController.playerRotation = transform.rotation;
            AdventureController.slimeIsAlive[collision.gameObject.GetComponent<SlimeController>().index] = false;
            SceneManager.LoadScene("Puzzle");
        }
    }

}
