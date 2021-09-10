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
    private BinaryFormatter formatter;


    void Start()
    {
        // Never destroy this object
        DontDestroyOnLoad(gameObject);

        client = new TcpClient();
        formatter = new BinaryFormatter();

        try
        {
            client.Connect(IPAddress.Loopback, port);
            networkStream = client.GetStream();

            Debug.Log("Connected to server");
        }
        catch(Exception e)
        {
            Debug.Log("Client connecting error : " + e.Message);

            // Try connecting again and again
            StartCoroutine(TryConnecting());
        }
    }

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

    void Update()
    {
        if (client.Connected)
        {
            if (networkStream.DataAvailable)
            {
                RecieveMassage(formatter.Deserialize(networkStream) as string);
            }
        }
    }

    private void RecieveMassage(string massage)
    {
        // format : Sender|Massage
        string[] data = massage.Split('|');
        if(data[0] == "Server")
        {
            switch (data[1]) 
            {
                case "WHORU":
                    // Send player name
                    SendMassage("", "Server");
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }
        else if(data[0] == "Client")
        {

        }
    }

    private void SendMassage(string massage, string target)
    {
        string[] data = new string[1];
        data[0] = massage;
        SendMassage(data, target);
    }
    private void SendMassage(string[] massage, string target)
    {
        // format : To Who|The data
        string data = target + "|";
        // Add data
        for (int i = 0; i < massage.Length; i++)
        {
            data += massage[i] + "|";
        }
         
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
