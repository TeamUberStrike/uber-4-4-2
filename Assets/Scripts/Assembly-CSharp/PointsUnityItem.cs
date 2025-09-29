using System;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public class PointsUnityItem : IUnityItem
{
	private class DummyItemView : BaseUberStrikeItemView
	{
		public override UberstrikeItemType ItemType
		{
			get
			{
				return UberstrikeItemType.Special;
			}
		}
	}

	public bool Equippable
	{
		get
		{
			return false;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return true;
		}
	}

	public string Name { get; private set; }

	public BaseUberStrikeItemView View { get; private set; }

	public GameObject Prefab
	{
		get
		{
			return null;
		}
	}

	public event Action<IUnityItem> OnPrefabLoaded;

	public PointsUnityItem(int points)
	{
		Name = points.ToString("N0") + " Points";
		View = new DummyItemView
		{
			Description = string.Format("An extra {0:N0} Points to fatten up your UberWallet!", points)
		};
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
		GUI.DrawTexture(position, ShopIcons.Points48x48);
	}
}
