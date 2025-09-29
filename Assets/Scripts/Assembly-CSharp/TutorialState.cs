using UberStrike.Realtime.UnitySdk;

internal class TutorialState : IState
{
	private TutorialGameMode _tutorialGameMode;

	private StateMachine _stateMachine;

	public TutorialState()
	{
		_stateMachine = new StateMachine();
		_stateMachine.RegisterState(25, new InGamePlayerPausedState(_stateMachine));
	}

	public void OnEnter()
	{
		CmuneEventHandler.AddListener<OnMatchStartEvent>(OnMatchStart);
		CmuneEventHandler.AddListener<OnPlayerRespawnEvent>(OnPlayerRespawn);
		CmuneEventHandler.AddListener<OnPlayerPauseEvent>(OnPlayerPause);
		_tutorialGameMode = new TutorialGameMode(GameConnectionManager.Rmi);
		GameState.CurrentGame = _tutorialGameMode;
		TabScreenPanelGUI.Instance.SetGameName("Tutorial");
		TabScreenPanelGUI.Instance.SetServerName(string.Empty);
		LevelCamera.Instance.SetLevelCamera(GameState.CurrentSpace.Camera, GameState.CurrentSpace.DefaultViewPoint.position, GameState.CurrentSpace.DefaultViewPoint.rotation);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		GameState.LocalPlayer.SetEnabled(true);
		Singleton<FrameRateHud>.Instance.Enable = true;
		Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.NONE);
		Singleton<PlayerStateMsgHud>.Instance.DisplayNone();
	}

	public void OnExit()
	{
		CmuneEventHandler.RemoveListener<OnMatchStartEvent>(OnMatchStart);
		CmuneEventHandler.RemoveListener<OnPlayerRespawnEvent>(OnPlayerRespawn);
		CmuneEventHandler.RemoveListener<OnPlayerPauseEvent>(OnPlayerPause);
		GameModeUtil.OnExitGameMode();
		_tutorialGameMode = null;
	}

	public void OnUpdate()
	{
		if (_tutorialGameMode.IsMatchRunning)
		{
			GameModeUtil.UpdatePlayerStateMsg(_tutorialGameMode, false);
		}
	}

	public void OnGUI()
	{
		_tutorialGameMode.DrawGui();
	}

	private void OnMatchStart(OnMatchStartEvent ev)
	{
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
	}

	private void OnPlayerRespawn(OnPlayerRespawnEvent ev)
	{
		GamePageManager.Instance.UnloadCurrentPage();
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.None);
		if (_tutorialGameMode.Sequence.State == TutorialCinematicSequence.TutorialState.AirlockMouseLookSubtitle)
		{
			GameState.LocalPlayer.IsWalkingEnabled = false;
		}
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.XpPoints;
	}

	private void OnPlayerPause(OnPlayerPauseEvent ev)
	{
		_stateMachine.PushState(25);
	}
}
