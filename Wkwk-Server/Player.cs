using System;
using System.Collections.Generic;
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

        // Player position in server List
        // 0 = onlineList
        // 1 = lobbyList
        // 2 = roomList
        // 3 = disconnectedList
        public int listPosition;

        // Stream
        public NetworkStream stream;
        // This player room
        private Room myRoom;
        // Is master of room
        private bool isMaster;

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
                            case "SYN":
                                SendMassage("Server", playerName, "SYNA");
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
                            case "SpawnPlayer":
                                SpawnPlayer();
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
                    else
                    {
                        foreach(Player player in myRoom.playerList)
                        {
                            // Forwarding massage from clint A to client B
                            if (info[0] == player.playerName)
                            {
                                if (info[1] == "SpawnPlayerToOther")
                                {
                                    // Client, SpawnPlayer, player.playername, rowPos
                                    string[] msg = new string[] { "SpawnPlayer", info[0], info[2] };
                                    SendMassage(playerName, info[0], msg);
                                    // Print massage
                                    Console.WriteLine(playerName + " : Send massage 'SpawnPlayerToOther' to " + info[0]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SendMassage(string fromWho, string target, string massage)
        {
            string[] data = new string[1];
            data[0] = massage;
            SendMassage(fromWho, target, data);
        }
        private void SendMassage(string fromWho, string target, string[] massage)
        {
            // Send to this player
            if (target == playerName)
            {
                // send format : FromWho|Data1|Data2|...
                string data = fromWho;
                // Add data
                for (int i = 0; i < massage.Length; i++)
                {
                    data += "|" + massage[i];
                }
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
            }

            // Send to All Player in room
            else if (target == "All")
            {
                // send format : FromWho|Data1|Data2|...
                string data = fromWho;
                // Add data
                for (int i = 0; i < massage.Length; i++)
                {
                    data += "|" + massage[i];
                }
                BinaryFormatter formatter = new BinaryFormatter();
                // Send to all
                foreach(Player player in myRoom.playerList)
                {
                    formatter.Serialize(player.stream, data);
                }
            }

            // Send to All Player in room excep sender (this)
            else
            {
                foreach (Player player in myRoom.playerList)
                {
                    if(target == player.playerName)
                    {
                        // send format : FromWho|Data1|Data2|...
                        string data = fromWho;
                        // Add data
                        for (int i = 0; i < massage.Length; i++)
                        {
                            data += "|" + massage[i];
                        }
                        // Send massage
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(player.stream, data);
                    }
                }
            }
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
                    SendMassage("Server", playerName, "JoinedLobby");

                    // Print massage
                    Console.WriteLine(playerName + " : Join to Lobby");

                    // Start matchmaking
                    Server.Matchmaking(this, lobbyList, roomList);

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
                        SendMassage("Server", playerName, "JoinedRoom");

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
                        SendMassage("Server", playerName, "CreatedRoom");

                        // Print massage in server
                        Console.WriteLine(playerName + " : Created room " + myRoom.roomName);

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

                    // Print massage
                    Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);
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

                    // Print massage
                    Console.WriteLine(playerName + " : Exit from room " + myRoom.roomName);
                }
            }
        }

        private bool[] randomPosUsed = new bool [5];
        // Spawning player in random start position (0-4)
        private void SpawnPlayer()
        {
            // Random pos
            Random rand = new Random();
            int randPos = rand.Next(5);

            // If it's used
            while(randomPosUsed[randPos] == true)
            {
                randPos = rand.Next(5);
            }

            // If it's not
            string[] massage = new string[] { "SpawnPlayer", playerName, randPos.ToString()};
            // Send massage to all player
            SendMassage("Client", "All", massage);

            // Print massage in server
            Console.WriteLine(playerName + " : Request Spawn Player, Get-" + randPos);
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
