using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class GearTestState : IState
{
	private GearTestMode _testGameMode;

	private StateMachine _stateMachine;

	private HudDrawFlags _hudDrawFlags = HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.XpPoints | HudDrawFlags.StateMsg;

	private AvatarDecorator _testAvatar;

	public ILoadout Loadout { get; set; }

	public GearTestState(ILoadout loadout)
	{
		Loadout = loadout;
		_stateMachine = new StateMachine();
		_stateMachine.RegisterState(17, new InGamePlayingState(_stateMachine, _hudDrawFlags));
		_stateMachine.RegisterState(25, new InGamePlayerPausedState(_stateMachine));
	}

	public void OnEnter()
	{
		CmuneEventHandler.AddListener<OnPlayerPauseEvent>(OnPlayerPause);
		CmuneEventHandler.AddListener<OnPlayerUnpauseEvent>(OnPlayerUnpause);
		_testGameMode = new GearTestMode(GameConnectionManager.Rmi);
		GameState.CurrentGame = _testGameMode;
		Singleton<SpawnPointManager>.Instance.ConfigureSpawnPoints(GameState.CurrentSpace.SpawnPoints.GetComponentsInChildren<SpawnPoint>(true));
		LevelCamera.Instance.SetLevelCamera(GameState.CurrentSpace.Camera, GameState.CurrentSpace.DefaultViewPoint.position, GameState.CurrentSpace.DefaultViewPoint.rotation);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		GameState.LocalPlayer.SetEnabled(true);
		Singleton<QuickItemController>.Instance.IsEnabled = true;
		Singleton<QuickItemController>.Instance.IsConsumptionEnabled = false;
		Singleton<QuickItemController>.Instance.Restriction.IsEnabled = false;
		ProjectileManager.CreateContainer();
		_testGameMode.InitializeMode();
		MenuPageManager.Instance.UnloadCurrentPage();
		HudController.Instance.XpPtsHud.OnGameStart();
		Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.NONE);
		Singleton<PlayerStateMsgHud>.Instance.DisplayNone();
		Singleton<FrameRateHud>.Instance.Enable = true;
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.HealthArmor | HudDrawFlags.Ammo | HudDrawFlags.Weapons | HudDrawFlags.Reticle | HudDrawFlags.XpPoints | HudDrawFlags.StateMsg;
		_testAvatar = Singleton<AvatarBuilder>.Instance.CreateRemoteAvatar(Loadout.GetAvatarGear(), Color.white);
		_testAvatar.SetPosition(new Vector3(-18f, -2.35f, -2f), Quaternion.Euler(0f, 110f, 0f));
	}

	public void OnExit()
	{
		CmuneEventHandler.RemoveListener<OnPlayerPauseEvent>(OnPlayerPause);
		CmuneEventHandler.RemoveListener<OnPlayerUnpauseEvent>(OnPlayerUnpause);
		GameModeUtil.OnExitGameMode();
		_testGameMode = null;
	}

	public void OnUpdate()
	{
		Singleton<QuickItemController>.Instance.Update();
	}

	public void OnGUI()
	{
		if (GUI.Button(new Rect((Screen.width - 100) / 2, Screen.height / 4 * 3, 100f, 60f), "Ragdoll", StormFront.ButtonBlue) && !_testAvatar)
		{
		}
	}

	private void OnPlayerPause(OnPlayerPauseEvent ev)
	{
		_stateMachine.SetState(25);
	}

	private void OnPlayerUnpause(OnPlayerUnpauseEvent ev)
	{
		_stateMachine.SetState(17);
	}
}
