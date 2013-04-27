using System.Collections.Generic;

namespace Shashkrid
{
	class PlayerPreferences
	{
		public readonly bool InsistsOnBlack;
		public readonly List<PiecePlacement> BlackPlacements;
		public readonly List<PiecePlacement> WhitePlacements;

		public PlayerPreferences(bool insistsOnBlack, List<PiecePlacement> blackPlacements, List<PiecePlacement> whitePlacements)
		{
			InsistsOnBlack = insistsOnBlack;
			BlackPlacements = blackPlacements;
			WhitePlacements = whitePlacements;
		}
	}
}
