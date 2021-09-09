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
    GameManager manager;

    private void Start()
    {
        startTime = Time.time;

        manager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    private void Update()
    {
       
    }

    // Manually start the games
    public void StartGamesManual()
    {
        StartGame();
    }


    public void StartGame()
    {
        // Start the games

        // Set Panel
        //waitingPlayerPanel.SetActive(false);
        Destroy(waitingPlayerPanel);

        // Multiplayer
        // Lock room (new player can't join)

        // Spawn player character
        manager.SpawnPlayer();

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

        Destroy(gameObject);
    }

}
