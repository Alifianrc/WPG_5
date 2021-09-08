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
    [SerializeField] private GameObject createRoomPanel;

    // Game data
    [HideInInspector] public SaveData theData { get; set; }

    // Max player in 1 room
    public static int MaxPlayerInRoom = 5;

    void Start()
    {
        // Connecting so server
        

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
    
    // Method for play button
    public void PlayButton()
    {
        // Setting Player Name
        

        // Activate matchmaking panel
        matchmakingPanel.SetActive(true);
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
