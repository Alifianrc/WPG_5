using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


// Class to control preparation of the games

public class StartPanel : MonoBehaviourPunCallbacks
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

        if (PhotonNetwork.IsMasterClient)
        {
            startManualButton.SetActive(true);
        }
        else
        {
            startManualButton.SetActive(false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Waiting for player or time
            if ((Time.time - startTime >= PlayerWaitingTime && PhotonNetwork.PlayerList.Length >= 2) || PhotonNetwork.PlayerList.Length == MainMenuManager.MaxPlayerInRoom)
            {
                photonView.RPC("StartGame", RpcTarget.All);
            }
        }
    }

    // Manually start the games
    public void StartGamesManual()
    {
        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    public void StartGame()
    {
        // Start the games

        // Set Panel
        //waitingPlayerPanel.SetActive(false);
        Destroy(waitingPlayerPanel);

        // Set Bool
        manager.GameIsStarted = true;

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

        Destroy(gameObject);
    }

}
