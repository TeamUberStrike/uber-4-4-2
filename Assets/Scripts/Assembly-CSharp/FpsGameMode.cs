using System;
using System.Collections.Generic;
using System.Text;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public abstract class FpsGameMode : ClientGameMode
{
	protected bool _hasGameStarted;

	protected bool _isLocalAvatarLoaded;

	protected bool _isGameClosed;

	protected int _nextSpawnPoint;

	protected int _nextSpawnCountdown;

	private int _playerReadyForNextRound;

	protected LocalCharacterState _localStateSender;

	protected GameStateInterpolator _stateInterpolator;

	protected Dictionary<int, CharacterConfig> _characterByActorId;

	protected int _roundStartTime;

	protected Dictionary<GameFlags.GAME_FLAGS, int[]> _singleWeaponSettings;

	private ShotCountSender _shotCountSender;

	public virtual bool IsWaitingForPlayers
	{
		get
		{
			return IsGameStarted && base.Players.Count <= 1;
		}
	}

	public bool IsGameAboutToEnd
	{
		get
		{
			return GameTime >= (float)(base.GameData.RoundTime - 1);
		}
	}

	public virtual bool CanShowTabscreen
	{
		get
		{
			return IsGameStarted || GameState.LocalCharacter.IsSpectator;
		}
	}

	public virtual float GameTime
	{
		get
		{
			return (float)(GameConnectionManager.Client.PeerListener.ServerTimeTicks - _roundStartTime) / 1000f;
		}
	}

	public bool IsWaitingForSpawn { get; protected set; }

	public float NextSpawnTime { get; private set; }

	public bool IsGameClosed
	{
		get
		{
			return _isGameClosed;
		}
		set
		{
			_isGameClosed = value;
		}
	}

	public ICharacterState MyCharacterState
	{
		get
		{
			return _localStateSender;
		}
	}

	public int PlayerReadyForNextRound
	{
		get
		{
			return _playerReadyForNextRound;
		}
	}

	public GameMode GameMode
	{
		get
		{
			return (GameMode)base.GameData.GameMode;
		}
	}

	public bool IsLocalAvatarLoaded
	{
		get
		{
			return _isLocalAvatarLoaded;
		}
	}

	public int CurrentSpawnPoint
	{
		get
		{
			return _nextSpawnPoint;
		}
	}

	public int PlayerCount
	{
		get
		{
			return _characterByActorId.Count;
		}
	}

	public IEnumerable<CharacterConfig> AllCharacters
	{
		get
		{
			return _characterByActorId.Values;
		}
	}

	protected FpsGameMode(RemoteMethodInterface rmi, GameMetaData gameData)
		: base(rmi, gameData)
	{
		_singleWeaponSettings = new Dictionary<GameFlags.GAME_FLAGS, int[]>(4);
		_singleWeaponSettings.Add(GameFlags.GAME_FLAGS.CannonArena, new int[4] { 0, 1020, 0, 0 });
		_singleWeaponSettings.Add(GameFlags.GAME_FLAGS.Instakill, new int[4] { 0, 1147, 0, 0 });
		_singleWeaponSettings.Add(GameFlags.GAME_FLAGS.NinjaArena, new int[4] { 1136, 0, 0, 0 });
		_singleWeaponSettings.Add(GameFlags.GAME_FLAGS.SniperArena, new int[4] { 0, 1018, 0, 0 });
		ApplyGameFlags();
		_characterByActorId = new Dictionary<int, CharacterConfig>(16);
		_stateInterpolator = new GameStateInterpolator();
		_localStateSender = new LocalCharacterState(GameState.LocalCharacter, this);
		_shotCountSender = new ShotCountSender(this);
	}

	public void FixedUpdate()
	{
		if (IsGameStarted && base.HasJoinedGame)
		{
			_localStateSender.SendUpdates();
		}
	}

	public void Update()
	{
		if (IsGameStarted)
		{
			_stateInterpolator.Interpolate();
			if ((float)base.GameData.RoundTime - GameTime > 10f)
			{
				_shotCountSender.UpdateEveryTenSeconds();
			}
			else
			{
				_shotCountSender.UpdateEverySecond();
			}
		}
	}

	public static bool IsSingleWeapon(GameMetaData data)
	{
		return GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.NinjaArena, data.GameModifierFlags) || GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.CannonArena, data.GameModifierFlags) || GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.SniperArena, data.GameModifierFlags) || GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.Instakill, data.GameModifierFlags);
	}

	public static string GetGameFlagText(GameMetaData data)
	{
		string result = string.Empty;
		if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.CannonArena, data.GameModifierFlags))
		{
			result = LocalizedStrings.CannonArena;
		}
		else if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.Instakill, data.GameModifierFlags))
		{
			result = LocalizedStrings.Instakill;
		}
		else if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.LowGravity, data.GameModifierFlags))
		{
			result = LocalizedStrings.LowGravity;
		}
		else if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.NinjaArena, data.GameModifierFlags))
		{
			result = LocalizedStrings.NinjaArena;
		}
		else if (GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.SniperArena, data.GameModifierFlags))
		{
			result = LocalizedStrings.SniperArena;
		}
		return result;
	}

	public virtual void RespawnPlayer()
	{
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
		IsWaitingForSpawn = false;
		Vector3 position = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(_nextSpawnPoint, (GameMode)base.GameData.GameMode, TeamID.NONE, out position, out rotation);
		SpawnPlayerAt(position, rotation);
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		if (base.GameData != null)
		{
			SendMethodToServer(70, (byte)Singleton<SpawnPointManager>.Instance.GetSpawnPointCount((GameMode)base.GameData.GameMode, TeamID.NONE), (byte)Singleton<SpawnPointManager>.Instance.GetSpawnPointCount((GameMode)base.GameData.GameMode, TeamID.RED), (byte)Singleton<SpawnPointManager>.Instance.GetSpawnPointCount((GameMode)base.GameData.GameMode, TeamID.BLUE));
		}
		if (_hasGameStarted)
		{
			InitializeMode(GameState.LocalCharacter.TeamID, GameState.LocalCharacter.IsSpectator);
		}
	}

	public void InitializeMode(TeamID team = TeamID.NONE, bool isSpectator = false)
	{
		_hasGameStarted = true;
		GameState.LocalCharacter.ResetState();
		GameState.LocalCharacter.ActorId = _rmi.Messenger.PeerListener.ActorIdSecure;
		GameState.LocalCharacter.CurrentRoom = _rmi.Messenger.PeerListener.CurrentRoom;
		GameState.LocalCharacter.Channel = ApplicationDataManager.Channel;
		GameState.LocalCharacter.TeamID = team;
		GameState.LocalCharacter.Cmid = PlayerDataManager.CmidSecure;
		GameState.LocalCharacter.PlayerName = ((!PlayerDataManager.IsPlayerInClan) ? PlayerDataManager.NameSecure : string.Format("[{0}] {1}", PlayerDataManager.ClanTag, PlayerDataManager.NameSecure));
		GameState.LocalCharacter.ClanTag = PlayerDataManager.ClanTag;
		GameState.LocalCharacter.Level = PlayerDataManager.PlayerLevelSecure;
		if (isSpectator)
		{
			GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Spectating);
			GameState.LocalCharacter.Set(PlayerStates.SPECTATOR);
		}
		else
		{
			if (Singleton<PlayerSpectatorControl>.Instance.IsEnabled)
			{
				GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Spectating);
			}
			else
			{
				GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.AfterRound);
			}
			GameState.LocalPlayer.UpdateLocalCharacterLoadout();
		}
		SendMethodToServer(1, GameState.LocalCharacter);
		OnModeInitialized();
		CmuneEventHandler.Route(new OnModeInitializedEvent());
	}

	protected virtual void OnModeInitialized()
	{
	}

	public void DebugAll()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("_avatars: {0}", _characterByActorId.Count);
		stringBuilder.AppendFormat("_coolDownTime: {0}", _nextSpawnCountdown);
		stringBuilder.AppendFormat("_instanceID: {0}", _instanceID);
		stringBuilder.AppendFormat("_lookupIndexMethod: {0}", _lookupIndexMethod.Count);
		stringBuilder.AppendFormat("_lookupNameIndex: {0}", _lookupNameIndex.Count);
		stringBuilder.AppendFormat("_nextSpawnPoint: {0}", _nextSpawnPoint);
		stringBuilder.AppendFormat("IsGameStarted: {0}", IsGameStarted);
		stringBuilder.AppendFormat("IsGlobal: {0}", IsGlobal);
		stringBuilder.AppendFormat("IsInitialized: {0}", IsInitialized);
		stringBuilder.AppendFormat("IsRoundRunning: {0}", IsMatchRunning);
		stringBuilder.AppendFormat("NetworkID: {0}", NetworkID);
		stringBuilder.AppendFormat("Players: {0}", base.Players.Count);
		Debug.Log(stringBuilder.ToString());
	}

	protected override void OnUninitialized()
	{
		Singleton<ChatManager>.Instance.UpdateLastgamePlayers();
		SendMethodToServer(2, base.MyActorId);
		int[] array = Conversion.ToArray((ICollection<int>)base.Players.Keys);
		int[] array2 = array;
		foreach (int actorId in array2)
		{
			OnPlayerLeft(actorId);
		}
		base.OnUninitialized();
	}

	protected override void Dispose(bool dispose)
	{
		Singleton<PlayerSpectatorControl>.Instance.IsEnabled = false;
		if (_isDisposed)
		{
			return;
		}
		if (dispose)
		{
			base.IsMatchRunning = false;
			Singleton<InGameChatHud>.Instance.ClearHistory();
			PopupSystem.ClearAll();
			if ((bool)GameState.LocalAvatar.Decorator)
			{
				GameState.LocalAvatar.Decorator.MeshRenderer.enabled = true;
			}
			UnloadAllPlayers();
		}
		base.Dispose(dispose);
	}

	private void ApplyGameFlags()
	{
		GameFlags.GAME_FLAGS gameModifierFlags = (GameFlags.GAME_FLAGS)base.GameData.GameModifierFlags;
		if (!_singleWeaponSettings.ContainsKey(gameModifierFlags))
		{
		}
	}

	private void ConfigureCharacter(UberStrike.Realtime.UnitySdk.CharacterInfo info, CharacterConfig character, bool isLocal)
	{
		if (character != null && info != null)
		{
			if (isLocal)
			{
				GameState.LocalPlayer.SetCurrentCharacterConfig(character);
				if ((bool)GameState.LocalAvatar.Decorator)
				{
					_localStateSender.Info.Position = GameState.LocalAvatar.Decorator.transform.position + Vector3.up;
					_localStateSender.Info.HorizontalRotation = GameState.LocalAvatar.Decorator.transform.rotation;
					character.Initialize(_localStateSender, GameState.LocalAvatar);
				}
				GameState.LocalPlayer.MoveController.IsLowGravity = GameFlags.IsFlagSet(GameFlags.GAME_FLAGS.LowGravity, base.GameData.GameModifierFlags);
			}
			else
			{
				Avatar avatar = new Avatar(Loadout.Create(info.Gear, info.Weapons.ItemIDs), false);
				avatar.Decorator = Singleton<AvatarBuilder>.Instance.CreateRemoteAvatar(avatar.Loadout.GetAvatarGear(), info.SkinColor);
				character.Initialize(_stateInterpolator.GetState(info.ActorId), avatar);
				if (info.ActorId > base.MyActorId)
				{
					Singleton<HudUtil>.Instance.AddInGameEvent(info.PlayerName, LocalizedStrings.JoinedTheGame, UberstrikeItemClass.FunctionalGeneral, InGameEventFeedbackType.CustomMessage, info.TeamID, TeamID.NONE);
				}
			}
			OnCharacterLoaded();
		}
		else
		{
			Debug.LogError(string.Format("OnAvatarLoaded failed because loaded Avatar is {0} and Info is {1}", character != null, info != null));
		}
	}

	protected virtual void OnCharacterLoaded()
	{
	}

	protected sealed override void OnPlayerJoined(SyncObject data, Vector3 position)
	{
		base.OnPlayerJoined(data, position);
		Singleton<ChatManager>.Instance.SetGameSection(base.GameData.RoomID, base.Players.Values);
		UberStrike.Realtime.UnitySdk.CharacterInfo value;
		if (base.Players.TryGetValue(data.Id, out value))
		{
			OnNormalJoin(value);
		}
		else
		{
			GameState.LocalPlayer.UnPausePlayer();
		}
	}

	protected virtual void OnNormalJoin(UberStrike.Realtime.UnitySdk.CharacterInfo info)
	{
		if (info.ActorId != base.MyActorId)
		{
			_stateInterpolator.AddCharacterInfo(info);
		}
		else
		{
			SendMethodToServer(62, base.MyActorId, PickupItem.GetRespawnDurations());
		}
		InstantiateCharacter(info);
	}

	protected override void OnPlayerLeft(int actorId)
	{
		try
		{
			UberStrike.Realtime.UnitySdk.CharacterInfo playerWithID = GetPlayerWithID(actorId);
			if (playerWithID != null)
			{
				Singleton<EventStreamHud>.Instance.AddEventText(playerWithID.PlayerName, playerWithID.TeamID, LocalizedStrings.LeftTheGame, string.Empty);
			}
			CharacterConfig value;
			if (_characterByActorId.TryGetValue(actorId, out value))
			{
				if ((bool)value)
				{
					value.Destroy();
				}
				_characterByActorId.Remove(actorId);
			}
			if (actorId == GameState.LocalCharacter.ActorId)
			{
				GameState.LocalCharacter.ResetState();
				GameState.LocalPlayer.SetCurrentCharacterConfig(null);
			}
			else
			{
				_stateInterpolator.RemoveCharacterInfo(actorId);
			}
		}
		catch
		{
			Debug.LogError(string.Format("OnPlayerLeft with actorId={0}", actorId));
			throw;
		}
		finally
		{
			base.OnPlayerLeft(actorId);
			Singleton<ChatManager>.Instance.SetGameSection(base.GameData.RoomID, base.Players.Values);
		}
	}

	public void SendCharacterInfoUpdate()
	{
		if (IsInitialized && GameState.LocalCharacter.ActorId > 0 && !GameState.LocalCharacter.IsSpectator)
		{
			SyncObject syncData = SyncObjectBuilder.GetSyncData(GameState.LocalCharacter, false);
			if (!syncData.IsEmpty)
			{
				SendMethodToServer(3, syncData);
			}
		}
	}

	public void SendPositionUpdate()
	{
		if (IsInitialized && GameState.LocalCharacter.PlayerNumber > 0)
		{
			List<byte> bytes = new List<byte>(14);
			DefaultByteConverter.FromInt(GameState.LocalCharacter.ActorId, ref bytes);
			ShortVector3.Bytes(bytes, GameState.LocalCharacter.Position);
			DefaultByteConverter.FromInt(GameConnectionManager.Client.PeerListener.ServerTimeTicks, ref bytes);
			SendUnreliableMethodToServer(83, bytes);
		}
	}

	[NetworkMethod(83)]
	protected void OnPositionsUpdate(List<byte> positions)
	{
		int i = 0;
		int num = positions[i++];
		byte[] array = positions.ToArray();
		List<PlayerPosition> list = new List<PlayerPosition>(num);
		for (int j = 0; j < num; j++)
		{
			if (i + 11 > array.Length)
			{
				break;
			}
			byte id = array[i++];
			int time = DefaultByteConverter.ToInt(array, ref i);
			ShortVector3 p = new ShortVector3(array, ref i);
			list.Add(new PlayerPosition(id, p, time));
		}
		_stateInterpolator.UpdatePositionSmooth(list);
	}

	protected override void OnGameFrameUpdate(List<SyncObject> deltas)
	{
		try
		{
			bool flag = false;
			foreach (SyncObject delta in deltas)
			{
				if (!delta.IsEmpty)
				{
					if (delta.Id == base.MyActorId)
					{
						ApplyCurrentGameFrameUpdates(delta);
						_localStateSender.RecieveDeltaUpdate(delta);
					}
					else
					{
						_stateInterpolator.UpdateCharacterInfo(delta);
					}
					if (delta.Contains(262144))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				UpdatePlayerCounters();
			}
		}
		catch (Exception ex)
		{
			ex.Data.Add("OnGameFrameUpdate", deltas.Count);
			throw;
		}
	}

	protected virtual void UpdatePlayerCounters()
	{
	}

	protected virtual void ApplyCurrentGameFrameUpdates(SyncObject delta)
	{
		try
		{
			if (delta.Contains(2097152))
			{
				int num = (short)delta.Data[2097152];
				Singleton<HpApHud>.Instance.HP = num;
				if (num <= 0)
				{
					GameState.LocalPlayer.SetPlayerDead();
				}
			}
			if (delta.Contains(67108864))
			{
				ArmorInfo armorInfo = (ArmorInfo)delta.Data[67108864];
				Singleton<HpApHud>.Instance.AP = armorInfo.ArmorPoints;
			}
			if (delta.Contains(16384))
			{
				StatsInfo statsInfo = (StatsInfo)delta.Data[16384];
				if (statsInfo.Kills == 0 && statsInfo.Deaths == 0)
				{
					HudController.Instance.XpPtsHud.OnGameStart();
				}
			}
		}
		catch
		{
			Debug.LogError(string.Format("ApplyCurrentGameFrameUpdates with delta.Id={0} and DeltaCode={1}", delta.Id, delta.DeltaCode));
			throw;
		}
	}

	protected override void OnStartGame()
	{
		base.OnStartGame();
		_stateInterpolator.Run();
	}

	[NetworkMethod(76)]
	protected virtual void OnMatchStart(string matchId, int matchEndServerTicks)
	{
		AutoMonoBehaviour<GameConnectionManager>.Instance.MatchID = matchId;
		GameConnectionManager.Client.PeerListener.UpdateServerTime();
		base.IsMatchRunning = true;
		_roundStartTime = matchEndServerTicks - base.GameData.RoundTime * 1000;
		_stateInterpolator.Run();
		LevelCamera.Instance.ResetFeedback();
		GameState.LocalPlayer.UpdateWeaponController();
		foreach (CharacterConfig value in _characterByActorId.Values)
		{
			value.IsAnimationEnabled = true;
		}
		OnMatchStartEvent onMatchStartEvent = new OnMatchStartEvent();
		onMatchStartEvent.MatchId = matchId;
		onMatchStartEvent.MatchEndServerTicks = matchEndServerTicks;
		CmuneEventHandler.Route(onMatchStartEvent);
	}

	[NetworkMethod(77)]
	protected void OnMatchEnd(EndOfMatchData endOfMatchData)
	{
		if (GameState.LocalPlayer.IsDead)
		{
			if (GameState.LocalAvatar.Decorator != null)
			{
				GameState.LocalAvatar.EnableDecorator();
			}
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(_nextSpawnPoint, (GameMode)base.GameData.GameMode, GameState.LocalCharacter.TeamID, out position, out rotation);
			GameState.LocalPlayer.SpawnPlayerAt(position, rotation);
		}
		if (GameState.LocalPlayer.Character != null)
		{
			switch (Singleton<WeaponController>.Instance.CurrentSlot)
			{
			case LoadoutSlotType.WeaponMelee:
				GameState.LocalPlayer.Character.WeaponSimulator.UpdateWeaponSlot(0, false);
				break;
			case LoadoutSlotType.WeaponPrimary:
				GameState.LocalPlayer.Character.WeaponSimulator.UpdateWeaponSlot(1, false);
				break;
			case LoadoutSlotType.WeaponSecondary:
				GameState.LocalPlayer.Character.WeaponSimulator.UpdateWeaponSlot(2, false);
				break;
			case LoadoutSlotType.WeaponTertiary:
				GameState.LocalPlayer.Character.WeaponSimulator.UpdateWeaponSlot(3, false);
				break;
			}
		}
		LevelCamera.SetBobMode(BobMode.Idle);
		if ((bool)Singleton<WeaponController>.Instance.CurrentDecorator)
		{
			Singleton<WeaponController>.Instance.CurrentDecorator.StopSound();
		}
		foreach (CharacterConfig value in _characterByActorId.Values)
		{
			value.State.Set(PlayerStates.PAUSED, true);
		}
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.AfterRound);
		Singleton<EndOfMatchStats>.Instance.Data = endOfMatchData;
		UpdatePlayerStatistics(endOfMatchData.PlayerStatsTotal, endOfMatchData.PlayerStatsBestPerLife);
		PlayerDataManager.AddPointsSecure(Singleton<EndOfMatchStats>.Instance.GainedPts);
		if (ApplicationDataManager.Channel == ChannelType.WebFacebook)
		{
			Debug.Log(string.Format("Match:{0} Total:{1}", endOfMatchData.PlayerStatsTotal.GetKills(), Singleton<PlayerDataManager>.Instance.ServerLocalPlayerStatisticsView.Splats));
			if (endOfMatchData.PlayerStatsTotal.GetKills() > 0)
			{
				AutoMonoBehaviour<FacebookInterface>.Instance.PublishScore(Singleton<PlayerDataManager>.Instance.ServerLocalPlayerStatisticsView.Splats);
				AchievementType playersFirstAchievement = GetPlayersFirstAchievement(endOfMatchData);
				if (playersFirstAchievement != AchievementType.None)
				{
					AutoMonoBehaviour<FacebookInterface>.Instance.PublishAchievement(playersFirstAchievement);
				}
			}
		}
		AutoMonoBehaviour<GameConnectionManager>.Instance.RoundsPlayed++;
		ApplicationDataManager.EventsSystem.SendFinishMatch(PlayerDataManager.CmidSecure, AutoMonoBehaviour<GameConnectionManager>.Instance.GameID, AutoMonoBehaviour<GameConnectionManager>.Instance.MatchID);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.Overview);
		CmuneEventHandler.Route(new OnMatchEndEvent());
		Singleton<ProjectileManager>.Instance.ClearAll();
		OnEndOfMatch();
	}

	protected virtual void OnEndOfMatch()
	{
	}

	private AchievementType GetPlayersFirstAchievement(EndOfMatchData endOfMatchData)
	{
		AchievementType result = AchievementType.None;
		StatsSummary statsSummary = endOfMatchData.MostValuablePlayers.Find((StatsSummary p) => p.Cmid == PlayerDataManager.Cmid);
		if (statsSummary != null)
		{
			List<AchievementType> list = new List<AchievementType>();
			foreach (KeyValuePair<byte, ushort> achievement in statsSummary.Achievements)
			{
				list.Add((AchievementType)achievement.Key);
			}
			if (list.Count > 0)
			{
				result = list[0];
			}
		}
		return result;
	}

	private void UpdatePlayerStatistics(StatsCollection totalStats, StatsCollection bestPerLife)
	{
		int playerLevelSecure = PlayerDataManager.PlayerLevelSecure;
		if (playerLevelSecure > 0 && playerLevelSecure < XpPointsUtil.MaxPlayerLevel)
		{
			Singleton<PlayerDataManager>.Instance.UpdatePlayerStats(totalStats, bestPerLife);
			if (PlayerDataManager.PlayerLevel != playerLevelSecure)
			{
				PopupSystem.Show(new LevelUpPopup(PlayerDataManager.PlayerLevel, playerLevelSecure));
			}
			GameState.LocalCharacter.Level = PlayerDataManager.PlayerLevelSecure;
		}
	}

	[NetworkMethod(97)]
	protected void OnSetPowerupState(List<int> pickedPowerupIds)
	{
		int num = 0;
		while (pickedPowerupIds != null && num < pickedPowerupIds.Count)
		{
			CmuneEventHandler.Route(new PickupItemEvent(pickedPowerupIds[num], false));
			num++;
		}
	}

	[NetworkMethod(61)]
	protected void OnPowerUpPicked(int powerupID, byte state)
	{
		CmuneEventHandler.Route(new PickupItemEvent(powerupID, state == 0));
	}

	[NetworkMethod(84)]
	protected void OnDoorOpened(int doorID)
	{
		CmuneEventHandler.Route(new DoorOpenedEvent(doorID));
	}

	[NetworkMethod(71)]
	protected virtual void OnSetNextSpawnPoint(int index, int coolDownTime)
	{
		if (!GameState.LocalCharacter.IsSpectator)
		{
			RespawnPlayerInSeconds(index, coolDownTime);
		}
	}

	public void RespawnPlayerInSeconds(int index, int seconds)
	{
		_nextSpawnPoint = index;
		if (seconds > 0)
		{
			_nextSpawnCountdown = seconds;
			NextSpawnTime = Time.time + (float)seconds;
			IsWaitingForSpawn = true;
		}
		else
		{
			RespawnPlayer();
		}
	}

	public bool HasAvatarLoaded(int actorId)
	{
		return _characterByActorId.ContainsKey(actorId);
	}

	[NetworkMethod(75)]
	protected void OnSetEndOfRoundCountdown(int secondsUntilNextRound)
	{
		OnSetEndOfMatchCountdownEvent onSetEndOfMatchCountdownEvent = new OnSetEndOfMatchCountdownEvent();
		onSetEndOfMatchCountdownEvent.SecondsUntilNextMatch = secondsUntilNextRound;
		CmuneEventHandler.Route(onSetEndOfMatchCountdownEvent);
	}

	[NetworkMethod(78)]
	protected virtual void OnDamageEvent(DamageEvent ev)
	{
		if (!GameState.HasCurrentPlayer)
		{
			return;
		}
		foreach (KeyValuePair<byte, byte> item in ev.Damage)
		{
			OnPlayerDamageEvent onPlayerDamageEvent = new OnPlayerDamageEvent();
			onPlayerDamageEvent.Angle = Conversion.Byte2Angle(item.Key);
			onPlayerDamageEvent.DamageValue = (int)item.Value;
			CmuneEventHandler.Route(onPlayerDamageEvent);
			if ((ev.DamageEffectFlag & 1) != 0)
			{
				GameState.LocalPlayer.DamageFactor = ev.DamgeEffectValue;
			}
		}
	}

	[NetworkMethod(80)]
	protected virtual void OnSplatGameEvent(int shooter, int target, byte weaponClass, byte bodyPart)
	{
		UberStrike.Realtime.UnitySdk.CharacterInfo value;
		UberStrike.Realtime.UnitySdk.CharacterInfo value2;
		if (!base.Players.TryGetValue(shooter, out value) || !base.Players.TryGetValue(target, out value2))
		{
			return;
		}
		if (shooter != target)
		{
			if (shooter == base.MyActorId)
			{
				OnPlayerKillEnemyEvent onPlayerKillEnemyEvent = new OnPlayerKillEnemyEvent();
				onPlayerKillEnemyEvent.EmemyInfo = value2;
				onPlayerKillEnemyEvent.WeaponCategory = (UberstrikeItemClass)weaponClass;
				onPlayerKillEnemyEvent.BodyHitPart = (BodyPart)bodyPart;
				CmuneEventHandler.Route(onPlayerKillEnemyEvent);
				return;
			}
			if (target == base.MyActorId)
			{
				OnPlayerKilledEvent onPlayerKilledEvent = new OnPlayerKilledEvent();
				onPlayerKilledEvent.ShooterInfo = value;
				onPlayerKilledEvent.WeaponCategory = (UberstrikeItemClass)weaponClass;
				onPlayerKilledEvent.BodyHitPart = (BodyPart)bodyPart;
				CmuneEventHandler.Route(onPlayerKilledEvent);
				return;
			}
			InGameEventFeedbackType eventType = InGameEventFeedbackType.None;
			if (weaponClass == 1)
			{
				eventType = InGameEventFeedbackType.Humiliation;
			}
			else
			{
				switch (bodyPart)
				{
				case 2:
					eventType = InGameEventFeedbackType.HeadShot;
					break;
				case 4:
					eventType = InGameEventFeedbackType.NutShot;
					break;
				}
			}
			Singleton<HudUtil>.Instance.AddInGameEvent(value.PlayerName, value2.PlayerName, (UberstrikeItemClass)weaponClass, eventType, value.TeamID, value2.TeamID);
		}
		else
		{
			OnPlayerSuicideEvent onPlayerSuicideEvent = new OnPlayerSuicideEvent();
			onPlayerSuicideEvent.PlayerInfo = value;
			CmuneEventHandler.Route(onPlayerSuicideEvent);
		}
	}

	[NetworkMethod(85)]
	protected void OnSetPlayerSpawnPosition(byte playerNumber, Vector3 pos)
	{
		_stateInterpolator.UpdatePositionHard(playerNumber, pos);
	}

	public void SendPlayerTeamChange()
	{
		SendMethodToServer(54, GameState.CurrentPlayerID);
	}

	public void SetReadyForNextMatch(bool isReady)
	{
		if (isReady)
		{
			SendMethodToServer(74, base.MyActorId);
		}
	}

	public void SendPlayerSpawnPosition(Vector3 position)
	{
		SendMethodToServer(85, base.MyActorId, position);
	}

	public void UpdatePlayerReadyForNextRound()
	{
		_playerReadyForNextRound = 0;
		foreach (UberStrike.Realtime.UnitySdk.CharacterInfo value in base.Players.Values)
		{
			if (value.IsReadyForGame)
			{
				_playerReadyForNextRound++;
			}
		}
	}

	protected void SpawnPlayerAt(Vector3 position, Quaternion rotation)
	{
		try
		{
			GameState.LocalPlayer.SpawnPlayerAt(position, rotation);
			GameState.LocalPlayer.InitializePlayer();
			CmuneEventHandler.Route(new OnPlayerRespawnEvent());
			GameState.LocalPlayer.UpdateWeaponController();
			SendMethodToServer(6, base.MyActorId);
		}
		catch
		{
			Debug.LogError(string.Format("SpawnPlayerAt with game {0}", CmunePrint.Properties(this)));
			throw;
		}
	}

	public virtual void RequestRespawn()
	{
		SendMethodToServer(72, base.MyActorId);
	}

	public virtual void IncreaseHealthAndArmor(int health, int armor)
	{
		SendMethodToServer(98, base.MyActorId, (byte)health, (byte)armor);
	}

	public virtual void PickupPowerup(int pickupID, PickupItemType type, int value)
	{
		SendMethodToServer(61, base.MyActorId, pickupID, (byte)type, (byte)value);
	}

	public void OpenDoor(int doorID)
	{
		SendMethodToServer(84, doorID);
	}

	public void EmitQuickItem(Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID)
	{
		SendMethodToAll(100, origin, direction, itemId, playerNumber, projectileID);
	}

	[NetworkMethod(100)]
	protected void OnEmitQuickItem(Vector3 origin, Vector3 direction, int itemId, byte playerNumber, int projectileID)
	{
		if (!GameState.CurrentGame.IsGameStarted)
		{
			return;
		}
		IUnityItem itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(itemId);
		if (itemInShop != null && itemInShop.View.ItemType == UberstrikeItemType.QuickUse)
		{
			if (itemInShop.IsLoaded && (bool)itemInShop.Prefab)
			{
				IGrenadeProjectile grenadeProjectile = itemInShop.Prefab.GetComponent<QuickItem>() as IGrenadeProjectile;
				IGrenadeProjectile grenadeProjectile2 = grenadeProjectile.Throw(origin, direction);
				if (playerNumber == GameState.CurrentPlayerID)
				{
					grenadeProjectile2.SetLayer(UberstrikeLayer.LocalProjectile);
				}
				else
				{
					grenadeProjectile2.SetLayer(UberstrikeLayer.RemoteProjectile);
				}
				Singleton<ProjectileManager>.Instance.AddProjectile(grenadeProjectile2, projectileID);
			}
			else
			{
				itemInShop.Create(origin, Quaternion.identity);
			}
		}
		else
		{
			Debug.LogWarning("OnEmitQuickItem failed because item not found: " + itemId + "/" + playerNumber + "/" + projectileID);
		}
	}

	public void EmitProjectile(Vector3 origin, Vector3 direction, LoadoutSlotType slot, int projectileID, bool explode)
	{
		SendMethodToAll(86, base.MyActorId, origin, direction, (byte)slot, projectileID, explode);
	}

	[NetworkMethod(86)]
	protected void OnEmitProjectile(int actorId, Vector3 origin, Vector3 direction, byte slot, int projectileID, bool explode)
	{
		CharacterConfig character;
		if (TryGetCharacter(actorId, out character))
		{
			IProjectile projectile = character.WeaponSimulator.EmitProjectile(actorId, character.State.PlayerNumber, origin, direction, (LoadoutSlotType)slot, projectileID, explode);
			if (projectile != null)
			{
				Singleton<ProjectileManager>.Instance.AddProjectile(projectile, projectileID);
			}
		}
	}

	public void RemoveProjectile(int projectileId, bool explode)
	{
		SendMethodToAll(87, projectileId, explode);
	}

	[NetworkMethod(87)]
	protected virtual void OnRemoveProjectile(int projectileId, bool explode)
	{
		Singleton<ProjectileManager>.Instance.RemoveProjectile(projectileId, explode);
	}

	public void SingleBulletFire()
	{
		SendMethodToAll(89, base.MyActorId);
	}

	[NetworkMethod(89)]
	protected virtual void OnSingleBulletFire(int actorId)
	{
		CharacterConfig character;
		if (TryGetCharacter(actorId, out character) && character.State.IsAlive && !character.IsLocal)
		{
			character.WeaponSimulator.Shoot(character.State);
		}
	}

	public virtual void PlayerHit(int targetPlayer, short damage, BodyPart part, Vector3 force, int shotCount, int weaponID, UberstrikeItemClass weaponClass, DamageEffectType damageEffectFlag, float damageEffectValue)
	{
		byte b = 0;
		Vector3 normalized = force.normalized;
		normalized.y = 0f;
		if (normalized.magnitude != 0f)
		{
			b = Conversion.Angle2Byte(Quaternion.LookRotation(normalized).eulerAngles.y);
		}
		SendMethodToServer(68, base.MyActorId, targetPlayer, damage, (byte)part, shotCount, b, weaponID, (byte)weaponClass, (int)damageEffectFlag, damageEffectValue);
		if (base.MyActorId == targetPlayer)
		{
			short finalDamage;
			byte finalArmorPoints;
			GameState.LocalCharacter.Armor.SimulateAbsorbDamage(damage, part, out finalDamage, out finalArmorPoints);
			Singleton<HpApHud>.Instance.HP = GameState.LocalCharacter.Health - finalDamage;
			Singleton<HpApHud>.Instance.AP = finalArmorPoints;
			GameState.LocalPlayer.MoveController.ApplyForce(force, CharacterMoveController.ForceType.Additive);
		}
	}

	public void SendPlayerHitFeedback(int targetPlayer, Vector3 force)
	{
		SendMethodToPlayer(targetPlayer, 68, force);
	}

	[NetworkMethod(68)]
	public void OnPlayerHit(Vector3 force)
	{
		GameState.LocalPlayer.MoveController.ApplyForce(force, CharacterMoveController.ForceType.Additive);
	}

	public void ActivateQuickItem(QuickItemLogic logic, int robotLifeTime, int scrapsLifeTime, bool isInstant = false)
	{
		SendMethodToAll(99, GameState.LocalCharacter.ActorId, (byte)logic, robotLifeTime, scrapsLifeTime, isInstant);
	}

	[NetworkMethod(99)]
	public void OnQuickItemEvent(int actorId, byte eventType, int robotLifeTime, int scrapsLifeTime, bool isInstant)
	{
		CharacterConfig character;
		if (TryGetCharacter(actorId, out character))
		{
			Singleton<QuickItemSfxController>.Instance.ShowThirdPersonEffect(character, (QuickItemLogic)eventType, robotLifeTime, scrapsLifeTime, isInstant);
		}
	}

	[NetworkMethod(101)]
	protected void KickPlayer(string message)
	{
		PopupSystem.ShowMessage("Attention", message, PopupSystem.AlertType.OK, delegate
		{
		});
		Singleton<GameStateController>.Instance.LeaveGame();
	}

	[NetworkMethod(31)]
	protected void OnModCustomMessage(string message)
	{
		CommConnectionManager.CommCenter.OnModerationCustomMessage(message);
	}

	[NetworkMethod(30)]
	protected void OnMutePlayer(bool mute)
	{
		CommConnectionManager.CommCenter.OnModerationMutePlayer(mute);
	}

	public bool TryGetCharacter(int actorId, out CharacterConfig character)
	{
		return _characterByActorId.TryGetValue(actorId, out character) && character != null;
	}

	public bool TryGetDecorator(int actorId, out AvatarDecorator decorator)
	{
		CharacterConfig value;
		if (_characterByActorId.TryGetValue(actorId, out value) && value != null)
		{
			decorator = value.Avatar.Decorator;
		}
		else
		{
			decorator = null;
		}
		return decorator != null;
	}

	protected void HideRemotePlayerHudFeedback()
	{
		foreach (CharacterConfig value in _characterByActorId.Values)
		{
			if (value != null && value.Avatar.Decorator != null)
			{
				value.Avatar.Decorator.HudInformation.Hide();
			}
		}
	}

	public bool TryGetPlayerWithCmid(int cmid, out UberStrike.Realtime.UnitySdk.CharacterInfo config)
	{
		config = null;
		foreach (UberStrike.Realtime.UnitySdk.CharacterInfo value in base.Players.Values)
		{
			if (value.Cmid == cmid)
			{
				config = value;
				break;
			}
		}
		return config != null;
	}

	protected void InstantiateCharacter(UberStrike.Realtime.UnitySdk.CharacterInfo info)
	{
		if (!_characterByActorId.ContainsKey(info.ActorId))
		{
			if (info.ActorId == base.MyActorId)
			{
				_isLocalAvatarLoaded = true;
				CharacterConfig characterConfig = PrefabManager.Instance.InstantiateLocalCharacter();
				_characterByActorId.Add(info.ActorId, characterConfig);
				ConfigureCharacter(info, characterConfig, true);
			}
			else
			{
				CharacterConfig characterConfig2 = PrefabManager.Instance.InstantiateRemoteCharacter();
				_characterByActorId.Add(info.ActorId, characterConfig2);
				ConfigureCharacter(info, characterConfig2, false);
			}
		}
		else
		{
			Debug.LogError(string.Format("Failed call of LoadAvatarAsset {0} because already loaded!", info.ActorId));
		}
	}

	protected void LeaveClientGameMode(int playerId)
	{
		base.OnPlayerLeft(playerId);
	}

	public void ChangeAllPlayerOutline(TeamID myTeam)
	{
		if (myTeam == TeamID.NONE)
		{
			return;
		}
		foreach (KeyValuePair<int, CharacterConfig> item in _characterByActorId)
		{
			if (item.Key != base.MyActorId)
			{
				UpdatePlayerOutlineByTeamID(item.Value, myTeam);
			}
		}
	}

	public void ChangePlayerOutlineById(int playerID)
	{
		CharacterConfig character;
		if (TryGetCharacter(playerID, out character))
		{
			ChangePlayerOutline(character);
		}
	}

	public void ChangePlayerOutline(CharacterConfig player)
	{
		UpdatePlayerOutlineByTeamID(player, GameState.LocalCharacter.TeamID);
	}

	public void UpdatePlayerOutlineByTeamID(CharacterConfig player, TeamID id)
	{
		if (player != null)
		{
			if (id != TeamID.NONE && player.Team == id)
			{
				player.Avatar.Decorator.EnableOutline(true);
			}
			else
			{
				player.Avatar.Decorator.EnableOutline(false);
			}
		}
		else
		{
			Debug.LogError("Failed to Change player outline");
		}
	}

	public void UnloadAllPlayers()
	{
		foreach (CharacterConfig value in _characterByActorId.Values)
		{
			value.Destroy();
		}
		_characterByActorId.Clear();
	}

	public void SendShotCounts(List<int> shotCounts)
	{
		SendMethodToServer(102, PlayerDataManager.CmidSecure, shotCounts);
	}
}
