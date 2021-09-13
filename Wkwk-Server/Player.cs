﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wkwk_Server
{
    class Player
    {
        // Name
        public string playerName;
        // Socket
        public TcpClient tcp;

        // List of online player (not in lobby or room)
        private List<Player> onlineList;
        // List of player want to play / queue
        private List<Player> lobbyList;
        // List of room of the Games
        private List<Room> roomList;
        // List of disconnected player
        private List<Player> disconnectedList;

        // Player position
        // 0 = onlineList
        // 1 = lobbyList
        // 2 = roomList
        // 3 = disconnectedList
        public int listPosition;

        // Stream
        NetworkStream stream;
        // This player room
        private Room myRoom;

        public Player(TcpClient tcp, List<Player> onlineList, List<Player> lobbyList, List<Room> roomList, List<Player> disconnectedList)
        {
            this.tcp = tcp;
            this.onlineList = onlineList;
            this.lobbyList = lobbyList;
            this.roomList = roomList;
            this.disconnectedList = disconnectedList;

            stream = tcp.GetStream();
        }

        public void StartReceiving()
        {
            Thread recieveThread = new Thread(RecievedMassage);
            recieveThread.Start();
        }

        private void RecievedMassage()
        {
            while (true)
            {
                if (stream.DataAvailable)
                {
                    // Format recieved : ToWho|Data1|Data2|... 
                    BinaryFormatter formatter = new BinaryFormatter();
                    string data = formatter.Deserialize(stream) as string;
                    string[] info = data.Split("|");

                    if (info[0] == "All")
                    {

                    }
                    else if (info[0] == "AllES")
                    {

                    }
                    else if (info[0] == "Server")
                    {
                        switch (info[1])
                        {
                            case "Check":
                                SendMassage(playerName, "Check");
                                break;
                            case "JoinLobby":
                                JoinLobby();
                                break;
                            case "CreateRoom":
                                if (info.Length > 4)
                                {
                                    CreateRoom(info[2], info[3], info[4]);
                                }
                                else
                                {
                                    CreateRoom();
                                }
                                break;
                            case "JoinRoom":
                                JoinRoom(info[2]);
                                break;
                            case "ExitLobby":
                                ExitLobby();
                                break;
                            case "ExitRoom":
                                ExitRoom();
                                break;
                            default:

                                break;
                        }
                    }
                }
            }
        }

        private void SendMassage(string target, string massage)
        {
            string[] data = new string[1];
            data[0] = massage;
            SendMassage(target, data);
        }
        private void SendMassage(string target, string[] massage)
        {
            // Send to this player
            if (target == playerName)
            {
                // send format : FromWho|Data1|Data2|...
                string data = "Server";
                // Add data
                for (int i = 0; i < massage.Length; i++)
                {
                    data += "|" + massage[i];
                }
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
            }

            // Send to All Player in room


            // Send to All Player in room excep sender (this)

        }

        private void JoinLobby()
        {
            foreach (Player a in onlineList)
            {
                if (a.tcp == tcp)
                {
                    // Remove from online list
                    onlineList.Remove(a);

                    // Add to Lobby list
                    lobbyList.Add(a);
                    listPosition = 1;

                    // Send massage to client
                    SendMassage(playerName, "JoinedLobby");

                    // Print massage
                    Console.WriteLine(playerName + " : Join to Lobby");

                    return;
                }
            }
        }

        // Create room auto
        public void CreateRoom()
        {
            CreateRoom(playerName + "Room", "5", "1");
        }
        // Create custom private room
        public void CreateRoom(string roomName, string playerMax, string isPublic)
        {
            // Generate room
            Room temp = new Room();
            temp.roomName = roomName;
            temp.MaxPlayer = int.Parse(playerMax);
            temp.isPublic = StringToBool(isPublic);
            temp.playerList = new List<Player>();

            if(listPosition == 0)
            {
                // Moving player from online to room
                foreach (Player a in onlineList)
                {
                    if (a.tcp == tcp)
                    {
                        // Remove from online list
                        onlineList.Remove(a);

                        // Add to room list
                        temp.playerList.Add(a);
                        listPosition = 2;

                        // Save room
                        myRoom = temp;

                        // Send massage to client
                        SendMassage("Server", "JoinedRoom");

                        // Print massage in server
                        Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                        return;
                    }
                }
            }
            else if(listPosition == 1)
            {
                // Moving player from lobby to room
                foreach (Player a in lobbyList)
                {
                    if (a.tcp == tcp)
                    {
                        // Remove from lobby list
                        lobbyList.Remove(a);

                        // Add to room list
                        temp.playerList.Add(a);
                        listPosition = 2;

                        // Save room
                        myRoom = temp;

                        // Send massage to client
                        SendMassage(playerName, "JoinedRoom");

                        // Print massage in server
                        Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                        return;
                    }
                }
            }
        }

        // Join other players room
        public void JoinRoom(string roomName)
        {
            foreach (Room a in roomList)
            {
                if (a.roomName == roomName && a.canJoin)
                {
                    onlineList.Remove(this);
                    a.playerList.Add(this);
                    listPosition = 2;
                    a.CheckRoom();
                    myRoom = a;
                }
            }
        }

        // Exit from lobby
        private void ExitLobby()
        {
            foreach (Player a in lobbyList)
            {
                if (a.tcp == tcp)
                {
                    // Remove from online list
                    lobbyList.Remove(a);

                    // Add to Lobby list
                    onlineList.Add(a);
                    listPosition = 0;
                }
            }

            // Print massage
            Console.WriteLine(playerName + " : Exit from Lobby");
        }

        // Exit from room
        public void ExitRoom()
        {
            foreach (Player a in myRoom.playerList)
            {
                if (a.tcp == tcp)
                {
                    // Remove from player list in room
                    myRoom.playerList.Remove(a);

                    // Add to online list
                    onlineList.Add(a);
                    listPosition = 0;

                    myRoom.CheckRoom();
                    myRoom = null;
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
    }
}
