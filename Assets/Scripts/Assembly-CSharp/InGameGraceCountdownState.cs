internal class InGameGraceCountdownState : IState
{
	public void OnEnter()
	{
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.Score | HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.RoundTime | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.RemainingKill | HudDrawFlags.InGameChat | HudDrawFlags.StateMsg;
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
		Singleton<PlayerStateMsgHud>.Instance.DisplayNone();
		Singleton<PopupHud>.Instance.PopupRoundStart();
	}

	public void OnExit()
	{
	}

	public void OnUpdate()
	{
	}

	public void OnGUI()
	{
	}
}
