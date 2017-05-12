using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed = 50f;
    public float maxSpeed = 1f;
    public float jumpPower = 200f;

    public Transform groundCheck;
    public bool grounded;
    public float minJumpDuration = 0.1f;

    private Rigidbody2D rb2d;
    private Animator anim;
    private SpriteRenderer sprite;

    private float jumpTime;
    private bool jumped;
    

	// Use this for initialization
	void Start () {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        // Update grounded to see if player is on the ground
        grounded = Physics2D.Linecast(gameObject.transform.position,
                                      groundCheck.position,
                                      1 << LayerMask.NameToLayer("Ground"));

        // Jump
        if (Input.GetButton("Jump") && grounded)
        {
            anim.SetTrigger("ninjaGirlJump");
            jumpTime = minJumpDuration;
            jumped = true;
            rb2d.AddForce((Vector2.up * jumpPower));
        }
        // Count down min jump time
        if(jumpTime > 0 && jumped)
        {
            jumpTime -= Time.deltaTime;
        }
        // Land from anything in the air
        // NOTE: This is buggy and will sometimes prevent the jump animation
        if (grounded && jumpTime <= 0 && jumped)
        {
            anim.SetTrigger("ninjaGirlLand");
            jumped = false;
        }


        // Control left and right movement
        float horizontal = Input.GetAxis("Horizontal");
        if(Mathf.Abs(horizontal) > float.Epsilon)
        {
            // Don't play run animation in the air
            if(grounded)
            {
                anim.SetTrigger("ninjaGirlRun");
            }
            rb2d.AddForce((Vector2.right * speed) * horizontal);
        }
        else
        {
            anim.SetTrigger("ninjaGirlStopRun");
        }



        // Flip sprite if moving left
        if (horizontal < 0)
        {
            sprite.flipX = true;
        }
        else
        {
            sprite.flipX = false;
        }

        // Implement speed limit
        if (rb2d.velocity.x >= maxSpeed)
        {
            rb2d.velocity = new Vector2(maxSpeed, rb2d.velocity.y);
        } else if (rb2d.velocity.x <= -maxSpeed)
        {
            rb2d.velocity = new Vector2(-maxSpeed, rb2d.velocity.y);
        }
    }
}
