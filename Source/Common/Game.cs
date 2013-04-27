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

		public Game(List<PiecePlacement> blackDeployment, List<PiecePlacement> whiteDeployment)
		{
			CreateGrid();
			DeployPieces(blackDeployment, PlayerColour.Black);
			DeployPieces(whiteDeployment, PlayerColour.White);
		}

		public Hex GetHex(Position position)
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
				hex.Piece = piece;
				typeCounts[placement.Type]++;
			}

			foreach (var pair in GameConstants.Pieces)
			{
				if (typeCounts[pair.Key] != pair.Value.Count)
					throw new GameException("Encountered an invalid number of pieces of a certain type");
			}
		}
	}
}
