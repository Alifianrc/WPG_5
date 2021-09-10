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
        // List of all player connected to server
        private List<PlayerHandler> playerHandlerList;
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
            playerHandlerList = new List<PlayerHandler>();

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
            BeginListening();
        }

        // Accepting client, not thread, but looping :v ----------------------------------------------
        private void BeginListening()
        {
            serverListener.BeginAcceptTcpClient(AcceptClient, serverListener);
        }
        private void AcceptClient(IAsyncResult result)
        {
            TcpListener listener = (TcpListener)result.AsyncState;

            // Ask for name
            Player player = new Player(listener.EndAcceptTcpClient(result));
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

            // Handle this new Player
            PlayerHandler temp = new PlayerHandler(player, onlineList, lobbyList, roomList, disconnectedList);
            playerHandlerList.Add(temp);

            // Print massage in console
            Console.WriteLine("Server : Client " + player.playerName + " is online");

            // Start listening agaian
            BeginListening();
        }
    }
}
