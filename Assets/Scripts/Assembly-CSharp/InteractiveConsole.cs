using System;
using System.Collections.Generic;
using Facebook;
using UnityEngine;

public sealed class InteractiveConsole : MonoBehaviour
{
	private bool isInit;

	public string FriendSelectorTitle = string.Empty;

	public string FriendSelectorMessage = "Derp";

	public string FriendSelectorFilters = "[\"all\",\"app_users\",\"app_non_users\"]";

	public string FriendSelectorData = "{}";

	public string FriendSelectorExcludeIds = string.Empty;

	public string FriendSelectorMax = string.Empty;

	public string DirectRequestTitle = string.Empty;

	public string DirectRequestMessage = "Herp";

	private string DirectRequestTo = string.Empty;

	public string FeedToId = string.Empty;

	public string FeedLink = string.Empty;

	public string FeedLinkName = string.Empty;

	public string FeedLinkCaption = string.Empty;

	public string FeedLinkDescription = string.Empty;

	public string FeedPicture = string.Empty;

	public string FeedMediaSource = string.Empty;

	public string FeedActionName = string.Empty;

	public string FeedActionLink = string.Empty;

	public string FeedReference = string.Empty;

	public bool IncludeFeedProperties;

	private Dictionary<string, string[]> FeedProperties = new Dictionary<string, string[]>();

	public string PayProduct = string.Empty;

	public string ApiQuery = string.Empty;

	private string status = "Ready";

	private string lastResponse = string.Empty;

	public GUIStyle textStyle = new GUIStyle();

	private Vector2 scrollPosition = Vector2.zero;

	private int buttonHeight = 60;

	private int mainWindowWidth = 610;

	private int mainWindowFullWidth = 640;

	private int TextWindowHeight
	{
		get
		{
			return (!IsHorizontalLayout()) ? 85 : Screen.height;
		}
	}

	private void CallFBInit()
	{
		FB.Init(OnInitComplete, OnHideUnity);
	}

	private void OnInitComplete()
	{
		Debug.Log("FB.Init completed");
		isInit = true;
	}

	private void OnHideUnity(bool isGameShown)
	{
		Debug.Log("Is game showing? " + isGameShown);
	}

	private void CallFBLogin()
	{
		FB.Login("email");
	}

	private void CallFBPublishInstall()
	{
		FB.PublishInstall(PublishComplete);
	}

	private void PublishComplete(string response)
	{
		Debug.Log("publish response: " + response);
	}

	private void CallAppRequestAsFriendSelector()
	{
		int? maxRecipients = null;
		if (FriendSelectorMax != string.Empty)
		{
			try
			{
				maxRecipients = int.Parse(FriendSelectorMax);
			}
			catch (Exception ex)
			{
				status = ex.Message;
			}
		}
		string[] excludeIds = ((!(FriendSelectorExcludeIds == string.Empty)) ? FriendSelectorExcludeIds.Split(',') : null);
		string friendSelectorFilters = FriendSelectorFilters;
		FB.AppRequest(FriendSelectorMessage, null, friendSelectorFilters, excludeIds, maxRecipients, FriendSelectorData, FriendSelectorTitle, Callback);
	}

	private void CallAppRequestAsDirectRequest()
	{
		if (DirectRequestTo == string.Empty)
		{
			throw new ArgumentException("\"To Comma Ids\" must be specificed", "to");
		}
		string directRequestTitle = DirectRequestTitle;
		APIDelegate callback = Callback;
		FB.AppRequest(DirectRequestMessage, DirectRequestTo.Split(','), string.Empty, null, null, string.Empty, directRequestTitle, callback);
	}

	private void CallFBFeed()
	{
		Dictionary<string, string[]> properties = null;
		if (IncludeFeedProperties)
		{
			properties = FeedProperties;
		}
		FB.Feed(FeedToId, FeedLink, FeedLinkName, FeedLinkCaption, FeedLinkDescription, FeedPicture, FeedMediaSource, FeedActionName, FeedActionLink, FeedReference, properties);
	}

	private void CallFBPay()
	{
		FB.Canvas.Pay(PayProduct);
	}

	private void CallFBAPI()
	{
		FB.API(ApiQuery, HttpMethod.GET, Callback);
	}

	private void Awake()
	{
		textStyle.alignment = TextAnchor.UpperLeft;
		textStyle.wordWrap = true;
		textStyle.padding = new RectOffset(10, 10, 10, 10);
		textStyle.stretchHeight = true;
		textStyle.stretchWidth = false;
		FeedProperties.Add("key1", new string[1] { "valueString1" });
		FeedProperties.Add("key2", new string[2] { "valueString2", "http://www.facebook.com" });
	}

	private void OnGUI()
	{
		if (IsHorizontalLayout())
		{
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
		}
		GUILayout.Box("Status: " + status, GUILayout.MinWidth(mainWindowWidth));
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			scrollPosition.y += Input.GetTouch(0).deltaPosition.y;
		}
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MinWidth(mainWindowFullWidth));
		GUILayout.BeginVertical();
		GUI.enabled = !isInit;
		if (Button("FB.Init"))
		{
			CallFBInit();
			status = "FB.Init() called with " + FB.AppId;
		}
		GUI.enabled = isInit && !FB.IsLoggedIn;
		if (Button("Login"))
		{
			CallFBLogin();
			status = "Login called";
		}
		GUI.enabled = isInit;
		if (Button("Publish Install"))
		{
			CallFBPublishInstall();
			status = "Install Published";
		}
		GUI.enabled = FB.IsLoggedIn;
		GUILayout.Space(10f);
		LabelAndTextField("Title (optional): ", ref FriendSelectorTitle);
		LabelAndTextField("Message: ", ref FriendSelectorMessage);
		LabelAndTextField("Exclude Ids (optional): ", ref FriendSelectorExcludeIds);
		LabelAndTextField("Filters (optional): ", ref FriendSelectorFilters);
		LabelAndTextField("Max Recipients (optional): ", ref FriendSelectorMax);
		LabelAndTextField("Data (optional): ", ref FriendSelectorData);
		if (Button("Open Friend Selector"))
		{
			try
			{
				CallAppRequestAsFriendSelector();
				status = "Friend Selector called";
			}
			catch (Exception ex)
			{
				status = ex.Message;
			}
		}
		GUILayout.Space(10f);
		LabelAndTextField("Title (optional): ", ref DirectRequestTitle);
		LabelAndTextField("Message: ", ref DirectRequestMessage);
		LabelAndTextField("To Comma Ids: ", ref DirectRequestTo);
		if (Button("Open Direct Request"))
		{
			try
			{
				CallAppRequestAsDirectRequest();
				status = "Direct Request called";
			}
			catch (Exception ex2)
			{
				status = ex2.Message;
			}
		}
		GUILayout.Space(10f);
		LabelAndTextField("To Id (optional): ", ref FeedToId);
		LabelAndTextField("Link (optional): ", ref FeedLink);
		LabelAndTextField("Link Name (optional): ", ref FeedLinkName);
		LabelAndTextField("Link Desc (optional): ", ref FeedLinkDescription);
		LabelAndTextField("Link Caption (optional): ", ref FeedLinkCaption);
		LabelAndTextField("Picture (optional): ", ref FeedPicture);
		LabelAndTextField("Media Source (optional): ", ref FeedMediaSource);
		LabelAndTextField("Action Name (optional): ", ref FeedActionName);
		LabelAndTextField("Action Link (optional): ", ref FeedActionLink);
		LabelAndTextField("Reference (optional): ", ref FeedReference);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Properties (optional)", GUILayout.Width(150f));
		IncludeFeedProperties = GUILayout.Toggle(IncludeFeedProperties, "Include");
		GUILayout.EndHorizontal();
		if (Button("Open Feed Dialog"))
		{
			try
			{
				CallFBFeed();
				status = "Feed dialog called";
			}
			catch (Exception ex3)
			{
				status = ex3.Message;
			}
		}
		GUILayout.Space(10f);
		LabelAndTextField("API: ", ref ApiQuery);
		if (Button("Call API"))
		{
			CallFBAPI();
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
		if (IsHorizontalLayout())
		{
			GUILayout.EndVertical();
		}
		GUI.enabled = true;
		GUILayout.TextArea(string.Format(" AppId: {0} \n Facebook Dll: {1} \n UserId: {2}\n IsLoggedIn: {3}\n AccessToken: {4}\n\n {5}", FB.AppId, (!isInit) ? "Not Loaded" : "Loaded Successfully", FB.UserId, FB.IsLoggedIn, FB.AccessToken, lastResponse), textStyle, GUILayout.Width(640f), GUILayout.Height(TextWindowHeight));
		if (IsHorizontalLayout())
		{
			GUILayout.EndHorizontal();
		}
	}

	private void Callback(string response)
	{
		lastResponse = "Success Response:\n" + response;
	}

	private bool Button(string label)
	{
		return GUILayout.Button(label, GUILayout.MinHeight(buttonHeight));
	}

	private void LabelAndTextField(string label, ref string text)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, GUILayout.Width(150f));
		text = GUILayout.TextField(text, GUILayout.MinWidth(300f));
		GUILayout.EndHorizontal();
	}

	private bool IsHorizontalLayout()
	{
		return Screen.orientation == ScreenOrientation.LandscapeLeft;
	}
}
