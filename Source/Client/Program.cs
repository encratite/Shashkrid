using System;

using Nil;

namespace Client
{
	class Program
	{
		static void Main(string[] arguments)
		{
			ClientConfiguration configuration;
			try
			{
				Serialiser<ClientConfiguration> serialiser = new Serialiser<ClientConfiguration>("Configuration.xml");
				configuration = serialiser.Load();
			}
			catch (Exception exception)
			{
				Console.WriteLine("Configuration error: {0}", exception.Message);
				return;
			}
			Client client = new Client(configuration);
			client.Run();
		}
	}
}
