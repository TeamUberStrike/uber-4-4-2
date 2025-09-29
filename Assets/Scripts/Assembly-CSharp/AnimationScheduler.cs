using System.Collections;
using System.Collections.Generic;

public class AnimationScheduler
{
	private class AnimationInfo
	{
		public IAnimatable2D _animatable;

		public DoAnim _animFunction;

		public string[] _animArgs;

		public OnAnimOver _animOverEvent;

		public string[] _eventArgs;

		public AnimationInfo(IAnimatable2D animatable, DoAnim anim, string[] animArgs, OnAnimOver animOverEvent, string[] eventArgs)
		{
			_animatable = animatable;
			_animFunction = anim;
			_animArgs = animArgs;
			_animOverEvent = animOverEvent;
			_eventArgs = eventArgs;
		}
	}

	public delegate IEnumerator DoAnim(IAnimatable2D animatable, string[] animArgs);

	public delegate IEnumerator OnAnimOver(IAnimatable2D animatable, string[] args);

	private List<AnimationInfo> _animGroup;

	private Queue<AnimationInfo> _standbyQueue;

	public AnimationScheduler()
	{
		_animGroup = new List<AnimationInfo>();
		_standbyQueue = new Queue<AnimationInfo>();
	}

	public void Draw()
	{
		ScheduleAnimation();
		foreach (AnimationInfo item in _animGroup)
		{
			item._animatable.Draw();
		}
	}

	public void AddAnimation(IAnimatable2D animatable, DoAnim anim, string[] animArgs, OnAnimOver animOverEvent, string[] eventArgs)
	{
		AnimationInfo animationInfo = new AnimationInfo(animatable, anim, animArgs, animOverEvent, eventArgs);
		animationInfo._animatable.Hide();
		_standbyQueue.Enqueue(animationInfo);
	}

	private void ScheduleAnimation()
	{
		if (_animGroup.Count == 0 && _standbyQueue.Count != 0)
		{
			AnimationInfo animInfo = _standbyQueue.Dequeue();
			MonoRoutine.Start(DoAnimation(animInfo));
		}
	}

	private IEnumerator DoAnimation(AnimationInfo animInfo)
	{
		_animGroup.Add(animInfo);
		animInfo._animatable.Show();
		if (animInfo._animFunction != null)
		{
			yield return MonoRoutine.Start(animInfo._animFunction(animInfo._animatable, animInfo._animArgs));
			if (animInfo._animOverEvent != null)
			{
				yield return MonoRoutine.Start(animInfo._animOverEvent(animInfo._animatable, animInfo._eventArgs));
			}
		}
		_animGroup.Remove(animInfo);
	}
}
