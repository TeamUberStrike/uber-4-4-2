internal class InGamePregameLoadoutState : IState
{
	public void OnEnter()
	{
		GamePageManager.Instance.LoadPage(PageType.PreGame);
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.XpPoints | HudDrawFlags.InGameChat;
		HudController.Instance.XpPtsHud.DisplayPermanently();
		HudController.Instance.XpPtsHud.ResetXp();
		HudController.Instance.XpPtsHud.IsXpPtsTextVisible = false;
		HudController.Instance.XpPtsHud.ResetTransform();
	}

	public void OnExit()
	{
		GamePageManager.Instance.UnloadCurrentPage();
	}

	public void OnUpdate()
	{
	}

	public void OnGUI()
	{
	}
}
