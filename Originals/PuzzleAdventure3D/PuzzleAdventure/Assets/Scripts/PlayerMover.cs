using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMover : MonoBehaviour {

    public float speed = 2f;
    public float maxSpeed = 2f;
    public GameObject[] slimePrefabs;
    public int numSlimes;
    public Vector2 slimeSpawnMins; // -23,-24
    public Vector2 slimeSpawnMaxs; // 21, 20
    public GameObject[] bushPrefabs;
    public int numBushes;

    public Text healthRemainingText;
    public Text slimesRemainingText;

    private Animator anim;
    private Rigidbody2D rb2d;

    private GameObject[] slimes;

    public static bool firstRun = true;
    private static Vector2 coord;
    private static Vector2[] slimeCoords;
    private static bool[] slimeActives;
    private static int[] slimeTypes;
    private static Dictionary<Vector2, int> coordsToIndex;
    private static int slimesKilled;

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
            PlayerMover.coord = new Vector3(0, 0, 0);
            transform.position = new Vector3(0, 0, 0);
            PlayerMover.bushes = new Bush[numBushes];
            PlayerMover.slimeCoords = new Vector2[numSlimes];
            PlayerMover.slimeActives = new bool[numSlimes];
            PlayerMover.slimeTypes = new int[numSlimes];
            PlayerMover.coordsToIndex = new Dictionary<Vector2, int>();
            PlayerMover.slimesKilled = 0;
            for (int i = 0; i < numSlimes; i++)
            {
                // Find location on the map to spawn a slime
                tmpLoc = new Vector3(Random.Range(slimeSpawnMins.x, slimeSpawnMaxs.x), Random.Range(slimeSpawnMins.y, slimeSpawnMaxs.y), 0f);
                // Set slime coordinates to that location
                PlayerMover.slimeCoords[i] = tmpLoc; 
                // Prevent slime from spawning inside another collider
                while (Physics2D.OverlapCircle(PlayerMover.slimeCoords[i],0f))
                {
                    tmpLoc = new Vector3(Random.Range(slimeSpawnMins.x, slimeSpawnMaxs.x), Random.Range(slimeSpawnMins.y, slimeSpawnMaxs.y), 0f);
                    PlayerMover.slimeCoords[i] = tmpLoc; // Camera.main.ScreenToWorldPoint(tmpLoc);
                }
                // Set slime to active so it spawns into the game
                PlayerMover.slimeActives[i]= true;
                // Set slime type so it remains the same color between loads
                PlayerMover.slimeTypes[i] = Random.Range(0, slimePrefabs.Length);
                // Add slime to dictionary for later lookup
                PlayerMover.coordsToIndex.Add(PlayerMover.slimeCoords[i], i);
            }
            for (int i = 0; i < numBushes; i++)
            {
                PlayerMover.bushes[i] = new Bush();
                // Find location on the screen to spawn a bush
                tmpLoc = new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0f);
                // Set bush coordinates to the world equivalent of that screen position
                PlayerMover.bushes[i].coord = Camera.main.ScreenToWorldPoint(tmpLoc);
                // Prevent bush from spawning inside another collider
                while (Physics2D.OverlapCircle(PlayerMover.bushes[i].coord, 0f))
                {
                    tmpLoc = new Vector3(Random.Range(Screen.width * 0.1f, Screen.width * 0.9f), Random.Range(Screen.height * 0.1f, Screen.height * 0.9f), 0f);
                    PlayerMover.bushes[i].coord = Camera.main.ScreenToWorldPoint(tmpLoc);
                }
                // Set bush type so it remains the same between loads
                PlayerMover.bushes[i].type = Random.Range(0, bushPrefabs.Length);
            }
        }

        // Update text boxes
        UpdateText();

        // Instantiate slimes
        for (int i = 0; i < numSlimes; i++)
        {
            if (PlayerMover.slimeActives[i])
            {
                Instantiate(slimePrefabs[PlayerMover.slimeTypes[i]],
                            PlayerMover.slimeCoords[i],
                            Quaternion.identity);
            }
        }

        // Instantiate bushes
        for (int i = 0; i < numBushes; i++)
        {
            Instantiate(bushPrefabs[PlayerMover.bushes[i].type],
                        PlayerMover.bushes[i].coord,
                        Quaternion.identity);
        }

        // Set player coordinates to the last known player coordinates
        gameObject.transform.position = PlayerMover.coord;

        // Check for victory condition
        bool victory = true;
        for (int i = 0; i < numSlimes; i++)
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

    private void UpdateText()
    {
        healthRemainingText.text = PuzzleController.playerHealth + "/" + PuzzleController.maxHealth + " HP";
        slimesRemainingText.text = PlayerMover.slimesKilled + "/" + numSlimes + " Slimes";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Slime")
        {
            // Launch puzzle game
            PlayerMover.coord = gameObject.transform.position;
            PlayerMover.slimeActives[PlayerMover.coordsToIndex[collision.transform.position]] = false;
            PlayerMover.slimesKilled++;
            UpdateText();
            Destroy(collision.gameObject);
            SceneManager.LoadScene("Puzzle");
        }

        if(collision.tag == "Bed")
        {
            // Restore player health
            PuzzleController.playerHealth = PuzzleController.maxHealth;
            UpdateText();
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
