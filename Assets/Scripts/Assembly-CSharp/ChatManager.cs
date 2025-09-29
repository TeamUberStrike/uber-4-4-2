using System.Collections.Generic;
using System.Text;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class ChatManager : Singleton<ChatManager>
{
	private List<CommUser> _otherUsers;

	private List<CommUser> _friendUsers;

	private List<CommUser> _lobbyUsers;

	public Dictionary<int, CommUser> _modUsers;

	private Dictionary<int, CommUser> _clanUsers;

	private List<CommUser> _ingameUsers;

	private List<CommUser> _lastgameUsers;

	private Dictionary<int, CommUser> _allTimePlayers;

	private HashSet<TabArea> _tabAreas;

	private float _nextRefreshTime;

	public int _selectedCmid;

	public ChatDialog _selectedDialog;

	public Property<bool> HasUnreadPrivateMessage = new Property<bool>(false);

	public Property<bool> HasUnreadClanMessage = new Property<bool>(false);

	public ChatGroupPanel[] _commPanes;

	public Dictionary<int, ChatDialog> _dialogsByCmid;

	public ChatDialog ClanDialog { get; private set; }

	public ChatDialog LobbyDialog { get; private set; }

	public ChatDialog InGameDialog { get; private set; }

	public ChatDialog ModerationDialog { get; private set; }

	public ICollection<CommUser> OtherUsers
	{
		get
		{
			return _otherUsers;
		}
	}

	public ICollection<CommUser> FriendUsers
	{
		get
		{
			return _friendUsers;
		}
	}

	public ICollection<CommUser> LobbyUsers
	{
		get
		{
			return _lobbyUsers;
		}
	}

	public ICollection<CommUser> ClanUsers
	{
		get
		{
			return _clanUsers.Values;
		}
	}

	public ICollection<CommUser> NaughtyUsers
	{
		get
		{
			return _modUsers.Values;
		}
	}

	public ICollection<CommUser> GameUsers
	{
		get
		{
			return _ingameUsers;
		}
	}

	public ICollection<CommUser> GameHistoryUsers
	{
		get
		{
			return _lastgameUsers;
		}
	}

	public int TabCounter
	{
		get
		{
			return _tabAreas.Count + (ShowTab(TabArea.InGame) ? 1 : 0) + (ShowTab(TabArea.Clan) ? 1 : 0) + (ShowTab(TabArea.Moderation) ? 1 : 0);
		}
	}

	public static ChatContext CurrentChatContext
	{
		get
		{
			return Singleton<PlayerSpectatorControl>.Instance.IsEnabled ? ChatContext.Spectator : ChatContext.Player;
		}
	}

	private ChatManager()
	{
		_otherUsers = new List<CommUser>();
		_friendUsers = new List<CommUser>();
		_lobbyUsers = new List<CommUser>();
		_clanUsers = new Dictionary<int, CommUser>();
		_modUsers = new Dictionary<int, CommUser>();
		_ingameUsers = new List<CommUser>();
		_lastgameUsers = new List<CommUser>();
		_allTimePlayers = new Dictionary<int, CommUser>();
		_dialogsByCmid = new Dictionary<int, ChatDialog>();
		ClanDialog = new ChatDialog(string.Empty);
		LobbyDialog = new ChatDialog(string.Empty);
		ModerationDialog = new ChatDialog(string.Empty);
		InGameDialog = new ChatDialog(string.Empty);
		InGameDialog.CanShow = CanShowMessage;
		_commPanes = new ChatGroupPanel[5];
		_commPanes[0] = new ChatGroupPanel();
		_commPanes[1] = new ChatGroupPanel();
		_commPanes[2] = new ChatGroupPanel();
		_commPanes[3] = new ChatGroupPanel();
		_commPanes[4] = new ChatGroupPanel();
		_tabAreas = new HashSet<TabArea>
		{
			TabArea.Lobby,
			TabArea.Private
		};
		ClanDialog.Title = LocalizedStrings.ChatInClan;
		LobbyDialog.Title = LocalizedStrings.ChatInLobby;
		ModerationDialog.Title = LocalizedStrings.Moderate;
		_commPanes[0].AddGroup(UserGroups.None, LocalizedStrings.Lobby, LobbyUsers);
		_commPanes[1].AddGroup(UserGroups.Friend, LocalizedStrings.Friends, FriendUsers);
		_commPanes[1].AddGroup(UserGroups.Other, LocalizedStrings.Others, OtherUsers);
		_commPanes[2].AddGroup(UserGroups.None, LocalizedStrings.Clan, ClanUsers);
		_commPanes[3].AddGroup(UserGroups.None, LocalizedStrings.Game, GameUsers);
		_commPanes[3].AddGroup(UserGroups.Other, "History", GameHistoryUsers);
		_commPanes[4].AddGroup(UserGroups.None, "Naughty List", NaughtyUsers);
		CmuneEventHandler.AddListener<LoginEvent>(OnLoginEvent);
	}

	protected override void OnDispose()
	{
		CmuneEventHandler.RemoveListener<LoginEvent>(OnLoginEvent);
	}

	private void OnLoginEvent(LoginEvent ev)
	{
		if (ev.AccessLevel > MemberAccessLevel.Default)
		{
			_tabAreas.Add(TabArea.Moderation);
		}
	}

	public bool ShowTab(TabArea tab)
	{
		switch (tab)
		{
		case TabArea.InGame:
			return GameState.HasCurrentGame || Singleton<ChatManager>.Instance.GameHistoryUsers.Count > 0;
		case TabArea.Clan:
			return PlayerDataManager.IsPlayerInClan;
		case TabArea.Moderation:
			return PlayerDataManager.AccessLevel >= MemberAccessLevel.Moderator;
		default:
			return _tabAreas.Contains(tab);
		}
	}

	public static bool CanShowMessage(ChatContext ctx)
	{
		return true;
	}

	public bool HasDialogWith(int cmid)
	{
		return _dialogsByCmid.ContainsKey(cmid);
	}

	public void UpdateClanSection()
	{
		Singleton<ChatManager>.Instance._clanUsers.Clear();
		foreach (ClanMemberView clanMember in Singleton<PlayerDataManager>.Instance.ClanMembers)
		{
			Singleton<ChatManager>.Instance._clanUsers[clanMember.Cmid] = new CommUser(clanMember);
		}
		RefreshAll(true);
	}

	public void RefreshAll(bool forceRefresh = false)
	{
		if (!forceRefresh && !(_nextRefreshTime < Time.time))
		{
			return;
		}
		_nextRefreshTime = Time.time + 5f;
		_lobbyUsers.Clear();
		foreach (CommActorInfo player in CommConnectionManager.CommCenter.Players)
		{
			if (player.ActorId > 0)
			{
				CommUser commUser = new CommUser(player);
				commUser.IsClanMember = PlayerDataManager.IsClanMember(player.Cmid);
				commUser.IsFriend = PlayerDataManager.IsFriend(player.Cmid);
				commUser.IsFacebookFriend = PlayerDataManager.IsFacebookFriend(player.Cmid);
				_lobbyUsers.Add(commUser);
			}
		}
		_lobbyUsers.Sort(new CommUserNameComparer());
		_lobbyUsers.Sort(new CommUserFriendsComparer());
		CommActorInfo info;
		foreach (CommUser lastgameUser in Singleton<ChatManager>.Instance._lastgameUsers)
		{
			lastgameUser.IsClanMember = PlayerDataManager.IsClanMember(lastgameUser.Cmid);
			lastgameUser.IsFriend = PlayerDataManager.IsFriend(lastgameUser.Cmid);
			lastgameUser.IsFacebookFriend = PlayerDataManager.IsFacebookFriend(lastgameUser.Cmid);
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(lastgameUser.Cmid, out info))
			{
				lastgameUser.SetActor(info);
			}
			else
			{
				lastgameUser.SetActor(null);
			}
		}
		Singleton<ChatManager>.Instance._lastgameUsers.Sort(new CommUserPresenceComparer());
		foreach (CommUser friendUser in Singleton<ChatManager>.Instance._friendUsers)
		{
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(friendUser.Cmid, out info))
			{
				friendUser.SetActor(info);
			}
			else
			{
				friendUser.SetActor(null);
			}
		}
		Singleton<ChatManager>.Instance._friendUsers.Sort(new CommUserPresenceComparer());
		foreach (CommUser value in Singleton<ChatManager>.Instance._clanUsers.Values)
		{
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(value.Cmid, out info))
			{
				value.SetActor(info);
			}
			else
			{
				value.SetActor(null);
			}
		}
		foreach (CommUser otherUser in Singleton<ChatManager>.Instance._otherUsers)
		{
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(otherUser.Cmid, out info))
			{
				otherUser.SetActor(info);
			}
			else
			{
				otherUser.SetActor(null);
			}
		}
		Singleton<ChatManager>.Instance._otherUsers.Sort(new CommUserNameComparer());
		foreach (KeyValuePair<int, CommUser> modUser in Singleton<ChatManager>.Instance._modUsers)
		{
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(modUser.Key, out info))
			{
				modUser.Value.SetActor(info);
			}
			else
			{
				modUser.Value.SetActor(null);
			}
		}
	}

	public void UpdateFriendSection()
	{
		List<CommUser> list = new List<CommUser>(Singleton<ChatManager>.Instance._friendUsers);
		Singleton<ChatManager>.Instance._friendUsers.Clear();
		foreach (PublicProfileView friend in Singleton<PlayerDataManager>.Instance.FriendList)
		{
			Singleton<ChatManager>.Instance._friendUsers.Add(new CommUser(friend));
		}
		foreach (PublicProfileView facebookFriend in Singleton<PlayerDataManager>.Instance.FacebookFriends)
		{
			Singleton<ChatManager>.Instance._friendUsers.Add(new CommUser(facebookFriend));
		}
		CommUser f;
		foreach (CommUser friendUser in Singleton<ChatManager>.Instance._friendUsers)
		{
			f = friendUser;
			ChatDialog value;
			if (Singleton<ChatManager>.Instance._otherUsers.RemoveAll((CommUser u) => u.Cmid == f.Cmid) > 0 && Singleton<ChatManager>.Instance._dialogsByCmid.TryGetValue(f.Cmid, out value))
			{
				value.Group = UserGroups.Friend;
			}
		}
		CommUser f2;
		foreach (CommUser item in list)
		{
			f2 = item;
			ChatDialog value2;
			if (Singleton<ChatManager>.Instance._dialogsByCmid.TryGetValue(f2.Cmid, out value2) && !Singleton<ChatManager>.Instance._friendUsers.Exists((CommUser u) => u.Cmid == f2.Cmid) && !Singleton<ChatManager>.Instance._otherUsers.Exists((CommUser u) => u.Cmid == f2.Cmid))
			{
				Singleton<ChatManager>.Instance._otherUsers.Add(f2);
				value2.Group = UserGroups.Other;
			}
		}
		Singleton<ChatManager>.Instance.RefreshAll();
	}

	public static Texture GetPresenceIcon(CommActorInfo user)
	{
		if (user != null)
		{
			return GetPresenceIcon((!user.IsInGame) ? PresenceType.Online : PresenceType.InGame);
		}
		return GetPresenceIcon(PresenceType.Offline);
	}

	public static Texture GetPresenceIcon(PresenceType index)
	{
		switch (index)
		{
		case PresenceType.InGame:
			return CommunicatorIcons.PresencePlaying;
		case PresenceType.Online:
			return CommunicatorIcons.PresenceOnline;
		case PresenceType.Offline:
			return CommunicatorIcons.PresenceOffline;
		default:
			return CommunicatorIcons.PresenceOffline;
		}
	}

	public void SetGameSection(CmuneRoomID roomId, IEnumerable<UberStrike.Realtime.UnitySdk.CharacterInfo> actors)
	{
		_ingameUsers.Clear();
		_lastgameUsers.Clear();
		_lastgameUsers.AddRange(_allTimePlayers.Values);
		UberStrike.Realtime.UnitySdk.CharacterInfo v;
		foreach (UberStrike.Realtime.UnitySdk.CharacterInfo actor in actors)
		{
			v = actor;
			CommUser commUser = new CommUser(v);
			commUser.CurrentGame = roomId;
			commUser.IsClanMember = PlayerDataManager.IsClanMember(commUser.Cmid);
			commUser.IsFriend = PlayerDataManager.IsFriend(commUser.Cmid);
			commUser.IsFacebookFriend = PlayerDataManager.IsFacebookFriend(commUser.Cmid);
			_ingameUsers.Add(commUser);
			_lastgameUsers.RemoveAll((CommUser p) => p.Cmid == v.Cmid);
			if (v.Cmid != PlayerDataManager.Cmid && !_allTimePlayers.ContainsKey(v.Cmid))
			{
				commUser = new CommUser(v);
				commUser.CurrentGame = roomId;
				_allTimePlayers[v.Cmid] = commUser;
			}
		}
		_ingameUsers.Sort(new CommUserNameComparer());
	}

	public List<CommUser> GetCommUsersToReport()
	{
		int capacity = _ingameUsers.Count + _lobbyUsers.Count + _otherUsers.Count;
		Dictionary<int, CommUser> dictionary = new Dictionary<int, CommUser>(capacity);
		foreach (CommUser ingameUser in _ingameUsers)
		{
			dictionary[ingameUser.Cmid] = ingameUser;
		}
		foreach (CommUser otherUser in _otherUsers)
		{
			dictionary[otherUser.Cmid] = otherUser;
		}
		foreach (CommUser lobbyUser in _lobbyUsers)
		{
			dictionary[lobbyUser.Cmid] = lobbyUser;
		}
		return new List<CommUser>(dictionary.Values);
	}

	public bool TryGetClanUsers(int cmid, out CommUser user)
	{
		return _clanUsers.TryGetValue(cmid, out user) && user != null;
	}

	public bool TryGetLobbyCommUser(int cmid, out CommUser user)
	{
		user = null;
		foreach (CommUser lobbyUser in _lobbyUsers)
		{
			if (lobbyUser.Cmid == cmid)
			{
				user = lobbyUser;
				return true;
			}
		}
		return false;
	}

	public bool TryGetFriend(int cmid, out CommUser user)
	{
		foreach (CommUser friendUser in _friendUsers)
		{
			if (friendUser.Cmid == cmid)
			{
				user = friendUser;
				return true;
			}
		}
		user = null;
		return false;
	}

	public void CreatePrivateChat(int cmid)
	{
		ChatDialog chatDialog = null;
		ChatDialog value;
		if (_dialogsByCmid.TryGetValue(cmid, out value) && value != null)
		{
			chatDialog = value;
		}
		else
		{
			CommUser commUser = null;
			CommActorInfo info = null;
			if (PlayerDataManager.IsFriend(cmid) || PlayerDataManager.IsFacebookFriend(cmid))
			{
				commUser = _friendUsers.Find((CommUser u) => u.Cmid == cmid);
				if (commUser != null && commUser.PresenceIndex != PresenceType.Offline)
				{
					chatDialog = new ChatDialog(commUser, UserGroups.Friend);
				}
			}
			else if (CommConnectionManager.CommCenter.TryGetActorWithCmid(cmid, out info))
			{
				ClanMemberView view;
				if (PlayerDataManager.TryGetClanMember(cmid, out view))
				{
					commUser = new CommUser(view);
					commUser.SetActor(info);
				}
				else
				{
					commUser = new CommUser(info);
				}
				_otherUsers.Add(commUser);
				chatDialog = new ChatDialog(commUser, UserGroups.Other);
			}
			if (chatDialog != null)
			{
				_dialogsByCmid.Add(cmid, chatDialog);
			}
		}
		if (chatDialog != null)
		{
			ChatPageGUI.SelectedTab = TabArea.Private;
			_selectedDialog = chatDialog;
			_selectedCmid = cmid;
		}
		else
		{
			Debug.LogError(string.Format("Player with cmuneID {0} not found in communicator!", cmid));
		}
	}

	public string GetAllChatMessagesForPlayerReport()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ICollection<InstantMessage> allMessages = Singleton<ChatManager>.Instance.InGameDialog.AllMessages;
		if (allMessages.Count > 0)
		{
			stringBuilder.AppendLine("In Game Chat:");
			foreach (InstantMessage item in allMessages)
			{
				stringBuilder.AppendLine(item.PlayerName + " : " + item.MessageText);
			}
			stringBuilder.AppendLine();
		}
		foreach (ChatDialog value in Singleton<ChatManager>.Instance._dialogsByCmid.Values)
		{
			allMessages = value.AllMessages;
			if (allMessages.Count <= 0)
			{
				continue;
			}
			stringBuilder.AppendLine("Private Chat:");
			foreach (InstantMessage item2 in allMessages)
			{
				stringBuilder.AppendLine(item2.PlayerName + " : " + item2.MessageText);
			}
			stringBuilder.AppendLine();
		}
		allMessages = Singleton<ChatManager>.Instance.ClanDialog.AllMessages;
		if (allMessages.Count > 0)
		{
			stringBuilder.AppendLine("Clan Chat:");
			foreach (InstantMessage item3 in allMessages)
			{
				stringBuilder.AppendLine(item3.PlayerName + " : " + item3.MessageText);
			}
			stringBuilder.AppendLine();
		}
		allMessages = Singleton<ChatManager>.Instance.LobbyDialog.AllMessages;
		if (allMessages.Count > 0)
		{
			stringBuilder.AppendLine("Lobby Chat:");
			foreach (InstantMessage item4 in allMessages)
			{
				stringBuilder.AppendLine(item4.PlayerName + " : " + item4.MessageText);
			}
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	public void UpdateLastgamePlayers()
	{
		Singleton<ChatManager>.Instance._lastgameUsers.Clear();
		foreach (CommUser value in Singleton<ChatManager>.Instance._allTimePlayers.Values)
		{
			value.IsInGame = false;
			value.IsClanMember = PlayerDataManager.IsClanMember(value.Cmid);
			value.IsFriend = PlayerDataManager.IsFriend(value.Cmid);
			value.IsFacebookFriend = PlayerDataManager.IsFacebookFriend(value.Cmid);
			CommActorInfo info;
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(value.Cmid, out info))
			{
				value.SetActor(info);
			}
			else
			{
				value.SetActor(null);
			}
			Singleton<ChatManager>.Instance._lastgameUsers.Add(value);
		}
		Singleton<ChatManager>.Instance._lastgameUsers.Sort(new CommUserPresenceComparer());
	}

	public void SetNaughtyList(List<CommActorInfo> hackers)
	{
		foreach (CommActorInfo hacker in hackers)
		{
			_modUsers[hacker.Cmid] = new CommUser(hacker);
		}
	}

	public void AddClanMessage(int cmid, InstantMessage msg)
	{
		ClanDialog.AddMessage(msg);
		if (cmid != PlayerDataManager.Cmid && ChatPageGUI.SelectedTab != TabArea.Clan)
		{
			HasUnreadClanMessage.Value = true;
			SfxManager.Play2dAudioClip(GameAudio.NewMessage);
		}
	}

	public void AddNewPrivateMessage(int cmid, InstantMessage msg)
	{
		try
		{
			ChatDialog value;
			if (!_dialogsByCmid.TryGetValue(cmid, out value))
			{
				CommActorInfo info;
				if (CommConnectionManager.CommCenter.TryGetActorWithCmid(cmid, out info))
				{
					CommUser commUser = new CommUser(info);
					value = AddNewDialog(commUser);
					if (!_friendUsers.Exists((CommUser p) => p.Cmid == cmid))
					{
						_otherUsers.Add(commUser);
					}
				}
				else
				{
					CommActorInfo commActorInfo = new CommActorInfo(msg.PlayerName, 0, ChannelType.WebPortal);
					commActorInfo.Cmid = cmid;
					commActorInfo.AccessLevel = (int)msg.AccessLevel;
					CommUser commUser2 = new CommUser(commActorInfo);
					value = AddNewDialog(commUser2);
					if (!_friendUsers.Exists((CommUser p) => p.Cmid == cmid))
					{
						_otherUsers.Add(commUser2);
						CommConnectionManager.CommCenter.SendContactList();
					}
				}
			}
			if (value != null)
			{
				value.AddMessage(msg);
				if (ChatPageGUI.SelectedTab != TabArea.Private || value != _selectedDialog)
				{
					value.HasUnreadMessage = true;
				}
			}
			if (msg.Cmid != PlayerDataManager.Cmid)
			{
				HasUnreadPrivateMessage.Value = true;
				if (ChatPageGUI.SelectedTab != TabArea.Private)
				{
					SfxManager.Play2dAudioClip(GameAudio.NewMessage);
				}
			}
		}
		catch
		{
			Debug.LogError(string.Format("AddNewPrivateMessage from cmid={0}", cmid));
			throw;
		}
	}

	public ChatDialog AddNewDialog(CommUser user)
	{
		ChatDialog value = null;
		if (user != null && !_dialogsByCmid.TryGetValue(user.Cmid, out value))
		{
			value = ((!PlayerDataManager.IsFriend(user.Cmid) && !PlayerDataManager.IsFacebookFriend(user.Cmid)) ? new ChatDialog(user, UserGroups.Other) : new ChatDialog(user, UserGroups.Friend));
			_dialogsByCmid.Add(user.Cmid, value);
		}
		return value;
	}

	internal void RemoveDialog(ChatDialog d)
	{
		_dialogsByCmid.Remove(d.UserCmid);
		_otherUsers.RemoveAll((CommUser u) => u.Cmid == d.UserCmid);
		_selectedDialog = null;
	}
}
