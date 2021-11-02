using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Class to control preparation of the games

public class StartPanel : MonoBehaviour
{
    // UI
    [SerializeField] private Text startText;
    [SerializeField] private GameObject waitingPlayerPanel;
    [SerializeField] private GameObject startManualButton;

    // Parameter
    [SerializeField] private float PlayerWaitingTime = 10f;
    [SerializeField] private float countDownTime = 4f;
    private float startTime;

    // Game manager
    private GameManager manager;
    private Client network;

    private void Start()
    {
        startTime = Time.time;

        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();

        //startManualButton.SetActive(false);

        StartCoroutine(CheckRoom());
    }

    // Update is called once per frame
    private void Update()
    {
       
    }

    // Check room condition
    IEnumerator CheckRoom()
    {
        while (!manager.GameIsStarted)
        {
            // Check room condition
            if (network.isMaster && network.PlayerCountInRoom() >= 2)
            {
                try
                {
                    startManualButton.SetActive(true);
                }
                catch
                {

                }
            }

            yield return new WaitForSeconds(2);
        }
    }

    // For manually start the game
    public void StartGameServer()
    {
        // Send massage to all player that the game is started
        network.SendMassageClient("All", "StartGame");
    }

    // Start the games
    public void StartGame()
    {
        // Start the games

        // Set Panel
        Destroy(waitingPlayerPanel);

        // Begin count down
        StartCoroutine(CountDownStart());
    }

    private IEnumerator CountDownStart()
    {
        // Start count down
        while(countDownTime > 0)
        {
            if(countDownTime - 1 == 0)
            {
                startText.text = "RUN!";
            }
            else
            {
                startText.text = (((int)countDownTime) - 1).ToString();
            }

            yield return new WaitForSeconds(1);

            countDownTime--;
        }

        // Set Bool
        manager.GameIsStarted = true;

        // Begin Couretine
        manager.StartSpawnPlatform();
        network.StartSyncPlayer();

        Destroy(gameObject);
    }

}
