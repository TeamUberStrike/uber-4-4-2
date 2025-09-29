using UnityEngine;

public class ScheduleStrategy
{
	public static void ScheduleInSequence(AnimationSchedulerNew animScheduler)
	{
		if (animScheduler.AnimGroupCount == 0 && animScheduler.StandbyQueueCount != 0)
		{
			ScheduleNextAnim(animScheduler);
		}
	}

	public static void PreemptiveSchedule(AnimationSchedulerNew animScheduler)
	{
		if (animScheduler.StandbyQueueCount != 0)
		{
			animScheduler.ClearAnimGroup();
			ScheduleNextAnim(animScheduler);
		}
	}

	public static void ScheduleOverlap(AnimationSchedulerNew animScheduler)
	{
		if (animScheduler.StandbyQueueCount == 0)
		{
			return;
		}
		if (animScheduler.AnimGroupCount == 0)
		{
			ScheduleNextAnim(animScheduler);
		}
		else if (animScheduler.AnimGroupCount == 1)
		{
			IAnim fromAnimGroup = animScheduler.GetFromAnimGroup(0);
			if (Time.time - fromAnimGroup.StartTime > fromAnimGroup.Duration * 0.5f)
			{
				ScheduleNextAnim(animScheduler);
			}
		}
	}

	private static void ScheduleNextAnim(AnimationSchedulerNew animScheduler)
	{
		IAnim anim = animScheduler.DequeueAnim();
		animScheduler.AddToAnimGroup(anim);
		anim.Start();
	}
}
