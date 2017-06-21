using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMover : MonoBehaviour {

    public float speed = 2f;
    public float maxSpeed = 2f;
    public GameObject[] slimePrefabs;
    

    private Animator anim;
    private Rigidbody2D rb2d;

    private GameObject[] slimes;
    private Dictionary<string, int> nameToIndex = new Dictionary<string, int>();

    private static bool firstRun = true;
    private static Vector2 coord;

	// Use this for initialization
	void Start () {
        Vector3 tmpLoc;

        rb2d = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();

        if(PlayerMover.firstRun)
        {
            PlayerMover.firstRun = false;
            for (int i = 0; i < slimePrefabs.Length; i++)
            {
                tmpLoc = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0f);
                slimePrefabs[i].GetComponent<Coordinates>().coord = Camera.main.ScreenToWorldPoint(tmpLoc);
                slimePrefabs[i].GetComponent<Coordinates>().active = true;
            }
        }
        
        nameToIndex.Add("BlueOverworld(Clone)", 0);
        nameToIndex.Add("GrayOverworld(Clone)", 1);
        nameToIndex.Add("GreenOverworld(Clone)", 2);
        nameToIndex.Add("PurpleOverworld(Clone)", 3);
        nameToIndex.Add("RedOverworld(Clone)", 4);
        nameToIndex.Add("YellowOverworld(Clone)", 5);

        for (int i = 0; i < slimePrefabs.Length; i++)
        {
            if (slimePrefabs[i].GetComponent<Coordinates>().active)
            {
                Instantiate(slimePrefabs[i],
                            slimePrefabs[i].GetComponent<Coordinates>().coord,
                            Quaternion.identity);
            }
        }

        gameObject.transform.position = PlayerMover.coord;
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Slime")
        {
            // Launch puzzle game
            PlayerMover.coord = gameObject.transform.position;
            SceneManager.LoadScene("Puzzle");
            slimePrefabs[nameToIndex[collision.name]].GetComponent<Coordinates>().active = false;
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
