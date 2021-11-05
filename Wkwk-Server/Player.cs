using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
        // List of room of the Games
        private List<Room> roomList;

        // Player position in server List
        // 1 = onlineList
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

        // Encryption
        RsaEncryption rsaEncryption;
        AesEncryption aesEncryption;

        // Private key
        private string ServerPrivateKey;

        // Constructor needed
        public Player(TcpClient tcp, List<Player> onlineList, List<Room> roomList)
        {
            this.tcp = tcp;
            this.onlineList = onlineList;
            this.roomList = roomList;

            stream = tcp.GetStream();
            isOnline = true;

            aesEncryption = new AesEncryption();

            // Load private key
            ServerPrivateKey = File.ReadAllText(Directory.GetCurrentDirectory() + "\\Private-Key.txt");
            rsaEncryption = new RsaEncryption(ServerPrivateKey);

            // Preparation
            PrepareEncryption();
            // Ask for name and start listening
            AskPlayerName();
        }

        // Preparation Method -------------------------------------------------------------------------
        private void PrepareEncryption()
        {
            // Wait to receive client public key
            BinaryFormatter formatter = new BinaryFormatter();
            string answer = formatter.Deserialize(stream) as string;

            // Decrypt it
            string key = rsaEncryption.Decrypt(answer, rsaEncryption.serverPrivateKey);

            // Save client public key
            rsaEncryption.SetClientPublicKey(key);

            // Create new symmetric key
            aesEncryption.GenerateNewKey();

            // Send it to client
            string encryptKey = rsaEncryption.Encrypt(aesEncryption.ConvertKeyToString(aesEncryption.aesKey), rsaEncryption.clientPublicKey);
            formatter.Serialize(stream, encryptKey);
        }
        private void AskPlayerName()
        {
            // Make the massage
            string massage = aesEncryption.Encrypt("Server|WHORU");
            
            // Send it
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, massage);

            // Waiting for answer
            string answer = aesEncryption.Decrypt(formatter.Deserialize(stream) as string);
            string[] info = answer.Split("|");

            // Add this player to list
            playerName = info[1];
            onlineList.Add(this);
            listPosition = 1;

            // Start listenign player
            StartReceiving();

            // Print massage in console
            Console.WriteLine("Server : Client " + playerName + " is online");
        }

        // Check player connection ---------------------------------------------------------------------
        private void CheckConnection()
        {
            while (isOnline)
            {
                Thread.Sleep(5000);
                SendMassage("Server", playerName, "SYNA");
            }
        }

        // Start receiving massage from this player ----------------------------------------------------
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
                    string data = aesEncryption.Decrypt(formatter.Deserialize(stream) as string);
                    string[] info = data.Split("|");

                    // Send data to all
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
                    // Send data to all excep sender
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
                    // Send data to server (this)
                    else if (info[0] == "Server")
                    {
                        switch (info[1])
                        {
                            case "Play":
                                // Start matchmaking (actually just finding room)
                                MatchMaking();
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
                    // Send data to specific player
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

        // Send massage ---------------------------------------------------------------------------------
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

                    SendSerializationDataHandler(stream, aesEncryption.Encrypt(data));
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
                        SendSerializationDataHandler(myRoom.playerList[i].stream, aesEncryption.Encrypt(data));
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
                            SendSerializationDataHandler(myRoom.playerList[i].stream, aesEncryption.Encrypt(data));
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
                            SendSerializationDataHandler(myRoom.playerList[i].stream, aesEncryption.Encrypt(data));
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

        // Room ------------------------------------------------------------------------------------------
        // Matchmaking
        public void MatchMaking()
        {
            // Check if there is room in list
            if(roomList.Count > 0)
            {
                // Check if there is room can join
                for(int i = 0; i < roomList.Count; i++)
                {
                    if (roomList[i].canJoin)
                    {
                        onlineList.Remove(this);
                        roomList[i].playerList.Add(this);
                        listPosition = 2;
                        roomList[i].CheckRoom();
                        myRoom = roomList[i];

                        // Send massage to client
                        SendMassage("Server", playerName, "JoinedRoom|" + myRoom.roomName);

                        // Print massage
                        Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                        return;
                    }
                }
            }

            // Just make new room if there is no room can be joined
            CreateRoom();
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

            // Moving player from online to new room
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

                    // Send massage to client
                    SendMassage("Server", playerName, "CreatedRoom");

                    // Print massage in server
                    Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                    return;
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

                    // Send massage to client
                    SendMassage("Server", playerName, "JoinedRoom|" + myRoom.roomName);

                    // Print massage
                    Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                    return;
                }
            }

            // If not join room
            SendMassage("Server", playerName, "RoomNotFound");
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
            if (listPosition == 1)
            {
                Server.DisconnectFromServer(this, onlineList);
            }
            else if (listPosition == 2)
            {
                // If this is master, set other player to master
                if (isMaster)
                {
                    for(int i = 0; i < myRoom.playerList.Count; i++)
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

        // Custom method to convert sting to bool -----------------------------------------------------------------
        // 0 = false ; 1,2,3...n = true
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
