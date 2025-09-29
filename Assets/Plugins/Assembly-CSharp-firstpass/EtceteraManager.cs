using System;
using System.Collections;
using Prime31;
using UnityEngine;

public class EtceteraManager : AbstractManager
{
	public delegate void EceteraTextureDelegate(Texture2D texture);

	public delegate void EceteraTextureFailedDelegate(string error);

	public static string pushIOApiKey;

	public static string[] pushIOCategories;

	public static string deviceToken { get; private set; }

	public static event Action dismissingViewControllerEvent;

	public static event Action imagePickerCancelledEvent;

	public static event Action<string> imagePickerChoseImageEvent;

	public static event Action saveImageToPhotoAlbumSucceededEvent;

	public static event Action<string> saveImageToPhotoAlbumFailedEvent;

	public static event Action<string> alertButtonClickedEvent;

	public static event Action promptCancelledEvent;

	public static event Action<string> singleFieldPromptTextEnteredEvent;

	public static event Action<string, string> twoFieldPromptTextEnteredEvent;

	public static event Action<string> remoteRegistrationSucceededEvent;

	public static event Action<string> remoteRegistrationFailedEvent;

	public static event Action urbanAirshipRegistrationSucceededEvent;

	public static event Action<string> urbanAirshipRegistrationFailedEvent;

	public static event Action<string> pushIORegistrationCompletedEvent;

	public static event Action<IDictionary> remoteNotificationReceivedEvent;

	public static event Action<IDictionary> remoteNotificationReceivedAtLaunchEvent;

	public static event Action<IDictionary> localNotificationWasReceivedEvent;

	public static event Action<IDictionary> localNotificationWasReceivedAtLaunchEvent;

	public static event Action<string> mailComposerFinishedEvent;

	public static event Action<string> smsComposerFinishedEvent;

	static EtceteraManager()
	{
		AbstractManager.initialize(typeof(EtceteraManager));
	}

	public void dismissingViewController()
	{
		if (EtceteraManager.dismissingViewControllerEvent != null)
		{
			EtceteraManager.dismissingViewControllerEvent();
		}
	}

	public void imagePickerDidCancel(string empty)
	{
		if (EtceteraManager.imagePickerCancelledEvent != null)
		{
			EtceteraManager.imagePickerCancelledEvent();
		}
	}

	public void imageSavedToDocuments(string filePath)
	{
		if (EtceteraManager.imagePickerChoseImageEvent != null)
		{
			EtceteraManager.imagePickerChoseImageEvent(filePath);
		}
	}

	public void saveImageToPhotoAlbumFailed(string error)
	{
		EtceteraManager.saveImageToPhotoAlbumFailedEvent.fire(error);
	}

	public void saveImageToPhotoAlbumSucceeded(string empty)
	{
		EtceteraManager.saveImageToPhotoAlbumSucceededEvent.fire();
	}

	public static IEnumerator textureFromFileAtPath(string filePath, EceteraTextureDelegate del, EceteraTextureFailedDelegate errorDel)
	{
		using (WWW www = new WWW(filePath))
		{
			yield return www;
			if (www.error != null && errorDel != null)
			{
				errorDel(www.error);
			}
			Texture2D tex = www.texture;
			if (tex != null)
			{
				del(tex);
			}
		}
	}

	public void alertViewClickedButton(string buttonTitle)
	{
		if (EtceteraManager.alertButtonClickedEvent != null)
		{
			EtceteraManager.alertButtonClickedEvent(buttonTitle);
		}
	}

	public void alertPromptCancelled(string empty)
	{
		if (EtceteraManager.promptCancelledEvent != null)
		{
			EtceteraManager.promptCancelledEvent();
		}
	}

	public void alertPromptEnteredText(string text)
	{
		string[] array = text.Split(new string[1] { "|||" }, StringSplitOptions.None);
		if (array.Length == 1 && EtceteraManager.singleFieldPromptTextEnteredEvent != null)
		{
			EtceteraManager.singleFieldPromptTextEnteredEvent(array[0]);
		}
		if (array.Length == 2 && EtceteraManager.twoFieldPromptTextEnteredEvent != null)
		{
			EtceteraManager.twoFieldPromptTextEnteredEvent(array[0], array[1]);
		}
	}

	public void remoteRegistrationDidSucceed(string deviceToken)
	{
		EtceteraManager.deviceToken = deviceToken;
		if (EtceteraManager.remoteRegistrationSucceededEvent != null)
		{
			EtceteraManager.remoteRegistrationSucceededEvent(deviceToken);
		}
		if (pushIOApiKey != null)
		{
			StartCoroutine(registerDeviceWithPushIO());
		}
	}

	private IEnumerator registerDeviceWithPushIO()
	{
		string url = string.Format("https://api.push.io/r/{0}?di={1}&dt={2}", pushIOApiKey, SystemInfo.deviceUniqueIdentifier, deviceToken);
		if (pushIOCategories != null && pushIOCategories.Length > 0)
		{
			url = url + "&c=" + string.Join(",", pushIOCategories);
		}
		using (WWW www = new WWW(url))
		{
			yield return www;
			if (EtceteraManager.pushIORegistrationCompletedEvent != null)
			{
				EtceteraManager.pushIORegistrationCompletedEvent(www.error);
			}
		}
	}

	public void remoteRegistrationDidFail(string error)
	{
		if (EtceteraManager.remoteRegistrationFailedEvent != null)
		{
			EtceteraManager.remoteRegistrationFailedEvent(error);
		}
	}

	public void urbanAirshipRegistrationDidSucceed(string empty)
	{
		if (EtceteraManager.urbanAirshipRegistrationSucceededEvent != null)
		{
			EtceteraManager.urbanAirshipRegistrationSucceededEvent();
		}
	}

	public void urbanAirshipRegistrationDidFail(string error)
	{
		if (EtceteraManager.urbanAirshipRegistrationFailedEvent != null)
		{
			EtceteraManager.urbanAirshipRegistrationFailedEvent(error);
		}
	}

	public void remoteNotificationWasReceived(string json)
	{
		if (EtceteraManager.remoteNotificationReceivedEvent != null)
		{
			EtceteraManager.remoteNotificationReceivedEvent(json.dictionaryFromJson());
		}
	}

	public void remoteNotificationWasReceivedAtLaunch(string json)
	{
		if (EtceteraManager.remoteNotificationReceivedAtLaunchEvent != null)
		{
			EtceteraManager.remoteNotificationReceivedAtLaunchEvent(json.dictionaryFromJson());
		}
	}

	public void localNotificationWasReceived(string json)
	{
		EtceteraManager.localNotificationWasReceivedEvent.fire(json.dictionaryFromJson());
	}

	public void localNotificationWasReceivedAtLaunch(string json)
	{
		EtceteraManager.remoteNotificationReceivedAtLaunchEvent.fire(json.dictionaryFromJson());
	}

	public void mailComposerFinishedWithResult(string result)
	{
		if (EtceteraManager.mailComposerFinishedEvent != null)
		{
			EtceteraManager.mailComposerFinishedEvent(result);
		}
	}

	public void smsComposerFinishedWithResult(string result)
	{
		if (EtceteraManager.smsComposerFinishedEvent != null)
		{
			EtceteraManager.smsComposerFinishedEvent(result);
		}
	}
}
