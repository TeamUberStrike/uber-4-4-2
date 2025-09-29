using System;
using System.Collections;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameStateController : Singleton<GameStateController>
{
	private TrainingState _trainingState;

	private ShopTryWeaponState _shopTryWeaponState;

	private GearTestState _gearTestState;

	private WeaponTestState _weaponTestState;

	private DeathMatchState _deathMatchState;

	private TeamDeathMatchState _teamDeathMatchState;

	public StateMachine StateMachine { get; private set; }

	private GameStateController()
	{
		StateMachine = new StateMachine();
		_trainingState = new TrainingState();
		_shopTryWeaponState = new ShopTryWeaponState();
		_gearTestState = new GearTestState(null);
		_weaponTestState = new WeaponTestState();
		_deathMatchState = new DeathMatchState();
		_teamDeathMatchState = new TeamDeathMatchState();
		StateMachine.RegisterState(10, _deathMatchState);
		StateMachine.RegisterState(11, _teamDeathMatchState);
		StateMachine.RegisterState(14, _shopTryWeaponState);
		StateMachine.RegisterState(15, _weaponTestState);
		StateMachine.RegisterState(16, _gearTestState);
		StateMachine.RegisterState(13, _trainingState);
		StateMachine.RegisterState(12, new TutorialState());
	}

	public void CreateGame(UberstrikeMap map, string name = " ", string password = "", int timeMinutes = 0, int killLimit = 1, int playerLimit = 1, GameModeType mode = GameModeType.None, GameFlags.GAME_FLAGS flags = GameFlags.GAME_FLAGS.None)
	{
		GameMetaData gameMetaData = new GameMetaData(0, name, (Singleton<GameServerController>.Instance.SelectedServer == null) ? string.Empty : Singleton<GameServerController>.Instance.SelectedServer.ConnectionString, map.Id, password, timeMinutes, playerLimit, mode.GetGameModeID());
		ApplicationDataManager.EventsSystem.SendCreateGame(PlayerDataManager.CmidSecure, map.Id, timeMinutes, (short)mode, killLimit);
		gameMetaData.GameModifierFlags = (int)flags;
		gameMetaData.SplatLimit = killLimit;
		gameMetaData.Password = password;
		JoinGame(gameMetaData);
	}

	public void JoinGame(GameMetaData data)
	{
		if (data != null)
		{
			UberstrikeMap mapWithId = Singleton<MapManager>.Instance.GetMapWithId(data.MapID);
			if (mapWithId != null)
			{
				PickupItem.ResetInstanceCounter();
				Singleton<MapManager>.Instance.LoadMap(mapWithId, delegate
				{
					CreateGame(data);
				});
			}
		}
		else
		{
			Debug.LogError("JoinGame failed because GameMetaData is null");
		}
		Singleton<ItemLoader>.Instance.Paused = true;
	}

	public void LeaveGame()
	{
		ApplicationDataManager.EventsSystem.SendLeaveGame(PlayerDataManager.CmidSecure, AutoMonoBehaviour<GameConnectionManager>.Instance.GameID, AutoMonoBehaviour<GameConnectionManager>.Instance.RoundsPlayed, !GameState.CurrentGame.IsGameStarted);
		UnloadGameMode();
		Singleton<MapManager>.Instance.UnloadMapBundle();
		Singleton<ItemLoader>.Instance.Paused = false;
		MonoRoutine.Start(StartLoadMenu());
	}

	private void CreateGame(GameMetaData game)
	{
		GameState.LocalAvatar.RebuildDecorator();
		LobbyConnectionManager.Stop();
		if (!AutoMonoBehaviour<GameConnectionManager>.Instance.IsConnectedToServer(game.ServerConnection))
		{
			GameConnectionManager.Stop();
		}
		if (IsMultiplayerGameMode(game.GameMode))
		{
			GameConnectionManager.Start(game);
		}
		GameState.LocalPlayer.SetEnabled(true);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		LoadGameMode((GameMode)game.GameMode, game);
	}

	public void SpectateCurrentGame()
	{
		if (GameState.HasCurrentGame)
		{
			ModeratorGameMode.ModerateGameMode(GameState.CurrentGame);
		}
		else
		{
			Debug.LogError("SpectateCurrentGame: GameState doesn't has any game!");
		}
	}

	public void LoadTryWeaponMode(int itemId = 0)
	{
		AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
		_shopTryWeaponState.ItemId = itemId;
		StateMachine.SetState(14);
	}

	public void LoadTestWeaponMode(int itemId = 0)
	{
		AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
		_shopTryWeaponState.ItemId = itemId;
		StateMachine.SetState(15);
	}

	public void LoadTestGearMode(ILoadout gearLoadout)
	{
		AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
		_gearTestState.Loadout = gearLoadout;
		StateMachine.SetState(16);
	}

	public static bool IsMultiplayerGameMode(int mode)
	{
		if (mode == 100 || mode == 101)
		{
			return true;
		}
		return false;
	}

	public void LoadGameMode(GameMode mode, GameMetaData data = null)
	{
		switch (mode)
		{
		case GameMode.DeathMatch:
			_deathMatchState.GameMetaData = data;
			StateMachine.SetState(10);
			break;
		case GameMode.TeamDeathMatch:
			_teamDeathMatchState.GameMetaData = data;
			StateMachine.SetState(11);
			break;
		case GameMode.Tutorial:
			StateMachine.SetState(12);
			break;
		case GameMode.Training:
			_trainingState.MapId = data.MapID;
			StateMachine.SetState(13);
			break;
		default:
			throw new NotImplementedException(string.Concat("The Game mode ", mode, " is not supported"));
		}
	}

	public void UnloadGameMode()
	{
		StateMachine.PopAllStates();
		Singleton<ItemLoader>.Instance.UnloadAll();
	}

	private IEnumerator StartLoadMenu()
	{
		yield return Singleton<SceneLoader>.Instance.LoadLevel("Menu");
		if (Singleton<GameServerController>.Instance.SelectedServer != null)
		{
			Singleton<GameServerController>.Instance.JoinLastGameServer();
		}
	}
}
