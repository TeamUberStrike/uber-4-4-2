using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public class ProxyItem : IUnityItem
{
	public const int CacheVersion = 3;

	private DynamicTexture _icon;

	private AssetBundle _assetBundle;

	private bool _loadingStarted;

	private string _version;

	private static readonly Shader _loadingShader = Shader.Find("LoadingShader");

	private static readonly Color _loadingColor = new Color32(135, 1, 145, byte.MaxValue);

	public bool Equippable
	{
		get
		{
			return true;
		}
	}

	public string Name
	{
		get
		{
			return View.Name;
		}
	}

	public BaseUberStrikeItemView View { get; private set; }

	public int CriticalStrikeBonus
	{
		get
		{
			if (View.ItemProperties.ContainsKey(ItemPropertyType.CritDamageBonus))
			{
				return View.ItemProperties[ItemPropertyType.CritDamageBonus];
			}
			return 0;
		}
	}

	public bool IsLoaded { get; private set; }

	public GameObject Prefab { get; private set; }

	public event Action<IUnityItem> OnPrefabLoaded = delegate
	{
	};

	public ProxyItem(BaseUberStrikeItemView view, string version)
	{
		try
		{
			string url = string.Format("{0}{1}-Icon.jpg", ApplicationDataManager.BaseItemsURL, view.PrefabName);
			_version = version;
			_icon = new DynamicTexture(url);
			View = view;
			Prefab = GetDefaultPrefab();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void UpdateProxyItem(BaseUberStrikeItemView view, string version)
	{
		if (_version != version)
		{
			Unload();
		}
		View = view;
	}

	public void Unload()
	{
		if (_assetBundle != null)
		{
			_assetBundle.Unload(true);
		}
		_assetBundle = null;
		_loadingStarted = false;
		Prefab = GetDefaultPrefab();
	}

	public GameObject Create(Vector3 position, Quaternion rotation)
	{
		if (!_loadingStarted)
		{
			LoadAssetBundle();
		}
		GameObject gameObject;
		if (IsLoaded && (bool)Prefab)
		{
			if (View.ItemClass == UberstrikeItemClass.GearHolo)
			{
				HoloGearItem component = Prefab.GetComponent<HoloGearItem>();
				gameObject = ((!component || !component.Configuration.Avatar) ? InstantiateDefaultPrefab() : (UnityEngine.Object.Instantiate(component.Configuration.Avatar.gameObject) as GameObject));
			}
			else
			{
				gameObject = UnityEngine.Object.Instantiate(Prefab, position, rotation) as GameObject;
			}
		}
		else
		{
			gameObject = InstantiateDefaultPrefab();
		}
		if ((bool)gameObject && View.ItemType == UberstrikeItemType.Weapon)
		{
			WeaponItem component2 = gameObject.GetComponent<WeaponItem>();
			if ((bool)component2)
			{
				ItemConfigurationUtil.CopyCustomProperties(View, component2.Configuration);
			}
			if (View.ItemProperties.ContainsKey(ItemPropertyType.CritDamageBonus))
			{
				component2.Configuration.CriticalStrikeBonus = View.ItemProperties[ItemPropertyType.CritDamageBonus];
			}
			else
			{
				component2.Configuration.CriticalStrikeBonus = 0;
			}
		}
		return gameObject;
	}

	public void DrawIcon(Rect position, bool forceAlpha = false)
	{
		_icon.Draw(position, forceAlpha);
	}

	public static void ApplyLoadingShader(GameObject obj)
	{
		if (!obj)
		{
			return;
		}
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			BaseWeaponEffect component = renderer.GetComponent<BaseWeaponEffect>();
			if (component == null || component is WeaponHeadAnimation)
			{
				renderer.material.shader = _loadingShader;
				renderer.material.color = _loadingColor;
				renderer.material.mainTexture = UnityItemConfiguration.Instance.StreamingItemTexture;
			}
		}
	}

	private GameObject InstantiateDefaultPrefab()
	{
		GameObject gameObject = null;
		if ((bool)Prefab)
		{
			gameObject = UnityEngine.Object.Instantiate(Prefab) as GameObject;
			ApplyLoadingShader(gameObject);
		}
		return gameObject;
	}

	private GameObject GetDefaultPrefab()
	{
		GameObject result = null;
		switch (View.ItemType)
		{
		case UberstrikeItemType.Gear:
			result = Singleton<ItemManager>.Instance.GetDefaultGearItem(View.ItemClass);
			break;
		case UberstrikeItemType.Weapon:
			result = Singleton<ItemManager>.Instance.GetDefaultWeaponItem(View.ItemClass);
			break;
		default:
			Debug.LogError("Unhandled item type: " + View.ItemType);
			break;
		case UberstrikeItemType.QuickUse:
			break;
		}
		return result;
	}

	private void LoadAssetBundle()
	{
		Singleton<ItemLoader>.Instance.AddTask(View.PrefabName, _version, delegate(AssetBundle bundle)
		{
			_assetBundle = bundle;
			MonoRoutine.Start(StartLoadingPrefabFromAssetBundle(_assetBundle));
		});
		_loadingStarted = true;
		Singleton<ItemLoader>.Instance.SetMustLoadItems(new List<string> { View.PrefabName });
	}

	private IEnumerator StartLoadingPrefabFromAssetBundle(AssetBundle bundle)
	{
		AssetBundleRequest request = bundle.LoadAsync(View.PrefabName, typeof(GameObject));
		yield return request;
		Prefab = request.asset as GameObject;
		if (View.ItemType == UberstrikeItemType.QuickUse)
		{
			QuickItem quickItem = Prefab.GetComponent<QuickItem>();
			if ((bool)quickItem && (bool)quickItem.Sfx)
			{
				Singleton<QuickItemSfxController>.Instance.RegisterQuickItemEffect(quickItem.Logic, quickItem.Sfx);
			}
		}
		IsLoaded = true;
		this.OnPrefabLoaded(this);
	}
}
