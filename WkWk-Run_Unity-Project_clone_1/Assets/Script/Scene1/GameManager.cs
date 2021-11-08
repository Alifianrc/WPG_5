using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitingPlayerPanel;
    [SerializeField] private GameObject gameOverPanel;

    // Boolean
    public bool GameIsStarted { get; set; }
    public bool GameIsFinished { get; set; }

    // Screen size
    private float screenWidth;
    private float rowCount = 5;
    private float rowDist;

    // Player
    [SerializeField] public GameObject[] playerPrefab;
    [HideInInspector] public GameObject[] PlayerList { get; set; }

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

    // Level value
    public int GameLevel { get; private set; }
    public int LevelDistance = 80;

    // All Player position
    [SerializeField] GameObject[] playersOrder;

    // Finish Position
    public Transform FinishPoint;
    public Slider PlayerSlider;
    public Image SliderImage;
    public Sprite[] SliderSpriteList;
    // UI
    public Text coinText;

    // Game Over Panel
    [SerializeField] private Image gameOverPanelSprite;
    [SerializeField] private Text gameOverPanelTitle;
    [SerializeField] private Text gameOverPanelCoin;
    [SerializeField] private Text gameOverPanelPlayerOrder;
    [SerializeField] public Sprite[] characterSpriteFace;

    // Leveling
    public int levelDistance { get; private set; }

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
        network.SendMassageClient("Server", "SpawnPlayer|" + GameDataLoader.TheData.selectedChar);
        // Set SLider Image
        SliderImage.sprite = SliderSpriteList[GameDataLoader.TheData.selectedChar];

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

    // Platform spaner -------------------------------------------------------------------------------------------------
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

    // Game Level -----------------------------------------------------------------------------------------------------
    public void IncreaseGameLevel()
    {
        GameLevel++;
        //allTrapRandomValue += 2;
        //trapSlowRandomValue -= 3;
        //trapMovingRandomValue += 3;
    }

    // Player Order
    // Find all players
    public void FindPlayers()
    {
        PlayerManager[] temps = FindObjectsOfType<PlayerManager>();
        playersOrder = new GameObject[temps.Length];

        for (int i = 0; i < temps.Length; i++)
        {
            playersOrder[i] = temps[i].gameObject;
        }

        StartCoroutine(PlayerOrder());
    }
    // Determine player order
    private IEnumerator PlayerOrder()
    {
        while (!GameIsFinished)
        {
            // Sorting algorithm
            if (playersOrder.Length > 1)
            {
                for (int i = 0; i < playersOrder.Length - 1; i++)
                {
                    int min = i;
                    for (var j = i + 1; j < playersOrder.Length; j++)
                    {
                        if (playersOrder[min].transform.position.y < playersOrder[j].transform.position.y)
                        {
                            min = j;
                        }
                    }

                    if (min != i)
                    {
                        var temp = playersOrder[min];
                        playersOrder[min] = playersOrder[i];
                        playersOrder[i] = temp;
                        playersOrder[i].GetComponent<PlayerManager>().ChangePlayerOrder(i + 1);
                    }
                    else
                    {
                        playersOrder[i].GetComponent<PlayerManager>().ChangePlayerOrder(i + 1);
                    }

                    if (i == playersOrder.Length - 2)
                    {
                        playersOrder[i + 1].GetComponent<PlayerManager>().ChangePlayerOrder(i + 2);
                    }
                }
            }
            else
            {
                playersOrder[0].GetComponent<PlayerManager>().ChangePlayerOrder(1);
            }
            yield return new WaitForSeconds(.8f);
        }
    }

    // Game Over method -----------------------------------------------------------------------------------------------
    public void GameOver(bool isWin, int order)
    {
        StartCoroutine(OpenGameOverPanel(isWin, order));
    }
    private IEnumerator OpenGameOverPanel(bool isWin, int order)
    {
        GameIsFinished = true;
        yield return new WaitForSeconds(1.5f);
        if (isWin)
        {
            //audio.Play("Finish");
        }
        else
        {
            //audio.Play("Lose");
        }

        Time.timeScale = .3f;

        gameOverPanel.SetActive(true);

        gameOverPanelSprite.sprite = characterSpriteFace[GameDataLoader.TheData.selectedChar];

        gameOverPanelCoin.text = GameDataLoader.TheData.Coin.ToString("n0");
        SaveGame.SaveProgress(GameDataLoader.TheData);

        if (isWin && order == 1)
        {
            gameOverPanelTitle.text = "YOU WIN !!!";
            gameOverPanelPlayerOrder.text = "1st";
        }
        else if (isWin && order == 2)
        {
            gameOverPanelTitle.text = "NICE PLAY !";
            gameOverPanelPlayerOrder.text = "2nd";
        }
        else if (isWin && order == 3)
        {
            gameOverPanelTitle.text = "NOT BAD !";
            gameOverPanelPlayerOrder.text = "3rd";
        }
        else if (isWin && order == 4)
        {
            gameOverPanelTitle.text = "TRY AGAIN !";
            gameOverPanelPlayerOrder.text = "4th";
        }
        else if (isWin && order == 5)
        {
            gameOverPanelTitle.text = "TRY AGAIN !";
            gameOverPanelPlayerOrder.text = "5th";
        }
        else if (!isWin)
        {
            gameOverPanelTitle.text = "YOU LOSE !";
            gameOverPanelPlayerOrder.text = "";
        }
    }

    // Exit Room
    public void ExitRoom()
    {
        network.SendMassageClient("Server", "ExitRoom");
    }
    public void OnExitRoom()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
