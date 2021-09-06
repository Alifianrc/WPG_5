using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    // Panels
    [SerializeField] private GameObject selectNamePanel;
    [SerializeField] private GameObject connectingPanel;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject createRoomPanel;

    // Game data
    public SaveData theData { get; set; }


    void Start()
    {
        // Connecting so server
        PhotonNetwork.ConnectUsingSettings();

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


    }

    public override void OnConnectedToMaster()
    {
        // Setting Panels
        connectingPanel.SetActive(false); Debug.Log("Connected");
    }
    
    // Method for play button
    public void PlayButton()
    {
        // Setting Player Name
        PhotonNetwork.NickName = theData.UserName;

        // Join lobby
        TypedLobby customLobby = new TypedLobby("WkWkLobby", LobbyType.Default);
        PhotonNetwork.JoinLobby(customLobby);

        // Activate matchmaking panel
        matchmakingPanel.SetActive(true);
    }

    // If Joined To Lobby
    public override void OnJoinedLobby()
    {
        // Join random room
        PhotonNetwork.JoinRandomRoom();
    }
    // If Join Room Failed
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // And it's because there is no room
        if(message == "No match found")
        {
            // Make a new room
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = 5;
            PhotonNetwork.CreateRoom( "", roomOptions, TypedLobby.Default);
        }
    }
    // If Joined to room
    public override void OnJoinedRoom()
    {
        // Load Game Scene
        SceneManager.LoadScene(1);

        Debug.Log("Joined to Room " + PhotonNetwork.CurrentRoom.Name);
    }

    // Method for Crate Room Button
    public void CreateRoomButton()
    {
        matchmakingPanel.SetActive(true);
    }

    void Update()
    {
        
    }
}
