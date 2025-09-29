using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class ChatPageGUI : PageGUI
{
	private const float TitleHeight = 24f;

	private const int SEARCH_HEIGHT = 36;

	private const int TAB_WIDTH = 300;

	private const int CHAT_USER_HEIGHT = 24;

	private const int CHAT_TEXTFIELD_HEIGHT = 36;

	private const float CHECK_PASSWORD_DELAY = 2f;

	private Rect _mainRect;

	private Vector2 _dialogScroll;

	private int _lastMessageCount;

	public static bool IsCompleteLobbyLoaded;

	[SerializeField]
	private Texture2D _speedhackerIcon;

	private float _nextFullLobbyUpdate;

	private float _spammingNotificationTime;

	private float _nextNaughtyListUpdate;

	private bool _checkForPassword;

	private bool _isPasswordOk = true;

	private float _yPosition;

	private float _lastMessageSentTimer = 0.3f;

	private float _nextPasswordCheck;

	private string _inputedPassword = string.Empty;

	private string _currentChatMessage = string.Empty;

	private GameMetaData _game;

	private PopupMenu _playerMenu;

	private TouchScreenKeyboard _chatKeyboard;

	private float _keyboardOffset;

	public static bool IsChatActive
	{
		get
		{
			return GUI.GetNameOfFocusedControl() == "@CurrentChatMessage";
		}
	}

	public static TabArea SelectedTab { get; set; }

	private void Awake()
	{
		_playerMenu = new PopupMenu();
		base.IsOnGUIEnabled = true;
	}

	private void Start()
	{
		_playerMenu.AddMenuItem("Add Friend", MenuCmdAddFriend, MenuChkAddFriend);
		_playerMenu.AddMenuItem("Unfriend", MenuCmdRemoveFriend, MenuChkRemoveFriend);
		_playerMenu.AddMenuItem(LocalizedStrings.PrivateChat, MenuCmdChat, MenuChkChat);
		_playerMenu.AddMenuItem(LocalizedStrings.SendMessage, MenuCmdSendMessage, MenuChkSendMessage);
		_playerMenu.AddMenuItem(LocalizedStrings.JoinGame, MenuCmdJoinGame, MenuChkJoinGame);
		_playerMenu.AddMenuItem(LocalizedStrings.InviteToClan, MenuCmdInviteClan, MenuChkInviteClan);
		if (PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.Moderator)
		{
			_playerMenu.AddMenuItem("MODERATE", MenuCmdModeratePlayer, (CommUser P_0) => true);
		}
	}

	private void Update()
	{
		if (_chatKeyboard != null)
		{
			_currentChatMessage = _chatKeyboard.text;
			_currentChatMessage = _currentChatMessage.Trim('\n');
			if (_chatKeyboard.done && !_chatKeyboard.wasCanceled)
			{
				SendChatMessage();
				_chatKeyboard = null;
			}
			else if (!_chatKeyboard.active)
			{
				_chatKeyboard = null;
			}
		}
		if (_lastMessageSentTimer < 0.3f)
		{
			_lastMessageSentTimer += Time.deltaTime;
		}
		if (_yPosition < 0f)
		{
			_yPosition = Mathf.Lerp(_yPosition, 0.1f, Time.deltaTime * 8f);
		}
		else
		{
			_yPosition = 0f;
		}
	}

	private void OnGUI()
	{
		if (base.IsOnGUIEnabled)
		{
			GUI.skin = BlueStonez.Skin;
			GUI.depth = 9;
			_mainRect = new Rect(0f, GlobalUIRibbon.Instance.Height(), Screen.width, Screen.height - GlobalUIRibbon.Instance.Height());
			DrawGUI(_mainRect);
			if (PopupMenu.Current != null)
			{
				PopupMenu.Current.Draw();
			}
		}
	}

	public override void DrawGUI(Rect rect)
	{
		GUI.BeginGroup(rect, BlueStonez.window);
		if (CommConnectionManager.Client.ConnectionState == PhotonClient.ConnectionStatus.RUNNING)
		{
			DoTabs(new Rect(10f, 0f, 300f, 30f));
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				GUIUtility.keyboardControl = 0;
			}
			Rect rect2 = new Rect(0f, 21f, 300f, rect.height - 21f);
			Rect rect3 = new Rect(299f, 0f, rect.width - 300f, 22f);
			if (_chatKeyboard != null)
			{
				_keyboardOffset = Mathf.Lerp(_keyboardOffset, Screen.height / 2, Time.deltaTime * 2f);
			}
			else
			{
				_keyboardOffset = Mathf.Lerp(_keyboardOffset, 0f, Time.deltaTime * 2f);
			}
			Rect rect4 = new Rect(300f, 22f, rect.width - 300f, rect.height - 22f - 36f - _keyboardOffset);
			Rect rect5 = new Rect(299f, rect.height - 37f, rect.width - 300f + 1f, 37f);
			ChatGroupPanel pane = Singleton<ChatManager>.Instance._commPanes[(int)SelectedTab];
			switch (SelectedTab)
			{
			case TabArea.Lobby:
				DoDialogFooter(rect5, pane, Singleton<ChatManager>.Instance.LobbyDialog);
				DoLobbyCommPane(rect2, pane);
				DoDialogHeader(rect3, Singleton<ChatManager>.Instance.LobbyDialog);
				DoDialog(rect4, pane, Singleton<ChatManager>.Instance.LobbyDialog);
				break;
			case TabArea.Private:
				DoDialogFooter(rect5, pane, Singleton<ChatManager>.Instance._selectedDialog);
				DrawCommPane(rect2, pane);
				DoPrivateDialogHeader(rect3, Singleton<ChatManager>.Instance._selectedDialog);
				DoDialog(rect4, pane, Singleton<ChatManager>.Instance._selectedDialog);
				break;
			case TabArea.Clan:
				DoDialogFooter(rect5, pane, Singleton<ChatManager>.Instance.ClanDialog);
				DrawCommPane(rect2, pane);
				DoDialogHeader(rect3, Singleton<ChatManager>.Instance.ClanDialog);
				DoDialog(rect4, pane, Singleton<ChatManager>.Instance.ClanDialog);
				break;
			case TabArea.InGame:
				DoDialogFooter(rect5, pane, Singleton<ChatManager>.Instance.InGameDialog);
				DrawCommPane(rect2, pane);
				DoDialogHeader(rect3, Singleton<ChatManager>.Instance.InGameDialog);
				DoDialog(rect4, pane, Singleton<ChatManager>.Instance.InGameDialog);
				break;
			case TabArea.Moderation:
				DoModeratorPaneFooter(rect5, pane);
				DrawModeratorCommPane(rect2, pane);
				DoDialogHeader(rect3, Singleton<ChatManager>.Instance.ModerationDialog);
				DoModeratorDialog(rect4, pane);
				break;
			}
		}
		else
		{
			GUI.color = Color.gray;
			if (CommConnectionManager.Client.ConnectionState == PhotonClient.ConnectionStatus.STOPPED)
			{
				GUI.Label(new Rect(0f, rect.height / 2f, rect.width, 20f), LocalizedStrings.ServerIsNotReachable, BlueStonez.label_interparkbold_11pt);
			}
			else
			{
				GUI.Label(new Rect(0f, rect.height / 2f, rect.width, 20f), LocalizedStrings.ConnectingToServer, BlueStonez.label_interparkbold_11pt);
			}
			GUI.color = Color.white;
		}
		GUI.EndGroup();
		if (_checkForPassword)
		{
			PasswordCheck(new Rect((float)(Screen.width - 280) * 0.5f, (float)(Screen.height - 200) * 0.5f, 280f, 200f));
		}
		GuiManager.DrawTooltip();
	}

	private void PasswordCheck(Rect position)
	{
		if (_game == null)
		{
			return;
		}
		GUITools.PushGUIState();
		GUI.BeginGroup(position, GUIContent.none, BlueStonez.window);
		GUI.Label(new Rect(0f, 0f, position.width, 56f), LocalizedStrings.EnterPassword, BlueStonez.tab_strip);
		GUI.Box(new Rect(16f, 55f, position.width - 32f, position.height - 56f - 64f), GUIContent.none, BlueStonez.window_standard_grey38);
		GUI.SetNextControlName("@EnterPassword");
		Rect position2 = new Rect((position.width - 188f) / 2f, 80f, 188f, 24f);
		_inputedPassword = GUI.PasswordField(position2, _inputedPassword, '*', 18, BlueStonez.textField);
		_inputedPassword = _inputedPassword.Trim('\n');
		if (string.IsNullOrEmpty(_inputedPassword) && GUI.GetNameOfFocusedControl() != "@EnterPassword")
		{
			GUI.color = new Color(1f, 1f, 1f, 0.3f);
			GUI.Label(position2, LocalizedStrings.TypePasswordHere, BlueStonez.label_interparkmed_11pt);
			GUI.color = Color.white;
		}
		GUI.enabled = Time.time > _nextPasswordCheck;
		if (GUITools.Button(new Rect(position.width - 100f - 10f, 152f, 100f, 32f), new GUIContent(LocalizedStrings.OkCaps), BlueStonez.button) || (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.Layout && Time.time > _nextPasswordCheck))
		{
			if (_inputedPassword == _game.Password)
			{
				_checkForPassword = false;
				_inputedPassword = string.Empty;
				_isPasswordOk = true;
				Singleton<GameStateController>.Instance.JoinGame(_game);
				_game = null;
			}
			else
			{
				_inputedPassword = string.Empty;
				_isPasswordOk = false;
				_nextPasswordCheck = Time.time + 2f;
			}
		}
		GUI.enabled = true;
		if (GUITools.Button(new Rect(10f, 152f, 100f, 32f), new GUIContent(LocalizedStrings.CancelCaps), BlueStonez.button))
		{
			_isPasswordOk = true;
			_checkForPassword = false;
			_inputedPassword = string.Empty;
			_game = null;
		}
		if (!_isPasswordOk && string.IsNullOrEmpty(_inputedPassword))
		{
			GUI.color = Color.red;
			GUI.Label(new Rect((position.width - 188f) / 2f, 110f, 188f, 24f), LocalizedStrings.PasswordIncorrect, BlueStonez.label_interparkbold_11pt);
			GUI.color = Color.white;
		}
		GUI.EndGroup();
		GUITools.PopGUIState();
	}

	public void JoinRoom(CmuneRoomID roomId)
	{
		GameConnectionManager.RequestRoomMetaData(roomId, OnRequestRoomMetaData);
	}

	private void OnRequestRoomMetaData(int returncode, GameMetaData data)
	{
		switch (returncode)
		{
		case 0:
			if (PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA)
			{
				_game = null;
				_checkForPassword = false;
				Singleton<GameStateController>.Instance.JoinGame(data);
			}
			else if (JoinGameUtil.IsMobileChannel(_playerMenu.SelectedUser.Channel) && !JoinGameUtil.IsMobileChannel(ApplicationDataManager.Channel))
			{
				PopupSystem.ShowMessage("Error Joining Game Server", "Sorry, only mobile players can join mobile game servers.");
			}
			else if (data.IsFull)
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ThisGameIsFull, PopupSystem.AlertType.OK, null);
			}
			else if (data.IsLevelAllowed(PlayerDataManager.PlayerLevelSecure))
			{
				if (data.IsPublic)
				{
					_game = null;
					_checkForPassword = false;
					Singleton<GameStateController>.Instance.JoinGame(data);
				}
				else
				{
					_game = data;
					_checkForPassword = true;
				}
			}
			else
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, string.Format(LocalizedStrings.YouHaveToReachLevelNToJoinThisGame, data.LevelMin));
			}
			break;
		case 1:
			PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ThisGameNoLongerExists, PopupSystem.AlertType.OK, null);
			break;
		case 2:
			PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ServerIsNotReachable, PopupSystem.AlertType.OK, null);
			break;
		}
	}

	private int DoModeratorControlPanel(Rect rect, ChatGroupPanel pane)
	{
		if (PlayerDataManager.AccessLevel >= MemberAccessLevel.Moderator)
		{
			int num = 0;
			bool flag = PlayerDataManager.AccessLevel >= MemberAccessLevel.Moderator;
			bool flag2 = flag && IsCompleteLobbyLoaded;
			rect = new Rect(rect.x, rect.yMax - 36f - (float)(flag2 ? 60 : (flag ? 30 : 0)) - 1f, rect.width, 37 + (flag2 ? 60 : (flag ? 30 : 0)));
			GUI.BeginGroup(rect, GUIContent.none, BlueStonez.window_standard_grey38);
			if (flag)
			{
				GUI.enabled = _nextNaughtyListUpdate < Time.time;
				if (GUITools.Button(new Rect(6f, rect.height - 61f, (rect.width - 12f) * 0.5f, 26f), new GUIContent((!(_nextNaughtyListUpdate < Time.time)) ? string.Format("Next Update ({0:N0})", _nextNaughtyListUpdate - Time.time) : "Update Naughty List"), BlueStonez.buttondark_medium))
				{
					_nextNaughtyListUpdate = Time.time + 10f;
					CommConnectionManager.CommCenter.UpdateActorsForModeration();
				}
				GUI.enabled = true;
				GUI.enabled = _nextNaughtyListUpdate < Time.time;
				if (GUITools.Button(new Rect(6f + (rect.width - 12f) * 0.5f, rect.height - 61f, (rect.width - 12f) * 0.5f, 26f), new GUIContent((!(_nextNaughtyListUpdate < Time.time)) ? string.Format("Next Update ({0:N0})", _nextNaughtyListUpdate - Time.time) : "Unban Next 50"), BlueStonez.buttondark_medium))
				{
					List<CommUser> list = new List<CommUser>(Singleton<ChatManager>.Instance.NaughtyUsers);
					int num2 = 0;
					foreach (CommUser item in list)
					{
						if (item.Name.StartsWith("Banned:"))
						{
							CommConnectionManager.CommCenter.SendClearAllFlags(item.Cmid);
							Singleton<ChatManager>.Instance._selectedCmid = 0;
							Singleton<ChatManager>.Instance._modUsers.Remove(item.Cmid);
							if (++num2 > 50)
							{
								break;
							}
						}
					}
				}
				GUI.enabled = true;
				num += ((!IsCompleteLobbyLoaded) ? 30 : 60);
			}
			bool flag3 = !string.IsNullOrEmpty(pane.SearchText);
			GUI.SetNextControlName("@ModSearch");
			GUI.changed = false;
			pane.SearchText = GUI.TextField(new Rect(6f, rect.height - 30f, rect.width - (float)((!flag3) ? 12 : 37), 24f), pane.SearchText, 20, BlueStonez.textField);
			if (!flag3 && GUI.GetNameOfFocusedControl() != "@ModSearch")
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
				GUI.Label(new Rect(12f, rect.height - 30f, rect.width - 20f, 24f), LocalizedStrings.Search, BlueStonez.label_interparkmed_10pt_left);
				GUI.color = Color.white;
			}
			if (flag3 && GUITools.Button(new Rect(rect.width - 28f, rect.height - 30f, 22f, 22f), new GUIContent("x"), BlueStonez.panelquad_button))
			{
				pane.SearchText = string.Empty;
				GUIUtility.keyboardControl = 0;
			}
			pane.SearchText = TextUtilities.Trim(pane.SearchText);
			GUI.EndGroup();
			return num + 36;
		}
		return 0;
	}

	public void DrawCommPane(Rect rect, ChatGroupPanel pane)
	{
		GUI.BeginGroup(rect);
		bool flag = GUI.enabled;
		GUI.enabled &= !PopupMenu.IsEnabled && !_checkForPassword;
		float height = rect.height;
		float height2 = Mathf.Max(height, pane.ContentHeight);
		float num = 0f;
		pane.Scroll = GUITools.BeginScrollView(new Rect(0f, 0f, rect.width, height), pane.Scroll, new Rect(0f, 0f, rect.width - 17f, height2), false, true);
		GUI.BeginGroup(new Rect(0f, 0f, rect.width, height + pane.Scroll.y));
		foreach (ChatGroup group in pane.Groups)
		{
			num += DrawPlayerGroup(group, num, rect.width - 17f, pane.SearchText.ToLower());
		}
		GUI.EndGroup();
		GUITools.EndScrollView();
		pane.ContentHeight = num;
		GUI.enabled = flag;
		GUI.EndGroup();
	}

	private void DoLobbyCommPane(Rect rect, ChatGroupPanel pane)
	{
		GUI.BeginGroup(rect);
		bool flag = GUI.enabled;
		GUI.enabled &= !PopupMenu.IsEnabled && !_checkForPassword;
		int num = DoLobbyControlPanel(new Rect(0f, 0f, rect.width, rect.height), pane);
		float num2 = rect.height - (float)num;
		float height = Mathf.Max(num2, pane.ContentHeight);
		float num3 = 0f;
		pane.Scroll = GUITools.BeginScrollView(new Rect(0f, 0f, rect.width, num2), pane.Scroll, new Rect(0f, 0f, rect.width - 17f, height), false, true);
		GUI.BeginGroup(new Rect(0f, 0f, rect.width, num2 + pane.Scroll.y));
		foreach (ChatGroup group in pane.Groups)
		{
			num3 += DrawPlayerGroup(group, num3, rect.width - 17f, pane.SearchText.ToLower());
		}
		GUI.EndGroup();
		GUITools.EndScrollView();
		pane.ContentHeight = num3;
		GUI.enabled = flag;
		GUI.EndGroup();
	}

	private void DrawModeratorCommPane(Rect rect, ChatGroupPanel pane)
	{
		GUI.BeginGroup(rect);
		bool flag = GUI.enabled;
		GUI.enabled &= PopupMenu.Current == null && !_checkForPassword;
		int num = 0;
		num = DoModeratorControlPanel(new Rect(0f, 0f, rect.width, rect.height), pane);
		float num2 = rect.height - (float)num;
		float height = Mathf.Max(num2, pane.ContentHeight);
		float num3 = 0f;
		pane.Scroll = GUITools.BeginScrollView(new Rect(0f, 0f, rect.width, num2), pane.Scroll, new Rect(0f, 0f, rect.width - 17f, height), false, true);
		GUI.BeginGroup(new Rect(0f, 0f, rect.width, num2 + pane.Scroll.y));
		foreach (ChatGroup group in pane.Groups)
		{
			num3 += DrawPlayerGroup(group, num3, rect.width - 17f, pane.SearchText.ToLower(), true);
			if (num3 > pane.Scroll.y + num2)
			{
				break;
			}
		}
		GUI.EndGroup();
		GUITools.EndScrollView();
		pane.ContentHeight = num3;
		GUI.enabled = flag;
		GUI.EndGroup();
	}

	private int DoLobbyControlPanel(Rect rect, ChatGroupPanel pane)
	{
		int num = 0;
		bool flag = PlayerDataManager.AccessLevel >= MemberAccessLevel.Moderator;
		bool flag2 = flag && IsCompleteLobbyLoaded;
		rect = new Rect(rect.x, rect.yMax - 36f - (float)(flag2 ? 60 : (flag ? 30 : 0)) - 1f, rect.width, 37 + (flag2 ? 60 : (flag ? 30 : 0)));
		GUI.BeginGroup(rect, GUIContent.none, BlueStonez.window_standard_grey38);
		if (flag)
		{
			GUI.enabled = _nextFullLobbyUpdate < Time.time;
			if (flag2 && GUITools.Button(new Rect(6f, 5f, rect.width - 12f, 26f), new GUIContent("Reset Lobby"), BlueStonez.buttondark_medium))
			{
				IsCompleteLobbyLoaded = false;
				_nextFullLobbyUpdate = Time.time + 10f;
				CommConnectionManager.CommCenter.SendUpdateResetLobby();
			}
			if (GUITools.Button(new Rect(6f, rect.height - 61f, rect.width - 12f, 26f), new GUIContent((!(_nextFullLobbyUpdate < Time.time)) ? string.Format("Next Update ({0:N0})", _nextFullLobbyUpdate - Time.time) : "Get All Players "), BlueStonez.buttondark_medium))
			{
				IsCompleteLobbyLoaded = true;
				_nextFullLobbyUpdate = Time.time + 10f;
				CommConnectionManager.CommCenter.SendUpdateAllPlayers();
			}
			GUI.enabled = true;
			num += ((!IsCompleteLobbyLoaded) ? 30 : 60);
		}
		bool flag3 = !string.IsNullOrEmpty(pane.SearchText);
		GUI.SetNextControlName("@LobbySearch");
		GUI.changed = false;
		pane.SearchText = GUI.TextField(new Rect(6f, rect.height - 30f, rect.width - (float)((!flag3) ? 12 : 37), 24f), pane.SearchText, 20, BlueStonez.textField);
		if (!flag3 && GUI.GetNameOfFocusedControl() != "@LobbySearch")
		{
			GUI.color = new Color(1f, 1f, 1f, 0.3f);
			GUI.Label(new Rect(12f, rect.height - 30f, rect.width - 20f, 24f), LocalizedStrings.Search, BlueStonez.label_interparkmed_10pt_left);
			GUI.color = Color.white;
		}
		if (flag3 && GUITools.Button(new Rect(rect.width - 28f, rect.height - 30f, 22f, 22f), new GUIContent("x"), BlueStonez.panelquad_button))
		{
			pane.SearchText = string.Empty;
			GUIUtility.keyboardControl = 0;
		}
		pane.SearchText = TextUtilities.Trim(pane.SearchText);
		GUI.EndGroup();
		return num + 36;
	}

	public float DrawPlayerGroup(ChatGroup group, float vOffset, float width, string search, bool allowSelfSelection = false)
	{
		Rect position = new Rect(0f, vOffset, width, 24f);
		GUITools.PushGUIState();
		GUI.enabled &= !PopupMenu.IsEnabled;
		GUI.Label(position, GUIContent.none, BlueStonez.window_standard_grey38);
		if (group.Players != null)
		{
			GUI.Label(position, group.Title + " (" + group.Players.Count + ")", BlueStonez.label_interparkbold_11pt);
		}
		GUITools.PopGUIState();
		vOffset += 24f;
		int num = 0;
		if (group.Players != null)
		{
			GUITools.PushGUIState();
			GUI.enabled &= !PopupMenu.IsEnabled;
			GUI.BeginGroup(new Rect(0f, vOffset, width, group.Players.Count * 24));
			foreach (CommUser player in group.Players)
			{
				if (string.IsNullOrEmpty(search) || player.Name.ToLower().Contains(search))
				{
					GroupDrawUser(num++ * 24, width, player, allowSelfSelection);
				}
			}
			GUI.EndGroup();
			GUITools.PopGUIState();
		}
		return 24f + (float)(group.Players.Count * 24);
	}

	private void DoTabs(Rect rect)
	{
		float num = Mathf.Floor(rect.width / (float)Singleton<ChatManager>.Instance.TabCounter);
		bool flag = false;
		bool flag2 = false;
		int num2 = 0;
		if (GUI.Toggle(new Rect(rect.x + (float)num2 * num, rect.y, num, rect.height), SelectedTab == TabArea.Lobby, LocalizedStrings.Lobby, BlueStonez.tab_medium) && SelectedTab != TabArea.Lobby)
		{
			SelectedTab = TabArea.Lobby;
			flag = true;
		}
		num2++;
		if (GUI.Toggle(new Rect(rect.x + (float)num2 * num, rect.y, num, rect.height), SelectedTab == TabArea.Private, LocalizedStrings.Private, BlueStonez.tab_medium) && SelectedTab != TabArea.Private)
		{
			SelectedTab = TabArea.Private;
			flag = true;
			Singleton<ChatManager>.Instance.HasUnreadPrivateMessage.Value = false;
		}
		if ((bool)Singleton<ChatManager>.Instance.HasUnreadPrivateMessage)
		{
			GUI.DrawTexture(new Rect(rect.x + (float)num2 * num, rect.y + 1f, 18f, 18f), CommunicatorIcons.NewInboxMessage);
		}
		num2++;
		if (Singleton<ChatManager>.Instance.ShowTab(TabArea.Clan))
		{
			if (GUI.Toggle(new Rect(rect.x + (float)num2 * num, rect.y, num, rect.height), SelectedTab == TabArea.Clan, LocalizedStrings.Clan, BlueStonez.tab_medium) && SelectedTab != TabArea.Clan)
			{
				SelectedTab = TabArea.Clan;
				flag = true;
				Singleton<ChatManager>.Instance.HasUnreadClanMessage.Value = false;
			}
			if (PlayerDataManager.IsPlayerInClan && (bool)Singleton<ChatManager>.Instance.HasUnreadClanMessage)
			{
				GUI.DrawTexture(new Rect(rect.x + (float)num2 * num, rect.y + 1f, 18f, 18f), CommunicatorIcons.NewInboxMessage);
			}
			num2++;
		}
		if (Singleton<ChatManager>.Instance.ShowTab(TabArea.InGame))
		{
			if (GUI.Toggle(new Rect(rect.x + (float)num2 * num, rect.y, num, rect.height), SelectedTab == TabArea.InGame, LocalizedStrings.Game, BlueStonez.tab_medium) && SelectedTab != TabArea.InGame)
			{
				SelectedTab = TabArea.InGame;
				_currentChatMessage = string.Empty;
				flag = true;
			}
			num2++;
		}
		if (Singleton<ChatManager>.Instance.ShowTab(TabArea.Moderation))
		{
			if (GUI.Toggle(new Rect(rect.x + (float)num2 * num, rect.y, num, rect.height), SelectedTab == TabArea.Moderation, LocalizedStrings.Admin, BlueStonez.tab_medium) && SelectedTab != TabArea.Moderation && PlayerDataManager.AccessLevelSecure > MemberAccessLevel.Default && PlayerDataManager.AccessLevelSecure <= MemberAccessLevel.Admin)
			{
				SelectedTab = TabArea.Moderation;
				_currentChatMessage = string.Empty;
				flag = true;
			}
			num2++;
		}
		if (flag)
		{
			_currentChatMessage = string.Empty;
			PopupMenu.Hide();
			GUIUtility.keyboardControl = 0;
		}
	}

	private void DoDialog(Rect rect, ChatGroupPanel pane, ChatDialog dialog)
	{
		if (dialog == null)
		{
			return;
		}
		dialog.CheckSize(rect);
		GUI.BeginGroup(new Rect(rect.x, rect.y + Mathf.Clamp(rect.height - dialog._heightCache, 0f, rect.height), rect.width, rect.height));
		int num = 0;
		float num2 = 0f;
		if (_lastMessageCount != dialog._msgQueue.Count)
		{
			if (!Input.GetMouseButton(0))
			{
				_dialogScroll.y = float.MaxValue;
			}
			_lastMessageCount = dialog._msgQueue.Count;
		}
		_dialogScroll = GUITools.BeginScrollView(new Rect(0f, 0f, dialog._frameSize.x, dialog._frameSize.y), _dialogScroll, new Rect(0f, 0f, dialog._contentSize.x, dialog._contentSize.y));
		foreach (InstantMessage item in dialog._msgQueue)
		{
			if (dialog.CanShow == null || dialog.CanShow(item.Context))
			{
				if (num % 2 == 0)
				{
					GUI.Label(new Rect(0f, num2, dialog._contentSize.x - 1f, dialog._msgHeight[num]), GUIContent.none, BlueStonez.box_grey38);
				}
				GUI.color = GetNameColor(item);
				GUI.Label(new Rect(4f, num2, dialog._contentSize.x - 8f, 20f), item.PlayerName + ":", BlueStonez.label_interparkbold_11pt_left);
				GUI.color = new Color(0.9f, 0.9f, 0.9f);
				GUI.Label(new Rect(4f, num2 + 20f, dialog._contentSize.x - 8f, dialog._msgHeight[num] - 20f), item.MessageText, BlueStonez.label_interparkmed_11pt_left);
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				GUI.Label(new Rect(4f, num2, dialog._contentSize.x - 8f, 20f), item.MessageDateTime, BlueStonez.label_interparkmed_10pt_right);
				GUI.color = Color.white;
				num2 += dialog._msgHeight[num];
				num++;
			}
		}
		GUITools.EndScrollView();
		dialog._heightCache = num2;
		GUI.EndGroup();
	}

	private void DoModeratorDialog(Rect rect, ChatGroupPanel pane)
	{
		if (PlayerDataManager.AccessLevel < MemberAccessLevel.Moderator)
		{
			return;
		}
		GUI.BeginGroup(rect, GUIContent.none);
		CommUser value;
		if (Singleton<ChatManager>.Instance._modUsers.TryGetValue(Singleton<ChatManager>.Instance._selectedCmid, out value) && value != null)
		{
			GUI.TextField(new Rect(10f, 15f, rect.width, 20f), "Name: " + value.Name, BlueStonez.label_interparkbold_11pt_left);
			GUI.TextField(new Rect(10f, 37f, rect.width, 20f), "Cmid: " + value.Cmid, BlueStonez.label_interparkmed_11pt_left);
			if (PlayerDataManager.AccessLevel == MemberAccessLevel.Admin)
			{
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					GUI.TextField(new Rect(10f, 52f, rect.width, 20f), "http://instrumentation.cmune.com/Members/SeeMember.aspx?cmid=" + value.Cmid, BlueStonez.label_interparkbold_11pt_left);
				}
				else if (GUITools.Button(new Rect(10f, 52f, 70f, 20f), new GUIContent("Open Profile"), BlueStonez.label_interparkmed_11pt_url))
				{
					Application.OpenURL("http://instrumentation.cmune.com/Members/SeeMember.aspx?cmid=" + value.Cmid);
				}
			}
			float num = rect.width - 20f;
			GUI.BeginGroup(new Rect(10f, 80f, num, rect.height - 70f), GUIContent.none, BlueStonez.box_grey50);
			if (GUITools.Button(new Rect(5f, 5f, num - 10f, 20f), new GUIContent("Clear and Unban"), BlueStonez.buttondark_medium))
			{
				CommConnectionManager.CommCenter.SendClearAllFlags(value.Cmid);
				Singleton<ChatManager>.Instance._selectedCmid = 0;
				Singleton<ChatManager>.Instance._modUsers.Remove(value.Cmid);
			}
			int num2 = 40;
			if ((value.ModerationFlag & 4) != 0)
			{
				GUI.Label(new Rect(8f, num2, num - 10f, 20f), "- BANNED", BlueStonez.label_interparkbold_11pt_left);
				num2 += 20;
			}
			if ((value.ModerationFlag & 2) != 0)
			{
				GUI.Label(new Rect(8f, num2, num - 10f, 20f), "- Ghosted", BlueStonez.label_interparkmed_11pt_left);
				num2 += 20;
			}
			if ((value.ModerationFlag & 1) != 0)
			{
				GUI.Label(new Rect(8f, num2, num - 10f, 20f), "- Muted", BlueStonez.label_interparkmed_11pt_left);
				num2 += 20;
			}
			if ((value.ModerationFlag & 8) != 0)
			{
				GUI.Label(new Rect(8f, num2, num - 10f, 20f), "- Speed " + value.ModerationInfo, BlueStonez.label_interparkmed_11pt_left);
				num2 += 20;
			}
			if ((value.ModerationFlag & 0x10) != 0)
			{
				GUI.Label(new Rect(8f, num2, num - 10f, 20f), "- Spamming", BlueStonez.label_interparkmed_11pt_left);
				num2 += 20;
			}
			if ((value.ModerationFlag & 0x20) != 0)
			{
				GUI.Label(new Rect(8f, num2, num - 10f, 20f), "- CrudeLanguage", BlueStonez.label_interparkmed_11pt_left);
				num2 += 20;
			}
			GUI.Label(new Rect(8f, num2 + 20, num - 10f, 100f), value.ModerationInfo, BlueStonez.label_interparkmed_11pt_left);
			GUI.EndGroup();
		}
		else
		{
			GUI.Label(new Rect(0f, rect.height / 2f, rect.width, 20f), "No user selected", BlueStonez.label_interparkmed_11pt);
		}
		GUI.EndGroup();
	}

	private void DoDialogHeader(Rect rect, ChatDialog d)
	{
		GUI.Label(rect, GUIContent.none, BlueStonez.window_standard_grey38);
		GUI.Label(rect, d.Title, BlueStonez.label_interparkbold_11pt);
	}

	private void DoPrivateDialogHeader(Rect rect, ChatDialog d)
	{
		GUI.Label(rect, GUIContent.none, BlueStonez.window_standard_grey38);
		if (d != null && d.UserCmid > 0)
		{
			GUI.Label(rect, d.Title, BlueStonez.label_interparkbold_11pt);
			if (GUITools.Button(new Rect(rect.x + rect.width - 20f, rect.y + 3f, 16f, 16f), new GUIContent("x"), BlueStonez.panelquad_button))
			{
				Singleton<ChatManager>.Instance.RemoveDialog(d);
			}
		}
		else
		{
			GUI.Label(rect, LocalizedStrings.PrivateChat, BlueStonez.label_interparkbold_11pt);
		}
	}

	private void DoModeratorPaneFooter(Rect rect, ChatGroupPanel pane)
	{
		GUI.BeginGroup(rect, BlueStonez.window_standard_grey38);
		CommUser user;
		if (Singleton<ChatManager>.Instance._selectedCmid > 0 && Singleton<ChatManager>.Instance.TryGetLobbyCommUser(Singleton<ChatManager>.Instance._selectedCmid, out user) && user != null)
		{
			if (GUITools.Button(new Rect(5f, 6f, rect.width - 10f, rect.height - 12f), new GUIContent("Moderate User"), BlueStonez.buttondark_medium))
			{
				ModerationPanelGUI moderationPanelGUI = PanelManager.Instance.OpenPanel(PanelType.Moderation) as ModerationPanelGUI;
				if ((bool)moderationPanelGUI)
				{
					moderationPanelGUI.SetSelectedUser(user);
				}
			}
		}
		else if (GUITools.Button(new Rect(5f, 6f, rect.width - 10f, rect.height - 12f), new GUIContent("Open Moderator"), BlueStonez.buttondark_medium))
		{
			PanelManager.Instance.OpenPanel(PanelType.Moderation);
		}
		GUI.EndGroup();
	}

	private void DoDialogFooter(Rect rect, ChatGroupPanel pane, ChatDialog dialog)
	{
		GUI.BeginGroup(rect, BlueStonez.window_standard_grey38);
		bool flag = GUI.enabled;
		GUI.enabled &= !ClientCommCenter.IsPlayerMuted && dialog != null && dialog.CanChat;
		if (SelectedTab == TabArea.InGame)
		{
			GUI.enabled &= GameState.HasCurrentGame && GameState.CurrentGame.IsGameStarted;
		}
		if (GUI.Button(new Rect(6f, 6f, rect.width - 60f, rect.height - 12f), new GUIContent(_currentChatMessage), BlueStonez.textField))
		{
			_chatKeyboard = TouchScreenKeyboard.Open(_currentChatMessage, TouchScreenKeyboardType.ASCIICapable, true, false, false, false);
		}
		if (_spammingNotificationTime > Time.time)
		{
			GUI.color = Color.red;
			GUI.Label(new Rect(15f, 6f, rect.width - 66f, rect.height - 12f), LocalizedStrings.DontSpamTheLobbyChat, BlueStonez.label_interparkmed_10pt_left);
			GUI.color = Color.white;
		}
		else
		{
			string empty = string.Empty;
			empty = ((dialog == null || dialog.UserCmid <= 0) ? LocalizedStrings.EnterAMessageHere : ((!dialog.CanChat) ? (dialog.UserName + LocalizedStrings.Offline) : LocalizedStrings.EnterAMessageHere));
			if (string.IsNullOrEmpty(_currentChatMessage) && GUI.GetNameOfFocusedControl() != "@CurrentChatMessage")
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
				GUI.Label(new Rect(10f, 6f, rect.width - 66f, rect.height - 12f), empty, BlueStonez.label_interparkmed_10pt_left);
				GUI.color = Color.white;
			}
		}
		if ((GUITools.Button(new Rect(rect.width - 51f, 6f, 45f, rect.height - 12f), new GUIContent(LocalizedStrings.Send), BlueStonez.buttondark_small) || Event.current.keyCode == KeyCode.Return) && !ClientCommCenter.IsPlayerMuted && _lastMessageSentTimer > 0.29f)
		{
			SendChatMessage();
			GUI.FocusControl("@CurrentChatMessage");
		}
		GUI.enabled = flag;
		GUI.EndGroup();
	}

	private void GroupDrawUser(float vOffset, float width, CommUser user, bool allowSelfSelection = false)
	{
		int cmid = PlayerDataManager.Cmid;
		Rect rect = new Rect(3f, vOffset, width - 3f, 24f);
		if (Singleton<ChatManager>.Instance._selectedCmid == user.Cmid)
		{
			Color uberStrikeBlue = ColorScheme.UberStrikeBlue;
			float r = uberStrikeBlue.r;
			Color uberStrikeBlue2 = ColorScheme.UberStrikeBlue;
			float g = uberStrikeBlue2.g;
			Color uberStrikeBlue3 = ColorScheme.UberStrikeBlue;
			GUI.color = new Color(r, g, uberStrikeBlue3.b, 0.5f);
			GUI.Label(rect, GUIContent.none, BlueStonez.box_white);
			GUI.color = Color.white;
		}
		bool flag = GUI.enabled;
		if (user.Cmid != cmid && CheckMouseClickIn(rect, 1))
		{
			SelectCommUser(user);
			_playerMenu.Show(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), user);
		}
		if (Singleton<MouseInput>.Instance.DoubleClick(rect) && user.Cmid != cmid && user.PresenceIndex != PresenceType.Offline && (SelectedTab != TabArea.Private || Singleton<ChatManager>.Instance._selectedCmid != user.Cmid))
		{
			SelectCommUser(user);
			Singleton<ChatManager>.Instance.CreatePrivateChat(user.Cmid);
			Event.current.Use();
			return;
		}
		if ((allowSelfSelection || user.Cmid != cmid) && CheckMouseClickIn(new Rect(rect.x, rect.y, rect.width - 20f, rect.height)))
		{
			SelectCommUser(user);
		}
		GUI.Label(new Rect(10f, vOffset + 3f, 11.2f, 16f), ChatManager.GetPresenceIcon(user.PresenceIndex), GUIStyle.none);
		GUI.Label(new Rect(23f, vOffset + 3f, 16f, 16f), UberstrikeIconsHelper.GetIconForChannel(user.Channel), GUIStyle.none);
		if (user.Cmid == PlayerDataManager.Cmid)
		{
			GUI.color = ColorScheme.ChatNameCurrentUser;
		}
		else if (user.IsFriend || user.IsClanMember)
		{
			GUI.color = ColorScheme.ChatNameFriendsUser;
		}
		else if (user.IsFacebookFriend)
		{
			GUI.color = ColorScheme.ChatNameFacebookFriendUser;
		}
		else
		{
			GUI.color = ColorScheme.GetNameColorByAccessLevel(user.AccessLevel);
		}
		GUI.Label(new Rect(44f, vOffset, width - 66f, 24f), user.Name, BlueStonez.label_interparkmed_10pt_left);
		GUI.color = Color.white;
		if (user.Cmid != cmid && GUI.Button(new Rect(rect.width - 17f, vOffset + 1f, 18f, 18f), GUIContent.none, BlueStonez.button_context))
		{
			SelectCommUser(user);
			_playerMenu.Show(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), user);
		}
		GUI.Box(rect.Expand(0, -1), GUIContent.none, BlueStonez.dropdown_list);
		ChatDialog value;
		if (SelectedTab == TabArea.Private && Singleton<ChatManager>.Instance._dialogsByCmid.TryGetValue(user.Cmid, out value) && value != null && value.HasUnreadMessage && value != Singleton<ChatManager>.Instance._selectedDialog)
		{
			GUI.Label(new Rect(rect.width - 50f, vOffset, 25f, 25f), CommunicatorIcons.NewInboxMessage);
		}
		if (PlayerDataManager.AccessLevel > MemberAccessLevel.Default)
		{
			int num = user.ModerationFlag & 0xC;
			if (num == 8)
			{
				GUI.Label(new Rect(width - 50f, vOffset + 3f, 20f, 20f), _speedhackerIcon);
			}
		}
		GUI.enabled = flag;
	}

	private void SelectCommUser(CommUser user)
	{
		Singleton<ChatManager>.Instance._selectedCmid = user.Cmid;
		if (SelectedTab == TabArea.Private)
		{
			ChatDialog value;
			if (Singleton<ChatManager>.Instance._dialogsByCmid.TryGetValue(user.Cmid, out value))
			{
				value.HasUnreadMessage = false;
			}
			else
			{
				value = Singleton<ChatManager>.Instance.AddNewDialog(user);
			}
			Singleton<ChatManager>.Instance._selectedDialog = value;
			_currentChatMessage = string.Empty;
		}
	}

	private void SendChatMessage()
	{
		if (string.IsNullOrEmpty(_currentChatMessage))
		{
			return;
		}
		_dialogScroll.y = float.MaxValue;
		_currentChatMessage = TextUtilities.ShortenText(TextUtilities.Trim(_currentChatMessage), 140, false);
		switch (SelectedTab)
		{
		case TabArea.InGame:
			CommConnectionManager.CommCenter.SendInGameChatMessage(_currentChatMessage, ChatContext.Player);
			break;
		case TabArea.Lobby:
			if (!CommConnectionManager.CommCenter.SendLobbyChatMessage(_currentChatMessage))
			{
				_spammingNotificationTime = Time.time + 5f;
			}
			break;
		case TabArea.Clan:
		{
			string nameSecure = PlayerDataManager.NameSecure;
			int cmidSecure = PlayerDataManager.CmidSecure;
			foreach (CommUser clanUser in Singleton<ChatManager>.Instance.ClanUsers)
			{
				if (clanUser.IsOnline)
				{
					CommConnectionManager.CommCenter.SendClanChatMessage(cmidSecure, clanUser.ActorId, nameSecure, _currentChatMessage);
				}
			}
			break;
		}
		case TabArea.Private:
		{
			CommActorInfo actor;
			if (Singleton<ChatManager>.Instance._selectedDialog != null && CommConnectionManager.TryGetActor(Singleton<ChatManager>.Instance._selectedDialog.UserCmid, out actor))
			{
				CommConnectionManager.CommCenter.SendPrivateChatMessage(actor, _currentChatMessage);
			}
			break;
		}
		}
		_lastMessageSentTimer = 0f;
		_currentChatMessage = string.Empty;
	}

	private Color GetNameColor(InstantMessage msg)
	{
		if (msg.Cmid == PlayerDataManager.Cmid)
		{
			return ColorScheme.ChatNameCurrentUser;
		}
		if (msg.IsFriend || msg.IsClan)
		{
			return ColorScheme.ChatNameFriendsUser;
		}
		if (msg.IsFacebookFriend)
		{
			return ColorScheme.ChatNameFacebookFriendUser;
		}
		return ColorScheme.GetNameColorByAccessLevel(msg.AccessLevel);
	}

	private bool CheckMouseClickIn(Rect rect, int mouse = 0)
	{
		return InputManager.GetMouseButtonDown(mouse) && rect.Contains(Event.current.mousePosition);
	}

	private void MenuCmdRemoveFriend(CommUser user)
	{
		if (user != null)
		{
			int friendCmid = user.Cmid;
			PopupSystem.ShowMessage(LocalizedStrings.RemoveFriendCaps, string.Format(LocalizedStrings.DoYouReallyWantToRemoveNFromYourFriendsList, user.Name), PopupSystem.AlertType.OKCancel, delegate
			{
				Singleton<InboxManager>.Instance.RemoveFriend(friendCmid);
			}, LocalizedStrings.Remove, null, LocalizedStrings.Cancel, PopupSystem.ActionType.Negative);
		}
	}

	private void MenuCmdAddFriend(CommUser user)
	{
		if (user != null)
		{
			FriendRequestPanelGUI friendRequestPanelGUI = PanelManager.Instance.OpenPanel(PanelType.FriendRequest) as FriendRequestPanelGUI;
			if ((bool)friendRequestPanelGUI)
			{
				friendRequestPanelGUI.SelectReceiver(user.Cmid, user.Name);
			}
		}
	}

	private void MenuCmdChat(CommUser user)
	{
		if (user != null)
		{
			Singleton<ChatManager>.Instance.CreatePrivateChat(user.Cmid);
		}
	}

	private void MenuCmdSendMessage(CommUser user)
	{
		if (user != null)
		{
			SendMessagePanelGUI sendMessagePanelGUI = PanelManager.Instance.OpenPanel(PanelType.SendMessage) as SendMessagePanelGUI;
			if ((bool)sendMessagePanelGUI)
			{
				sendMessagePanelGUI.SelectReceiver(user.Cmid, user.Name);
			}
		}
	}

	private void MenuCmdJoinGame(CommUser user)
	{
		if (user != null && !user.CurrentGame.IsEmpty)
		{
			JoinRoom(user.CurrentGame);
		}
	}

	private void MenuCmdInviteClan(CommUser user)
	{
		if (user != null)
		{
			InviteToClanPanelGUI inviteToClanPanelGUI = PanelManager.Instance.OpenPanel(PanelType.ClanRequest) as InviteToClanPanelGUI;
			if ((bool)inviteToClanPanelGUI)
			{
				inviteToClanPanelGUI.SelectReceiver(user.Cmid, user.ShortName);
			}
		}
	}

	private void MenuCmdModeratePlayer(CommUser user)
	{
		CommActorInfo info;
		if (user != null && CommConnectionManager.CommCenter.TryGetActorWithCmid(Singleton<ChatManager>.Instance._selectedCmid, out info) && info != null)
		{
			ModerationPanelGUI moderationPanelGUI = PanelManager.Instance.OpenPanel(PanelType.Moderation) as ModerationPanelGUI;
			if ((bool)moderationPanelGUI)
			{
				moderationPanelGUI.SetSelectedUser(user);
			}
		}
	}

	private bool MenuChkAddFriend(CommUser user)
	{
		return user != null && user.Cmid != PlayerDataManager.Cmid && user.AccessLevel <= PlayerDataManager.AccessLevel && !PlayerDataManager.IsFriend(user.Cmid) && !PlayerDataManager.IsFacebookFriend(user.Cmid);
	}

	private bool MenuChkRemoveFriend(CommUser user)
	{
		return user != null && user.Cmid != PlayerDataManager.Cmid && PlayerDataManager.IsFriend(user.Cmid);
	}

	private bool MenuChkChat(CommUser user)
	{
		return user != null && user.Cmid != PlayerDataManager.Cmid && user.IsOnline;
	}

	private bool MenuChkSendMessage(CommUser user)
	{
		return user != null && user.Cmid != PlayerDataManager.Cmid && !GameState.HasCurrentGame;
	}

	private bool MenuChkJoinGame(CommUser user)
	{
		return user != null && user.Cmid != PlayerDataManager.Cmid && user.IsOnline && !user.CurrentGame.IsEmpty && user.CurrentGame != CommConnectionManager.CurrentRoomID;
	}

	private bool MenuChkInviteClan(CommUser user)
	{
		return user != null && user.Cmid != PlayerDataManager.Cmid && (user.AccessLevel <= PlayerDataManager.AccessLevel || PlayerDataManager.IsFriend(user.Cmid) || PlayerDataManager.IsFacebookFriend(user.Cmid)) && PlayerDataManager.IsPlayerInClan && PlayerDataManager.CanInviteToClan && !PlayerDataManager.IsClanMember(user.Cmid);
	}
}
