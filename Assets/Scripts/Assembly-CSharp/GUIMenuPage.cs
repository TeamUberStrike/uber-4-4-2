using System.Collections;
using UnityEngine;

public class GUIMenuPage : GUIPageBase
{
	[SerializeField]
	private UIEventReceiver backButton;

	protected override IEnumerator OnBringIn()
	{
		float duration = bringInDuration / 3f;
		yield return StartCoroutine(AnimateAlpha(1f, duration, backButton));
	}

	protected override IEnumerator OnDismiss()
	{
		float duration = dismissDuration;
		yield return StartCoroutine(AnimateAlpha(0f, duration, backButton));
	}
}
