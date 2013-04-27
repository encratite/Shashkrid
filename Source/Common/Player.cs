using System.Collections.Generic;

namespace Shashkrid
{
	class Player
	{
		public readonly PlayerColour Colour;
		public readonly List<Piece> Pieces;
		public readonly List<PieceType> Captures;

		public Player(PlayerColour colour)
		{
			Colour = colour;
			Pieces = new List<Piece>();
			Captures = new List<PieceType>();
		}
	}
}
