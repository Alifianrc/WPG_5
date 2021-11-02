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

        NetworkStream stream;

        // This player room
        private Room myRoom;

        public PlayerHandler(Player player, List<Player> onlineList, List<Player> lobbyList, List<Room> roomList, List<Player> disconnectedList)
        {
            this.player = player;
            this.onlineList = onlineList;
            this.lobbyList = lobbyList;
            this.roomList = roomList;
            this.disconnectedList = disconnectedList;

            stream = player.tcp.GetStream();

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
                                SendMassage(player.playerName, "Check");
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

                        // Print massage
                        if (info[1] != "Check")
                        {
                            Console.WriteLine(player.playerName + " : " + info[1]);
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
            if(target == player.playerName)
            {
                // send format : FromWho|Data1|Data2|...
                string data = "Server" + "|";
                // Add data
                for (int i = 0; i < massage.Length; i++)
                {
                    data += massage[i] + "|";
                }
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, data);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending massage : " + e.Message);
                }
            }
            
            // Send to All Player in room


            // Send to All Player in room excep sender (this)

        }

        // Join Lobby method
        private void JoinLobby()
        {
            foreach(Player a in onlineList)
            {
                if(a.tcp == player.tcp)
                {
                    // Remove from online list
                    onlineList.Remove(a);

                    // Add to Lobby list
                    lobbyList.Add(a);

                    // Send massage to client
                    SendMassage("Server", "JoinedLobby");

                    // Print massage
                    Console.WriteLine(player.playerName + " : Move to Lobby");
                    Console.WriteLine(lobbyList.Count);
                    return;
                }
            }
        }

        // Create room auto
        public void CreateRoom()
        {
            CreateRoom(player.playerName + "Room", "5", "1");
        }
        // Create custom private room
        public void CreateRoom(string roomName, string playerMax, string canJoin)
        {
            // Generate room
            Room temp = new Room();
            temp.roomName = roomName;
            temp.MaxPlayer = int.Parse(playerMax);
            temp.canJoin = StringToBool(canJoin);

            // Moving player to room
            foreach (Player a in onlineList)
            {
                if (a.tcp == player.tcp)
                {
                    // Remove from online list
                    onlineList.Remove(a);

                    // Add to Lobby list
                    temp.playerList.Add(a);

                    // Save room
                    myRoom = temp;

                    // Send massage to client
                    SendMassage("Server", "JoinedRoom");

                    return;
                }
            }
        }

        // Join other players room
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

        // Exit from lobby
        private void ExitLobby()
        {
            foreach (Player a in lobbyList)
            {
                if (a.tcp == player.tcp)
                {
                    // Remove from online list
                    lobbyList.Remove(a);

                    // Add to Lobby list
                    onlineList.Add(a);
                }
            }

            // Print massage
            Console.WriteLine(player.playerName + " : Exit from Lobby");
        }

        // Exit from room
        public void ExitRoom()
        {
            foreach(Player a in myRoom.playerList)
            {
                if (a.tcp == player.tcp)
                {
                    // Remove from player list in room
                    myRoom.playerList.Remove(a);

                    // Add to online list
                    onlineList.Add(a);
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
