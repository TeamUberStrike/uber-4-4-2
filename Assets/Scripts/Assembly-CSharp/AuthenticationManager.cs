using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Core.ViewModel;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;

public class AuthenticationManager : Singleton<AuthenticationManager>
{
	private ProgressPopupDialog _progress;

	public bool ForceFacebookRelogin;

	public static int WebPlayerCmid = 0;

	public static string WebPlayerHash = string.Empty;

	public static string WebPlayerChannelType = string.Empty;

	public static string FacebookHash = string.Empty;

	public static string FacebookEsnsMemberId = string.Empty;

	private AuthenticationManager()
	{
		_progress = new ProgressPopupDialog(LocalizedStrings.SettingUp, LocalizedStrings.ProcessingLogin);
	}

	public void LoginByChannel()
	{
		if (ApplicationDataManager.IsEditor || Application.absoluteURL.StartsWith("file://"))
		{
			PanelManager.Instance.OpenPanel(PanelType.Login);
			return;
		}
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.WebPortal:
			MonoRoutine.Start(StartLoginMemberPortal(WebPlayerCmid, WebPlayerHash));
			break;
		case ChannelType.WebFacebook:
			MonoRoutine.Start(StartLoginMemberFacebook(FacebookEsnsMemberId, FacebookHash));
			break;
		case ChannelType.WindowsStandalone:
		case ChannelType.MacAppStore:
		case ChannelType.OSXStandalone:
		case ChannelType.IPhone:
		case ChannelType.IPad:
		case ChannelType.Android:
			PopupSystem.ClearAll();
			PanelManager.Instance.OpenPanel(PanelType.Login);
			break;
		default:
			Debug.LogError("No login mode defined for unsupported channel: " + ApplicationDataManager.Channel);
			ShowLoginErrorPopup(LocalizedStrings.Error, string.Concat("No login mode defined for unsupported channel: ", ApplicationDataManager.Channel, "\nPlease visit support.uberstrike.com"));
			break;
		}
	}

	public IEnumerator StartLoginMemberEmail(string emailAddress, string password)
	{
		if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(password))
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("email-or-password-empty");
			ShowLoginErrorPopup(LocalizedStrings.Error, "Your login credentials are not correct. Please try to login again.");
			yield break;
		}
		_progress.Text = "Authenticating Account";
		_progress.Progress = 0.1f;
		PopupSystem.Show(_progress);
		if (Application.isEditor && (ApplicationDataManager.Channel == ChannelType.WebFacebook || ApplicationDataManager.Channel == ChannelType.IPad))
		{
			_progress.Text = "Initializing Facebook...";
			yield return MonoRoutine.Start(AutoMonoBehaviour<FacebookInterface>.Instance.Init());
			if (!AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn && ApplicationDataManager.Channel != ChannelType.IPad)
			{
				_progress.Text = "Facebook Logging In...";
				yield return MonoRoutine.Start(AutoMonoBehaviour<FacebookInterface>.Instance.Login());
			}
		}
		if ((bool)AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn)
		{
			_progress.Text = "Authenticating Facebook account...";
			bool isFacebookValid = false;
			yield return FacebookWebServiceClient.CheckFacebookSession(PlayerDataManager.AuthToken, AutoMonoBehaviour<FacebookInterface>.Instance.UserId, delegate(bool ev)
			{
				isFacebookValid = ev;
			}, delegate(Exception ex)
			{
				DebugConsoleManager.SendExceptionReport(ex);
			});
			if (!isFacebookValid)
			{
				AutoMonoBehaviour<FacebookInterface>.Instance.Logout();
			}
		}
		MemberAuthenticationResultView authenticationView = null;
		yield return AuthenticationWebServiceClient.LoginMemberEmail(emailAddress, password, ApplicationDataManager.Channel, SystemInfo.deviceUniqueIdentifier, delegate(MemberAuthenticationResultView ev)
		{
			authenticationView = ev;
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex);
		});
		if (authenticationView == null)
		{
			ApplicationDataManager.EventsSystem.SendLoginError();
			ShowLoginErrorPopup(LocalizedStrings.Error, "The login could not be processed. Please check your internet connection and try again.");
		}
		else
		{
			yield return MonoRoutine.Start(CompleteAuthentication(authenticationView));
		}
	}

	private void FacebookHideGameCallback(bool isGameShown)
	{
		if (!isGameShown)
		{
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = 1f;
		}
	}

	public IEnumerator StartLoginMemberFacebook(string facebookID, string hash)
	{
		_progress.Text = "Authenticating Account";
		_progress.Progress = 0.1f;
		PopupSystem.Show(_progress);
		_progress.Text = "Initializing Facebook...";
		yield return MonoRoutine.Start(AutoMonoBehaviour<FacebookInterface>.Instance.Init());
		if (!AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn)
		{
			_progress.Text = "Facebook Logging In...";
			yield return MonoRoutine.Start(AutoMonoBehaviour<FacebookInterface>.Instance.Login());
		}
		if (!AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn)
		{
			ShowLoginErrorPopup(LocalizedStrings.Error, "You need to authorize UberStrike with Facebook to play.");
			yield break;
		}
		MemberAuthenticationResultView authenticationView = null;
		yield return AuthenticationWebServiceClient.LoginMemberFacebookUnitySdk(FB.AccessToken, SystemInfo.deviceUniqueIdentifier, delegate(MemberAuthenticationResultView ev)
		{
			authenticationView = ev;
		}, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoginError();
			DebugConsoleManager.SendExceptionReport(ex);
		});
		yield return MonoRoutine.Start(CompleteAuthentication(authenticationView));
	}

	public IEnumerator StartLoginMemberPortal(int cmid, string hash)
	{
		_progress.Progress = 0.1f;
		_progress.Text = "Authenticating Account";
		PopupSystem.Show(_progress);
		if (!string.IsNullOrEmpty(hash) && cmid > 0)
		{
			MemberAuthenticationResultView authenticationView = null;
			yield return AuthenticationWebServiceClient.LoginMemberPortal(cmid, hash, SystemInfo.deviceUniqueIdentifier, delegate(MemberAuthenticationResultView ev)
			{
				authenticationView = ev;
			}, delegate(Exception ex)
			{
				ApplicationDataManager.EventsSystem.SendLoginError();
				DebugConsoleManager.SendExceptionReport(ex, "There was an error authenticating your account. Please refresh the page and try again.");
			});
			yield return MonoRoutine.Start(CompleteAuthentication(authenticationView));
		}
		else
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("hash-or-cmid-empty");
			ShowLoginErrorPopup(LocalizedStrings.Error, "Your login credentials are not correct. Please refresh the page to try again.");
		}
	}

	public IEnumerator StartLoginMemberFacebook(string facebookAuthToken)
	{
		_progress.Text = "Authenticating Account";
		_progress.Progress = 0.1f;
		PopupSystem.Show(_progress);
		if (!string.IsNullOrEmpty(facebookAuthToken))
		{
			MemberAuthenticationResultView authenticationView = null;
			yield return AuthenticationWebServiceClient.FacebookSingleSignOn(facebookAuthToken, ApplicationDataManager.Channel, SystemInfo.deviceUniqueIdentifier, delegate(MemberAuthenticationResultView ev)
			{
				authenticationView = ev;
			}, delegate(Exception ex)
			{
				ApplicationDataManager.EventsSystem.SendLoginError();
				DebugConsoleManager.SendExceptionReport(ex);
			});
			if (authenticationView != null && authenticationView.MemberAuthenticationResult != MemberAuthenticationResult.Ok)
			{
				AutoMonoBehaviour<FacebookInterface>.Instance.Logout();
			}
			yield return MonoRoutine.Start(CompleteAuthentication(authenticationView));
		}
		else
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("facebook-authtoken-empty");
			ShowLoginErrorPopup(LocalizedStrings.Error, "Your login was invalid. Please try again.");
		}
	}

	private IEnumerator CompleteAuthentication(MemberAuthenticationResultView authView, bool isRegistrationLogin = false)
	{
		if (authView == null)
		{
			ApplicationDataManager.EventsSystem.SendLoginError();
			ShowLoginErrorPopup(LocalizedStrings.Error, "There was an error with your authentication. Please contact us on support.cmune.com");
			yield break;
		}
		if (authView.MemberAuthenticationResult == MemberAuthenticationResult.IsBanned || authView.MemberAuthenticationResult == MemberAuthenticationResult.IsIpBanned)
		{
			ApplicationDataManager.EventsSystem.SendLoginInvalid("rejected-with-id-" + authView.MemberAuthenticationResult);
			ApplicationDataManager.LockApplication(LocalizedStrings.YourAccountHasBeenBanned);
			yield break;
		}
		if (authView.MemberAuthenticationResult != MemberAuthenticationResult.Ok)
		{
			Debug.Log("Result: " + authView.MemberAuthenticationResult);
			ApplicationDataManager.EventsSystem.SendLoginInvalid("rejected-with-id-" + authView.MemberAuthenticationResult);
			ShowLoginErrorPopup(LocalizedStrings.Error, "Your login credentials are not correct. Please try to login again.");
			yield break;
		}
		Singleton<PlayerDataManager>.Instance.SetLocalPlayerMemberView(authView.MemberView);
		PlayerDataManager.AuthToken = authView.AuthToken;
		ApplicationDataManager.ServerDateTime = authView.ServerTime;
		ApplicationDataManager.EventsSystem.SendLogin(PlayerDataManager.CmidSecure);
		if (isRegistrationLogin)
		{
			AutoMonoBehaviour<MATInterface>.Instance.RecordRegistration(PlayerDataManager.CmidSecure);
		}
		if (ApplicationDataManager.IsMobile)
		{
			CmuneEventHandler.Route(new LoginEvent(MemberAccessLevel.Default));
		}
		else
		{
			CmuneEventHandler.Route(new LoginEvent(authView.MemberView.PublicProfile.AccessLevel));
		}
		_progress.Text = LocalizedStrings.LoadingFriendsList;
		_progress.Progress = 0.2f;
		yield return MonoRoutine.Start(Singleton<CommsManager>.Instance.GetContactsByGroups());
		if ((bool)AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn)
		{
			yield return MonoRoutine.Start(Singleton<CommsManager>.Instance.MergeFacebookFriends());
		}
		_progress.Text = LocalizedStrings.LoadingCharacterData;
		_progress.Progress = 0.3f;
		yield return ApplicationWebServiceClient.GetConfigurationData("4.4.2m", delegate(ApplicationConfigurationView appConfigView)
		{
			XpPointsUtil.Config = appConfigView;
		}, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			DebugConsoleManager.SendExceptionReport(ex);
			ApplicationDataManager.LockApplication(LocalizedStrings.ErrorLoadingData);
		});
		Singleton<PlayerDataManager>.Instance.SetPlayerStatisticsView(authView.PlayerStatisticsView);
		DefinitionType definition = DefinitionType.StandardDefinition;
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.WindowsStandalone:
		case ChannelType.MacAppStore:
			definition = DefinitionType.HighDefinition;
			break;
		case ChannelType.Android:
			definition = DefinitionType.Android;
			break;
		case ChannelType.IPhone:
		case ChannelType.IPad:
			definition = DefinitionType.iPhone;
			break;
		}
		_progress.Text = LocalizedStrings.LoadingMapData;
		_progress.Progress = 0.5f;
		bool mapsLoadedSuccessfully = false;
		yield return ApplicationWebServiceClient.GetMaps("4.4.2m", definition, delegate(List<MapView> callback)
		{
			mapsLoadedSuccessfully = Singleton<MapManager>.Instance.InitializeMapsToLoad(callback);
		}, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			DebugConsoleManager.SendExceptionReport(ex);
			ApplicationDataManager.LockApplication(LocalizedStrings.ErrorLoadingMaps);
		});
		if (!mapsLoadedSuccessfully)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			ShowLoginErrorPopup(LocalizedStrings.Error, LocalizedStrings.ErrorLoadingMapsSupport);
			PopupSystem.HideMessage(_progress);
			yield break;
		}
		_progress.Progress = 0.6f;
		_progress.Text = LocalizedStrings.LoadingWeaponAndGear;
		yield return MonoRoutine.Start(Singleton<ItemManager>.Instance.StartGetShop());
		if (!Singleton<ItemManager>.Instance.ValidateItemMall())
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			ShowLoginErrorPopup(LocalizedStrings.ErrorGettingShopData, LocalizedStrings.ErrorGettingShopDataSupport);
			PopupSystem.HideMessage(_progress);
			yield break;
		}
		_progress.Progress = 0.7f;
		_progress.Text = LocalizedStrings.LoadingPlayerInventory;
		yield return MonoRoutine.Start(Singleton<ItemManager>.Instance.StartGetInventory(false));
		_progress.Progress = 0.8f;
		_progress.Text = LocalizedStrings.GettingPlayerLoadout;
		yield return MonoRoutine.Start(Singleton<PlayerDataManager>.Instance.StartGetLoadout());
		if (!Singleton<LoadoutManager>.Instance.ValidateLoadout())
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			ShowLoginErrorPopup(LocalizedStrings.ErrorGettingPlayerLoadout, LocalizedStrings.ErrorGettingPlayerLoadoutSupport);
			yield break;
		}
		_progress.Progress = 0.85f;
		_progress.Text = LocalizedStrings.LoadingPlayerStatistics;
		yield return MonoRoutine.Start(Singleton<PlayerDataManager>.Instance.StartGetMember());
		if (!Singleton<PlayerDataManager>.Instance.ValidateMemberData())
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			ShowLoginErrorPopup(LocalizedStrings.ErrorGettingPlayerStatistics, LocalizedStrings.ErrorPlayerStatisticsSupport);
			yield break;
		}
		_progress.Progress = 0.9f;
		_progress.Text = LocalizedStrings.LoadingClanData;
		yield return ClanWebServiceClient.GetMyClanId(PlayerDataManager.AuthToken, delegate(int id)
		{
			PlayerDataManager.ClanID = id;
			Singleton<ClanDataManager>.Instance.RefreshClanData(true);
		}, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			DebugConsoleManager.SendExceptionReport(ex);
		});
		GameState.LocalAvatar.Decorator = Singleton<AvatarBuilder>.Instance.CreateLocalAvatar();
		yield return new WaitForEndOfFrame();
		Singleton<LotteryManager>.Instance.RefreshLotteryItems();
		yield return new WaitForEndOfFrame();
		Singleton<InboxManager>.Instance.Initialize();
		yield return new WaitForEndOfFrame();
		Singleton<BundleManager>.Instance.Initialize();
		yield return new WaitForEndOfFrame();
		if (!authView.IsTutorialComplete || EditorConfiguration.StartTutorial)
		{
			if (ApplicationDataManager.IsMobile)
			{
				ApplicationDataManager.ApplicationOptions.UseMultiTouch = false;
			}
			_progress.Text = "Loading Tutorial...";
			yield return Singleton<SceneLoader>.Instance.LoadLevel("Tutorial", delegate
			{
				Debug.LogError("Failed to load tutorial!");
				ApplicationDataManager.EventsSystem.SendLoadingError();
			}, delegate(float progress)
			{
				_progress.Progress += progress * 0.05f;
			});
			ApplicationDataManager.EventsSystem.SendStartTutorial(PlayerDataManager.CmidSecure);
			PopupSystem.HideMessage(_progress);
			Screen.lockCursor = true;
			yield break;
		}
		if (!authView.IsAccountComplete)
		{
			PopupSystem.HideMessage(_progress);
			yield return Singleton<SceneLoader>.Instance.LoadLevel("Menu");
			ApplicationDataManager.EventsSystem.SendFinishLoadingApplicationData(PlayerDataManager.CmidSecure);
			PanelManager.Instance.OpenPanel(PanelType.CompleteAccount);
			yield break;
		}
		PopupSystem.HideMessage(_progress);
		yield return Singleton<SceneLoader>.Instance.LoadLevel("Menu");
		ApplicationDataManager.EventsSystem.SendFinishLoadingApplicationData(PlayerDataManager.CmidSecure);
		PerformanceTest.Instance.enabled = true;
		if (authView.LuckyDraw != null)
		{
			LuckyDrawPopup popup = Singleton<LotteryManager>.Instance.RunLuckyDraw(authView.LuckyDraw.ToUnityItem());
			popup.ShowNavigationArrows = false;
			popup.ClickAnywhereToExit = false;
			popup.OnLuckyDrawCompleted += delegate
			{
				MonoRoutine.Start(OnLuckyDrawCompleted());
			};
		}
	}

	private IEnumerator OnLuckyDrawCompleted()
	{
		yield return new WaitForSeconds(3f);
		EtceteraBinding.askForReview(4, 24f, "Review!", "Enjoying UberStrike? Please give us a good review!", "itms-apps://ax.itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=541843276");
	}

	public void StartLogout()
	{
		MonoRoutine.Start(Logout());
	}

	private IEnumerator Logout()
	{
		if (GameState.HasCurrentGame)
		{
			Singleton<GameStateController>.Instance.LeaveGame();
			yield return new WaitForSeconds(3f);
		}
		MenuPageManager.Instance.LoadPage(PageType.Home);
		MenuPageManager.Instance.UnloadCurrentPage();
		GlobalUIRibbon.Instance.Hide();
		GameState.Instance.Reset();
		Singleton<PlayerDataManager>.Instance.Dispose();
		Singleton<InventoryManager>.Instance.Dispose();
		Singleton<LoadoutManager>.Instance.Dispose();
		Singleton<ClanDataManager>.Instance.Dispose();
		Singleton<ChatManager>.Instance.Dispose();
		Singleton<InboxManager>.Instance.Dispose();
		Singleton<TransactionHistory>.Instance.Dispose();
		Singleton<BundleManager>.Instance.Dispose();
		Singleton<LotteryManager>.Instance.Dispose();
		PhotonClient.IsPhotonEnabled = true;
		LobbyConnectionManager.Reconnect();
		CommConnectionManager.Reconnect();
		AutoMonoBehaviour<FacebookInterface>.Instance.Logout();
		InboxThread.Current = null;
		CmuneEventHandler.Route(new LogoutEvent());
		GameData.Instance.MainMenu.Value = MainMenuState.Logout;
		Singleton<AuthenticationManager>.Instance.LoginByChannel();
	}

	private void ShowLoginErrorPopup(string title, string message)
	{
		Debug.Log("Login Error!");
		PopupSystem.HideMessage(_progress);
		PopupSystem.ShowMessage(title, message, PopupSystem.AlertType.OK, delegate
		{
			LoginPanelGUI.ErrorMessage = string.Empty;
			LoginByChannel();
		});
	}
}
