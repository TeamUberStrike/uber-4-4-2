using UberStrike.Realtime.UnitySdk;

internal class InGamePlayerPausedState : IState
{
	private const HudDrawFlags pauseDrawFlagTuning = ~(HudDrawFlags.Weapons | HudDrawFlags.Reticle);

	private StateMachine _stateMachine;

	public InGamePlayerPausedState(StateMachine stateMachine)
	{
		_stateMachine = stateMachine;
	}

	public void OnEnter()
	{
		Singleton<HudDrawFlagGroup>.Instance.AddFlag(~(HudDrawFlags.Weapons | HudDrawFlags.Reticle));
		Singleton<HudUtil>.Instance.ShowContinueButton();
		if (!ApplicationDataManager.IsMobile)
		{
			Singleton<WeaponsHud>.Instance.QuickItems.Expand();
		}
		CmuneEventHandler.AddListener<OnPlayerUnpauseEvent>(OnPlayerUnpaused);
	}

	public void OnExit()
	{
		Singleton<PlayerStateMsgHud>.Instance.ButtonEnabled = false;
		Singleton<HudDrawFlagGroup>.Instance.RemoveFlag(~(HudDrawFlags.Weapons | HudDrawFlags.Reticle));
		if (!ApplicationDataManager.IsMobile)
		{
			Singleton<WeaponsHud>.Instance.QuickItems.Collapse();
		}
		CmuneEventHandler.RemoveListener<OnPlayerUnpauseEvent>(OnPlayerUnpaused);
	}

	public void OnUpdate()
	{
	}

	public void OnGUI()
	{
	}

	private void OnPlayerUnpaused(OnPlayerUnpauseEvent ev)
	{
		_stateMachine.PopState();
	}
}
