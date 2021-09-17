using System;
using System.Collections.Generic;
using System.Text;

namespace Wkwk_Server
{
    class Room
    {
        // Name of this room
        public string roomName;
        // Room is public or not
        public bool isPublic;
        // Can Join Room or Not
        public bool canJoin;
        // Maximum player in this room
        public int MaxPlayer;
        // List of player in this room
        public List<Player> playerList;

        public void CheckRoom()
        {
            if(playerList.Count == MaxPlayer)
            {
                canJoin = false;
                // Start the game
            }
            else
            {
                canJoin = true;
            }

            if (playerList.Count >= 2)
            {
                // Play button active

            }
        }
    }
}
