using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ItemLoadingTask
{
	private string _assetbundleUrl;

	public string PrefabName { get; private set; }

	public float Progress { get; set; }

	public event Action<AssetBundle> OnLoaded = delegate
	{
	};

	public ItemLoadingTask(string prefabName, string version, Action<AssetBundle> OnLoaded)
	{
		if (OnLoaded != null)
		{
			this.OnLoaded = (Action<AssetBundle>)Delegate.Combine(this.OnLoaded, OnLoaded);
		}
		PrefabName = prefabName;
		_assetbundleUrl = string.Format("{0}{1}-{2}.unity3d", ApplicationDataManager.BaseItemsURL, prefabName, version);
	}

	public IEnumerator StartLoading()
	{
		string fileName = Path.GetFileName(_assetbundleUrl);
		string directory = _assetbundleUrl.Remove(_assetbundleUrl.LastIndexOf(fileName));
		yield return MonoRoutine.Start(AssetBundleLoader.LoadAssetBundle(fileName, directory, delegate(float progress)
		{
			Progress = progress;
		}, delegate(AssetBundle bundle)
		{
			this.OnLoaded(bundle);
		}));
	}
}
