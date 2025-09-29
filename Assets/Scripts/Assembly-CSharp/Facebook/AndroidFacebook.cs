using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

namespace Facebook
{
	internal sealed class AndroidFacebook : MonoBehaviour, IFacebook
	{
		public const int BrowserDialogMode = 0;

		private const string AccessTokenKey = "access_token";

		private const string AndroidJavaFacebookClass = "com.facebook.unity.FB";

		private const string CallbackIdKey = "callback_id";

		private string userId;

		private string accessToken;

		private bool isLoggedIn;

		private int nextApiDelegateId;

		private Dictionary<string, APIDelegate> apiDelegates = new Dictionary<string, APIDelegate>();

		private List<AuthChangeDelegate> authChangeDelegates = new List<AuthChangeDelegate>();

		private InitDelegate onInitComplete;

		public string UserId
		{
			get
			{
				return userId;
			}
		}

		public string AccessToken
		{
			get
			{
				return accessToken;
			}
		}

		public bool IsLoggedIn
		{
			get
			{
				return isLoggedIn;
			}
		}

		public int DialogMode
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		private void CallFB(string method, string args)
		{
			FbDebug.Error("Using Android when not on an Android build!  Doesn't Work!");
		}

		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
			accessToken = string.Empty;
			isLoggedIn = false;
			userId = string.Empty;
			nextApiDelegateId = 0;
		}

		private bool IsErrorResponse(string response)
		{
			return false;
		}

		public void Init(InitDelegate onInitComplete, string appId, bool cookie = false, bool logging = true, bool status = true, bool xfbml = false, string channelUrl = "", string authResponse = null, bool frictionlessRequests = false, HideUnityDelegate hideUnityDelegate = null)
		{
			if (appId == null || appId == string.Empty)
			{
				throw new ArgumentException("appId cannot be null or empty!");
			}
			FbDebug.Log("start android init");
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (appId != string.Empty)
			{
				dictionary.Add("appId", appId);
			}
			if (cookie)
			{
				dictionary.Add("cookie", true);
			}
			if (!logging)
			{
				dictionary.Add("logging", false);
			}
			if (!status)
			{
				dictionary.Add("status", false);
			}
			if (xfbml)
			{
				dictionary.Add("xfbml", true);
			}
			if (channelUrl != string.Empty)
			{
				dictionary.Add("channelUrl", channelUrl);
			}
			if (authResponse != null)
			{
				dictionary.Add("authResponse", authResponse);
			}
			if (frictionlessRequests)
			{
				dictionary.Add("frictionlessRequests", true);
			}
			string text = Json.Serialize(dictionary);
			this.onInitComplete = onInitComplete;
			CallFB("Init", text.ToString());
		}

		public void OnInitComplete(string message)
		{
			if (onInitComplete != null)
			{
				onInitComplete();
			}
			OnLoginComplete(message);
		}

		public void Login(string scope = "", AuthChangeDelegate callback = null)
		{
			FbDebug.Log("Login(" + scope + ")");
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("scope", scope);
			string args = Json.Serialize(dictionary);
			authChangeDelegates.Add(callback);
			CallFB("Login", args);
		}

		public void OnLoginComplete(string message)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(message);
			if (dictionary.ContainsKey("user_id"))
			{
				isLoggedIn = true;
				userId = (string)dictionary["user_id"];
				accessToken = (string)dictionary["access_token"];
			}
			foreach (AuthChangeDelegate authChangeDelegate in authChangeDelegates)
			{
				authChangeDelegate();
			}
			authChangeDelegates.Clear();
		}

		public void Logout()
		{
			CallFB("Logout", string.Empty);
		}

		public void OnLogoutComplete(string message)
		{
			FbDebug.Log("OnLogoutComplete");
			isLoggedIn = false;
			userId = string.Empty;
			accessToken = string.Empty;
		}

		public void AppRequest(string message, string[] to = null, string filters = "", string[] excludeIds = null, int? maxRecipients = null, string data = "", string title = "", APIDelegate callback = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["message"] = message;
			if (callback != null)
			{
				dictionary["callback_id"] = AddApiDelegate(callback);
			}
			if (to != null)
			{
				dictionary["to"] = string.Join(",", to);
			}
			if (filters != string.Empty)
			{
				dictionary["filters"] = filters;
			}
			if (maxRecipients.HasValue)
			{
				dictionary["max_recipients"] = maxRecipients.Value;
			}
			if (data != string.Empty)
			{
				dictionary["data"] = data;
			}
			if (title != string.Empty)
			{
				dictionary["title"] = title;
			}
			CallFB("AppRequest", Json.Serialize(dictionary));
		}

		public void OnAppRequestsComplete(string message)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(message);
			if (!dictionary.ContainsKey("callback_id"))
			{
				return;
			}
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			string callbackId = (string)dictionary["callback_id"];
			dictionary.Remove("callback_id");
			if (dictionary.Count > 0)
			{
				List<string> list = new List<string>(dictionary.Count - 1);
				foreach (string key in dictionary.Keys)
				{
					if (!key.StartsWith("to"))
					{
						dictionary2[key] = dictionary[key];
					}
					else
					{
						list.Add((string)dictionary[key]);
					}
				}
				dictionary2.Add("to", list);
				dictionary.Clear();
				CallApiDelegate(callbackId, Json.Serialize(dictionary2));
			}
			else
			{
				dictionary2.Add("error", "Malformed request response.  Please file a bug with facebook here: https://developers.facebook.com/bugs");
				CallApiDelegate(callbackId, Json.Serialize(dictionary2));
			}
		}

		public void FeedRequest(string toId = "", string link = "", string linkName = "", string linkCaption = "", string linkDescription = "", string picture = "", string mediaSource = "", string actionName = "", string actionLink = "", string reference = "", Dictionary<string, string[]> properties = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (toId != string.Empty)
			{
				dictionary.Add("to", toId);
			}
			if (link != string.Empty)
			{
				dictionary.Add("link", link);
			}
			if (linkName != string.Empty)
			{
				dictionary.Add("name", linkName);
			}
			if (linkCaption != string.Empty)
			{
				dictionary.Add("caption", linkCaption);
			}
			if (linkDescription != string.Empty)
			{
				dictionary.Add("description", linkDescription);
			}
			if (picture != string.Empty)
			{
				dictionary.Add("picture", picture);
			}
			if (mediaSource != string.Empty)
			{
				dictionary.Add("source", mediaSource);
			}
			if (actionName != string.Empty && actionLink != string.Empty)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2.Add("name", actionName);
				dictionary2.Add("link", actionLink);
				dictionary.Add("actions", new Dictionary<string, object>[1] { dictionary2 });
			}
			if (reference != string.Empty)
			{
				dictionary.Add("ref", reference);
			}
			if (properties != null)
			{
				Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
				foreach (KeyValuePair<string, string[]> property in properties)
				{
					if (property.Value.Length >= 1)
					{
						if (property.Value.Length == 1)
						{
							dictionary3.Add(property.Key, property.Value[0]);
							continue;
						}
						Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
						dictionary4.Add("text", property.Value[0]);
						dictionary4.Add("href", property.Value[1]);
						dictionary3.Add(property.Key, dictionary4);
					}
				}
				dictionary.Add("properties", dictionary3);
			}
			CallFB("FeedRequest", Json.Serialize(dictionary));
		}

		public void OnFeedRequestComplete(string message)
		{
			FbDebug.Log("OnFeedRequestComplete: " + message);
		}

		public void Pay(string product, string action = "purchaseitem", int quantity = 1, int? quantityMin = null, int? quantityMax = null, string requestId = null, string pricepointId = null, string testCurrency = null, APIDelegate callback = null)
		{
			throw new PlatformNotSupportedException("There is no Facebook Pay Dialog on Android");
		}

		public void API(string query, HttpMethod method, APIDelegate callback = null, Dictionary<string, string> formData = null, ErrorDelegate errorCallback = null)
		{
			FbDebug.Log("Calling API");
			if (!query.StartsWith("/"))
			{
				query = "/" + query;
			}
			string url = IntegratedPluginCanvasLocation.GraphUrl + query;
			Dictionary<string, string> dictionary = ((formData == null) ? new Dictionary<string, string>(1) : CopyByValue(formData));
			if (!dictionary.ContainsKey("access_token"))
			{
				dictionary["access_token"] = AccessToken;
			}
			StartCoroutine(graphAPI(url, method, callback, dictionary, delegate(string errorString)
			{
				callback(errorString);
			}));
		}

		public void GetAuthResponse(AuthChangeDelegate callback = null)
		{
			authChangeDelegates.Add(callback);
		}

		public void PublishInstall(string appId, APIDelegate callback = null)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
			dictionary["app_id"] = appId;
			if (callback != null)
			{
				dictionary["callback_id"] = AddApiDelegate(callback);
			}
			CallFB("PublishInstall", Json.Serialize(dictionary));
		}

		public void OnPublishInstallComplete(string message)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(message);
			if (dictionary.ContainsKey("callback_id"))
			{
				CallApiDelegate((string)dictionary["callback_id"], string.Empty);
			}
		}

		private string AddApiDelegate(APIDelegate callback)
		{
			nextApiDelegateId++;
			apiDelegates.Add(nextApiDelegateId.ToString(), callback);
			return nextApiDelegateId.ToString();
		}

		private void CallApiDelegate(string callbackId, string result)
		{
			APIDelegate value = null;
			if (apiDelegates.TryGetValue(callbackId, out value))
			{
				FbDebug.Log("Calling callback with value: " + result);
				value(result);
				apiDelegates.Remove(callbackId);
			}
			else
			{
				FbDebug.Warn("Could not find requested callback. " + result);
			}
		}

		private IEnumerator graphAPI(string url, HttpMethod method, APIDelegate callback = null, Dictionary<string, string> formData = null, ErrorDelegate errorCallback = null)
		{
			WWW www;
			if (method == HttpMethod.GET)
			{
				string query = ((!url.Contains("?")) ? "?" : "&");
				if (formData != null)
				{
					foreach (KeyValuePair<string, string> pair in formData)
					{
						query += string.Format("{0}={1}&", Uri.EscapeDataString(pair.Key), Uri.EscapeDataString(pair.Value));
					}
				}
				www = new WWW(url + query);
			}
			else
			{
				WWWForm query2 = new WWWForm();
				foreach (KeyValuePair<string, string> pair2 in formData)
				{
					query2.AddField(pair2.Key, pair2.Value);
				}
				www = new WWW(url, query2);
			}
			FbDebug.Log("Fetching from " + www.url);
			yield return www;
			if (www.error != null)
			{
				if (errorCallback != null)
				{
					errorCallback(www.error);
				}
				else
				{
					FbDebug.Error("Web Error: " + www.error);
				}
			}
			else if (callback != null)
			{
				callback(www.text);
			}
			else
			{
				FbDebug.Log(www.text);
			}
			www.Dispose();
			formData.Clear();
		}

		private Dictionary<string, string> CopyByValue(Dictionary<string, string> data)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(data.Count);
			foreach (KeyValuePair<string, string> datum in data)
			{
				dictionary[datum.Key] = string.Copy(datum.Value);
			}
			return dictionary;
		}

		private void Start()
		{
		}
	}
}
