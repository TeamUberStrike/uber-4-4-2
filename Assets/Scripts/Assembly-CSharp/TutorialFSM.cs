using System.Collections.Generic;

public class TutorialFSM
{
	public enum State
	{
		Init = 0,
		AirlockCameraFollow = 1,
		AirlockWelcome = 2,
		AirlockMouseLook = 3,
		AirlockWASDWalk = 4,
		AirlockDoorOpen = 5,
		Tunnel = 6,
		Armory = 7,
		ShootingArea = 8,
		HeadingToFight = 9,
		ArenaField = 10,
		Fini = 11
	}

	private int cur_state;

	private List<ITutorialStateTransitionObserver> observers = new List<ITutorialStateTransitionObserver>();

	public State CurrentState
	{
		get
		{
			return (State)cur_state;
		}
	}

	public void AddObserver(ITutorialStateTransitionObserver ob)
	{
		if (ob != null && !observers.Contains(ob))
		{
			observers.Add(ob);
		}
	}

	public void RemoveObserver(ITutorialStateTransitionObserver ob)
	{
		if (ob != null && observers.Contains(ob))
		{
			observers.Remove(ob);
		}
	}

	public void OnWaitForSeconds()
	{
		switch (cur_state)
		{
		case 0:
			cur_state = 1;
			{
				foreach (ITutorialStateTransitionObserver observer in observers)
				{
					if (observer != null)
					{
						observer.TransitFromInitToAirlockCameraFollowOnWaitForSeconds();
					}
				}
				break;
			}
		case 1:
			cur_state = 2;
			{
				foreach (ITutorialStateTransitionObserver observer2 in observers)
				{
					if (observer2 != null)
					{
						observer2.TransitFromAirlockCameraFollowToAirlockWelcomeOnWaitForSeconds();
					}
				}
				break;
			}
		case 2:
			cur_state = 3;
			{
				foreach (ITutorialStateTransitionObserver observer3 in observers)
				{
					if (observer3 != null)
					{
						observer3.TransitFromAirlockWelcomeToAirlockMouseLookOnWaitForSeconds();
					}
				}
				break;
			}
		}
	}

	public void OnWelcomeAirlock()
	{
	}

	public void OnLookAirlock()
	{
		int num = cur_state;
		if (num != 3)
		{
			return;
		}
		cur_state = 4;
		foreach (ITutorialStateTransitionObserver observer in observers)
		{
			if (observer != null)
			{
				observer.TransitFromAirlockMouseLookToAirlockWASDWalkOnMouseLookAirlock();
			}
		}
	}

	public void OnWASDWalkAirlock()
	{
		int num = cur_state;
		if (num != 4)
		{
			return;
		}
		cur_state = 5;
		foreach (ITutorialStateTransitionObserver observer in observers)
		{
			if (observer != null)
			{
				observer.TransitFromAirlockWASDWalkToAirlockDoorOpenOnWASDWalkAirlock();
			}
		}
	}

	public void OnEnterArmory()
	{
		switch (cur_state)
		{
		case 5:
			cur_state = 6;
			{
				foreach (ITutorialStateTransitionObserver observer in observers)
				{
					if (observer != null)
					{
						observer.TransitFromAirlockDoorOpenToTunnelOnEnterArmory();
					}
				}
				break;
			}
		case 6:
			cur_state = 7;
			{
				foreach (ITutorialStateTransitionObserver observer2 in observers)
				{
					if (observer2 != null)
					{
						observer2.TransitFromTunnelToArmoryOnEnterArmory();
					}
				}
				break;
			}
		}
	}

	public void OnLeaveArmory()
	{
		int num = cur_state;
		if (num != 7)
		{
			return;
		}
		cur_state = 9;
		foreach (ITutorialStateTransitionObserver observer in observers)
		{
			if (observer != null)
			{
				observer.TransitFromArmoryToHeadingToFightOnLeaveArmory();
			}
		}
	}

	public void OnEnterPlayground()
	{
		int num = cur_state;
		if (num != 9)
		{
			return;
		}
		cur_state = 10;
		foreach (ITutorialStateTransitionObserver observer in observers)
		{
			if (observer != null)
			{
				observer.TransitFromHeadingToFightToArenaFieldOnEnterPlayground();
			}
		}
	}

	public void OnAllBotsKilled()
	{
		int num = cur_state;
		if (num != 10)
		{
			return;
		}
		cur_state = 11;
		foreach (ITutorialStateTransitionObserver observer in observers)
		{
			if (observer != null)
			{
				observer.TransitFromArenaFieldToFiniOnAllBotsKilled();
			}
		}
	}
}
