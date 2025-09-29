using System;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class LevelCamera : MonoBehaviour, IObserver
{
	private class CameraConfiguration
	{
		public Transform Parent;

		public int CullingMask;

		public float Fov;
	}

	private class CameraBobManager
	{
		private struct BobData
		{
			private float _xAmplitude;

			private float _zAmplitude;

			private float _frequency;

			public float XAmplitude
			{
				get
				{
					return _xAmplitude;
				}
			}

			public float ZAmplitude
			{
				get
				{
					return _zAmplitude;
				}
			}

			public float Frequency
			{
				get
				{
					return _frequency;
				}
			}

			public BobData(float xamp, float zamp, float freq)
			{
				_xAmplitude = xamp;
				_zAmplitude = zamp;
				_frequency = freq;
			}
		}

		private float _strength;

		private BobData _data;

		private BobMode _bobMode;

		private readonly Dictionary<BobMode, BobData> _bobData;

		public BobMode Mode
		{
			get
			{
				return _bobMode;
			}
			set
			{
				if (_bobMode != value)
				{
					_strength = 0f;
					_bobMode = value;
					_data = _bobData[value];
				}
			}
		}

		public CameraBobManager()
		{
			_bobData = new Dictionary<BobMode, BobData>();
			foreach (int value in Enum.GetValues(typeof(BobMode)))
			{
				switch ((BobMode)value)
				{
				case BobMode.Idle:
					_bobData[(BobMode)value] = new BobData(0.2f, 0f, 2f);
					break;
				case BobMode.Crouch:
					_bobData[(BobMode)value] = new BobData(0.8f, 0.8f, 12f);
					break;
				case BobMode.Run:
					_bobData[(BobMode)value] = new BobData(0.5f, 0.3f, 8f);
					break;
				case BobMode.Walk:
					_bobData[(BobMode)value] = new BobData(0.3f, 0.3f, 6f);
					break;
				default:
					_bobData[(BobMode)value] = new BobData(0f, 0f, 0f);
					break;
				}
			}
			_data = _bobData[BobMode.Idle];
		}

		public void Update()
		{
			Transform transform = Instance._transform;
			switch (_bobMode)
			{
			case BobMode.Idle:
			{
				float num4 = Mathf.Sin(Time.time * _data.Frequency);
				transform.rotation = Quaternion.AngleAxis(num4 * _data.XAmplitude * _strength, transform.right) * Quaternion.AngleAxis(num4 * _data.ZAmplitude, transform.forward) * transform.rotation;
				break;
			}
			case BobMode.Walk:
			{
				float num3 = Mathf.Sin(Time.time * _data.Frequency);
				transform.rotation = Quaternion.AngleAxis(Mathf.Abs(num3 * _data.XAmplitude), transform.right) * Quaternion.AngleAxis(num3 * _data.ZAmplitude, transform.forward) * transform.rotation;
				break;
			}
			case BobMode.Run:
			{
				float num2 = Mathf.Sin(Time.time * _data.Frequency);
				transform.rotation = Quaternion.AngleAxis(Mathf.Abs(num2 * _data.XAmplitude * _strength), transform.right) * Quaternion.AngleAxis(num2 * _data.ZAmplitude, transform.forward) * transform.rotation;
				break;
			}
			case BobMode.Swim:
			{
				float angle2 = Mathf.Sin(Time.time * _data.Frequency) * _data.ZAmplitude;
				transform.rotation = Quaternion.AngleAxis(angle2, transform.forward) * transform.rotation;
				break;
			}
			case BobMode.Fly:
			{
				float angle = Mathf.Sin(Time.time * _data.Frequency) * _data.ZAmplitude;
				transform.rotation = Quaternion.AngleAxis(angle, transform.forward) * transform.rotation;
				break;
			}
			case BobMode.Crouch:
			{
				float num = Mathf.Sin(Time.time * _data.Frequency);
				transform.rotation = Quaternion.AngleAxis(Mathf.Abs(num * _data.XAmplitude), transform.right) * Quaternion.AngleAxis(num * _data.ZAmplitude, transform.forward) * transform.rotation;
				break;
			}
			}
			_strength = Mathf.Clamp01(_strength + Time.deltaTime);
		}
	}

	public class FeedbackData
	{
		public float timeToPeak = 0.2f;

		public float timeToEnd = 0.15f;

		public float noise = 0.5f;

		public float angle;

		public float strength = 0.3f;
	}

	private struct Feedback
	{
		public float time;

		public float noise;

		public float angle;

		public float timeToPeak;

		public float timeToEnd;

		public float strength;

		public Vector3 direction;

		public Vector3 rotationAxis;

		private float _angle;

		private float _currentNoise;

		private Vector3 shakePos;

		public float DebugAngle
		{
			get
			{
				return _angle;
			}
		}

		public float Duration
		{
			get
			{
				return timeToPeak + timeToEnd;
			}
		}

		public void HandleFeedback()
		{
			if (Duration == 0f)
			{
				return;
			}
			float num = 0f;
			float num2 = UnityEngine.Random.Range(0f - noise, noise);
			if (time < Duration)
			{
				if (time < timeToPeak)
				{
					num = strength * Mathf.Sin(time * (float)Math.PI * 0.5f / timeToPeak);
					_angle = Mathf.Lerp(0f, angle, time / timeToPeak);
				}
				else
				{
					float t = (time - timeToPeak) / timeToEnd;
					num = strength * Mathf.Cos((time - timeToPeak) * (float)Math.PI * 0.5f / timeToEnd);
					_angle = Mathf.Lerp(_angle, 0f, t);
					if (time != 0f)
					{
						num2 = 0f;
					}
				}
				_currentNoise = Mathf.Lerp(noise, 0f, time / Duration);
				shakePos = Vector3.Lerp(shakePos, UnityEngine.Random.insideUnitSphere * _currentNoise, Time.deltaTime * 30f);
				time += Time.deltaTime;
				Instance._transform.position += num * direction;
				Instance._transform.rotation = Instance._transform.rotation * Quaternion.AngleAxis(_angle, rotationAxis) * Quaternion.AngleAxis(num2, Instance._transform.forward);
			}
			else
			{
				time = 0f;
				timeToEnd = 0f;
				timeToPeak = 0f;
				_angle = 0f;
			}
		}

		public void Reset()
		{
			_angle = 0f;
			time = 0f;
			timeToEnd = 0f;
			timeToPeak = 0f;
		}
	}

	private struct ZoomData
	{
		public float TargetAlpha;

		public float TargetFOV;

		public float Speed;

		private float _alpha;

		public bool IsFovChanged
		{
			get
			{
				return TargetFOV != Instance.FOV;
			}
		}

		public void Update()
		{
			_alpha = Mathf.Lerp(_alpha, TargetAlpha, Time.deltaTime * Speed);
			if ((bool)Instance.MainCamera)
			{
				Instance.MainCamera.fieldOfView = Mathf.Lerp(Instance.MainCamera.fieldOfView, TargetFOV, Time.deltaTime * Speed);
			}
		}

		public void ResetZoom()
		{
			if ((bool)Instance.MainCamera)
			{
				TargetFOV = 60f;
				Instance.MainCamera.fieldOfView = TargetFOV;
			}
		}
	}

	public enum CameraMode
	{
		None = 0,
		Spectator = 1,
		OrbitAround = 2,
		FirstPerson = 3,
		ThirdPerson = 4,
		SmoothFollow = 5,
		FollowTurret = 6,
		Ragdoll = 7,
		Death = 8,
		Overview = 9
	}

	public enum FeedbackType
	{
		JumpLand = 0,
		GetDamage = 1,
		ShootWeapon = 2
	}

	public abstract class CameraState
	{
		protected Vector3 LookAtPosition(Transform target, Quaternion lookRot, Quaternion xRot, Quaternion yRot, float distance, float height)
		{
			Vector3 position = lookRot * Vector3.back * distance;
			return target.up * Instance.LookAtHeight + target.TransformPoint(position);
		}

		protected Quaternion LookAtRotation(Transform target, Quaternion rotation)
		{
			Vector3 direction = rotation * Vector3.forward;
			return Quaternion.LookRotation(target.TransformDirection(direction));
		}

		public abstract void Update();

		public virtual void Finish()
		{
		}

		public virtual void OnDrawGizmos()
		{
		}

		public override string ToString()
		{
			return "Abstract state";
		}
	}

	private class NoneState : CameraState
	{
		public override void Update()
		{
		}
	}

	private class FirstPersonState : CameraState
	{
		private const float _duration = 1f;

		private bool _handleFeedback = true;

		public override void Update()
		{
			if (!(Instance._targetTransform == null))
			{
				Vector3 position = Instance._targetTransform.position + Instance.EyePosition;
				Instance._transform.position = position;
				Instance._transform.rotation = Instance._targetTransform.rotation;
				if (_handleFeedback)
				{
					Instance._feedback.HandleFeedback();
					Instance._bobManager.Update();
				}
				if (Instance._zoomData.IsFovChanged)
				{
					Instance._zoomData.Update();
				}
			}
		}

		public override void Finish()
		{
			Instance._zoomData.ResetZoom();
		}

		public override void OnDrawGizmos()
		{
			Gizmos.DrawRay(Instance._transform.position, Instance._transform.TransformDirection(Instance._feedback.rotationAxis));
		}

		public override string ToString()
		{
			return "FPS state";
		}
	}

	private class ThirdPersonState : CameraState
	{
		private float _right = 1f;

		private float _collideDistance;

		private float _distance = 2.5f;

		private CameraCollisionDetector _ccd;

		private Vector3 TargetCheckPoint
		{
			get
			{
				return Instance._targetTransform.position + Instance._targetTransform.up * Instance.LookAtHeight;
			}
		}

		private Vector3 LeftCheckPoint
		{
			get
			{
				return Instance._transform.position - Instance._transform.right * _right;
			}
		}

		private Vector3 RightCheckPoint
		{
			get
			{
				return Instance._transform.position + Instance._transform.right * _right;
			}
		}

		public ThirdPersonState()
		{
			_collideDistance = _distance / 2f;
			_ccd = new CameraCollisionDetector();
			_ccd.Offset = 1f;
		}

		public override void Update()
		{
			TransformCamera();
			if (Instance._zoomData.IsFovChanged)
			{
				Instance._zoomData.Update();
			}
		}

		public override void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Vector3 up = Instance._targetTransform.up;
			if (Instance._targetTransform != null)
			{
				Gizmos.DrawWireSphere(TargetCheckPoint, 0.1f);
				Quaternion xRot = Quaternion.Euler(Instance._targetTransform.rotation.eulerAngles.x, 0f, 0f);
				Quaternion yRot = Quaternion.Euler(0f, Instance._targetTransform.rotation.eulerAngles.y, 0f);
				Vector3 vector = LookAtPosition(Instance._targetTransform, Quaternion.identity, xRot, yRot, _distance, Instance.LookAtHeight);
				Gizmos.DrawLine(TargetCheckPoint, vector - Instance._targetTransform.right);
				Gizmos.DrawLine(TargetCheckPoint, vector + Instance._targetTransform.right);
			}
			_ccd.OnDrawGizmos();
			Gizmos.color = Color.red;
			Gizmos.DrawRay(Instance._targetTransform.position, Instance._targetTransform.right);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(Instance._targetTransform.position, up);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(Instance._targetTransform.position, Instance._targetTransform.forward);
		}

		private void TransformCamera()
		{
			Vector3 eulerAngles = Instance._targetTransform.rotation.eulerAngles;
			if (eulerAngles.x > 90f)
			{
				eulerAngles.x = Mathf.Clamp(eulerAngles.x, 320f, 360f);
			}
			else
			{
				eulerAngles.x = Mathf.Clamp(eulerAngles.x, 0f, 60f);
			}
			Quaternion xRot = Quaternion.Euler(eulerAngles.x, 0f, 0f);
			Quaternion yRot = Quaternion.Euler(0f, eulerAngles.y, 0f);
			Vector3 to = LookAtPosition(Instance._targetTransform, Quaternion.identity, xRot, yRot, _distance, Instance.LookAtHeight);
			if (_ccd.Detect(TargetCheckPoint, to, Instance._targetTransform.right))
			{
				float distance = _ccd.Distance;
				if (distance < _collideDistance)
				{
					_collideDistance = Mathf.Clamp(distance, 0.5f, _distance);
				}
				else
				{
					_collideDistance = Mathf.Lerp(_collideDistance, distance, Time.deltaTime);
				}
			}
			else
			{
				_collideDistance = Mathf.Lerp(_collideDistance, _distance, Time.deltaTime);
			}
			Instance._transform.position = LookAtPosition(Instance._targetTransform, Quaternion.identity, xRot, yRot, _collideDistance, Instance.LookAtHeight);
			Instance._transform.rotation = Quaternion.LookRotation(TargetCheckPoint - Instance._transform.position);
		}

		public override string ToString()
		{
			return "3rd person state";
		}
	}

	private class SmoothFollowState : CameraState
	{
		private const float _zoomSpeed = 40f;

		private float _collideDistance;

		private float _distance = 1.5f;

		private Quaternion _targetRotationY = Quaternion.identity;

		private Vector3 TargetCheckPoint
		{
			get
			{
				return Instance._targetTransform.position + Instance._targetTransform.up * Instance.LookAtHeight;
			}
		}

		public SmoothFollowState()
		{
			_collideDistance = _distance / 2f;
			Instance.InitUserInput();
		}

		public override void Update()
		{
			float num = AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.NextWeapon);
			if (num != 0f)
			{
				_distance -= Mathf.Sign(num) * 40f * Time.deltaTime;
				_distance = Mathf.Clamp(_distance, 1f, 4f);
			}
			Vector3 eulerAngles = Instance._targetTransform.eulerAngles;
			_targetRotationY = Quaternion.Lerp(_targetRotationY, Quaternion.Euler(0f, eulerAngles.y, 0f), Time.deltaTime * 2f);
			Vector3 targetPosition = Instance._targetTransform.position + Vector3.up * Instance.LookAtHeight;
			Instance.UpdateUserInput();
			Instance.TransformFollowCamera(targetPosition, _targetRotationY, _distance, ref _collideDistance);
		}

		public override string ToString()
		{
			return "Smooth follow state";
		}
	}

	private class OrbitAroundState : CameraState
	{
		private float _distance;

		private float _angle;

		private CameraCollisionDetector _ccd;

		public OrbitAroundState()
		{
			_distance = 1f;
			_angle = 0f;
			_ccd = new CameraCollisionDetector();
			_ccd.Offset = 1f;
		}

		public override void Update()
		{
			Quaternion yRot = Quaternion.Euler(0f, _angle += Time.deltaTime * Instance.OrbitSpeed, 0f);
			Vector3 vector = Instance._targetTransform.position + Vector3.up * Instance.LookAtHeight;
			Vector3 to = LookAtPosition(Instance._targetTransform, Quaternion.identity, Quaternion.identity, yRot, 1f, Instance.LookAtHeight);
			if (_ccd.Detect(vector, to, Instance._transform.right))
			{
				if (_distance < _ccd.Distance)
				{
					_distance = _ccd.Distance;
				}
				else
				{
					_distance = Mathf.Lerp(_distance, _ccd.Distance, Time.deltaTime);
				}
			}
			else
			{
				_distance = Mathf.Lerp(_distance, Instance.OrbitDistance, Time.deltaTime);
			}
			Instance._transform.position = LookAtPosition(Instance._targetTransform, Quaternion.identity, Quaternion.identity, yRot, _distance, Instance.LookAtHeight);
			Instance._transform.rotation = Quaternion.LookRotation(vector - Instance._transform.position);
		}
	}

	private class RagdollState : CameraState
	{
		private const float MaxDuration = 1f;

		private const float MinimalCameraHeight = -20f;

		private Vector3 _targetPosition;

		public RagdollState()
		{
			if (GameState.LocalAvatar.Decorator != null && GameState.LocalAvatar.Ragdoll != null)
			{
				Instance.SetTarget(GameState.LocalAvatar.Ragdoll.GetBone(BoneIndex.Hips));
			}
			if (Instance._targetTransform != null)
			{
				_targetPosition = Instance._targetTransform.position;
			}
		}

		public override void Update()
		{
			if (Instance._targetTransform != null)
			{
				if (_targetPosition.y > -20f && Mathf.Abs(_targetPosition.y - Instance._targetTransform.position.y) > 0.2f)
				{
					_targetPosition = Instance._targetTransform.position;
				}
				Instance._transform.rotation = Quaternion.Slerp(Instance._transform.rotation, Quaternion.LookRotation(_targetPosition - Instance.transform.position), Time.deltaTime * 4f);
				Instance._transform.position = Vector3.Lerp(Instance._transform.position, _targetPosition + new Vector3(0f, 2f, 0f) - Instance._transform.forward * 3f, Time.deltaTime * 4f);
			}
		}
	}

	private class SpectatorState : CameraState
	{
		private const int MaxSpeed = 22;

		private const float verticalSpeed = 0.8f;

		private Vector3 _targetPosition;

		private float _speed = 11f;

		public SpectatorState()
		{
			Vector3 eulerAngles = Instance._transform.rotation.eulerAngles;
			UserInput.SetRotation(eulerAngles.y, eulerAngles.x);
			_targetPosition = Instance._transform.position;
		}

		public override void Update()
		{
			if (!Singleton<InGameChatHud>.Instance.CanInput && Screen.lockCursor)
			{
				UserInput.UpdateDirections();
				int num = ((!UserInput.IsWalking) ? 4 : 6);
				_speed = Mathf.Lerp(_speed, (!UserInput.IsWalking) ? 11 : 22, Time.deltaTime);
				_targetPosition += (UserInput.Rotation * UserInput.HorizontalDirection + UserInput.VerticalDirection * 0.8f) * _speed * Time.deltaTime;
				Instance._transform.position = Vector3.Lerp(Instance._transform.position, _targetPosition, Time.deltaTime * (float)num);
				Instance._transform.rotation = UserInput.Rotation;
			}
		}
	}

	private class DeadState : CameraState
	{
		private const float _zoomSpeed = 40f;

		private bool _isFollowing;

		private float _distance = 1.5f;

		private Vector3 TargetCheckPoint
		{
			get
			{
				return Instance._targetTransform.position + Instance._targetTransform.up * Instance.LookAtHeight;
			}
		}

		public override void Update()
		{
			float num = 1f;
			if (Instance._targetTransform == null)
			{
				return;
			}
			Vector3 vector = Instance._targetTransform.position + Vector3.up * num;
			Vector3 vector2 = vector + Vector3.Normalize(Instance._transform.position - vector) * _distance;
			Quaternion to = Quaternion.LookRotation(vector - Instance._transform.position);
			if (!_isFollowing)
			{
				Instance._transform.position = Vector3.Lerp(Instance._transform.position, vector2, Time.deltaTime * 4f);
				float num2 = Vector3.Distance(vector2, Instance._transform.position);
				if (num2 <= _distance)
				{
					_isFollowing = true;
				}
			}
			Instance._transform.rotation = Quaternion.Lerp(Instance._transform.rotation, to, Time.deltaTime * 4f);
		}

		public override string ToString()
		{
			return "Smooth follow state";
		}
	}

	private class TurretState : CameraState
	{
		public override void Update()
		{
			Instance._transform.position = Vector3.Lerp(Instance._transform.position, Instance._targetTransform.position, Time.deltaTime * 2f);
		}
	}

	public class OverviewState : CameraState
	{
		public const float InitialDistance = 7f;

		private const float FinalDistance = 4f;

		private const float InterpolationSpeed = 3f;

		public static readonly Vector3 ViewDirection = new Vector3(-0.5f, -0.1f, -1f);

		public static readonly Vector3 Offset = new Vector3(0f, 1.5f, 0f);

		private Quaternion _finalRotation;

		private float _distance;

		public OverviewState()
		{
			if (GameState.LocalAvatar.Decorator != null)
			{
				Instance.SetTarget(GameState.LocalAvatar.Decorator.transform);
			}
			if ((bool)Instance._targetTransform)
			{
				_distance = 7f;
				_finalRotation = Quaternion.LookRotation(Instance._targetTransform.TransformDirection(ViewDirection));
				Vector3 b = Instance._targetTransform.TransformPoint(Instance._targetTransform.InverseTransformDirection(_finalRotation * Vector3.back * 4f));
				if (Vector3.Distance(Instance._transform.position, b) > 1f)
				{
					Quaternion quaternion = Quaternion.LookRotation(Instance._targetTransform.TransformDirection(new Vector3(-1f, -1f, 1f)));
					Instance._transform.rotation = quaternion;
					Instance._transform.position = Instance._targetTransform.TransformPoint(Instance._targetTransform.InverseTransformDirection(quaternion * Vector3.back * 7f));
				}
			}
		}

		public override void Update()
		{
			_distance = Mathf.Lerp(_distance, 4f, Time.deltaTime * 3f);
			if ((bool)Instance._targetTransform)
			{
				_finalRotation = Quaternion.LookRotation(Instance._targetTransform.TransformDirection(ViewDirection));
				Quaternion quaternion = Quaternion.Slerp(Instance._transform.rotation, _finalRotation, Time.deltaTime * 3f);
				Vector3 to = Instance._targetTransform.TransformPoint(Instance._targetTransform.InverseTransformDirection(quaternion * Vector3.back * _distance)) + Offset;
				Instance._transform.position = Vector3.Lerp(Instance._transform.position, to, Time.deltaTime * 3f);
				Instance._transform.rotation = quaternion;
			}
		}

		public override string ToString()
		{
			return "Overview State";
		}
	}

	private static LevelCamera _instance;

	private CameraConfiguration _cameraConfiguration = new CameraConfiguration();

	private FeedbackData _jumpFeedback = new FeedbackData();

	private Feedback _feedback;

	private CameraBobManager _bobManager;

	private CameraCollisionDetector _ccd;

	private ZoomData _zoomData;

	private CameraState _currentState;

	private float _height;

	private float _orbitDistance;

	private float _orbitSpeed;

	private bool _isZoomedIn;

	private Vector3 _eyePosition;

	private Transform _transform;

	private Transform _targetTransform;

	private CameraMode _currentMode;

	private Quaternion _userInputCache;

	private Quaternion _userInputRotation;

	public static LevelCamera Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("LevelCamera", typeof(AudioListener), typeof(DontDestroyOnLoad));
				gameObject.layer = 18;
				SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
				sphereCollider.isTrigger = true;
				sphereCollider.radius = 0.01f;
				Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
				_instance = gameObject.AddComponent<LevelCamera>();
			}
			return _instance;
		}
	}

	public bool IsZoomedIn
	{
		get
		{
			return _isZoomedIn;
		}
		set
		{
			_isZoomedIn = false;
		}
	}

	public static bool HasCamera
	{
		get
		{
			return Instance.MainCamera != null;
		}
	}

	public Camera MainCamera { get; private set; }

	public Transform TransformCache
	{
		get
		{
			return _transform;
		}
	}

	public float FOV
	{
		get
		{
			return (!(MainCamera != null)) ? 65f : MainCamera.fieldOfView;
		}
	}

	public Vector3 EyePosition
	{
		get
		{
			return _eyePosition;
		}
	}

	public float LookAtHeight
	{
		get
		{
			return _height;
		}
	}

	public float OrbitDistance
	{
		get
		{
			return _orbitDistance;
		}
	}

	public float OrbitSpeed
	{
		get
		{
			return _orbitSpeed;
		}
	}

	public CameraMode CurrentMode
	{
		get
		{
			return _currentMode;
		}
	}

	public BobMode CurrentBob
	{
		get
		{
			return _bobManager.Mode;
		}
	}

	public bool CanDip { get; set; }

	private void Awake()
	{
		_transform = base.transform;
		_currentState = new NoneState();
		_bobManager = new CameraBobManager();
		_ccd = new CameraCollisionDetector();
		_ccd.Offset = 1f;
		_ccd.LayerMask = 1;
	}

	private void LateUpdate()
	{
		if (_currentMode != CameraMode.SmoothFollow)
		{
			_currentState.Update();
		}
	}

	private void OnDrawGizmos()
	{
		if (_targetTransform != null)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(_targetTransform.position, 0.1f);
			Gizmos.color = Color.white;
		}
	}

	public void Notify()
	{
		if (_currentMode == CameraMode.SmoothFollow)
		{
			_currentState.Update();
		}
	}

	private void InitUserInput()
	{
		Vector3 eulerAngles = UserInput.Rotation.eulerAngles;
		_userInputCache = UserInput.Rotation;
		eulerAngles.x = Mathf.Clamp(eulerAngles.x, 0f, 60f);
		_userInputRotation = Quaternion.Euler(eulerAngles);
	}

	private void UpdateUserInput()
	{
		Vector3 eulerAngles = UserInput.Rotation.eulerAngles;
		float num = _userInputCache.eulerAngles.x;
		float num2 = UserInput.Rotation.eulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		eulerAngles.x = Mathf.Clamp(_userInputRotation.eulerAngles.x + (num2 - num), 0f, 60f);
		_userInputCache = UserInput.Rotation;
		_userInputRotation = Quaternion.Euler(eulerAngles);
	}

	private void TransformFollowCamera(Vector3 targetPosition, Quaternion targetRotation, float distance, ref float collideDistance)
	{
		Vector3 v = _userInputRotation * Vector3.back * collideDistance;
		Matrix4x4 matrix4x = Matrix4x4.TRS(targetPosition, targetRotation, Vector3.one);
		Vector3 vector = matrix4x.MultiplyPoint3x4(v);
		Quaternion quaternion = Quaternion.LookRotation(targetPosition - vector);
		Vector3 to = matrix4x.MultiplyPoint3x4(_userInputRotation * Vector3.back * distance);
		if (_ccd.Detect(targetPosition, to, quaternion * Vector3.right))
		{
			float distance2 = _ccd.Distance;
			if (distance2 < collideDistance)
			{
				collideDistance = Mathf.Clamp(distance2, 1f, distance);
			}
			else
			{
				collideDistance = Mathf.Lerp(collideDistance, distance2, Time.deltaTime * 3f);
			}
		}
		else if (!Mathf.Approximately(collideDistance, distance))
		{
			collideDistance = Mathf.Lerp(collideDistance, distance, Time.deltaTime * 5f);
		}
		else
		{
			collideDistance = distance;
		}
		_transform.position = vector;
		_transform.rotation = quaternion;
	}

	private void TransformDeathCamera(Vector3 targetPosition, Quaternion targetRotation, float distance, ref float collideDistance)
	{
		Vector3 v = Vector3.back * collideDistance;
		Matrix4x4 matrix4x = Matrix4x4.TRS(targetPosition, targetRotation, Vector3.one);
		Vector3 vector = matrix4x.MultiplyPoint3x4(v);
		Quaternion quaternion = Quaternion.LookRotation(targetPosition - vector);
		Vector3 to = matrix4x.MultiplyPoint3x4(Vector3.back * distance);
		if (_ccd.Detect(targetPosition, to, quaternion * Vector3.right))
		{
			float distance2 = _ccd.Distance;
			if (distance2 < collideDistance)
			{
				collideDistance = Mathf.Clamp(distance2, 1f, distance);
			}
			else
			{
				collideDistance = Mathf.Lerp(collideDistance, distance2, Time.deltaTime * 3f);
			}
		}
		else if (!Mathf.Approximately(collideDistance, distance))
		{
			collideDistance = Mathf.Lerp(collideDistance, distance, Time.deltaTime * 5f);
		}
		else
		{
			collideDistance = distance;
		}
		_transform.position = vector;
		_transform.rotation = quaternion;
	}

	public void SetTarget(Transform target)
	{
		_targetTransform = target;
	}

	public void SetLevelCamera(Camera camera, Vector3 position, Quaternion rotation)
	{
		if (camera != MainCamera && camera != null)
		{
			if (MainCamera != null)
			{
				ResetCamera(MainCamera, _cameraConfiguration);
			}
			_cameraConfiguration.Parent = camera.transform.parent;
			_cameraConfiguration.Fov = camera.fieldOfView;
			_cameraConfiguration.CullingMask = camera.cullingMask;
			MainCamera = camera;
			MainCamera.cullingMask = LayerUtil.AddToLayerMask(camera.cullingMask, UberstrikeLayer.LocalProjectile);
			ReparentCamera(camera, base.transform);
			camera.transform.localPosition = Vector3.zero;
			camera.transform.localRotation = Quaternion.identity;
			_zoomData.TargetFOV = camera.fieldOfView;
			_transform.position = position;
			_transform.rotation = rotation;
		}
	}

	public void ReleaseCamera()
	{
		if (MainCamera != null)
		{
			ResetCamera(MainCamera, _cameraConfiguration);
			MainCamera = null;
		}
	}

	private void ResetCamera(Camera camera, CameraConfiguration config)
	{
		camera.fieldOfView = config.Fov;
		camera.cullingMask = config.CullingMask;
		ReparentCamera(camera, config.Parent);
	}

	private void ReparentCamera(Camera camera, Transform parent)
	{
		camera.transform.parent = parent;
	}

	public void SetMode(CameraMode mode)
	{
		if (mode == _currentMode)
		{
			return;
		}
		_feedback.timeToEnd = 0f;
		_currentMode = mode;
		_currentState.Finish();
		if (MainCamera != null)
		{
			switch (mode)
			{
			case CameraMode.FirstPerson:
				MainCamera.cullingMask = LayerUtil.RemoveFromLayerMask(MainCamera.cullingMask, UberstrikeLayer.LocalPlayer, UberstrikeLayer.Weapons);
				MainCamera.cullingMask = LayerUtil.AddToLayerMask(MainCamera.cullingMask, UberstrikeLayer.RemoteProjectile);
				_currentState = new FirstPersonState();
				break;
			case CameraMode.ThirdPerson:
				MainCamera.cullingMask = LayerUtil.AddToLayerMask(MainCamera.cullingMask, UberstrikeLayer.LocalPlayer, UberstrikeLayer.Weapons);
				_currentState = new ThirdPersonState();
				break;
			case CameraMode.SmoothFollow:
				MainCamera.cullingMask = LayerUtil.AddToLayerMask(MainCamera.cullingMask, UberstrikeLayer.LocalPlayer);
				_currentState = new SmoothFollowState();
				break;
			case CameraMode.OrbitAround:
				MainCamera.cullingMask = LayerUtil.AddToLayerMask(MainCamera.cullingMask, UberstrikeLayer.LocalPlayer);
				_currentState = new OrbitAroundState();
				break;
			case CameraMode.Ragdoll:
				MainCamera.cullingMask = LayerUtil.AddToLayerMask(MainCamera.cullingMask, UberstrikeLayer.LocalPlayer);
				_currentState = new RagdollState();
				break;
			case CameraMode.Spectator:
				MainCamera.cullingMask = LayerUtil.AddToLayerMask(MainCamera.cullingMask, UberstrikeLayer.LocalPlayer);
				_currentState = new SpectatorState();
				break;
			case CameraMode.Death:
				_currentState = new DeadState();
				break;
			case CameraMode.FollowTurret:
				_currentState = new TurretState();
				break;
			case CameraMode.None:
				_currentState = new NoneState();
				break;
			case CameraMode.Overview:
				_currentState = new OverviewState();
				break;
			default:
				Debug.LogError("Camera does not support " + mode);
				break;
			}
		}
	}

	public void SetEyePosition(float x, float y, float z)
	{
		_eyePosition = new Vector3(x, y, z);
	}

	public void SetLookAtHeight(float height)
	{
		_height = height;
	}

	public void SetOrbitDistance(float distance)
	{
		_orbitDistance = distance;
	}

	public void SetOrbitSpeed(float speed)
	{
		_orbitSpeed = speed;
	}

	public static void SetBobMode(BobMode mode)
	{
		if (Instance != null)
		{
			Instance._bobManager.Mode = mode;
			if (WeaponFeedbackManager.Exists)
			{
				WeaponFeedbackManager.Instance.SetBobMode(mode);
			}
		}
	}

	public void DoFeedback(FeedbackType type, Vector3 direction, float strength, float noise, float timeToPeak, float timeToEnd, float angle, Vector3 axis)
	{
		_feedback.time = 0f;
		_feedback.noise = noise / 4f;
		_feedback.strength = strength;
		_feedback.timeToPeak = timeToPeak;
		_feedback.timeToEnd = timeToEnd;
		_feedback.direction = direction;
		_feedback.angle = angle;
		_feedback.rotationAxis = axis;
	}

	public bool DoLandFeedback(bool shake)
	{
		if (_currentMode == CameraMode.FirstPerson && CanDip && (_feedback.time == 0f || _feedback.time >= _feedback.Duration))
		{
			_feedback.time = 0f;
			_feedback.angle = _jumpFeedback.angle;
			_feedback.noise = ((!shake) ? 0f : _jumpFeedback.noise);
			_feedback.strength = _jumpFeedback.strength;
			_feedback.timeToPeak = _jumpFeedback.timeToPeak;
			_feedback.timeToEnd = _jumpFeedback.timeToEnd;
			_feedback.direction = Vector3.down;
			_feedback.rotationAxis = Vector3.right;
			WeaponFeedbackManager.Instance.LandingDip();
			return true;
		}
		return false;
	}

	public void DoZoomIn(float fov, float speed)
	{
		if (fov < 1f || fov > 100f || speed < 0.001f || speed > 100f)
		{
			Debug.LogError("Invalid parameters specified!\n FOV should be >1 & <100, Speed should be >0.001 & <100.\nFOV = " + fov + " Speed = " + speed);
		}
		else if (!_isZoomedIn || fov != FOV)
		{
			_zoomData.Speed = speed;
			_zoomData.TargetFOV = fov;
			_zoomData.TargetAlpha = 1f;
			_isZoomedIn = true;
		}
	}

	public void DoZoomOut(float fov, float speed)
	{
		if (fov < 1f || fov > 100f || speed < 0.001f || speed > 100f)
		{
			Debug.LogError("Invalid parameters specified!\n FOV should be >1 & <100, Speed should be >0.001 & <100.\nFOV = " + fov + " Speed = " + speed);
		}
		else if (_isZoomedIn)
		{
			_zoomData.Speed = speed;
			_zoomData.TargetFOV = fov;
			_zoomData.TargetAlpha = 0f;
			_isZoomedIn = false;
			CmuneEventHandler.Route(new OnCameraZoomOutEvent());
		}
	}

	public void ResetZoom()
	{
		_isZoomedIn = false;
		_zoomData.ResetZoom();
	}

	public Ray ScreenPointToRay(Vector3 point)
	{
		if ((bool)MainCamera)
		{
			return MainCamera.ScreenPointToRay(point);
		}
		return default(Ray);
	}

	public void ResetFeedback()
	{
		_feedback.Reset();
	}
}
