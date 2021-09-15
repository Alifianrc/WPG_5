﻿using System;
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
    private int port = 1313;
    public IPAddress ipAd = IPAddress.Parse("127.0.0.1");
    // 182.253.90.115
    // 127.0.0.1

    public bool isConnected { get; private set; }

    // Check connection timer
    float CheckTime = 5;
    float checkCountDown;

    // Master of room
    public bool isMaster { get; private set; }

    // Checking connection
    bool needCheck;

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

            // Try connecting again and again
            StartCoroutine(TryConnecting());
        }
    }

    // Try connecting to server
    private IEnumerator TryConnecting()
    {
        // Wait 2 second and try agaian
        yield return new WaitForSeconds(2);

        int count = 0;
        while (!client.Connected)
        {
            count++;
            try
            {
                client.Connect(IPAddress.Loopback, port);
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
        }

        // Checking connection
        if(checkCountDown > 0)
        {
            checkCountDown -= Time.deltaTime;
            isConnected = true;
        }
        else
        {
            isConnected = false;
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
                    SendMassageClient("Server", SaveGame.LoadData().UserName);
                    needCheck = true;
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
                case "SpawnPlayer":
                    // Spawn player
                    SpawnPlayer(data[2], int.Parse(data[3]), true);
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
                    SpawnPlayer(data[2], int.Parse(data[3]), false);
                    break;
                case "SpawnPlatform":
                    int[] platformData = new int[] { int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]), };
                    FindObjectOfType<GameManager>().SpawnPlatformGames(platformData);
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }
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

    private void SpawnPlayer(string name, int row, bool needFeedback)
    {
        PlayerManager tempPlay = Instantiate(playerPrefab).GetComponent<PlayerManager>();
        tempPlay.playerName = name;
        tempPlay.rowPos = row;
        playerList.Add(tempPlay);

        // Check is it's mine
        if(name == SaveGame.LoadData().UserName)
        {
            myPlayer = tempPlay;
        }
        else if (needCheck)
        {
            // Send Feedback
            string[] mass = new string[] { "SpawnMyPlayer", myPlayer.playerName, myPlayer.rowPos.ToString() };
            SendMassageClient(name, mass);
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
