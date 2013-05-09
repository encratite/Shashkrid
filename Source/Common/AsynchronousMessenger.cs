using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

using ProtoBuf;

namespace Shashkrid
{
	public abstract class AsynchronousMessenger<IncomingMessageType, OutgoingMessageType>
	{
		protected Socket Socket;

		byte[] Buffer;
		MemoryStream Stream;
		Queue<OutgoingMessageType> Messages;

		bool Sending;

		public AsynchronousMessenger(Socket socket)
		{
			Socket = socket;
			Buffer = new byte[Socket.ReceiveBufferSize];
			Stream = new MemoryStream();
			Messages = new Queue<OutgoingMessageType>();
			Sending = false;
		}

		public void Run()
		{
			lock (this)
				Receive();
		}

		public void Shutdown()
		{
			lock (this)
				Close();
		}

		protected void Close()
		{
			Stream.Close();
			Socket.Close();
		}

		protected void SendMessage(OutgoingMessageType message)
		{
			Messages.Enqueue(message);
			Send();
		}

		abstract protected void OnMessage(IncomingMessageType message);

		abstract protected void OnDisconnect();

		abstract protected void OnMessengerError(MessengerException exception);

		void Receive()
		{
			if (Socket.Connected)
				Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
			else
				OnDisconnect();
		}

		void Send()
		{
			if (Socket.Connected && !Sending && Messages.Any())
			{
				OutgoingMessageType message = Messages.Dequeue();
				MemoryStream stream = new MemoryStream();
				Serializer.SerializeWithLengthPrefix(stream, message, PrefixStyle.Fixed32BigEndian);
				Socket.BeginSend(stream.GetBuffer(), 0, (int)stream.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
				Sending = true;
			}
		}

		void OnReceive(IAsyncResult result)
		{
			lock (this)
			{
				try
				{
					int bytesReceived = Socket.EndReceive(result);
					Stream.Write(Buffer, 0, bytesReceived);
					List<IncomingMessageType> messages = Messenger.ReadMessages<IncomingMessageType>(ref Stream);
					foreach (IncomingMessageType message in messages)
						OnMessage(message);
					Receive();
				}
				catch (MessengerException exception)
				{
					Close();
					OnMessengerError(exception);
					OnDisconnect();
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
	}
}

