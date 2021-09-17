using System;
using System.Threading;

namespace Wkwk_Server
{
    class Program
    {
        static void Main()
        {
            // Make the server (server is auto start)
            Server server = new Server();

            // Just for not closing
            while (server.serverIsOnline)
            {

            }
        }
    }
}
