using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class LocalCharacterState : ICharacterState
{
	private const int PlayerSyncMask = 337920000;

	private int posSyncFrame;

	private int bagSyncFrame;

	private Vector3 lastPosition;

	private int sendCounter;

	private UberStrike.Realtime.UnitySdk.CharacterInfo _myInfo;

	private FpsGameMode _game;

	public UberStrike.Realtime.UnitySdk.CharacterInfo Info
	{
		get
		{
			return _myInfo;
		}
	}

	public Vector3 LastPosition
	{
		get
		{
			return _myInfo.Position;
		}
	}

	private event Action<SyncObject> _updateRecievedEvent;

	public LocalCharacterState(UberStrike.Realtime.UnitySdk.CharacterInfo info, FpsGameMode game)
	{
		_myInfo = info;
		_game = game;
		posSyncFrame = SystemTime.Running;
		bagSyncFrame = SystemTime.Running;
	}

	public void RecieveDeltaUpdate(SyncObject data)
	{
		data.DeltaCode &= 337920000;
		if (data.DeltaCode != 0)
		{
			_myInfo.ReadSyncData(data, 337920000);
			if (this._updateRecievedEvent != null)
			{
				this._updateRecievedEvent(data);
			}
		}
	}

	public void SubscribeToEvents(CharacterConfig config)
	{
		this._updateRecievedEvent = null;
		this._updateRecievedEvent = (Action<SyncObject>)Delegate.Combine(this._updateRecievedEvent, new Action<SyncObject>(config.OnCharacterStateUpdated));
	}

	public void UnSubscribeAll()
	{
		this._updateRecievedEvent = null;
	}

	public void SendUpdates()
	{
		if (SystemTime.Running >= posSyncFrame)
		{
			posSyncFrame = SystemTime.Running + 50;
			if (lastPosition != GameState.LocalCharacter.Position)
			{
				sendCounter = 0;
			}
			else
			{
				sendCounter++;
			}
			if (sendCounter < 10)
			{
				lastPosition = GameState.LocalCharacter.Position;
				_game.SendPositionUpdate();
			}
		}
		if (SystemTime.Running >= bagSyncFrame)
		{
			bagSyncFrame = SystemTime.Running + 100;
			_game.SendCharacterInfoUpdate();
		}
	}
}
