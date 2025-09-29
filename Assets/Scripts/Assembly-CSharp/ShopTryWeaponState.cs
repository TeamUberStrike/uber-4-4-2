using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;

internal class ShopTryWeaponState : IState
{
	private ShopWeaponMode _shopGameMode;

	public int ItemId { get; set; }

	public void OnEnter()
	{
		CmuneEventHandler.AddListener<OnPlayerRespawnEvent>(OnPlayerRespawn);
		CmuneEventHandler.AddListener<OnMobileBackPressedEvent>(OnMobileBackPressed);
		CmuneEventHandler.AddListener<OnPlayerPauseEvent>(OnPlayerPause);
		_shopGameMode = new ShopWeaponMode(GameConnectionManager.Rmi);
		GameState.CurrentGame = _shopGameMode;
		Singleton<SpawnPointManager>.Instance.ConfigureSpawnPoints(GameState.CurrentSpace.SpawnPoints.GetComponentsInChildren<SpawnPoint>(true));
		LevelCamera.Instance.SetLevelCamera(GameState.CurrentSpace.Camera, GameState.CurrentSpace.DefaultViewPoint.position, GameState.CurrentSpace.DefaultViewPoint.rotation);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		GameState.LocalPlayer.SetEnabled(true);
		ShootingTargetBehaviour componentInChildren = GameState.CurrentSpace.GetComponentInChildren<ShootingTargetBehaviour>();
		if ((bool)componentInChildren)
		{
			componentInChildren.StartGame();
		}
		Singleton<QuickItemController>.Instance.IsEnabled = true;
		Singleton<QuickItemController>.Instance.IsConsumptionEnabled = false;
		Singleton<QuickItemController>.Instance.Restriction.IsEnabled = false;
		ProjectileManager.CreateContainer();
		_shopGameMode.InitializeMode();
		MenuPageManager.Instance.UnloadCurrentPage();
		HudController.Instance.XpPtsHud.OnGameStart();
		Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.NONE);
		Singleton<PlayerStateMsgHud>.Instance.DisplayNone();
		Singleton<FrameRateHud>.Instance.Enable = true;
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.XpPoints | HudDrawFlags.StateMsg;
		ShowShopMessages();
	}

	public void OnExit()
	{
		ShootingTargetBehaviour componentInChildren = GameState.CurrentSpace.GetComponentInChildren<ShootingTargetBehaviour>();
		if ((bool)componentInChildren)
		{
			componentInChildren.StopGame();
		}
		CmuneEventHandler.RemoveListener<OnPlayerRespawnEvent>(OnPlayerRespawn);
		CmuneEventHandler.RemoveListener<OnMobileBackPressedEvent>(OnMobileBackPressed);
		CmuneEventHandler.RemoveListener<OnPlayerPauseEvent>(OnPlayerPause);
		GameModeUtil.OnExitGameMode();
		_shopGameMode = null;
	}

	public void OnUpdate()
	{
		Singleton<QuickItemController>.Instance.Update();
	}

	private void Unload()
	{
		MenuPageManager.Instance.LoadPage(PageType.Shop, true);
		Singleton<GameStateController>.Instance.UnloadGameMode();
	}

	public void OnGUI()
	{
	}

	private void OnPlayerRespawn(OnPlayerRespawnEvent ev)
	{
		Singleton<WeaponController>.Instance.SetPickupWeapon(ItemId, false, true);
	}

	private void ShowShopMessages()
	{
		if (!ApplicationDataManager.IsMobile)
		{
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Empty);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.ShopTutorialMsg01);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.MessageQuickItemsTry);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.ShopTutorialMsg02, 20f);
		}
	}

	private void OnMobileBackPressed(OnMobileBackPressedEvent ev)
	{
		QuitTryWeapon();
	}

	private void OnPlayerPause(OnPlayerPauseEvent ev)
	{
		QuitTryWeapon();
	}

	private void QuitTryWeapon()
	{
		MenuPageManager.Instance.LoadPage(PageType.Shop, true);
		Singleton<GameStateController>.Instance.UnloadGameMode();
		if (ItemId > 0)
		{
			BuyPanelGUI buyPanelGUI = PanelManager.Instance.OpenPanel(PanelType.BuyItem) as BuyPanelGUI;
			if ((bool)buyPanelGUI)
			{
				buyPanelGUI.SetItem(Singleton<ItemManager>.Instance.GetItemInShop(ItemId), BuyingLocationType.Shop, BuyingRecommendationType.None);
			}
		}
		AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.SeletronRadioShort);
	}
}
