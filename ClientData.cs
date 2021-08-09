using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Droneboi_Server
{
	public class ClientData
	{
		public static Dictionary<int, ClientData> clients;
		public static int FindByIpPoint(IPEndPoint _point)
		{
			foreach (var client in ClientData.clients)
				if (client.Value != null && client.Value.IPpoint.Address == _point.Address && client.Value.IPpoint.Port == _point.Port)
					return client.Key;
			return -1;
		}
		public static int FindByUsername(string name)
		{
			foreach (var client in ClientData.clients)
				if (client.Value != null && client.Value.username == name)
					return client.Key;
			return -1;
		}

		public int id;
		public string vehicleKey;
		public string username;
		public string userId;
		public IPEndPoint IPpoint;
		public bool premium;
		public TCP tcp;
		public Team team;

		public void SendTCP(Packet packet)
		{
			packet.WriteLength();
			tcp.SendData(packet);
		}
		public void SendUDP(Packet packet)
        {
			packet.WriteLength();
			Server.instance.udp.SendData(packet, IPpoint);
        }
		public void Disconnect(bool kick = false)
		{
			Debug.Log("Client[" + id.ToString() + "]: Disconnecting...");
			tcp.socket.Close();
			tcp.stream.Close();
			tcp.stream = null;
			tcp.socket = null;
			tcp = null;
			if (!kick)
				ServerSend.SendMessage(id, username + " left the game");
			ServerSend.SendRemovePlayer(id);
			ClientData.clients.Remove(id);
        }

		public class TCP
		{
			public TCP(int id)
			{
				client = ClientData.clients[id];
			}

			public ClientData client;
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
					Debug.Log("TcpClient[" + client.id.ToString() + "]: Connected succesfully");
					stream.BeginRead(receiveBuffer, 0, Server.dataBufferSize, ReceiveCallback, null);
				}
			}

			public void SendData(Packet packet)
			{
				try
				{
					if (socket != null)
					{
						stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
						Debug.Log($"TcpClient[{client.id.ToString()}] ({client.username}): Sent message");
					}
				}
				catch (Exception arg)
				{
					Debug.Log($"Error sending data to server via TCP: {arg}");
				}
			}

			private void ReceiveCallback(IAsyncResult result)
			{
				try
				{
					int num = stream.EndRead(result);
					Debug.Log("TcpClient[" + client.id.ToString() + "]: Got new message");
					if (num <= 0)
					{
						Debug.Log("TcpClient[" + client.id.ToString() + "]: I should disconnect");
						client.Disconnect();
						return;
					}
					byte[] array = new byte[num];
					Array.Copy(receiveBuffer, array, num);
					Debug.Log("TcpClient[" + client.id.ToString() + "]: " + Encoding.UTF8.GetString(array));
					receivedData.Reset(HandleData(array));
					stream.BeginRead(receiveBuffer, 0, Server.dataBufferSize, ReceiveCallback, null);
				}
				catch (Exception arg)
				{
					Debug.Log($"Error receiving TCP data: {arg}");
					client.Disconnect();
				}
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
					using Packet packet = new Packet(_packetBytes);
					int key = packet.ReadInt();
					Server.packetHandlers[key](client.id, packet);

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
		}
	}
}
