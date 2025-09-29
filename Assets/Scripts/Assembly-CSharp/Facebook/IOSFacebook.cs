using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Facebook
{
	internal class IOSFacebook : MonoBehaviour, IFacebook
	{
		private class NativeDict
		{
			public int numEntries;

			public string[] keys;

			public string[] vals;

			public NativeDict()
			{
				numEntries = 0;
				keys = null;
				vals = null;
			}
		}

		public enum FBInsightsFlushBehavior
		{
			FBInsightsFlushBehaviorAuto = 0,
			FBInsightsFlushBehaviorExplicitOnly = 1
		}

		private bool isLoggedIn;

		private string userId;

		private string accessToken;

		private int dialogMode = 1;

		private string appVersion;

		private FBInsightsFlushBehavior flushBehavior;

		private InitDelegate externalInitDelegate;

		private List<AuthChangeDelegate> authResponseCallbacks;

		private int currRequestId;

		private Dictionary<int, APIDelegate> OutstandingRequestCallbacks;

		public bool IsLoggedIn
		{
			get
			{
				return isLoggedIn;
			}
		}

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

		public string AppVersion
		{
			get
			{
				return appVersion;
			}
		}

		public int DialogMode
		{
			get
			{
				return dialogMode;
			}
			set
			{
				dialogMode = value;
				iosSetShareDialogMode(dialogMode);
			}
		}

		public FBInsightsFlushBehavior FlushBehavior
		{
			get
			{
				return flushBehavior;
			}
		}

		[DllImport("__Internal")]
		private static extern void iosInit(bool cookie, bool logging, bool status, bool frictionlessRequests);

		[DllImport("__Internal")]
		private static extern void iosLogin(string scope);

		[DllImport("__Internal")]
		private static extern void iosLogout();

		[DllImport("__Internal")]
		private static extern void iosSetShareDialogMode(int mode);

		[DllImport("__Internal")]
		private static extern void iosFeedRequest(int requestId, string toId, string link, string linkName, string linkCaption, string linkDescription, string picture, string mediaSource, string actionName, string actionLink, string reference);

		[DllImport("__Internal")]
		private static extern void iosAppRequest(int requestId, string message, string[] to = null, int toLength = 0, string filters = "", string[] excludeIds = null, int excludeIdsLength = 0, bool hasMaxRecipients = false, int maxRecipients = 0, string data = "", string title = "");

		[DllImport("__Internal")]
		private static extern void iosCallFbApi(int requestId, string query, string method, string[] formDataKeys = null, string[] formDataVals = null, int formDataLen = 0);

		[DllImport("__Internal")]
		private static extern void iosFBInsightsFlush();

		[DllImport("__Internal")]
		private static extern void iosFBInsightsLogConversionPixel(string pixelID, double value);

		[DllImport("__Internal")]
		private static extern void iosFBInsightsLogPurchase(double purchaseAmount, string currency, int numParams, string[] paramKeys, string[] paramVals);

		[DllImport("__Internal")]
		private static extern void iosFBInsightsSetAppVersion(string version);

		[DllImport("__Internal")]
		private static extern void iosFBInsightsSetFlushBehavior(int behavior);

		[DllImport("__Internal")]
		private static extern void iosFBSettingsPublishInstall(int requestId, string appId);

		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
			accessToken = string.Empty;
			isLoggedIn = false;
			userId = string.Empty;
			currRequestId = 0;
			OutstandingRequestCallbacks = new Dictionary<int, APIDelegate>();
			authResponseCallbacks = new List<AuthChangeDelegate>();
		}

		public void Init(InitDelegate onInitComplete, string appId, bool cookie = false, bool logging = true, bool status = true, bool xfbml = false, string channelUrl = "", string authResponse = null, bool frictionlessRequests = false, HideUnityDelegate hideUnityDelegate = null)
		{
			iosInit(cookie, logging, status, frictionlessRequests);
			externalInitDelegate = onInitComplete;
		}

		public void Login(string scope = "", AuthChangeDelegate callback = null)
		{
			AddAuthCallback(callback);
			iosLogin(scope);
		}

		public void Logout()
		{
			iosLogout();
			isLoggedIn = false;
		}

		public void AppRequest(string message, string[] to = null, string filters = "", string[] excludeIds = null, int? maxRecipients = null, string data = "", string title = "", APIDelegate callback = null)
		{
			iosAppRequest(GetCallbackHandle(callback), message, to, (to != null) ? to.Length : 0, filters, excludeIds, (excludeIds != null) ? excludeIds.Length : 0, maxRecipients.HasValue, maxRecipients.HasValue ? maxRecipients.Value : 0, data, title);
		}

		public void FeedRequest(string toId = "", string link = "", string linkName = "", string linkCaption = "", string linkDescription = "", string picture = "", string mediaSource = "", string actionName = "", string actionLink = "", string reference = "", Dictionary<string, string[]> properties = null)
		{
			iosFeedRequest(GetCallbackHandle(null), toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference);
		}

		public void Pay(string product, string action = "purchaseitem", int quantity = 1, int? quantityMin = null, int? quantityMax = null, string requestId = null, string pricepointId = null, string testCurrency = null, APIDelegate callback = null)
		{
			throw new PlatformNotSupportedException("There is no Facebook Pay Dialog on iOS");
		}

		public void API(string query, HttpMethod method, APIDelegate callback = null, Dictionary<string, string> formData = null, ErrorDelegate errorCallback = null)
		{
			string[] array = null;
			string[] array2 = null;
			if (formData != null && formData.Count > 0)
			{
				array = new string[formData.Count];
				array2 = new string[formData.Count];
				int num = 0;
				foreach (KeyValuePair<string, string> formDatum in formData)
				{
					array[num] = formDatum.Key;
					array2[num] = string.Copy(formDatum.Value);
					num++;
				}
			}
			iosCallFbApi(GetCallbackHandle(callback), query, (method == null) ? null : method.ToString(), array, array2, (formData != null) ? formData.Count : 0);
		}

		public void GetAuthResponse(AuthChangeDelegate callback = null)
		{
			AddAuthCallback(callback);
		}

		public void InsightsFlush()
		{
			iosFBInsightsFlush();
		}

		public void InsightsLogConversionPixel(string pixelID, double val)
		{
			iosFBInsightsLogConversionPixel(pixelID, val);
		}

		public void InsightsLogPurchase(double purchaseAmount, string currency, Dictionary<string, string> properties = null)
		{
			NativeDict nativeDict = MarshallDict(properties);
			iosFBInsightsLogPurchase(purchaseAmount, currency, nativeDict.numEntries, nativeDict.keys, nativeDict.vals);
		}

		public void InsightsSetAppVersion(string version)
		{
			appVersion = version;
			iosFBInsightsSetAppVersion(version);
		}

		public void InsightsSetFlushBehavior(FBInsightsFlushBehavior behavior)
		{
			flushBehavior = behavior;
			iosFBInsightsSetFlushBehavior((int)flushBehavior);
		}

		public void PublishInstall(string appId, APIDelegate callback = null)
		{
			iosFBSettingsPublishInstall(GetCallbackHandle(callback), appId);
		}

		private NativeDict MarshallDict(Dictionary<string, string> dict)
		{
			NativeDict nativeDict = new NativeDict();
			if (dict != null && dict.Count > 0)
			{
				nativeDict.keys = new string[dict.Count];
				nativeDict.vals = new string[dict.Count];
				nativeDict.numEntries = 0;
				foreach (KeyValuePair<string, string> item in dict)
				{
					nativeDict.keys[nativeDict.numEntries] = item.Key;
					nativeDict.vals[nativeDict.numEntries] = item.Value;
					nativeDict.numEntries++;
				}
			}
			return nativeDict;
		}

		private int GetCallbackHandle(APIDelegate callback)
		{
			currRequestId++;
			OutstandingRequestCallbacks.Add(currRequestId, callback);
			return currRequestId;
		}

		private void AddAuthCallback(AuthChangeDelegate callback)
		{
			if (callback != null && !authResponseCallbacks.Contains(callback))
			{
				authResponseCallbacks.Add(callback);
			}
		}

		private void OnInitComplete(string msg)
		{
			externalInitDelegate();
			if (msg != null && msg.Length > 0)
			{
				OnLogin(msg);
			}
		}

		private void OnAuthResponse()
		{
			foreach (AuthChangeDelegate authResponseCallback in authResponseCallbacks)
			{
				authResponseCallback();
			}
			authResponseCallbacks.Clear();
		}

		public void OnLogin(string msg)
		{
			int num = msg.IndexOf(":");
			if (num > 0)
			{
				isLoggedIn = true;
				userId = msg.Substring(0, num);
				accessToken = msg.Substring(num + 1);
			}
			OnAuthResponse();
		}

		public void OnLogout(string msg)
		{
			isLoggedIn = false;
		}

		public void OnRequestComplete(string msg)
		{
			int num = msg.IndexOf(":");
			if (num <= 0)
			{
				FbDebug.Error("Malformed callback from ios.  I expected the form id:message but couldn't find either the ':' character or the id.");
				FbDebug.Error("Here's the message that errored: " + msg);
				return;
			}
			string text = msg.Substring(0, num);
			string text2 = msg.Substring(num + 1);
			FbDebug.Info("id:" + text + " msg:" + text2);
			try
			{
				int key = Convert.ToInt32(text);
				if (OutstandingRequestCallbacks.ContainsKey(key))
				{
					if (OutstandingRequestCallbacks[key] != null)
					{
						OutstandingRequestCallbacks[key](text2);
					}
					OutstandingRequestCallbacks.Remove(key);
				}
			}
			catch (Exception ex)
			{
				FbDebug.Error("Error converting callback id from string to int: " + ex);
				FbDebug.Error("Here's the message that errored: " + msg);
			}
		}
	}
}
