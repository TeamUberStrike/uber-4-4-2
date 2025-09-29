using System;
using UberStrike.Realtime.UnitySdk;

internal class TeamDeathMatchState : IState
{
	private const HudDrawFlags _hudDrawFlag = HudDrawFlags.Score | HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg;

	private TeamDeathMatchGameMode _teamDeathMatchGameMode;

	private InGameCountdown _ingameCountdown;

	private StateMachine _stateMachine;

	public GameMetaData GameMetaData { get; set; }

	public TeamDeathMatchState()
	{
		_ingameCountdown = new InGameCountdown();
		_stateMachine = new StateMachine();
		_stateMachine.RegisterState(18, new InGamePregameLoadoutState());
		_stateMachine.RegisterState(17, new InGamePlayingState(_stateMachine, HudDrawFlags.Score | HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg));
		_stateMachine.RegisterState(20, new InGameEndOfMatchState());
		_stateMachine.RegisterState(22, new InGamePlayerKilledState(_stateMachine, HudDrawFlags.Score | HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg, true));
		_stateMachine.RegisterState(25, new InGamePlayerPausedState(_stateMachine));
		_stateMachine.RegisterState(21, new InGameSpectatingState(_stateMachine, HudDrawFlags.Score | HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg));
	}

	public void OnEnter()
	{
		if (GameMetaData == null)
		{
			throw new NullReferenceException("Load team death match with invalid GameMetaData");
		}
		_teamDeathMatchGameMode = new TeamDeathMatchGameMode(GameMetaData);
		GameModeUtil.OnEnterGameMode(_teamDeathMatchGameMode);
		TabScreenPanelGUI.Instance.SortPlayersByRank = TabScreenPlayerSorter.SortTeamMatchPlayers;
		Singleton<QuickItemController>.Instance.IsConsumptionEnabled = true;
		Singleton<QuickItemController>.Instance.Restriction.IsEnabled = true;
		Singleton<QuickItemController>.Instance.Restriction.RenewGameUses();
		CmuneEventHandler.AddListener<OnModeInitializedEvent>(OnModeStart);
		CmuneEventHandler.AddListener<OnMatchStartEvent>(OnMatchStart);
		CmuneEventHandler.AddListener<OnMatchEndEvent>(OnMatchEnd);
		CmuneEventHandler.AddListener<OnUpdateTeamScoreEvent>(OnUpdateTeamScore);
		CmuneEventHandler.AddListener<OnChangeTeamSuccessEvent>(TeamGameModeUtil.OnChangeTeamSuccess);
		CmuneEventHandler.AddListener<OnChangeTeamFailEvent>(TeamGameModeUtil.OnChangeTeamFail);
		CmuneEventHandler.AddListener<OnPlayerChangeTeamEvent>(OnPlayerChangeTeam);
		_stateMachine.SetState(18);
	}

	public void OnExit()
	{
		_stateMachine.PopAllStates();
		CmuneEventHandler.RemoveListener<OnModeInitializedEvent>(OnModeStart);
		CmuneEventHandler.RemoveListener<OnMatchStartEvent>(OnMatchStart);
		CmuneEventHandler.RemoveListener<OnMatchEndEvent>(OnMatchEnd);
		CmuneEventHandler.RemoveListener<OnUpdateTeamScoreEvent>(OnUpdateTeamScore);
		CmuneEventHandler.RemoveListener<OnChangeTeamSuccessEvent>(TeamGameModeUtil.OnChangeTeamSuccess);
		CmuneEventHandler.RemoveListener<OnChangeTeamFailEvent>(TeamGameModeUtil.OnChangeTeamFail);
		CmuneEventHandler.RemoveListener<OnPlayerChangeTeamEvent>(OnPlayerChangeTeam);
		GameModeUtil.OnExitGameMode();
		_teamDeathMatchGameMode = null;
	}

	public void OnUpdate()
	{
		Singleton<QuickItemController>.Instance.Update();
		if (_teamDeathMatchGameMode.IsMatchRunning)
		{
			_ingameCountdown.Update();
			TeamGameModeUtil.DetectTeamChange(_teamDeathMatchGameMode);
			GameModeUtil.UpdatePlayerStateMsg(_teamDeathMatchGameMode, true);
		}
		else
		{
			_ingameCountdown.Stop();
		}
		_stateMachine.Update();
	}

	public void OnGUI()
	{
	}

	private void OnModeStart(OnModeInitializedEvent ev)
	{
		Singleton<HudUtil>.Instance.SetPlayerTeam(GameState.LocalCharacter.TeamID);
		Singleton<GameModeObjectiveHud>.Instance.DisplayGameMode(GameMode.TeamDeathMatch);
		HudController.Instance.XpPtsHud.OnGameStart();
		Singleton<MatchStatusHud>.Instance.RemainingSeconds = 0;
		Singleton<InGameHelpHud>.Instance.EnableChangeTeamHelp = true;
		Singleton<FrameRateHud>.Instance.Enable = true;
	}

	private void OnMatchStart(OnMatchStartEvent ev)
	{
		_ingameCountdown.EndTime = ev.MatchEndServerTicks;
		_stateMachine.SetState(17);
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
		HudController.Instance.XpPtsHud.OnGameStart();
		Singleton<MatchStatusHud>.Instance.ResetKillsLeftAudio();
		Singleton<MatchStatusHud>.Instance.RemainingKills = _teamDeathMatchGameMode.GameData.SplatLimit;
		Singleton<PlayerLeadStatus>.Instance.ResetPlayerLead();
		Singleton<HudUtil>.Instance.SetTeamScore(0, 0);
	}

	private void OnMatchEnd(OnMatchEndEvent ev)
	{
		_stateMachine.SetState(20);
		int redTeamSplat = _teamDeathMatchGameMode.RedTeamSplat;
		int blueTeamSplat = _teamDeathMatchGameMode.BlueTeamSplat;
		if (redTeamSplat > blueTeamSplat)
		{
			Singleton<PopupHud>.Instance.PopupWinTeam(TeamID.RED);
		}
		else if (redTeamSplat < blueTeamSplat)
		{
			Singleton<PopupHud>.Instance.PopupWinTeam(TeamID.BLUE);
		}
		else
		{
			Singleton<PopupHud>.Instance.PopupWinTeam(TeamID.NONE);
		}
	}

	private void OnUpdateTeamScore(OnUpdateTeamScoreEvent ev)
	{
		int redScore = ev.RedScore;
		int blueScore = ev.BlueScore;
		bool isLeading = ev.IsLeading;
		int num = ((GameState.LocalCharacter.TeamID != TeamID.RED) ? blueScore : redScore);
		Singleton<MatchStatusHud>.Instance.RemainingKills = GameMetaData.SplatLimit - num;
		bool flag = !_teamDeathMatchGameMode.IsGameAboutToEnd && blueScore != GameMetaData.SplatLimit && redScore != GameMetaData.SplatLimit && _teamDeathMatchGameMode.RedTeamPlayerCount > 0 && _teamDeathMatchGameMode.BlueTeamPlayerCount > 0;
		if (flag)
		{
			Singleton<MatchStatusHud>.Instance.PlayKillsLeftAudio(GameMetaData.SplatLimit - Math.Max(redScore, blueScore));
		}
		switch (GameState.LocalCharacter.TeamID)
		{
		case TeamID.RED:
			Singleton<PlayerLeadStatus>.Instance.PlayLeadAudio(redScore, blueScore, isLeading, flag);
			break;
		case TeamID.BLUE:
			Singleton<PlayerLeadStatus>.Instance.PlayLeadAudio(blueScore, redScore, isLeading, flag);
			break;
		}
		Singleton<HudUtil>.Instance.SetTeamScore(blueScore, redScore);
	}

	private void OnPlayerChangeTeam(OnPlayerChangeTeamEvent ev)
	{
		TeamGameModeUtil.OnPlayerChangeTeam(_teamDeathMatchGameMode, ev.PlayerID, ev.PlayerInfo, ev.TargetTeamID);
	}
}
