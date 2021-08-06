using System;
using System.Collections.Generic;
using System.Text;

namespace Droneboi_Server
{
    class ServerSend
	{
		/*
        1 - ClientHandle.Welcome
		2 - ClientHandle.InitPlayer
	    3 - ClientHandle.RemovePlayer
	    4 - ClientHandle.Kick
	    5 - ClientHandle.SpawnAVehicle
	    6 - ClientHandle.RemoveAVehicle
	    7 - ClientHandle.UpdateAVehicle
	    8 - ClientHandle.FireAVehicleWeapon
	    9 - ClientHandle.ReceiveChat
        */
		public static void SendWelcome(int id)
		{
			Packet sendPacket = new Packet(1);
			sendPacket.Write(Server.versions.Count);
			foreach (string version in Server.versions)
			{
				sendPacket.Write(version);
			}
			sendPacket.Write(id);
			sendPacket.Write(1);
			ClientData.clients[ClientData.FindById(id)].SendTCP(sendPacket);
		}

		public static void SendInitPlayer(int id)
		{
			ClientData newClient = ClientData.clients[ClientData.FindById(id)];
			foreach (ClientData client in ClientData.clients)
			{
				Packet packet = new Packet(2);
				packet.Write(client.id);
				packet.Write(client.username);
				packet.Write(false);
				packet.Write(false);
				packet.Write(false);
				packet.Write(client.premium);
				newClient.SendTCP(packet);
			}
			Packet sendPacket = new Packet(2);
			sendPacket.Write(id);
			sendPacket.Write(newClient.username);
			sendPacket.Write(false);
			sendPacket.Write(false);
			sendPacket.Write(false);
			sendPacket.Write(newClient.premium);
			foreach (ClientData client in ClientData.clients)
			{
				client.SendTCP(sendPacket);
			}
		}
		public static void SendMessage(int fromId, string message)
        {
            Packet sendPacket = new Packet(9);
			sendPacket.Write(fromId);
			sendPacket.Write(message);
			sendPacket.Write(message);
            foreach (ClientData client in ClientData.clients)
            {
                client.SendTCP(sendPacket);
            }
        }
    }
}
