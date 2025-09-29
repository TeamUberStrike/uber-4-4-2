using System;
using System.Collections.Generic;
using System.Text;
using UberStrike.Core.Types;
using UnityEngine;

public class Loadout : ILoadout
{
	private Dictionary<LoadoutSlotType, IUnityItem> _items = new Dictionary<LoadoutSlotType, IUnityItem>();

	public int Count
	{
		get
		{
			return _items.Count;
		}
	}

	public event Action OnGearChanged = delegate
	{
	};

	public event Action<LoadoutSlotType> OnWeaponChanged = delegate
	{
	};

	public Loadout(Loadout gearLoadout)
		: this(gearLoadout._items)
	{
	}

	public Loadout(Dictionary<LoadoutSlotType, IUnityItem> items)
	{
		foreach (KeyValuePair<LoadoutSlotType, IUnityItem> item in items)
		{
			SetSlot(item.Key, item.Value);
		}
	}

	private Loadout(Dictionary<LoadoutSlotType, int> gearItemIds)
	{
		LoadoutSlotType[] array = new LoadoutSlotType[12]
		{
			LoadoutSlotType.GearHolo,
			LoadoutSlotType.GearHead,
			LoadoutSlotType.GearFace,
			LoadoutSlotType.GearGloves,
			LoadoutSlotType.GearUpperBody,
			LoadoutSlotType.GearLowerBody,
			LoadoutSlotType.GearBoots,
			LoadoutSlotType.WeaponMelee,
			LoadoutSlotType.WeaponPrimary,
			LoadoutSlotType.WeaponSecondary,
			LoadoutSlotType.WeaponTertiary,
			LoadoutSlotType.WeaponPickup
		};
		LoadoutSlotType[] array2 = array;
		foreach (LoadoutSlotType loadoutSlotType in array2)
		{
			int value;
			if (gearItemIds.TryGetValue(loadoutSlotType, out value))
			{
				SetSlot(loadoutSlotType, Singleton<ItemManager>.Instance.GetItemInShop(value));
			}
		}
	}

	public static Loadout Create(List<int> gearItemIds, List<int> weaponItemIds)
	{
		if (gearItemIds.Count < 7 || weaponItemIds.Count < 5)
		{
			Debug.LogError("Invalid parameters: gear count = " + gearItemIds.Count + " weapon count = " + weaponItemIds.Count);
		}
		Dictionary<LoadoutSlotType, int> dictionary = new Dictionary<LoadoutSlotType, int>();
		int num = gearItemIds[0];
		int num2 = gearItemIds[1];
		int num3 = gearItemIds[2];
		int num4 = gearItemIds[3];
		int num5 = gearItemIds[4];
		int num6 = gearItemIds[5];
		int num7 = gearItemIds[6];
		if (num > 0)
		{
			dictionary.Add(LoadoutSlotType.GearHolo, num);
		}
		dictionary.Add(LoadoutSlotType.GearHead, num2);
		dictionary.Add(LoadoutSlotType.GearFace, num3);
		dictionary.Add(LoadoutSlotType.GearGloves, num4);
		dictionary.Add(LoadoutSlotType.GearUpperBody, num5);
		dictionary.Add(LoadoutSlotType.GearLowerBody, num6);
		dictionary.Add(LoadoutSlotType.GearBoots, num7);
		int value = weaponItemIds[0];
		int value2 = weaponItemIds[1];
		int value3 = weaponItemIds[2];
		int value4 = weaponItemIds[3];
		int value5 = weaponItemIds[4];
		dictionary.Add(LoadoutSlotType.WeaponMelee, value);
		dictionary.Add(LoadoutSlotType.WeaponPrimary, value2);
		dictionary.Add(LoadoutSlotType.WeaponSecondary, value3);
		dictionary.Add(LoadoutSlotType.WeaponTertiary, value4);
		dictionary.Add(LoadoutSlotType.WeaponPickup, value5);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Holo = " + num);
		stringBuilder.AppendLine("Head = " + num2);
		stringBuilder.AppendLine("Face = " + num3);
		stringBuilder.AppendLine("Gloves = " + num4);
		stringBuilder.AppendLine("Upper = " + num5);
		stringBuilder.AppendLine("Lower = " + num6);
		stringBuilder.AppendLine("Boots = " + num7);
		return new Loadout(dictionary);
	}

	public bool TryGetItem(LoadoutSlotType slot, out IUnityItem item)
	{
		return _items.TryGetValue(slot, out item);
	}

	public void SetSlot(LoadoutSlotType slot, IUnityItem item)
	{
		if (item == null || !CanGoInSlot(slot, item.View.ItemType))
		{
			return;
		}
		IUnityItem value;
		if (_items.TryGetValue(slot, out value))
		{
			if (value != null)
			{
				value.OnPrefabLoaded -= delegate(IUnityItem i)
				{
					OnItemPrefabUpdated(LoadoutSlotType.None, i);
				};
			}
			_items.Remove(slot);
		}
		_items.Add(slot, item);
		item.OnPrefabLoaded += delegate(IUnityItem i)
		{
			OnItemPrefabUpdated(slot, i);
		};
		this.OnGearChanged();
	}

	public bool CanGoInSlot(LoadoutSlotType slot, UberstrikeItemType type)
	{
		switch (type)
		{
		case UberstrikeItemType.Functional:
			return slot >= LoadoutSlotType.FunctionalItem1 && slot <= LoadoutSlotType.FunctionalItem3;
		case UberstrikeItemType.Gear:
			return slot >= LoadoutSlotType.GearHead && slot <= LoadoutSlotType.GearHolo;
		case UberstrikeItemType.QuickUse:
			return slot >= LoadoutSlotType.QuickUseItem1 && slot <= LoadoutSlotType.QuickUseItem3;
		case UberstrikeItemType.Weapon:
			return slot >= LoadoutSlotType.WeaponMelee && slot <= LoadoutSlotType.WeaponPickup;
		default:
			Debug.LogError("Item attempted to be equipped into a slot that isn't supported.");
			return false;
		}
	}

	public void ClearSlot(LoadoutSlotType slot)
	{
		IUnityItem value;
		if (!_items.TryGetValue(slot, out value))
		{
			return;
		}
		if (value != null)
		{
			value.OnPrefabLoaded -= delegate(IUnityItem i)
			{
				OnItemPrefabUpdated(LoadoutSlotType.None, i);
			};
		}
		_items.Remove(slot);
		this.OnGearChanged();
	}

	public void ClearAllSlots()
	{
		foreach (IUnityItem value in _items.Values)
		{
			if (value != null)
			{
				value.OnPrefabLoaded -= delegate(IUnityItem i)
				{
					OnItemPrefabUpdated(LoadoutSlotType.None, i);
				};
			}
		}
	}

	public bool Equals(Loadout loadout)
	{
		bool flag = Count == loadout.Count;
		if (flag)
		{
			foreach (KeyValuePair<LoadoutSlotType, IUnityItem> item in _items)
			{
				IUnityItem value;
				if (loadout._items.TryGetValue(item.Key, out value))
				{
					if (value != item.Value)
					{
						return false;
					}
					continue;
				}
				return false;
			}
		}
		return flag;
	}

	public bool Contains(string prefabName)
	{
		bool result = false;
		foreach (IUnityItem value in _items.Values)
		{
			if (value.View.PrefabName.Equals(prefabName))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool Contains(int itemId)
	{
		bool result = false;
		foreach (IUnityItem value in _items.Values)
		{
			if (value.View.ID == itemId)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<LoadoutSlotType, IUnityItem> item in _items)
		{
			stringBuilder.AppendLine(string.Format("{0}: {1}", item.Key, item.Value.Name));
		}
		return stringBuilder.ToString();
	}

	public Dictionary<LoadoutSlotType, IUnityItem>.Enumerator GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	private void OnItemPrefabUpdated(LoadoutSlotType slot, IUnityItem item)
	{
		IUnityItem value;
		if (_items.TryGetValue(slot, out value) && value == item)
		{
			switch (item.View.ItemType)
			{
			case UberstrikeItemType.Gear:
				CheckAllGear();
				break;
			case UberstrikeItemType.Weapon:
				this.OnWeaponChanged(slot);
				break;
			case UberstrikeItemType.QuickUse:
				break;
			case UberstrikeItemType.WeaponMod:
				break;
			}
		}
	}

	private void CheckAllGear()
	{
		bool flag = false;
		IUnityItem value;
		if (_items.TryGetValue(LoadoutSlotType.GearHolo, out value))
		{
			flag = value.IsLoaded;
		}
		else
		{
			bool flag2 = true;
			LoadoutSlotType[] gearSlots = LoadoutManager.GearSlots;
			foreach (LoadoutSlotType key in gearSlots)
			{
				if (_items.TryGetValue(key, out value))
				{
					flag2 &= value.IsLoaded;
				}
			}
			flag = flag2;
		}
		if (flag)
		{
			this.OnGearChanged();
		}
	}

	public AvatarGearParts GetAvatarGear()
	{
		bool flag = false;
		bool flag2 = false;
		AvatarGearParts avatarGearParts = new AvatarGearParts();
		IUnityItem value;
		if (_items.TryGetValue(LoadoutSlotType.GearHolo, out value))
		{
			flag = (flag2 = !value.IsLoaded);
			avatarGearParts.Base = value.Create(Vector3.zero, Quaternion.identity);
		}
		if (!avatarGearParts.Base)
		{
			flag = true;
			avatarGearParts.Base = UnityEngine.Object.Instantiate(PrefabManager.Instance.DefaultAvatar.gameObject) as GameObject;
		}
		if (flag)
		{
			LoadoutSlotType[] gearSlots = LoadoutManager.GearSlots;
			foreach (LoadoutSlotType loadoutSlotType in gearSlots)
			{
				if (_items.TryGetValue(loadoutSlotType, out value))
				{
					GameObject gameObject = value.Create(Vector3.zero, Quaternion.identity);
					if ((bool)gameObject)
					{
						avatarGearParts.Parts.Add(gameObject);
						if (flag2)
						{
							ProxyItem.ApplyLoadingShader(gameObject);
						}
					}
					continue;
				}
				GameObject defaultGearItem = Singleton<ItemManager>.Instance.GetDefaultGearItem(ItemUtil.ItemClassFromSlot(loadoutSlotType));
				if (!defaultGearItem)
				{
					continue;
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate(defaultGearItem) as GameObject;
				if ((bool)gameObject2)
				{
					avatarGearParts.Parts.Add(gameObject2);
					if (flag2)
					{
						ProxyItem.ApplyLoadingShader(gameObject2);
					}
				}
			}
		}
		return avatarGearParts;
	}

	public AvatarGearParts GetRagdollGear()
	{
		bool flag = false;
		AvatarGearParts avatarGearParts = new AvatarGearParts();
		try
		{
			IUnityItem value;
			if (_items.TryGetValue(LoadoutSlotType.GearHolo, out value))
			{
				flag = !value.IsLoaded;
				if ((bool)value.Prefab)
				{
					HoloGearItem component = value.Prefab.GetComponent<HoloGearItem>();
					if ((bool)component && (bool)component.Configuration.Ragdoll)
					{
						avatarGearParts.Base = UnityEngine.Object.Instantiate(component.Configuration.Ragdoll.gameObject) as GameObject;
					}
					else
					{
						avatarGearParts.Base = UnityEngine.Object.Instantiate(PrefabManager.Instance.DefaultRagdoll.gameObject) as GameObject;
					}
				}
				else
				{
					avatarGearParts.Base = UnityEngine.Object.Instantiate(PrefabManager.Instance.DefaultRagdoll.gameObject) as GameObject;
				}
			}
			else
			{
				flag = true;
				avatarGearParts.Base = UnityEngine.Object.Instantiate(PrefabManager.Instance.DefaultRagdoll.gameObject) as GameObject;
			}
			if (flag)
			{
				LoadoutSlotType[] gearSlots = LoadoutManager.GearSlots;
				foreach (LoadoutSlotType loadoutSlotType in gearSlots)
				{
					if (_items.TryGetValue(loadoutSlotType, out value))
					{
						GameObject gameObject = value.Create(Vector3.zero, Quaternion.identity);
						if ((bool)gameObject)
						{
							avatarGearParts.Parts.Add(gameObject);
						}
					}
					else if (Singleton<ItemManager>.Instance.TryGetDefaultItem(ItemUtil.ItemClassFromSlot(loadoutSlotType), out value))
					{
						GameObject gameObject2 = value.Create(Vector3.zero, Quaternion.identity);
						if ((bool)gameObject2)
						{
							avatarGearParts.Parts.Add(gameObject2);
						}
					}
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return avatarGearParts;
	}
}
