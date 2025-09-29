using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[NetworkClass(100)]
public class TeamDeathMatchGameMode : FpsGameMode
{
	protected int _redTeamPlayerCount;

	protected int _blueTeamPlayerCount;

	protected int _redTeamSplats;

	protected int _blueTeamSplats;

	protected bool _canChangeTeamInThisLife;

	protected bool HasMyTeamMorePlayers
	{
		get
		{
			return (GameState.LocalCharacter.TeamID == TeamID.RED && _redTeamPlayerCount > _blueTeamPlayerCount) || (GameState.LocalCharacter.TeamID == TeamID.BLUE && _blueTeamPlayerCount > _redTeamPlayerCount);
		}
	}

	public int BlueTeamPlayerCount
	{
		get
		{
			return _blueTeamPlayerCount;
		}
	}

	public int RedTeamPlayerCount
	{
		get
		{
			return _redTeamPlayerCount;
		}
	}

	public int RedTeamSplat
	{
		get
		{
			return _redTeamSplats;
		}
	}

	public int BlueTeamSplat
	{
		get
		{
			return _blueTeamSplats;
		}
	}

	public bool CanJoinBlueTeam
	{
		get
		{
			return _redTeamPlayerCount >= _blueTeamPlayerCount;
		}
	}

	public bool CanJoinRedTeam
	{
		get
		{
			return _redTeamPlayerCount <= _blueTeamPlayerCount;
		}
	}

	public TeamDeathMatchGameMode(GameMetaData gameData)
		: base(GameConnectionManager.Rmi, gameData)
	{
	}

	[NetworkMethod(79)]
	protected virtual void OnUpdateSplatCount(int blueScore, int redScore, bool isLeading)
	{
		if (_blueTeamSplats != blueScore || _redTeamSplats != redScore)
		{
			OnUpdateTeamScoreEvent onUpdateTeamScoreEvent = new OnUpdateTeamScoreEvent();
			onUpdateTeamScoreEvent.BlueScore = blueScore;
			onUpdateTeamScoreEvent.RedScore = redScore;
			onUpdateTeamScoreEvent.IsLeading = isLeading;
			CmuneEventHandler.Route(onUpdateTeamScoreEvent);
			_blueTeamSplats = blueScore;
			_redTeamSplats = redScore;
		}
	}

	[NetworkMethod(81)]
	protected void OnTeamBalanceUpdate(int blueCount, int redCount)
	{
		_blueTeamPlayerCount = blueCount;
		_redTeamPlayerCount = redCount;
	}

	public override void RespawnPlayer()
	{
		try
		{
			GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
			base.IsWaitingForSpawn = false;
			_canChangeTeamInThisLife = true;
			Vector3 position;
			Quaternion rotation;
			Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(_nextSpawnPoint, (GameMode)base.GameData.GameMode, GameState.LocalCharacter.TeamID, out position, out rotation);
			SpawnPlayerAt(position, rotation);
		}
		catch
		{
			Debug.LogError(string.Format("RespawnPlayer with GameState.LocalCharacter {0}", CmunePrint.Properties(GameState.LocalCharacter)));
			throw;
		}
	}

	protected override void OnEndOfMatch()
	{
		base.IsWaitingForSpawn = false;
		base.IsMatchRunning = false;
		_stateInterpolator.Pause();
		GameState.LocalPlayer.Pause(true);
		GameState.IsReadyForNextGame = false;
		HideRemotePlayerHudFeedback();
		TeamGameEndEvent teamGameEndEvent = new TeamGameEndEvent();
		teamGameEndEvent.RedTeamSplats = _redTeamSplats;
		teamGameEndEvent.BlueTeamSplats = _blueTeamSplats;
		CmuneEventHandler.Route(teamGameEndEvent);
	}

	protected override void UpdatePlayerCounters()
	{
		_redTeamPlayerCount = 0;
		_blueTeamPlayerCount = 0;
		foreach (UberStrike.Realtime.UnitySdk.CharacterInfo value in base.Players.Values)
		{
			if (value.TeamID == TeamID.RED)
			{
				_redTeamPlayerCount++;
			}
			else if (value.TeamID == TeamID.BLUE)
			{
				_blueTeamPlayerCount++;
			}
		}
	}

	[NetworkMethod(54)]
	protected void OnPlayerTeamChange(int playerID, byte teamId)
	{
		if (GameState.HasCurrentGame)
		{
			UberStrike.Realtime.UnitySdk.CharacterInfo playerWithID = GameState.CurrentGame.GetPlayerWithID(playerID);
			if (playerWithID != null)
			{
				playerWithID.TeamID = (TeamID)teamId;
				UpdatePlayerCounters();
				OnPlayerChangeTeamEvent onPlayerChangeTeamEvent = new OnPlayerChangeTeamEvent();
				onPlayerChangeTeamEvent.PlayerID = playerID;
				onPlayerChangeTeamEvent.TargetTeamID = playerWithID.TeamID;
				CmuneEventHandler.Route(onPlayerChangeTeamEvent);
			}
		}
	}

	protected override void OnSplatGameEvent(int shooter, int target, byte weaponClass, byte bodyPart)
	{
		base.OnSplatGameEvent(shooter, target, weaponClass, bodyPart);
		if (target == base.MyActorId)
		{
			_canChangeTeamInThisLife = false;
		}
	}

	public virtual void ChangeTeam()
	{
		OnChangeTeamSuccessEvent onChangeTeamSuccessEvent;
		if (Singleton<PlayerSpectatorControl>.Instance.IsEnabled && HasMyTeamMorePlayers)
		{
			onChangeTeamSuccessEvent = new OnChangeTeamSuccessEvent();
			onChangeTeamSuccessEvent.CurrentTeamID = GameState.LocalCharacter.TeamID;
			CmuneEventHandler.Route(onChangeTeamSuccessEvent);
			SendPlayerTeamChange();
			return;
		}
		if (!HasMyTeamMorePlayers)
		{
			OnChangeTeamFailEvent onChangeTeamFailEvent = new OnChangeTeamFailEvent();
			onChangeTeamFailEvent.Reason = OnChangeTeamFailEvent.FailReason.CannotChangeToATeamWithEqual;
			CmuneEventHandler.Route(onChangeTeamFailEvent);
			return;
		}
		if (!_canChangeTeamInThisLife)
		{
			OnChangeTeamFailEvent onChangeTeamFailEvent = new OnChangeTeamFailEvent();
			onChangeTeamFailEvent.Reason = OnChangeTeamFailEvent.FailReason.OnlyOneTeamChangePerLife;
			CmuneEventHandler.Route(onChangeTeamFailEvent);
			return;
		}
		onChangeTeamSuccessEvent = new OnChangeTeamSuccessEvent();
		onChangeTeamSuccessEvent.CurrentTeamID = GameState.LocalCharacter.TeamID;
		CmuneEventHandler.Route(onChangeTeamSuccessEvent);
		SendPlayerTeamChange();
		if (_isLocalAvatarLoaded && GameState.LocalCharacter.IsAlive)
		{
			_canChangeTeamInThisLife = false;
		}
	}
}
