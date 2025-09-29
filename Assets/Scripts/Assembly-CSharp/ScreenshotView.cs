using System.Collections;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class ScreenshotView : MonoBehaviour
{
	[SerializeField]
	private UIEventReceiver actionButton;

	[SerializeField]
	private UILabel messageText;

	[SerializeField]
	private UITweener messageTween;

	private Vector3 originalMessageSize;

	private string DisplayText
	{
		set
		{
			if (!(messageText == null))
			{
				messageText.pivot = UIWidget.Pivot.Left;
				messageText.text = value;
			}
		}
	}

	private void OnEnable()
	{
		AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn.Changed += ShowView;
		CmuneEventHandler.AddListener<OnScreenshotTakenEvent>(ScreenshotTakenEvent);
	}

	private void OnDisable()
	{
		AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn.Changed -= ShowView;
		CmuneEventHandler.RemoveListener<OnScreenshotTakenEvent>(ScreenshotTakenEvent);
	}

	private void Start()
	{
		actionButton.OnClicked = delegate
		{
			AutoMonoBehaviour<FacebookInterface>.Instance.ShareScreenShot();
		};
		originalMessageSize = messageText.transform.localScale;
		ShowView(AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn);
	}

	private void ShowView(bool isLoggedIn)
	{
		if (!(actionButton == null) && !(messageText == null))
		{
			actionButton.gameObject.SetActive(isLoggedIn);
			messageText.gameObject.SetActive(isLoggedIn);
			SetDefaultText();
		}
	}

	private void SetDefaultText()
	{
		messageText.cachedTransform.localScale = originalMessageSize;
		messageTween.enabled = false;
		if (ApplicationDataManager.Channel == ChannelType.IPad)
		{
			DisplayText = LocalizedStrings.SharePhotoIPad;
			return;
		}
		string keyAssignmentString = AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString(GameInputKey.SendScreenshotToFacebook);
		if (string.IsNullOrEmpty(keyAssignmentString) || keyAssignmentString == "Not set")
		{
			DisplayText = LocalizedStrings.SharePhotoIPad;
		}
		else
		{
			DisplayText = string.Format(LocalizedStrings.SharePhotoFacebook, keyAssignmentString);
		}
	}

	private void ScreenshotTakenEvent(OnScreenshotTakenEvent ev)
	{
		StartCoroutine(OnScreenshotEvent());
	}

	private IEnumerator OnScreenshotEvent()
	{
		DisplayText = LocalizedStrings.ScreenshotTaken;
		messageTween.enabled = true;
		messageText.pivot = UIWidget.Pivot.Center;
		yield return new WaitForSeconds(1.5f);
		SetDefaultText();
	}
}
