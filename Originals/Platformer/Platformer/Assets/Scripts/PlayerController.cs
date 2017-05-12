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
    public int hp = 10;

    private Rigidbody2D rb2d;
    private Animator anim;
    private SpriteRenderer sprite;

    private float jumpTime;
    private bool isJumping = false;
    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isThrowing = false;
    private bool isDead = false;
    private bool isGliding = false;

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
        isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("NinjaGirlAttack")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("NinjaGirlJumpAttack");
        isThrowing = anim.GetCurrentAnimatorStateInfo(0).IsName("NinjaGirlThrow")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("NinjaGirlJumpThrow");

        // Update grounded to see if player is on the ground
        grounded = Physics2D.Linecast(gameObject.transform.position,
                                      groundCheck.position,
                                      1 << LayerMask.NameToLayer("Ground"));

        if(hp <= 0)
        {
            if (!isDead)
            {
                anim.SetTrigger("ninjaGirlDead");
                isDead = true;
            }
        }

        // Jump
        if (!isDead && Input.GetButtonDown("Jump") && grounded)
        {
            anim.SetTrigger("ninjaGirlJump");
            jumpTime = minJumpDuration;
            isJumping = true;
            rb2d.AddForce((Vector2.up * jumpPower));
        }
        // Count down min jump time
        if(jumpTime > 0 && isJumping)
        {
            jumpTime -= Time.deltaTime;
        }
        // Land from anything in the air
        // NOTE: This is buggy and will sometimes prevent the jump animation
        if (!isDead && grounded && jumpTime <= 0 && isJumping)
        {
            anim.SetTrigger("ninjaGirlLand");
            isJumping = false;
            isGliding = false;
        }


        // Control left and right movement
        float horizontal = Input.GetAxis("Horizontal");
        if(!isDead && !isThrowing && !isAttacking && Mathf.Abs(horizontal) > float.Epsilon)
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


        // Attack 
        if(!isDead && Input.GetButtonDown("Fire1"))
        {
           if(!isAttacking && !isThrowing)
           {
                if(grounded)
                {
                    anim.SetTrigger("ninjaGirlAttack");
                }
                else
                {
                    anim.SetTrigger("ninjaGirlJumpAttack");
                }    
           }
        }


        // Glide
        if(isJumping && !isDead && !isAttacking && !isThrowing && Input.GetButton("Fire3"))
        {
            isGliding = true;
            anim.SetTrigger("ninjaGirlGlide");
        }
        else
        {
            isGliding = false;
            anim.SetTrigger("ninjaGirlStopGlide");
        }
        
        // If gliding down
        if(isGliding && rb2d.velocity.y < 0)
        {
            rb2d.gravityScale = 0.2f;
        }
        else
        {
            rb2d.gravityScale = 1f;
        }

        // Flip sprite if moving left
        if (!isDead && !isAttacking && !isThrowing && horizontal < 0)
        {
            sprite.flipX = true;
            facingRight = false;
        }
        else if (!isDead && !isAttacking && !isThrowing && horizontal > 0)
        {
            sprite.flipX = false;
            facingRight = true;
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
