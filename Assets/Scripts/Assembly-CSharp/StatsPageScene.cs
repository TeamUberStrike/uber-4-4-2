public class StatsPageScene : PageScene
{
	public override PageType PageType
	{
		get
		{
			return PageType.Stats;
		}
	}

	protected override void OnLoad()
	{
		if ((bool)_avatarAnchor)
		{
			GameState.LocalAvatar.Decorator.SetPosition(_avatarAnchor.position, _avatarAnchor.rotation);
		}
	}
}
