using System;
using System.Collections.Generic;
using UberStrike.Core.Types;
using UnityEngine;

public class UnityItemConfiguration : MonoBehaviour
{
	[Serializable]
	public class FunctionalItemHolder
	{
		public string Name;

		public Texture2D Icon;

		public int ItemId;
	}

	public List<GearItem> UnityItemsDefaultGears;

	public List<WeaponItem> UnityItemsDefaultWeapons;

	public List<FunctionalItemHolder> UnityItemsFunctional;

	public List<Texture2D> DefaultWeaponIcons;

	public Texture2D StreamingItemTexture;

	public static UnityItemConfiguration Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	public bool Contains(string prefabName)
	{
		return (bool)UnityItemsDefaultGears.Find((GearItem item) => item.name.Equals(prefabName)) || (bool)UnityItemsDefaultWeapons.Find((WeaponItem item) => item.name.Equals(prefabName));
	}

	public Texture2D GetDefaultTexture(UberstrikeItemClass itemClass)
	{
		switch (itemClass)
		{
		case UberstrikeItemClass.WeaponCannon:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Cannon"));
		case UberstrikeItemClass.WeaponLauncher:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Launcher"));
		case UberstrikeItemClass.WeaponMachinegun:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Machine"));
		case UberstrikeItemClass.WeaponMelee:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Melee"));
		case UberstrikeItemClass.WeaponShotgun:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Shot"));
		case UberstrikeItemClass.WeaponSniperRifle:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Sniper"));
		case UberstrikeItemClass.WeaponSplattergun:
			return DefaultWeaponIcons.Find((Texture2D icon) => icon.name.Contains("Splatter"));
		default:
			return null;
		}
	}

	public Texture2D GetFunctionalItemIcon(int itemId)
	{
		FunctionalItemHolder functionalItemHolder = UnityItemsFunctional.Find((FunctionalItemHolder holder) => holder.ItemId == itemId);
		if (functionalItemHolder == null)
		{
			Debug.LogError("Failed to find icon for functional item with id: " + itemId);
			return null;
		}
		return functionalItemHolder.Icon;
	}
}
