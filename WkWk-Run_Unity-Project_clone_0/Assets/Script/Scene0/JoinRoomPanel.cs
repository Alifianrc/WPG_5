﻿using UnityEngine;
using UnityEngine.UI;

public class JoinRoomPanel : MonoBehaviour
{
    [SerializeField] private InputField inputRoomName;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject roomNotFound;
    private Client network;

    private void Start()
    {
        network = FindObjectOfType<Client>();
        confirmButton.SetActive(false);
        roomNotFound.SetActive(false);
    }

    // Check name character size
    public void MaxMinName()
    {
        if (inputRoomName.text.Length < 4 || inputRoomName.text.Length > 15)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }
    }

    // Join Room
    public void JoinRoom()
    {
        StartPanel.RoomName = inputRoomName.text;
        string[] massage = new string[] { "JoinRoom", inputRoomName.text };
        network.SendMassageClient("Server", massage);
    }

    // If room not found
    public void RoomNotFound()
    {
        roomNotFound.SetActive(true);
    }
}
