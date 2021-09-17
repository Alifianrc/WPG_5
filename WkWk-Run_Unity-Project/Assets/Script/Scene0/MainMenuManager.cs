using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject selectNamePanel;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject matchmakingPanel;

    // Game data
    [HideInInspector] public SaveData theData { get; set; }

    // Network manager
    private Client network;

    void Start()
    {
        network = FindObjectOfType<Client>();        

        // Load game data
        theData = SaveGame.LoadData();

        // Setting Panels
        connectingPanel.SetActive(true);
        if(theData.UserName == "null")
        {
            selectNamePanel.SetActive(true);
        }
        else
        {
            selectNamePanel.SetActive(false);
        }

        // Checking connection regulary
        StartCoroutine(CheckConnection());
    }
    
    // Method for play button
    public void PlayButton()
    {
        // Join Lobby
        network.SendMassageClient("Server", "JoinLobby");
        matchmakingPanel.SetActive(true);
    }
    public void OnJoinedLobby()
    {
        matchmakingPanel.SetActive(true);
    }
    public void OnJoinedRoom()
    {
        SceneManager.LoadScene(1);
    }
    

    // Checking connection regulary
    private IEnumerator CheckConnection()
    {
        while (true)
        {
            if (network.isConnected)
            {
                connectingPanel.SetActive(false);
            }
            else
            {
                connectingPanel.SetActive(true);
            }
            
            // Delay
            yield return new WaitForSeconds(2);
        }
    }

    void Update()
    {
        
    }
}
