using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Droneboi_Server
{
	public class Server
	{
		public const float tickDelay = 0.1f;
		float deltatime;
		float lasttime;
		public const string path = "";
		public static Database data;
		public static int id_counter;
		public static List<string> versions = new List<string>
		{
			"0.40", "0.41", "0.41.1"
		};
		public static Server instance;
		public TCP tcp;
		public UDP udp;

		public static int dataBufferSize = 4096;

		public string localIP = "127.0.0.1";
		public int localPort = 26950;

		public IPEndPoint localPoint;

		public static Dictionary<int, Server.PacketHandler> packetHandlers;

		public delegate void PacketHandler(int _id, Packet _packet);

		public Server()
		{
			Server.instance = this;
			id_counter = 0;
			localPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);
			InitServerData();
			tcp = new TCP();
			udp = new UDP();
		}
		void SetupServer()
		{

		}
		public class TCP
		{
			public class TCPobject
			{
				public TcpClient client;
				public Packet receivedData;
				public byte[] receiveBuffer;

				public TCPobject()
				{
					receiveBuffer = new byte[Server.dataBufferSize];
				}
			}

			TcpListener listener;



			public void Listen()
			{
				listener = new TcpListener(Server.instance.localPoint);
				listener.Start();
				Debug.Log("TcpListener is running, waiting for connection...");
				listener.BeginAcceptTcpClient(AcceptCallback, listener);
			}

			private void AcceptCallback(IAsyncResult result)
			{
				TcpClient client = listener.EndAcceptTcpClient(result);
				listener.BeginAcceptTcpClient(AcceptCallback, listener);
				Debug.Log(DateTime.Now.ToLongTimeString() + " - " + client.Client.RemoteEndPoint.ToString() + " connected");
				Debug.Log("TcpListener: I think it's a new player");
				int id = id_counter;
				id_counter++;
				ClientData.clients.Add(id, new ClientData
				{
					id = id,
					IPpoint = ((IPEndPoint)client.Client.RemoteEndPoint),
					tcp = new ClientData.TCP(id),
					team = Team.None,
					veh = new Vehicle()
				});
				Debug.Log("TcpListener: New player was added with id " + id.ToString());
				ClientData.clients[id].tcp.Connect(client);
				ServerSend.SendWelcome(id);
				Thread.Sleep(1000);
				Debug.Log("TcpListener: sent welcome to player");
			}
		}

		public class UDP
		{
			public UdpClient socket;

			public void Listen()
			{
				socket = new UdpClient(Server.instance.localPort);
				Debug.Log("UdpListener is running, waiting for connection...");
				socket.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
			}

			public void SendData(Packet packet, IPEndPoint point)
			{
				try
				{
					if (socket != null)
					{
						socket.Connect(point);
						socket.BeginSend(packet.ToArray(), packet.Length(), EndSend, null);
					}
				}
				catch (Exception arg)
				{
					Debug.Log(string.Format("Error sending data to server via UDP: {0}", arg));
				}
			}
			private void EndSend(IAsyncResult result)
            {
				socket.EndSend(result);
            }
			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
					byte[] array = socket.EndReceive(result, ref point);
					socket.BeginReceive(new AsyncCallback(ReceiveCallback), null);
					if (array.Length < 4)
					{
						Debug.Log("UdpListener: i should disconnect");
					}
					else
					{
						HandleData(array);
					}
				}
				catch (Exception e)
				{
					Debug.Log(e.Message);
				}
			}

			private void HandleData(byte[] _data)
			{
				Packet packet = new Packet(_data);
				int id = packet.ReadInt();
				if (packet.UnreadLength() > 4)
				{
					int length = packet.ReadInt();
					_data = packet.ReadBytes(length);
					using Packet packet2 = new Packet(_data);
					int key = packet2.ReadInt();
					Server.packetHandlers[key](id, packet2);
				}
			}
		}



		public void InitServerData()
		{
			packetHandlers = new Dictionary<int, PacketHandler>
			{
				{
					1,
					ServerHandle.ReceiveWelcome
				},
				{
					2,
					ServerHandle.ReceiveSpawnVehicle
				},
				{
					3,
					ServerHandle.ReceiveRemoveVehicle
				},
				{
					4,
					ServerHandle.ReceiveUpdateVehicle
				},
				{
					5,
					ServerHandle.ReceiveFireWeapon
				},
				{
					6,
					ServerHandle.ReceiveMessage
				}
			};
		}

		public void Update()
        {
            while (true)
            {
				float curtime = DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;
				float t = curtime - lasttime;
				lasttime = curtime;
				if (t < 0) t += 60;
				deltatime += t;
				if (deltatime > tickDelay)
                {
					deltatime -= tickDelay;
					Tick();
                }
            }
        }
		public void Tick()
        {
			foreach (var client in ClientData.clients)
				if (client.Value.isConnected && client.Value.isPlaying)
					foreach (var updClient in ClientData.clients)
						if (updClient.Value.isConnected && updClient.Value.isPlaying)
							ServerSend.SendUpdateVehicle(client.Value.id, updClient.Value.id);
        }

		public static void KickPlayer(int id, string reason, bool ban = false)
		{
			ClientData client = ClientData.clients[id];
			if (ban)
			{
				ServerSend.SendAllServerMessage($"{client.username} was banned\nReason: {reason}");
				data.ban_list.Add(new Database.Ban
				{
					username = client.username,
					userId = client.userId,
					reason = reason
				});
				SaveData();
				ServerSend.SendKickPlayer(id, 1, reason);
			}
            else
            {
				ServerSend.SendAllServerMessage($"{client.username} was kicked\nReason: {reason}");
				ServerSend.SendKickPlayer(id, 0, reason);
			}
			client.Disconnect(true);
        }

		public static void LoadData()
        {
			if (!File.Exists(path + "Database.json"))
			{
				File.Create(path + "Database.json").Close();
				SaveData();
			}
			else
				JsonConvert.PopulateObject(File.ReadAllText(path + "Database.json"), Server.data);
		}
		public static void SaveData()
        {
			File.WriteAllText(path + "Database.json", JsonConvert.SerializeObject(Server.data));
		}

		public static int IsBanned(string username, string userId)
        {
			for (int i = 0; i < data.ban_list.Count; i++)
				if (data.ban_list[i].userId == userId || data.ban_list[i].username == username)
					return i;
			return -1;
        }
	}
}
