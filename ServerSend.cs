using System;
using System.Collections.Generic;
using System.Text;

namespace Droneboi_Server
{
	class ServerSend
	{
		public const string actionColor = "#C0C0C0";

		public const string developerColor = "#f1c40f";
		public const string adminColor = "#ff4949";
		public const string modColor = "#6aadf3";
		public const string supporterColor = "#dd74d0";

		public const string redColor = "#FF0000";
		public const string greenColor = "#00FF00";
		public const string blueColor = "#0000FF";
		public const string yellowColor = "#FFFF00";

		public const string serverPrefix = "[SERVER] ";
		public const string developerPrefix = "[Developer] ";
		public const string adminPrefix = "[Admin] ";
		public const string modPrefix = "[Mod] ";
		public const string supporterPrefix = "[cool guy] ";

		/*
        1 - ClientHandle.Welcome
		2 - ClientHandle.InitPlayer
	    *3 - ClientHandle.RemovePlayer
	    *4 - ClientHandle.Kick
	    *5 - ClientHandle.SpawnAVehicle
	    *6 - ClientHandle.RemoveAVehicle
	    *7 - ClientHandle.UpdateAVehicle
	    *8 - ClientHandle.FireAVehicleWeapon
	    9 - ClientHandle.ReceiveChat
        */
		public static void SendWelcome(int id) //1
		{
			using (Packet sendPacket = new Packet(1))
			{
				sendPacket.Write(Server.versions.Count);
				foreach (string version in Server.versions)
				{
					sendPacket.Write(version);
				}
				sendPacket.Write(id);
				sendPacket.Write(1);
				ClientData.clients[ClientData.FindById(id)].SendTCP(sendPacket);
			}
		}

		public static void SendInitPlayer(int id) //2
		{
			ClientData newClient = ClientData.clients[ClientData.FindById(id)];
			foreach (ClientData client in ClientData.clients)
			{
				using (Packet packet = new Packet(2))
				{
					packet.Write(client.id);
					packet.Write(client.username);
					packet.Write(false);
					packet.Write(false);
					packet.Write(false);
					packet.Write(client.premium);
					newClient.SendTCP(packet);
				}
			}
			foreach (ClientData client in ClientData.clients)
			{
				using (Packet sendPacket = new Packet(2))
				{
					sendPacket.Write(id);
					sendPacket.Write(newClient.username);
					sendPacket.Write(false);
					sendPacket.Write(false);
					sendPacket.Write(false);
					sendPacket.Write(newClient.premium);
					client.SendTCP(sendPacket);
				}
			}
		}
		public static void SendMessage(int fromId, string rawmessage) //9
		{
			ClientData fromClient = ClientData.clients[ClientData.FindById(fromId)];
			string message = rawmessage;
			if (fromClient.premium)
			{
				message = ColorString(supporterColor, supporterPrefix) + message;
				rawmessage = supporterPrefix + rawmessage;
			}
			message = ColorNicks(message);
			foreach (ClientData client in ClientData.clients)
			{

				using (Packet sendPacket = new Packet(9))
				{
					sendPacket.Write(fromId);
					sendPacket.Write(message);
					sendPacket.Write(rawmessage);
					client.SendTCP(sendPacket);
				}
			}
		}
		public static void SendServerMessage(int fromId, string rawmessage)
		{
			ClientData fromClient = ClientData.clients[ClientData.FindById(fromId)];
			string message = rawmessage;
			message = ColorString(actionColor, serverPrefix) + " : " + message;
			rawmessage = serverPrefix + " : " + rawmessage;
			using (Packet sendPacket = new Packet(9))
			{
				sendPacket.Write(fromId);
				sendPacket.Write(message);
				sendPacket.Write(rawmessage);
				fromClient.SendTCP(sendPacket);
			}
		}
		public static string ColorString(string color, string orig)
        {
			return "<color=" + color +">" + orig + "</color>";
        }
		public static string ReplaceAll(string orig, string replace_from, string replace_to)
		{
			string ret = orig;
			string temp = orig;
			if (replace_from != replace_to && replace_from != "")
				while (temp.Contains(replace_from))
				{
					temp = temp.Replace(replace_from, "");
					ret = ret.Replace(replace_from, replace_to);
				}
			return ret;
		}

		public static string ColorNicks(string orig)
        {
			string ret = orig;
			foreach (ClientData client in ClientData.clients)
			{
				if (client.team != Team.None)
					switch (client.team)
					{
						case Team.Green:
							ret = ReplaceAll(ret, client.username, ColorString(greenColor, client.username));
							break;
						case Team.Red:
							ret = ReplaceAll(ret, client.username, ColorString(redColor, client.username));
							break;
						case Team.Blue:
							ret = ReplaceAll(ret, client.username, ColorString(blueColor, client.username));
							break;
						case Team.Yellow:
							ret = ReplaceAll(ret, client.username, ColorString(yellowColor, client.username));
							break;
					}
			}
			return ret;
        }
	}
}
