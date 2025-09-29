using UnityEngine;

internal class InGameCountdown
{
	private int _remainingSeconds;

	public int EndTime { get; set; }

	public int RemainingSeconds
	{
		get
		{
			return _remainingSeconds;
		}
		private set
		{
			if (_remainingSeconds != value)
			{
				_remainingSeconds = value;
				OnUpdateRemainingSeconds();
			}
		}
	}

	public void Stop()
	{
		RemainingSeconds = 0;
	}

	public void Update()
	{
		int num = Mathf.CeilToInt((float)(EndTime - GameConnectionManager.Client.PeerListener.ServerTimeTicks) / 1000f);
		if (RemainingSeconds != num)
		{
			Singleton<MatchStatusHud>.Instance.RemainingSeconds = num;
			RemainingSeconds = num;
		}
	}

	private void OnUpdateRemainingSeconds()
	{
		Singleton<MatchStatusHud>.Instance.RemainingSeconds = _remainingSeconds;
	}
}
