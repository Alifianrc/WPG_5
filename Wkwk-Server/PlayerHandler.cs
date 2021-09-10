using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wkwk_Server
{
    class PlayerHandler
    {
        Player player;
        // List of online player (not in lobby or room)
        private List<Player> onlineList;
        // List of player want to play / queue
        private List<Player> lobbyList;
        // List of room of the Games
        private List<Room> roomList;
        // List of disconnected player
        private List<Player> disconnectedList;

        // This player room
        private Room myRoom;

        BinaryFormatter formatter;
        NetworkStream stream;

        public PlayerHandler(Player player, List<Player> onlineList, List<Player> lobbyList, List<Room> roomList, List<Player> disconnectedList)
        {
            this.player = player;
            this.onlineList = onlineList;
            this.lobbyList = lobbyList;
            this.roomList = roomList;
            this.disconnectedList = disconnectedList;


            formatter = new BinaryFormatter();
            stream = player.tcp.GetStream();

            Thread recieveThread = new Thread(RecievedMassage);
            recieveThread.Start();
        }

        private void RecievedMassage()
        {
            // ToWho|Data1|Data2|... 

            string data = formatter.Deserialize(stream) as string;
            string[] info = data.Split("|");
            
            if(info[0] == "All")
            {

            }
            else if(info[0] == "AllES")
            {

            }
            else if(info[0] == "Server")
            {
                switch (info[1])
                {
                    case "JoinLobby":
                        JoinLobby();
                        break;
                    case "CreateRoom":
                        if(info.Length > 4)
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
                    default:
                        
                        break;
                }
            }
        }

        // Join Lobby method
        private void JoinLobby()
        {
            foreach(Player a in onlineList)
            {
                if(a.playerName == player.playerName)
                {
                    // Remove from online list
                    onlineList.Remove(a);

                    // Add to Lobby list
                    lobbyList.Add(a);
                }
            }

            // Print massage
            Console.WriteLine(player.playerName + " : Move to Lobby");
        }

        // Create room auto
        private void CreateRoom()
        {
            CreateRoom(player.playerName + "Room", "5", "0");
        }
        // Create custom private room
        private void CreateRoom(string roomName, string playerMax, string canJoin)
        {
            // Generate room
            Room temp = new Room();
            temp.roomName = roomName;
            temp.MaxPlayer = int.Parse(playerMax);
            temp.canJoin = StringToBool(canJoin);

            // Moving player to room
            foreach (Player a in onlineList)
            {
                if (a.playerName == player.playerName)
                {
                    // Remove from online list
                    onlineList.Remove(a);

                    // Add to Lobby list
                    temp.playerList.Add(a);
                }
            }

            // Save room
            myRoom = temp;
        }

        // Join Other Players room manually
        private void JoinRoom(string roomName)
        {
            foreach(Room a in roomList)
            {
                if(a.roomName == roomName && a.canJoin)
                {
                    onlineList.Remove(player);
                    a.playerList.Add(player);

                    myRoom = a;
                }
            }
        }

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
