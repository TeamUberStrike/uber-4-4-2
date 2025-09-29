using System;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public class FunctionalItem : IUnityItem
{
	private Texture2D _icon;

	public bool Equippable
	{
		get
		{
			return false;
		}
	}

	public string Name
	{
		get
		{
			return View.Name;
		}
		set
		{
			View.Name = value;
		}
	}

	public UberstrikeItemClass ItemClass
	{
		get
		{
			return View.ItemClass;
		}
	}

	public string PrefabName
	{
		get
		{
			return string.Empty;
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
			return null;
		}
	}

	public BaseUberStrikeItemView View { get; private set; }

	public event Action<IUnityItem> OnPrefabLoaded;

	public FunctionalItem(BaseUberStrikeItemView view)
	{
		View = view;
		_icon = UnityItemConfiguration.Instance.GetFunctionalItemIcon(view.ID);
	}

	public void Unload()
	{
	}

	public GameObject Create(Vector3 position, Quaternion rotation)
	{
		return null;
	}

	public void DrawIcon(Rect position, bool forceAlpha = false)
	{
		GUI.DrawTexture(position, _icon);
	}
}
