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
            Thread UpdateTask = new Thread(new ThreadStart(Server.instance.Update));
            UpdateTask.Start();
            Thread ThreadTask = new Thread(new ThreadStart(ThreadManager.Update));
            ThreadTask.Start();
            while (true)
            {
                string text = Console.ReadLine().Trim();
                if (text.StartsWith('/'))
                {
                    text = text.Remove(0, 1);
                    string command = text.Split(' ')[0].ToLower();
                    text = text.Remove(0, command.Length).Trim();
                    string reason;
                    int id;
                    switch (command)
                    {
                        case "kick":
                            id = ClientData.FindByUsername(text);
                            if (id != -1)
                            {
                                Console.WriteLine("Reason: ");
                                reason = Console.ReadLine().Trim();
                                Server.KickPlayer(id, reason);
                            }
                            else
                                Console.WriteLine(text + " is currently not on server");
                            break;
                        case "ban":
                            id = ClientData.FindByUsername(text);
                            if (id != -1)
                            {
                                Console.WriteLine("Reason: ");
                                reason = Console.ReadLine().Trim();
                                Server.KickPlayer(id, reason, true);
                            }
                            else
                                Console.WriteLine(text + " is currently not on server");
                            break;
                        case "unban":
                            for (int i = 0; i < Server.data.ban_list.Count; i++)
                                if (Server.data.ban_list[i].username == text)
                                {
                                    Server.data.ban_list.RemoveAt(i);
                                    Server.SaveData();
                                    Console.WriteLine(text + " was unbanned");
                                    break;
                                }
                                else if (i == Server.data.ban_list.Count - 1)
                                    Console.WriteLine(text + " is not banned");
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
