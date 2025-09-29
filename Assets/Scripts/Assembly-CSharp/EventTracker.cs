using UnityEngine;

public class EventTracker : AutoMonoBehaviour<EventTracker>
{
	private float _lastTimerSet;

	private float appLastBackgrounded;

	public int ClicksOnPage { get; private set; }

	public float TimeOnPage
	{
		get
		{
			return Time.realtimeSinceStartup - _lastTimerSet;
		}
	}

	private new void Start()
	{
		_lastTimerSet = Time.realtimeSinceStartup;
		ClicksOnPage = 0;
		appLastBackgrounded = 0f;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			ClicksOnPage++;
		}
	}

	public void Reset()
	{
		_lastTimerSet = Time.realtimeSinceStartup;
		ClicksOnPage = 0;
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			appLastBackgrounded = Time.realtimeSinceStartup;
			ApplicationDataManager.EventsSystem.SendAppBackgrounded();
		}
		else
		{
			float timeAway = Time.realtimeSinceStartup - appLastBackgrounded;
			ApplicationDataManager.EventsSystem.SendAppReturnFromBackground(timeAway, GameState.HasCurrentGame);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			float timeAway = Time.realtimeSinceStartup - appLastBackgrounded;
			ApplicationDataManager.EventsSystem.SendAppReturnFromBackground(timeAway, GameState.HasCurrentGame);
		}
		else
		{
			appLastBackgrounded = Time.realtimeSinceStartup;
			ApplicationDataManager.EventsSystem.SendAppBackgrounded();
		}
	}

	private void OnApplicationQuit()
	{
		ApplicationDataManager.EventsSystem.SendAppQuit();
	}
}
