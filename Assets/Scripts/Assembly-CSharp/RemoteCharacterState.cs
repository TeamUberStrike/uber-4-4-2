using System;
using System.Text;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class RemoteCharacterState : ICharacterState
{
	private int _counter;

	private int _lastInterpolation;

	private BezierSplines _interpolator;

	private UberStrike.Realtime.UnitySdk.CharacterInfo _currentState;

	public UberStrike.Realtime.UnitySdk.CharacterInfo Info
	{
		get
		{
			return _currentState;
		}
	}

	public Vector3 LastPosition
	{
		get
		{
			return _interpolator.LatestPosition();
		}
	}

	private event Action<SyncObject> _updateRecievedEvent;

	public RemoteCharacterState(UberStrike.Realtime.UnitySdk.CharacterInfo info)
	{
		_currentState = info;
		_interpolator = new BezierSplines();
		SetHardPosition(GameConnectionManager.Client.PeerListener.ServerTimeTicks, info.Position);
	}

	public BezierSplines GetPositionInterpolator()
	{
		return _interpolator;
	}

	public void RecieveDeltaUpdate(SyncObject delta)
	{
		_currentState.ReadSyncData(delta);
		if (this._updateRecievedEvent != null)
		{
			this._updateRecievedEvent(delta);
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

	public void UpdatePositionSmooth(PlayerPosition p)
	{
		_counter++;
		_interpolator.AddSample(p.Time, p.Position);
	}

	public void SetHardPosition(int time, Vector3 pos)
	{
		_interpolator.Packets.Clear();
		_interpolator.LastPacket.Time = 0;
		_interpolator.AddSample(time, pos);
		_interpolator.PreviousPacket = _interpolator.LastPacket;
		_currentState.Position = pos;
	}

	public void Interpolate(int time)
	{
		if (_currentState.IsAlive)
		{
			Vector3 oPos;
			_lastInterpolation = _interpolator.ReadPosition(time, out oPos);
			if (_lastInterpolation > 0)
			{
				_currentState.Position = oPos;
				_currentState.Distance = Vector3.Distance(_interpolator.LastPacket.Pos, oPos);
			}
		}
	}

	internal void UpdatePosition()
	{
		Vector3 vector = _interpolator.LastPacket.Pos - _interpolator.PreviousPacket.Pos;
		Vector3 to = _interpolator.PreviousPacket.Pos + vector * Time.deltaTime;
		_currentState.Position = Vector3.Lerp(_currentState.Position, to, Time.deltaTime * 10f);
		_currentState.Position.y = Mathf.Lerp(_currentState.Position.y, _interpolator.LastPacket.Pos.y, Time.deltaTime * 10f);
		_currentState.Velocity = vector.magnitude;
		_currentState.Distance = Vector3.Distance(_interpolator.LastPacket.Pos, _currentState.Position);
	}

	public string DebugAll()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("time: {0}/{1} ", _lastInterpolation, _interpolator.Packets.Count);
		stringBuilder.AppendFormat("cntr: {0}\n", _counter);
		return stringBuilder.ToString();
	}
}
