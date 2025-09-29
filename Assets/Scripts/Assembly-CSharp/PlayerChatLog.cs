using System;
using System.Collections.Generic;
using System.Text;

public static class PlayerChatLog
{
	public enum ChatContextType
	{
		Lobby = 0,
		Game = 1,
		Private = 2,
		Clan = 3
	}

	private class ChatContext
	{
		public int MaxMessagesCount = 100;

		public string Title = string.Empty;

		public ChatContextType Type;

		public Queue<string> Messages;

		public ChatContext(string title, ChatContextType type, int messageCap)
		{
			Title = title;
			Type = type;
			Messages = new Queue<string>(messageCap);
			MaxMessagesCount = messageCap;
		}

		public void Add(string message)
		{
			Messages.Enqueue(message);
			while (Messages.Count > MaxMessagesCount)
			{
				Messages.Dequeue();
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Title);
			foreach (string message in Messages)
			{
				stringBuilder.AppendLine(message);
			}
			return stringBuilder.ToString();
		}
	}

	public struct ChatParticipant
	{
		public int Cmid;

		public string Name;

		public ChatParticipant(int cmid, string name)
		{
			Cmid = cmid;
			Name = name;
		}
	}

	private static Dictionary<int, ChatParticipant> _participants = new Dictionary<int, ChatParticipant>();

	private static Dictionary<int, ChatContext> _privateContexts = new Dictionary<int, ChatContext>();

	private static ChatContext _ingameContext = new ChatContext("InGame", ChatContextType.Game, 100);

	private static ChatContext _lobbyContext = new ChatContext("Lobby", ChatContextType.Lobby, 100);

	public static int ParticipantsCount
	{
		get
		{
			return _participants.Count;
		}
	}

	public static ICollection<ChatParticipant> AllParticipants
	{
		get
		{
			return _participants.Values;
		}
	}

	public static void AddMessage(string message, ChatContextType type)
	{
		ChatContext chatContext = null;
		switch (type)
		{
		case ChatContextType.Lobby:
			chatContext = _lobbyContext;
			break;
		case ChatContextType.Game:
			chatContext = _ingameContext;
			break;
		}
		if (chatContext != null)
		{
			chatContext.Add(string.Format("{0}: {3}", DateTime.Now, message));
		}
	}

	public static void AddIncomingMessage(int senderCmid, string senderName, string message, ChatContextType type)
	{
		if (!_participants.ContainsKey(senderCmid))
		{
			_participants.Add(senderCmid, new ChatParticipant(senderCmid, senderName));
		}
		ChatContext value = null;
		switch (type)
		{
		case ChatContextType.Lobby:
			value = _lobbyContext;
			break;
		case ChatContextType.Game:
			value = _ingameContext;
			break;
		case ChatContextType.Private:
			if (!_privateContexts.TryGetValue(senderCmid, out value))
			{
				value = new ChatContext("Private Chat with " + senderName, ChatContextType.Private, 30);
				_privateContexts.Add(senderCmid, value);
			}
			break;
		}
		string text = string.Empty;
		if (PlayerDataManager.Cmid == senderCmid)
		{
			text = "*** ";
		}
		if (value != null)
		{
			value.Add(string.Format("{0}{1} ({2}) \"{3}\": {4}", text, DateTime.Now, senderCmid, senderName, message));
		}
	}

	public static void AddOutgoingPrivateMessage(int recieverCmid, string recieverName, string message, ChatContextType type)
	{
		if (!_participants.ContainsKey(recieverCmid))
		{
			_participants.Add(recieverCmid, new ChatParticipant(recieverCmid, recieverName));
		}
		ChatContext value = null;
		switch (type)
		{
		case ChatContextType.Lobby:
			value = _lobbyContext;
			break;
		case ChatContextType.Game:
			value = _ingameContext;
			break;
		case ChatContextType.Private:
			if (!_privateContexts.TryGetValue(recieverCmid, out value))
			{
				value = new ChatContext("Private Chat with " + recieverName, ChatContextType.Private, 30);
				_privateContexts.Add(recieverCmid, value);
			}
			break;
		}
		if (value != null)
		{
			value.Add(string.Format("*** {0} ({1}) \"{2}\": {3}", DateTime.Now, PlayerDataManager.CmidSecure, PlayerDataManager.NameSecure, message));
		}
	}

	public static string DumpLogs()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (_lobbyContext != null)
		{
			stringBuilder.AppendLine(_lobbyContext.ToString());
		}
		if (_ingameContext != null)
		{
			stringBuilder.AppendLine(_ingameContext.ToString());
		}
		foreach (ChatContext value in _privateContexts.Values)
		{
			stringBuilder.AppendLine(value.ToString());
		}
		return stringBuilder.ToString();
	}
}
