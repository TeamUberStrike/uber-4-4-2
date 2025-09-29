using System;
using System.Collections;
using System.Collections.Generic;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public class MapManager : Singleton<MapManager>
{
	private const float MaxBundleLoadProgress = 0.9f;

	private const float MaxSceneLoadProgress = 0.1f;

	private Dictionary<string, UberstrikeMap> _mapsByName = new Dictionary<string, UberstrikeMap>();

	private Dictionary<int, int> _itemRecommendationsPerMap = new Dictionary<int, int>();

	private AssetBundle loadedMapBundle;

	public IEnumerable<UberstrikeMap> AllMaps
	{
		get
		{
			return _mapsByName.Values;
		}
	}

	public int Count
	{
		get
		{
			return _mapsByName.Count;
		}
	}

	public bool IsLoading { get; private set; }

	public float Progress { get; private set; }

	public bool IsSimulateWebplayer { get; private set; }

	public string SimulatedWebPlayerPath { get; private set; }

	private MapManager()
	{
		Clear();
	}

	public void SimulateWebplayer(string path)
	{
		IsSimulateWebplayer = true;
		SimulatedWebPlayerPath = path;
	}

	public IUnityItem GetRecommendedItem(string mapName)
	{
		int mapId;
		int value;
		if (TryGetMapId(mapName, out mapId) && _itemRecommendationsPerMap.TryGetValue(mapId, out value))
		{
			return Singleton<ItemManager>.Instance.GetItemInShop(value);
		}
		IUnityItem item;
		if (Singleton<ItemManager>.Instance.TryGetDefaultItem(UberstrikeItemClass.WeaponMachinegun, out item))
		{
			return item;
		}
		return null;
	}

	public string GetMapDescription(int mapId)
	{
		UberstrikeMap mapWithId = GetMapWithId(mapId);
		if (mapWithId != null)
		{
			return mapWithId.Description;
		}
		return LocalizedStrings.None;
	}

	public string GetMapName(string name)
	{
		UberstrikeMap value;
		if (_mapsByName.TryGetValue(name, out value))
		{
			return value.Name;
		}
		return LocalizedStrings.None;
	}

	public string GetMapName(int mapId)
	{
		UberstrikeMap mapWithId = GetMapWithId(mapId);
		if (mapWithId != null)
		{
			return mapWithId.Name;
		}
		return LocalizedStrings.None;
	}

	public string GetMapSceneName(int mapId)
	{
		UberstrikeMap mapWithId = GetMapWithId(mapId);
		if (mapWithId != null)
		{
			return mapWithId.SceneName;
		}
		return LocalizedStrings.None;
	}

	public UberstrikeMap GetMapWithId(int mapId)
	{
		foreach (UberstrikeMap value in _mapsByName.Values)
		{
			if (value.Id == mapId)
			{
				return value;
			}
		}
		return null;
	}

	public bool MapExistsWithId(int mapId)
	{
		return GetMapWithId(mapId) != null;
	}

	public bool IsBlueBox(int mapId)
	{
		UberstrikeMap mapWithId = GetMapWithId(mapId);
		if (mapWithId != null)
		{
			return mapWithId.IsBluebox;
		}
		return false;
	}

	public bool HasMapWithId(int mapId)
	{
		return GetMapWithId(mapId) != null;
	}

	public UberstrikeMap AddMapView(MapView mapView, bool isVisible = true, bool isBuiltIn = false)
	{
		UberstrikeMap uberstrikeMap = new UberstrikeMap(mapView);
		uberstrikeMap.IsVisible = isVisible;
		uberstrikeMap.IsBuiltIn = isBuiltIn;
		UberstrikeMap uberstrikeMap2 = uberstrikeMap;
		_mapsByName[mapView.SceneName] = uberstrikeMap2;
		_itemRecommendationsPerMap[mapView.MapId] = mapView.RecommendedItemId;
		return uberstrikeMap2;
	}

	private void Clear()
	{
		_mapsByName.Clear();
		AddMapView(new MapView
		{
			Description = "Menu",
			DisplayName = "Menu",
			SceneName = "Menu"
		}, false, true);
		AddMapView(new MapView
		{
			Description = "Tutorial",
			DisplayName = "Tutorial",
			SceneName = "Tutorial"
		}, false, true);
	}

	public bool InitializeMapsToLoad(List<MapView> mapViews)
	{
		Clear();
		foreach (MapView mapView in mapViews)
		{
			AddMapView(mapView);
		}
		return _mapsByName.Count > 0;
	}

	public void CancelLoadMap(UberstrikeMap map)
	{
		if (!string.IsNullOrEmpty(map.AssetbundleName) && Singleton<SceneLoader>.Instance.CurrentScene != map.SceneName)
		{
			AssetBundleLoader.CancelLoading(map.AssetbundleName);
		}
	}

	public Coroutine LoadMap(UberstrikeMap map, Action OnMapLoaded)
	{
		return MonoRoutine.Start(StartLoadingMap(map, OnMapLoaded));
	}

	public void UnloadMapBundle()
	{
		if (loadedMapBundle != null)
		{
			loadedMapBundle.Unload(false);
		}
	}

	private IEnumerator StartLoadingMap(UberstrikeMap map, Action OnMapLoaded)
	{
		if (IsLoading)
		{
			yield break;
		}
		IsLoading = true;
		float timeout = Time.time + 240f;
		string mapName = map.Name;
		ProgressPopupDialog dialog = PopupSystem.ShowProgress("Loading Map", "Loading " + mapName + "...", () => Singleton<MapManager>.Instance.Progress);
		bool loadError = false;
		if (map.IsBuiltIn)
		{
			AssetBundleLoader.SetStateToLoaded(map.AssetbundleName);
			Progress = 0.9f;
			Singleton<SceneLoader>.Instance.LoadLevel(map.SceneName, delegate
			{
				loadError = true;
			}, delegate(float p)
			{
				Progress = 0.9f + p * 0.1f;
			});
		}
		else
		{
			UberstrikeMap map2 = default(UberstrikeMap);
			dialog.SetCancelable(delegate
			{
				Singleton<MapManager>.Instance.CancelLoadMap(map2);
			});
			List<AssetBundleXmlReader.Config> configs = new List<AssetBundleXmlReader.Config>();
			string url = ApplicationDataManager.BaseMapsURL + "MapAssetBundle.xml?arg=" + DateTime.Now.Ticks;
			yield return MonoRoutine.Start(AssetBundleXmlReader.Read(url, configs));
			AssetBundleXmlReader.Config mapConfig = configs.Find((AssetBundleXmlReader.Config c) => c.Name.Equals(map2.SceneName));
			if (mapConfig != null)
			{
				map.SetVersion(mapConfig.Version);
				MonoRoutine.Start(LoadMapAssetbundle(map.AssetbundleName, map.SceneName, delegate
				{
					loadError = true;
				}));
			}
			else
			{
				loadError = true;
				PopupSystem.ShowMessage("Error", "The Map could not be loaded.\nPlease try again later");
			}
		}
		while (!loadError && GameState.CurrentSpace.SceneName != map.SceneName)
		{
			switch (AssetBundleLoader.State(map.AssetbundleName))
			{
			case AssetBundleLoader.LoadingState.Loaded:
				dialog.SetCancelable(null);
				break;
			case AssetBundleLoader.LoadingState.Cancelled:
			case AssetBundleLoader.LoadingState.Error:
				dialog.SetCancelable(null);
				IsLoading = false;
				yield break;
			}
			yield return new WaitForEndOfFrame();
		}
		PopupSystem.HideMessage(dialog);
		if (AssetBundleLoader.State(map.AssetbundleName) == AssetBundleLoader.LoadingState.Loaded)
		{
			if (GameState.CurrentSpace.SceneName == map.SceneName)
			{
				if (OnMapLoaded != null)
				{
					OnMapLoaded();
				}
			}
			else
			{
				Singleton<GameStateController>.Instance.LeaveGame();
				PopupSystem.ShowMessage("Error", "The Map '" + map.Name + "' coudn't be loaded.\nPlease try again later");
				Debug.LogError("StartLoadingMap canceled or couldn't be loaded, timeout: " + (Time.time > timeout));
			}
		}
		else
		{
			Debug.LogWarning("GameLoadeer cancelled by user " + map.SceneName + " " + AssetBundleLoader.State(map.SceneName));
		}
		IsLoading = false;
	}

	private IEnumerator LoadMapAssetbundle(string filename, string sceneName, Action<string> onError = null)
	{
		Progress = 0f;
		AssetBundle mapBundle = null;
		yield return AssetBundleLoader.LoadMapAssetBundle(filename, delegate(float p)
		{
			Progress = p * 0.9f;
		}, delegate(AssetBundle b)
		{
			mapBundle = b;
		}, onError);
		Progress = 0.9f;
		if (AssetBundleLoader.State(filename) == AssetBundleLoader.LoadingState.Loaded)
		{
			if (mapBundle != null)
			{
				yield return Singleton<SceneLoader>.Instance.LoadLevel(sceneName, onError, delegate(float p)
				{
					Progress = 0.9f + p * 0.1f;
				});
				loadedMapBundle = mapBundle;
			}
		}
		else if (mapBundle != null)
		{
			mapBundle.Unload(false);
		}
	}

	internal bool TryGetMapId(string mapName, out int mapId)
	{
		foreach (UberstrikeMap value in _mapsByName.Values)
		{
			if (value.SceneName == mapName)
			{
				mapId = value.Id;
				return true;
			}
		}
		mapId = 0;
		return false;
	}

	internal void SetRecomendations(Dictionary<int, int> recommendationsPerMap)
	{
		_itemRecommendationsPerMap = recommendationsPerMap;
	}
}
