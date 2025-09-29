using System.Collections.Generic;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class TouchInput : AutoMonoBehaviour<TouchInput>
{
	public enum TouchKeyType
	{
		None = 0,
		Look = 1,
		Move = 2,
		PrimaryFire = 3,
		SecondaryFire = 4,
		Forward = 5,
		Backward = 6,
		Left = 7,
		Right = 8,
		Jump = 9,
		Crouch = 10,
		Zoom = 11,
		Score = 12,
		Menu = 13,
		Chat = 14,
		Loadout = 15,
		ChangeWeaponMode = 16,
		ChangeTeam = 17,
		FreeMove = 18
	}

	public enum TouchState
	{
		None = 0,
		Playing = 1,
		Chatting = 2,
		Sniping = 3,
		Death = 4,
		Paused = 5,
		Scoreboard = 6,
		Spectator = 7,
		Testing = 8
	}

	private class TouchStateNone : IState
	{
		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
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

	private class TouchStatePlaying : IState
	{
		private bool _walkingEnabled;

		private bool _inputEnabled;

		public void OnEnter()
		{
			if (UseMultiTouch && !AutoMonoBehaviour<TouchInput>.Instance.HasDisplayedFireHelp)
			{
				AutoMonoBehaviour<TouchInput>.Instance.CheckIdleTime = true;
				AutoMonoBehaviour<TouchInput>.Instance.LastFireTime = Time.time;
			}
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = true;
			if (Singleton<GameStateController>.Instance.StateMachine.CurrentStateId == 14)
			{
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = true;
				AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = Singleton<WeaponController>.Instance.HasAnyWeapon;
				AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.UpdateConsumablesHeld();
				AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = true;
			}
			else if (Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 12)
			{
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = true;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
				AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = Singleton<WeaponController>.Instance.HasAnyWeapon;
				AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.UpdateConsumablesHeld();
				AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = true;
			}
			else
			{
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			}
			UpdateWalkingEnabled();
			Vector2 position = Singleton<WeaponsHud>.Instance.QuickItems.Group.GetPosition();
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Boundary = new Rect(position.x, position.y, 63f, 60f);
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.IgnoreRect(AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Boundary);
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = true;
		}

		public void OnExit()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Reset();
			WishLook = Vector2.zero;
			WishDirection = Vector2.zero;
		}

		private void UpdateWalkingEnabled()
		{
			_walkingEnabled = GameState.LocalPlayer.IsWalkingEnabled;
			_inputEnabled = AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled;
			if (_walkingEnabled && _inputEnabled)
			{
				WeaponSlot currentWeapon = Singleton<WeaponController>.Instance.GetCurrentWeapon();
				if (UseMultiTouch)
				{
					AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = true;
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
					AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
				}
				else
				{
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = currentWeapon != null;
					AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = true;
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = true;
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = true;
					((TouchButtonCircle)AutoMonoBehaviour<TouchInput>.Instance.Buttons[10]).ShowHighlight = WishCrouch;
					AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
				}
				if (currentWeapon != null)
				{
					AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = currentWeapon.View.WeaponSecondaryAction != 0;
				}
			}
			else
			{
				AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			}
		}

		public void OnUpdate()
		{
			if (_walkingEnabled != GameState.LocalPlayer.IsWalkingEnabled)
			{
				UpdateWalkingEnabled();
			}
			if (_inputEnabled != AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled)
			{
				UpdateWalkingEnabled();
			}
			float num = 1f;
			if (IsFiring)
			{
				num = 0.75f;
			}
			if (ApplicationDataManager.ApplicationOptions.UseMultiTouch)
			{
				WishDirection = AutoMonoBehaviour<TouchInput>.Instance.Dpad.Direction;
			}
			Vector2 vector = AutoMonoBehaviour<TouchInput>.Instance.Shooter.Aim * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * num;
			WishLook.x = Mathf.Lerp(WishLook.x, vector.x, Time.deltaTime * AutoMonoBehaviour<TouchInput>.Instance.lookInteriaRolloff.x);
			WishLook.y = Mathf.Lerp(WishLook.y, vector.y, Time.deltaTime * AutoMonoBehaviour<TouchInput>.Instance.lookInteriaRolloff.y);
		}

		public void OnGUI()
		{
		}
	}

	private class TouchStateTesting : IState
	{
		public void OnEnter()
		{
			Debug.Log("Entering testing");
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = true;
			((TouchButtonCircle)AutoMonoBehaviour<TouchInput>.Instance.Buttons[10]).ShowHighlight = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = true;
		}

		public void OnExit()
		{
			Debug.Log("Exiting testing");
			AutoMonoBehaviour<TouchInput>.Instance.Reset();
		}

		public void OnUpdate()
		{
		}

		public void OnGUI()
		{
		}
	}

	private class TouchStateChatting : IState
	{
		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
		}

		public void OnExit()
		{
		}

		public void OnUpdate()
		{
			AutoMonoBehaviour<TouchInput>.Instance.CheckKeyboardDone();
		}

		public void OnGUI()
		{
		}
	}

	private class TouchStateSniping : IState
	{
		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Follower.Enabled = false;
			Singleton<WeaponsHud>.Instance.SetEnabled(false);
			if (UseMultiTouch)
			{
				AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = true;
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
				AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
			}
			else
			{
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = true;
				AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = true;
				AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			}
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = true;
			UberStrikeItemWeaponView view = Singleton<WeaponController>.Instance.GetCurrentWeapon().View;
			ZoomInfo zoomInfo = new ZoomInfo(view.DefaultZoomMultiplier, view.MinZoomMultiplier, view.MaxZoomMultiplier);
			if (zoomInfo.DefaultMultiplier != 1f && zoomInfo.MaxMultiplier != zoomInfo.MinMultiplier)
			{
				AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = true;
			}
			else
			{
				AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			}
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.IgnoreRect(AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Boundary);
		}

		public void OnExit()
		{
			WishLook = Vector2.zero;
			WishDirection = Vector2.zero;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.UnignoreRect(AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Boundary);
			Singleton<WeaponsHud>.Instance.SetEnabled(true);
		}

		public void OnUpdate()
		{
			if (ApplicationDataManager.ApplicationOptions.UseMultiTouch)
			{
				WishDirection = AutoMonoBehaviour<TouchInput>.Instance.Dpad.Direction;
			}
			float num = 0.5f;
			Vector2 vector = AutoMonoBehaviour<TouchInput>.Instance.Shooter.Aim * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * num;
			WishLook.x = Mathf.Lerp(WishLook.x, vector.x, Time.deltaTime * AutoMonoBehaviour<TouchInput>.Instance.lookInteriaRolloff.x);
			WishLook.y = Mathf.Lerp(WishLook.y, vector.y, Time.deltaTime * AutoMonoBehaviour<TouchInput>.Instance.lookInteriaRolloff.y);
		}

		public void OnGUI()
		{
		}
	}

	private class TouchStatePaused : IState
	{
		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = !AutoMonoBehaviour<TouchInput>.Instance.TeamAlreadyChanged && GameState.CurrentGameMode == GameMode.TeamDeathMatch;
			AutoMonoBehaviour<TouchInput>.Instance.Follower.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
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

	private class TouchStateDead : IState
	{
		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.TeamAlreadyChanged = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = !GlobalUIRibbon.Instance.enabled;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = GameState.CurrentGameMode == GameMode.TeamDeathMatch;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
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

	private class TouchStateScoreboard : IState
	{
		private bool hasWeaponsDrawn;

		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
			WishLook = Vector2.zero;
			if ((HudDrawFlags.Weapons & Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag) == HudDrawFlags.Weapons)
			{
				Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag & ~HudDrawFlags.Weapons;
				hasWeaponsDrawn = true;
			}
			if (UseMultiTouch)
			{
				AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = !GameState.LocalPlayer.IsDead && !GameState.LocalPlayer.IsGamePaused;
				AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
			}
			else
			{
				AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = !GameState.LocalPlayer.IsDead && !GameState.LocalPlayer.IsGamePaused;
				AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			}
		}

		public void OnExit()
		{
			if (hasWeaponsDrawn)
			{
				Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag | HudDrawFlags.Weapons;
			}
			WishDirection = Vector2.zero;
		}

		public void OnUpdate()
		{
			if (ApplicationDataManager.ApplicationOptions.UseMultiTouch)
			{
				WishDirection = AutoMonoBehaviour<TouchInput>.Instance.Dpad.Direction;
			}
		}

		public void OnGUI()
		{
		}
	}

	private class TouchStateSpectator : IState
	{
		public void OnEnter()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[13].Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[14].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[12].Enabled = Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 13;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[15].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[17].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[10].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[9].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ConsumableChanger.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ScopeSwipe.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Shooter.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Follower.Enabled = true;
			AutoMonoBehaviour<TouchInput>.Instance.Dpad.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Joystick.Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.AimHelpText.IsEnabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.ShootHelpText.IsEnabled = false;
		}

		public void OnExit()
		{
			AutoMonoBehaviour<TouchInput>.Instance.Follower.Enabled = false;
		}

		public void OnUpdate()
		{
			if (ApplicationDataManager.ApplicationOptions.UseMultiTouch)
			{
				WishDirection = AutoMonoBehaviour<TouchInput>.Instance.Dpad.Direction;
			}
			float num = 0.5f;
			Vector2 vector = AutoMonoBehaviour<TouchInput>.Instance.Follower.Aim * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * num;
			WishLook.x = Mathf.Lerp(WishLook.x, vector.x, Time.deltaTime * AutoMonoBehaviour<TouchInput>.Instance.lookInteriaRolloff.x);
			WishLook.y = Mathf.Lerp(WishLook.y, vector.y, Time.deltaTime * AutoMonoBehaviour<TouchInput>.Instance.lookInteriaRolloff.y);
		}

		public void OnGUI()
		{
		}
	}

	private const float leftSideRatio = 0.4f;

	public static Vector2 WishLook;

	public static Vector2 WishDirection;

	public static bool WishJump;

	public static bool WishCrouch;

	public static bool IsFiring;

	public Dictionary<int, TouchButton> Buttons;

	public TouchDPad Dpad;

	public TouchFollow Follower;

	public TouchShooter Shooter;

	public TouchJoystick Joystick;

	public TouchSwipeBar ScopeSwipe;

	public TouchWeaponChanger WeaponChanger;

	public TouchConsumableChanger ConsumableChanger;

	public MeshGUIText AimHelpText;

	public MeshGUIText ShootHelpText;

	private Vector2 lookInteriaRolloff = new Vector2(10f, 12f);

	public static bool UseMultiTouch;

	public static bool DisableIdleTimer;

	public float IdleTimeBeforeHelp = 10f;

	private Rect _screenLeftRect;

	private Vector2 _firePos;

	private Vector2 _secondFirePos;

	private Vector2 _jumpPos;

	private Vector2 _crouchPos;

	private Rect _joystickRect;

	private Vector2 _nextWeaponPos;

	private Rect _scopeSwipeRect;

	private Vector2 _scoreButtonPos;

	private Vector2 _backButtonPos;

	private Vector2 _chatPos;

	private Rect _loadoutRect;

	private Rect _changeWeaponModeRect;

	private Rect _changeTeamRect;

	private bool _toggleSecondaryFire;

	private bool _playerMoving;

	public float LastFireTime;

	public bool HasDisplayedFireHelp;

	public bool CheckIdleTime;

	public bool HasDisplayedFiveFingerHelp;

	private float _lastPaused;

	private UberstrikeItemClass _currWeapon;

	private TouchScreenKeyboard _keyboard;

	private StateMachine _stateMachine;

	private TouchState _previousState;

	public bool TeamAlreadyChanged { get; set; }

	public void CheckKeyboardDone()
	{
		if (_keyboard != null && _keyboard.done && !_keyboard.wasCanceled)
		{
			Singleton<InGameChatHud>.Instance.PushMessage(_keyboard.text);
			_keyboard = null;
			_stateMachine.SetState((int)_previousState);
		}
		else if (_keyboard != null && !_keyboard.active)
		{
			_keyboard = null;
			_stateMachine.SetState((int)_previousState);
		}
	}

	private new void Start()
	{
		UseMultiTouch = ApplicationDataManager.ApplicationOptions.UseMultiTouch;
		_screenLeftRect = new Rect(0f, 0f, (float)Screen.width * 0.4f, Screen.height);
		_currWeapon = (UberstrikeItemClass)0;
		IsFiring = false;
		SetupRects();
		Buttons = new Dictionary<int, TouchButton>();
		TouchButtonCircle touchButtonCircle = new TouchButtonCircle(MobileIcons.TouchMenuButton);
		touchButtonCircle.CenterPosition = _backButtonPos;
		touchButtonCircle.OnPushed += OnMenu;
		touchButtonCircle.ShowEffect = false;
		Buttons.Add(13, touchButtonCircle);
		TouchButtonCircle touchButtonCircle2 = new TouchButtonCircle(MobileIcons.TouchChatButton);
		touchButtonCircle2.CenterPosition = _chatPos;
		touchButtonCircle2.OnPushed += OnChatBegan;
		touchButtonCircle2.ShowEffect = false;
		Buttons.Add(14, touchButtonCircle2);
		TouchButtonCircle touchButtonCircle3 = new TouchButtonCircle(MobileIcons.TouchScoreboardButton);
		touchButtonCircle3.CenterPosition = _scoreButtonPos;
		touchButtonCircle3.OnTouchBegan += OnScoreTouchBegan;
		touchButtonCircle3.OnTouchEnded += OnScoreTouchEnd;
		touchButtonCircle3.ShowEffect = false;
		touchButtonCircle3.MinGUIAlpha = 0.5f;
		Buttons.Add(12, touchButtonCircle3);
		WeaponChanger = new TouchWeaponChanger(new Rect[9]
		{
			MobileIcons.TouchWeaponMelee,
			MobileIcons.TouchWeaponMelee,
			MobileIcons.TouchWeaponHandgun,
			MobileIcons.TouchWeaponMachinegun,
			MobileIcons.TouchWeaponShotgun,
			MobileIcons.TouchWeaponSniperrifle,
			MobileIcons.TouchWeaponCannon,
			MobileIcons.TouchWeaponSplattergun,
			MobileIcons.TouchWeaponLauncher
		});
		WeaponChanger.Position = _nextWeaponPos;
		WeaponChanger.OnNextWeapon += OnNextWeapon;
		WeaponChanger.OnPrevWeapon += OnPrevWeapon;
		ConsumableChanger = new TouchConsumableChanger();
		ConsumableChanger.OnNextConsumable += OnNextConsumable;
		ConsumableChanger.OnPrevConsumable += OnPrevConsumable;
		ConsumableChanger.OnStartUseConsumable += OnStartUseConsumable;
		ConsumableChanger.OnEndUseConsumable += OnEndUseConsumable;
		ScopeSwipe = new TouchSwipeBar();
		ScopeSwipe.Boundary = _scopeSwipeRect;
		ScopeSwipe.OnSwipeUp += OnScopeUp;
		ScopeSwipe.OnSwipeDown += OnScopeDown;
		Dpad = new TouchDPad(MobileIcons.TouchKeyboardDpad);
		Dpad.TopLeftPosition = new Vector2(75f, Screen.height - 300);
		Dpad.Rotation = -15f;
		Dpad.JumpButton.OnTouchBegan += OnJump;
		Dpad.JumpButton.OnTouchEnded += OnJumpTouchEnded;
		Dpad.CrouchButton.OnTouchBegan += OnCrouchBegan;
		Dpad.CrouchButton.OnTouchEnded += OnCrouchEnded;
		TouchButtonCircle touchButtonCircle4 = new TouchButtonCircle(MobileIcons.TouchJumpButton);
		touchButtonCircle4.CenterPosition = _jumpPos;
		touchButtonCircle4.OnTouchBegan += OnFreeMove;
		touchButtonCircle4.OnTouchEnded += OnFreeMoveEnded;
		touchButtonCircle4.MinGUIAlpha = 1f;
		touchButtonCircle4.Enabled = false;
		Buttons.Add(18, touchButtonCircle4);
		TouchButtonCircle touchButtonCircle5 = new TouchButtonCircle(MobileIcons.TouchJumpButton);
		touchButtonCircle5.CenterPosition = _jumpPos;
		touchButtonCircle5.OnTouchBegan += OnJump;
		touchButtonCircle5.OnTouchEnded += OnJumpTouchEnded;
		touchButtonCircle5.MinGUIAlpha = 1f;
		Buttons.Add(9, touchButtonCircle5);
		TouchButtonCircle touchButtonCircle6 = new TouchButtonCircle(MobileIcons.TouchCrouchButton, MobileIcons.TouchCrouchButtonActive);
		touchButtonCircle6.CenterPosition = _crouchPos;
		touchButtonCircle6.OnTouchBegan += OnCrouchPushed;
		touchButtonCircle6.MinGUIAlpha = 1f;
		Buttons.Add(10, touchButtonCircle6);
		Joystick = new TouchJoystick();
		Joystick.Boundary = _joystickRect;
		Joystick.OnJoystickMoved += OnJoystickMoved;
		Joystick.OnJoystickStopped += OnJoystickStopped;
		TouchButtonCircle touchButtonCircle7 = new TouchButtonCircle(MobileIcons.TouchFireButton);
		touchButtonCircle7.CenterPosition = _firePos;
		touchButtonCircle7.OnTouchBegan += OnFireTouchBegan;
		touchButtonCircle7.OnTouchEnded += OnFireTouchEnded;
		touchButtonCircle7.MinGUIAlpha = 1f;
		Buttons.Add(3, touchButtonCircle7);
		TouchButtonCircle touchButtonCircle8 = new TouchButtonCircle(MobileIcons.TouchSecondFireButton);
		touchButtonCircle8.CenterPosition = _secondFirePos;
		touchButtonCircle8.OnTouchBegan += OnSecondaryFireTouchBegan;
		touchButtonCircle8.MinGUIAlpha = 1f;
		touchButtonCircle8.ShowEffect = false;
		Buttons.Add(4, touchButtonCircle8);
		TouchButton touchButton = new TouchButton("Loadout", StormFront.ButtonBlue);
		touchButton.Boundary = _loadoutRect;
		touchButton.OnPushed += OnLoadoutPushed;
		Buttons.Add(15, touchButton);
		string title = ((!UseMultiTouch) ? "Use Multi-touch Input" : "Use Simple Input");
		TouchButton touchButton2 = new TouchButton(title, StormFront.ButtonBlue);
		touchButton2.Boundary = _changeWeaponModeRect;
		touchButton2.OnPushed += OnModeChangePushed;
		Buttons.Add(16, touchButton2);
		TouchButton changeTeam = new TouchButton("Change team", StormFront.ButtonBlue);
		changeTeam.Boundary = _changeTeamRect;
		changeTeam.OnPushed += delegate
		{
			changeTeam.Enabled = false;
			TeamAlreadyChanged = true;
		};
		changeTeam.OnPushed += OnChangeTeamPushed;
		Buttons.Add(17, changeTeam);
		Follower = new TouchFollow();
		Follower.OnFired += OnFollowerStart;
		Follower.IgnoreRect(Joystick.Boundary);
		Follower.IgnoreRect(new Rect(0f, touchButtonCircle.Boundary.y, touchButtonCircle3.Boundary.width, touchButtonCircle3.Boundary.yMax - touchButtonCircle.Boundary.y));
		Shooter = new TouchShooter();
		Shooter.Boundary = new Rect(0f, 0f, Screen.width, Screen.height);
		if (UseMultiTouch)
		{
			Shooter.OnFireStart += OnFireStart;
			Shooter.OnFireEnd += OnFireEnd;
		}
		Shooter.IgnoreRect(WeaponChanger.Boundary);
		Shooter.IgnoreRect(Joystick.Boundary);
		Shooter.IgnoreRect(new Rect(0f, touchButtonCircle.Boundary.y, touchButtonCircle3.Boundary.width, touchButtonCircle3.Boundary.yMax - touchButtonCircle.Boundary.y));
		AimHelpText = new MeshGUIText("Drag finger to aim", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(AimHelpText);
		AimHelpText.Position = new Vector2(Screen.width - 300, Screen.height - 150);
		AimHelpText.Scale = new Vector2(0.6f, 0.6f);
		AimHelpText.Alpha = 0f;
		ShootHelpText = new MeshGUIText("Tap second finger to shoot", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(ShootHelpText);
		ShootHelpText.Position = new Vector2(Screen.width - 300, Screen.height - 100);
		ShootHelpText.Scale = new Vector2(0.6f, 0.6f);
		ShootHelpText.Alpha = 0f;
		_stateMachine = new StateMachine();
		_stateMachine.RegisterState(0, new TouchStateNone());
		_stateMachine.RegisterState(1, new TouchStatePlaying());
		_stateMachine.RegisterState(3, new TouchStateSniping());
		_stateMachine.RegisterState(2, new TouchStateChatting());
		_stateMachine.RegisterState(5, new TouchStatePaused());
		_stateMachine.RegisterState(4, new TouchStateDead());
		_stateMachine.RegisterState(6, new TouchStateScoreboard());
		_stateMachine.RegisterState(7, new TouchStateSpectator());
		_stateMachine.RegisterState(8, new TouchStateTesting());
		_stateMachine.SetState(0);
	}

	private void OnFollowerStart()
	{
		Singleton<PlayerSpectatorControl>.Instance.FollowNextPlayer();
	}

	public void EnablePerformanceChecker()
	{
		_stateMachine.PushState(8);
	}

	public void DisablePerformanceChecker()
	{
		_stateMachine.PopState();
	}

	private void Reset()
	{
		_currWeapon = (UberstrikeItemClass)0;
		WeaponChanger.Reset();
	}

	private void OnChangeTeamPushed()
	{
		if (GameState.CurrentGameMode == GameMode.TeamDeathMatch)
		{
			TeamDeathMatchGameMode teamDeathMatchGameMode = GameState.CurrentGame as TeamDeathMatchGameMode;
			teamDeathMatchGameMode.ChangeTeam();
		}
	}

	private void OnIdleTime()
	{
		if (ApplicationDataManager.ApplicationOptions.UseMultiTouch)
		{
			ShootHelpText.FadeAlphaTo(1f, 1f);
			AimHelpText.FadeAlphaTo(1f, 1f);
			CheckIdleTime = false;
		}
	}

	private void OnStartUseConsumable()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.UseQuickItem, 1f));
	}

	private void OnEndUseConsumable()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.UseQuickItem, 0f));
	}

	private void OnPrevConsumable()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrevQuickItem, 1f));
	}

	private void OnNextConsumable()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.NextQuickItem, 1f));
	}

	private void OnJoystickStopped()
	{
		WishDirection = Vector2.zero;
		_playerMoving = false;
	}

	private void OnJoystickMoved(Vector2 dir)
	{
		if (!_playerMoving)
		{
			_playerMoving = true;
		}
		WishDirection = dir;
	}

	private void OnScoreTouchEnd(Vector2 obj)
	{
		_stateMachine.PopState();
		_stateMachine.PushState((int)_previousState);
		TabScreenPanelGUI.Instance.ForceShow = false;
	}

	private void OnScoreTouchBegan(Vector2 obj)
	{
		_previousState = (TouchState)_stateMachine.CurrentStateId;
		_stateMachine.PushState(6);
		TabScreenPanelGUI.Instance.ForceShow = true;
	}

	private void OnWeaponChanged()
	{
		if (_stateMachine.CurrentStateId == 7)
		{
			return;
		}
		AutoMonoBehaviour<TouchInput>.Instance.WeaponChanger.CheckWeaponChanged();
		WeaponSlot currentWeapon = Singleton<WeaponController>.Instance.GetCurrentWeapon();
		if (currentWeapon != null)
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = !UseMultiTouch && !GameState.LocalPlayer.IsDead;
			if (currentWeapon.View.WeaponSecondaryAction != 0 && (_stateMachine.CurrentStateId == 1 || _stateMachine.CurrentStateId == 3))
			{
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = true;
			}
			else
			{
				AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
			}
		}
		else
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[3].Enabled = false;
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[4].Enabled = false;
		}
	}

	private void SetupRects()
	{
		_joystickRect = new Rect(0f, Screen.height / 2, _screenLeftRect.width, Screen.height / 2);
		_backButtonPos = new Vector2(30f, 200f);
		_chatPos = new Vector2(30f, 260f);
		_scoreButtonPos = new Vector2(30f, 320f);
		_nextWeaponPos = new Vector2(Screen.width - 95, 105f);
		_scopeSwipeRect = new Rect((float)Screen.width - MobileIcons.TouchZoomScrollbar.width * (float)MobileIcons.TextureAtlas.mainTexture.width - 24f, (float)(Screen.height - 274) - MobileIcons.TouchZoomScrollbar.height * (float)MobileIcons.TextureAtlas.mainTexture.height, MobileIcons.TouchZoomScrollbar.width * (float)MobileIcons.TextureAtlas.mainTexture.width + 24f, MobileIcons.TouchZoomScrollbar.height * (float)MobileIcons.TextureAtlas.mainTexture.height);
		_firePos = new Vector2(Screen.width - 160, Screen.height - 170);
		_secondFirePos = new Vector2(Screen.width - 64, Screen.height - 234);
		_jumpPos = new Vector2(Screen.width - 88, Screen.height - 72);
		_crouchPos = new Vector2(Screen.width - 255, Screen.height - 116);
		_loadoutRect = new Rect(20f, (float)Screen.height * 0.6f, 200f, 50f);
		_changeWeaponModeRect = new Rect(20f, (float)Screen.height * 0.7f, 200f, 50f);
		_changeTeamRect = new Rect(20f, (float)Screen.height * 0.8f, 200f, 50f);
	}

	private void Update()
	{
		_stateMachine.Update();
		if (Input.GetKeyDown(KeyCode.Escape) && Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 12)
		{
			CmuneEventHandler.Route(new OnMobileBackPressedEvent());
		}
		if (!GameState.HasCurrentGame)
		{
			if (_stateMachine.CurrentStateId != 0 && _stateMachine.CurrentStateId != 8)
			{
				_stateMachine.SetState(0);
				CheckIdleTime = false;
				ShootHelpText.Alpha = 0f;
				AimHelpText.Alpha = 0f;
			}
			return;
		}
		if (CheckIdleTime && !HasDisplayedFireHelp && LastFireTime + IdleTimeBeforeHelp < Time.time)
		{
			OnIdleTime();
		}
		AimHelpText.Draw();
		ShootHelpText.Draw();
		if (_playerMoving || Dpad.Moving)
		{
			Singleton<TouchController>.Instance.GUIAlpha = Mathf.Lerp(Singleton<TouchController>.Instance.GUIAlpha, 0f, Time.deltaTime * 4f);
		}
		else
		{
			Singleton<TouchController>.Instance.GUIAlpha = Mathf.Lerp(Singleton<TouchController>.Instance.GUIAlpha, 1f, Time.deltaTime * 2f);
		}
		WeaponSlot currentWeapon = Singleton<WeaponController>.Instance.GetCurrentWeapon();
		if (currentWeapon != null && currentWeapon.View.ItemClass != _currWeapon)
		{
			OnWeaponChanged();
			_currWeapon = currentWeapon.View.ItemClass;
		}
		if (UseMultiTouch != ApplicationDataManager.ApplicationOptions.UseMultiTouch)
		{
			OnMultiTouchChanged();
		}
	}

	private void OnMultiTouchChanged()
	{
		UseMultiTouch = ApplicationDataManager.ApplicationOptions.UseMultiTouch;
		if (UseMultiTouch)
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Content.text = "Use Simple Input";
		}
		else
		{
			AutoMonoBehaviour<TouchInput>.Instance.Buttons[16].Content.text = "Use Multi-touch Input";
		}
		WishDirection = Vector2.zero;
		WishLook = Vector2.zero;
		if (UseMultiTouch)
		{
			Shooter.OnFireStart += OnFireStart;
			Shooter.OnFireEnd += OnFireEnd;
		}
		else
		{
			Shooter.OnFireStart -= OnFireStart;
			Shooter.OnFireEnd -= OnFireEnd;
		}
		int currentStateId = _stateMachine.CurrentStateId;
		_stateMachine.SetState(currentStateId);
	}

	private void OnModeChangePushed()
	{
		ApplicationDataManager.ApplicationOptions.UseMultiTouch = !ApplicationDataManager.ApplicationOptions.UseMultiTouch;
		ApplicationDataManager.ApplicationOptions.SaveApplicationOptions();
		OnMultiTouchChanged();
		if (!ApplicationDataManager.ApplicationOptions.UseMultiTouch)
		{
			ShootHelpText.Alpha = 0f;
			AimHelpText.Alpha = 0f;
		}
		AutoMonoBehaviour<TouchInput>.Instance.CheckIdleTime = !HasDisplayedFireHelp && ApplicationDataManager.ApplicationOptions.UseMultiTouch;
		AutoMonoBehaviour<TouchInput>.Instance.LastFireTime = Time.time;
	}

	private void OnLoadoutPushed()
	{
		if (GamePageManager.IsCurrentPage(PageType.None))
		{
			GamePageManager.Instance.LoadPage(PageType.Shop);
		}
		else
		{
			GamePageManager.Instance.UnloadCurrentPage();
		}
	}

	private void OnScopeDown()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrevWeapon, 1f));
	}

	private void OnScopeUp()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.NextWeapon, 1f));
	}

	private void OnFireTouchBegan(Vector2 obj)
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrimaryFire, 1f));
	}

	private void OnFireTouchEnded(Vector2 obj)
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrimaryFire, 0f));
	}

	private void OnSecondaryFireTouchBegan(Vector2 obj)
	{
		_toggleSecondaryFire = !_toggleSecondaryFire;
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.SecondaryFire, _toggleSecondaryFire ? 1 : 0));
		WeaponSlot currentWeapon = Singleton<WeaponController>.Instance.GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return;
		}
		if (currentWeapon.View.WeaponSecondaryAction == 1)
		{
			if (_toggleSecondaryFire)
			{
				_stateMachine.SetState(3);
			}
			else
			{
				_stateMachine.SetState(1);
			}
		}
		else if (currentWeapon.View.WeaponSecondaryAction == 2)
		{
			if (_toggleSecondaryFire)
			{
				WeaponChanger.Enabled = false;
			}
			else
			{
				WeaponChanger.Enabled = true;
			}
		}
	}

	private void OnFireEnd()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrimaryFire, 0f));
		IsFiring = false;
	}

	private void OnFireStart()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrimaryFire, 1f));
		IsFiring = true;
		if (!HasDisplayedFireHelp)
		{
			CheckIdleTime = false;
			HasDisplayedFireHelp = true;
			AimHelpText.FadeAlphaTo(0f, 0.3f);
			ShootHelpText.FadeAlphaTo(0f, 0.3f);
		}
	}

	private void OnJumpTouchEnded(Vector2 obj)
	{
		WishJump = false;
	}

	private void OnCrouchPushed(Vector2 obj)
	{
		WishCrouch = !WishCrouch;
		((TouchButtonCircle)Buttons[10]).ShowHighlight = WishCrouch;
	}

	private void OnCrouchBegan(Vector2 obj)
	{
		WishCrouch = true;
	}

	private void OnCrouchEnded(Vector2 obj)
	{
		WishCrouch = false;
	}

	private void OnJump(Vector2 pos)
	{
		if (WishCrouch)
		{
			WishCrouch = false;
			((TouchButtonCircle)Buttons[10]).ShowHighlight = WishCrouch;
		}
		else
		{
			WishJump = true;
		}
	}

	private void OnFreeMove(Vector2 pos)
	{
		OnFireStart();
	}

	private void OnFreeMoveEnded(Vector2 pos)
	{
		OnFireEnd();
	}

	private void OnMenu()
	{
		_stateMachine.PushState(5);
		if (GameState.HasCurrentGame && !GameState.LocalPlayer.IsGamePaused)
		{
			GameState.LocalPlayer.Pause();
		}
		if (GlobalUIRibbon.Instance != null)
		{
			GlobalUIRibbon.Instance.Show();
		}
	}

	private void OnNextWeapon()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.NextWeapon, 1f));
	}

	private void OnPrevWeapon()
	{
		CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrevWeapon, 1f));
	}

	private void OnChatBegan()
	{
		_keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default, false, false, false, false);
		Singleton<InGameChatHud>.Instance.OpenChat();
		_previousState = (TouchState)_stateMachine.CurrentStateId;
		_stateMachine.SetState(2);
	}

	private void OnApplicationFocus(bool isFocused)
	{
		if (Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 12 && !isFocused && _keyboard == null)
		{
			OnMenu();
		}
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (isPaused)
		{
			_lastPaused = Time.realtimeSinceStartup;
		}
		else if (PlayerDataManager.IsPlayerLoggedIn && (double)Time.realtimeSinceStartup > (double)_lastPaused + 10.0 && Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 12 && !DisableIdleTimer)
		{
			Debug.Log("Idle time hit - returning to lobby");
			if (GameState.HasCurrentGame)
			{
				Singleton<GameStateController>.Instance.LeaveGame();
			}
		}
		else if ((double)Time.realtimeSinceStartup < (double)_lastPaused + 1.0 && GameState.HasCurrentGame && ApplicationDataManager.ApplicationOptions.UseMultiTouch && !HasDisplayedFiveFingerHelp)
		{
			EtceteraBinding.showAlertWithTitleMessageAndButtons("Hint", "Disable Multitasking Gestures in Settings > General", new string[1] { "Thanks!" });
			HasDisplayedFiveFingerHelp = true;
		}
	}

	private void OnEnable()
	{
		CmuneEventHandler.AddListener<OnPlayerDeadEvent>(OnPlayerDead);
		CmuneEventHandler.AddListener<OnPlayerRespawnEvent>(OnPlayerRespawned);
		CmuneEventHandler.AddListener<OnPlayerPauseEvent>(OnPlayerPaused);
		CmuneEventHandler.AddListener<OnPlayerUnpauseEvent>(OnPlayerUnpaused);
		CmuneEventHandler.AddListener<OnModeInitializedEvent>(OnModeInitialized);
		CmuneEventHandler.AddListener<OnMatchEndEvent>(OnMatchEndEvent);
		CmuneEventHandler.AddListener<OnMobileBackPressedEvent>(OnMobileBackPressed);
		CmuneEventHandler.AddListener<OnPlayerSpectatingEvent>(OnPlayerSpectating);
	}

	private void OnDisable()
	{
		CmuneEventHandler.RemoveListener<OnPlayerDeadEvent>(OnPlayerDead);
		CmuneEventHandler.RemoveListener<OnPlayerRespawnEvent>(OnPlayerRespawned);
		CmuneEventHandler.RemoveListener<OnPlayerPauseEvent>(OnPlayerPaused);
		CmuneEventHandler.RemoveListener<OnPlayerUnpauseEvent>(OnPlayerUnpaused);
		CmuneEventHandler.RemoveListener<OnModeInitializedEvent>(OnModeInitialized);
		CmuneEventHandler.RemoveListener<OnMatchEndEvent>(OnMatchEndEvent);
		CmuneEventHandler.RemoveListener<OnMobileBackPressedEvent>(OnMobileBackPressed);
		CmuneEventHandler.RemoveListener<OnPlayerSpectatingEvent>(OnPlayerSpectating);
	}

	private void OnMatchEndEvent(OnMatchEndEvent ev)
	{
		if (GameState.HasCurrentGame && !GameState.LocalPlayer.IsGamePaused)
		{
			GameState.LocalPlayer.Pause();
		}
		if (GlobalUIRibbon.Instance != null)
		{
			GlobalUIRibbon.Instance.Show();
		}
		Debug.Log("Current state is: " + _stateMachine.CurrentStateId);
		Buttons[16].Enabled = false;
	}

	private void OnPlayerDead(OnPlayerDeadEvent ev)
	{
		if (_stateMachine.CurrentStateId == 2 || _stateMachine.CurrentStateId == 6 || _stateMachine.CurrentStateId == 7)
		{
			_previousState = TouchState.Death;
		}
		else
		{
			_stateMachine.SetState(4);
		}
	}

	private void OnPlayerRespawned(OnPlayerRespawnEvent ev)
	{
		if (_stateMachine.CurrentStateId == 2 || _stateMachine.CurrentStateId == 6)
		{
			_previousState = TouchState.Playing;
		}
		else
		{
			_stateMachine.SetState(1);
		}
	}

	private void OnPlayerPaused(OnPlayerPauseEvent ev)
	{
		if (_stateMachine.CurrentStateId == 2)
		{
			_previousState = TouchState.Paused;
		}
		else
		{
			_stateMachine.SetState(5);
		}
	}

	private void OnPlayerUnpaused(OnPlayerUnpauseEvent ev)
	{
		if (_stateMachine.CurrentStateId == 2)
		{
			_previousState = TouchState.Playing;
		}
		else if (Singleton<PlayerSpectatorControl>.Instance.IsEnabled)
		{
			_stateMachine.SetState(7);
		}
		else
		{
			_stateMachine.SetState(1);
		}
	}

	private void OnModeInitialized(OnModeInitializedEvent ev)
	{
		HasDisplayedFireHelp = false;
	}

	private void OnMobileBackPressed(OnMobileBackPressedEvent ev)
	{
		if (Singleton<GameStateController>.Instance.StateMachine.CurrentStateId == 12 || Singleton<GameStateController>.Instance.StateMachine.CurrentStateId == 14 || MenuPageManager.Instance == null)
		{
			return;
		}
		if (GameState.HasCurrentGame && GameState.LocalPlayer.IsGamePaused)
		{
			GameState.LocalPlayer.UnPausePlayer();
			if (GlobalUIRibbon.Instance != null)
			{
				GlobalUIRibbon.Instance.Hide();
			}
		}
		PageType currentPage = MenuPageManager.Instance.GetCurrentPage();
		if (PanelManager.IsAnyPanelOpen)
		{
			if (!PanelManager.Instance.IsPanelOpen(PanelType.Login))
			{
				PanelManager.Instance.CloseAllPanels();
			}
		}
		else if (currentPage != PageType.Home && currentPage != PageType.Login)
		{
			MenuPageManager.Instance.LoadPage(PageType.Home);
		}
	}

	private void OnPlayerSpectating(OnPlayerSpectatingEvent ev)
	{
		if (_stateMachine.CurrentStateId == 2 || _stateMachine.CurrentStateId == 6)
		{
			_previousState = TouchState.Spectator;
		}
		else
		{
			_stateMachine.SetState(7);
		}
	}
}
