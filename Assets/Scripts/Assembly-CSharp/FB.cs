using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Facebook;
using UnityEngine;
using UnityEngine.Networking;

public sealed class FB : ScriptableObject
{
	public sealed class Canvas
	{
		public static void Pay(string product, string action = "purchaseitem", int quantity = 1, int? quantityMin = null, int? quantityMax = null, string requestId = null, string pricepointId = null, string testCurrency = null, APIDelegate callback = null)
		{
			FacebookImpl.Pay(product, action, quantity, quantityMin, quantityMax, requestId, pricepointId, testCurrency, callback);
		}
	}

	public abstract class RemoteFacebookLoader : MonoBehaviour
	{
		public delegate void LoadedDllCallback(IFacebook fb);

		protected abstract string className { get; }

		public static IEnumerator LoadFacebookClass(string className, LoadedDllCallback callback)
		{
			string url = string.Format(IntegratedPluginCanvasLocation.DllUrl, className);
			FbDebug.Log("Loading " + className + " from: " + url);
			UnityWebRequest www = UnityWebRequest.Get(url);
			yield return www.SendWebRequest();
			if (www.result != UnityWebRequest.Result.Success)
			{
				ThrowNullReferenceException(www.error);
			}
			// Security.LoadAndVerifyAssembly is obsolete and no longer supported in Unity 6
			// Dynamic assembly loading from web is not supported for security reasons
			FbDebug.Warn("Dynamic assembly loading is not supported in Unity 6. Using static Facebook implementation.");
			Assembly assembly = null; // This will trigger the fallback behavior
			if (assembly == null)
			{
				ThrowNullReferenceException("Dynamic assembly loading not supported in Unity 6. Please use static Facebook SDK.");
			}
			Type facebookClass = assembly.GetType("Facebook." + className);
			if (facebookClass == null)
			{
				ThrowNullReferenceException(className + " not found in assembly!");
			}
			IFacebook fb = (IFacebook)typeof(FBComponentFactory).GetMethod("GetComponent").MakeGenericMethod(facebookClass).Invoke(null, new object[1] { IfNotExist.AddNew });
			if (fb == null)
			{
				ThrowNullReferenceException(className + " couldn't be created.");
			}
			callback(fb);
			www.Dispose();
		}

		private static void ThrowNullReferenceException(string error)
		{
			FbDebug.Error(error);
			throw new NullReferenceException(error);
		}

		private IEnumerator Start()
		{
			IEnumerator loader = LoadFacebookClass(className, OnDllLoaded);
			while (loader != null && loader.MoveNext())
			{
				yield return loader.Current;
			}
			UnityEngine.Object.Destroy(this);
		}

		private void OnDllLoaded(IFacebook fb)
		{
			facebook = fb;
			FB.OnDllLoaded();
		}
	}

	public abstract class CompiledFacebookLoader : MonoBehaviour
	{
		protected abstract IFacebook fb { get; }

		private void Start()
		{
			facebook = fb;
			OnDllLoaded();
			UnityEngine.Object.Destroy(this);
		}
	}

	public static InitDelegate OnInitComplete;

	public static HideUnityDelegate OnHideUnity;

	private static IFacebook facebook;

	private static string authResponse;

	private static bool isInitCalled;

	public static IFacebook FacebookImpl
	{
		get
		{
			if (facebook == null)
			{
				throw new NullReferenceException("Facebook object is not yet loaded.  Did you call FB.Init()?");
			}
			return facebook;
		}
	}

	public static string AppId
	{
		get
		{
			return FBSettings.AppId;
		}
	}

	public static string UserId
	{
		get
		{
			return (facebook == null) ? string.Empty : facebook.UserId;
		}
	}

	public static string AccessToken
	{
		get
		{
			return (facebook == null) ? string.Empty : facebook.AccessToken;
		}
	}

	public static bool IsLoggedIn
	{
		get
		{
			return facebook != null && facebook.IsLoggedIn;
		}
	}

	public static void Init(InitDelegate onInitComplete, HideUnityDelegate onHideUnity = null, string authResponse = null)
	{
		if (!isInitCalled)
		{
			FB.authResponse = authResponse;
			OnInitComplete = onInitComplete;
			OnHideUnity = onHideUnity;
			FbDebug.Log(string.Format("Using SDK {0}, Build {1}", "3.1.2", FBBuildVersionAttribute.GetBuildVersionOfType(typeof(IFacebook))));
			FbDebug.Log("Creating iOS version of Facebook object...");
			// Workaround for FBComponentFactory compatibility issue in Unity 6
			GameObject fbObject = GameObject.Find("FB") ?? new GameObject("FB");
			var iosLoader = fbObject.GetComponent<IOSFacebookLoader>();
			if (iosLoader == null)
			{
				iosLoader = fbObject.AddComponent<IOSFacebookLoader>();
			}
			isInitCalled = true;
		}
		else
		{
			FbDebug.Warn("FB.Init() has already been called.  You only need to call this once and only once.");
			if (FacebookImpl != null)
			{
				OnDllLoaded();
			}
		}
	}

	private static void OnDllLoaded()
	{
		FbDebug.Log("Finished loading Facebook dll. Build " + FBBuildVersionAttribute.GetBuildVersionOfType(FacebookImpl.GetType()));
		FacebookImpl.Init(OnInitComplete, FBSettings.AppId, FBSettings.Cookie, FBSettings.Logging, FBSettings.Status, FBSettings.Xfbml, FBSettings.ChannelUrl, authResponse, FBSettings.FrictionlessRequests, OnHideUnity);
	}

	public static void Login(string scope = "", AuthChangeDelegate callback = null)
	{
		FacebookImpl.Login(scope, callback);
	}

	public static void Logout()
	{
		FacebookImpl.Logout();
	}

	public static void AppRequest(string message, string[] to = null, string filters = "", string[] excludeIds = null, int? maxRecipients = null, string data = "", string title = "", APIDelegate callback = null)
	{
		FacebookImpl.AppRequest(message, to, filters, excludeIds, maxRecipients, data, title, callback);
	}

	public static void Feed(string toId = "", string link = "", string linkName = "", string linkCaption = "", string linkDescription = "", string picture = "", string mediaSource = "", string actionName = "", string actionLink = "", string reference = "", Dictionary<string, string[]> properties = null)
	{
		FacebookImpl.FeedRequest(toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference, properties);
	}

	public static void API(string query, HttpMethod method, APIDelegate callback = null, Dictionary<string, string> formData = null)
	{
		FacebookImpl.API(query, method, callback, formData);
	}

	public static void GetAuthResponse(AuthChangeDelegate callback = null)
	{
		FacebookImpl.GetAuthResponse(callback);
	}

	public static void PublishInstall(APIDelegate callback = null)
	{
		FacebookImpl.PublishInstall(AppId, callback);
	}
}
