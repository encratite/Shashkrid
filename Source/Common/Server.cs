using System.Net;
using System.Net.Sockets;

namespace Shashkrid
{
	class Server
	{
		TcpListener Listener;
		OutputManager OutputManager;

		public Server(IPEndPoint endpoint, OutputManager outputManager)
		{
			Listener = new TcpListener(endpoint);
			OutputManager = outputManager;
		}

		public void Run()
		{
			while (true)
			{
				Socket socket = Listener.AcceptSocket();
			}
		}

		public void Message(string message)
		{
			OutputManager.Message(message);
		}
	}
}
