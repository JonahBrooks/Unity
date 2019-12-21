using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AdventureController : MonoBehaviour
{
    public static bool firstRun = true;
    public static int slimesRemaining;
    public static Vector3 playerCoords;
    public static Quaternion playerRotation;
    public static bool[] slimeIsAlive;
    public static Vector3[] slimeCoords;

    public GameObject[] slimes;

    public Text hpText;
    public Text slimeText;

    public Transform slimeCollection;

    public int numSlimes;
    public float minSlimeX;
    public float maxSlimeX;
    public float minSlimeZ;
    public float maxSlimeZ;

    private GameObject[] slimeObjects;

    // Start is called before the first frame update
    void Start()
    {
        if (AdventureController.firstRun)
        {
            AdventureController.firstRun = false;
            playerCoords = GameObject.FindWithTag("Player").transform.position;
            playerRotation = GameObject.FindWithTag("Player").transform.rotation;
            slimesRemaining = numSlimes;
            hpText.text = string.Format("HP {0}/{1}", PuzzleController.playerHealth, PuzzleController.maxHealth);
            slimeText.text = string.Format("{0}/{1} Slimes", slimesRemaining, numSlimes);
            // Randomly drop slimes
            slimeIsAlive = new bool[numSlimes];
            slimeCoords = new Vector3[numSlimes];
            slimeObjects = new GameObject[numSlimes];
            for(int i = 0; i < numSlimes; i++)
            {
                slimeIsAlive[i] = true;
                slimeObjects[i] = Instantiate(slimes[i % slimes.Length], slimeCollection);
                slimeObjects[i].GetComponent<SlimeController>().index = i;
                float x = Random.Range(minSlimeX, maxSlimeX);
                float z = Random.Range(minSlimeZ, maxSlimeZ);
                slimeObjects[i].transform.position = new Vector3(x, 0, z);
                slimeCoords[i] = new Vector3(x, 0, z);
                // Slimes that landed in a house trigger will be relocated in Update()
            }
        }
        else
        {
            // Reinstantiate slimes
            slimeObjects = new GameObject[numSlimes];
            for (int i = 0; i < numSlimes; i++)
            {
                if(slimeIsAlive[i])
                {
                    slimeObjects[i] = Instantiate(slimes[i % slimes.Length], slimeCollection);
                    slimeObjects[i].transform.position = slimeCoords[i];
                }
            }
            // Reposition the player character
            GameObject.FindWithTag("Player").transform.position = playerCoords;
            GameObject.FindWithTag("Player").transform.rotation = playerRotation;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        // Update the UI
        hpText.text = string.Format("HP {0}/{1}", PuzzleController.playerHealth, PuzzleController.maxHealth);
        slimeText.text = string.Format("{0}/{1} Slimes", slimesRemaining, numSlimes);

        for(int i = 0; i < numSlimes; i++)
        {
            if(slimeIsAlive[i])
            {
                GameObject slime = slimeObjects[i];
                // If the slime is in a house trigger, relocate it.
                if (slime.GetComponent<SlimeController>().inHouseTrigger)
                {
                    float x = Random.Range(minSlimeX, maxSlimeX);
                    float z = Random.Range(minSlimeZ, maxSlimeZ);
                    slime.transform.position = new Vector3(x, 0, z);
                    slimeCoords[i] = new Vector3(x, 0, z);
                }
            }
            
        }
        
    }
}
