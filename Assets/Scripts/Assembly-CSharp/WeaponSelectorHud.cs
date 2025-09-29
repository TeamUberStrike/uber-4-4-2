using System.Collections.Generic;
using UnityEngine;

public class WeaponSelectorHud
{
	private Dictionary<LoadoutSlotType, IUnityItem> _loadoutWeapons;

	private Dictionary<LoadoutSlotType, int> _weaponIndicesInList;

	private MeshGUIList _weaponList;

	private float _weaponListHideTime;

	private float _weaponListDisplayTime;

	private bool _isWeaponListFadingOut;

	public bool Enabled
	{
		get
		{
			return _weaponList.Enabled;
		}
		set
		{
			_weaponList.Enabled = value;
		}
	}

	public WeaponSelectorHud()
	{
		_loadoutWeapons = new Dictionary<LoadoutSlotType, IUnityItem>();
		_weaponIndicesInList = new Dictionary<LoadoutSlotType, int>();
		_weaponListDisplayTime = 3f;
		if (HudAssets.Exists)
		{
			_weaponList = new MeshGUIList(OnDrawWeaponList);
			_weaponList.Enabled = false;
		}
		_weaponList.Enabled = false;
	}

	public void Draw()
	{
		_weaponList.Draw();
	}

	public void Update()
	{
		_weaponList.Update();
	}

	public void SetSlotWeapon(LoadoutSlotType slot, IUnityItem weapon)
	{
		if (weapon != null)
		{
			_loadoutWeapons[slot] = weapon;
		}
		else
		{
			_loadoutWeapons.Remove(slot);
		}
		OnWeaponSlotsChange();
	}

	public IUnityItem GetLoadoutWeapon(LoadoutSlotType loadoutSlotType)
	{
		if (_loadoutWeapons.ContainsKey(loadoutSlotType))
		{
			return _loadoutWeapons[loadoutSlotType];
		}
		return null;
	}

	public void SetActiveWeaponLoadout(LoadoutSlotType loadoutSlotType)
	{
		if (_loadoutWeapons.ContainsKey(loadoutSlotType))
		{
			_weaponList.AnimToIndex(_weaponIndicesInList[loadoutSlotType], 0.1f);
			OnWeaponListTrigger();
		}
	}

	private void OnWeaponSlotsChange()
	{
		ResetWeaponListItems();
		OnWeaponListTrigger();
	}

	private void OnWeaponListTrigger()
	{
		_weaponListHideTime = Time.time + _weaponListDisplayTime;
		_isWeaponListFadingOut = false;
	}

	private void OnDrawWeaponList()
	{
		if (CanWeaponListFadeOut())
		{
			FadeOutWeaponList();
		}
	}

	private bool CanWeaponListFadeOut()
	{
		return Time.time > _weaponListHideTime && !_isWeaponListFadingOut;
	}

	private void FadeOutWeaponList()
	{
		_isWeaponListFadingOut = true;
		_weaponList.FadeOut(1f, EaseType.Out);
	}

	private void ResetWeaponListItems()
	{
		int num = 5;
		int num2 = 0;
		_weaponIndicesInList.Clear();
		_weaponList.ClearAllItems();
		for (int i = 0; i < num; i++)
		{
			LoadoutSlotType key = (LoadoutSlotType)(7 + i);
			if (_loadoutWeapons.ContainsKey(key))
			{
				string name = _loadoutWeapons[key].Name;
				_weaponIndicesInList.Add(key, num2++);
				_weaponList.AddItem(name);
			}
		}
	}
}
