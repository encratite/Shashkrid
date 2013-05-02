namespace Shashkrid
{
	public enum PieceTypeIdentifier
	{
		Pawn,
		Martyr,
		Guardian,
		Chariot,
		Serpent,
	}

	public class PieceType
	{
		public readonly PieceTypeIdentifier Identifier;
		public readonly string Name;
		public readonly int Count;
		public readonly int Attack;
		public readonly int Defence;
		public readonly int Movement;
		public readonly bool PassThrough;

		public PieceType(PieceTypeIdentifier identifier, string name, int count, int attack, int defence, int movement, bool passThrough)
		{
			Identifier = identifier;
			Name = name;
			Count = count;
			Attack = attack;
			Defence = defence;
			Movement = movement;
			PassThrough = passThrough;
		}
	}
}
