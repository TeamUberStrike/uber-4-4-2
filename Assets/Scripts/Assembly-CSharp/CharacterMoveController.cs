using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CharacterMoveController
{
	public enum ForceType
	{
		Additive = 0,
		Exclusive = 1
	}

	public const float POWERUP_HASTE_SCALE = 1.3f;

	public const float PLAYER_WADE_SCALE = 0.8f;

	public const float PLAYER_SWIM_SCALE = 0.6f;

	public const float PLAYER_DUCK_SCALE = 0.7f;

	public const float PLAYER_TERMINAL_GRAVITY = -100f;

	public const float PLAYER_INITIAL_GRAVITY = -1f;

	public const float PLAYER_ZOOM_SCALE = 0.7f;

	public const float PLAYER_MIN_SCALE = 0.5f;

	public const float PLAYER_IRON_SIGHT = 1f;

	public bool IsLowGravity;

	private readonly CharacterController _controller;

	private readonly PlayerAttributes _attributes;

	private readonly Transform _transform;

	private readonly Transform _playerBase;

	private MovingPlatform _platform;

	private EnviromentSettings _currentEnviroment;

	private CollisionFlags _collisionFlag;

	private Vector3 _currentVelocity;

	private Vector3 _acceleration;

	private bool _isOnLatter;

	private bool _isGrounded = true;

	private bool _canJump = true;

	private int _ungroundedCount;

	private int _waterLevel;

	private float _waterEnclosure;

	private ForceType _forceType;

	private Vector3 _externalForce;

	private bool _hasExternalForce;

	public bool IsJumpDisabled { get; set; }

	public float DamageSlowDown { get; set; }

	public float PlayerHeight
	{
		get
		{
			return (!GameState.LocalCharacter.Is(PlayerStates.DUCKED)) ? 1.6f : 0.9f;
		}
	}

	public Vector3 Velocity
	{
		get
		{
			return _currentVelocity;
		}
	}

	public float SpeedModifier
	{
		get
		{
			float num = 1f;
			float num2 = ((!Singleton<WeaponController>.Instance.IsSecondaryAction) ? 1f : 0.7f);
			float num3 = ((!WeaponFeedbackManager.Instance.IsIronSighted) ? 1f : 1f);
			float num4 = 1f - Mathf.Clamp01(Singleton<PlayerDataManager>.Instance.GearWeight) * 0.3f;
			float num5 = 1f - Mathf.Clamp01(GameState.LocalPlayer.DamageFactor) * 0.3f;
			num *= Mathf.Min(1f, num2, num3, num4, num5);
			if (WaterLevel > 0)
			{
				num = ((WaterLevel != 3) ? (num * 0.8f) : (num * 0.6f));
			}
			else if (IsGrounded && GameState.LocalCharacter.Is(PlayerStates.DUCKED))
			{
				num *= 0.7f;
			}
			return Mathf.Max(0.5f, num);
		}
	}

	public MovingPlatform Platform
	{
		get
		{
			return _platform;
		}
		set
		{
			_platform = value;
		}
	}

	public Vector3 CurrentVelocity
	{
		get
		{
			return _currentVelocity;
		}
	}

	public int WaterLevel
	{
		get
		{
			return _waterLevel;
		}
		private set
		{
			_waterLevel = value;
		}
	}

	public bool IsGrounded
	{
		get
		{
			return _isGrounded;
		}
		private set
		{
			_isGrounded = value;
		}
	}

	public CharacterController DebugController
	{
		get
		{
			return _controller;
		}
	}

	public Bounds DebugEnvBounds
	{
		get
		{
			return _currentEnviroment.EnviromentBounds;
		}
	}

	private float Gravity
	{
		get
		{
			return ((!IsLowGravity) ? 1f : 0.4f) * _currentEnviroment.Gravity * Time.deltaTime;
		}
	}

	public event Action<float> CharacterLanded;

	public CharacterMoveController(CharacterController controller, Transform characterBase)
	{
		_controller = controller;
		_transform = _controller.transform;
		_attributes = new PlayerAttributes();
		_playerBase = characterBase;
		CmuneEventHandler.AddListener<InputChangeEvent>(OnInputChanged);
	}

	public void Init()
	{
		if (LevelEnviroment.Instance != null)
		{
			_currentEnviroment = LevelEnviroment.Instance.Settings;
		}
		else
		{
			Debug.LogWarning("You are trying to access the LevelEnvironment Instance that has not had Awake called.");
		}
	}

	public void Start()
	{
		Reset();
	}

	public void UpdatePlayerMovement()
	{
		if (GameState.HasCurrentPlayer)
		{
			UpdateMovementStates();
			UpdateMovement();
		}
	}

	public void ResetDuckMode()
	{
		_controller.height = 1.6f;
		_controller.center = new Vector3(0f, 0f, 0f);
	}

	public static bool HasCollision(Vector3 pos, float height)
	{
		return Physics.CheckSphere(pos + Vector3.up * (height - 0.5f), 0.6f, UberstrikeLayerMasks.CrouchMask);
	}

	public void ApplyForce(Vector3 v, ForceType type)
	{
		_hasExternalForce = true;
		_externalForce = v;
		_forceType = type;
	}

	public void ClearForce()
	{
		_externalForce = Vector3.zero;
	}

	public void ResetEnviroment()
	{
		_currentEnviroment = LevelEnviroment.Instance.Settings;
		_currentEnviroment.EnviromentBounds = default(Bounds);
		_isOnLatter = false;
	}

	public void SetEnviroment(EnviromentSettings settings, Bounds bounds)
	{
		_currentEnviroment = settings;
		_currentEnviroment.EnviromentBounds = new Bounds(bounds.center, bounds.size);
		_isOnLatter = _currentEnviroment.Type == EnviromentSettings.TYPE.LATTER;
	}

	private void UpdateMovementStates()
	{
		if (_currentEnviroment.Type == EnviromentSettings.TYPE.LATTER && !_currentEnviroment.EnviromentBounds.Intersects(_controller.bounds))
		{
			ResetEnviroment();
		}
		if (_currentEnviroment.Type == EnviromentSettings.TYPE.WATER)
		{
			_currentEnviroment.CheckPlayerEnclosure(_playerBase.position, PlayerHeight, out _waterEnclosure);
			int num = 1;
			if (_waterEnclosure >= 0.8f)
			{
				num = 3;
			}
			else if (_waterEnclosure >= 0.4f)
			{
				num = 2;
			}
			if (WaterLevel != num)
			{
				SetWaterlevel(num);
			}
		}
		else if (WaterLevel != 0)
		{
			SetWaterlevel(0);
		}
		if ((GameState.LocalCharacter.Keys & KeyState.Jump) == 0)
		{
			_canJump = true;
		}
		if (GameState.LocalCharacter.Is(PlayerStates.GROUNDED | PlayerStates.JUMPING))
		{
			GameState.LocalCharacter.Set(PlayerStates.JUMPING, false);
		}
	}

	private void UpdateMovement()
	{
		if ((_collisionFlag & CollisionFlags.Sides) == 0)
		{
			CheckDuck();
		}
		if (GameState.LocalCharacter.Is(PlayerStates.FLYING))
		{
			FlyInAir();
		}
		else if (WaterLevel > 2)
		{
			MoveInWater();
		}
		else if (_isOnLatter)
		{
			MoveOnLatter();
		}
		else if (IsGrounded)
		{
			MoveOnGround();
		}
		else if (WaterLevel == 2)
		{
			MoveOnWaterRim();
		}
		else
		{
			MoveInAir();
		}
		if (_hasExternalForce)
		{
			switch (_forceType)
			{
			case ForceType.Additive:
				_currentVelocity = Vector3.Scale(_currentVelocity, new Vector3(1f, 0.5f, 1f)) + _externalForce * 0.035f;
				break;
			case ForceType.Exclusive:
				_currentVelocity = _externalForce * 0.035f;
				break;
			}
			_externalForce *= 0f;
			_hasExternalForce = false;
			GameState.LocalCharacter.Set(PlayerStates.JUMPING, true);
		}
		Vector3 vector;
		if (IsGrounded && (bool)Platform)
		{
			vector = Platform.GetMovementDelta();
			if (vector.y > 0f)
			{
				_currentVelocity.y = 0f;
			}
		}
		else
		{
			vector = Vector3.zero;
		}
		_currentVelocity[1] = Mathf.Clamp(_currentVelocity[1], -150f, 150f);
		_collisionFlag = _controller.Move(_currentVelocity * Time.deltaTime);
		if (IsGrounded && (bool)Platform)
		{
			_transform.localPosition += vector;
		}
		_currentVelocity = _controller.velocity;
		if ((_collisionFlag & CollisionFlags.Below) != CollisionFlags.None)
		{
			if (_ungroundedCount > 5 && this.CharacterLanded != null)
			{
				this.CharacterLanded(_currentVelocity.y);
			}
			_ungroundedCount = 0;
			IsGrounded = true;
		}
		else if (GameState.LocalCharacter.Is(PlayerStates.JUMPING))
		{
			_ungroundedCount++;
			IsGrounded = false;
		}
		else if (_ungroundedCount > 5)
		{
			IsGrounded = false;
		}
		else
		{
			_ungroundedCount++;
			IsGrounded = true;
		}
		GameState.LocalCharacter.Set(PlayerStates.GROUNDED, IsGrounded);
		GameState.LocalCharacter.Position = _controller.transform.position;
	}

	private void OnInputChanged(InputChangeEvent ev)
	{
		if (GameState.LocalCharacter != null && GameState.LocalPlayer.IsWalkingEnabled)
		{
			if (ev.IsDown)
			{
				GameState.LocalCharacter.Keys |= UserInput.GetkeyState(ev.Key);
			}
			else
			{
				GameState.LocalCharacter.Keys &= ~UserInput.GetkeyState(ev.Key);
			}
		}
	}

	private void Reset()
	{
		SetWaterlevel(0);
		_currentVelocity = Vector3.zero;
		_forceType = ForceType.Additive;
		_hasExternalForce = false;
		_externalForce = Vector3.zero;
		_canJump = true;
		_isGrounded = true;
		_ungroundedCount = 0;
		_platform = null;
		IsJumpDisabled = false;
	}

	private void ApplyFriction()
	{
		Vector3 currentVelocity = _currentVelocity;
		float magnitude = currentVelocity.magnitude;
		if (magnitude == 0f)
		{
			return;
		}
		if (magnitude < 0.5f && _acceleration.sqrMagnitude == 0f)
		{
			if (_isOnLatter)
			{
				_currentVelocity[1] = 0f;
			}
			_currentVelocity[0] = 0f;
			_currentVelocity[2] = 0f;
			return;
		}
		float num = 0f;
		if (WaterLevel < 3)
		{
			if (_isOnLatter || GameState.LocalCharacter.Is(PlayerStates.GROUNDED))
			{
				float num2 = Mathf.Max(_currentEnviroment.StopSpeed, magnitude);
				num += num2 * _currentEnviroment.GroundFriction;
			}
		}
		else if (WaterLevel > 0)
		{
			num += Mathf.Max(_currentEnviroment.StopSpeed, magnitude) * _currentEnviroment.WaterFriction * (float)WaterLevel / 3f;
		}
		if (GameState.LocalCharacter.Is(PlayerStates.FLYING))
		{
			float num2 = Mathf.Max(_currentEnviroment.StopSpeed, magnitude);
			num += num2 * _currentEnviroment.FlyFriction;
		}
		num *= Time.deltaTime;
		float num3 = magnitude - num;
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		num3 /= magnitude;
		_currentVelocity *= num3;
	}

	private void ApplyAcceleration(Vector3 wishdir, float wishspeed, float accel, bool clamp = false)
	{
		float num = Vector3.Dot(_currentVelocity, wishdir);
		float num2 = wishspeed - num;
		if (num2 <= 0f)
		{
			_acceleration = Vector3.zero;
			return;
		}
		_acceleration = accel * wishspeed * wishdir * Time.deltaTime;
		float magnitude = (_currentVelocity + _acceleration).magnitude;
		if (magnitude < wishspeed)
		{
			_currentVelocity += _acceleration;
		}
	}

	private void CheckDuck()
	{
		if (WaterLevel < 3 && GameState.HasCurrentPlayer && !GameState.LocalCharacter.Is(PlayerStates.JUMPING) && !GameState.LocalCharacter.Is(PlayerStates.FLYING))
		{
			if (UserInput.IsPressed(KeyState.Crouch) && !GameState.LocalCharacter.Is(PlayerStates.DUCKED))
			{
				GameState.LocalCharacter.Set(PlayerStates.DUCKED, true);
				_controller.height = 0.9f;
				_controller.center = new Vector3(0f, -0.4f, 0f);
			}
			else if (!UserInput.IsPressed(KeyState.Crouch) && GameState.LocalCharacter.Is(PlayerStates.DUCKED) && !HasCollision(_playerBase.position, 1.6f))
			{
				GameState.LocalCharacter.Set(PlayerStates.DUCKED, false);
				_controller.height = 1.6f;
				_controller.center = new Vector3(0f, -0.1f, 0f);
			}
		}
	}

	private bool CheckJump()
	{
		if (IsJumpDisabled || GameState.LocalCharacter.Is(PlayerStates.DUCKED) || (GameState.LocalCharacter.Keys & KeyState.Jump) == 0)
		{
			return false;
		}
		if (_isOnLatter)
		{
			return true;
		}
		if (_canJump)
		{
			_canJump = false;
			GameState.LocalCharacter.Set(PlayerStates.GROUNDED, false);
			GameState.LocalCharacter.Set(PlayerStates.JUMPING, true);
			_currentVelocity.y = _attributes.JumpForce;
			return true;
		}
		UserInput.HorizontalDirection.y = 0f;
		return false;
	}

	private bool CheckWaterJump()
	{
		if ((GameState.LocalCharacter.Keys & KeyState.Jump) != KeyState.Still && (_collisionFlag & CollisionFlags.Sides) != CollisionFlags.None)
		{
			if (!_canJump)
			{
				UserInput.HorizontalDirection.y = 0f;
				return false;
			}
			GameState.LocalCharacter.Set(PlayerStates.JUMPING, true);
			_currentVelocity.y = _attributes.JumpForce;
			return true;
		}
		return false;
	}

	private void FlyInAir()
	{
		ApplyFriction();
		Vector3 wishdir = Vector3.zero;
		if (UserInput.IsWalking)
		{
			wishdir = UserInput.Rotation * UserInput.HorizontalDirection;
		}
		if (UserInput.VerticalDirection.y != 0f)
		{
			wishdir.y = UserInput.VerticalDirection.y;
		}
		ApplyAcceleration(wishdir, _attributes.Speed, _currentEnviroment.FlyAcceleration);
	}

	private void MoveInWater()
	{
		ApplyFriction();
		Vector3 wishdir = Vector3.zero;
		if (UserInput.IsWalking)
		{
			wishdir = UserInput.Rotation * UserInput.HorizontalDirection;
		}
		if (UserInput.IsMovingVertically)
		{
			wishdir.y = UserInput.VerticalDirection.y;
		}
		ApplyAcceleration(wishdir, _attributes.Speed * SpeedModifier, _currentEnviroment.WaterAcceleration);
		if (_currentVelocity[1] > -3f)
		{
			CharacterMoveController characterMoveController = this;
			CharacterMoveController obj = characterMoveController;
			int index2;
			int index = (index2 = 1);
			float num = obj._currentVelocity[index2];
			characterMoveController._currentVelocity[index] = num - Gravity * 0.1f;
		}
		else
		{
			_currentVelocity[1] = Mathf.Lerp(_currentVelocity[1], -3f, Time.deltaTime * 6f);
		}
	}

	private void MoveOnLatter()
	{
		ApplyFriction();
		Vector3 wishdir = Vector3.zero;
		if (UserInput.IsWalking)
		{
			wishdir = UserInput.Rotation * UserInput.HorizontalDirection;
		}
		if (UserInput.IsMovingVertically)
		{
			wishdir.y = UserInput.VerticalDirection.y;
		}
		ApplyAcceleration(wishdir, _attributes.Speed * SpeedModifier, _currentEnviroment.GroundAcceleration);
	}

	private void MoveOnWaterRim()
	{
		ApplyFriction();
		Vector3 wishdir = Vector3.zero;
		if (UserInput.IsWalking)
		{
			wishdir = UserInput.Rotation * UserInput.HorizontalDirection;
		}
		if (UserInput.IsMovingDown)
		{
			wishdir.y = UserInput.VerticalDirection.y;
		}
		else if (UserInput.IsMovingUp && _waterEnclosure > 0.8f)
		{
			wishdir.y = UserInput.VerticalDirection.y * 0.5f;
		}
		else
		{
			wishdir.y = 0f;
		}
		ApplyAcceleration(wishdir, _attributes.Speed * SpeedModifier, _currentEnviroment.WaterAcceleration, true);
		if (_waterEnclosure < 0.7f || !UserInput.IsMovingVertically)
		{
			if (_currentVelocity[1] > -3f)
			{
				CharacterMoveController characterMoveController = this;
				CharacterMoveController obj = characterMoveController;
				int index2;
				int index = (index2 = 1);
				float num = obj._currentVelocity[index2];
				characterMoveController._currentVelocity[index] = num - Gravity * 0.1f;
			}
			else
			{
				_currentVelocity[1] = Mathf.Lerp(_currentVelocity[1], -3f, Time.deltaTime * 6f);
			}
		}
		else if (_currentVelocity[1] > 0f && _waterEnclosure < 0.8f)
		{
			_currentVelocity[1] = Mathf.Lerp(_currentVelocity[1], -1f, Time.deltaTime * 4f);
		}
		CheckWaterJump();
	}

	private void MoveInAir()
	{
		ApplyFriction();
		Vector3 wishdir = UserInput.Rotation * UserInput.HorizontalDirection;
		wishdir[1] = 0f;
		ApplyAcceleration(wishdir, _attributes.Speed, _currentEnviroment.AirAcceleration);
		CharacterMoveController characterMoveController = this;
		CharacterMoveController obj = characterMoveController;
		int index2;
		int index = (index2 = 1);
		float num = obj._currentVelocity[index2];
		characterMoveController._currentVelocity[index] = num - Gravity;
	}

	private void MoveOnGround()
	{
		if (CheckJump())
		{
			if (WaterLevel > 1)
			{
				MoveInWater();
			}
			else
			{
				MoveInAir();
			}
			return;
		}
		ApplyFriction();
		Vector3 wishdir = Quaternion.Euler(0f, UserInput.Rotation.eulerAngles.y, 0f) * UserInput.HorizontalDirection;
		wishdir[1] = 0f;
		if (wishdir.sqrMagnitude > 1f)
		{
			wishdir.Normalize();
		}
		ApplyAcceleration(wishdir, _attributes.Speed * SpeedModifier, _currentEnviroment.GroundAcceleration);
		_currentVelocity[1] = 0f - Gravity;
	}

	private void SetWaterlevel(int level)
	{
		_waterLevel = level;
		if (GameState.HasCurrentPlayer)
		{
			GameState.LocalCharacter.Set(PlayerStates.DIVING, level == 3);
			GameState.LocalCharacter.Set(PlayerStates.SWIMMING, level == 2);
			GameState.LocalCharacter.Set(PlayerStates.WADING, level == 1);
		}
		else
		{
			Debug.LogError("Failed to set water level!");
		}
	}
}
