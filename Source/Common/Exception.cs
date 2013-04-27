using System;

namespace Shashkrid
{
	class GameException : Exception
	{
		public GameException(string message) :
			base(message)
		{
		}
	}

	class MissingFeatureException : Exception
	{
		public MissingFeatureException(string message) :
			base(string.Format("Missing feature: {0}", message))
		{
		}
	}
}
