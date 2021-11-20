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
    // 127.0.0.1
    // 45.130.229.104

    // Player and Room name

    public string MyName { get; set; }
    public string roomName { get; set; }

    // Connection status
    public bool isConnected { get; private set; }
    private bool isReady = false;

    // Check connection timer
    float CheckTime = 8;
    float checkCountDown;

    // Master of room
    [SerializeField] public bool isMaster;

    // All players (in room)
    private List<PlayerManager> playerList;
    private PlayerManager myPlayer;

    RsaEncryption rsaEncryption;
    AesEncryption aesEncryption;

    void Start()
    {
        // Never destroy this object
        DontDestroyOnLoad(gameObject);

        MyName = GameDataLoader.TheData.UserName;

        client = new TcpClient();
        checkCountDown = CheckTime;
        playerList = new List<PlayerManager>();

        rsaEncryption = new RsaEncryption();
        aesEncryption = new AesEncryption();
 
        StartCoroutine(TryConnecting());
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
                client.Connect(ipAd, port);
                networkStream = client.GetStream();
                PrepareEncryption();

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
        if (client.Connected && isReady)
        {
            if (networkStream.DataAvailable)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ReceiveMassage(formatter.Deserialize(networkStream) as string);
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

    // Preparation ---------------------------------------------------------------------------------------------
    private void PrepareEncryption()
    {
        // Send client public key to server
        BinaryFormatter formatter = new BinaryFormatter();
        string key = rsaEncryption.ConvertKeyToString(rsaEncryption.clientPublicKey);
        string encrypKey = rsaEncryption.Encrypt(key, rsaEncryption.serverPublicKey);
        formatter.Serialize(networkStream, encrypKey);

        // Wait for new key
        string answer = formatter.Deserialize(networkStream) as string;
        string aesKey = rsaEncryption.Decrypt(answer, rsaEncryption.clientPrivateKey);

        // Save the new key
        aesEncryption.SetKey(aesEncryption.ConvertStringToKey(aesKey));

        // Ready to communicate
        isReady = true;
        isConnected = true;
    }

    // Receiving Massage ---------------------------------------------------------------------------------------
    private void ReceiveMassage(string massage)
    {
        // Decrypt
        string decryptMassage = aesEncryption.Decrypt(massage);

        // Debugging
        //Debug.Log(decryptMassage);

        // receive format : Sender|Data1|Data2|...
        string[] data = decryptMassage.Split('|');
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
                case "CreatedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    // If creating room, auto room owner (master)
                    isMaster = true;
                    break;
                case "JoinedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    roomName = data[2];
                    break;
                case "RoomNotFound":
                    FindObjectOfType<JoinRoomPanel>().RoomNotFound();
                    break;
                case "SpawnPlayer":
                    // Spawn player
                    SpawnPlayer(data[2], int.Parse(data[3]), int.Parse(data[4]), StringToBool(data[5]));
                    break;
                case "SetToMaster":
                    isMaster = true;
                    break;
                case "ExitRoom":
                    FindObjectOfType<GameManager>().OnExitRoom();
                    break;
                case "Disconnect":
                    foreach (PlayerManager a in playerList)
                    {
                        // Refresh player position
                        if (a.playerName == data[2])
                        {
                            // Do something to disconnect player

                        }
                    }
                    break;
                default:
                    Debug.Log("Unreconized massage : " + decryptMassage);
                    break;
            }
        }
        else if(data[0] == "Client")
        {
            switch (data[1])
            {
                case "SpawnPlayer":
                    SpawnPlayer(data[2], int.Parse(data[3]), int.Parse(data[4]), StringToBool(data[5]));
                    break;
                case "SpawnObstacle":
                    int[] platformData = new int[] { int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]), };
                    FindObjectOfType<GameManager>().SpawnObstacle(platformData);
                    break;
                case "SyncPlr":
                    foreach(PlayerManager a in playerList)
                    {
                        // Refresh player position
                        if(a.playerName == data[2])
                        {
                            Debug.Log("Sync Player : " + data[2]);
                            // Sync player here
                            //a.gameObject.transform.position = new Vector2(int.Parse(data[3]), int.Parse(data[4]));
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
                case "SpawnCoin":
                    FindObjectOfType<GameManager>().SpawnCoin(int.Parse(data[2]), int.Parse(data[3]));
                    break;
                case "SpawnBooster":
                    FindObjectOfType<GameManager>().SpawnBooster(int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]));
                    break;
                case "PlayerDead":
                    foreach (PlayerManager a in playerList)
                    {
                        // Refresh player position
                        if (a.playerName == data[2])
                        {
                            // Do something to dead player

                        }
                    }
                    break;
                default:
                    Debug.Log("Unreconized massage : " + decryptMassage);
                    break;
            }
        }
    }

    // Sending Massage -----------------------------------------------------------------------------------------
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
        formatter.Serialize(networkStream, aesEncryption.Encrypt(data));
    }

    // General Method ------------------------------------------------------------------------------------------
    private void SpawnPlayer(string name, int row, int skin, bool needFeedBack)
    {
        GameManager manager = FindObjectOfType<GameManager>();
        PlayerManager tempPlay = Instantiate(manager.playerPrefab[skin]).GetComponent<PlayerManager>();
        tempPlay.playerName = name;
        tempPlay.rowPos = row;
        playerList.Add(tempPlay);
        Debug.Log(name);
        // Check is it's mine
        if(name == MyName)
        {
            myPlayer = tempPlay;
            return;
        }
        else if (name != MyName && needFeedBack)
        {
            // Send Feedback
            string[] mass = new string[] { "SpawnMyPlayer", myPlayer.playerName, myPlayer.rowPos.ToString(), GameDataLoader.TheData.selectedChar.ToString(), BoolToString(false) };
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

    // Custom method to convert sting to bool ----------------------------------------------------------------------------
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
