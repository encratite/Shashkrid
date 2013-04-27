namespace Shashkrid
{
	class PiecePlacement
	{
		public readonly PieceTypeIdentifier Type;
		public readonly Position Position;

		public PiecePlacement(PieceTypeIdentifier type, Position position)
		{
			Type = type;
			Position = position;
		}
	}
}
