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

		public Game()
		{
			CreateGrid();
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
	}
}
