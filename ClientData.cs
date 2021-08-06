using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Droneboi_Server
{
	public class ClientData
	{
		public static List<ClientData> clients;

		public static int FindById(int id)
		{
			for (int i = 0; i < ClientData.clients.Count; i++)
				if (ClientData.clients[i] != null && ClientData.clients[i].id == id)
					return i;
			return -1;
		}
		public static int FindByIpPoint(IPEndPoint _point)
		{
			for (int i = 0; i < ClientData.clients.Count; i++)
				if (ClientData.clients[i] != null && ClientData.clients[i].IPpoint.Address == _point.Address && ClientData.clients[i].IPpoint.Port == _point.Port)
					return i;
			return -1;
		}

		public int id;
		public string vehicleKey;
		public string username;
		public string userId;
		public IPEndPoint IPpoint;
		public bool premium;
		public TCP tcp;

		public void SendTCP(Packet packet)
		{
			packet.WriteLength();
			tcp.SendData(packet);
		}

		public class TCP
		{
			public TCP(int _id)
			{
				id = _id;
			}

			public int id;
			public TcpClient socket;
			public NetworkStream stream;
			private Packet receivedData;
			private byte[] receiveBuffer;

			public void Connect(TcpClient _socket)
			{
				socket = _socket;
				receiveBuffer = new byte[Server.dataBufferSize];
				if (socket.Connected)
				{
					stream = socket.GetStream();
					receivedData = new Packet();
					Console.WriteLine("TcpClient[" + id.ToString() + "]: Connected succesfully");
					stream.BeginRead(receiveBuffer, 0, Server.dataBufferSize, ReceiveCallback, null);
				}
			}

			public void SendData(Packet packet)
			{
				//try {
				if (socket != null)
				{
					stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
					Console.WriteLine($"TcpClient[{id.ToString()}] ({ClientData.clients[id].username}): Sent message");
					Console.WriteLine(socket.Client.LocalEndPoint.ToString() + " --> " + socket.Client.RemoteEndPoint.ToString());
				}
				/*}
				catch (Exception arg)
				{
					//Debug.Log($"Error sending data to server via TCP: {arg}");
				}*/
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				//try {
				int num = stream.EndRead(result);
				Console.WriteLine("TcpClient[" + id.ToString() + "]: Got new message");
				if (num <= 0)
				{
					Console.WriteLine("TcpClient[" + id.ToString() + "]: I should disconnect");
					//instance.Disconnect();
					return;
				}
				byte[] array = new byte[num];
				Array.Copy(receiveBuffer, array, num);
				Console.WriteLine("TcpClient[" + id.ToString() + "]: " + Encoding.UTF8.GetString(array));
				receivedData.Reset(HandleData(array));
				stream.BeginRead(receiveBuffer, 0, Server.dataBufferSize, ReceiveCallback, null);
				/*}
				catch (Exception arg)
				{
					Console.WriteLine($"Error receiving TCP data: {arg}");
					Disconnect();
				}*/
			}

			private bool HandleData(byte[] data)
			{
				int num = 0;
				receivedData.SetBytes(data);
				if (receivedData.UnreadLength() >= 4)
				{
					num = receivedData.ReadInt();
					if (num <= 0)
					{
						return true;
					}
				}
				while (num > 0 && num <= receivedData.UnreadLength())
				{
					byte[] _packetBytes = receivedData.ReadBytes(num);
					/*ThreadManager.ExecuteOnMainThread(delegate
					{
						using Packet packet = new Packet(_packetBytes);
						int key = packet.ReadInt();
						packetHandlers[key](packet);
					});*/

					using Packet packet = new Packet(_packetBytes);
					int key = packet.ReadInt();
					Server.packetHandlers[key](id, packet);

					num = 0;
					if (receivedData.UnreadLength() >= 4)
					{
						num = receivedData.ReadInt();
						if (num <= 0)
						{
							return true;
						}
					}
				}
				if (num <= 1)
				{
					return true;
				}
				return false;
			}

			private void Disconnect()
			{
				//instance.Disconnect();
				stream = null;
				receivedData = null;
				receiveBuffer = null;
				socket = null;
			}
		}
	}
}
