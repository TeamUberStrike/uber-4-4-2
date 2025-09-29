using System;
using UnityEngine;

[AddComponentMenu("Game/UI/Tween Loop Stopper")]
public class UITweenerLoopStopper : MonoBehaviour
{
	[SerializeField]
	private UITweener tweener;

	[SerializeField]
	private int numberOfCycles = 1;

	private int currentCycles;

	private void Awake()
	{
		if (tweener == null && (bool)base.gameObject.GetComponent<UITweener>())
		{
			tweener = base.gameObject.GetComponent<UITweener>();
		}
		if (tweener != null)
		{
			UITweener uITweener = tweener;
			uITweener.onCycleFinished = (UITweener.OnFinished)Delegate.Combine(uITweener.onCycleFinished, new UITweener.OnFinished(HandleOnCycleFinished));
		}
		else
		{
			Debug.LogError("No tween was assigned to UITweenerLoopStopper in " + base.gameObject.name);
		}
	}

	private void HandleOnCycleFinished(UITweener tween)
	{
		if (currentCycles > numberOfCycles)
		{
			tweener.Reset();
			tweener.enabled = false;
			currentCycles = 0;
		}
		else
		{
			currentCycles++;
		}
	}
}
