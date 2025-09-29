using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoader
{
	public enum LoadingState
	{
		None = 0,
		Loading = 1,
		Loaded = 2,
		Cancelled = 3,
		Error = 4
	}

	private static readonly Dictionary<string, LoadingState> _loadingState = new Dictionary<string, LoadingState>();

	public static void CancelLoading(string fileName)
	{
		LoadingState value;
		if (_loadingState.TryGetValue(fileName, out value) && value == LoadingState.Loading)
		{
			_loadingState[fileName] = LoadingState.Cancelled;
		}
	}

	public static void SetStateToLoaded(string fileName)
	{
		_loadingState[fileName] = LoadingState.Loaded;
	}

	public static LoadingState State(string fileName)
	{
		LoadingState value;
		if (_loadingState.TryGetValue(fileName, out value))
		{
			return value;
		}
		return LoadingState.None;
	}

	public static Coroutine LoadItemAssetBundle(string ItemFileName, Action<float> progress = null, Action<AssetBundle> onLoaded = null, Action<string> onError = null, Action<bool> isDownloading = null)
	{
		return MonoRoutine.Start(LoadAssetBundle(ItemFileName, ApplicationDataManager.BaseItemsURL, progress, onLoaded, onError, isDownloading));
	}

	public static Coroutine LoadMapAssetBundle(string MapFileName, Action<float> progress = null, Action<AssetBundle> onLoaded = null, Action<string> onError = null)
	{
		return MonoRoutine.Start(LoadAssetBundle(MapFileName, ApplicationDataManager.BaseMapsURL, progress, onLoaded, onError));
	}

	public static IEnumerator LoadWebPlayerBundle(string fileName, Action<float> progress = null, Action<AssetBundle> onLoaded = null, Action<string> onError = null)
	{
		string url = Application.absoluteURL.Replace("UberStrikeHeader", fileName);
		int arguments = url.IndexOf('?');
		if (arguments > 0)
		{
			url = url.Remove(arguments);
		}
		Debug.Log("Loading: " + url);
		WWW loader = ((!CacheManager.IsAuthorized || url.StartsWith("file://")) ? new WWW(url) : WWW.LoadFromCacheOrDownload(url, 3));
		while (!loader.isDone)
		{
			yield return new WaitForEndOfFrame();
			if (progress != null)
			{
				progress(loader.progress);
			}
		}
		if (!string.IsNullOrEmpty(loader.error) || loader.assetBundle == null)
		{
			Debug.LogError("Failed to locate Asset " + fileName + ". Error" + loader.error);
			if (onError != null)
			{
				onError("Failed to locate Asset " + fileName + ". Error" + loader.error);
			}
		}
		else
		{
			AssetBundle asset = loader.assetBundle;
			if (onLoaded != null)
			{
				onLoaded(asset);
			}
		}
		if (progress != null)
		{
			progress(1f);
		}
	}

	public static IEnumerator LoadAssetBundle(string fileName, string alternativeBaseUrl, Action<float> progress = null, Action<AssetBundle> onLoaded = null, Action<string> onError = null, Action<bool> isDownloading = null)
	{
		Screen.sleepTimeout = -1;
		_loadingState[fileName] = LoadingState.Loading;
		WWW loader;
		if (!string.IsNullOrEmpty(ApplicationDataManager.BaseStandaloneBundlesURL))
		{
			string url = ApplicationDataManager.BaseStandaloneBundlesURL + fileName;
			loader = new WWW(url);
			while (!loader.isDone && _loadingState[fileName] == LoadingState.Loading)
			{
				yield return new WaitForEndOfFrame();
				if (progress != null)
				{
					progress(loader.progress);
				}
			}
			if (!string.IsNullOrEmpty(loader.error))
			{
				url = alternativeBaseUrl + fileName;
				if (CacheManager.IsAuthorized && !alternativeBaseUrl.StartsWith("file://"))
				{
					if (!Caching.IsVersionCached(url, 1) && isDownloading != null)
					{
						isDownloading(true);
					}
					loader = WWW.LoadFromCacheOrDownload(url, 3);
				}
				else
				{
					if (isDownloading != null)
					{
						isDownloading(true);
					}
					loader = new WWW(url);
				}
			}
		}
		else
		{
			string url2 = alternativeBaseUrl + fileName;
			if (CacheManager.IsAuthorized && !alternativeBaseUrl.StartsWith("file://"))
			{
				loader = WWW.LoadFromCacheOrDownload(url2, 3);
				if (!Caching.IsVersionCached(url2, 1) && isDownloading != null)
				{
					isDownloading(true);
				}
			}
			else
			{
				loader = new WWW(url2);
				if (isDownloading != null)
				{
					isDownloading(true);
				}
			}
		}
		while (!loader.isDone && _loadingState[fileName] == LoadingState.Loading)
		{
			yield return new WaitForEndOfFrame();
			if (progress != null)
			{
				progress(loader.progress);
			}
		}
		if (!string.IsNullOrEmpty(loader.error) || loader.assetBundle == null)
		{
			_loadingState[fileName] = LoadingState.Error;
			Debug.LogError("Failed to locate Asset '" + loader.url + " " + (loader.assetBundle != null) + "' Error" + loader.error);
			if (onError != null)
			{
				onError("Failed to locate Asset '" + loader.url + "' Error" + loader.error);
			}
		}
		else
		{
			_loadingState[fileName] = LoadingState.Loaded;
			AssetBundle asset = loader.assetBundle;
			if (onLoaded != null)
			{
				onLoaded(loader.assetBundle);
			}
		}
		if (progress != null)
		{
			progress(1f);
		}
		Screen.sleepTimeout = -2;
	}

	public static IEnumerator LoadAssetBundleNoCache(string path, Action<float> progress = null, Action<AssetBundle> onLoaded = null, Action<string> onError = null)
	{
		WWW loader = new WWW(path);
		while (!loader.isDone)
		{
			yield return new WaitForEndOfFrame();
			if (progress != null)
			{
				progress(loader.progress);
			}
		}
		if (!string.IsNullOrEmpty(loader.error))
		{
			Debug.LogError("Failed to locate Asset " + path + ". Error" + loader.error);
			if (onError != null)
			{
				onError("Failed to locate Asset " + path + ". Error" + loader.error);
			}
		}
		else if (onLoaded != null)
		{
			onLoaded(loader.assetBundle);
		}
		if (progress != null)
		{
			progress(1f);
		}
	}
}
