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

        // Player position in server List
        // 0 = onlineList
        // 1 = lobbyList
        // 2 = roomList
        public int listPosition;

        // Stream
        public NetworkStream stream;
        // This player room
        private Room myRoom;
        // Is master of room
        private bool isMaster;
        // Is Online
        private bool isOnline;
        private bool needCheck;

       

        // Constructor needed
        public Player(TcpClient tcp, List<Player> onlineList, List<Player> lobbyList, List<Room> roomList)
        {
            this.tcp = tcp;
            this.onlineList = onlineList;
            this.lobbyList = lobbyList;
            this.roomList = roomList;

            stream = tcp.GetStream();
            isOnline = true;
            needCheck = true;
        }

        // Check player connection
        private void CheckConnection()
        {
            while (isOnline && needCheck)
            {
                Thread.Sleep(5000);
                SendMassage("Server", playerName, "SYNA");
            }
        }

        // Start receiving massage from this player
        public void StartReceiving()
        {
            Thread recieveThread = new Thread(RecievedMassage);
            Thread checkConnectionThread = new Thread(CheckConnection);
            recieveThread.Start();
            checkConnectionThread.Start();
        }
        private void RecievedMassage()
        {
            while (isOnline)
            {
                if (stream.DataAvailable)
                {
                    // Format received : ToWho|Data1|Data2|... 
                    BinaryFormatter formatter = new BinaryFormatter();
                    string data = formatter.Deserialize(stream) as string;
                    string[] info = data.Split("|");

                    if (info[0] == "All")
                    {
                        switch (info[1])
                        {
                            case "SpawnPlatform":
                                string[] mass = new string[] { "SpawnPlatform", info[2], info[3], info[4], info[5], info[6] };
                                SendMassage("Client", "All", mass);
                                break;
                            case "StartGame":
                                // Lock room
                                myRoom.SetCanJoin(false);
                                SendMassage("Client", "All", "StartGame");
                                Console.WriteLine(playerName + " : Start Game on Room " + myRoom.roomName);
                                break;
                            case "ChangeRow":
                                string[] massag = new string[] { "ChangeRow", playerName, info[2] };
                                SendMassage("Client", "All", massag);
                                break;
                            default:

                                break;
                        }
                    }
                    else if (info[0] == "AllES")
                    {
                        switch (info[1])
                        {
                            case "SyncPlr":
                                string[] mass = new string[] { "SyncPlr", playerName, info[2], info[3] };
                                SendMassage("Client", "AllES", mass);
                                break;
                            default:

                                break;
                        }
                    }
                    else if (info[0] == "Server")
                    {
                        switch (info[1])
                        {
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
                            case "ChangeName":
                                playerName = info[2];
                                break;
                            default:

                                break;
                        }
                    }
                    else
                    {
                        for(int i = 0; i < myRoom.playerList.Count; i++)
                        {
                            if (myRoom.playerList[i].playerName == info[0])
                            {
                                switch (info[1])
                                {
                                    case "SpawnMyPlayer":
                                        string[] theData = new string[] { "SpawnPlayer", info[2], info[3], BoolToString(false) };
                                        SendMassage("Client", myRoom.playerList[i].playerName, theData);
                                        // Print massage
                                        Console.WriteLine(playerName + " : Send Object Player to " + myRoom.playerList[i].playerName);
                                        break;
                                    default:

                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Send massage
        private void SendMassage(string fromWho, string target, string massage)
        {
            string[] data = new string[1];
            data[0] = massage;
            SendMassage(fromWho, target, data);
        }
        private void SendMassage(string fromWho, string target, string[] massage)
        {
            if (isOnline)
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

                    SendSerializationDataHandler(stream, data);
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

                    // Send to all
                    for(int i = 0; i < myRoom.playerList.Count; i++)
                    {
                        SendSerializationDataHandler(myRoom.playerList[i].stream, data);
                    }
                }

                // Send to All Player in room excep sender (this)
                else if (target == "AllES")
                {
                    for (int i = 0; i < myRoom.playerList.Count; i++)
                    {
                        if (myRoom.playerList[i].playerName != playerName)
                        {
                            // send format : FromWho|Data1|Data2|...
                            string data = fromWho;
                            // Add data
                            for (int j = 0; j < massage.Length; j++)
                            {
                                data += "|" + massage[j];
                            }
                            // Send massage
                            SendSerializationDataHandler(myRoom.playerList[i].stream, data);
                        }
                    }
                }

                // Send to specific client
                else
                {
                    for (int i = 0; i < myRoom.playerList.Count; i++)
                    {
                        if(myRoom.playerList[i].playerName == target)
                        {
                            // send format : FromWho|Data1|Data2|...
                            string data = fromWho;
                            // Add data
                            for (int j = 0; j < massage.Length; j++)
                            {
                                data += "|" + massage[j];
                            }

                            // Send massage
                            SendSerializationDataHandler(myRoom.playerList[i].stream, data);
                        }
                    }
                }
            }
        }
        private void SendSerializationDataHandler(NetworkStream Thestream, string Thedata)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(Thestream, Thedata);
            }
            catch(Exception e)
            {
                Console.WriteLine("Send massage error from " + playerName + " : " + e.Message);
                // Disconnect client from server
                DisconnectFromServer();
            }
        }

        // Room and Lobby --------------------------------------------------------------------------
        // Join lobby method
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

                    needCheck = true;

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
            temp.SetCanJoin(true);

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
                        isMaster = true;
                        roomList.Add(myRoom);

                        needCheck = false;

                        // Send massage to client
                        SendMassage("Server", playerName, "CreatedRoom");

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
                        isMaster = true;
                        roomList.Add(myRoom);

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

                    needCheck = false;

                    // Send massage to client
                    SendMassage("Server", playerName, "JoinedRoom");

                    // Print massage
                    Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                    return;
                }
            }

            // If not join room
            SendMassage("Server", playerName, "RoomNotFound");
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

                    needCheck = true;
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

                    needCheck = true;

                    // Print massage
                    Console.WriteLine(playerName + " : Exit from room " + myRoom.roomName);
                }
            }
        }

        // Others Method -----------------------------------------------------------------------------------------------
        // Spawning player in random start position (0-4)
        private void SpawnPlayer()
        {
            // Random pos
            Random rand = new Random();
            int randPos = rand.Next(5);

            // If it's used
            while(myRoom.randomPosUsed[randPos])
            {
                randPos = rand.Next(5);
            }

            // If it's not
            myRoom.randomPosUsed[randPos] = true;

            // If it's not
            string[] massage = new string[] { "SpawnPlayer", playerName, randPos.ToString(), BoolToString(true) };
            // Send massage to all player
            SendMassage("Server", "All", massage);

            // Print massage in server
            Console.WriteLine(playerName + " : Request Spawn Player, Get-" + randPos);
        }
       
        // Disconnect from server
        private void DisconnectFromServer()
        {
            // Print massage
            Console.WriteLine(playerName + " : Disconnected from server");
            isOnline = false;
            if (listPosition == 0)
            {
                Server.DisconnectFromServer(this, onlineList);
            }
            else if (listPosition == 1)
            {
                Server.DisconnectFromServer(this, lobbyList);
            }
            else if (listPosition == 2)
            {
                // If this is master, set other player to master
                if (isMaster)
                {
                    for(int i = 0;i < myRoom.playerList.Count; i++)
                    {
                        if(myRoom.playerList[i].playerName != playerName)
                        {
                            myRoom.playerList[i].SetToMaster();
                        }
                    }
                }

                Server.DisconnectFromServer(this, myRoom, roomList);
            }
        }

        // Set this player to master
        public void SetToMaster()
        {
            isMaster = true;
            SendMassage("Server", playerName, "SetToMaster");
            // print massage
            Console.WriteLine(playerName + " : Become a master of room " + myRoom.roomName);
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
}
