using UnityEngine;

public class KillComboCounter
{
	private const float MultiKillInterval = 10f;

	private float _lastKillTime;

	private int _killCounter;

	public void OnKillEnemy()
	{
		if (Time.time < _lastKillTime + 10f)
		{
			_killCounter++;
			_lastKillTime = Time.time;
			Singleton<PopupHud>.Instance.PopupMultiKill(_killCounter);
		}
		else
		{
			_killCounter = 1;
			_lastKillTime = Time.time;
		}
	}

	public void ResetCounter()
	{
		_killCounter = 0;
	}
}
