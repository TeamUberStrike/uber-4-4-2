using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[NetworkClass(4)]
public class ClientCommCenter : ClientNetworkClass
{
	private class ActorList
	{
		private Dictionary<int, CommActorInfo> _actorsByCmid = new Dictionary<int, CommActorInfo>();

		private Dictionary<int, int> _cmidByActorId = new Dictionary<int, int>();

		public IEnumerable<CommActorInfo> Actors
		{
			get
			{
				return _actorsByCmid.Values;
			}
		}

		public int ActorCount
		{
			get
			{
				return _actorsByCmid.Count;
			}
		}

		public void Add(CommActorInfo actor)
		{
			if (actor != null && actor.Cmid > 0 && actor.ActorId > 0)
			{
				if (_cmidByActorId.ContainsKey(actor.ActorId))
				{
					_actorsByCmid.Remove(_cmidByActorId[actor.ActorId]);
					_cmidByActorId.Remove(actor.ActorId);
				}
				if (_actorsByCmid.ContainsKey(actor.Cmid))
				{
					_cmidByActorId.Remove(_actorsByCmid[actor.Cmid].ActorId);
					_actorsByCmid.Remove(actor.Cmid);
				}
				_actorsByCmid.Add(actor.Cmid, actor);
				_cmidByActorId.Add(actor.ActorId, actor.Cmid);
			}
		}

		public void Remove(CommActorInfo actor)
		{
			_actorsByCmid.Remove(actor.Cmid);
			_cmidByActorId.Remove(actor.ActorId);
		}

		public bool TryGetByActorId(int actorId, out CommActorInfo actor)
		{
			actor = null;
			int value;
			return _cmidByActorId.TryGetValue(actorId, out value) && TryGetByCmid(value, out actor);
		}

		public bool TryGetByCmid(int cmid, out CommActorInfo actor)
		{
			actor = null;
			return cmid > 0 && _actorsByCmid.TryGetValue(cmid, out actor) && actor != null;
		}

		public void Clear()
		{
			_actorsByCmid.Clear();
			_cmidByActorId.Clear();
		}
	}

	private class Message
	{
		public float Time;

		public string Text;

		public int Count;

		public Message(string text)
		{
			Text = text;
			Time = UnityEngine.Time.time;
		}
	}

	private int _totalContactsCount;

	private LimitedQueue<Message> _lastMessages = new LimitedQueue<Message>(5);

	private CommActorInfo _myInfo;

	private ActorList _actors;

	private static bool _isPlayerMuted;

	public IEnumerable<CommActorInfo> Players
	{
		get
		{
			return _actors.Actors;
		}
	}

	public int PlayerCount
	{
		get
		{
			return _actors.ActorCount;
		}
	}

	public CommActorInfo MyInfo
	{
		get
		{
			return _myInfo;
		}
	}

	public static bool IsPlayerMuted
	{
		get
		{
			return _isPlayerMuted;
		}
		private set
		{
			_isPlayerMuted = value;
		}
	}

	public ClientCommCenter(RemoteMethodInterface rmi)
		: base(rmi)
	{
		_actors = new ActorList();
		_myInfo = new CommActorInfo(string.Empty, 0, ChannelType.WebPortal);
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		JoinCommServer();
	}

	protected override void OnUninitialized()
	{
		Debug.Log("OnUninitialized ClientCommMode");
		MyInfo.ActorId = -1;
		MyInfo.CurrentRoom = CmuneRoomID.Empty;
		_actors.Clear();
		Singleton<ChatManager>.Instance.RefreshAll();
		base.OnUninitialized();
	}

	public void UpdatePlayerRoom(CmuneRoomID room)
	{
		SendMethodToServer(21, _rmi.Messenger.PeerListener.ActorIdSecure, room);
	}

	public void ResetPlayerRoom()
	{
		SendMethodToServer(22, _rmi.Messenger.PeerListener.ActorIdSecure);
	}

	public void Login()
	{
		JoinCommServer();
	}

	private void UpdateMyInfo()
	{
		MyInfo.PlayerName = ((!string.IsNullOrEmpty(PlayerDataManager.ClanTag)) ? string.Format("[{0}] {1}", PlayerDataManager.ClanTag, PlayerDataManager.NameSecure) : PlayerDataManager.NameSecure);
		MyInfo.ClanTag = ((!string.IsNullOrEmpty(PlayerDataManager.ClanTag)) ? string.Empty : PlayerDataManager.ClanTag);
		MyInfo.Cmid = PlayerDataManager.CmidSecure;
		MyInfo.AccessLevel = (int)PlayerDataManager.AccessLevelSecure;
	}

	private void JoinCommServer()
	{
		if (IsInitialized && PlayerDataManager.IsPlayerLoggedIn)
		{
			UpdateMyInfo();
			MyInfo.ActorId = _rmi.Messenger.PeerListener.ActorIdSecure;
			MyInfo.CurrentRoom = _rmi.Messenger.PeerListener.CurrentRoom;
			MyInfo.Channel = ApplicationDataManager.Channel;
			SendMethodToServer(1, MyInfo);
			SendContactList();
			if (GameConnectionManager.Client.PeerListener.HasJoinedRoom)
			{
				UpdatePlayerRoom(GameConnectionManager.Client.PeerListener.CurrentRoom);
			}
		}
	}

	public void SendUpdatedActorInfo()
	{
		UpdateMyInfo();
		if ((bool)GameState.LocalAvatar.Decorator)
		{
			GameState.LocalAvatar.Decorator.HudInformation.SetAvatarLabel(PlayerDataManager.NameAndTag);
		}
		_rmi.Messenger.SendOperationToServerApplication(delegate
		{
		}, 5);
	}

	[NetworkMethod(2)]
	protected void OnPlayerLeft(int cmid)
	{
		CommActorInfo info;
		if (TryGetActorWithCmid(cmid, out info))
		{
			OnPlayerLeft(info, true);
		}
	}

	[NetworkMethod(51)]
	protected void OnPlayerHide(int cmid)
	{
		CommActorInfo info;
		if (TryGetActorWithCmid(cmid, out info) && !PlayerDataManager.IsClanMember(cmid) && !PlayerDataManager.IsFriend(cmid) && !PlayerDataManager.IsFacebookFriend(cmid) && !Singleton<ChatManager>.Instance.HasDialogWith(cmid))
		{
			OnPlayerLeft(info, true);
		}
	}

	protected void OnPlayerLeft(CommActorInfo user, bool refreshComm)
	{
		_actors.Remove(user);
		user.CurrentRoom = CmuneRoomID.Empty;
		user.ActorId = -1;
		Singleton<ChatManager>.Instance.RefreshAll(refreshComm);
	}

	[NetworkMethod(3)]
	protected void OnPlayerUpdate(SyncObject data)
	{
		bool forceRefresh = false;
		CommActorInfo actor;
		if (_actors.TryGetByActorId(data.Id, out actor))
		{
			actor.ReadSyncData(data);
			forceRefresh = actor.Cmid == PlayerDataManager.Cmid;
		}
		else if (!data.IsEmpty)
		{
			actor = new CommActorInfo(data);
			_actors.Add(actor);
		}
		Singleton<ChatManager>.Instance.RefreshAll(forceRefresh);
		GameData.Instance.Players.Value = new List<CommActorInfo>(_actors.Actors);
	}

	[NetworkMethod(44)]
	protected void OnUpdateContacts(List<SyncObject> updated, List<int> removed)
	{
		foreach (SyncObject item in updated)
		{
			OnPlayerJoined(item);
		}
		foreach (int item2 in removed)
		{
			CommActorInfo actor;
			if (_actors.TryGetByCmid(item2, out actor))
			{
				OnPlayerLeft(actor, false);
			}
		}
		Singleton<ChatManager>.Instance.RefreshAll(true);
		GameData.Instance.Players.Value = new List<CommActorInfo>(_actors.Actors);
	}

	[NetworkMethod(4)]
	protected void OnFullPlayerListUpdate(List<SyncObject> players)
	{
		_actors.Clear();
		foreach (SyncObject player in players)
		{
			if (player.Id == _rmi.Messenger.PeerListener.ActorId)
			{
				MyInfo.ReadSyncData(player);
				_actors.Add(MyInfo);
			}
			else
			{
				_actors.Add(new CommActorInfo(player));
			}
		}
		Singleton<ChatManager>.Instance.RefreshAll();
		GameData.Instance.Players.Value = new List<CommActorInfo>(_actors.Actors);
	}

	private void OnPlayerJoined(SyncObject data)
	{
		CommActorInfo actor;
		if (_actors.TryGetByActorId(data.Id, out actor))
		{
			actor.ReadSyncData(data);
			return;
		}
		if (data.Id == _rmi.Messenger.PeerListener.ActorIdSecure)
		{
			MyInfo.ReadSyncData(data);
			actor = MyInfo;
		}
		else
		{
			actor = new CommActorInfo(data);
		}
		_actors.Add(actor);
	}

	[NetworkMethod(42)]
	public void OnClanChatMessage(int cmid, int actorId, string name, string message)
	{
		InstantMessage msg = new InstantMessage(cmid, actorId, name, message, MemberAccessLevel.Default);
		Singleton<ChatManager>.Instance.AddClanMessage(cmid, msg);
		PlayerChatLog.AddIncomingMessage(cmid, name, message, PlayerChatLog.ChatContextType.Clan);
	}

	[NetworkMethod(23)]
	protected void OnInGameChatMessage(int cmid, int actorID, string name, string message, byte accessLevel, byte context)
	{
		if (ChatManager.CanShowMessage((ChatContext)context))
		{
			Singleton<InGameChatHud>.Instance.AddChatMessage(name, message, (MemberAccessLevel)accessLevel);
		}
		Singleton<ChatManager>.Instance.InGameDialog.AddMessage(new InstantMessage(cmid, actorID, name, message, (MemberAccessLevel)accessLevel, (ChatContext)context));
		PlayerChatLog.AddIncomingMessage(cmid, name, message, PlayerChatLog.ChatContextType.Game);
	}

	[NetworkMethod(25)]
	protected void OnLobbyChatMessage(int cmid, int actorID, string name, string message)
	{
		MemberAccessLevel level = MemberAccessLevel.Default;
		CommActorInfo actor;
		if (_actors.TryGetByCmid(cmid, out actor))
		{
			level = (MemberAccessLevel)actor.AccessLevel;
		}
		InstantMessage msg = new InstantMessage(cmid, actorID, name, message, level);
		Singleton<ChatManager>.Instance.LobbyDialog.AddMessage(msg);
		PlayerChatLog.AddIncomingMessage(cmid, name, message, PlayerChatLog.ChatContextType.Lobby);
	}

	[NetworkMethod(24)]
	protected void OnPrivateChatMessage(int cmid, int actorID, string name, string message)
	{
		MemberAccessLevel level = MemberAccessLevel.Default;
		CommActorInfo actor;
		if (_actors.TryGetByCmid(cmid, out actor))
		{
			level = (MemberAccessLevel)actor.AccessLevel;
		}
		InstantMessage msg = new InstantMessage(cmid, actorID, name, message, level);
		Singleton<ChatManager>.Instance.AddNewPrivateMessage(cmid, msg);
		PlayerChatLog.AddIncomingMessage(cmid, name, message, PlayerChatLog.ChatContextType.Private);
	}

	[NetworkMethod(35)]
	protected void OnGameInviteMessage(int actorId, string message, CmuneRoomID roomId)
	{
	}

	[NetworkMethod(36)]
	public void OnDisconnectAndDisablePhoton(string message)
	{
		if (PhotonClient.IsPhotonEnabled)
		{
			if (GameState.HasCurrentGame)
			{
				Singleton<GameStateController>.Instance.LeaveGame();
			}
			PhotonClient.IsPhotonEnabled = false;
			LobbyConnectionManager.Stop();
			CommConnectionManager.Stop();
			ApplicationDataManager.LockApplication(message);
		}
	}

	[NetworkMethod(26)]
	protected virtual void OnUpdateIngameGroup(List<int> actorIDs)
	{
	}

	[NetworkMethod(37)]
	protected virtual void OnUpdateInboxRequests()
	{
		Singleton<InboxManager>.Instance.RefreshAllRequests();
	}

	[NetworkMethod(38)]
	protected virtual void OnUpdateFriendsList()
	{
		MonoRoutine.Start(Singleton<CommsManager>.Instance.GetContactsByGroups());
	}

	[NetworkMethod(39)]
	protected void OnUpdateInboxMessages(int messageId)
	{
		Singleton<InboxManager>.Instance.GetMessageWithId(messageId);
	}

	[NetworkMethod(40)]
	protected void OnUpdateClanMembers()
	{
		Singleton<ClanDataManager>.Instance.RefreshClanData(true);
	}

	[NetworkMethod(53)]
	protected void OnUpdateClanData()
	{
		Singleton<ClanDataManager>.Instance.CheckCompleteClanData();
	}

	[NetworkMethod(46)]
	public void OnUpdateActorsForModeration(List<SyncObject> allHackers)
	{
		List<CommActorInfo> list = new List<CommActorInfo>(allHackers.Count);
		foreach (SyncObject allHacker in allHackers)
		{
			CommActorInfo actor;
			if (_actors.TryGetByActorId(allHacker.Id, out actor) && actor != null)
			{
				actor.ReadSyncData(allHacker);
			}
			else
			{
				actor = new CommActorInfo(allHacker);
			}
			list.Add(actor);
		}
		Singleton<ChatManager>.Instance.SetNaughtyList(list);
		SendContactList();
	}

	[NetworkMethod(31)]
	public void OnModerationCustomMessage(string message)
	{
		if (GameState.HasCurrentGame && !GameState.LocalPlayer.IsGamePaused)
		{
			GameState.LocalPlayer.Pause();
		}
		PopupSystem.ShowMessage("Administrator Message", message, PopupSystem.AlertType.OK, delegate
		{
		});
	}

	[NetworkMethod(30)]
	public void OnModerationMutePlayer(bool isPlayerMuted)
	{
		IsPlayerMuted = isPlayerMuted;
		if (isPlayerMuted)
		{
			PopupSystem.ShowMessage("ADMIN MESSAGE", "You have been muted!", PopupSystem.AlertType.OK, delegate
			{
			});
		}
	}

	[NetworkMethod(32)]
	public void OnModerationKickGame()
	{
		Singleton<GameStateController>.Instance.LeaveGame();
		PopupSystem.ShowMessage("ADMIN MESSAGE", "You were kicked out of the game!", PopupSystem.AlertType.OK, delegate
		{
		});
	}

	public void SendModerationBanPlayer(int cmid)
	{
		CommConnectionManager.CommCenter.SendMethodToServer(33, PlayerDataManager.CmidSecure, cmid);
	}

	public void SendModerationUnbanPlayer(int cmid)
	{
		CommConnectionManager.CommCenter.SendMethodToServer(34, cmid);
	}

	public void SendKickFromGame(int actorId)
	{
		CommConnectionManager.CommCenter.SendMethodToPlayer(actorId, 32);
		CommConnectionManager.CommCenter.SendMethodToServer(32, actorId, PlayerDataManager.CmidSecure);
	}

	public void SendMutePlayer(int cmid, int actorId, int minutes)
	{
		CommConnectionManager.CommCenter.SendMethodToServer(30, cmid, minutes, actorId, true);
	}

	public void SendGhostPlayer(int cmid, int actorId, int minutes)
	{
		CommConnectionManager.CommCenter.SendMethodToServer(30, cmid, minutes, actorId, false);
	}

	public void SendUnmutePlayer(int actorId)
	{
		CommConnectionManager.CommCenter.SendMethodToPlayer(actorId, 30, false);
	}

	public void SendUpdateClanMembers(IEnumerable<ClanMemberView> list)
	{
		List<int> list2 = new List<int>();
		foreach (ClanMemberView item in list)
		{
			list2.Add(item.Cmid);
		}
		list2.RemoveAll((int id) => id == PlayerDataManager.CmidSecure);
		CommConnectionManager.CommCenter.SendMethodToServer(40, list2);
	}

	public void SendRefreshClanData(int cmid)
	{
		CommConnectionManager.CommCenter.SendMethodToServer(53, cmid);
	}

	public void SendContactList()
	{
		HashSet<int> hashSet = new HashSet<int>();
		if (Singleton<PlayerDataManager>.Instance.FriendList != null)
		{
			foreach (PublicProfileView friend in Singleton<PlayerDataManager>.Instance.FriendList)
			{
				hashSet.Add(friend.Cmid);
			}
		}
		if (Singleton<PlayerDataManager>.Instance.ClanMembers != null)
		{
			foreach (ClanMemberView clanMember in Singleton<PlayerDataManager>.Instance.ClanMembers)
			{
				hashSet.Add(clanMember.Cmid);
			}
		}
		if (Singleton<PlayerDataManager>.Instance.FacebookFriends != null)
		{
			foreach (PublicProfileView facebookFriend in Singleton<PlayerDataManager>.Instance.FacebookFriends)
			{
				hashSet.Add(facebookFriend.Cmid);
			}
		}
		foreach (CommUser value in Singleton<ChatManager>.Instance._modUsers.Values)
		{
			hashSet.Add(value.Cmid);
		}
		foreach (CommUser otherUser in Singleton<ChatManager>.Instance.OtherUsers)
		{
			hashSet.Add(otherUser.Cmid);
		}
		_totalContactsCount = hashSet.Count;
		if (_totalContactsCount > 0)
		{
			SendMethodToServer(43, PlayerDataManager.CmidSecure, hashSet);
		}
	}

	public void UpdateContacts()
	{
		if (_totalContactsCount > 0)
		{
			SendMethodToServer(44, PlayerDataManager.CmidSecure);
		}
	}

	public void SendUpdateResetLobby()
	{
		_actors.Clear();
		_actors.Add(MyInfo);
		Singleton<ChatManager>.Instance.RefreshAll();
		SendMethodToServer(4, _rmi.Messenger.PeerListener.ActorIdSecure);
	}

	public void SendUpdateAllPlayers()
	{
		SendMethodToServer(47, _rmi.Messenger.PeerListener.ActorIdSecure);
	}

	public void SendSpeedhackDetection(List<float> timeDifference)
	{
		SendMethodToServer(50, PlayerDataManager.CmidSecure, timeDifference);
	}

	public void UpdateActorsForModeration()
	{
		SendMethodToServer(46, _rmi.Messenger.PeerListener.ActorIdSecure);
	}

	public void SendClanChatMessage(int cmid, int playerId, string name, string message)
	{
		message = CleanupChatMessage(message);
		if (!string.IsNullOrEmpty(message))
		{
			SendMethodToPlayer(playerId, 42, cmid, playerId, name, message);
		}
	}

	public bool SendLobbyChatMessage(string message)
	{
		bool flag = CheckSpamFilter(message);
		message = CleanupChatMessage(message);
		if (flag && !string.IsNullOrEmpty(message))
		{
			OnLobbyChatMessage(PlayerDataManager.CmidSecure, _rmi.Messenger.PeerListener.ActorIdSecure, MyInfo.PlayerName, message);
			SendMethodToServer(25, _rmi.Messenger.PeerListener.ActorIdSecure, message);
			return true;
		}
		return false;
	}

	public bool SendInGameChatMessage(string message, ChatContext context)
	{
		bool flag = CheckSpamFilter(message);
		message = CleanupChatMessage(message);
		if (flag && !string.IsNullOrEmpty(message))
		{
			OnInGameChatMessage(PlayerDataManager.CmidSecure, _rmi.Messenger.PeerListener.ActorIdSecure, PlayerDataManager.NameSecure, message, (byte)PlayerDataManager.AccessLevelSecure, (byte)ChatManager.CurrentChatContext);
			SendMethodToServer(23, _rmi.Messenger.PeerListener.ActorIdSecure, message, (byte)ChatManager.CurrentChatContext);
		}
		return flag;
	}

	public void SendPrivateChatMessage(CommActorInfo info, string message)
	{
		message = CleanupChatMessage(message);
		if (!string.IsNullOrEmpty(message))
		{
			int cmidSecure = PlayerDataManager.CmidSecure;
			InstantMessage msg = new InstantMessage(cmidSecure, _rmi.Messenger.PeerListener.ActorIdSecure, PlayerDataManager.NameSecure, message, PlayerDataManager.AccessLevelSecure);
			Singleton<ChatManager>.Instance.AddNewPrivateMessage(info.Cmid, msg);
			SendMethodToServer(24, _rmi.Messenger.PeerListener.ActorIdSecure, info.ActorId, message);
			PlayerChatLog.AddOutgoingPrivateMessage(cmidSecure, info.PlayerName, message, PlayerChatLog.ChatContextType.Private);
		}
	}

	private bool CheckSpamFilter(string message)
	{
		bool flag = false;
		bool flag2 = false;
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		string value = string.Empty;
		foreach (Message lastMessage in _lastMessages)
		{
			if (lastMessage.Time + 5f > Time.time)
			{
				if (message.StartsWith(lastMessage.Text, StringComparison.InvariantCultureIgnoreCase))
				{
					lastMessage.Time = Time.time;
					lastMessage.Count++;
					flag = lastMessage.Count > 1;
					flag2 = true;
				}
				if (num2 != 0f)
				{
					num += Mathf.Clamp(1f - (lastMessage.Time - num2), 0f, 1f);
					num3++;
				}
			}
			num2 = lastMessage.Time;
			value = lastMessage.Text;
		}
		if (!flag2)
		{
			_lastMessages.Enqueue(new Message(message));
		}
		if (message.Equals(value, StringComparison.InvariantCultureIgnoreCase) && num2 + 10f > Time.time)
		{
			flag = true;
		}
		if (num3 > 0)
		{
			num /= (float)num3;
		}
		flag = flag || num > 0.3f;
		return !flag;
	}

	private string CleanupChatMessage(string msg)
	{
		return TextUtilities.ShortenText(TextUtilities.Trim(msg), 140, false);
	}

	public void SendPrivateGameInvitation(CommActorInfo info, string message, CmuneRoomID roomId)
	{
		SendMethodToPlayer(info.ActorId, 35, _rmi.Messenger.PeerListener.ActorIdSecure, message, roomId);
	}

	public void SendPlayerReport(int[] players, MemberReportType type, string details)
	{
		string allChatMessagesForPlayerReport = Singleton<ChatManager>.Instance.GetAllChatMessagesForPlayerReport();
		SendMethodToServer(28, PlayerDataManager.CmidSecure, players, (int)type, details, allChatMessagesForPlayerReport);
	}

	public void SendClearAllFlags(int cmid)
	{
		SendMethodToServer(48, cmid);
	}

	public bool TryGetActorWithCmid(int cmid, out CommActorInfo info)
	{
		return _actors.TryGetByCmid(cmid, out info);
	}

	public bool HasActorWithCmid(int cmid)
	{
		CommActorInfo actor;
		return _actors.TryGetByCmid(cmid, out actor) && actor != null;
	}

	public void UpdateInboxRequest(int actorId)
	{
		SendMethodToPlayer(actorId, 37);
	}

	public void NotifyFriendUpdate(int cmid)
	{
		SendMethodToServer(38, cmid);
	}

	public void MessageSentWithId(int messageId, int cmid)
	{
		SendMethodToServer(39, cmid, messageId);
	}

	public void SendGetPlayersWithMatchingName(string str)
	{
		_actors.Clear();
		Singleton<ChatManager>.Instance.RefreshAll();
		SendMethodToServer(52, PlayerDataManager.CmidSecure, str);
	}
}
