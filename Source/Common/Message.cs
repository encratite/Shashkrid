using System.Collections.Generic;

using ProtoBuf;

namespace Shashkrid
{
	enum ClientToServerMessageType
	{
		PlayGame,
		MovePiece,
		DropPiece,
	}

	enum ServerToClientMessageType
	{
		Error,
		GameStarted,
		NewTurn,
		PieceMoved,
		PieceDropped,
		GameEnded,
	}

	enum GameOutcomeType
	{
		Annihilation,
		Domination,
		Desertion,
		Draw,
	}

	[ProtoContract]
	class ClientToServerMessage
	{
		[ProtoMember(1)]
		public ClientToServerMessageType Type;

		[ProtoMember(2, IsRequired = false)]
		public PlayerPreferences Preferences;

		[ProtoMember(3, IsRequired = false)]
		public PieceMove Move;

		[ProtoMember(4, IsRequired = false)]
		public PieceDrop Drop;

		public ClientToServerMessage()
		{
		}

		ClientToServerMessage(ClientToServerMessageType type)
		{
			Type = type;
		}

		public static ClientToServerMessage PlayGameMessage(PlayerPreferences preferences)
		{
			ClientToServerMessage message = new ClientToServerMessage(ClientToServerMessageType.PlayGame);
			message.Preferences = preferences;
			return message;
		}

		public static ClientToServerMessage MovePieceMessage(PieceMove move)
		{
			ClientToServerMessage message = new ClientToServerMessage(ClientToServerMessageType.MovePiece);
			message.Move = move;
			return message;
		}

		public static ClientToServerMessage DropPieceMessage(PieceDrop drop)
		{
			ClientToServerMessage message = new ClientToServerMessage(ClientToServerMessageType.DropPiece);
			message.Drop = drop;
			return message;
		}

	}

	[ProtoContract]
	class ServerToClientMessage
	{
		[ProtoMember(1)]
		public ServerToClientMessageType Type;

		[ProtoMember(2, IsRequired = false)]
		public Error Error;

		[ProtoMember(3, IsRequired = false)]
		public GameStart GameStart;

		[ProtoMember(4, IsRequired = false)]
		public NewTurn NewTurn;

		[ProtoMember(5, IsRequired = false)]
		public PieceMove Move;

		[ProtoMember(6, IsRequired = false)]
		public PieceDrop Drop;

		[ProtoMember(7, IsRequired = false)]
		public GameOutcome Outcome;

		public ServerToClientMessage()
		{
		}

		ServerToClientMessage(ServerToClientMessageType type)
		{
			Type = type;
		}

		public static ServerToClientMessage ErrorMessage(Error error)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.Error);
			message.Error = error;
			return message;
		}

		public static ServerToClientMessage GameStartedMessage(GameStart gameStart)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.GameStarted);
			message.GameStart = gameStart;
			return message;
		}

		public static ServerToClientMessage NewTurnMessage(NewTurn newTurn)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.NewTurn);
			message.NewTurn = newTurn;
			return message;
		}

		public static ServerToClientMessage PieceMovedMessage(PieceMove move)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.PieceMoved);
			message.Move = move;
			return message;
		}

		public static ServerToClientMessage PieceDroppedMessage(PieceDrop drop)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.PieceDropped);
			message.Drop = drop;
			return message;
		}

		public static ServerToClientMessage GameEndedMessage(GameOutcome outcome)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.GameEnded);
			message.Outcome = outcome;
			return message;
		}
	}

	[ProtoContract]
	class PlayerPreferences
	{
		[ProtoMember(1)]
		public int ProtocolVersion;

		[ProtoMember(2)]
		public string PlayerName;

		[ProtoMember(3)]
		public string GameName;

		[ProtoMember(4)]
		public bool WantsBlack;

		[ProtoMember(5)]
		public List<PiecePlacement> BlackDeployment;

		[ProtoMember(6)]
		public List<PiecePlacement> WhiteDeployment;

		public PlayerPreferences()
		{
		}

		public PlayerPreferences(string playerName, string gameName, bool wantsBlack, List<PiecePlacement> blackPlacements, List<PiecePlacement> whitePlacements)
		{
			ProtocolVersion = Server.ProtocolVersion;
			PlayerName = playerName;
			GameName = gameName;
			WantsBlack = wantsBlack;
			BlackDeployment = blackPlacements;
			WhiteDeployment = whitePlacements;
		}
	}

	[ProtoContract]
	class PieceMove
	{
		[ProtoMember(1)]
		public Position Source;

		[ProtoMember(2)]
		public Position Destination;

		public PieceMove()
		{
		}

		public PieceMove(Position source, Position destination)
		{
			Source = source;
			Destination = destination;
		}
	}

	[ProtoContract]
	class PieceDrop
	{
		[ProtoMember(1)]
		public PieceTypeIdentifier Piece;

		[ProtoMember(2)]
		public Position Destination;

		public PieceDrop()
		{
		}

		public PieceDrop(PieceTypeIdentifier piece, Position destination)
		{
			Piece = piece;
			Destination = destination;
		}
	}

	[ProtoContract]
	class Error
	{
		[ProtoMember(1)]
		public string Message;

		public Error()
		{
		}

		public Error(string message)
		{
			Message = message;
		}
	}

	[ProtoContract]
	class PlayerDescription
	{
		[ProtoMember(1)]
		public string Name;

		[ProtoMember(2)]
		public List<PiecePlacement> Deployment;

		public PlayerDescription()
		{
		}

		public PlayerDescription(string name, List<PiecePlacement> deployment)
		{
			Name = name;
			Deployment = deployment;
		}
	}

	[ProtoContract]
	class GameStart
	{
		[ProtoMember(1)]
		public PlayerDescription Black;

		[ProtoMember(2)]
		public PlayerDescription White;

		public GameStart()
		{
		}

		public GameStart(PlayerDescription black, PlayerDescription white)
		{
			Black = black;
			White = white;
		}
	}

	[ProtoContract]
	class NewTurn
	{
		[ProtoMember(1)]
		public PlayerColour ActivePlayer;

		public NewTurn()
		{
		}

		public NewTurn(PlayerColour activePlayer)
		{
			ActivePlayer = activePlayer;
		}
	}

	[ProtoContract]
	class GameOutcome
	{
		[ProtoMember(1)]
		public GameOutcomeType Outcome;

		[ProtoMember(2, IsRequired = false)]
		public PlayerColour? Winner;

		public GameOutcome()
		{
		}

		public GameOutcome(GameOutcomeType outcome, PlayerColour winner)
		{
			Outcome = outcome;
			Winner = winner;
		}
	}
}
