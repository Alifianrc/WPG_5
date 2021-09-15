using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitingPlayerPanel;

    // Boolean
    public bool GameIsStarted { get; set; }
    public bool GameIsFinished { get; set; }

    // Screen size
    private float screenWidth;
    private float rowCount = 5;
    private float rowDist;

    // Player
    [SerializeField] private GameObject playerPrefab;

    // Platform Spawn Point
    [SerializeField] private Transform platformSpawnPoint;

    // Platform
    [SerializeField] private GameObject platformGround; // ID = 0
    [SerializeField] private GameObject platformWater; // ID = 1
    private float randomPlatfromValue;
    // Trap
    [SerializeField] private GameObject trapLava; // ID = 2
    [SerializeField] private GameObject trapBomb; // ID = 3

    // Scaling
    private float scaleFix;

    // Row position
    [SerializeField] private Transform[] rowPos;
    [HideInInspector] public float[] rowXPos { get; private set; }

    // Network
    private Client network;
    [SerializeField] private GameObject networkPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        // Networking
        network = FindObjectOfType<Client>();

        // Panels
        startPanel.SetActive(true);
        waitingPlayerPanel.SetActive(true);

        // Set screen size
        Vector2 minPosCamera = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 maxPosCamera = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));

        // Screen width size
        screenWidth = maxPosCamera.x - minPosCamera.x;

        // Calculating row position
        rowDist = screenWidth / rowCount;
        rowXPos = new float[(int)rowCount];
        for(int i = 0; i < rowPos.Length; i++)
        {
            rowPos[i].position = new Vector3((minPosCamera.x + (rowDist * 0.5f)) + (rowDist * i), minPosCamera.y + (rowDist * 0.5f) - (rowDist * 5), 0);
            rowXPos[i] = rowPos[i].position.x;
        }

        // Calculating scale
        float width = Camera.main.orthographicSize * 2.0f * Screen.width / Screen.height;
        scaleFix = width / rowCount;

        // Set Random value in %
        randomPlatfromValue = 95;

        // Spawn Player
        network.SendMassageClient("Server", "SpawnPlayer"); // Need more parameter in future

        // Creating start map
        StartMapSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        // If the game is started
        if (GameIsStarted && network.isMaster)
        {
            SpawnPlatformGames();
        }

    }

    // Creating map for start
    private void StartMapSpawn()
    {
        while(platformSpawnPoint.position.y > rowPos[0].position.y)
        {
            for (int i = 0; i < rowPos.Length; i++)
            {
                // Instantiate new platform
                GameObject temp = Instantiate(platformGround, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
                temp.transform.localScale = new Vector3(scaleFix, scaleFix, scaleFix);

                // relocate row position
                rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
            } 
        }
    }

    // Platform spaner
    private void SpawnPlatformGames()
    {
        // Check spawn position
        if(platformSpawnPoint.position.y > rowPos[0].position.y)
        {
            // Preparing massage data
            string[] massage = new string[(int)rowCount + 1];
            massage[0] = "SpawnPlatform";
            // Randomize
            int[] temp = new int[(int)rowCount];
            for (int i = 0; i < rowCount; i++)
            {
                //temp[i] = new int();
                int platRand = Random.Range(1, 101);
                if (platRand < randomPlatfromValue)
                {
                    temp[i] = 0;
                    massage[i + 1] = temp[i].ToString();
                }
                else
                {
                    int trapRand = Random.Range(2, 4);
                    temp[i] = trapRand;
                    massage[i + 1] = temp[i].ToString();
                }
            }

            // Send to other client if host
            network.SendMassageClient("All", massage);
            SpawnPlatformGames(temp);
        }
    }
    public void SpawnPlatformGames(int[] platform)
    {
        // Spawn
        for (int i = 0; i < rowPos.Length; i++)
        {
            // Instantiate new platform
            GameObject temp;
            if (platform[i] == 0)
            {
                // Spawn ground
                temp = Instantiate(platformGround, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
            }
            else if(platform[i] == 1)
            {
                // Spawn water
                temp = Instantiate(platformWater, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
            }
            else if (platform[i] == 2)
            {
                // Spawn bomb
                temp = Instantiate(trapBomb, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
            }
            else if (platform[i] == 3)
            {
                // Spawn lava
                temp = Instantiate(trapLava, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
            }
            else
            {
                // For default just spawn ground
                temp = Instantiate(platformGround, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 10), Quaternion.identity);
            }

            // Re-scale
            temp.transform.localScale = new Vector3(scaleFix, scaleFix, scaleFix);
            // relocate row position
            rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
        }
    }
}
