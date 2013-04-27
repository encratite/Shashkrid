namespace Shashkrid
{
	enum PieceTypeIdentifier
	{
		Pawn,
		Martyr,
		Guardian,
		Chariot,
		Serpent,
	}

	class PieceType
	{
		public readonly string Name;
		public readonly int Count;
		public readonly int Attack;
		public readonly int Defence;
		public readonly int Movement;
		public readonly bool PassThrough;

		public PieceType(string name, int count, int attack, int defence, int movement, bool passThrough)
		{
			Name = name;
			Count = count;
			Attack = attack;
			Defence = defence;
			Movement = movement;
			PassThrough = passThrough;
		}
	}
}
