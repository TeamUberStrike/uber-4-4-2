using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public abstract class ClientGameMode : ClientNetworkClass, IGameMode
{
	private Dictionary<int, UberStrike.Realtime.UnitySdk.CharacterInfo> _players;

	private GameMetaData _gameData;

	public bool HasJoinedGame { get; protected set; }

	public bool IsMatchRunning { get; protected set; }

	public bool IsGameStarted { get; private set; }

	public GameMetaData GameData
	{
		get
		{
			return _gameData;
		}
	}

	public Dictionary<int, UberStrike.Realtime.UnitySdk.CharacterInfo> Players
	{
		get
		{
			return _players;
		}
	}

	public int MyActorId
	{
		get
		{
			return _rmi.Messenger.PeerListener.ActorId;
		}
	}

	protected ClientGameMode(RemoteMethodInterface rmi, GameMetaData gameData)
		: base(rmi)
	{
		_gameData = gameData;
		_players = new Dictionary<int, UberStrike.Realtime.UnitySdk.CharacterInfo>();
	}

	public UberStrike.Realtime.UnitySdk.CharacterInfo GetPlayerWithID(int actorId)
	{
		if (actorId == GameState.LocalCharacter.ActorId)
		{
			return GameState.LocalCharacter;
		}
		UberStrike.Realtime.UnitySdk.CharacterInfo value;
		if (!Players.TryGetValue(actorId, out value))
		{
		}
		return value;
	}

	[NetworkMethod(1)]
	protected virtual void OnPlayerJoined(SyncObject data, Vector3 position)
	{
		if (data.IsEmpty)
		{
			Debug.LogError("ClientGameMode: OnPlayerJoined - SyncObject is empty!");
		}
		else if (data.Id == GameState.LocalCharacter.ActorId)
		{
			GameState.LocalCharacter.ReadSyncData(data);
			if (!GameState.LocalCharacter.IsSpectator)
			{
				Players[data.Id] = GameState.LocalCharacter;
				HasJoinedGame = true;
			}
		}
		else
		{
			Players[data.Id] = new UberStrike.Realtime.UnitySdk.CharacterInfo(data);
			Players[data.Id].Position = position;
		}
	}

	[NetworkMethod(2)]
	protected virtual void OnPlayerLeft(int actorId)
	{
		if (Players.Remove(actorId))
		{
		}
		if (actorId == MyActorId)
		{
			HasJoinedGame = false;
		}
	}

	[NetworkMethod(4)]
	protected virtual void OnFullPlayerListUpdate(List<SyncObject> data, List<Vector3> positions)
	{
		for (int i = 0; i < data.Count && i < positions.Count; i++)
		{
			OnPlayerJoined(data[i], positions[i]);
		}
	}

	[NetworkMethod(5)]
	protected virtual void OnGameFrameUpdate(List<SyncObject> data)
	{
		foreach (SyncObject datum in data)
		{
			UberStrike.Realtime.UnitySdk.CharacterInfo value;
			if (!datum.IsEmpty && Players.TryGetValue(datum.Id, out value))
			{
				value.ReadSyncData(datum);
			}
		}
	}

	[NetworkMethod(3)]
	protected virtual void OnPlayerUpdate(SyncObject data)
	{
		UberStrike.Realtime.UnitySdk.CharacterInfo value;
		if (!data.IsEmpty && Players.TryGetValue(data.Id, out value))
		{
			value.ReadSyncData(data);
		}
	}

	[NetworkMethod(21)]
	protected virtual void OnStartGame()
	{
		IsGameStarted = true;
	}

	[NetworkMethod(22)]
	protected virtual void OnStopGame()
	{
		IsGameStarted = false;
	}
}
