using System.Collections;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class LocalPlayer : MonoBehaviour
{
	public enum PlayerState
	{
		None = 0,
		FirstPerson = 1,
		ThirdPerson = 2,
		Death = 3,
		FreeMove = 4,
		Overview = 5
	}

	private const float RundownThreshold = -300f;

	public const float Threshold = 0.5f;

	[SerializeField]
	private Transform _cameraTarget;

	[SerializeField]
	private Transform _characterBase;

	[SerializeField]
	private Transform _firstPersonView;

	[SerializeField]
	private Transform _weaponAttachPoint;

	[SerializeField]
	private WeaponCamera _weaponCamera;

	protected PlayerHudState _weaponControlState;

	private CharacterMoveController _moveController;

	private CharacterConfig _currentCharacter;

	private PlayerState _controlState;

	private Quaternion _viewPointRotation = Quaternion.identity;

	private bool _isWalkingEnabled = true;

	private bool _isShootingEnabled = true;

	private bool? _isPaused;

	private bool _isQuitting;

	private float _damageFactor;

	private float _damageFactorDuration;

	private float _lastGrounded;

	public static readonly Vector3 EyePosition = new Vector3(0f, -0.1f, 0f);

	public bool IsInitialized { get; private set; }

	public CharacterConfig Character
	{
		get
		{
			return _currentCharacter;
		}
	}

	public AvatarDecorator Decorator
	{
		get
		{
			return (!_currentCharacter || _currentCharacter.Avatar == null) ? null : _currentCharacter.Avatar.Decorator;
		}
	}

	public AvatarDecoratorConfig Killer { get; set; }

	public bool IsGamePaused
	{
		get
		{
			bool? isPaused = _isPaused;
			return isPaused.HasValue && isPaused.Value;
		}
	}

	public float DamageFactor
	{
		get
		{
			return _damageFactor;
		}
		set
		{
			_damageFactor = Mathf.Clamp01(value);
			_damageFactorDuration = _damageFactor * 15f;
			_damageFactor /= 0.15f;
		}
	}

	public bool IsPlayerRespawned { get; set; }

	public bool IsMouseLockStateConsistent
	{
		get
		{
			return Screen.lockCursor;
		}
	}

	public bool IsWalkingEnabled
	{
		get
		{
			return _isWalkingEnabled && AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled;
		}
		set
		{
			_isWalkingEnabled = value;
		}
	}

	public bool IsShootingEnabled
	{
		get
		{
			return _isShootingEnabled && AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled;
		}
		set
		{
			_isShootingEnabled = value;
			Singleton<WeaponController>.Instance.IsEnabled = value;
		}
	}

	public PlayerState CurrentCameraControl
	{
		get
		{
			return _controlState;
		}
	}

	public CharacterMoveController MoveController
	{
		get
		{
			return _moveController;
		}
	}

	public WeaponCamera WeaponCamera
	{
		get
		{
			return _weaponCamera;
		}
	}

	public Transform WeaponAttachPoint
	{
		get
		{
			return _weaponAttachPoint;
		}
	}

	public bool IsDead { get; private set; }

	private void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	internal void Initialize()
	{
		_moveController = new CharacterMoveController(GetComponent<CharacterController>(), _characterBase);
		_moveController.CharacterLanded += OnCharacterGrounded;
	}

	private void OnEnable()
	{
		_moveController.Init();
		if (HudController.Exists)
		{
			HudController.Instance.enabled = true;
		}
		StartCoroutine(StartPlayerIdentification());
		StartCoroutine(StartUpdatePlayerPingTime(5));
	}

	private void OnApplicationQuit()
	{
		_isQuitting = true;
	}

	private void OnDisable()
	{
		if (!_isQuitting)
		{
			_isPaused = null;
			Screen.lockCursor = false;
			AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
			if (HudController.Exists)
			{
				HudController.Instance.enabled = false;
			}
			if (GlobalUIRibbon.Instance != null)
			{
				GlobalUIRibbon.Instance.Show();
			}
			IsInitialized = false;
		}
	}

	private void FixedUpdate()
	{
		if (_moveController != null && GameState.HasCurrentPlayer)
		{
			_moveController.UpdatePlayerMovement();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && Singleton<GameStateController>.Instance.StateMachine.CurrentStateId != 12)
		{
			if (!IsGamePaused)
			{
				Pause();
			}
			if (GlobalUIRibbon.Instance != null)
			{
				GlobalUIRibbon.Instance.Show();
			}
		}
		else if (!Singleton<InGameChatHud>.Instance.CanInput && Input.GetKeyDown(KeyCode.Backspace))
		{
			if (!IsGamePaused)
			{
				Pause();
			}
			if (GlobalUIRibbon.Instance != null)
			{
				GlobalUIRibbon.Instance.Show();
			}
		}
		if (AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled)
		{
			UserInput.UpdateMouse();
			if (GameState.LocalCharacter.IsAlive)
			{
				UserInput.UpdateDirections();
			}
			if (!GameState.HasCurrentPlayer)
			{
				return;
			}
			_cameraTarget.localPosition = Vector3.Lerp(_cameraTarget.localPosition, GameState.LocalCharacter.CurrentOffset, 10f * Time.deltaTime);
			DoCameraBob();
			UpdateRotation();
			if (_damageFactor != 0f)
			{
				if (_damageFactorDuration > 0f)
				{
					_damageFactorDuration -= Time.deltaTime;
				}
				if (_damageFactorDuration <= 0f || !GameState.LocalCharacter.IsAlive)
				{
					_damageFactor = 0f;
					_damageFactorDuration = 0f;
				}
			}
		}
		else
		{
			UserInput.ResetDirection();
		}
	}

	private void LateUpdate()
	{
		Singleton<WeaponController>.Instance.LateUpdate();
	}

	private void UpdateRotation()
	{
		_cameraTarget.localRotation = _viewPointRotation * UserInput.Rotation;
		GameState.LocalCharacter.HorizontalRotation = UserInput.Rotation;
		GameState.LocalCharacter.VerticalRotation = (UserInput.Mouse.y + 90f) / 180f;
	}

	private IEnumerator StartPlayerIdentification()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.3f);
			if (IsGamePaused || !Camera.main || !GameState.HasCurrentPlayer)
			{
				continue;
			}
			Vector3 start = GameState.LocalCharacter.ShootingPoint + EyePosition;
			Vector3 end = start + GameState.LocalCharacter.ShootingDirection * 1000f;
			RaycastHit hit;
			if (Physics.Linecast(start, end, out hit, UberstrikeLayerMasks.IdentificationMask))
			{
				CharacterHitArea hitArea = hit.collider.GetComponent<CharacterHitArea>();
				if ((bool)hitArea && hitArea.Shootable != null && !hitArea.Shootable.IsLocal)
				{
					CharacterConfig character = hitArea.Shootable as CharacterConfig;
					if (character != null && (bool)character.AimTrigger)
					{
						character.AimTrigger.HudInfo.Show(2);
						character.AimTrigger.HudInfo.IsBarVisible = true;
						Singleton<ReticleHud>.Instance.FocusCharacter(character.Team);
						SfxManager.Play2dAudioClip(GameAudio.FocusEnemy);
					}
				}
				else
				{
					Singleton<ReticleHud>.Instance.UnFocusCharacter();
				}
			}
			else
			{
				Singleton<ReticleHud>.Instance.UnFocusCharacter();
			}
		}
	}

	private IEnumerator StartUpdatePlayerPingTime(int sec)
	{
		while (true)
		{
			if (GameState.HasCurrentPlayer)
			{
				GameState.LocalCharacter.Ping = GameConnectionManager.Client.PeerListener.Ping;
			}
			yield return new WaitForSeconds(sec);
		}
	}

	private void OnCharacterGrounded(float velocity)
	{
		if (!GameState.HasCurrentGame || !GameState.CurrentGame.IsMatchRunning || LevelCamera.Instance.CurrentBob != BobMode.None || !(_lastGrounded + 0.5f < Time.time) || GameState.LocalCharacter.Is(PlayerStates.DIVING))
		{
			return;
		}
		_lastGrounded = Time.time;
		if ((bool)_currentCharacter && (bool)_currentCharacter.Avatar.Decorator)
		{
			_currentCharacter.Avatar.Decorator.PlayFootSound(_currentCharacter.WalkingSoundSpeed);
			if (velocity < -20f)
			{
				LevelCamera.Instance.DoLandFeedback(true);
				SfxManager.Play2dAudioClip(GameAudio.LandingGrunt);
			}
			else
			{
				LevelCamera.Instance.DoLandFeedback(false);
			}
		}
	}

	private void DoCameraBob()
	{
		switch (GameState.LocalCharacter.PlayerState)
		{
		case PlayerStates.SWIMMING:
			LevelCamera.SetBobMode(BobMode.Swim);
			break;
		case PlayerStates.GROUNDED | PlayerStates.DUCKED:
			if (UserInput.IsWalking)
			{
				if (Singleton<WeaponController>.Instance.IsSecondaryAction)
				{
					LevelCamera.SetBobMode(BobMode.None);
				}
				else
				{
					LevelCamera.SetBobMode(BobMode.Crouch);
				}
			}
			else if (Singleton<WeaponController>.Instance.IsSecondaryAction)
			{
				LevelCamera.SetBobMode(BobMode.None);
			}
			else
			{
				LevelCamera.SetBobMode(BobMode.Idle);
			}
			break;
		case PlayerStates.FLYING:
			LevelCamera.SetBobMode(BobMode.Fly);
			break;
		case PlayerStates.GROUNDED:
			if (UserInput.IsWalking)
			{
				if (Singleton<WeaponController>.Instance.IsSecondaryAction)
				{
					LevelCamera.SetBobMode(BobMode.None);
				}
				else
				{
					LevelCamera.SetBobMode(BobMode.Run);
				}
			}
			else if (Singleton<WeaponController>.Instance.IsSecondaryAction)
			{
				LevelCamera.SetBobMode(BobMode.None);
			}
			else if (!UserInput.IsWalking || _moveController.CurrentVelocity.y < -300f)
			{
				LevelCamera.SetBobMode(BobMode.Idle);
			}
			break;
		case PlayerStates.IDLE:
			if (!UserInput.IsWalking || _moveController.CurrentVelocity.y < -300f)
			{
				LevelCamera.SetBobMode(BobMode.None);
			}
			break;
		case PlayerStates.JUMPING:
			LevelCamera.SetBobMode(BobMode.None);
			break;
		default:
			if (!UserInput.IsWalking || _moveController.CurrentVelocity.y < -300f)
			{
				LevelCamera.SetBobMode(BobMode.None);
			}
			break;
		}
	}

	public void InitializePlayer()
	{
		IsInitialized = true;
		try
		{
			if (LevelCamera.Instance != null)
			{
				LevelCamera.SetBobMode(BobMode.None);
				LevelCamera.Instance.CanDip = true;
				LevelCamera.Instance.IsZoomedIn = false;
			}
			if (GameState.LocalCharacter != null)
			{
				GameState.LocalCharacter.ResetState();
				if (HudAssets.Exists)
				{
					Singleton<HpApHud>.Instance.AP = GameState.LocalCharacter.Armor.ArmorPoints;
					Singleton<HpApHud>.Instance.HP = GameState.LocalCharacter.Health;
				}
				Singleton<WeaponController>.Instance.InitializeAllWeapons(_weaponAttachPoint);
				UpdateLocalCharacterLoadout();
				UpdateRotation();
			}
			else
			{
				Debug.LogError("CurrentPlayer is null!");
			}
			_currentCharacter.Avatar.EnableDecorator();
			if (Decorator != null)
			{
				SetPlayerControlState(PlayerState.FirstPerson, Decorator.Configuration);
				Decorator.UpdateLayers();
				Decorator.MeshRenderer.enabled = false;
				Decorator.HudInformation.enabled = false;
			}
			IsDead = false;
			_moveController.Start();
			_moveController.ResetDuckMode();
			Singleton<QuickItemController>.Instance.Reset();
			if (!PanelManager.IsAnyPanelOpen)
			{
				UnPausePlayer();
			}
			DamageFactor = 0f;
		}
		catch
		{
			Debug.LogError(string.Format("InitializePlayer with {0}", CmunePrint.Properties(this)));
			throw;
		}
	}

	public void UpdateLocalCharacterLoadout()
	{
		UberStrike.Realtime.UnitySdk.CharacterInfo localCharacter = GameState.LocalCharacter;
		localCharacter.Gear[0] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearHolo);
		localCharacter.Gear[1] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearHead);
		localCharacter.Gear[2] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearFace);
		localCharacter.Gear[3] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearGloves);
		localCharacter.Gear[4] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearUpperBody);
		localCharacter.Gear[5] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearLowerBody);
		localCharacter.Gear[6] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearBoots);
		localCharacter.QuickItems[0] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.QuickUseItem1);
		localCharacter.QuickItems[1] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.QuickUseItem2);
		localCharacter.QuickItems[2] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.QuickUseItem3);
		localCharacter.FunctionalItems[0] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.FunctionalItem1);
		localCharacter.FunctionalItems[1] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.FunctionalItem2);
		localCharacter.FunctionalItems[2] = Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.FunctionalItem3);
		IUnityItem itemOnSlot = Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponMelee);
		IUnityItem itemOnSlot2 = Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponPrimary);
		IUnityItem itemOnSlot3 = Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponSecondary);
		IUnityItem itemOnSlot4 = Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponTertiary);
		localCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Melee, (itemOnSlot != null) ? itemOnSlot.View.ID : 0, (itemOnSlot != null) ? itemOnSlot.View.ItemClass : ((UberstrikeItemClass)0));
		localCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Primary, (itemOnSlot2 != null) ? itemOnSlot2.View.ID : 0, (itemOnSlot2 != null) ? itemOnSlot2.View.ItemClass : ((UberstrikeItemClass)0));
		localCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Secondary, (itemOnSlot3 != null) ? itemOnSlot3.View.ID : 0, (itemOnSlot3 != null) ? itemOnSlot3.View.ItemClass : ((UberstrikeItemClass)0));
		localCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Tertiary, (itemOnSlot4 != null) ? itemOnSlot4.View.ID : 0, (itemOnSlot4 != null) ? itemOnSlot4.View.ItemClass : ((UberstrikeItemClass)0));
		localCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Pickup, 0, (UberstrikeItemClass)0);
		localCharacter.SkinColor = PlayerDataManager.SkinColor;
		int armorPoints = 0;
		int absorbtionRatio = 0;
		Singleton<LoadoutManager>.Instance.GetArmorValues(out armorPoints, out absorbtionRatio);
		localCharacter.Armor.AbsorbtionPercentage = (byte)absorbtionRatio;
		localCharacter.Armor.ArmorPointCapacity = armorPoints;
		localCharacter.Armor.ArmorPoints = armorPoints;
	}

	public void SetPlayerDead()
	{
		if (!IsDead)
		{
			IsDead = true;
			Killer = null;
			if (!Singleton<PlayerSpectatorControl>.Instance.IsEnabled)
			{
				AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
			}
			UpdateWeaponController();
			CmuneEventHandler.Route(new OnPlayerDeadEvent());
		}
	}

	public void SpawnPlayerAt(Vector3 pos, Quaternion rot)
	{
		try
		{
			base.transform.position = pos + Vector3.up;
			_cameraTarget.localRotation = rot;
			UserInput.SetRotation(rot.eulerAngles.y, 0f);
			if (LevelCamera.Instance != null)
			{
				LevelCamera.Instance.ResetFeedback();
			}
			if (GameState.HasCurrentGame)
			{
				GameState.CurrentGame.SendPlayerSpawnPosition(pos);
			}
			if (GameState.HasCurrentPlayer)
			{
				GameState.LocalCharacter.Position = pos;
			}
			MoveController.ResetEnviroment();
			MoveController.Platform = null;
		}
		catch
		{
			Debug.LogError(string.Format("SpawnPlayerAt with LocalPlayer {0}", CmunePrint.Properties(GameState.LocalPlayer)));
			throw;
		}
	}

	public void SetCurrentCharacterConfig(CharacterConfig character)
	{
		_currentCharacter = character;
	}

	public void SetPlayerControlState(PlayerState s, AvatarDecoratorConfig decorator = null)
	{
		_controlState = s;
		switch (_controlState)
		{
		case PlayerState.FirstPerson:
		{
			_viewPointRotation = _firstPersonView.localRotation;
			LevelCamera.Instance.SetTarget(_cameraTarget);
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.FirstPerson);
			LevelCamera instance = LevelCamera.Instance;
			Vector3 eyePosition = EyePosition;
			float x = eyePosition.x;
			Vector3 eyePosition2 = EyePosition;
			float y = eyePosition2.y;
			Vector3 eyePosition3 = EyePosition;
			instance.SetEyePosition(x, y, eyePosition3.z);
			if (LevelCamera.HasCamera)
			{
				LevelCamera.Instance.MainCamera.transform.localPosition = Vector3.zero;
				LevelCamera.Instance.MainCamera.transform.localRotation = Quaternion.identity;
			}
			if ((bool)GameState.LocalPlayer.WeaponCamera)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(true);
			}
			break;
		}
		case PlayerState.ThirdPerson:
			_viewPointRotation = _firstPersonView.localRotation * Quaternion.Euler(10f, 0f, 0f);
			LevelCamera.Instance.SetTarget(_cameraTarget);
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.ThirdPerson);
			LevelCamera.Instance.MainCamera.transform.localPosition = new Vector3(2f, 3f, 0f);
			LevelCamera.Instance.MainCamera.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
			if ((bool)GameState.LocalPlayer.WeaponCamera)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(false);
			}
			break;
		case PlayerState.FreeMove:
			LevelCamera.Instance.SetLookAtHeight(0f);
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.Spectator);
			if ((bool)GameState.LocalPlayer.WeaponCamera)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(false);
			}
			break;
		case PlayerState.Death:
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.Ragdoll);
			LevelCamera.Instance.SetLookAtHeight(1f);
			if ((bool)GameState.LocalPlayer.WeaponCamera)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(false);
			}
			break;
		case PlayerState.Overview:
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.Overview);
			LevelCamera.Instance.SetLookAtHeight(1f);
			if (GameState.LocalAvatar.Decorator != null)
			{
				GameState.LocalAvatar.Decorator.SetLayers(UberstrikeLayer.RemotePlayer);
				GameState.LocalAvatar.Decorator.MeshRenderer.enabled = true;
			}
			if ((bool)GameState.LocalPlayer.WeaponCamera)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(false);
			}
			break;
		default:
			if ((bool)Camera.main && (bool)GameState.CurrentSpace && (bool)GameState.CurrentSpace.DefaultViewPoint)
			{
				Camera.main.transform.rotation = GameState.CurrentSpace.DefaultViewPoint.rotation;
			}
			LevelCamera.Instance.SetMode(LevelCamera.CameraMode.None);
			if ((bool)GameState.LocalPlayer.WeaponCamera)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(false);
			}
			break;
		}
	}

	public void Pause(bool force = false)
	{
		if (force || !_isPaused.HasValue || !_isPaused.Value)
		{
			_isPaused = true;
			AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = false;
			LevelCamera.SetBobMode(BobMode.Idle);
			if (GameState.HasCurrentPlayer)
			{
				GameState.LocalCharacter.Keys = KeyState.Still;
				GameState.LocalCharacter.IsFiring = false;
			}
			if (GameState.HasCurrentGame && GameState.HasCurrentPlayer)
			{
				Singleton<WeaponController>.Instance.StopInputHandler();
			}
			Screen.lockCursor = false;
			UpdateWeaponController();
			if (GameState.HasCurrentGame)
			{
				CmuneEventHandler.Route(new OnPlayerPauseEvent());
			}
		}
	}

	public void UnPausePlayer()
	{
		PopupSystem.ClearAll();
		_isPaused = false;
		AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled = true;
		Screen.lockCursor = true;
		if (GlobalUIRibbon.Instance != null)
		{
			GlobalUIRibbon.Instance.Hide();
		}
		UpdateWeaponController();
		if (GameState.HasCurrentGame)
		{
			CmuneEventHandler.Route(new OnPlayerUnpauseEvent());
		}
	}

	public void SetWeaponControlState(PlayerHudState state)
	{
		_weaponControlState = state;
		UpdateWeaponController();
	}

	public void UpdateWeaponController()
	{
		switch (_weaponControlState)
		{
		case PlayerHudState.Playing:
			Singleton<WeaponController>.Instance.IsEnabled = !IsGamePaused && GameState.LocalCharacter.IsAlive;
			break;
		case PlayerHudState.None:
		case PlayerHudState.Spectating:
		case PlayerHudState.AfterRound:
			Singleton<WeaponController>.Instance.IsEnabled = false;
			break;
		}
	}

	public void SetEnabled(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}
}
