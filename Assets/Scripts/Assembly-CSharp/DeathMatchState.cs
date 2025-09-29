using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class DeathMatchState : IState
{
	private const HudDrawFlags _hudDrawFlag = HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg;

	private DeathMatchGameMode _deathMatchGameMode;

	private InGameCountdown _ingameCountdown;

	private StateMachine _stateMachine;

	public GameMetaData GameMetaData { get; set; }

	public DeathMatchState()
	{
		_ingameCountdown = new InGameCountdown();
		_stateMachine = new StateMachine();
		_stateMachine.RegisterState(18, new InGamePregameLoadoutState());
		_stateMachine.RegisterState(17, new InGamePlayingState(_stateMachine, HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg));
		_stateMachine.RegisterState(20, new InGameEndOfMatchState());
		_stateMachine.RegisterState(22, new InGamePlayerKilledState(_stateMachine, HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg, true));
		_stateMachine.RegisterState(25, new InGamePlayerPausedState(_stateMachine));
		_stateMachine.RegisterState(21, new InGameSpectatingState(_stateMachine, HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg));
	}

	public void OnEnter()
	{
		if (GameMetaData == null)
		{
			throw new NullReferenceException("Load death match with invalid GameMetaData");
		}
		_deathMatchGameMode = new DeathMatchGameMode(GameMetaData);
		GameModeUtil.OnEnterGameMode(_deathMatchGameMode);
		TabScreenPanelGUI.Instance.SortPlayersByRank = TabScreenPlayerSorter.SortDeathMatchPlayers;
		Singleton<QuickItemController>.Instance.IsConsumptionEnabled = true;
		Singleton<QuickItemController>.Instance.Restriction.IsEnabled = true;
		Singleton<QuickItemController>.Instance.Restriction.RenewGameUses();
		CmuneEventHandler.AddListener<OnModeInitializedEvent>(OnModeStart);
		CmuneEventHandler.AddListener<OnMatchStartEvent>(OnMatchStart);
		CmuneEventHandler.AddListener<OnMatchEndEvent>(OnMatchEnd);
		CmuneEventHandler.AddListener<OnUpdateDeathMatchScoreEvent>(OnUpdateScore);
		_stateMachine.SetState(18);
	}

	public void OnExit()
	{
		_stateMachine.PopAllStates();
		CmuneEventHandler.RemoveListener<OnModeInitializedEvent>(OnModeStart);
		CmuneEventHandler.RemoveListener<OnMatchStartEvent>(OnMatchStart);
		CmuneEventHandler.RemoveListener<OnMatchEndEvent>(OnMatchEnd);
		CmuneEventHandler.RemoveListener<OnUpdateDeathMatchScoreEvent>(OnUpdateScore);
		GameModeUtil.OnExitGameMode();
		_deathMatchGameMode = null;
	}

	public void OnUpdate()
	{
		Singleton<QuickItemController>.Instance.Update();
		if (_deathMatchGameMode.IsMatchRunning)
		{
			_ingameCountdown.Update();
			GameModeUtil.UpdatePlayerStateMsg(_deathMatchGameMode, true);
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
		_stateMachine.SetState(17);
		Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.NONE);
		HudController.Instance.XpPtsHud.OnGameStart();
		Singleton<MatchStatusHud>.Instance.RemainingSeconds = 0;
		Singleton<GameModeObjectiveHud>.Instance.DisplayGameMode(GameMode.DeathMatch);
		Singleton<InGameHelpHud>.Instance.EnableChangeTeamHelp = false;
		Singleton<FrameRateHud>.Instance.Enable = true;
	}

	private void OnMatchStart(OnMatchStartEvent ev)
	{
		_ingameCountdown.EndTime = ev.MatchEndServerTicks;
		_stateMachine.SetState(17);
		Singleton<MatchStatusHud>.Instance.RemainingKills = GameState.CurrentGame.GameData.SplatLimit;
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
		HudController.Instance.XpPtsHud.OnGameStart();
		Singleton<MatchStatusHud>.Instance.ResetKillsLeftAudio();
	}

	private void OnMatchEnd(OnMatchEndEvent ev)
	{
		Singleton<PlayerLeadStatus>.Instance.OnDeathMatchOver();
		_stateMachine.SetState(20);
	}

	private void OnUpdateScore(OnUpdateDeathMatchScoreEvent ev)
	{
		int myScore = ev.MyScore;
		int otherPlayerScore = ev.OtherPlayerScore;
		bool isLeading = ev.IsLeading;
		int num = GameMetaData.SplatLimit - Mathf.Max(myScore, otherPlayerScore);
		bool flag = !_deathMatchGameMode.IsGameAboutToEnd && myScore != GameMetaData.SplatLimit && otherPlayerScore != GameMetaData.SplatLimit && _deathMatchGameMode.PlayerCount > 1;
		if (num != Singleton<MatchStatusHud>.Instance.RemainingKills)
		{
			Singleton<MatchStatusHud>.Instance.RemainingKills = num;
			if (flag)
			{
				Singleton<MatchStatusHud>.Instance.PlayKillsLeftAudio(GameMetaData.SplatLimit - Mathf.Max(otherPlayerScore, myScore));
			}
		}
		Singleton<PlayerLeadStatus>.Instance.PlayLeadAudio(myScore, otherPlayerScore, isLeading, flag);
	}
}
