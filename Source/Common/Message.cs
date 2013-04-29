using System.Collections.Generic;

using ProtoBuf;

namespace Shashkrid
{
	enum ClientToServerMessageType
	{
	}

	enum ServerToClientMessageType
	{
	}

	[ProtoContract]
	class ClientToServerMessage
	{
		[ProtoMember(1)]
		public ClientToServerMessageType Type;
	}

	[ProtoContract]
	class ServerToClientMessage
	{
		[ProtoMember(1)]
		public ServerToClientMessageType Type;
	}
}
