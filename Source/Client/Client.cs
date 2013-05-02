using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Shashkrid
{
	class ClientException : Exception
	{
		public ClientException(string message, params object[] parameters) :
			base(string.Format(message, parameters))
		{
		}
	}

	abstract class Client : AsynchronousMessenger<ServerToClientMessage, ClientToServerMessage>
	{
		delegate void MessageHandler(ServerToClientMessage message);

		Dictionary<ServerToClientMessageType, MessageHandler> MessageHandlers;

		PlayerPreferences Preferences;
		PlayerColour? Colour;
		Game Game;
		string Opponent;

		public Client(Socket socket, PlayerPreferences preferences) :
			base(socket)
		{
			InitialiseMessageHandlers();

			Preferences = preferences;
			Colour = null;
			Game = null;
			Opponent = null;

			Play();
		}

		abstract protected void OnMyTurn();
		abstract protected void OnGameException(GameException exception);
		abstract protected void OnServerError(string message);

		override protected void OnMessage(ServerToClientMessage message)
		{
			if (!MessageHandlers.ContainsKey(message.Type))
				throw new Exception("Unable to locate a message handler");
			MessageHandler handler = MessageHandlers[message.Type];
			try
			{
				handler(message);
			}
			catch (GameException exception)
			{
				OnGameException(exception);
				Shutdown();
			}
		}

		void InitialiseMessageHandlers()
		{
			MessageHandlers = new Dictionary<ServerToClientMessageType, MessageHandler>();
			MessageHandlers[ServerToClientMessageType.Error] = OnError;
			MessageHandlers[ServerToClientMessageType.GameStarted] = OnGameStarted;
			MessageHandlers[ServerToClientMessageType.NewTurn] = OnNewTurn;
			MessageHandlers[ServerToClientMessageType.PieceMoved] = OnPieceMoved;
			MessageHandlers[ServerToClientMessageType.PieceDropped] = OnPieceDropped;
			MessageHandlers[ServerToClientMessageType.GameEnded] = OnGameEnded;
		}

		void Play()
		{
			ClientToServerMessage message = ClientToServerMessage.PlayGameMessage(Preferences);
			SendMessage(message);
		}

		void OnError(ServerToClientMessage message)
		{
			Error error = message.Error;
			OnServerError(error.Message);
		}

		void OnGameStarted(ServerToClientMessage message)
		{
			GameStart start = message.GameStart;
			Game = new Game(start.Black.Deployment, start.White.Deployment);
			if (start.Black.Name == Preferences.PlayerName)
			{
				Colour = PlayerColour.Black;
				Opponent = start.White.Name;
			}
			else
			{
				Colour = PlayerColour.White;
				Opponent = start.Black.Name;
			}

			if (Colour == PlayerColour.Black)
				OnMyTurn();
		}

		void OnNewTurn(ServerToClientMessage message)
		{
			Game.NewTurn();
			NewTurn newTurn = message.NewTurn;
			if (Colour == newTurn.ActivePlayer)
				OnMyTurn();
		}

		void OnPieceMoved(ServerToClientMessage message)
		{
			PieceMove move = message.Move;
			Game.MovePiece(move.Source, move.Destination);
		}

		void OnPieceDropped(ServerToClientMessage message)
		{
			PieceDrop drop = message.Drop;
			Game.DropPiece(drop.Destination, drop.Piece);
		}

		void OnGameEnded(ServerToClientMessage message)
		{
			GameOutcome outcome = message.Outcome;
		}
	}
}
