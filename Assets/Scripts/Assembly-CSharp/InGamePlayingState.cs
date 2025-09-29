using UberStrike.Realtime.UnitySdk;

internal class InGamePlayingState : IState
{
	private const HudDrawFlags cameraZoomedDrawFlagTuning = ~HudDrawFlags.Weapons;

	private StateMachine _stateMachine;

	private KillComboCounter _killComboCounter;

	private HudDrawFlags _hudDrawFlag;

	public InGamePlayingState(StateMachine stateMachine, HudDrawFlags hudDrawFlag)
	{
		_stateMachine = stateMachine;
		_killComboCounter = new KillComboCounter();
		_hudDrawFlag = hudDrawFlag;
	}

	public void OnEnter()
	{
		Singleton<PlayerLeadStatus>.Instance.ResetPlayerLead();
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = _hudDrawFlag;
		_killComboCounter.ResetCounter();
		Singleton<QuickItemController>.Instance.IsEnabled = true;
		CmuneEventHandler.AddListener<OnPlayerKillEnemyEvent>(OnPlayerKillEnemy);
		CmuneEventHandler.AddListener<OnPlayerSuicideEvent>(GameModeUtil.OnPlayerSuicide);
		CmuneEventHandler.AddListener<OnPlayerKilledEvent>(GameModeUtil.OnPlayerKilled);
		CmuneEventHandler.AddListener<OnPlayerDamageEvent>(GameModeUtil.OnPlayerDamage);
		CmuneEventHandler.AddListener<OnPlayerDeadEvent>(OnPlayerDead);
		CmuneEventHandler.AddListener<OnPlayerPauseEvent>(OnPlayerPaused);
		CmuneEventHandler.AddListener<OnPlayerSpectatingEvent>(OnPlayerSpectating);
		CmuneEventHandler.AddListener<OnCameraZoomInEvent>(OnCameraZoomIn);
		CmuneEventHandler.AddListener<OnCameraZoomOutEvent>(OnCameraZoomOut);
		if (GameState.LocalPlayer.IsGamePaused)
		{
			_stateMachine.PushState(25);
		}
		Singleton<HudDrawFlagGroup>.Instance.RemoveFlag(~HudDrawFlags.Weapons);
	}

	public void OnExit()
	{
		Singleton<QuickItemController>.Instance.IsEnabled = false;
		CmuneEventHandler.RemoveListener<OnPlayerKillEnemyEvent>(OnPlayerKillEnemy);
		CmuneEventHandler.RemoveListener<OnPlayerSuicideEvent>(GameModeUtil.OnPlayerSuicide);
		CmuneEventHandler.RemoveListener<OnPlayerKilledEvent>(GameModeUtil.OnPlayerKilled);
		CmuneEventHandler.RemoveListener<OnPlayerDamageEvent>(GameModeUtil.OnPlayerDamage);
		CmuneEventHandler.RemoveListener<OnPlayerDeadEvent>(OnPlayerDead);
		CmuneEventHandler.RemoveListener<OnPlayerPauseEvent>(OnPlayerPaused);
		CmuneEventHandler.RemoveListener<OnPlayerSpectatingEvent>(OnPlayerSpectating);
		CmuneEventHandler.RemoveListener<OnCameraZoomInEvent>(OnCameraZoomIn);
		CmuneEventHandler.RemoveListener<OnCameraZoomOutEvent>(OnCameraZoomOut);
	}

	public void OnUpdate()
	{
	}

	public void OnGUI()
	{
	}

	private void OnPlayerKillEnemy(OnPlayerKillEnemyEvent ev)
	{
		_killComboCounter.OnKillEnemy();
		GameModeUtil.OnPlayerKillEnemy(ev);
	}

	private void OnPlayerPaused(OnPlayerPauseEvent ev)
	{
		_stateMachine.PushState(25);
	}

	private void OnPlayerSpectating(OnPlayerSpectatingEvent ev)
	{
		_stateMachine.SetState(21);
	}

	private void OnPlayerDead(OnPlayerDeadEvent ev)
	{
		_stateMachine.PushState(22);
	}

	private void OnCameraZoomIn(OnCameraZoomInEvent ev)
	{
		Singleton<HudDrawFlagGroup>.Instance.AddFlag(~HudDrawFlags.Weapons);
	}

	private void OnCameraZoomOut(OnCameraZoomOutEvent ev)
	{
		Singleton<HudDrawFlagGroup>.Instance.RemoveFlag(~HudDrawFlags.Weapons);
	}
}
