using System;
using System.Collections.Generic;
using System.Text;

namespace Wkwk_Server
{
    class Room
    {
        // Name of this room
        public string roomName;
        // Player can join or not
        public bool canJoin;
        // Maximum player in this room
        public int MaxPlayer;
        // List of player in this room
        public List<Player> playerList;



    }
}
