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
		4 - updateMyVehicle
		5 - fireMyVehicleWeapon,
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
			ServerSend.SendMessage(client, username + " connected to the server");
			ServerSend.SendInitPlayer(id);

		}
		public static void ReceiveSpawnVehicle(int id, Packet packet) //2
		{
			ClientData client = ClientData.clients[id];
			string vehicleKey = packet.ReadString();
			client.veh.veh_key = vehicleKey;
			client.isPlaying = true;
			ServerSend.SendSpawnVehicle(id, vehicleKey);
		}
		public static void ReceiveRemoveVehicle(int id, Packet packet) //3
		{
			ClientData client = ClientData.clients[id];
			client.isPlaying = false;
			ServerSend.SendRemoveVehicle(id);
		}
		public static void ReceiveUpdateVehicle(int id, Packet packet) //4
		{
			ClientData client = ClientData.clients[id];
			int childCount = packet.ReadInt();
			lock (client.veh)
			{
				client.veh.parts = new List<Part>();
				for (int i = 0; i < childCount; i++)
				{
					client.veh.parts.Add(new Part());
					client.veh.parts[i].position = packet.ReadVector2();
					client.veh.parts[i].eulerAnglesZ = packet.ReadFloat();
					client.veh.parts[i].velocity = packet.ReadVector2();
					client.veh.parts[i].angularVelocity = packet.ReadFloat();
				}
				int count1 = packet.ReadInt();
				client.veh.thrusters = new List<Block>();
				for (int i = 0; i < count1; i++)
				{
					client.veh.thrusters.Add(new Block());
					client.veh.thrusters[i].isPlaying = packet.ReadBool();
				}
				int count2 = packet.ReadInt();
				client.veh.momentumWheels = new List<Block>();
				for (int i = 0; i < count2; i++)
				{
					client.veh.momentumWheels.Add(new Block());
					client.veh.momentumWheels[i].isPlaying = packet.ReadBool();
				}
				int count3 = packet.ReadInt();
				client.veh.connectors = new List<Block>();
				for (int i = 0; i < count3; i++)
				{
					client.veh.connectors.Add(new Block());
					client.veh.connectors[i].powered = packet.ReadBool();
				}
				int count4 = packet.ReadInt();
				client.veh.solarPanels = new List<Block>();
				for (int i = 0; i < count4; i++)
				{
					client.veh.solarPanels.Add(new Block());
					client.veh.solarPanels[i].charging = packet.ReadBool();
				}
				int count5 = packet.ReadInt();
				client.veh.miningLasers = new List<Block>();
				for (int i = 0; i < count5; i++)
				{
					client.veh.miningLasers.Add(new Block());
					client.veh.miningLasers[i].drilling = packet.ReadBool();
				}
			}
		}
		public static void ReceiveFireWeapon(int id, Packet packet) //5
		{
			int ind = packet.ReadInt();
			ServerSend.SendFireWeapon(id, ind);
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
				ServerSend.SendMessage(client, client.username + " : " + message);
		}
	}
}