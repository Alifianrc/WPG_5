using System;
using System.Collections;
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
        // List of disconnected player
        private List<Player> disconnectedList;

        // The port
        public int port = 1313;

        // Server listener
        private TcpListener serverListener;
        // Server condition
        private bool serverIsStarted;

        // Constructor / Start method ----------------------------------------------------------------
        public Server()
        {
            onlineList = new List<Player>();
            lobbyList = new List<Player>();
            roomList = new List<Room>();
            disconnectedList = new List<Player>();

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

            serverIsStarted = true;

            // Start accepting client
            Thread beginListenThread = new Thread(BeginListening);
            beginListenThread.Start();

            // Matchmaking thread
            Thread matchmakingThread = new Thread(Matchmaking);
            matchmakingThread.Start();
        }

        // Accepting client, not thread, but looping :v ----------------------------------------------
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
            Player player = new Player(listener.EndAcceptTcpClient(result), onlineList, lobbyList, roomList, disconnectedList);
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
        public void Matchmaking()
        {
            while (true)
            {
                if(lobbyList.Count > 0)
                {
                    bool joinedRoom = false;
                    for (int i = 0; i < lobbyList.Count; i++)
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
                                    lobbyList[i].JoinRoom(roomList[j].roomName);
                                    joinedRoom = true;
                                    return;
                                }
                            }
                        }
                        // If there is room check each room
                        else if (roomList.Count <= 0 && joinedRoom == false)
                        {
                            // Make a new one
                            lobbyList[i].CreateRoom();
                        }
                    }
                }
            }
        }
    }
}
