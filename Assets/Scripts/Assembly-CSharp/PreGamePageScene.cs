public class PreGamePageScene : PageScene
{
	public override PageType PageType
	{
		get
		{
			return PageType.PreGame;
		}
	}

	protected override void OnLoad()
	{
		GamePageUtil.SpawnLocalAvatar();
		LevelCamera.Instance.SetMode(LevelCamera.CameraMode.Overview);
	}
}
