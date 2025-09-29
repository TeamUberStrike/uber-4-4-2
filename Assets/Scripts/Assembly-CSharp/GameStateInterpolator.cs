using System;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameStateInterpolator
{
	public enum INTERPOLATION_STATE
	{
		RUNNING = 0,
		PAUSED = 1
	}

	private Dictionary<int, RemoteCharacterState> _remoteStateByID;

	private Dictionary<byte, RemoteCharacterState> _remoteStateByNumber;

	private float _timeDelta = 0.01f;

	private INTERPOLATION_STATE _internalState;

	public float SimulationFrequency
	{
		get
		{
			return _timeDelta;
		}
		set
		{
			_timeDelta = value;
		}
	}

	public GameStateInterpolator()
	{
		_remoteStateByID = new Dictionary<int, RemoteCharacterState>(20);
		_remoteStateByNumber = new Dictionary<byte, RemoteCharacterState>(20);
		_internalState = INTERPOLATION_STATE.PAUSED;
	}

	public void Interpolate()
	{
		if (_internalState != INTERPOLATION_STATE.RUNNING)
		{
			return;
		}
		foreach (RemoteCharacterState value in _remoteStateByID.Values)
		{
			value.Interpolate(GameConnectionManager.Client.PeerListener.ServerTimeTicks - 500);
		}
	}

	public void Run()
	{
		_internalState = INTERPOLATION_STATE.RUNNING;
	}

	public void Pause()
	{
		_internalState = INTERPOLATION_STATE.PAUSED;
	}

	public void UpdateCharacterInfo(SyncObject update)
	{
		RemoteCharacterState value;
		if (_remoteStateByID.TryGetValue(update.Id, out value))
		{
			value.RecieveDeltaUpdate(update);
		}
		else
		{
			Debug.LogWarning("UpdateUberStrike.Realtime.UnitySdk.CharacterInfo but state not found for actor " + update.Id);
		}
	}

	public void UpdatePositionSmooth(List<PlayerPosition> all)
	{
		foreach (PlayerPosition item in all)
		{
			RemoteCharacterState value;
			if (_remoteStateByNumber.TryGetValue(item.Player, out value))
			{
				value.UpdatePositionSmooth(item);
			}
		}
	}

	public void UpdatePositionHard(byte playerNumber, Vector3 pos)
	{
		RemoteCharacterState value;
		if (_remoteStateByNumber.TryGetValue(playerNumber, out value))
		{
			value.SetHardPosition(GameConnectionManager.Client.PeerListener.ServerTimeTicks, pos);
			return;
		}
		Debug.LogWarning("UpdatePositionSmooth failed for " + playerNumber + " " + pos);
	}

	public void AddCharacterInfo(UberStrike.Realtime.UnitySdk.CharacterInfo user)
	{
		_remoteStateByID[user.ActorId] = new RemoteCharacterState(user);
		_remoteStateByNumber[user.PlayerNumber] = _remoteStateByID[user.ActorId];
	}

	public void RemoveCharacterInfo(int playerID)
	{
		RemoteCharacterState value;
		if (_remoteStateByID.TryGetValue(playerID, out value))
		{
			_remoteStateByNumber.Remove(value.Info.PlayerNumber);
		}
	}

	public RemoteCharacterState GetState(int playerID)
	{
		RemoteCharacterState value;
		if (_remoteStateByID.TryGetValue(playerID, out value))
		{
			return value;
		}
		throw new Exception(string.Format("GameStateInterpolator:GetPlayerState({0}) failed because CharacterState was not inserted", playerID));
	}
}
