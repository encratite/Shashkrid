namespace Shashkrid
{
	class PieceType
	{
		public readonly string Name;
		public readonly int Attack;
		public readonly int Defence;
		public readonly int Movement;
		public readonly bool PassThrough;

		public PieceType(string name, int attack, int defence, int movement, bool passThrough)
		{
			Name = name;
			Attack = attack;
			Defence = defence;
			Movement = movement;
			PassThrough = passThrough;
		}
	}
}
