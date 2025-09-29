using System.Collections.Generic;
using UnityEngine;

public class ItemCollection
{
	private Dictionary<string, GearItem> _gears;

	private Dictionary<string, HoloGearItem> _holos;

	private Dictionary<string, WeaponItem> _weapons;

	private Dictionary<string, QuickItem> _quickItems;

	public ICollection<GearItem> Gears
	{
		get
		{
			return _gears.Values;
		}
	}

	public ICollection<HoloGearItem> Holos
	{
		get
		{
			return _holos.Values;
		}
	}

	public ICollection<WeaponItem> Weapons
	{
		get
		{
			return _weapons.Values;
		}
	}

	public ICollection<QuickItem> QuickItems
	{
		get
		{
			return _quickItems.Values;
		}
	}

	public ItemCollection()
	{
		_gears = new Dictionary<string, GearItem>();
		_holos = new Dictionary<string, HoloGearItem>();
		_weapons = new Dictionary<string, WeaponItem>();
		_quickItems = new Dictionary<string, QuickItem>();
	}

	public void AddItem(GameObject item)
	{
		GearItem component;
		HoloGearItem component2;
		WeaponItem component3;
		QuickItem component4;
		if ((bool)(component = item.GetComponent<GearItem>()))
		{
			AddGear(component);
		}
		else if ((bool)(component2 = item.GetComponent<HoloGearItem>()))
		{
			AddHolo(component2);
		}
		else if ((bool)(component3 = item.GetComponent<WeaponItem>()))
		{
			AddWeapon(component3);
		}
		else if ((bool)(component4 = item.GetComponent<QuickItem>()))
		{
			AddQuickItem(component4);
		}
	}

	public int GetCount()
	{
		return _gears.Count + _holos.Count + _weapons.Count;
	}

	private void AddGear(GearItem gear)
	{
		if (_gears.ContainsKey(gear.name))
		{
			Debug.LogError("Duplicated gear: " + gear.name);
		}
		else
		{
			_gears.Add(gear.name, gear);
		}
	}

	private void AddHolo(HoloGearItem holo)
	{
		if (_holos.ContainsKey(holo.name))
		{
			Debug.LogError("Duplicated holo: " + holo.name);
		}
		else
		{
			_holos.Add(holo.name, holo);
		}
	}

	private void AddWeapon(WeaponItem weapon)
	{
		if (_weapons.ContainsKey(weapon.name))
		{
			Debug.LogError("Duplicated weapon: " + weapon.name);
		}
		else
		{
			_weapons.Add(weapon.name, weapon);
		}
	}

	private void AddQuickItem(QuickItem quickItem)
	{
		if (_quickItems.ContainsKey(quickItem.name))
		{
			Debug.LogError("Duplicated QuickItem: " + quickItem.name);
		}
		else
		{
			_quickItems.Add(quickItem.name, quickItem);
		}
	}
}
