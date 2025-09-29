public class ShopPageScene : PageScene
{
	public override PageType PageType
	{
		get
		{
			return PageType.Shop;
		}
	}

	protected override void OnLoad()
	{
		if (!GameState.HasCurrentGame)
		{
			if ((bool)_avatarAnchor)
			{
				GameState.LocalAvatar.Decorator.SetPosition(_avatarAnchor.position, _avatarAnchor.rotation);
			}
			AutoMonoBehaviour<AvatarAnimationManager>.Instance.ResetAnimationState(PageType);
			if (GameState.LocalAvatar.Decorator != null)
			{
				GameState.LocalAvatar.Decorator.HideWeapons();
			}
			if (GameState.HasCurrentGame && !GameState.LocalPlayer.IsGamePaused)
			{
				GameState.LocalPlayer.Pause();
			}
			Singleton<ArmorHud>.Instance.Enabled = true;
		}
	}

	protected override void OnUnload()
	{
		if (!GameState.HasCurrentGame)
		{
			Singleton<TemporaryLoadoutManager>.Instance.ResetGearLoadout();
			Singleton<ArmorHud>.Instance.Enabled = false;
		}
	}
}
