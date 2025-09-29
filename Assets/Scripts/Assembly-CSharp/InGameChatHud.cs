using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class InGameChatHud : Singleton<InGameChatHud>
{
	private class ChatMessage
	{
		public string Sender;

		public string Content;

		public float Timer;

		public int Height;

		public Rect Position;

		public Color Color;
	}

	private enum State
	{
		Normal = 0,
		Ghost = 1,
		Mute = 2
	}

	private const int _maxMessageLength = 140;

	private const int InputHeight = 30;

	private bool _isEnabled;

	private bool _canInput;

	private Rect MsgPosition = new Rect(15f, 40f, 400f, 170f);

	private float MsgLifespan = 10f;

	private float MsgFadeSpeed = 1f;

	private string _inputContent = string.Empty;

	private string _muteMessage = "You are not allowed to chat!";

	private string _spamMessage = "Don't spam!";

	private GUIStyle _msgStyleCache;

	private bool _paused;

	private bool _doFocusOnChat;

	private List<ChatMessage> _chatMsgs;

	private float _chatTimer;

	private float _muteTimer;

	private float _spamTimer;

	private GUIStyle _textFieldStyle = StormFront.InGameChatBlue;

	public bool Enabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			_isEnabled = value;
		}
	}

	public bool CanInput
	{
		get
		{
			return _canInput;
		}
	}

	private GUIStyle MsgStyle
	{
		get
		{
			if (_msgStyleCache == null)
			{
				_msgStyleCache = BlueStonez.label_ingamechat;
			}
			return _msgStyleCache;
		}
	}

	private InGameChatHud()
	{
		_chatMsgs = new List<ChatMessage>(10);
		ClearAll();
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
	}

	public bool CanStartChat()
	{
		return _spamTimer <= 0f && _chatTimer <= 0f && !ClientCommCenter.IsPlayerMuted;
	}

	public void OpenChat()
	{
	}

	public void PushMessage(string input)
	{
		_inputContent = input;
		EndChat();
	}

	public void Update()
	{
		for (int i = 0; i < _chatMsgs.Count; i++)
		{
			_chatMsgs[i].Timer -= Time.deltaTime * MsgFadeSpeed;
			if (_chatMsgs[i].Timer < 0f)
			{
				_chatMsgs.RemoveAt(i);
			}
		}
		if (_chatTimer > 0f)
		{
			_chatTimer -= Time.deltaTime;
		}
		if (_spamTimer > 0f)
		{
			_spamTimer -= Time.deltaTime;
		}
	}

	public void Draw()
	{
		GUI.depth = 9;
		if (!TabScreenPanelGUI.Enabled)
		{
			GUI.BeginGroup(MsgPosition);
			DoChatMessages();
			if (ClientCommCenter.IsPlayerMuted)
			{
				DoMuteMessage();
			}
			else if (_spamTimer > 0f)
			{
				DoSpamMessage();
			}
			else if (_canInput)
			{
				DoChatInput();
			}
			GUI.EndGroup();
			if (_doFocusOnChat)
			{
				GUI.FocusControl("input");
				_doFocusOnChat = false;
			}
		}
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		switch (ev.TeamId)
		{
		case TeamID.NONE:
		case TeamID.BLUE:
			_textFieldStyle = StormFront.InGameChatBlue;
			break;
		case TeamID.RED:
			_textFieldStyle = StormFront.InGameChatRed;
			break;
		}
	}

	private void DoChatMessages()
	{
		MsgStyle.wordWrap = true;
		for (int i = 0; i < _chatMsgs.Count; i++)
		{
			ChatMessage chatMessage = _chatMsgs[i];
			string text = chatMessage.Sender + ": ";
			int num = Mathf.CeilToInt(MsgStyle.CalcHeight(new GUIContent(text), MsgPosition.width));
			string text2 = text + chatMessage.Content;
			float alpha = Mathf.Clamp01(chatMessage.Timer);
			GUI.color = Color.black.SetAlpha(alpha);
			GUI.Label(new Rect(chatMessage.Position.x + 1f, chatMessage.Position.y + 1f, chatMessage.Position.width, chatMessage.Position.height), text2, MsgStyle);
			GUI.color = Color.white.SetAlpha(alpha);
			GUI.Label(chatMessage.Position, text2, MsgStyle);
			GUI.color = chatMessage.Color.SetAlpha(alpha);
			GUI.Label(new Rect(chatMessage.Position.x, chatMessage.Position.y, chatMessage.Position.width, num), text, MsgStyle);
		}
		MsgStyle.wordWrap = false;
	}

	private void DoChatInput()
	{
	}

	private void DoMuteMessage()
	{
		if (!(_muteTimer <= 0f))
		{
			GUI.color = Color.red;
			GUI.Label(new Rect(1f, MsgPosition.height - 30f + 1f, MsgPosition.width, 30f), _muteMessage, MsgStyle);
			GUI.color = Color.white;
			GUI.Label(new Rect(0f, MsgPosition.height - 30f, MsgPosition.width, 30f), _muteMessage, MsgStyle);
			_muteTimer -= Time.deltaTime;
		}
	}

	private void DoSpamMessage()
	{
		GUI.color = Color.red;
		GUI.Label(new Rect(1f, MsgPosition.height - 30f + 1f, MsgPosition.width, 30f), _spamMessage, MsgStyle);
		GUI.color = Color.white;
		GUI.Label(new Rect(0f, MsgPosition.height - 30f, MsgPosition.width, 30f), _spamMessage, MsgStyle);
	}

	public void AddChatMessage(string sender, string message, MemberAccessLevel accessLevel)
	{
		MsgStyle.wordWrap = true;
		string text = sender + ": ";
		string text2 = text + message;
		int num = Mathf.CeilToInt(MsgStyle.CalcHeight(new GUIContent(text2), MsgPosition.width));
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.Sender = sender;
		chatMessage.Content = message;
		chatMessage.Timer = MsgLifespan;
		chatMessage.Height = num;
		chatMessage.Position = new Rect(0f, 0f, MsgPosition.width, num);
		chatMessage.Color = ColorScheme.GetNameColorByAccessLevel(accessLevel);
		ChatMessage item = chatMessage;
		_chatMsgs.Insert(0, item);
		UpdateMessagePosition();
	}

	public void ClearHistory()
	{
		_chatMsgs.Clear();
	}

	public void SendChatMessage()
	{
		string text = TextUtilities.Trim(_inputContent);
		if (text.Length != 0)
		{
			if (!ClientCommCenter.IsPlayerMuted && !CommConnectionManager.CommCenter.SendInGameChatMessage(text, ChatContext.Player))
			{
				_spamTimer = 5f;
			}
			_inputContent = string.Empty;
		}
	}

	public void ClearAll()
	{
		_canInput = false;
		_inputContent = string.Empty;
		_chatMsgs.Clear();
	}

	public void Pause()
	{
		if (_canInput)
		{
			_paused = true;
		}
	}

	public void OnFullScreen()
	{
		UpdateMessagePosition();
	}

	private void BeginChat()
	{
		_doFocusOnChat = true;
		_chatTimer = 0.5f;
		AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
	}

	private void EndChat()
	{
		SendChatMessage();
		_chatTimer = 0.3f;
		if (_paused)
		{
			_paused = false;
			if (GameState.HasCurrentGame && GameState.CurrentGame.IsMatchRunning)
			{
				GameState.LocalPlayer.UnPausePlayer();
			}
		}
		if (!GameState.LocalCharacter.IsAlive)
		{
			Screen.lockCursor = false;
		}
		if (GameState.HasCurrentGame && GameState.CurrentGame.IsMatchRunning && !GameState.LocalPlayer.IsGamePaused)
		{
			AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = true;
		}
	}

	private void UpdateMessagePosition()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < _chatMsgs.Count; i++)
		{
			ChatMessage chatMessage = _chatMsgs[i];
			chatMessage.Position.y = MsgPosition.height - 30f - (float)chatMessage.Height - (float)num;
			num += chatMessage.Height;
			if (chatMessage.Position.y < 0f)
			{
				num2 = i;
				flag = true;
				break;
			}
			if (flag)
			{
				_chatMsgs.RemoveRange(num2, _chatMsgs.Count - num2);
			}
		}
	}
}
