using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // Player name
    [HideInInspector] public string playerName { get; set; }

    // Player default speed
    private float playerDefaultSpeed = 3f;
    private float playerDefaultSwipeSpeed = .2f;
    [HideInInspector] public float playerSpeed { get; set; }
    [HideInInspector] public float playerSwipeSpeed { get; set; }

    // Position in arena (0 - 4)
    public int rowPos { get; set; }

    // Main control
    private GameManager manager;
    private Client network;
    private bool isDead = false;

    // UI 
    [SerializeField] private Canvas canvas;
    private Slider mySlider;
    private Transform finishPoint;
    private Vector2 startPos;
    [SerializeField] private Text nameText;
    private Text coinText;
    [SerializeField] private GameObject disconnectLogo;

    // Order in race
    public int PlayerOrder { get; private set; }
    [SerializeField] private Text orderText;

    // Animation
    private Animator animator;

    // Swipe control
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 currentTouchPos;
    private bool touchStopped;
    private bool touchIsOn;
    private bool rowChanged;
    // Swipe control range
    private float swipeRange = 50;
    private float tabRange = 10;

    // Effect
    private bool slowEffectIsActive;
    private float trapSpeed = .3f;
    private float trapTime = 1.5f;

    // Level for increase speed
    private float levelThreshold;
    private float increaseSpeed = 1f;

    // Network
    bool isDisconnect;

    // Audio
    private AudioManager audio;

    void Start()
    {
        // Game manager
        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();
        finishPoint = manager.FinishPoint;

        // Animation
        try
        {
            animator = GetComponent<Animator>();
        }
        catch
        {

        }

        // Set camera and UI
        if (playerName == network.MyName)
        {
            FindObjectOfType<CameraFollow>().playerPos = gameObject.transform;
            mySlider = manager.PlayerSlider;
            mySlider.value = 0;
            startPos = new Vector2(transform.position.x, transform.position.y);
            finishPoint = manager.FinishPoint;
            coinText = manager.coinText;
            coinText.text = GameDataLoader.TheData.Coin.ToString("n0");
        }
        // Set Name
        nameText.text = playerName;
        // Set UI order
        PlayerOrder = 1;
        orderText.text = PlayerOrder.ToString();
        // Set Camera
        canvas.worldCamera = FindObjectOfType<CameraFollow>().GetComponent<Camera>();

        // Set player speed
        playerSpeed = playerDefaultSpeed;

        // Set Start position
        transform.position = new Vector3(manager.rowXPos[rowPos], -2, 0);

        // Level
        levelThreshold = manager.LevelDistance;

        // Network 
        isDisconnect = false;
    }

    
    void Update()
    {
        // If the game is started
        if (manager.GameIsStarted && !isDisconnect && !isDead)
        {
            // Player start running
            transform.position = new Vector2(transform.position.x, transform.position.y + (playerSpeed * Time.deltaTime));

            // Check changing row
            if (rowChanged)
            {
                // Send massage
                MovePositionRow();
            }

            // If this player is mine
            if (playerName == network.MyName)
            {
                // Detect swipe screen
                SwipeControl();

                // UI
                mySlider.value = (transform.position.y - startPos.y) / (finishPoint.transform.position.y - startPos.y);

                // Check if finish
                if (transform.position.y > finishPoint.position.y)
                {
                    // Some UI
                    manager.GameOver(true, PlayerOrder);
                }
            }

            if(transform.position.y > levelThreshold && !slowEffectIsActive)
            {
                levelThreshold += manager.LevelDistance;
                playerDefaultSpeed += increaseSpeed;
                playerSpeed = playerDefaultSpeed;
            }
        }
    }

    // Start Sync position -------------------------------------------------------------------------------------------------
    public void BeginSyncPos()
    {
        StartCoroutine(SyncPos());
    }
    private IEnumerator SyncPos()
    {
        while (manager.GameIsStarted)
        {
            // Sync position
            string[] massage = new string[] { "SyncPlr", transform.position.x.ToString(), transform.position.y.ToString() };
            network.SendMassageClient("AllES", massage);

            yield return new WaitForSeconds(5f);
        }
    }

    // Player Control ------------------------------------------------------------------------------------------------------
    private void SwipeControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            touchIsOn = true;
        }
        if(touchIsOn)
        {
            currentTouchPos = Input.mousePosition;
            Vector2 distance = currentTouchPos - startTouchPos;
            
            if (!touchStopped)
            {
                if (distance.x < -swipeRange)
                {
                    if(rowPos != 0)
                    {
                        rowPos--;
                        string[] mas = new string[] { "ChangeRow", rowPos.ToString() };
                        network.SendMassageClient("All", mas);
                    }
                    touchStopped = true;
                }
                else if (distance.x > swipeRange)
                {
                    if (rowPos != 4)
                    {
                        rowPos++;
                        string[] mas = new string[] { "ChangeRow", rowPos.ToString() };
                        network.SendMassageClient("All", mas);
                    }
                    touchStopped = true;
                }
                else if (distance.y > swipeRange)
                {
                    Debug.Log("Up");
                    touchStopped = true;
                }
                else if (distance.y < -swipeRange)
                {
                    Debug.Log("Down");
                    touchStopped = true;
                }
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            touchIsOn = false;
            touchStopped = false;
            endTouchPos = Input.mousePosition;
            Vector2 distance = endTouchPos - startTouchPos;

            if(Mathf.Abs(distance.x) < tabRange && Mathf.Abs(distance.y) < tabRange)
            {
                Debug.Log("Tab");
            }
        }
    }
    private void MovePositionRow()
    {
        float beginPos = transform.position.x;
        Vector2 newPos = new Vector2(Mathf.Lerp(beginPos, manager.rowXPos[rowPos], .1f), transform.position.y);
        transform.position = newPos;

        if(transform.position.x == manager.rowXPos[rowPos])
        {
            rowChanged = false;
        }
    }
    public void SetBoolRowChange(int newRow)
    {
        rowPos = newRow;
        rowChanged = true;
    }

    // Traps ---------------------------------------------------------------------------------------------------------------
    public void SlowMovement()
    {
        if (!slowEffectIsActive && !isDead)
        {
            //audio.Play("Fall");
            // Slowdown player
            slowEffectIsActive = true;
            playerSpeed = playerDefaultSpeed * trapSpeed;
            playerSwipeSpeed = playerDefaultSwipeSpeed * trapSpeed;
            StartCoroutine(TrapActive());
        }
    }
    private IEnumerator TrapActive()
    {
        // Return player speed after few second
        yield return new WaitForSeconds(trapTime);
        if (!isDead)
        {
            slowEffectIsActive = false;
            playerSpeed = playerDefaultSpeed;
            playerSwipeSpeed = playerDefaultSwipeSpeed;
        }
    }
    public void Dead()
    {
        string[] a = { "PlayerDead", transform.position.x.ToString(), transform.position.y.ToString() };
        network.SendMassageClient("AllES", a);
        DeadMethod();
    }
    public void DeadMethod()
    {
        if (!isDead)
        {
            //audio.Stop("Run");
            //audio.Play("Scream");
            isDead = true;
            // Player dead
            if (playerName == network.MyName)
            {
                manager.GameOver(false, 0);
                playerSpeed = 0f;
                if (animator != null)
                    animator.SetFloat("Speed", playerSpeed);
            }
            else
            {
                playerSpeed = 0;
                if (animator != null)
                    animator.SetFloat("Speed", playerSpeed);
            }
        }
    }
    public void GetCoin(int value)
    {
        GameDataLoader.TheData.Coin += value;
        coinText.text = GameDataLoader.TheData.Coin.ToString("n0");
    }
    public void Disconnected()
    {
        Color dark = new Color(80 / 255f, 80 / 255f, 80 / 255f);

        if (playerName == network.MyName)
        {

        }
        else
        {
            isDisconnect = true;
            gameObject.GetComponent<SpriteRenderer>().color = dark;
            disconnectLogo.SetActive(true);
            playerSpeed = 0;
        }
    }
    public void SyncPos(float x, float y)
    {
        if(Mathf.Abs(gameObject.transform.position.y - y) > .8f)
        {
            transform.position = new Vector2(x, y);
        }
    }
    // UI -----------------------------------------------------------------------------------
    public void ChangePlayerOrder(int value)
    {
        PlayerOrder = value;
        orderText.text = value.ToString();
    }

}
