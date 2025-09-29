using System;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public class DefaultItem : IUnityItem
{
	private Texture2D _icon;

	private GameObject _prefab;

	public bool Equippable
	{
		get
		{
			return true;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return true;
		}
	}

	public GameObject Prefab
	{
		get
		{
			return _prefab;
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

	public event Action<IUnityItem> OnPrefabLoaded;

	public DefaultItem(BaseUberStrikeItemView view)
	{
		View = view;
		switch (view.ItemType)
		{
		case UberstrikeItemType.Gear:
			_prefab = Singleton<ItemManager>.Instance.GetDefaultGearItem(view.ItemClass);
			break;
		case UberstrikeItemType.Weapon:
			_prefab = Singleton<ItemManager>.Instance.GetDefaultWeaponItem(view.ItemClass);
			break;
		}
		_icon = UnityItemConfiguration.Instance.GetDefaultTexture(view.ItemClass);
	}

	public DefaultItem(GameObject prefab)
	{
		_prefab = prefab;
	}

	public void Unload()
	{
	}

	public GameObject Create(Vector3 position, Quaternion rotation)
	{
		if ((bool)_prefab)
		{
			return UnityEngine.Object.Instantiate(_prefab.gameObject, position, rotation) as GameObject;
		}
		Debug.LogError("Failed to create default item: " + View.Name);
		return null;
	}

	public void DrawIcon(Rect position, bool forceAlpha = false)
	{
		if ((bool)_icon)
		{
			float a = ((!GUI.enabled) ? 0.5f : 1f);
			Color color = GUI.color;
			GUI.color = new Color(color.r, color.g, color.b, a);
			GUI.DrawTexture(position, _icon);
			GUI.color = color;
		}
	}
}
