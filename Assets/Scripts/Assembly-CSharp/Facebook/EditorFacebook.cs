using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Facebook
{
	internal class EditorFacebook : MonoBehaviour, IFacebook
	{
		public IFacebook fb;

		private bool isFakeLoggedIn;

		public string UserId
		{
			get
			{
				return "0";
			}
		}

		public string AccessToken
		{
			get
			{
				return "abcdefghijklmnopqrstuvwxyz";
			}
		}

		public bool IsLoggedIn
		{
			get
			{
				return isFakeLoggedIn;
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

		private void Awake()
		{
			StartCoroutine(FB.RemoteFacebookLoader.LoadFacebookClass("CanvasFacebook", OnDllLoaded));
		}

		public void Init(InitDelegate onInitComplete, string appId, bool cookie = false, bool logging = true, bool status = true, bool xfbml = false, string channelUrl = "", string authResponse = null, bool frictionlessRequests = false, HideUnityDelegate hideUnityDelegate = null)
		{
			StartCoroutine(OnInit(onInitComplete, appId, cookie, logging, status, xfbml, channelUrl, authResponse, frictionlessRequests, hideUnityDelegate));
		}

		private IEnumerator OnInit(InitDelegate onInitComplete, string appId, bool cookie = false, bool logging = true, bool status = true, bool xfbml = false, string channelUrl = "", string authResponse = null, bool frictionlessRequests = false, HideUnityDelegate hideUnityDelegate = null)
		{
			while (fb == null)
			{
				yield return null;
			}
			fb.Init(onInitComplete, appId, cookie, logging, status, xfbml, channelUrl, authResponse, frictionlessRequests, hideUnityDelegate);
			if (status || cookie)
			{
				isFakeLoggedIn = true;
			}
			if (onInitComplete != null)
			{
				onInitComplete();
			}
		}

		private void OnDllLoaded(IFacebook fb)
		{
			this.fb = fb;
		}

		public void Login(string scope = "", AuthChangeDelegate callback = null)
		{
			if (isFakeLoggedIn)
			{
				FbDebug.Warn("User is already logged in.  You don't need to call this again.");
			}
			isFakeLoggedIn = true;
		}

		public void Logout()
		{
			isFakeLoggedIn = false;
		}

		public void AppRequest(string message, string[] to = null, string filters = "", string[] excludeIds = null, int? maxRecipients = null, string data = "", string title = "", APIDelegate callback = null)
		{
			fb.AppRequest(message, to, filters, excludeIds, maxRecipients, data, title, callback);
		}

		public void FeedRequest(string toId = "", string link = "", string linkName = "", string linkCaption = "", string linkDescription = "", string picture = "", string mediaSource = "", string actionName = "", string actionLink = "", string reference = "", Dictionary<string, string[]> properties = null)
		{
			fb.FeedRequest(toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference);
		}

		public void Pay(string product, string action = "purchaseitem", int quantity = 1, int? quantityMin = null, int? quantityMax = null, string requestId = null, string pricepointId = null, string testCurrency = null, APIDelegate callback = null)
		{
			FbDebug.Info("Pay method only works with Facebook Canvas.  Does nothing in the Unity Editor, iOS or Android");
		}

		public void API(string query, HttpMethod method, APIDelegate callback = null, Dictionary<string, string> formData = null, ErrorDelegate errorCallback = null)
		{
			if (query.StartsWith("me"))
			{
				FbDebug.Warn("graph.facebook.com/me does not work within the Unity Editor");
			}
			if (!query.Contains("access_token=") && (formData == null || !formData.ContainsKey("access_token")))
			{
				FbDebug.Warn("Without an access_token param explicitly passed in formData, some API graph calls will 404 error in the Unity Editor.");
			}
			fb.API(query, method, callback, formData, errorCallback);
		}

		public void GetAuthResponse(AuthChangeDelegate callback = null)
		{
			fb.GetAuthResponse(callback);
		}

		public void PublishInstall(string appId, APIDelegate callback = null)
		{
		}
	}
}
