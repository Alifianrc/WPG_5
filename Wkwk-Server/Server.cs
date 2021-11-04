using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Wkwk_Server
{
    class Server
    {
        // List of online player (not in lobby or room)
        private List<Player> onlineList;
        // List of player want to play / queue
        private List<Player> lobbyList;
        // List of room of the Games
        private List<Room> roomList;

        // The port
        public int port = 3002;

        // Server listener
        private TcpListener serverListener;

        // Boolean
        public bool serverIsOnline;

        // Constructor / Start method ----------------------------------------------------------------
        public Server()
        {
            // Initialization
            onlineList = new List<Player>();
            lobbyList = new List<Player>();
            roomList = new List<Room>();

            // Try start the server
            try
            {
                serverListener = new TcpListener(IPAddress.Any, port);
                serverListener.Start();
                serverIsOnline = true;

                Console.WriteLine("------- Server Port " + port + " Created -------\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Server Start Error : " + e.Message);
            }        
        }

        public void StartListening()
        {
            // Start accepting client
            Console.WriteLine(">> Server : Begin Listening");
            Thread beginListenThread = new Thread(BeginAcceptClient);
            beginListenThread.Start();
        }

        // Accepting client thread ------------------------------------------------------------------
        private void BeginAcceptClient()
        {
            while (serverIsOnline)
            {
                // Accept Client
                TcpClient client = serverListener.AcceptTcpClient();

                // Ask for name
                Player player = new Player(client, onlineList, lobbyList, roomList);
                NetworkStream tempStream = player.tcp.GetStream();
                string massage = "Server|WHORU";

                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(tempStream, massage);

                // Waiting for answer
                string answer = formatter.Deserialize(tempStream) as string;
                string[] info = answer.Split("|");

                // Add to list
                player.playerName = info[1];
                onlineList.Add(player);
                player.listPosition = 0;

                // Start player
                player.StartReceiving();

                // Print massage in console
                Console.WriteLine("Server : Client " + player.playerName + " is online");
            }
        }

        // Matchmaking -------------------------------------------------------------------------------
        public static void Matchmaking(Player player, List<Player> lobbyList, List<Room> roomList)
        {
            // If there is some room
            if (roomList.Count > 0)
            {
                // Check each room
                for (int j = 0; j < roomList.Count; j++)
                {
                    // If room is public
                    if (roomList[j].isPublic && roomList[j].canJoin)
                    {
                        player.JoinRoom(roomList[j].roomName);
                        return;
                    }
                    if (roomList[j].isPublic)
                    {
                        Console.WriteLine("Is Public");
                    }
                    if (roomList[j].canJoin)
                    {
                        Console.WriteLine("Can Join");
                    }
                }
            }
            // If there is no room that can join
            // Make a new one
            player.CreateRoom();
        }

        //
        // Disconnect from server --------------------------------------------------------------
        public static void DisconnectFromServer(Player player, List<Player> theList)
        {
            for (int i = 0; i < theList.Count; i++)
            {
                if (theList[i].tcp == player.tcp)
                {
                    Console.WriteLine("Server : Player " + player.playerName + " removed from list");

                    theList.Remove(theList[i]);
                    player.tcp.Close();
                    player = null;

                    return;
                }
            }
        }
        public static void DisconnectFromServer(Player player, Room theRoom, List<Room> roomList)
        {
            for (int i = 0; i < theRoom.playerList.Count; i++)
            {
                if (theRoom.playerList[i].tcp == player.tcp)
                {
                    Console.WriteLine("Server : Player " + player.playerName + " removed from room " + theRoom.roomName);

                    theRoom.playerList.Remove(theRoom.playerList[i]);
                    player.tcp.Close();
                    player = null;

                    break;
                }
            }

            if(theRoom.playerList.Count <= 0)
            {
                DestroyRoom(theRoom, roomList);
            }
        }
        public static void DestroyRoom(Room room, List<Room> roomList)
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                if(roomList[i].roomName == room.roomName)
                {
                    Console.WriteLine("Server : Room " + room.roomName + " destroyed");

                    roomList.Remove(room);
                    room = null;

                    return;
                }
            }
        }
    }
}
