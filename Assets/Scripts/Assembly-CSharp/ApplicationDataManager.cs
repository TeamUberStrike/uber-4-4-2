using System;
using System.Collections;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public static class ApplicationDataManager
{
	public const string AssetVersion = "4.4.2";

	public const string Version = "4.4.2m";

	public const int MinimalWidth = 989;

	public const int MinimalHeight = 560;

	public const string HeaderFilename = "UberStrikeHeader";

	public const string MainFilename = "UberStrikeMain";

	public const string TutorialFileName = "UberStrikeTutorial";

	public const string StandaloneFilename = "UberStrike";

	private static CmuneSystemInfo localSystemInfo;

	public static EventsSystemWrapper EventsSystem;

	private static float applicationDateTime;

	private static DateTime serverDateTime;

	private static bool _isEditor;

	public static bool WebPlayerHasResult;

	public static ClientConfiguration Config { get; set; }

	public static WebPlayerSrcValues WebPlayerSrcValues { get; set; }

	public static ChannelType Channel
	{
		get
		{
			if (Application.isWebPlayer && WebPlayerSrcValues.HasValues)
			{
				return WebPlayerSrcValues.ChannelType;
			}
			if (Config != null)
			{
				return Config.ChannelType;
			}
			return ChannelType.WebFacebook;
		}
	}

	public static string ChannelSuffix
	{
		get
		{
			switch (Channel)
			{
			case ChannelType.Android:
				return "Android";
			case ChannelType.IPhone:
			case ChannelType.IPad:
				return "iOS";
			default:
				return "SD";
			}
		}
	}

	public static string BuildNumber
	{
		get
		{
			if (Application.isEditor)
			{
				return "Editor";
			}
			ChannelType channel = Channel;
			if (channel == ChannelType.WebPortal || channel == ChannelType.WebFacebook || channel == ChannelType.Kongregate)
			{
				string text = Application.absoluteURL;
				int num = text.IndexOf(".unity3d?");
				if (num > 0)
				{
					text = text.Remove(num);
				}
				int num2 = text.LastIndexOf('/');
				if (num2 > 0)
				{
					text = text.Substring(num2);
				}
				int num3 = text.LastIndexOf('.');
				if (num3 > 0)
				{
					return text.Substring(num3 + 1);
				}
				Debug.LogError("SetBuildNumber failed because URL malformed: " + Application.absoluteURL);
				return "Error";
			}
			return "N/A";
		}
	}

	public static string ConfigPath
	{
		get
		{
			if (Application.isEditor)
			{
				return "file://" + Application.dataPath + "/../EditorConfiguration.xml";
			}
			switch (Application.platform)
			{
			case RuntimePlatform.OSXWebPlayer:
			case RuntimePlatform.WindowsWebPlayer:
			{
				int num = Application.absoluteURL.IndexOf(".unity3d?");
				if (num > 0)
				{
					return Application.absoluteURL.Remove(num) + ".xml?arg=" + DateTime.Now.Ticks;
				}
				return Application.absoluteURL.Replace(".unity3d", ".xml") + "?arg=" + DateTime.Now.Ticks;
			}
			case RuntimePlatform.WindowsPlayer:
				return "file://" + Application.dataPath + "/UberStrike.xml";
			case RuntimePlatform.OSXPlayer:
				return "file://" + Application.dataPath + "/Data/UberStrike.xml";
			case RuntimePlatform.Android:
				return "jar:file://" + Application.dataPath + "!/assets/UberStrike.xml";
			case RuntimePlatform.IPhonePlayer:
				return "file://" + Application.dataPath + "/Raw/UberStrike.xml";
			default:
				return string.Empty;
			}
		}
	}

	public static BuildType BuildType
	{
		get
		{
			return Config.BuildType;
		}
	}

	public static ApplicationOptions ApplicationOptions { get; private set; }

	public static bool IsOnline { get; set; }

	public static bool IsMobile
	{
		get
		{
			return Channel == ChannelType.Android || Channel == ChannelType.IPad || Channel == ChannelType.IPhone;
		}
	}

	public static bool IsEditor
	{
		get
		{
			return _isEditor;
		}
		set
		{
			_isEditor = value;
		}
	}

	public static LocaleType CurrentLocale { get; set; }

	public static string BaseAudioURL
	{
		get
		{
			if (Config != null)
			{
				return Config.ContentBaseUrl + "Audio/";
			}
			throw new NullReferenceException("BaseAudioURL was called before Configuration file was successfully loaded.");
		}
	}

	public static string BaseMapsURL
	{
		get
		{
			if (Config != null)
			{
				return Config.ContentBaseUrl + "Maps/4.4.2/" + ChannelSuffix + "/";
			}
			throw new NullReferenceException("BaseMapsURL was called before Configuration file was successfully loaded.");
		}
	}

	public static string BaseItemsURL
	{
		get
		{
			if (Config != null)
			{
				return Config.ContentBaseUrl + "Items/4.4.2/" + ChannelSuffix + "/";
			}
			throw new NullReferenceException("BaseItemsURL was called before Configuration file was successfully loaded.");
		}
	}

	public static string BaseImageURL
	{
		get
		{
			if (Config != null)
			{
				return Config.ContentBaseUrl + "Images/";
			}
			throw new NullReferenceException("BaseImageURL was called before Configuration file was successfully loaded.");
		}
	}

	public static string BaseStandaloneBundlesURL
	{
		get
		{
			switch (Application.platform)
			{
			case RuntimePlatform.OSXPlayer:
				return "file://" + Application.dataPath + "/Data/";
			case RuntimePlatform.WindowsPlayer:
				return "file://" + Application.dataPath + "/";
			case RuntimePlatform.Android:
				return "jar:file://" + Application.dataPath + "!/assets/";
			case RuntimePlatform.IPhonePlayer:
				return "file://" + Application.dataPath + "/Raw/";
			default:
				return string.Empty;
			}
		}
	}

	public static string FrameRate
	{
		get
		{
			int num = Mathf.Max(Mathf.RoundToInt(Time.smoothDeltaTime * 1000f), 1);
			return string.Format("{0} ({1}ms)", 1000 / num, num);
		}
	}

	public static CmuneSystemInfo LocalSystemInfo
	{
		get
		{
			return localSystemInfo;
		}
	}

	public static DateTime ServerDateTime
	{
		get
		{
			return serverDateTime.AddSeconds(Time.time - applicationDateTime);
		}
		set
		{
			serverDateTime = value;
			applicationDateTime = Time.realtimeSinceStartup;
		}
	}

	static ApplicationDataManager()
	{
		applicationDateTime = 0f;
		serverDateTime = DateTime.Now;
		_isEditor = Application.isEditor;
		WebPlayerHasResult = false;
		if (Application.isWebPlayer)
		{
			WebPlayerSrcValues = new WebPlayerSrcValues(Application.srcValue);
		}
		localSystemInfo = new CmuneSystemInfo();
		ApplicationOptions = new ApplicationOptions();
		EventsSystem = new EventsSystemWrapper();
		AutoMonoBehaviour<UnityRuntime>.Instance.OnAppFocus += OnApplicationFocus;
	}

	private static void OnApplicationFocus(bool isFocused)
	{
		if (isFocused || ScreenResolutionManager.IsFullScreen)
		{
			Application.targetFrameRate = -1;
		}
		else
		{
			Application.targetFrameRate = 20;
		}
	}

	public static void LockApplication(string message = "An error occured that forced UberStrike to halt.")
	{
		PopupSystem.ClearAll();
		switch (Channel)
		{
		case ChannelType.WindowsStandalone:
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
		case ChannelType.IPhone:
		case ChannelType.IPad:
		case ChannelType.Android:
			PopupSystem.ShowMessage(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, Singleton<AuthenticationManager>.Instance.StartLogout);
			break;
		default:
			PopupSystem.ShowMessage(LocalizedStrings.Error, message, PopupSystem.AlertType.None);
			break;
		}
	}

	public static void SetBrowserInfo(string result)
	{
		bool flag = true;
		if (result != null)
		{
			string[] array = result.Split('%');
			if (array != null)
			{
				localSystemInfo.BrowserIdentifier = ((array.Length <= 0 || array[0] == null) ? "Error getting data." : array[0]);
				localSystemInfo.BrowserVersion = ((array.Length <= 1 || array[1] == null) ? "Error getting data." : array[1]);
				localSystemInfo.BrowserMajorVersion = ((array.Length <= 2 || array[2] == null) ? "Error getting data." : array[2]);
				localSystemInfo.BrowserMinorVersion = ((array.Length <= 3 || array[3] == null) ? "Error getting data." : array[3]);
				localSystemInfo.BrowserEngine = ((array.Length <= 4 || array[4] == null) ? "Error getting data." : array[4]);
				localSystemInfo.BrowserEngineVersion = ((array.Length <= 5 || array[5] == null) ? "Error getting data." : array[5]);
				localSystemInfo.BrowserUserAgent = ((array.Length <= 6 || array[6] == null) ? "Error getting data." : array[6]);
				flag = false;
			}
		}
		if (flag)
		{
			localSystemInfo.BrowserIdentifier = (localSystemInfo.BrowserVersion = (localSystemInfo.BrowserMajorVersion = (localSystemInfo.BrowserMinorVersion = (localSystemInfo.BrowserEngine = (localSystemInfo.BrowserEngineVersion = (localSystemInfo.BrowserUserAgent = "Error communicating with browser."))))));
		}
	}

	public static void OpenUrl(string title, string url)
	{
		if (Application.isWebPlayer)
		{
			Application.ExternalCall("displayMessage", title, url);
			return;
		}
		if (Screen.fullScreen && Application.platform != RuntimePlatform.WindowsPlayer)
		{
			ScreenResolutionManager.IsFullScreen = false;
		}
		Application.OpenURL(url);
	}

	public static void OpenBuyCredits()
	{
		switch (Channel)
		{
		case ChannelType.WebPortal:
		case ChannelType.Kongregate:
			ScreenResolutionManager.IsFullScreen = false;
			Application.ExternalCall("getCreditsWrapper", PlayerDataManager.CmidSecure);
			break;
		case ChannelType.WindowsStandalone:
		case ChannelType.OSXStandalone:
			OpenUrl(string.Empty, GetStandalonePaymentUrl());
			MonoRoutine.Start(ShowStandaloneRefreshBalancePopup(2f));
			break;
		case ChannelType.WebFacebook:
			LoadBuyCreditsPage();
			break;
		case ChannelType.IPhone:
		case ChannelType.IPad:
			if (Singleton<BundleManager>.Instance.CanMakeMasPayments)
			{
				LoadBuyCreditsPage();
			}
			else
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, "Sorry, In App Purchases are currently unavailable.", PopupSystem.AlertType.OK);
			}
			break;
		case ChannelType.MacAppStore:
			if (Singleton<BundleManager>.Instance.CanMakeMasPayments)
			{
				LoadBuyCreditsPage();
			}
			else
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, "Sorry, In App Purchases are only available in Mac OSX Lion (v10.7) and above.", PopupSystem.AlertType.OK);
			}
			break;
		case ChannelType.Android:
			if (Singleton<BundleManager>.Instance.CanMakeMasPayments)
			{
				LoadBuyCreditsPage();
			}
			else
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, "Sorry, In App Purchases are currently unavailable.", PopupSystem.AlertType.OK);
			}
			break;
		default:
			Debug.LogError("OpenBuyCredits not supported on channel: " + Channel);
			break;
		}
	}

	private static void LoadBuyCreditsPage()
	{
		if (GameState.HasCurrentGame)
		{
			if (GamePageManager.Instance.HasPage)
			{
				PageScene currentPage = GamePageManager.Instance.GetCurrentPage();
				SceneGuiController component = currentPage.GetComponent<SceneGuiController>();
				if (component != null)
				{
					component.SetShopArea();
				}
			}
		}
		else
		{
			GameData.Instance.MainMenu.Value = MainMenuState.None;
			MenuPageManager.Instance.LoadPage(PageType.Shop);
		}
		SelectShopAreaEvent selectShopAreaEvent = new SelectShopAreaEvent();
		selectShopAreaEvent.ShopArea = ShopArea.Credits;
		CmuneEventHandler.Route(selectShopAreaEvent);
	}

	private static IEnumerator ShowStandaloneRefreshBalancePopup(float delayInSecs)
	{
		yield return new WaitForSeconds(delayInSecs);
		PopupSystem.ShowMessage("Buy Credits", "Did you make a purchase? If so, click Update to refresh your point and credit balance.", PopupSystem.AlertType.OKCancel, "UPDATE", RefreshWallet);
	}

	private static string GetStandalonePaymentUrl()
	{
		string text = string.Empty;
		switch (BuildType)
		{
		case BuildType.Dev:
			text = "http://dev.uberstrike.com/";
			break;
		case BuildType.Staging:
			text = "http://qa.uberstrike.com/";
			break;
		case BuildType.Prod:
			text = "http://uberstrike.com/";
			break;
		}
		return text + string.Format("account/externallogin?channel={0}&email={1}&lang={2}", (int)Channel, PlayerDataManager.EmailSecure, CurrentLocale.ToString().Replace("_", "-"));
	}

	private static string GetStandaloneUpdateUrl()
	{
		string text = string.Empty;
		switch (BuildType)
		{
		case BuildType.Dev:
			text = "http://dev.uberstrike.com/";
			break;
		case BuildType.Staging:
			text = "http://qa.uberstrike.com/";
			break;
		case BuildType.Prod:
			text = "http://uberstrike.com/";
			break;
		}
		return text + string.Format("download?mode=update&channel={0}", (int)Channel);
	}

	private static IEnumerator StartRefreshWalletInventory()
	{
		yield return MonoRoutine.Start(Singleton<PlayerDataManager>.Instance.StartGetMemberWallet());
		yield return MonoRoutine.Start(Singleton<ItemManager>.Instance.StartGetInventory(true));
	}

	public static void RefreshWallet()
	{
		MonoRoutine.Start(StartRefreshWalletInventory());
	}
}
