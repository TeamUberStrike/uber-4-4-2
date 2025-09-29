using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class SinglePlayer : MonoBehaviour
{
	private AvatarDecorator _decorator;

	private GameMode _gameMode = GameMode.DeathMatch;

	private TeamID _team;

	private int _spawnPoint;

	[SerializeField]
	private Transform _firstPersonWeapons;

	[SerializeField]
	private Transform _thirdPersonWeapons;

	public Transform FirstPersonWeapons
	{
		get
		{
			return _firstPersonWeapons;
		}
	}

	public Transform ThirdPersonWeapons
	{
		get
		{
			return _thirdPersonWeapons;
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		if (GameState.CurrentSpace != null)
		{
			LevelCamera.Instance.SetLevelCamera(GameState.CurrentSpace.Camera, GameState.CurrentSpace.DefaultViewPoint.position, GameState.CurrentSpace.DefaultViewPoint.rotation);
			GameState.LocalPlayer.InitializePlayer();
			GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.FirstPerson);
			GameState.LocalPlayer.SetWeaponControlState(PlayerHudState.Playing);
			Singleton<SpawnPointManager>.Instance.ConfigureSpawnPoints(GameState.CurrentSpace.SpawnPoints.GetComponentsInChildren<SpawnPoint>(true));
			GameState.LocalPlayer.Pause();
			_decorator = GetComponentInChildren<AvatarDecorator>();
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 150f, 30f), "Game Modes:");
		if (GUI.Toggle(new Rect(160f, 10f, 60f, 30f), _gameMode == GameMode.DeathMatch, "DM") && GUI.changed)
		{
			_gameMode = GameMode.DeathMatch;
		}
		if (GUI.Toggle(new Rect(220f, 10f, 60f, 30f), _gameMode == GameMode.TeamDeathMatch, "TDM") && GUI.changed)
		{
			_gameMode = GameMode.TeamDeathMatch;
		}
		GUI.Label(new Rect(10f, 40f, 150f, 30f), "Teams:");
		if (GUI.Toggle(new Rect(160f, 40f, 60f, 30f), _team == TeamID.NONE, "NONE") && GUI.changed)
		{
			_team = TeamID.NONE;
		}
		if (GUI.Toggle(new Rect(220f, 40f, 60f, 30f), _team == TeamID.RED, "RED") && GUI.changed)
		{
			_team = TeamID.RED;
		}
		if (GUI.Toggle(new Rect(280f, 40f, 60f, 30f), _team == TeamID.BLUE, "BLUE") && GUI.changed)
		{
			_team = TeamID.BLUE;
		}
		GUI.Label(new Rect(10f, 70f, 150f, 30f), "Points:");
		for (int i = 0; i < Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(_gameMode, _team); i++)
		{
			if (GUI.Toggle(new Rect(160 + 30 * i, 70f, 30f, 20f), _spawnPoint == i, string.Empty + (i + 1)) && GUI.changed)
			{
				_spawnPoint = i;
				Respawn();
			}
		}
		if (Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(_gameMode, _team) == 0)
		{
			GUI.Label(new Rect(160f, 70f, 200f, 20f), "No points found!");
		}
		if (!Screen.lockCursor && GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2, 200f, 30f), "CONTINUE"))
		{
			GameState.LocalPlayer.UnPausePlayer();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			_spawnPoint = (_spawnPoint + 1) % Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(_gameMode, _team);
			Respawn();
		}
		if ((bool)_decorator)
		{
			_decorator.transform.position = GameState.LocalCharacter.Position;
			_decorator.transform.rotation = GameState.LocalCharacter.HorizontalRotation;
		}
	}

	private void Respawn()
	{
		Vector3 position;
		Quaternion rotation;
		Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(_spawnPoint, _gameMode, _team, out position, out rotation);
		GameState.LocalPlayer.SpawnPlayerAt(position, rotation);
	}
}
