using System.Collections.Generic;

namespace Shashkrid
{
	public enum PieceTypeIdentifier
	{
		Pawn,
		Martyr,
		Guardian,
		Lance,
		Serpent,
	}

	abstract public class PieceType
	{
		public readonly PieceTypeIdentifier Identifier;
		public readonly string Name;

		protected int _Attack;
		protected int _Support;
		protected int _Defence;
		protected int _Movement;
		protected bool _PassThrough;

		public int Attack { get { return _Attack; } }
		public int Support { get { return _Support; } }
		public int Defence { get { return _Defence; } }
		public int Movement { get { return Movement; } }
		public bool PassThrough { get { return _PassThrough; } }

		public PieceType(PieceTypeIdentifier identifier, string name)
		{
			Identifier = identifier;
			Name = name;
			_Attack = 1;
			_Support = 1;
			_Defence = 3;
			_Movement = 2;
			_PassThrough = false;
		}

		virtual public void FilterMovementMap(Position initialPosition, PlayerColour colour, HashSet<Position> map)
		{
		}
	}

	public class Pawn : PieceType
	{
		public Pawn() :
			base(PieceTypeIdentifier.Pawn, "Pawn")
		{
		}
	}

	public class Martyr : PieceType
	{
		public Martyr() :
			base(PieceTypeIdentifier.Martyr, "Martyr")
		{
			_Attack = 2;
		}

		override public void FilterMovementMap(Position initialPosition, PlayerColour colour, HashSet<Position> map)
		{
			int direction = colour == PlayerColour.Black ? 1 : -1;
			foreach (Position position in map)
			{
				int yDirection = direction * (position.Y - initialPosition.Y);
				if (yDirection < 1 && initialPosition.GetDistance(position) > 1)
					map.Remove(position);
			}
		}
	}

	public class Guardian : PieceType
	{
		public Guardian() :
			base(PieceTypeIdentifier.Guardian, "Guardian")
		{
			_Attack = 0;
			_Defence = 4;
		}
	}

	public class Lance : PieceType
	{
		public Lance() :
			base(PieceTypeIdentifier.Lance, "Lance")
		{
			_Defence = 2;
			_Movement = 4;
		}

		override public void FilterMovementMap(Position initialPosition, PlayerColour colour, HashSet<Position> map)
		{
			foreach (Position position in map)
			{
				bool xMovement = position.X != initialPosition.X;
				bool yMovement = position.Y != initialPosition.Y;
				if (xMovement && yMovement)
					map.Remove(position);
			}
		}
	}

	public class Serpent : PieceType
	{
		public Serpent() :
			base(PieceTypeIdentifier.Serpent, "Serpent")
		{
			_Movement = 3;
			_PassThrough = true;
		}

		override public void FilterMovementMap(Position initialPosition, PlayerColour colour, HashSet<Position> map)
		{
			foreach (Position position in map)
			{
				if (initialPosition.GetDistance(position) == 2)
					map.Remove(position);
			}
		}
	}
}
