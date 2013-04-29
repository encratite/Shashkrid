using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using ProtoBuf;

namespace Shashkrid
{
	class ServerClient
	{
		public const int ProtocolVersion = 0;

		const int MaximumStreamLength = 1024 * 1024;
		const int PrefixSize = 4;

		Server Server;

		Socket Socket;
		byte[] Buffer;
		MemoryStream Stream;
		Queue<ClientToServerMessage> Messages;

		bool Sending;

		public ServerClient(Socket socket, Server server)
		{
			Server = server;
			Socket = socket;
			Buffer = new byte[Socket.ReceiveBufferSize];
			Stream = new MemoryStream();
			Messages = new Queue<ClientToServerMessage>();
			Sending = false;
		}

		public void Run()
		{
			lock (this)
			{
				Receive();
			}
		}

		void Message(string message, params object[] arguments)
		{
			string input = string.Format(message, arguments);
			string clientMessage = string.Format("[{0}] {1}", Socket.RemoteEndPoint, input);
			Server.Message(clientMessage);
		}

		void Receive()
		{
			if(Socket.Connected)
				Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
		}

		void Send()
		{
			if (Socket.Connected && !Sending && Messages.Any())
			{
				ClientToServerMessage message = Messages.Dequeue();
				MemoryStream stream = new MemoryStream();
				Serializer.SerializeWithLengthPrefix(stream, message, PrefixStyle.Fixed32BigEndian);
				Socket.BeginSend(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
				Sending = true;
			}
		}

		void SendMessage(ClientToServerMessage message)
		{
			Messages.Enqueue(message);
			Send();
		}

		void OnReceive(IAsyncResult result)
		{
			lock (this)
			{
				try
				{
					int bytesReceived = Socket.EndReceive(result);
					Stream.Write(Buffer, 0, bytesReceived);
					ProcessStream();
					Receive();
				}
				catch (ServerClientException exception)
				{
					HandleException(exception);
				}
			}
		}

		void OnSend(IAsyncResult result)
		{
			lock (this)
			{
				Socket.EndSend(result);
				Sending = false;
				Send();
			}
		}

		void HandleException(Exception exception)
		{
			Message("Error: {0}", exception.Message);
			Socket.Close();
		}

		void ProcessStream()
		{
			if (Stream.Length < PrefixSize)
				return;
			if (Stream.Length > MaximumStreamLength)
			{
				Stream.Close();
				throw new ServerClientException("Maximum stream length exceeded");
			}
			Stream.Seek(0, SeekOrigin.Begin);
			int sizePrefix = 0;
			for (int i = 0; i < PrefixSize; i++)
			{
				sizePrefix <<= 8;
				sizePrefix |= Stream.ReadByte();
			}
			if (sizePrefix > MaximumStreamLength)
			{
				Stream.Close();
				throw new ServerClientException("The size prefix exceeds the maximum stream length");
			}
			int totalSize = PrefixSize + sizePrefix;
			if (Stream.Length < totalSize)
			{
				Stream.Seek(0, SeekOrigin.End);
				return;
			}
			ServerToClientMessage message = Serializer.Deserialize<ServerToClientMessage>(Stream);
			MemoryStream newStream = new MemoryStream();
			newStream.Write(Stream.GetBuffer(), (int)Stream.Position, (int)(Stream.Length - Stream.Position));
			Stream.Close();
			Stream = newStream;
			OnMessage(message);
		}

		void OnMessage(ServerToClientMessage message)
		{
			throw new NotImplementedException("OnMessage");
		}
	}
}
