using System.Collections.Generic;

using ProtoBuf;

namespace Shashkrid
{
	public enum ClientToServerMessageType
	{
		PlayGame,
		MovePiece,
		PromotePiece,
	}

	public enum ServerToClientMessageType
	{
		Error,
		GameStarted,
		NewTurn,
		PieceMoved,
		PiecePromoted,
		GameEnded,
	}

	public enum GameOutcomeType
	{
		Annihilation,
		Domination,
		Desertion,
		Draw,
	}

	public abstract class Protocol
	{
		public const int Version = 1;
	}

	[ProtoContract]
	public class ClientToServerMessage
	{
		[ProtoMember(1)]
		public ClientToServerMessageType Type;

		[ProtoMember(2, IsRequired = false)]
		public PlayerPreferences Preferences;

		[ProtoMember(3, IsRequired = false)]
		public PieceMove Move;

		[ProtoMember(4, IsRequired = false)]
		public PiecePromotion Promotion;

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

		public static ClientToServerMessage PromotePieceMessage(PiecePromotion promotion)
		{
			ClientToServerMessage message = new ClientToServerMessage(ClientToServerMessageType.PromotePiece);
			message.Promotion = promotion;
			return message;
		}

	}

	[ProtoContract]
	public class ServerToClientMessage
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
		public PiecePromotion Promotion;

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

		public static ServerToClientMessage PiecePromotedMessage(PiecePromotion promotion)
		{
			ServerToClientMessage message = new ServerToClientMessage(ServerToClientMessageType.PiecePromoted);
			message.Promotion = promotion;
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
	public class PlayerPreferences
	{
		[ProtoMember(1)]
		public int ProtocolVersion;

		[ProtoMember(2)]
		public string PlayerName;

		[ProtoMember(3)]
		public string GameName;

		[ProtoMember(4)]
		public bool WantsBlack;

		public PlayerPreferences()
		{
		}

		public PlayerPreferences(string playerName, string gameName, bool wantsBlack)
		{
			ProtocolVersion = Protocol.Version;
			PlayerName = playerName;
			GameName = gameName;
			WantsBlack = wantsBlack;
		}
	}

	[ProtoContract]
	public class PieceMove
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
	public class PiecePromotion
	{
		[ProtoMember(1)]
		public Position Position;

		[ProtoMember(2)]
		public PieceTypeIdentifier Type;

		public PiecePromotion()
		{
		}

		public PiecePromotion(Position location, PieceTypeIdentifier promotion)
		{
			Position = location;
			Type = promotion;
		}
	}

	[ProtoContract]
	public class Error
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
	public class PlayerDescription
	{
		[ProtoMember(1)]
		public string Name;

		public PlayerDescription()
		{
		}

		public PlayerDescription(string name)
		{
			Name = name;
		}
	}

	[ProtoContract]
	public class GameStart
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
	public class NewTurn
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
	public class GameOutcome
	{
		[ProtoMember(1)]
		public GameOutcomeType Outcome;

		[ProtoMember(2, IsRequired = false)]
		public PlayerColour? Winner;

		public GameOutcome()
		{
		}

		public GameOutcome(GameOutcomeType outcome, PlayerColour? winner)
		{
			Outcome = outcome;
			Winner = winner;
		}
	}
}
