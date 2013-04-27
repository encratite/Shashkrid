using System.Collections.Generic;

namespace Shashkrid
{
	enum PlayerColour
	{
		Black,
		White,
	}

	class Game
	{
		List<Hex> Grid;
		List<Piece> Pieces;
		PlayerColour CurrentTurnPlayer;
		int CurrentTurn;
		int CurrentTurnMoves;

		public Game(List<PiecePlacement> blackDeployment, List<PiecePlacement> whiteDeployment)
		{
			CurrentTurnPlayer = PlayerColour.Black;
			CurrentTurn = 1;
			CurrentTurnMoves = 0;

			CreateGrid();
			DeployPieces(blackDeployment, PlayerColour.Black);
			DeployPieces(whiteDeployment, PlayerColour.White);
		}

		public void NewTurn()
		{
			if (CurrentTurnPlayer == PlayerColour.Black)
				CurrentTurnPlayer = PlayerColour.White;
			else
				CurrentTurnPlayer = PlayerColour.Black;
			CurrentTurn++;
			CurrentTurnMoves = 0;
			foreach (Piece piece in Pieces)
				piece.CanMove = true;
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
			}
			attacker.Hex = destinationHex;
			sourceHex.Piece = null;
			throw new MissingFeatureException("Move");
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

		void DeployPieces(List<PiecePlacement> deployment, PlayerColour colour)
		{
			Pieces = new List<Piece>();
			Dictionary<PieceTypeIdentifier, int> typeCounts = new Dictionary<PieceTypeIdentifier, int>();
			foreach (PiecePlacement placement in deployment)
			{
				if(!IsValidDeploymentPosition(placement.Position, colour))
					throw new GameException("Invalid deployment position");
				Hex hex = GetHex(placement.Position);
				if(hex.Piece != null)
					throw new GameException("Tried to deploy two pieces to the same hex");
				PieceType type = GameConstants.Pieces[placement.Type];
				Piece piece = new Piece(type, colour);
				Pieces.Add(piece);
				hex.Piece = piece;
				typeCounts[placement.Type]++;
			}

			foreach (var pair in GameConstants.Pieces)
			{
				if (typeCounts[pair.Key] != pair.Value.Count)
					throw new GameException("Encountered an invalid number of pieces of a certain type");
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
	}
}
