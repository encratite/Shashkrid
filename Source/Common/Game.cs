using System;
using System.Collections.Generic;
using System.Linq;

namespace Shashkrid
{
	public enum PlayerColour
	{
		Black,
		White,
	}

	public class GameException : Exception
	{
		public GameException(string message, params object[] parameters) :
			base(string.Format(message, parameters))
		{
		}
	}

	public class Game
	{
		List<Hex> _Grid;
		Player Black;
		Player White;
		List<Player> Players;
		Player CurrentTurnPlayer;
		int CurrentTurn;
		int CurrentTurnActions;
		PlayerColour? _Winner;

		public PlayerColour CurrentPlayer { get { return CurrentTurnPlayer.Colour; } }
		public int Turn { get { return CurrentTurn; } }
		public int Actions { get { return CurrentTurnActions; } }
		public PlayerColour? Winner { get { return _Winner; } }

		public Game()
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
			CurrentTurnActions = 0;
			_Winner = null;

			CreateGrid();
			DeployPawns(Black);
			DeployPawns(White);
		}

		public void NewTurn()
		{
			if (object.ReferenceEquals(CurrentTurnPlayer, Black))
				CurrentTurnPlayer = White;
			else
				CurrentTurnPlayer = Black;
			CurrentTurn++;
			CurrentTurnActions = 0;
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
			if (CurrentTurnActions >= GameConstants.ActionsPerTurn)
				throw new GameException("You have already performed the maximum number of actions in this turn");
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
			attacker.CanMove = false;
			CurrentTurnActions++;
		}

		public void PromotePiece(Position position, PieceTypeIdentifier type)
		{
			position.CheckValidity();
			if (CurrentTurnActions >= GameConstants.ActionsPerTurn)
				throw new GameException("You may not perform any more actions this turn");
			Hex hex = GetHex(position);
			Piece piece = hex.Piece;
			if (piece == null)
				throw new GameException("There is no piece to promote, the specified hex is empty");
			if (piece.Type.Identifier != PieceTypeIdentifier.Pawn)
				throw new GameException("Only pawns may be promoted");
			if (!piece.CanMove)
				throw new GameException("This piece has already been moved this turn and can hence not be promoted");
			if(type == PieceTypeIdentifier.Pawn)
				throw new GameException("Invalid promotion identifier");
			bool isClear = true;
			foreach (Hex neighbour in hex.Neighbours)
			{
				Piece neighbourPiece = neighbour.Piece;
				if (neighbourPiece != null && !object.ReferenceEquals(neighbourPiece.Owner, CurrentTurnPlayer))
				{
					isClear = false;
					break;
				}
			}
			if (!isClear)
				throw new GameException("You cannot promote a piece that is in direct proximity of an opponent's piece");
			Piece newPiece = new Piece(GameConstants.Pieces[type], CurrentTurnPlayer);
			hex.Piece = newPiece;
			newPiece.Hex = hex;
			newPiece.CanMove = false;
			piece.Hex = null;
			CurrentTurnActions++;
		}

		public bool NoActionsLeft()
		{
			return CurrentTurnActions >= Math.Min(CurrentTurnPlayer.Pieces.Count, GameConstants.ActionsPerTurn);
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

		public Hex GetHex(Position position)
		{
			position.CheckValidity();
			int index = position.X + position.Y * GameConstants.GridSizeX;
			return _Grid[index];
		}

		bool PlayerWasAnnihilated(Player annihilator, Player victim)
		{
			if (victim.Pieces.Any())
				return false;
			_Winner = annihilator.Colour;
			return true;
		}		

		void CreateGrid()
		{
			_Grid = new List<Hex>();
			for (int y = 0; y < GameConstants.GridSizeY; y++)
			{
				for (int x = 0; x < GameConstants.GridSizeX; x++)
				{
					Position position = new Position(x, y);
					Hex hex = new Hex(position);
					_Grid.Add(hex);
				}
			}
			foreach (Hex hex in _Grid)
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

		void DeployPawn(Position position, Player player)
		{
			Hex hex = GetHex(position);
			Piece piece = new Piece(GameConstants.Pieces[PieceTypeIdentifier.Pawn], player);
			player.Pieces.Add(piece);
			hex.Piece = piece;
		}

		void DeployPawns(Player player)
		{
			int firstRankY;
			int secondRankY;
			int secondRankInitialX;
			int secondRankFinalX;
			if(player.Colour == PlayerColour.Black)
			{
				firstRankY = 2;
				secondRankY = 1;
				secondRankInitialX = 6;
				secondRankFinalX = 12;
			}
			else
			{
				firstRankY = 6;
				secondRankY = 7;
				secondRankInitialX = 0;
				secondRankFinalX = 6;
			}
			for (int x = 0; x < GameConstants.GridSizeX; x++)
			{
				Position position = new Position(x, firstRankY);
				DeployPawn(position, player);
			}
			for (int x = secondRankInitialX; x <= secondRankFinalX; x++)
			{
				Position position = new Position(x, secondRankY);
				DeployPawn(position, player);
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
				piece.Type.FilterMovementMap(piece.Hex.Position, piece.Owner.Colour, map);
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
			int attackSum = attacker.Type.Attack;
			foreach (Hex neighbour in defender.Hex.Neighbours)
			{
				Piece piece = neighbour.Piece;
				if (piece != null && piece != attacker && piece.Owner == attacker.Owner)
					attackSum += piece.Type.Support;
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
					if (!position.IsValid())
						break;
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
