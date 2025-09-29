using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cmune.DataCenter.Common.Entities;
using Facebook;
using MiniJSON;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;

public class FacebookInterface : AutoMonoBehaviour<FacebookInterface>
{
	public const string GIFT_SEND_EVENT_ID = "gift_facebook_sent";

	public const string INVITE_FRIEND_EVENT_ID = "invite_facebook_friend";

	public static List<FacebookUser> GameFriends = new List<FacebookUser>();

	public List<string> FacebookFriendUrls { get; private set; }

	public Property<string> UserId { get; set; }

	public Property<bool> IsInitialized { get; set; }

	public Property<bool> IsLoggedIn { get; set; }

	public Property<List<FacebookRequest>> Requests { get; set; }

	public FacebookCurrency Currency { get; private set; }

	public string Id
	{
		get
		{
			return FB.UserId;
		}
	}

	public string AccessToken
	{
		get
		{
			return FB.AccessToken;
		}
	}

	public void OpenLinkFacebookUrl()
	{
	}

	private void Awake()
	{
		UserId = new Property<string>(string.Empty);
		IsInitialized = new Property<bool>(false);
		IsLoggedIn = new Property<bool>(false);
		Requests = new Property<List<FacebookRequest>>(new List<FacebookRequest>());
		Currency = new FacebookCurrency();
		FacebookFriendUrls = new List<string>();
		if (AutoMonoBehaviour<FacebookInterface>._instance == null)
		{
			AutoMonoBehaviour<FacebookInterface>._instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public IEnumerator Init()
	{
		if (!IsInitialized)
		{
			FB.Init(delegate
			{
				IsInitialized.Value = true;
			});
			while (!IsInitialized)
			{
				yield return 0;
			}
			yield return 0;
			IsLoggedIn.Value = FB.IsLoggedIn;
			if (Application.isEditor)
			{
				EditorFacebook fbEditor = FB.FacebookImpl as EditorFacebook;
				fbEditor.fb.GetType().GetMethod("OnFacebookAuthResponse", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fbEditor.fb, new object[1] { "{\"authResponse\":{\"accessToken\":\"" + AccessToken + "\", \"userID\":\"" + Id + "\"}}" });
			}
		}
	}

	public IEnumerator Login()
	{
		yield return StartCoroutine(Init());
		if (!IsLoggedIn.Value)
		{
			bool error = false;
			string scope = "email,user_games_activity,publish_stream,publish_actions,user_birthday";
			FB.Login(scope, delegate
			{
				error = true;
			});
			while (!FB.IsLoggedIn && !error)
			{
				yield return 0;
			}
			yield return 0;
			UserId.Value = FB.UserId;
			IsLoggedIn.Value = FB.IsLoggedIn;
			if (FB.IsLoggedIn)
			{
				FB.API("/me?fields=currency", HttpMethod.GET, delegate(string el)
				{
					Dictionary<string, object> dictionary = Json.Deserialize(el) as Dictionary<string, object>;
					Currency = JsonHelper.DeserializeFromJson<FacebookCurrency>(dictionary["currency"]);
				});
			}
		}
		if ((bool)IsLoggedIn)
		{
			yield return StartCoroutine(GetGameFriends());
		}
	}

	private IEnumerator GetGameFriends()
	{
		bool ready = false;
		FB.API(FB.AppId + "/scores", HttpMethod.GET, delegate(string el)
		{
			ready = true;
			Dictionary<string, object> dictionary = Json.Deserialize(el) as Dictionary<string, object>;
			List<object> list = dictionary["data"] as List<object>;
			GameFriends = list.ConvertAll((object obj) => JsonHelper.DeserializeFromJson<FacebookUser>((obj as Dictionary<string, object>)["user"]));
			GameFriends.RemoveAll((FacebookUser user) => user.Id.ToString() == AutoMonoBehaviour<FacebookInterface>.Instance.UserId);
		}, new Dictionary<string, string>());
		while (!ready)
		{
			yield return 0;
		}
	}

	public void Logout()
	{
		if ((bool)IsLoggedIn)
		{
			FB.Logout();
			IsLoggedIn.Value = false;
		}
	}

	public void Connect()
	{
		StartCoroutine(ConnectCrt());
	}

	private IEnumerator ConnectCrt()
	{
		yield return StartCoroutine(Login());
		if (string.IsNullOrEmpty(UserId.Value))
		{
			Logout();
			yield break;
		}
		MemberOperationResult operationResult = MemberOperationResult.InvalidCmid;
		yield return FacebookWebServiceClient.AttachFacebookAccountToCmuneAccount(PlayerDataManager.AuthToken, UserId.Value, delegate(MemberOperationResult ev)
		{
			operationResult = ev;
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex, "There was an error Claiming Facebook gift from server. Please refresh the page and try again.");
		});
		if (operationResult != MemberOperationResult.Ok)
		{
			Logout();
			string message = "Sorry, there was an authentication error, please try again later.";
			if (operationResult == MemberOperationResult.DuplicateHandle)
			{
				message = "This Facebook account is already attached to a different UberStrike account";
			}
			if (operationResult == MemberOperationResult.AlreadyHasAnESNSAccountOfThisTypeAttached)
			{
				message = "You already have a different Facebook account set tup to your UberStrike account";
			}
			PopupSystem.ShowMessage("Facebook Connect Failed", message, PopupSystem.AlertType.OK, delegate
			{
				LoginPanelGUI.ErrorMessage = string.Empty;
			});
			Debug.Log("Operation result = " + operationResult);
		}
		else
		{
			StartCoroutine(Singleton<CommsManager>.Instance.MergeFacebookFriends());
		}
	}

	public static AppRequest AppRequestSendGift(string message, string data, string title = "I sent you a gift")
	{
		return AutoMonoBehaviour<FacebookInterface>.Instance.SendAppRequest(message, null, "[\"app_users\",\"all\"]", data, title, "gift_facebook_sent");
	}

	public AppRequest SendAppRequest(string message, string[] to, string filters, string data, string title, string eventId)
	{
		AppRequest request = new AppRequest();
		FB.AppRequest(message, to, filters, null, null, data, title, delegate(string el)
		{
			request.Ready = true;
			if (!string.IsNullOrEmpty(el) && !el.Equals("null") && !el.Contains("://"))
			{
				request.Response = new AppRequest.ReqResult();
				AppRequest.ReqResult reqResult = JsonHelper.Deserialize<AppRequest.ReqResult>(el);
				request.Response.To.AddRange(reqResult.To);
				if (eventId != null)
				{
					foreach (string item in request.Response.To)
					{
						ApplicationDataManager.EventsSystem.SendFacebookAppRequest(eventId, PlayerDataManager.Cmid, item);
					}
				}
			}
		});
		return request;
	}

	public void TryCheckForRequests()
	{
		StartCoroutine(TryCheckForRequestsCrt());
	}

	private IEnumerator TryCheckForRequestsCrt()
	{
		string url = Id + "/apprequests";
		string result = null;
		FB.API(url, HttpMethod.GET, delegate(string el)
		{
			result = el;
		});
		while (result == null)
		{
			yield return 0;
		}
		if (!string.IsNullOrEmpty(result))
		{
			try
			{
				Dictionary<string, object> json = Json.Deserialize(result) as Dictionary<string, object>;
				Requests.Value = JsonHelper.DeserializeFromJson<List<FacebookRequest>>(json["data"]);
				Debug.Log("Requests count " + Requests.Value.Count);
			}
			catch (Exception ex)
			{
				Debug.LogError("Error: " + result + " " + ex);
				Requests.Value = new List<FacebookRequest>();
			}
		}
	}

	public AppRequest RemoveRequest(string requestId)
	{
		AppRequest request = new AppRequest();
		FB.API(requestId, HttpMethod.DELETE, delegate
		{
			request.Ready = true;
		}, new Dictionary<string, string> { { "method", "delete" } });
		return request;
	}

	public void PurchaseBundle(int bundleId)
	{
		string product = "http://staging-payments.cmune.com/bundles/" + bundleId;
		APIDelegate callback = JsOnFacebookPayment;
		FB.Canvas.Pay(product, "purchaseitem", 1, null, null, null, null, null, callback);
	}

	public void PublishScore(int score)
	{
		if (FB.IsLoggedIn)
		{
			FB.API("/me/scores", HttpMethod.POST, PublishFbScoreCallback, new Dictionary<string, string> { 
			{
				"score",
				score.ToString()
			} });
		}
		else
		{
			Application.ExternalCall("usHelper.publishScore", score.ToString());
		}
	}

	public void PublishAchievement(AchievementType achievementType)
	{
		string text = "earn";
		string text2 = string.Empty;
		switch (achievementType)
		{
		case AchievementType.CostEffective:
			text2 = "cost_effective";
			break;
		case AchievementType.HardestHitter:
			text2 = "hardest_hitter";
			break;
		case AchievementType.MostAggressive:
			text2 = "most_aggressive";
			break;
		case AchievementType.MostValuable:
			text2 = "most_valuable";
			break;
		case AchievementType.SharpestShooter:
			text2 = "sharpest_shooter";
			break;
		case AchievementType.TriggerHappy:
			text2 = "trigger_happy";
			break;
		}
		string text3 = "uberstrike";
		string text4 = "https://static-ssl.cmune.com/Facebook/production/" + text2 + ".html";
		FB.API("/me/" + text3 + ":" + text + "?" + text2 + "=" + text4, HttpMethod.POST, PublishFbAchievementCallback, new Dictionary<string, string>());
	}

	public void PublishLevelUp(int level)
	{
		FB.Feed(string.Empty, "https://apps.facebook.com/uberstrike", "UberStrike Level Up!", "I leveled up in UberStrike!", "I just got to level " + level + " in UberStrike! Think you can do better?", "https://static-ssl.cmune.com/UberStrike/Facebook/images/logo.jpg", string.Empty, "Leveled up!", string.Empty, string.Empty);
	}

	public void RefreshWallet()
	{
		ApplicationDataManager.RefreshWallet();
	}

	public void ShareScreenShot()
	{
		MonoRoutine.Start(GetScreenshot());
	}

	private IEnumerator GetScreenshot()
	{
		if (!GameState.HasCurrentSpace || !FB.IsLoggedIn || Screen.width == 0 || Screen.height == 0)
		{
			yield break;
		}
		int width = Mathf.Min(1024, (int)AutoMonoBehaviour<CameraRectController>.Instance.PixelWidth);
		float aspectRatio = (float)width / AutoMonoBehaviour<CameraRectController>.Instance.PixelWidth;
		int height = (int)Mathf.Min(768f, (float)Screen.height * aspectRatio);
		RenderTexture tx = new RenderTexture(width, height, 24);
		RenderTexture prevRenderTexture = RenderTexture.active;
		GameObject screenshotCam = new GameObject("ScreenshotCamera");
		screenshotCam.AddComponent(typeof(Camera));
		bool wasScreenshotTaken = false;
		if ((bool)GameState.CurrentSpace.Camera)
		{
			screenshotCam.camera.CopyFrom(GameState.CurrentSpace.Camera);
			screenshotCam.camera.targetTexture = tx;
			screenshotCam.camera.rect = new Rect(0f, 0f, 1f, 1f);
			screenshotCam.camera.Render();
			wasScreenshotTaken = true;
		}
		if (GameState.LocalPlayer.CurrentCameraControl == LocalPlayer.PlayerState.FirstPerson && (bool)GameState.LocalPlayer.WeaponCamera)
		{
			screenshotCam.camera.CopyFrom(GameState.LocalPlayer.WeaponCamera.camera);
			screenshotCam.camera.targetTexture = tx;
			screenshotCam.camera.rect = new Rect(0f, 0f, 1f, 1f);
			screenshotCam.camera.Render();
			wasScreenshotTaken = true;
		}
		RenderTexture.active = tx;
		if (wasScreenshotTaken)
		{
			SfxManager.Play2dAudioClip(GameAudio.FBScreenshot);
			CmuneEventHandler.Route(new OnScreenshotTakenEvent());
			Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
			screenShot.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
			RenderTexture.active = prevRenderTexture;
			if (screenShot.width > UberstrikeIcons.FBScreenshotWatermark.width + 10 && screenShot.height > UberstrikeIcons.FBScreenshotWatermark.height + 10)
			{
				for (int i = 0; i < UberstrikeIcons.FBScreenshotWatermark.width; i++)
				{
					for (int j = 0; j < UberstrikeIcons.FBScreenshotWatermark.height; j++)
					{
						Color blend = Color.black;
						Color orig = screenShot.GetPixel(i + 10, j + 10);
						Color mark = UberstrikeIcons.FBScreenshotWatermark.GetPixel(i, j);
						blend.r = orig.r * (1f - mark.a) + mark.r * mark.a;
						blend.g = orig.g * (1f - mark.a) + mark.g * mark.a;
						blend.b = orig.b * (1f - mark.a) + mark.b * mark.a;
						screenShot.SetPixel(i + 10, j + 10, blend);
					}
				}
				screenShot.Apply();
			}
			byte[] bytes = screenShot.EncodeToPNG();
			WWWForm form = new WWWForm();
			form.AddBinaryData("image", bytes, "uberstrike.png");
			WWW w = new WWW("https://graph.facebook.com/me/photos?access_token=" + FB.AccessToken, form);
			yield return w;
			if (!string.IsNullOrEmpty(w.error))
			{
				Debug.Log(w.error);
			}
			UnityEngine.Object.DestroyObject(screenShot);
		}
		UnityEngine.Object.DestroyObject(tx);
		UnityEngine.Object.DestroyObject(screenshotCam);
	}

	public void GetFbFriendsCallback(string response)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(response) as Dictionary<string, object>;
		System.Random random = new System.Random();
		List<object> list = (List<object>)dictionary["data"];
		for (int i = 0; i < list.Count; i++)
		{
			int index = random.Next(i, list.Count);
			object value = list[i];
			list[i] = list[index];
			list[index] = value;
		}
		int num = 0;
		foreach (Dictionary<string, object> item in list)
		{
			foreach (KeyValuePair<string, object> item2 in item)
			{
				if (item2.Key.Equals("id"))
				{
					FacebookFriendUrls.Add(string.Concat("https://graph.facebook.com/", item2.Value, "/picture?width=128&height=128"));
					num++;
					if (num > 9)
					{
						break;
					}
				}
			}
		}
	}

	public void JsOnFacebookPayment(string status)
	{
		Singleton<BundleManager>.Instance.OnFacebookPayment(status);
	}

	private void PublishFbLevelUpCallback(string response)
	{
		Debug.Log("PublishFbLevelUpCallback: " + response);
	}

	private void OpenInviteFbFriendsCallback(string response)
	{
		Debug.Log("OpenInviteFbFriendsCallback: " + response);
	}

	private void PublishFbAchievementCallback(string response)
	{
		Debug.Log("OpenInviteFbFriendsCallback: " + response);
	}

	private void PublishFbScoreCallback(string response)
	{
		Debug.Log("PublishFbScoreCallback: " + response);
	}
}
