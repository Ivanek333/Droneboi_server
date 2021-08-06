using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Droneboi_Server
{
    class Program
    {
        static void Main()
        {
            Server.instance = new Server();
            ClientData.clients = new List<ClientData>();
            Thread TcpListenTask = new Thread(new ThreadStart(Server.instance.tcp.Listen));
            TcpListenTask.Start();
            Thread UdpListenTask = new Thread(new ThreadStart(Server.instance.udp.Listen));
            UdpListenTask.Start();
            Console.ReadLine();
        }
    }
}
