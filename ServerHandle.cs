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
		public static void ReceiveWelcome(int id, Packet packet)
		{
			ClientData client = ClientData.clients[ClientData.FindById(id)];

			int checkId = packet.ReadInt();
			string username = packet.ReadString();
			string userId = packet.ReadString();
			bool hideRoles = packet.ReadBool();
			bool premium = true;// packet.ReadBool();

			if (checkId != id)
			{
				Console.WriteLine($"TcpServer[{id.ToString()}]: Hey, you gave wrong id ({checkId.ToString()}), be careful");
			}
			if (premium)
            {
				username = "[cool guy] " + username;
            }
			client.userId = userId;
			client.username = username;
			client.premium = premium;

			ServerSend.SendMessage(id, username + " connected to the server");
		}

		public static void ReceiveSpawnVehicle(int id, Packet packet)
		{
			ClientData client = ClientData.clients[ClientData.FindById(id)];
			string vehicleKey = packet.ReadString();
			client.vehicleKey = vehicleKey;
			ServerSend.SendMessage(id, client.username + " wants to spawn vehicle");
		}
		public static void ReceiveMessage(int id, Packet packet)
		{
			string message = packet.ReadString();
			ServerSend.SendMessage(id, ClientData.clients[ClientData.FindById(id)].username + " : " + message);
		}
	}
}