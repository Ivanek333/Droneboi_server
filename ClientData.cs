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
		public Vehicle veh;
		public string username;
		public string userId;
		public IPEndPoint IPpoint;
		public bool premium;
		public TCP tcp;
		public Team team;
		public bool isConnected;
		public bool isPlaying;

		public void SendTCP(Packet packet)
		{
			try
			{
				packet.WriteLength();
				tcp.SendData(packet);
			}
			catch (Exception e)
            {
				Debug.Log("Client[" + id.ToString() + "] SendTCP error: " + e.Message);
            }
		}
		public void SendUDP(Packet packet)
		{
			try
			{
				packet.WriteLength();
				Server.instance.udp.SendData(packet, IPpoint);
			}
			catch (Exception e)
			{
				Debug.Log("Client[" + id.ToString() + "] SendUDP error" + e.Message);
			}
		}
		public void Disconnect(bool kick = false)
		{
			isConnected = false;
			Debug.Log("Client[" + id.ToString() + "]: Disconnecting...");
			tcp.stream.Close();
			tcp.socket.Close();
			tcp.stream = null;
			tcp.socket = null;
			tcp = null;
			ClientData.clients.Remove(id);
			if (!kick)
			{
				Debug.Log("Client[" + id.ToString() + "]: Not Kick...");
				ServerSend.SendMessage(this, username + " left the game");
			}
			if (isPlaying)
				ServerSend.SendRemoveVehicle(id);
			ServerSend.SendRemovePlayer(id);
        }

		public class TCP
		{
			public TCP(int _id)
			{
				id = _id; 
			}

			public int id;
			public ClientData client;
			public TcpClient socket;
			public NetworkStream stream;
			private Packet receivedData;
			private byte[] receiveBuffer;

			public void Connect(TcpClient _socket)
			{
				client = ClientData.clients[id];
				client.isConnected = true;
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
						if (client.isConnected)
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
					Debug.Log($"Error receiving TCP data: {arg.Message}");
					if (client.isConnected)
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
