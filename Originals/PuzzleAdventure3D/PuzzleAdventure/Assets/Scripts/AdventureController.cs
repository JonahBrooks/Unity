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

    public Transform treeCollection;

    public int numRowsOfTrees;
    public GameObject treePrefab;
    public float treeLineMinX;
    public float treeLineMaxX;
    public float treeLineMinZ;
    public float treeLineMaxZ;

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

        // Instantiate tree lines
        float treeDiameter = treePrefab.GetComponent<Renderer>().bounds.size.x;
        for (int row = 0; row < numRowsOfTrees; row++)
        {
            // Calculate the offset for this row of trees, including an alternating half diameter offset to prevent the trees from being in a perfect row
            float currentTreeLineMinX = treeLineMinX - (treeDiameter * row);
            float currentTreeLineMaxX = treeLineMaxX + (treeDiameter * row);
            float currentTreeLineMinZ = treeLineMinZ - (treeDiameter * row);
            float currentTreeLineMaxZ = treeLineMaxZ + (treeDiameter * row);
            // Instantiate tree line along the north and south tree lines
            float treeX = currentTreeLineMinX;
            while (treeX < currentTreeLineMaxX)
            {
                Instantiate(treePrefab, new Vector3(treeX + (.5f * treeDiameter) * (row % 2), 0, currentTreeLineMinZ), Quaternion.identity, treeCollection).transform.Rotate(new Vector3(-90, 0, 0));
                Instantiate(treePrefab, new Vector3(treeX + (.5f * treeDiameter) * (row % 2), 0, currentTreeLineMaxZ), Quaternion.identity, treeCollection).transform.Rotate(new Vector3(-90, 0, 0)); ;
                treeX += treeDiameter;
            }                                                                     
            // Instantiate tree line along the east and west tree lines
            float treeZ = currentTreeLineMinZ;
            while (treeZ < currentTreeLineMaxZ)
            {
                Instantiate(treePrefab, new Vector3(currentTreeLineMinX, 0, treeZ + (.5f * treeDiameter) * (row % 2)), Quaternion.identity, treeCollection).transform.Rotate(new Vector3(-90, 0, 0)); ;
                Instantiate(treePrefab, new Vector3(currentTreeLineMaxX, 0, treeZ + (.5f * treeDiameter) * (row % 2)), Quaternion.identity, treeCollection).transform.Rotate(new Vector3(-90, 0, 0)); ;
                treeZ += treeDiameter;
            }
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
