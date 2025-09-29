using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;

public class TemporaryLoadoutManager : Singleton<TemporaryLoadoutManager>
{
	private Avatar _avatar;

	public bool IsGearLoadoutModified
	{
		get
		{
			return !_avatar.Loadout.Equals(Singleton<LoadoutManager>.Instance.GearLoadout);
		}
	}

	private TemporaryLoadoutManager()
	{
		_avatar = GameState.LocalAvatar;
	}

	public void SetGearLoadout(LoadoutSlotType slot, IUnityItem item)
	{
		if (item != null)
		{
			IUnityItem item2;
			if (_avatar.Loadout.TryGetItem(slot, out item2) && item2 != item && !Singleton<InventoryManager>.Instance.Contains(item2.View.ID))
			{
				item2.Unload();
			}
			_avatar.Loadout.SetSlot(slot, item);
		}
	}

	public bool IsGearLoadoutModifiedOnSlot(LoadoutSlotType slot)
	{
		IUnityItem item;
		return _avatar.Loadout.TryGetItem(slot, out item) && item != Singleton<LoadoutManager>.Instance.GetItemOnSlot(slot);
	}

	public void ResetGearLoadout(LoadoutSlotType slot)
	{
		_avatar.Loadout.ClearSlot(slot);
	}

	public void ResetGearLoadout()
	{
		if (_avatar.Loadout != null && !_avatar.Loadout.Equals(Singleton<LoadoutManager>.Instance.GearLoadout))
		{
			_avatar.Loadout.ClearAllSlots();
		}
		_avatar.Loadout = new Loadout(Singleton<LoadoutManager>.Instance.GearLoadout);
		if (!GameState.HasCurrentGame)
		{
			Singleton<ItemLoader>.Instance.UnloadAll();
		}
	}

	public void TryGear(IUnityItem item)
	{
		if (item.View.ItemType == UberstrikeItemType.Gear)
		{
			if (item.View.ItemClass == UberstrikeItemClass.GearHolo)
			{
				SetGearLoadout(LoadoutSlotType.GearHolo, item);
			}
			else
			{
				SetGearLoadout(InventoryManager.GetSlotTypeForGear(item), item);
			}
			AutoMonoBehaviour<AvatarAnimationManager>.Instance.SetAnimationState(PageType.Shop, item.View.ItemClass);
			switch (item.View.ItemType)
			{
			case UberstrikeItemType.Gear:
			{
				SelectLoadoutAreaEvent selectLoadoutAreaEvent = new SelectLoadoutAreaEvent();
				selectLoadoutAreaEvent.Area = LoadoutArea.Gear;
				CmuneEventHandler.Route(selectLoadoutAreaEvent);
				break;
			}
			case UberstrikeItemType.Weapon:
			{
				SelectLoadoutAreaEvent selectLoadoutAreaEvent = new SelectLoadoutAreaEvent();
				selectLoadoutAreaEvent.Area = LoadoutArea.Weapons;
				CmuneEventHandler.Route(selectLoadoutAreaEvent);
				break;
			}
			case UberstrikeItemType.QuickUse:
			{
				SelectLoadoutAreaEvent selectLoadoutAreaEvent = new SelectLoadoutAreaEvent();
				selectLoadoutAreaEvent.Area = LoadoutArea.QuickItems;
				CmuneEventHandler.Route(selectLoadoutAreaEvent);
				break;
			}
			case UberstrikeItemType.WeaponMod:
				break;
			}
		}
	}
}
