using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

using ProtoBuf;

namespace Shashkrid
{
	class ServerClient : AsynchronousMessenger<ClientToServerMessage, ServerToClientMessage>
	{
		Server Server;

		public ServerClient(Socket socket, Server server)
			: base(socket)
		{
			Server = server;
		}

		override protected void OnMessage(ClientToServerMessage message)
		{
			throw new NotImplementedException("OnMessage");
		}

		override protected void OnException(MessengerException exception)
		{
			Message("Error: {0}", exception.Message);
		}

		void Message(string message, params object[] arguments)
		{
			string input = string.Format(message, arguments);
			string clientMessage = string.Format("[{0}] {1}", Socket.RemoteEndPoint, input);
			Server.Message(clientMessage);
		}
	}
}
