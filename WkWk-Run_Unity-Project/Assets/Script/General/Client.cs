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
    private int port = 1313;

    public bool isConnected { get; private set; }

    // Check connection timer
    float CheckTime = 10;
    float checkCountDown;

    void Start()
    {
        // Never destroy this object
        DontDestroyOnLoad(gameObject);

        client = new TcpClient();

        checkCountDown = CheckTime;

        try
        {
            client.Connect(IPAddress.Loopback, port);
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

        StartCoroutine(CheckConnection());
    }

    // Try connecting to server
    private IEnumerator TryConnecting()
    {
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

            // Wait 2 second and try agaian
            yield return new WaitForSeconds(2);
        }
    }

    private IEnumerator CheckConnection()
    {
        while (true)
        {
            yield return new WaitForSeconds(CheckTime / 2);

            if (client.Connected)
            {
                SendMassage("Server", "Check");
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
                    SendMassage("Server", SaveGame.LoadData().UserName);
                    break;
                case "Check":
                    // Connection check success
                    checkCountDown = CheckTime;
                    break;
                case "JoinedLobby":
                    // If joined in lobby
                    FindObjectOfType<MainMenuManager>().OnJoinedLobby();
                    break;
                case "JoinedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }
        else if(data[0] == "Client")
        {

        }

        // Debugging
        Debug.Log(massage);
    }

    public void SendMassage(string target, string massage)
    {
        string[] data = new string[1];
        data[0] = massage;
        SendMassage(target, data);
    }
    public void SendMassage(string target, string[] massage)
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
