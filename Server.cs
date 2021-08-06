﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Droneboi_Server
{
	public class Server
	{
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
				Console.WriteLine("TcpListener is running, waiting for connection...");
				listener.BeginAcceptTcpClient(AcceptCallback, listener);
			}

			private void AcceptCallback(IAsyncResult result)
			{
				TcpClient client = listener.EndAcceptTcpClient(result);
				listener.BeginAcceptTcpClient(AcceptCallback, listener);
				Console.WriteLine(DateTime.Now.ToLongTimeString() + " - " + client.Client.RemoteEndPoint.ToString() + " connected");
				Console.WriteLine("TcpListener: I think it's a new player");
				int id = ClientData.clients.Count;
				ClientData.clients.Add(new ClientData
				{
					id = id,
					IPpoint = ((IPEndPoint)client.Client.RemoteEndPoint),
					tcp = new ClientData.TCP(id)
				});
				Console.WriteLine("TcpListener: New player was added with id " + id.ToString());
				ClientData.clients[id].tcp.Connect(client);
				ServerSend.SendWelcome(id);
				Thread.Sleep(1000);
				Console.WriteLine("TcpListener: sent welcome to player");
			}
		}

		public class UDP
		{
			public UdpClient socket;
			public IPEndPoint localPoint;

			public void Listen()
			{
				localPoint = new IPEndPoint(IPAddress.Any, 0);
				socket = new UdpClient(Server.instance.localPort);
				Console.WriteLine("UdpListener is running, waiting for connection...");
				socket.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
			}

			public void SendData(Packet packet)
			{
				//try
				//{
				if (socket != null)
				{
					socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
				}
				/*catch (Exception arg)
				{
					Console.WriteLine(arg.ToString());
					//Debug.Log(string.Format("Error sending data to server via UDP: {0}", arg));
				}*/
			}
			private void ReceiveCallback(IAsyncResult result)
			{
				//try {

				Console.WriteLine("UdpListener: new connection from");
				IPEndPoint point = new IPEndPoint(IPAddress.Any, 0);
				byte[] array = socket.EndReceive(result, ref point);
				Console.WriteLine("UdpListener: new connection from");
				Console.WriteLine(point.ToString());
				socket.BeginReceive(new AsyncCallback(ReceiveCallback), null);
				if (array.Length < 4)
				{
					Console.WriteLine("UdpListener: i should disconnect");
					//Server.instance.Disconnect(false);
				}
				else
				{
					Console.WriteLine("UdpListener: new message");
					HandleData(array);
				}

				/*}
				catch
				{
					//this.Disconnect();
				}*/
			}

			private void HandleData(byte[] _data)
			{
				Packet packet = new Packet(_data);
				int id = packet.ReadInt();
				if (packet.UnreadLength() > 4)
				{
					int length = packet.ReadInt();
					_data = packet.ReadBytes(length);

					/*ThreadManager.ExecuteOnMainThread(delegate
					{
						using Packet packet2 = new Packet(_data);
						int key = packet2.ReadInt();
						packetHandlers[key](packet2);
					});*/

					using Packet packet2 = new Packet(_data);
					int key = packet2.ReadInt();
					Server.packetHandlers[key](id, packet2);
				}
			}
			private void Disconnect()
			{
				//Server.instance.Disconnect(false);
				//endPoint = null;
				//socket = null;
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
					6,
					ServerHandle.ReceiveMessage
				}
			};
		}
	}
}
