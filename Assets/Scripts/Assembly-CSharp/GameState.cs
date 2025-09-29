using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameState : MonoBehaviour
{
	public const float InterpolationFactor = 10f;

	public static bool IsRagdollShootable = false;

	[SerializeField]
	private LocalPlayer _playerPrefab;

	private LocalPlayer _currentPlayer;

	private static FpsGameMode _currentGameMode;

	private static UberStrike.Realtime.UnitySdk.CharacterInfo _localCharacter = new UberStrike.Realtime.UnitySdk.CharacterInfo();

	public static GameState Instance { get; private set; }

	public static LocalPlayer LocalPlayer
	{
		get
		{
			return Instance._currentPlayer;
		}
	}

	public static UberStrike.Realtime.UnitySdk.CharacterInfo LocalCharacter
	{
		get
		{
			return _localCharacter;
		}
	}

	public static Avatar LocalAvatar { get; set; }

	public static bool IsShuttingDown { get; private set; }

	public static bool UsePlayerPing
	{
		get
		{
			return true;
		}
	}

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public static bool IsReadyForNextGame { get; set; }

	public static Transform WeaponCameraTransform
	{
		get
		{
			return LocalPlayer.WeaponCamera.transform;
		}
	}

	public static int CurrentPlayerID
	{
		get
		{
			return (LocalCharacter != null) ? LocalCharacter.ActorId : 0;
		}
	}

	public static bool HasCurrentPlayer
	{
		get
		{
			return Exists && Instance._currentPlayer != null;
		}
	}

	public static GameMode CurrentGameMode
	{
		get
		{
			return HasCurrentGame ? CurrentGame.GameMode : GameMode.None;
		}
	}

	public static bool IsSinglePlayer
	{
		get
		{
			return !GameStateController.IsMultiplayerGameMode((int)CurrentGameMode);
		}
	}

	public static FpsGameMode CurrentGame
	{
		get
		{
			return _currentGameMode;
		}
		set
		{
			if (_currentGameMode != null)
			{
				_currentGameMode.Dispose();
			}
			_currentGameMode = value;
		}
	}

	public static bool HasCurrentGame
	{
		get
		{
			return _currentGameMode != null;
		}
	}

	public static MapConfiguration CurrentSpace { get; set; }

	public static bool HasCurrentSpace
	{
		get
		{
			return CurrentSpace != null;
		}
	}

	public static event Action DrawGizmos;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		LocalAvatar = new Avatar(Singleton<LoadoutManager>.Instance.GearLoadout, true);
		_currentPlayer = UnityEngine.Object.Instantiate(_playerPrefab) as LocalPlayer;
		UnityEngine.Object.DontDestroyOnLoad(_currentPlayer.gameObject);
	}

	public void Reset()
	{
		AvatarBuilder.Destroy(LocalAvatar.Decorator.gameObject);
		AvatarBuilder.Destroy(_currentPlayer.gameObject);
		_currentPlayer = UnityEngine.Object.Instantiate(_playerPrefab) as LocalPlayer;
		UnityEngine.Object.DontDestroyOnLoad(_currentPlayer.gameObject);
	}

	private void FixedUpdate()
	{
		if (CurrentGame != null)
		{
			CurrentGame.FixedUpdate();
		}
	}

	private void Update()
	{
		if (CurrentGame != null)
		{
			CurrentGame.Update();
		}
		Singleton<GameStateController>.Instance.StateMachine.Update();
	}

	private void OnGUI()
	{
		Singleton<GameStateController>.Instance.StateMachine.OnGUI();
	}

	private void OnDrawGizmos()
	{
		if (GameState.DrawGizmos != null)
		{
			GameState.DrawGizmos();
		}
	}

	private void OnApplicationQuit()
	{
		IsShuttingDown = true;
	}
}
