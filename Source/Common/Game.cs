using System;
using System.Collections.Generic;
using System.Linq;

namespace Shashkrid
{
	enum PlayerColour
	{
		Black,
		White,
	}

	class GameException : Exception
	{
		public GameException(string message, params object[] parameters) :
			base(string.Format(message, parameters))
		{
		}
	}

	class Game
	{
		List<Hex> Grid;
		Player Black;
		Player White;
		List<Player> Players;
		Player CurrentTurnPlayer;
		int CurrentTurn;
		int CurrentTurnMoves;
		PlayerColour? _Winner;

		public PlayerColour CurrentPlayer { get { return CurrentTurnPlayer.Colour; } }
		public int Turn { get { return CurrentTurn; } }
		public int Moves { get { return CurrentTurnMoves; } }
		public PlayerColour? Winner { get { return _Winner; } }

		public Game(List<PiecePlacement> blackDeployment, List<PiecePlacement> whiteDeployment)
		{
			Black = new Player(PlayerColour.Black);
			White = new Player(PlayerColour.White);
			Players = new List<Player>()
			{
				Black,
				White,
			};

			CurrentTurnPlayer = Black;
			CurrentTurn = 1;
			CurrentTurnMoves = 0;
			_Winner = null;

			CreateGrid();
			DeployPieces(blackDeployment, Black);
			DeployPieces(whiteDeployment, White);
		}

		public void NewTurn()
		{
			if (object.ReferenceEquals(CurrentTurnPlayer, Black))
				CurrentTurnPlayer = White;
			else
				CurrentTurnPlayer = Black;
			CurrentTurn++;
			CurrentTurnMoves = 0;
			foreach (Player player in Players)
			{
				foreach (Piece piece in player.Pieces)
					piece.CanMove = true;
			}
		}

		public void MovePiece(Position source, Position destination)
		{
			source.CheckValidity();
			destination.CheckValidity();
			if (source.Equals(destination))
				throw new GameException("Tried to move a piece to its current location");
			if (CurrentTurnMoves >= GameConstants.MovesPerTurn)
				throw new GameException("You have already performed the maximum number of moves in this turn");
			Hex sourceHex = GetHex(source);
			Hex destinationHex = GetHex(destination);
			Piece attacker = sourceHex.Piece;
			Piece defender = destinationHex.Piece;
			if (attacker == null)
				throw new GameException("There is no piece on the specified hex");
			if (attacker.Owner != CurrentTurnPlayer)
				throw new GameException("Tried to move a piece of the opponent");
			if (!attacker.CanMove)
				throw new GameException("Tried to move a piece that had already been used in this turn");
			if (!PieceCanReachHex(attacker, destinationHex))
				throw new GameException("The piece is unable to reach the destination");
			if (defender != null)
			{
				if (attacker.Owner == defender.Owner)
					throw new GameException("You cannot capture your own pieces");
				int attackSum = GetAttackSum(attacker, defender);
				if (attackSum < defender.Type.Defence)
					throw new GameException("Your attack is too weak to capture this piece");
				attacker.Owner.Captures.Add(defender.Type);
				defender.Owner.Pieces.Remove(defender);
			}
			attacker.Hex = destinationHex;
			sourceHex.Piece = null;
			destinationHex.Piece = attacker;
		}

		public void DropPiece(Position location, PieceType type)
		{
			location.CheckValidity();
			if (CurrentTurnMoves != 0)
				throw new GameException("You have already made a move this turn");
			bool isValid = false;
			List<Hex> zoneOfControl = GetZoneOfControl(CurrentTurnPlayer);
			foreach (Hex hex in zoneOfControl)
			{
				if (hex.Position.Equals(location))
				{
					isValid = true;
					break;
				}
			}
			if (!isValid)
				throw new GameException("This hex is not in your zone of control");
			if (!CurrentTurnPlayer.Captures.Remove(type))
				throw new GameException("You have not captured a piece of this type");
			Hex dropHex = GetHex(location);
			if (dropHex.Piece != null)
				throw new GameException("This hex is occupied");
			Piece piece = new Piece(type, CurrentTurnPlayer);
			dropHex.Piece = piece;
			CurrentTurnMoves = GameConstants.MovesPerTurn;
		}

		public bool IsAnnihilation()
		{
			return PlayerWasAnnihilated(Black, White) || PlayerWasAnnihilated(White, Black);
		}

		public bool IsDomination()
		{
			List<Hex> blackZone = GetZoneOfControl(Black);
			List<Hex> whiteZone = GetZoneOfControl(White);
			if (blackZone.Count == whiteZone.Count)
			{
				_Winner = null;
				return false;
			}
			else if (blackZone.Count > whiteZone.Count)
				_Winner = PlayerColour.Black;
			else
				_Winner = PlayerColour.White;
			return true;
		}

		bool PlayerWasAnnihilated(Player annihilator, Player victim)
		{
			if (victim.Pieces.Any())
				return false;
			_Winner = annihilator.Colour;
			return true;
		}

		Hex GetHex(Position position)
		{
			position.CheckValidity();
			int index = position.X + position.Y * GameConstants.GridSizeX;
			return Grid[index];
		}

		void CreateGrid()
		{
			Grid = new List<Hex>();
			for (int y = 0; y < GameConstants.GridSizeY; y++)
			{
				for (int x = 0; x < GameConstants.GridSizeX; x++)
				{
					Position position = new Position(x, y);
					Hex hex = new Hex(position);
					Grid.Add(hex);
				}
			}
			foreach (Hex hex in Grid)
			{
				foreach (Position offset in Position.NeighbourOffsets)
				{
					Position position = hex.Position + offset;
					if (position.IsValid())
					{
						Hex neighbour = GetHex(position);
						hex.Neighbours.Add(hex);
					}
				}
			}
		}

		bool IsValidDeploymentPosition(Position position, PlayerColour colour)
		{
			if (colour == PlayerColour.Black)
				return position.Y < GameConstants.DeploymentYLimit;
			else
				return position.Y > GameConstants.DeploymentYLimit;
		}

		void DeployPieces(List<PiecePlacement> deployment, Player player)
		{
			Dictionary<PieceTypeIdentifier, int> typeCounts = new Dictionary<PieceTypeIdentifier, int>();
			foreach (PiecePlacement placement in deployment)
			{
				if(!IsValidDeploymentPosition(placement.Position, player.Colour))
					throw new GameException("Invalid deployment position");
				Hex hex = GetHex(placement.Position);
				if(hex.Piece != null)
					throw new GameException("Tried to deploy two pieces to the same hex");
				PieceType type = GameConstants.Pieces[placement.Type];
				Piece piece = new Piece(type, player);
				player.Pieces.Add(piece);
				hex.Piece = piece;
				typeCounts[placement.Type]++;
			}

			foreach (var pair in GameConstants.Pieces)
			{
				PieceTypeIdentifier pieceType = pair.Key;
				int actualCount = typeCounts[pieceType];
				int expectedCount = pair.Value.Count;
				if (actualCount != expectedCount)
					throw new GameException("Player {0} deployed {1} pieces of type {2}, expected {3}", player.Colour, actualCount, pieceType, expectedCount);
			}
		}

		bool PieceCanReachHex(Piece piece, Hex hex)
		{
			if (piece.Type.PassThrough)
				return piece.Type.Movement >= piece.Hex.Position.GetDistance(hex.Position);
			else
			{
				HashSet<Position> map = new HashSet<Position>();
				GetMovementMap(piece.Hex, piece.Type.Movement, map);
				return map.Contains(hex.Position);
			}
		}

		void GetMovementMap(Hex source, int movementPointsLeft, HashSet<Position> map)
		{
			if (movementPointsLeft <= 0)
				return;
			map.Add(source.Position);
			foreach (Hex neighbour in source.Neighbours)
			{
				if (neighbour.Piece != null || map.Contains(neighbour.Position))
					continue;
				GetMovementMap(neighbour, movementPointsLeft - 1, map);
			}
		}

		int GetAttackSum(Piece attacker, Piece defender)
		{
			int attackSum = 0;
			foreach (Hex neighbour in defender.Hex.Neighbours)
			{
				Piece piece = neighbour.Piece;
				if (piece != null && piece != attacker && piece.Owner == attacker.Owner)
					attackSum += piece.Type.Attack;
			}
			return attackSum;
		}

		List<Hex> GetZoneOfControl(Player player)
		{
			int initial;
			int direction;
			if (player.Colour == PlayerColour.Black)
			{
				initial = 0;
				direction = 1;
			}
			else
			{
				initial = GameConstants.GridSizeY - 1;
				direction = -1;
			}
			List<Hex> zone = new List<Hex>();
			for (int x = 0; x < GameConstants.GridSizeX; x++)
			{
				int? maximum = null;
				for (int y = initial; y >= 0 && y < GameConstants.GridSizeY; y += direction)
				{
					Position position = new Position(x, y);
					Hex hex = GetHex(position);
					Piece piece = hex.Piece;
					if (piece == null)
						continue;
					else if (object.ReferenceEquals(piece.Owner, player))
						maximum = y;
					else
						break;
				}
				if (maximum == null)
					continue;
				for (int y = initial; ; y += direction)
				{
					Position position = new Position(x, y);
					Hex hex = GetHex(position);
					zone.Add(hex);
					if (y == maximum)
						break;
				}
			}
			return zone;
		}
	}
}
