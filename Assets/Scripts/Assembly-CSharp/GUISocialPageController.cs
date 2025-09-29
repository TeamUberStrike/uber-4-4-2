using System.Collections;
using UnityEngine;

public class GUISocialPageController : GUIPageBase
{
	[SerializeField]
	private FBViewController fbViewController;

	[SerializeField]
	private OnlineNowViewController onlineNowViewController;

	protected override IEnumerator OnBringIn()
	{
		float duration = bringInDuration / 2f;
		yield return StartCoroutine(AnimateAlpha(1f, duration, fbViewController.gameObject));
		yield return StartCoroutine(AnimateAlpha(1f, duration, onlineNowViewController.gameObject));
	}

	protected override IEnumerator OnDismiss()
	{
		float duration = dismissDuration;
		yield return StartCoroutine(AnimateAlpha(0f, duration, fbViewController.gameObject, onlineNowViewController.gameObject));
	}
}
