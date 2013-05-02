using System.Collections.Generic;

namespace Shashkrid
{
	public class GameConstants
	{
		public const int GridSizeX = 11;
		public const int GridSizeY = 11;

		public const int DeploymentYLimit = 5;

		public const int MovesPerTurn = 3;

		public const int TurnLimit = 2 * 11;

		public static Dictionary<PieceTypeIdentifier, PieceType> Pieces = new Dictionary<PieceTypeIdentifier, PieceType>()
		{
			{PieceTypeIdentifier.Pawn, new PieceType("Pawn", 11, 1, 3, 3, false)},
			{PieceTypeIdentifier.Martyr, new PieceType("Martyr", 11, 2, 3, 3, false)},
			{PieceTypeIdentifier.Guardian, new PieceType("Guardian", 11, 1, 4, 3, false)},
			{PieceTypeIdentifier.Chariot, new PieceType("Chariot", 11, 1, 3, 4, false)},
			{PieceTypeIdentifier.Serpent, new PieceType("Serpent", 11, 1, 3, 3, true)},
		};
	}
}
