using System.Collections.Generic;

namespace Shashkrid
{
	class Hex
	{
		public readonly Position Position;
		public Piece Piece;
		public List<Hex> Neighbours;

		public Hex(Position position)
		{
			Position = position;
			Piece = null;
			Neighbours = new List<Hex>();
		}
	}
}
