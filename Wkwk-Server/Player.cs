using System;
using System.Net.Sockets;

namespace Wkwk_Server
{
    class Player
    {
        public string playerName;
        public TcpClient tcp;

        public Player(TcpClient tcp)
        {
            this.tcp = tcp;
        }
    }
}
