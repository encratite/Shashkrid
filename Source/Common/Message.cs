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
	}

	[ProtoContract]
	class ServerToClientMessage
	{
		[ProtoMember(1)]
		public ServerToClientMessageType Type;
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

		[ProtoMember(2)]
		public bool IsFatal;

		public Error(string message, bool isFatal)
		{
			Message = message;
			IsFatal = isFatal;
		}
	}

	[ProtoContract]
	class PlayerDescription
	{
		[ProtoMember(1)]
		public string Name;

		[ProtoMember(2)]
		public List<PiecePlacement> Deployment;

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

		[ProtoMember(2)]
		public PlayerColour? Winner;

		public GameOutcome(GameOutcomeType outcome, PlayerColour winner)
		{
			Outcome = outcome;
			Winner = winner;
		}
	}
}
