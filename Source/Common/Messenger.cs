using System;
using System.Collections.Generic;
using System.IO;

using ProtoBuf;

namespace Shashkrid
{
	public class MessengerException : Exception
	{
		public MessengerException(string message, params object[] parameters) :
			base(string.Format(message, parameters))
		{
		}
	}

	public class Messenger
	{
		const int MaximumStreamLength = 1024 * 1024;
		const int PrefixSize = 4;

		public static List<IncomingMessageType> ReadMessages<IncomingMessageType>(ref MemoryStream stream)
		{
			List<IncomingMessageType> messages = new List<IncomingMessageType>();
			while (true)
			{
				IncomingMessageType message = ReadMessage<IncomingMessageType>(ref stream);
				if (message == null)
					break;
				messages.Add(message);
			}
			return messages;
		}

		static IncomingMessageType ReadMessage<IncomingMessageType>(ref MemoryStream stream)
		{
			if (stream.Length < PrefixSize)
				return default(IncomingMessageType);
			if (stream.Length > MaximumStreamLength)
				throw new MessengerException("Maximum stream length exceeded");
			stream.Seek(0, SeekOrigin.Begin);
			int sizePrefix = 0;
			for (int i = 0; i < PrefixSize; i++)
			{
				sizePrefix <<= 8;
				sizePrefix |= stream.ReadByte();
			}
			if (sizePrefix > MaximumStreamLength)
				throw new MessengerException("The size prefix exceeds the maximum stream length");
			int totalSize = PrefixSize + sizePrefix;
			if (stream.Length < totalSize)
			{
				stream.Seek(0, SeekOrigin.End);
				return default(IncomingMessageType);
			}
			try
			{
				IncomingMessageType message = Serializer.Deserialize<IncomingMessageType>(stream);
				MemoryStream newStream = new MemoryStream();
				newStream.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
				stream.Close();
				stream = newStream;
				return message;
			}
			catch (ProtoException exception)
			{
				throw new MessengerException("Deserialisation error: {0}", exception.Message);
			}
		}
	}
}
