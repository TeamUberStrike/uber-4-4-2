using UberStrike.Realtime.UnitySdk;

internal class InGameSpectatingState : IState
{
	private HudDrawFlags _gameModeFlag;

	private StateMachine _stateMachine;

	public InGameSpectatingState(StateMachine stateMachine, HudDrawFlags gameModeFlag)
	{
		_gameModeFlag = gameModeFlag;
		_stateMachine = stateMachine;
	}

	public void OnEnter()
	{
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag &= ~(HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle);
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag |= HudDrawFlags.InGameHelp;
		CmuneEventHandler.AddListener<OnPlayerPauseEvent>(OnPlayerPaused);
		Singleton<QuickItemController>.Instance.IsEnabled = false;
		if (GameState.LocalPlayer.IsGamePaused)
		{
			_stateMachine.PushState(25);
		}
	}

	public void OnExit()
	{
		GamePageManager.Instance.UnloadCurrentPage();
		CmuneEventHandler.RemoveListener<OnPlayerPauseEvent>(OnPlayerPaused);
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = _gameModeFlag;
	}

	public void OnUpdate()
	{
	}

	public void OnGUI()
	{
	}

	private void OnPlayerPaused(OnPlayerPauseEvent ev)
	{
		_stateMachine.PushState(25);
	}
}
