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

    // UI in Main Panel
    [SerializeField] private Image mainMenuCharImage;
    [SerializeField] private Text coinText;
    [SerializeField] public Text nameText;

    // Sprite list
    [SerializeField] public Sprite[] characterSprite;

    void Start()
    {
        network = FindObjectOfType<Client>();

        // Load game data
        theData = network.TheData;

        // Setting Panels
        connectingPanel.SetActive(true);
        if(theData.UserName == "")
        {
            selectNamePanel.SetActive(true);
        }
        else
        {
            selectNamePanel.SetActive(false);
        }

        // Set UI
        mainMenuCharImage.sprite = characterSprite[theData.selectedChar];
        coinText.text = theData.Coin.ToString("n0");
       
        nameText.text = theData.UserName;

        // Audio

        // Checking connection regulary
        StartCoroutine(CheckConnection());
    }
    
    // Method for play button
    public void PlayButton()
    {
        // Join Lobby
        network.SendMassageClient("Server", "Play");
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
    public IEnumerator CheckConnection()
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


    // UI
    public void ChangeSelectedCharacter(int value)
    {
        mainMenuCharImage.sprite = characterSprite[value];
    }
    public void ChangeCoinValue(int value)
    {
        coinText.text = value.ToString("n0");
    }

    // Exit Games
    public void ExitGame()
    {
        Application.Quit();
    }
}
