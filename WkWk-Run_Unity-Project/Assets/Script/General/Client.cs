using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

public class Client : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream networkStream;
    private int port = 3002;
    public IPAddress ipAd = IPAddress.Parse("127.0.0.1");
    // 182.253.90.115
    // 127.0.0.1

    // Name 
    [HideInInspector] public SaveData TheData;
    [SerializeField] public string MyName;

    public bool isConnected { get; private set; }

    // Check connection timer
    float CheckTime = 8;
    float checkCountDown;

    // Master of room
    [SerializeField] public bool isMaster;

    // Player
    [SerializeField] private GameObject playerPrefab;
    private List<PlayerManager> playerList;
    private PlayerManager myPlayer;

    void Start()
    {
        // Never destroy this object
        DontDestroyOnLoad(gameObject);

        client = new TcpClient();

        checkCountDown = CheckTime;

        TheData = SaveGame.LoadData();
        MyName = TheData.UserName;

        playerList = new List<PlayerManager>();

        try
        {
            client.Connect(ipAd, port);
            networkStream = client.GetStream();

            isConnected = true;
            Debug.Log("Connected to server");
        }
        catch(Exception e)
        {
            Debug.Log("Client connecting error : " + e.Message);

            isConnected = false;

            // Try connecting again and again
            StartCoroutine(TryConnecting());
        }
    }

    // Try connecting to server
    private IEnumerator TryConnecting()
    {
        int count = 0;
        while (!client.Connected)
        {
            // Wait 2 second and try agaian
            yield return new WaitForSeconds(2);

            count++;
            try
            {
                // http://45.130.229.104:3002/
                // 192.168.66.92  45.130.229.104
                client.Connect("45.130.229.104", port);
                networkStream = client.GetStream();

                isConnected = true;
                Debug.Log("Connected to server");
            }
            catch (Exception e)
            {
                Debug.Log("Try connecting-" + count + " error : " + e.Message);
            }
        }
    }

    void Update()
    {
        if (client.Connected)
        {
            if (networkStream.DataAvailable)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                RecieveMassage(formatter.Deserialize(networkStream) as string);
            }

            // Checking connection
            if (checkCountDown > 0)
            {
                checkCountDown -= Time.deltaTime;
                isConnected = true;
            }
            else
            {
                isConnected = false;
            }
        }
    }

    private void RecieveMassage(string massage)
    {
        // recieve format : Sender|Data1|Data2|...
        string[] data = massage.Split('|');
        if(data[0] == "Server")
        {
            switch (data[1]) 
            {
                case "WHORU":
                    // Send player name
                    SendMassageClient("Server", MyName);
                    break;
                case "SYNA":
                    // Connection check success
                    checkCountDown = CheckTime;
                    break;
                case "JoinedLobby":
                    // If joined in lobby
                    FindObjectOfType<MainMenuManager>().OnJoinedLobby();
                    break;
                case "CreatedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    // If creating room, auto room owner (master)
                    isMaster = true;
                    break;
                case "JoinedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    break;
                case "RoomNotFound":
                    FindObjectOfType<JoinRoomPanel>().RoomNotFound();
                    break;
                case "SpawnPlayer":
                    // Spawn player
                    SpawnPlayer(data[2], int.Parse(data[3]), StringToBool(data[4]));
                    break;
                case "SetToMaster":
                    isMaster = true;
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }
        else if(data[0] == "Client")
        {
            switch (data[1])
            {
                case "SpawnPlayer":
                    SpawnPlayer(data[2], int.Parse(data[3]), StringToBool(data[4]));
                    break;
                case "SpawnPlatform":
                    int[] platformData = new int[] { int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]), };
                    FindObjectOfType<GameManager>().SpawnPlatformGames(platformData);
                    break;
                case "SyncPlr":
                    foreach(PlayerManager a in playerList)
                    {
                        // Refresh player position
                        if(a.playerName == data[2])
                        {
                            a.gameObject.transform.position = new Vector2(int.Parse(data[3]), int.Parse(data[4]));
                        }
                    }
                    break;
                case "StartGame":
                    FindObjectOfType<StartPanel>().StartGame();
                    Debug.Log("Game Started");
                    break;
                case "ChangeRow":
                    ChangePlayerRow(data[2], int.Parse(data[3]));
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }

        Debug.Log(massage);
    }

    public void SendMassageClient(string target, string massage)
    {
        string[] data = new string[1];
        data[0] = massage;
        SendMassageClient(target, data);
    }
    public void SendMassageClient(string target, string[] massage)
    {
        // send format : ToWho|Data1|Data2|...
        string data = target;
        // Add data
        for (int i = 0; i < massage.Length; i++)
        {
            data += "|" + massage[i];
        }

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(networkStream, data);
    }

    private void SpawnPlayer(string name, int row, bool needFeedBack)
    {
        PlayerManager tempPlay = Instantiate(playerPrefab).GetComponent<PlayerManager>();
        tempPlay.playerName = name;
        tempPlay.rowPos = row;
        playerList.Add(tempPlay);

        // Check is it's mine
        if(name == MyName)
        {
            myPlayer = tempPlay;
        }
        else if(needFeedBack)
        {
            // Send Feedback
            string[] mass = new string[] { "SpawnMyPlayer", myPlayer.playerName, myPlayer.rowPos.ToString(), BoolToString(false) };
            SendMassageClient(name, mass);
        }
    }

    public void StartSyncPlayer()
    {
        foreach(PlayerManager a in playerList)
        {
            if(a.playerName == myPlayer.playerName)
            {
                a.BeginSyncPos();
            }
        }
    }

    public int PlayerCountInRoom()
    {
        return playerList.Count;
    }

    public void ChangePlayerRow(string thePlayerName, int row)
    {
        foreach(PlayerManager a in playerList)
        {
            if(a.playerName == thePlayerName)
            {
                a.SetBoolRowChange(row);
            }
        }
    }

    // Custom method to convert sting to bool
    // 0 = false ; 1 = true
    private bool StringToBool(string a)
    {
        if (a == "0")
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private string BoolToString(bool a)
    {
        if (a == false)
        {
            return "0";
        }
        else
        {
            return "1";
        }
    }
}
