using UnityEngine;

internal class InGameFeatHud : Singleton<InGameFeatHud>, IUpdatable
{
	private AnimationSchedulerNew _animScheduler;

	public float TextHeight
	{
		get
		{
			return (float)Screen.height * 0.08f;
		}
	}

	public Vector2 AnchorPoint
	{
		get
		{
			return new Vector2(Screen.width / 2, (float)Screen.height * 0.26f);
		}
	}

	public AnimationSchedulerNew AnimationScheduler
	{
		get
		{
			return _animScheduler;
		}
	}

	private InGameFeatHud()
	{
		_animScheduler = new AnimationSchedulerNew();
		_animScheduler.ScheduleAnimation = ScheduleStrategy.ScheduleOverlap;
	}

	public void Update()
	{
		_animScheduler.Update();
	}
}
