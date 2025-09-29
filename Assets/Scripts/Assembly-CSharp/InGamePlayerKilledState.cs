using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class InGamePlayerKilledState : IState
{
	private HudDrawFlags _gameModeFlag;

	private bool _showInGameHelp;

	private StateMachine _stateMachine;

	public InGamePlayerKilledState(StateMachine stateMachine, HudDrawFlags gameModeFlag, bool showInGameHelp)
	{
		_gameModeFlag = gameModeFlag;
		_showInGameHelp = showInGameHelp;
		_stateMachine = stateMachine;
	}

	public void OnEnter()
	{
		Singleton<GameModeObjectiveHud>.Instance.Clear();
		Singleton<InGameFeatHud>.Instance.AnimationScheduler.ClearAll();
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag &= ~(HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.EventStream);
		if (_showInGameHelp)
		{
			Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag |= HudDrawFlags.InGameHelp;
		}
		Screen.lockCursor = false;
		Singleton<QuickItemController>.Instance.IsEnabled = false;
		CmuneEventHandler.AddListener<OnPlayerRespawnEvent>(OnPlayerRespawn);
	}

	public void OnExit()
	{
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = _gameModeFlag;
		GamePageManager.Instance.UnloadCurrentPage();
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
		Singleton<PlayerStateMsgHud>.Instance.ButtonEnabled = false;
		Singleton<QuickItemController>.Instance.IsEnabled = true;
		CmuneEventHandler.RemoveListener<OnPlayerRespawnEvent>(OnPlayerRespawn);
	}

	public void OnUpdate()
	{
	}

	public void OnGUI()
	{
	}

	private void OnPlayerRespawn(OnPlayerRespawnEvent ev)
	{
		_stateMachine.PopState();
	}
}
