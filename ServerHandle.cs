using System;
using System.Collections.Generic;
using System.Net;

namespace Droneboi_Server
{
	public class ServerHandle
	{
		/*
		1 - welcomeReceived
		2 - spawnMyVehicle
		3 - removeMyVehicle
		*4 - updateMyVehicle
		*5 - fireMyVehicleWeapon,
		6 - sendChat
		 */
		public static void ReceiveWelcome(int id, Packet packet) //1
		{
			ClientData client = ClientData.clients[id];

			int checkId = packet.ReadInt();
			string username = packet.ReadString();
			string userId = packet.ReadString();
			bool hideRoles = packet.ReadBool();
			bool premium = true;// packet.ReadBool();

			if (checkId != id)
			{
				Debug.Log($"TcpServer[{id.ToString()}]: Hey, you gave wrong id ({checkId.ToString()}), be careful");
			}
			client.userId = userId;
			client.username = username;
			client.premium = premium;

			int banned = Server.IsBanned(username, userId);
			if (banned != -1)
			{
				ServerSend.SendKickPlayer(id, 1, Server.data.ban_list[banned].reason);
				client.Disconnect(true);
				return;
            }

			ServerSend.SendMessage(id, username + " connected to the server");
		}
		public static void ReceiveSpawnVehicle(int id, Packet packet) //2
		{
			ClientData client = ClientData.clients[id];
			string vehicleKey = packet.ReadString();
			client.vehicleKey = vehicleKey;
			ServerSend.SendAllServerMessage(client.username + " wants to spawn vehicle");
			ServerSend.SendSpawnVehicle(id, vehicleKey);
		}
		public static void ReceiveRemoveVehicle(int id, Packet packet) //2
		{
			ClientData client = ClientData.clients[id];
			ServerSend.SendAllServerMessage(client.username + " wants to remove vehicle");
			ServerSend.SendRemoveVehicle(id);
		}
		public static void ReceiveMessage(int id, Packet packet) //6
		{
			ClientData client = ClientData.clients[id];
			string message = packet.ReadString();
			Debug.Log($"Client[{id}] ({client.username}): {message}");
			if (message.StartsWith("/"))
            {
				message = message.Remove(0, 1);
				string command = message.Split(' ')[0];
				message = message.Remove(0, command.Length).Trim();
				if (command == "team")
                {
					switch(message.ToLower())
					{
						case "green":
							client.team = Team.Green;
							break;
						case "red":
							client.team = Team.Red;
							break;
						case "blue":
							client.team = Team.Blue;
							break;
						case "yellow":
							client.team = Team.Yellow;
							break;
						default:
							ServerSend.SendServerMessage(id, "wrong team");
							break;
					}
                }
                else
                {
					ServerSend.SendServerMessage(id, "wrong command");
                }
            }
			else
				ServerSend.SendMessage(id, client.username + " : " + message);
		}
	}
}