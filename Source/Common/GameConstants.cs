using System.Collections.Generic;

namespace Shashkrid
{
	public class GameConstants
	{
		public const int GridSizeX = 13;
		public const int GridSizeY = 9;

		public const int ActionsPerTurn = 3;

		public const int TurnLimit = 2 * GridSizeX;

		public static List<Position> TheVoid = new List<Position>()
		{
			new Position(6, 4),
		};

		public static Dictionary<PieceTypeIdentifier, PieceType> Pieces = new Dictionary<PieceTypeIdentifier, PieceType>()
		{
			{PieceTypeIdentifier.Pawn, new Pawn()},
			{PieceTypeIdentifier.Martyr, new Martyr()},
			{PieceTypeIdentifier.Guardian, new Guardian()},
			{PieceTypeIdentifier.Lance, new Lance()},
			{PieceTypeIdentifier.Serpent, new Serpent()},
		};
	}
}
