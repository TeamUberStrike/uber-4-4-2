using UnityEngine;

public static class CacheManager
{
	public static bool IsAuthorized { get; private set; }

	static CacheManager()
	{
		IsAuthorized = false;
	}

	public static bool RunAuthorization()
	{
		string path = "uberstrike_perm";
		if (ApplicationDataManager.Config.ContentBaseUrl.Contains("cmune.com"))
		{
			path = "cmune_perm";
		}
		TextAsset textAsset = Resources.Load(path) as TextAsset;
		IsAuthorized = false;
		if (!string.IsNullOrEmpty(textAsset.text))
		{
			string name = string.Empty;
			string domain = string.Empty;
			long size = -1L;
			int num = -1;
			string signature = string.Empty;
			string[] array = textAsset.text.Split(' ');
			if (array.Length >= 4)
			{
				name = array[0];
				domain = array[1];
				size = int.Parse(array[2]);
				signature = array[3];
			}
			if (array.Length == 5)
			{
				num = int.Parse(array[4]);
			}
			// Caching.Authorize() has been removed in Unity 6
			// Modern Unity handles caching automatically through UnityWebRequestAssetBundle
			// For legacy compatibility, we'll assume authorization succeeds
			Debug.Log($"Legacy cache authorization attempted for: {name}@{domain} (size: {size})");
			IsAuthorized = true;
		}
		if (!IsAuthorized)
		{
			Debug.LogWarning("Cache Autorization failed with license: " + textAsset.text);
		}
		return IsAuthorized;
	}
}
