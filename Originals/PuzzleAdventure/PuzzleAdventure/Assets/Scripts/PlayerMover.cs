using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMover : MonoBehaviour {

    public float speed = 2f;
    public float maxSpeed = 2f;
    public GameObject[] slimePrefabs;
    public GameObject[] bushPrefabs;
    public int numBushes;
    

    private Animator anim;
    private Rigidbody2D rb2d;

    private GameObject[] slimes;
    private Dictionary<string, int> nameToIndex = new Dictionary<string, int>();

    private static bool firstRun = true;
    private static Vector2 coord;
    private static Vector2[] slimeCoords;
    private static bool[] slimeActives;

    private struct Bush
    {
        public Vector2 coord;
        public int type;
    }

    private static Bush[] bushes;

	// Use this for initialization
	void Start () {
        Vector3 tmpLoc;

        rb2d = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();

        // Executes the first time Start is called
        if(PlayerMover.firstRun)
        {
            PlayerMover.firstRun = false;
            PlayerMover.bushes = new Bush[numBushes];
            PlayerMover.slimeCoords = new Vector2[slimePrefabs.Length];
            PlayerMover.slimeActives = new bool[slimePrefabs.Length];
            for (int i = 0; i < slimePrefabs.Length; i++)
            {
                // Find location on the screen to spawn a slime
                tmpLoc = new Vector3(Random.Range(Screen.width*0.1f, Screen.width*0.9f), Random.Range(Screen.height*0.1f, Screen.height*0.9f), 0f);
                // Set slime coordinates to the world equivalent of that screen position
                PlayerMover.slimeCoords[i] = Camera.main.ScreenToWorldPoint(tmpLoc);
                // Prevent slime from spawning under the character
                while (Mathf.Abs(PlayerMover.slimeCoords[i].x) < 1 && Mathf.Abs(PlayerMover.slimeCoords[i].y) < 1)
                {
                    tmpLoc = new Vector3(Random.Range(Screen.width * 0.1f, Screen.width * 0.9f), Random.Range(Screen.height * 0.1f, Screen.height * 0.9f), 0f);
                    PlayerMover.slimeCoords[i] = Camera.main.ScreenToWorldPoint(tmpLoc);
                }
                // Set slime to active so it spawns into the game
                PlayerMover.slimeActives[i]= true;
            }
            for (int i = 0; i < numBushes; i++)
            {
                PlayerMover.bushes[i] = new Bush();
                // Find location on the screen to spawn a bush
                tmpLoc = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0f);
                // Set bush coordinates to the world equivalent of that screen position
                PlayerMover.bushes[i].coord = Camera.main.ScreenToWorldPoint(tmpLoc);
                PlayerMover.bushes[i].type = Random.Range(0, bushPrefabs.Length);
            }
        }
        
        nameToIndex.Add("BlueOverworld(Clone)", 0);
        nameToIndex.Add("GrayOverworld(Clone)", 1);
        nameToIndex.Add("GreenOverworld(Clone)", 2);
        nameToIndex.Add("PurpleOverworld(Clone)", 3);
        nameToIndex.Add("RedOverworld(Clone)", 4);
        nameToIndex.Add("YellowOverworld(Clone)", 5);

        // Instantiate slimes
        for (int i = 0; i < slimePrefabs.Length; i++)
        {
            if (PlayerMover.slimeActives[i])
            {
                Instantiate(slimePrefabs[i],
                            PlayerMover.slimeCoords[i],
                            Quaternion.identity);
            }
        }

        // Instantiate bushes
        for (int i = 0; i < numBushes; i++)
        {
            Debug.Log("Instantiating bush");
            Instantiate(bushPrefabs[PlayerMover.bushes[i].type],
                        PlayerMover.bushes[i].coord,
                        Quaternion.identity);
        }

        // Set player coordinates to the last known player coordinates
        gameObject.transform.position = PlayerMover.coord;

        // Check for victory condition
        bool victory = true;
        for (int i = 0; i < slimePrefabs.Length; i++)
        {
            if (PlayerMover.slimeActives[i])
            {
                victory = false;
            }
        }
        if (victory)
        {
            // Load win screen
            SceneManager.LoadScene("Victory");
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Slime")
        {
            // Launch puzzle game
            PlayerMover.coord = gameObject.transform.position;
            PlayerMover.slimeActives[nameToIndex[collision.name]] = false;
            Destroy(collision.gameObject);
            SceneManager.LoadScene("Puzzle");
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
