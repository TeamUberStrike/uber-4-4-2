using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CharacterMoveSimulator
{
	private Transform _transform;

	private IObserver _positionObserver;

	public CharacterMoveSimulator(Transform transform)
	{
		_transform = transform;
	}

	public void Update(UberStrike.Realtime.UnitySdk.CharacterInfo state)
	{
		if (state != null)
		{
			_transform.localPosition = state.Position;
			_transform.localRotation = Quaternion.Lerp(_transform.rotation, state.HorizontalRotation, Time.deltaTime * 5f);
			if (_positionObserver != null)
			{
				_positionObserver.Notify();
			}
		}
	}

	public void AddPositionObserver(IObserver observer)
	{
		_positionObserver = observer;
	}

	public void RemovePositionObserver()
	{
		_positionObserver = null;
	}
}
