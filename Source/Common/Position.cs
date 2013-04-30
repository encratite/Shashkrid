using System;

using ProtoBuf;

namespace Shashkrid
{
	[ProtoContract]
	class Position
	{
		public static Position[] NeighbourOffsets =
		{
			// Northwest
			new Position(0, -1),
			// North
			new Position(-1, 0),
			// Northeast
			new Position(-1, 1),
			// Southwest
			new Position(1, -1),
			// South
			new Position(1, 0),
			// Southeast
			new Position(0, 1),
		};

		[ProtoMember(1)]
		public int X;

		[ProtoMember(2)]
		public int Y;

		public int Z
		{
			get
			{
				return -X - Y;
			}
		}

		public Position(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int GetDistance(Position position)
		{
			int dx = Math.Abs(position.X - X);
			int dy = Math.Abs(position.Y - Y);
			int dz = Math.Abs(position.Z - Z);
			int distance = Math.Max(Math.Max(dx, dy), dz);
			return distance;
		}

		public bool IsValid()
		{
			return X >= 0 && X < GameConstants.GridSizeX && Y >= 0 && Y < GameConstants.GridSizeY;
		}

		public void CheckValidity()
		{
			if (!IsValid())
				throw new GameException("Invalid position specified");
		}

		public override bool Equals(object input)
		{
			if (input == null || GetType() != input.GetType())
				return false;
			Position position = (Position)input;
			return X == position.X && Y == position.Y;
		}

		public override int GetHashCode()
		{
			return (X << 16) | Y;
		}

		public static Position operator +(Position a, Position b)
		{
			return new Position(a.X + b.X, a.Y + b.Y);
		}

		public static Position operator -(Position a, Position b)
		{
			return new Position(a.X - b.X, a.Y - b.Y);
		}
	}
}
