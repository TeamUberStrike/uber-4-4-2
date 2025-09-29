using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Cmune.Core.Models.Views;
using Cmune.DataCenter.Common.Entities;
using MiniJSON;
using UberStrike.Core.Types;
using UberStrike.Core.ViewModel;
using UberStrike.DataCenter.Common.Entities;
using UberStrike.DataCenter.UnitySdk;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GlobalSceneLoader : MonoBehaviour
{
	[SerializeField]
	private GUISkin popupSkin;

	public static string ErrorMessage { get; private set; }

	public static bool IsError
	{
		get
		{
			return !string.IsNullOrEmpty(ErrorMessage);
		}
	}

	public static bool IsInitialised { get; set; }

	public static float GlobalSceneProgress { get; private set; }

	public static bool IsGlobalSceneLoaded { get; private set; }

	public static float ItemAssetBundleProgress { get; private set; }

	public static bool IsItemAssetBundleLoaded { get; private set; }

	public static bool IsItemAssetBundleDownloading { get; private set; }

	private void Awake()
	{
		PopupSkin.Initialize(popupSkin);
	}

	public void SendFacebookLoginParams(string json)
	{
		ApplicationDataManager.WebPlayerHasResult = true;
		try
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(json);
			AuthenticationManager.FacebookHash = dictionary["hash"].ToString();
			AuthenticationManager.FacebookEsnsMemberId = dictionary["esnsmemberid"].ToString();
			ApplicationDataManager.Config.ChannelType = ChannelType.WebFacebook;
		}
		catch (Exception ex)
		{
			ErrorMessage = "Couldn't get your login details!";
			Debug.Log("Couldn't get params from JS: " + ex.Message);
		}
	}

	public void SendWebPlayerLoginParams(string json)
	{
		ApplicationDataManager.WebPlayerHasResult = true;
		try
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(json);
			AuthenticationManager.WebPlayerHash = dictionary["hash"].ToString();
			AuthenticationManager.WebPlayerCmid = int.Parse(dictionary["cmid"].ToString());
			ApplicationDataManager.Config.ChannelType = ChannelType.WebPortal;
		}
		catch (Exception ex)
		{
			ErrorMessage = "Couldn't get your login details!";
			Debug.Log("Couldn't get params from JS: " + ex.Message);
		}
	}

	private static void AOTHelperMethod()
	{
		SortedList<int, ItemTransactionsViewModel> sortedList = new SortedList<int, ItemTransactionsViewModel>();
		SortedList<int, PointDepositsViewModel> sortedList2 = new SortedList<int, PointDepositsViewModel>();
		SortedList<int, CurrencyDepositsViewModel> sortedList3 = new SortedList<int, CurrencyDepositsViewModel>();
	}

	private IEnumerator Start()
	{
		ApplicationDataManager.EventsSystem.SendStartClient();
		AutoMonoBehaviour<MATInterface>.Instance.OnStartup();
		Application.runInBackground = true;
		while (Application.internetReachability == NetworkReachability.NotReachable)
		{
			yield return new WaitForSeconds(1f);
		}
		yield return StartCoroutine(LoadConfig());
		if (IsError)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			yield break;
		}
		ApplicationDataManager.EventsSystem.SendConfigLoaded((int)ApplicationDataManager.Channel, ApplicationDataManager.Config.WebServiceBaseUrl);
		yield return StartCoroutine(BeginAuthenticateApplication());
		if (IsError)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			yield break;
		}
		CacheManager.RunAuthorization();
		SceneManager.LoadScene("GlobalScene", LoadSceneMode.Additive);
		GlobalSceneProgress = 1f;
		yield return new WaitForEndOfFrame();
		if (IsError)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			yield break;
		}
		yield return new WaitForSeconds(0.5f);
		ItemAssetBundleProgress = 1f;
		IsItemAssetBundleLoaded = true;
		if (IsError)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			yield break;
		}
		InitializeGlobalScene();
		IsGlobalSceneLoaded = true;
		ApplicationDataManager.EventsSystem.SendLoadingFinished();
		Singleton<AuthenticationManager>.Instance.LoginByChannel();
	}

	private IEnumerator LoadConfig()
	{
		string path = ApplicationDataManager.ConfigPath;
		if (string.IsNullOrEmpty(path))
		{
			ErrorMessage = string.Concat("Cannot load Configuration Xml, ", Application.platform, " Platform is not supported.");
			Debug.LogError(ErrorMessage);
			yield break;
		}
		UnityWebRequest www = UnityWebRequest.Get(path);
		yield return www.SendWebRequest();
		ClientConfiguration config = new ClientConfiguration();
		try
		{
			if (www.result == UnityWebRequest.Result.Success)
			{
				using (XmlReader xmlReader = XmlReader.Create(new StringReader(www.downloadHandler.text)))
				{
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Application")
					{
						if (xmlReader.MoveToAttribute("DebugLevel"))
						{
							config.SetDebugLevel(xmlReader.Value);
						}
						if (xmlReader.MoveToAttribute("BuildType"))
						{
							config.SetBuildType(xmlReader.Value);
						}
						if (xmlReader.MoveToAttribute("WebServiceBaseUrl"))
						{
							config.WebServiceBaseUrl = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("ContentBaseUrl"))
						{
							config.ContentBaseUrl = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("ChannelType"))
						{
							config.SetChannelType(xmlReader.Value);
						}
						if (xmlReader.MoveToAttribute("ContentRouterBaseUrl"))
						{
							config.ContentRouterBaseUrl = xmlReader.Value;
						}
						if (xmlReader.MoveToAttribute("FacebookAppId"))
						{
							config.FacebookAppId = xmlReader.Value;
						}
					}
				}
				}
			}
			else
			{
				ErrorMessage = "Failed to load configuration: " + www.error;
				Debug.LogError(ErrorMessage);
				yield break;
			}
			if (!config.IsValid())
			{
				ErrorMessage = "Missing critical elements and/or attributes.";
				Debug.LogError(ErrorMessage);
				yield break;
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			ErrorMessage = "The Configuration XML was malformed.\n" + ex2.Message;
			Debug.LogError(ErrorMessage);
			yield break;
		}
		if (!string.IsNullOrEmpty(config.FacebookAppId) && config.FacebookAppId != "0")
		{
			FBSettings.CustomAppId = config.FacebookAppId;
		}
		ApplicationDataManager.Config = config;
		Configuration.WebserviceBaseUrl = ApplicationDataManager.Config.WebServiceBaseUrl + UberStrike.DataCenter.UnitySdk.ApiVersion.Current + "/";
	}

	private void InitializeGlobalScene()
	{
		ApplicationDataManager.CurrentLocale = LocaleType.en_US;
		ApplicationDataManager.ApplicationOptions.Initialize();
		StartCoroutine(GUITools.StartScreenSizeListener(1f));
		AutoMonoBehaviour<TouchInput>.Instance.enabled = true;
		AutoMonoBehaviour<SfxManager>.Instance.EnableAudio(ApplicationDataManager.ApplicationOptions.AudioEnabled);
		AutoMonoBehaviour<SfxManager>.Instance.UpdateMasterVolume();
		AutoMonoBehaviour<SfxManager>.Instance.UpdateMusicVolume();
		AutoMonoBehaviour<SfxManager>.Instance.UpdateEffectsVolume();
		AutoMonoBehaviour<InputManager>.Instance.ReadAllKeyMappings();
		PerformanceTest.RunPerformanceCheck();
	}

	private IEnumerator BeginAuthenticateApplication()
	{
		yield return ApplicationWebServiceClient.AuthenticateApplication("4.4.2m", ApplicationDataManager.Channel, string.Empty, delegate(AuthenticateApplicationView callback)
		{
			OnAuthenticateApplication(callback);
		}, delegate(Exception exception)
		{
			OnAuthenticateApplicationException(exception);
		});
		Debug.Log("Connected to : " + Configuration.WebserviceBaseUrl);
	}

	private void OnAuthenticateApplication(AuthenticateApplicationView ev)
	{
		try
		{
			IsInitialised = true;
			if (ev != null && ev.IsEnabled)
			{
				Configuration.EncryptionInitVector = ev.EncryptionInitVector;
				Configuration.EncryptionPassPhrase = ev.EncryptionPassPhrase;
				ApplicationDataManager.IsOnline = true;
				if (ApplicationDataManager.IsMobile)
				{
					Singleton<GameServerManager>.Instance.AddPhotonGameServers(ev.GameServers.FindAll((PhotonView i) => i.UsageType == PhotonUsageType.Mobile));
				}
				else
				{
					Singleton<GameServerManager>.Instance.AddPhotonGameServers(ev.GameServers.FindAll((PhotonView i) => i.UsageType == PhotonUsageType.All));
				}
				CmuneNetworkManager.CurrentCommServer = new GameServerView(ev.CommServer);
				if (ev.WarnPlayer)
				{
					HandleVersionWarning();
				}
			}
			else
			{
				Debug.Log(string.Concat("OnAuthenticateApplication failed with 4.4.2m/", ApplicationDataManager.Channel, ": ", ErrorMessage));
				ErrorMessage = "Please update.";
				HandleVersionError();
			}
		}
		catch (Exception ex)
		{
			ErrorMessage = ex.Message + " " + ex.StackTrace;
			Debug.LogError(string.Concat("OnAuthenticateApplication crashed with 4.4.2m/", ApplicationDataManager.Channel, ": ", ErrorMessage));
			HandleApplicationAuthenticationError("There was a problem loading UberStrike. Please check your internet connection and try again.");
		}
	}

	private void OnAuthenticateApplicationException(Exception exception)
	{
		ErrorMessage = exception.Message;
		Debug.LogError(string.Concat("An exception occurred while authenticating the application with 4.4.2m/", ApplicationDataManager.Channel, ": ", exception.Message));
		HandleApplicationAuthenticationError("There was a problem loading UberStrike. Please check your internet connection and try again.");
	}

	private void RetryAuthentiateApplication()
	{
		ErrorMessage = string.Empty;
		StartCoroutine(BeginAuthenticateApplication());
	}

	private void HandleApplicationAuthenticationError(string message)
	{
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, Application.Quit);
			break;
		case ChannelType.WebPortal:
		case ChannelType.WebFacebook:
		case ChannelType.Kongregate:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.None);
			break;
		case ChannelType.WindowsStandalone:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, RetryAuthentiateApplication);
			break;
		case ChannelType.IPhone:
		case ChannelType.IPad:
		case ChannelType.Android:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, RetryAuthentiateApplication);
			break;
		default:
			PopupSystem.ShowError(LocalizedStrings.Error, message + "This client type is not supported.", PopupSystem.AlertType.OK, Application.Quit);
			break;
		}
	}

	private void HandleConfigurationMissingError(string message)
	{
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, Application.Quit);
			break;
		case ChannelType.WebPortal:
		case ChannelType.WebFacebook:
		case ChannelType.Kongregate:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.None);
			break;
		case ChannelType.WindowsStandalone:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, Application.Quit);
			break;
		case ChannelType.IPhone:
		case ChannelType.IPad:
		case ChannelType.Android:
			PopupSystem.ShowError(LocalizedStrings.Error, message, PopupSystem.AlertType.OK, Application.Quit);
			break;
		default:
			PopupSystem.ShowError(LocalizedStrings.Error, message + "This client type is not supported.", PopupSystem.AlertType.OK, Application.Quit);
			break;
		}
	}

	private void HandleVersionWarning()
	{
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
			PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Click OK to update from the Mac App Store.", PopupSystem.AlertType.OKCancel, OpenMacAppStoreUpdatesPage, Singleton<AuthenticationManager>.Instance.LoginByChannel);
			break;
		case ChannelType.WebPortal:
		case ChannelType.WebFacebook:
		case ChannelType.Kongregate:
			PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. You should refresh your browser.", PopupSystem.AlertType.OK, Singleton<AuthenticationManager>.Instance.LoginByChannel);
			break;
		case ChannelType.WindowsStandalone:
			PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Click OK to update from our website.", PopupSystem.AlertType.OKCancel, OpenUberStrikeUpdatePage, Singleton<AuthenticationManager>.Instance.LoginByChannel);
			break;
		case ChannelType.Android:
			PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Click OK to update from our website.", PopupSystem.AlertType.OKCancel, OpenAndroidAppStoreUpdatesPage, Singleton<AuthenticationManager>.Instance.LoginByChannel);
			break;
		case ChannelType.IPhone:
		case ChannelType.IPad:
			PopupSystem.ShowError("Warning", "Your UberStrike client is out of date. Click OK to update from the App Store.", PopupSystem.AlertType.OKCancel, OpenIosAppStoreUpdatesPage, Singleton<AuthenticationManager>.Instance.LoginByChannel);
			break;
		default:
			PopupSystem.ShowError(LocalizedStrings.Error, string.Concat("Your UberStrike client is not supported. Please update from our website.\n(Invalid Channel: ", ApplicationDataManager.Channel, ")"), PopupSystem.AlertType.OK, OpenUberStrikeUpdatePage);
			break;
		}
	}

	private void HandleVersionError()
	{
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
			PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please update from the Mac App Store.", PopupSystem.AlertType.OK, OpenMacAppStoreUpdatesPage);
			break;
		case ChannelType.WebPortal:
		case ChannelType.WebFacebook:
		case ChannelType.Kongregate:
			PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please refresh your browser.", PopupSystem.AlertType.None);
			break;
		case ChannelType.WindowsStandalone:
			PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please update from our website.", PopupSystem.AlertType.OK, OpenUberStrikeUpdatePage);
			break;
		case ChannelType.Android:
			PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please update from our website.", PopupSystem.AlertType.OK, OpenAndroidAppStoreUpdatesPage);
			break;
		case ChannelType.IPhone:
		case ChannelType.IPad:
			PopupSystem.ShowError(LocalizedStrings.Error, "Your UberStrike client is out of date. Please update from the App Store.", PopupSystem.AlertType.OK, OpenIosAppStoreUpdatesPage);
			break;
		default:
			PopupSystem.ShowError(LocalizedStrings.Error, string.Concat("Your UberStrike client is not supported. Please update from our website.\n(Invalid Channel: ", ApplicationDataManager.Channel, ")"), PopupSystem.AlertType.OK, OpenUberStrikeUpdatePage);
			break;
		}
	}

	private void OpenMacAppStoreUpdatesPage()
	{
		ApplicationDataManager.OpenUrl(string.Empty, "macappstore://showUpdatesPage");
		Application.Quit();
	}

	private void OpenIosAppStoreUpdatesPage()
	{
		ApplicationDataManager.OpenUrl(string.Empty, "itms-apps://itunes.com/apps/uberstrike");
	}

	private void OpenAndroidAppStoreUpdatesPage()
	{
		ApplicationDataManager.OpenUrl(string.Empty, "market://details?id=com.cmune.uberstrike.android");
	}

	private void OpenUberStrikeUpdatePage()
	{
		ApplicationDataManager.OpenUrl(string.Empty, "http://uberstrike.com");
		Application.Quit();
	}

	private static IEnumerator PrefetchSocketPolicyAsync(string address)
	{
		CrossdomainPolicy.RemovePolicyEntry(address);
		try
		{
			string address2 = default(string);
			ThreadPool.QueueUserWorkItem(delegate
			{
				// Security.PrefetchSocketPolicy is obsolete and no longer supported in Unity 6
				// Socket policies were only needed for Unity Web Player which is discontinued
				// For modern Unity builds, we'll assume the policy is always allowed
				CrossdomainPolicy.SetPolicyValue(address2, true);
			});
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			Debug.LogError("Failed CheckPolicy: " + ex2.Message);
		}
		while (!CrossdomainPolicy.HasPolicyEntry(address))
		{
			yield return new WaitForEndOfFrame();
		}
	}
}
