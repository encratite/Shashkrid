using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

using ProtoBuf;

namespace Shashkrid
{
	class MessengerException : Exception
	{
		public MessengerException(string message, params object[] parameters) :
			base(string.Format(message, parameters))
		{
		}
	}

	abstract class AsynchronousMessenger<IncomingMessageType, OutgoingMessageType>
	{
		const int MaximumStreamLength = 1024 * 1024;
		const int PrefixSize = 4;

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
					ProcessStreamContent();
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

		void ProcessStreamContent()
		{
			if (Stream.Length < PrefixSize)
				return;
			if (Stream.Length > MaximumStreamLength)
				throw new MessengerException("Maximum stream length exceeded");
			Stream.Seek(0, SeekOrigin.Begin);
			int sizePrefix = 0;
			for (int i = 0; i < PrefixSize; i++)
			{
				sizePrefix <<= 8;
				sizePrefix |= Stream.ReadByte();
			}
			if (sizePrefix > MaximumStreamLength)
				throw new MessengerException("The size prefix exceeds the maximum stream length");
			int totalSize = PrefixSize + sizePrefix;
			if (Stream.Length < totalSize)
			{
				Stream.Seek(0, SeekOrigin.End);
				return;
			}
			try
			{
				IncomingMessageType message = Serializer.Deserialize<IncomingMessageType>(Stream);
				MemoryStream newStream = new MemoryStream();
				newStream.Write(Stream.GetBuffer(), (int)Stream.Position, (int)(Stream.Length - Stream.Position));
				Stream.Close();
				Stream = newStream;
				OnMessage(message);
			}
			catch (ProtoException exception)
			{
				throw new MessengerException("Deserialisation error: {0}", exception.Message);
			}
		}
	}
}

