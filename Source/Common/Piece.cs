namespace Shashkrid
{
	class Piece
	{
		public readonly PieceType Type;
		public readonly Player Owner;
		public Hex Hex;
		public bool CanMove;

		public Piece(PieceType type, Player owner)
		{
			Type = type;
			Owner = owner;
			Hex = null;
			CanMove = true;
		}
	}
}
