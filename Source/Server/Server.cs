using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Shashkrid
{
	class Server
	{
		public const int StringLengthLimit = 100;

		TcpListener Listener;
		OutputManager OutputManager;

		List<ServerClient> Clients;

		public Server(IPEndPoint endpoint, OutputManager outputManager)
		{
			Listener = new TcpListener(endpoint);
			OutputManager = outputManager;
			Clients = new List<ServerClient>();
		}

		public void Run()
		{
			while (true)
			{
				Socket socket = Listener.AcceptSocket();
				lock (this)
				{
					ServerClient client = new ServerClient(socket, this);
					client.OnConnect();
					client.Run();
					Clients.Add(client);
				}
			}
		}

		public void Message(string message)
		{
			OutputManager.Message(message);
		}

		public bool NameIsInUse(string playerName)
		{
			foreach (ServerClient client in Clients)
			{
				if (client.PlayerName == playerName)
					return true;
			}
			return false;
		}

		public ServerClient FindOpponent(string gameName)
		{
			foreach (ServerClient client in Clients)
			{
				if (client.WaitingInGame(gameName))
					return client;
			}
			return null;
		}

		public void OnDisconnect(ServerClient client)
		{
			Clients.Remove(client);
		}
	}
}
