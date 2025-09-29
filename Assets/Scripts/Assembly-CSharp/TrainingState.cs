using UberStrike.Realtime.UnitySdk;

internal class TrainingState : IState
{
	private const HudDrawFlags _hudDrawFlag = HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.StateMsg;

	private TrainingFpsMode _trainingGameMode;

	private StateMachine _stateMachine;

	public int MapId { get; set; }

	public TrainingState()
	{
		_stateMachine = new StateMachine();
		_stateMachine.RegisterState(18, new InGamePregameLoadoutState());
		_stateMachine.RegisterState(17, new InGamePlayingState(_stateMachine, HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.StateMsg));
		_stateMachine.RegisterState(22, new InGamePlayerKilledState(_stateMachine, HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.XpPoints | HudDrawFlags.EventStream | HudDrawFlags.StateMsg, false));
		_stateMachine.RegisterState(25, new InGamePlayerPausedState(_stateMachine));
	}

	public void OnEnter()
	{
		_trainingGameMode = new TrainingFpsMode(GameConnectionManager.Rmi, MapId);
		GameState.CurrentGame = _trainingGameMode;
		LevelCamera.Instance.SetLevelCamera(GameState.CurrentSpace.Camera, GameState.CurrentSpace.DefaultViewPoint.position, GameState.CurrentSpace.DefaultViewPoint.rotation);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		GameState.LocalPlayer.SetEnabled(true);
		CmuneEventHandler.AddListener<OnModeInitializedEvent>(OnModeStart);
		Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.NONE);
		Singleton<QuickItemController>.Instance.IsConsumptionEnabled = false;
		Singleton<QuickItemController>.Instance.Restriction.IsEnabled = false;
		ProjectileManager.CreateContainer();
		_stateMachine.SetState(18);
	}

	public void OnExit()
	{
		_stateMachine.PopAllStates();
		CmuneEventHandler.RemoveListener<OnModeInitializedEvent>(OnModeStart);
		GameModeUtil.OnExitGameMode();
		_trainingGameMode = null;
	}

	public void OnUpdate()
	{
		if (_trainingGameMode.IsMatchRunning)
		{
			Singleton<QuickItemController>.Instance.Update();
			if (_trainingGameMode.IsWaitingForSpawn)
			{
				GameModeUtil.UpdateWaitingForSpawnMsg(_trainingGameMode, false);
			}
			_stateMachine.Update();
		}
	}

	public void OnGUI()
	{
		if (_trainingGameMode.IsMatchRunning)
		{
			_stateMachine.OnGUI();
		}
	}

	private void OnModeStart(OnModeInitializedEvent ev)
	{
		_stateMachine.SetState(17);
		GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
		Singleton<HudUtil>.Instance.ClearAllFeedbackHud();
		Singleton<FrameRateHud>.Instance.Enable = true;
		ShowTrainingGameMessages();
		Singleton<HudUtil>.Instance.AddInGameEvent(GameState.LocalCharacter.PlayerName, LocalizedStrings.EnteredTrainingMode);
	}

	private void ShowTrainingGameMessages()
	{
		if (!ApplicationDataManager.IsMobile)
		{
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Empty);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.TrainingTutorialMsg01);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.MessageQuickItemsTry);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.TrainingTutorialMsg03);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.TrainingTutorialMsg04);
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.TrainingTutorialMsg05, AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Forward), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Left), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Backward), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Right)));
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.TrainingTutorialMsg06, AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.PrimaryFire)));
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.TrainingTutorialMsg07, AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.NextWeapon), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.PrevWeapon)));
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.TrainingTutorialMsg08, AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.WeaponMelee), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Weapon1), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Weapon2), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Weapon3), AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Weapon4)));
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.TrainingTutorialMsg09, AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Crouch)));
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.TrainingTutorialMsg10, AutoMonoBehaviour<InputManager>.Instance.InputChannelForSlot(GameInputKey.Fullscreen)));
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.TrainingTutorialMsg11);
		}
	}
}
