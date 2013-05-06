using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Shashkrid
{
	enum ServerClientState
	{
		Connected,
		WaitingForOpponent,
		InGame,
	}

	class ServerClientException : Exception
	{
		public ServerClientException(string message, params object[] parameters) :
			base(string.Format(message, parameters))
		{
		}
	}

	class ServerClient : AsynchronousMessenger<ClientToServerMessage, ServerToClientMessage>
	{
		delegate void MessageHandler(ClientToServerMessage message);

		Server Server;
		Dictionary<ClientToServerMessageType, MessageHandler> MessageHandlers;

		ServerClientState State;
		PlayerPreferences Preferences;
		PlayerColour? Colour;
		Game Game;
		ServerClient Opponent;

		public string PlayerName { get { return Preferences.PlayerName; } }
		string GameName { get { return Preferences.GameName; } }
		bool WantsBlack { get { return Preferences.WantsBlack; } }

		public ServerClient(Socket socket, Server server)
			: base(socket)
		{
			Server = server;
			InitialiseMessageHandlers();
			SetDefaultState();
		}

		public void OnConnect()
		{
			Message("Connected");
		}

		public bool WaitingInGame(string gameName)
		{
			return State == ServerClientState.WaitingForOpponent && GameName == gameName;
		}

		void InitialiseMessageHandlers()
		{
			MessageHandlers = new Dictionary<ClientToServerMessageType, MessageHandler>();
			MessageHandlers[ClientToServerMessageType.PlayGame] = OnPlayGame;
			MessageHandlers[ClientToServerMessageType.MovePiece] = OnMovePiece;
			MessageHandlers[ClientToServerMessageType.PromotePiece] = OnPromotePiece;
		}

		public void SetDefaultState()
		{
			State = ServerClientState.Connected;
			Preferences = null;
			Colour = null;
			Game = null;
			Opponent = null;
		}

		override protected void OnMessage(ClientToServerMessage message)
		{
			lock (Server)
			{
				if (!MessageHandlers.ContainsKey(message.Type))
					throw new Exception("Unable to locate a message handler");
				MessageHandler handler = MessageHandlers[message.Type];
				try
				{
					handler(message);
				}
				catch (ServerClientException exception)
				{
					SendError(exception.Message);
				}
				catch (GameException exception)
				{
					SendError(exception.Message);
				}
			}
		}

		override protected void OnDisconnect()
		{
			if (State == ServerClientState.InGame)
			{
				Message("Deserted game \"{0}\"", GameName);
				GameOutcome outcome = new GameOutcome(GameOutcomeType.Desertion, Opponent.Colour);
				ServerToClientMessage message = ServerToClientMessage.GameEndedMessage(outcome);
				Opponent.SendMessage(message);
				Opponent.SetDefaultState();
			}
			Message("Disconnected");
			SetDefaultState();
			Server.OnDisconnect(this);
		}

		override protected void OnMessengerError(MessengerException exception)
		{
			Message("Error: {0}", exception.Message);
		}

		void Message(string message, params object[] arguments)
		{
			string input = string.Format(message, arguments);
			string clientMessage;
			if(PlayerName != null)
				clientMessage = string.Format("[{0} {1}] {2}", Socket.RemoteEndPoint, PlayerName, input);
			else
				clientMessage = string.Format("[{0}] {1}", Socket.RemoteEndPoint, input);
			Server.Message(clientMessage);
		}

		void SendError(string errorMessage)
		{
			Error error = new Error(errorMessage);
			ServerToClientMessage message = ServerToClientMessage.ErrorMessage(error);
			SendMessage(message);
		}

		void SetColour()
		{
			if (WantsBlack)
			{
				if (Opponent.WantsBlack)
					SetRandomColour();
				else
					Colour = PlayerColour.Black;
			}
			else
			{
				if (Opponent.WantsBlack)
					Colour = PlayerColour.White;
				else
					SetRandomColour();
			}
		}

		void SetRandomColour()
		{
			Random generator = new Random();
			if (generator.Next(0, 1) == 1)
				Colour = PlayerColour.Black;
			else
				Colour = PlayerColour.White;
		}

		void SetPreferences(PlayerPreferences preferences)
		{
			if (preferences == null)
				throw new ServerClientException("Preferences were not specified");
			if (preferences.ProtocolVersion != Protocol.Version)
				throw new ServerClientException("Invalid protocol version, this server uses version {0}", Protocol.Version);
			if (preferences.PlayerName == null)
				throw new ServerClientException("No player name has been specified");
			if (preferences.PlayerName.Length > Server.StringLengthLimit)
				throw new ServerClientException("The player name exceeds the maximum length of {0}", Server.StringLengthLimit);
			if (Server.NameIsInUse(preferences.PlayerName))
				throw new ServerClientException("This name has already been taken");
			if (preferences.GameName == null)
				throw new ServerClientException("No game name has been specified");
			if (preferences.GameName.Length > Server.StringLengthLimit)
				throw new ServerClientException("The game name exceeds the maximum length of {0}", Server.StringLengthLimit);
			Preferences = preferences;
		}

		void InitialiseGame()
		{
			SetColour();
			PlayerDescription blackDescription, whiteDescription;
			Game = new Game();
			if (Colour == PlayerColour.Black)
			{
				Opponent.Colour = PlayerColour.White;
				blackDescription = new PlayerDescription(PlayerName);
				whiteDescription = new PlayerDescription(Opponent.PlayerName);
			}
			else
			{
				Opponent.Colour = PlayerColour.Black;
				blackDescription = new PlayerDescription(Opponent.PlayerName);
				whiteDescription = new PlayerDescription(PlayerName);
			}
			Opponent.Game = Game;
			State = ServerClientState.InGame;
			Opponent.State = ServerClientState.InGame;
			GameStart start = new GameStart(blackDescription, whiteDescription);
			ServerToClientMessage startMessage = ServerToClientMessage.GameStartedMessage(start);
			BroadcastMessage(startMessage);
			NewTurn newTurn = new NewTurn(PlayerColour.Black);
			ServerToClientMessage newTurnMessage = ServerToClientMessage.NewTurnMessage(newTurn);
			BroadcastMessage(newTurnMessage);
		}

		void BroadcastMessage(ServerToClientMessage message)
		{
			SendMessage(message);
			Opponent.SendMessage(message);
		}

		void EndOfTurn()
		{
			GameOutcomeType? outcomeType = null;
			if (Game.IsAnnihilation())
				outcomeType = GameOutcomeType.Annihilation;
			else if (Game.Turn >= GameConstants.TurnLimit)
			{
				if (Game.IsDomination())
					outcomeType = GameOutcomeType.Domination;
				else
					outcomeType = GameOutcomeType.Draw;
			}
			ServerToClientMessage message;
			if (outcomeType == null)
			{
				Game.NewTurn();
				NewTurn newTurn = new NewTurn(Game.CurrentPlayer);
				message = ServerToClientMessage.NewTurnMessage(newTurn);
			}
			else
			{
				GameOutcome outcome = new GameOutcome(outcomeType.Value, Game.Winner);
				message = ServerToClientMessage.GameEndedMessage(outcome);
				SetDefaultState();
				Opponent.SetDefaultState();
			}
			BroadcastMessage(message);
		}

		void OnPlayGame(ClientToServerMessage message)
		{
			if (State != ServerClientState.Connected)
				throw new ServerClientException("You are already in a game");
			SetPreferences(message.Preferences);
			ServerClient opponent = Server.FindOpponent(GameName);
			if (opponent == null)
			{
				State = ServerClientState.WaitingForOpponent;
				return;
			}
			Opponent = opponent;
			InitialiseGame();
		}

		void OnMovePiece(ClientToServerMessage message)
		{
			if (State != ServerClientState.InGame)
				throw new ServerClientException("You tried to move a piece even though you are not in a game");
			if(Game.CurrentPlayer != Colour)
				throw new ServerClientException("You tried to move a piece during your opponent's turn");
			PieceMove move = message.Move;
			if (move == null)
				throw new ServerClientException("No move has been specified");
			if (move.Source == null)
				throw new ServerClientException("Source missing");
			if (move.Destination == null)
				throw new ServerClientException("Destination missing");
			Game.MovePiece(move.Source, move.Destination);
			ServerToClientMessage moveMessage = ServerToClientMessage.PieceMovedMessage(move);
			BroadcastMessage(moveMessage);
			if (Game.NoActionsLeft())
				EndOfTurn();
		}

		void OnPromotePiece(ClientToServerMessage message)
		{
			if (State != ServerClientState.InGame)
				throw new ServerClientException("You tried to promote a piece even though you are not in a game");
			if (Game.CurrentPlayer != Colour)
				throw new ServerClientException("You tried to promote a piece during your opponent's turn");
			PiecePromotion promotion = message.Promotion;
			if (promotion == null)
				throw new ServerClientException("No promotion has been specified");
			if (promotion.Position == null)
				throw new ServerClientException("No location has been specified");
			Game.PromotePiece(promotion.Position, promotion.Type);
			ServerToClientMessage promotionMessage = ServerToClientMessage.PiecePromotedMessage(promotion);
			BroadcastMessage(promotionMessage);
			EndOfTurn();
		}
	}
}
