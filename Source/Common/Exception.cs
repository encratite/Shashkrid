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
}
