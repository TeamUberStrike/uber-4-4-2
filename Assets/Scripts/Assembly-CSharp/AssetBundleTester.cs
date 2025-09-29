using System;
using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;

[ExecuteInEditMode]
public class AssetBundleTester : MonoBehaviour
{
	private const string BaseContentUrlProduction = "http://static.cmune.com/UberStrike/";

	private const string BaseContentUrlExternalQA = "http://static.cmune.com/UberStrike/Qa/";

	private const string BaseContentUrlInternalDev = "http://client.dev.uberstrike.com/";

	private bool _isReadingUrl;

	private bool _isLoadingBundle;

	private float _loadingProgress;

	private string _searchText = string.Empty;

	private string _bundlePath = "file://";

	private string _baseUrl = string.Empty;

	private ItemCollection _itemCollection;

	private ItemCollectionGrid _itemsGrid;

	private string[] _bundleDirs = new string[3] { "DEV", "QA", "PROD" };

	private int _selectedBundleDir = -1;

	private void Awake()
	{
		_itemCollection = new ItemCollection();
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 10f, 120f, 20f), "AssetBundle Test");
		GUI.Label(new Rect(10f, 40f, 120f, 20f), "AssetBundle Path:");
		GUI.enabled = !_isReadingUrl && !_isLoadingBundle;
		int num = GUI.SelectionGrid(new Rect(125f, 40f, 100f, 60f), _selectedBundleDir, _bundleDirs, 1, GUI.skin.toggle);
		if (num != _selectedBundleDir)
		{
			_selectedBundleDir = num;
			UpdateBundleDir();
		}
		_bundlePath = GUI.TextField(new Rect(125f, 110f, 300f, 20f), _bundlePath);
		if (GUI.Button(new Rect(430f, 110f, 100f, 20f), "Load"))
		{
			_isLoadingBundle = true;
			if (_itemsGrid != null)
			{
				_itemsGrid.Dispose();
			}
			StartCoroutine(AssetBundleLoader.LoadAssetBundleNoCache(_bundlePath, delegate(float p)
			{
				_loadingProgress = p;
			}, OnAssetbundleLoaded));
		}
		if (_loadingProgress > 0f)
		{
			GUI.Label(new Rect(540f, 110f, 100f, 20f), _loadingProgress * 100f + "%");
		}
		GUI.Label(new Rect(10f, 140f, 100f, 20f), "Find item name: ");
		_searchText = GUI.TextField(new Rect(125f, 140f, 100f, 20f), _searchText);
		if (_itemsGrid != null)
		{
			_itemsGrid.SetFilter(_searchText);
			_itemsGrid.Draw(new Rect(0f, 160f, Screen.width, Screen.height - 160));
		}
		GUI.enabled = true;
	}

	private void UpdateBundleDir()
	{
		switch (_selectedBundleDir)
		{
		case 0:
			_baseUrl = "http://client.dev.uberstrike.com/";
			break;
		case 1:
			_baseUrl = "http://static.cmune.com/UberStrike/Qa/";
			break;
		case 2:
			_baseUrl = "http://static.cmune.com/UberStrike/";
			break;
		}
		StartCoroutine(StartGettingItemAssetBundleUrl(_baseUrl));
	}

	private void OnAssetbundleLoaded(AssetBundle bundle)
	{
		_loadingProgress = 0f;
		StartCoroutine(InstantiateItems(bundle));
	}

	private IEnumerator StartGettingItemAssetBundleUrl(string baseUrl)
	{
		_isReadingUrl = true;
		WWW www = new WWW(baseUrl + "Items/4.4.2/ItemAssetBundle.xml?arg=" + DateTime.Now.Ticks);
		yield return www;
		if (string.IsNullOrEmpty(www.error))
		{
			_bundlePath = _baseUrl + "Items/4.4.2/" + GetItemAssetBundleName(www.text);
		}
		_isReadingUrl = false;
	}

	private string GetItemAssetBundleName(string xml)
	{
		string result = string.Empty;
		XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
		while (xmlReader.Read())
		{
			if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name.Equals("Hash"))
			{
				result = xmlReader.ReadString() + string.Format("-SD.unity3d");
				break;
			}
		}
		return result;
	}

	private IEnumerator InstantiateItems(AssetBundle bundle)
	{
		AudioListener.volume = 0f;
		if ((bool)bundle)
		{
			UnityEngine.Object[] array = bundle.LoadAll(typeof(GameObject));
			for (int i = 0; i < array.Length; i++)
			{
				GameObject obj = (GameObject)array[i];
				_itemCollection.AddItem(obj);
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			Debug.LogError("AssetBundle is null!");
		}
		_itemsGrid = new ItemCollectionGrid(_itemCollection);
		_loadingProgress = 0f;
		_isLoadingBundle = false;
	}
}
