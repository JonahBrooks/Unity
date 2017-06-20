using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMover : MonoBehaviour {

    public float speed = 2f;
    public float maxSpeed = 2f;

    private Animator anim;
    private Rigidbody2D rb2d;

	// Use this for initialization
	void Start () {
        anim = gameObject.GetComponent<Animator>();
        rb2d = gameObject.GetComponent<Rigidbody2D>();
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Slime")
        {
            // Launch puzzle game
            Debug.Log("Launching Puzzle");
            SceneManager.LoadScene("Puzzle");
            Debug.Log("Back from puzzle!");
            Destroy(collision.gameObject);
        }
    }


    void FixedUpdate () {
        float hori;
        float vert;
        float epsi = 0.5f;

        hori = Input.GetAxis("Horizontal");
        vert = Input.GetAxis("Vertical");
        
        // Adjust sprite for horizontal movement
        if (hori < -epsi)
        {
            anim.SetTrigger("WalkLeft");
            // Apply force for horizontal movement
            rb2d.AddForce(Vector2.right * speed * hori);
        }
        else if (hori > epsi)
        {
            anim.SetTrigger("WalkRight");
            // Apply force for horizontal movement
            rb2d.AddForce(Vector2.right * speed * hori);
        }
        else
        {
            // Stop horizontal movement
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
        }

        // Adjust sprite for vertical movement
        if (vert < -epsi)
        {
            // Don't play vertical walk animation if horizontal animation is already playing
            if (Mathf.Abs(hori) < epsi)
            { 
                anim.SetTrigger("WalkToward");
            }
            // Apply force for vertical movement
            rb2d.AddForce(Vector2.up * speed * vert);
        }
        else if (vert > epsi)
        {
            // Don't play vertical walk animation if horizontal animation is already playing
            if (Mathf.Abs(hori) < epsi)
            {
                anim.SetTrigger("WalkAway");
            }
            // Apply force for vertical movement
            rb2d.AddForce(Vector2.up * speed * vert);
        }
        else
        {
            // Stop horizontal movement
            rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
        }



        // Implement speed limit
        if (rb2d.velocity.x >= maxSpeed)
        {
            rb2d.velocity = new Vector2(maxSpeed, rb2d.velocity.y);
        }
        else if (rb2d.velocity.x <= -maxSpeed)
        {
            rb2d.velocity = new Vector2(-maxSpeed, rb2d.velocity.y);
        }

        if (rb2d.velocity.y >= maxSpeed)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, maxSpeed);
        }
        else if (rb2d.velocity.y <= -maxSpeed)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, -maxSpeed);
        }
    }
}
