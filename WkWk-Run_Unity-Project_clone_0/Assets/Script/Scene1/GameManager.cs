using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitingPlayerPanel;
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject losePanel;

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

    // Main platform spawn position
    private Vector3 platformSpawnPos;

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

    // Finish Position
    public Transform FinishPoint;
    public Slider PlayerSlider; 

    // Network
    private Client network;

    // Start is called before the first frame update
    private void Start()
    {
        // Networking
        network = FindObjectOfType<Client>();

        // Panels
        startPanel.SetActive(true);
        waitingPlayerPanel.SetActive(true);

        // Platform start pos
        platformSpawnPos = new Vector3(0, 0, 10);

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
            
        }
        else if(GameIsStarted && !network.isMaster)
        {
            
        }
    }

    // Creating map for start
    private void StartMapSpawn()
    {
        while(platformSpawnPoint.position.y > rowPos[0].position.y)
        {
            // Create platform
            if (platformSpawnPos.y < platformSpawnPoint.position.y)
            {
                Instantiate(platformGround, platformSpawnPos, Quaternion.identity);
                platformSpawnPos = new Vector3(platformSpawnPos.x, platformSpawnPos.y + 10, platformSpawnPos.z);
            }

            for (int i = 0; i < rowPos.Length; i++)
            {
                // relocate row position
                rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
            }
        }
    }

    // Platform spaner
    public void StartSpawnPlatform()
    {
        StartCoroutine(SpawnPlatformGames());
    }
    private IEnumerator SpawnPlatformGames()
    {
        while (GameIsStarted)
        {
            // Check spawn position and if this is master
            if (platformSpawnPoint.position.y > rowPos[0].position.y && network.isMaster)
            {
                // Preparing massage data
                string[] massage = new string[(int)rowCount + 1];
                massage[0] = "SpawnPlatform";
                
                // Spawn here
                int[] temp = new int[(int)rowCount];
                for (int i = 0; i < rowCount; i++)
                {
                    massage[i + 1] = "0";
                }

                // Send to other client if host
                network.SendMassageClient("All", massage);
                SpawnPlatformGames(temp);
            }

            // Alwasy spawn platform ground
            if (platformSpawnPos.y < platformSpawnPoint.position.y)
            {
                SpawnPlatformGround();
            }

            yield return new WaitForSeconds(.7f);
        }
    }
    public void SpawnPlatformGames(int[] platform)
    {
        // Spawn
        for (int i = 0; i < rowPos.Length; i++)
        {
            // Instantiate new platform
            GameObject temp = null;
            if (platform[i] == 1)
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

            if(temp != null)
            {
                // Re-scale
                temp.transform.localScale = new Vector3(scaleFix, scaleFix, scaleFix);
            }
           
            // relocate row position
            rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
        }
    }
    public void SpawnPlatformGround()
    {
        Instantiate(platformGround, platformSpawnPos, Quaternion.identity);
        platformSpawnPos = new Vector3(platformSpawnPos.x, platformSpawnPos.y + 10, platformSpawnPos.z);
    }
}
