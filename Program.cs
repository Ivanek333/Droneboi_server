using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Droneboi_Server
{
    class Program
    {
        static void Main()
        {
            Server.instance = new Server();
            Server.data = new Database();
            Server.LoadData();
            ClientData.clients = new Dictionary<int, ClientData>();
            Thread TcpListenTask = new Thread(new ThreadStart(Server.instance.tcp.Listen));
            TcpListenTask.Start();
            Thread UdpListenTask = new Thread(new ThreadStart(Server.instance.udp.Listen));
            UdpListenTask.Start();
            while (true)
            {
                string text = Console.ReadLine().Trim();
                if (text.StartsWith('/'))
                {
                    text = text.Remove(0, 1);
                    string command = text.Split(' ')[0].ToLower();
                    text = text.Remove(0, command.Length).Trim();
                    string reason;
                    switch (command)
                    {
                        case "kick":
                            Console.Write("Reason: ");
                            reason = Console.ReadLine().Trim();
                            Server.KickPlayer(ClientData.FindByUsername(text), reason);
                            break;
                        case "ban":
                            Console.Write("Reason: ");
                            reason = Console.ReadLine().Trim();
                            Server.KickPlayer(ClientData.FindByUsername(text), reason, true);
                            break;
                        default:
                            Console.WriteLine("Wrong command");
                            break;
                    }
                }
                else
                {
                    ServerSend.SendAllServerMessage(text);
                }
            }
        }
    }
}
