public class HomePageScene : PageScene
{
	public override PageType PageType
	{
		get
		{
			return PageType.Home;
		}
	}

	protected override void OnLoad()
	{
		if ((bool)GameState.LocalAvatar.Decorator)
		{
			if ((bool)_avatarAnchor)
			{
				GameState.LocalAvatar.Decorator.SetPosition(_avatarAnchor.position, _avatarAnchor.rotation);
			}
			GameState.LocalAvatar.Decorator.HideWeapons();
			GameState.LocalAvatar.Decorator.HudInformation.SetAvatarLabel(PlayerDataManager.NameAndTag);
		}
		AutoMonoBehaviour<AvatarAnimationManager>.Instance.ResetAnimationState(PageType);
		Singleton<EventPopupManager>.Instance.ShowNextPopup(1);
	}
}
