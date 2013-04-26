namespace Shashkrid
{
	class Piece
	{
		public readonly PieceType Type;
		public readonly PlayerColour Owner;
		public Hex Hex;
		public bool CanMove;

		public Piece(PieceType type, PlayerColour owner)
		{
			Type = type;
			Owner = owner;
			Hex = null;
			CanMove = true;
		}
	}
}
