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
        public int port = 1313;

        // Server listener
        private TcpListener serverListener;

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

                Console.WriteLine("------- Server Started -------\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Server Start Error : " + e.Message);
            }

            // Start accepting client
            Thread beginListenThread = new Thread(BeginListening);
            beginListenThread.Start();
        }

        // Accepting client thread ------------------------------------------------------------------
        private void BeginListening()
        {
            while (true)
            {
                serverListener.BeginAcceptTcpClient(AcceptClient, serverListener);
            }
        }
        private void AcceptClient(IAsyncResult result)
        {
            TcpListener listener = (TcpListener)result.AsyncState;

            // Ask for name
            Player player = new Player(listener.EndAcceptTcpClient(result), onlineList, lobbyList, roomList);
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

        // Matchmaking -------------------------------------------------------------------------------
        public static void Matchmaking(Player player, List<Player> lobbyList, List<Room> roomList)
        {
            // Check list
            if(lobbyList.Count > 0)
            {
                bool joinedRoom = false;
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
                            joinedRoom = true;
                            return;
                        }
                    }
                }
                // If there is room check each room
                else if (roomList.Count <= 0 && joinedRoom == false)
                {
                    // Make a new one
                    player.CreateRoom();
                }
            }
        }

        //
        // Disconnect from server
        public static void DisconnectFromServer(Player player, List<Player> theList)
        {
            for (int i = 0; i < theList.Count; i++)
            {
                if (theList[i].tcp == player.tcp)
                {
                    theList.Remove(theList[i]);
                    player.tcp.Close();
                    player = null;
                }
            }
        }
        public static void DisconnectFromServer(Player player, Room theRoom)
        {
            for (int i = 0; i < theRoom.playerList.Count; i++)
            {
                if (theRoom.playerList[i].tcp == player.tcp)
                {
                    theRoom.playerList.Remove(theRoom.playerList[i]);
                    player.tcp.Close();
                    player = null;
                }
            }
        }
    }
}
