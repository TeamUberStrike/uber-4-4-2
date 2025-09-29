using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public static class RecommendationUtils
{
	public class WeaponRecommendation
	{
		public string Debug;

		public bool IsComplete { get; set; }

		public IUnityItem ItemWeapon { get; set; }

		public CombatRangeCategory CombatRange { get; set; }

		public LoadoutSlotType LoadoutSlot { get; set; }

		public ItemPrice Price { get; set; }
	}

	public static IUnityItem FallBackWeapon
	{
		get
		{
			return Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponMelee);
		}
	}

	public static WeaponRecommendation GetRecommendedWeapon(int playerLevel, CombatRangeTier mapCombatRange, List<IUnityItem> loadout = null, List<IUnityItem> inventory = null)
	{
		if (loadout == null)
		{
			loadout = new List<IUnityItem>(4);
			if (Singleton<LoadoutManager>.Instance.HasLoadoutItem(LoadoutSlotType.WeaponMelee))
			{
				loadout.Add(Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponMelee));
			}
			if (Singleton<LoadoutManager>.Instance.HasLoadoutItem(LoadoutSlotType.WeaponPrimary))
			{
				loadout.Add(Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponPrimary));
			}
			if (Singleton<LoadoutManager>.Instance.HasLoadoutItem(LoadoutSlotType.WeaponSecondary))
			{
				loadout.Add(Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponSecondary));
			}
			if (Singleton<LoadoutManager>.Instance.HasLoadoutItem(LoadoutSlotType.WeaponTertiary))
			{
				loadout.Add(Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponTertiary));
			}
		}
		if (inventory == null)
		{
			inventory = new List<IUnityItem>();
			foreach (InventoryItem allItem in Singleton<InventoryManager>.Instance.GetAllItems(false))
			{
				if (allItem.Item.View.ItemType == UberstrikeItemType.Weapon && (allItem.DaysRemaining > 0 || allItem.IsPermanent))
				{
					inventory.Add(allItem.Item);
				}
			}
		}
		WeaponRecommendation weaponRecommendation = CheckMyLoadout(mapCombatRange, loadout, inventory);
		weaponRecommendation.Debug += "[RC: ";
		if (weaponRecommendation.IsComplete)
		{
			int num = 0;
			foreach (IUnityItem item in GetWeakestItemsInLoadout(loadout, mapCombatRange))
			{
				KeyValuePair<IUnityItem, ItemPrice> nextBestAffordableWeapon = GetNextBestAffordableWeapon(item, playerLevel, inventory);
				if (nextBestAffordableWeapon.Key != null)
				{
					weaponRecommendation.ItemWeapon = nextBestAffordableWeapon.Key;
					weaponRecommendation.Price = nextBestAffordableWeapon.Value;
					weaponRecommendation.Debug += string.Format("Better in Class, try:{0}] ", num);
					return weaponRecommendation;
				}
				num++;
			}
			KeyValuePair<IUnityItem, ItemPrice> nextBestWeapon = GetNextBestWeapon(GetWeakestItemsInLoadout(loadout, mapCombatRange)[0], inventory);
			if (nextBestWeapon.Key != null)
			{
				weaponRecommendation.ItemWeapon = nextBestWeapon.Key;
				weaponRecommendation.Price = nextBestWeapon.Value;
				weaponRecommendation.Debug += "Better in Class | too exp] ";
				return weaponRecommendation;
			}
			weaponRecommendation.Debug += "NULL] ";
			return weaponRecommendation;
		}
		if (weaponRecommendation.ItemWeapon == null)
		{
			KeyValuePair<IUnityItem, ItemPrice> additionalWeapon = GetAdditionalWeapon(weaponRecommendation.CombatRange, playerLevel, inventory, loadout);
			weaponRecommendation.ItemWeapon = additionalWeapon.Key;
			weaponRecommendation.Price = additionalWeapon.Value;
			weaponRecommendation.Debug += "Add Weapon] ";
			return weaponRecommendation;
		}
		weaponRecommendation.Debug += "None] ";
		return weaponRecommendation;
	}

	public static List<CombatRangeCategory> GetCategoriesForCombatRange(CombatRangeTier mapCombatRange)
	{
		int num = mapCombatRange.CloseRange;
		int num2 = mapCombatRange.MediumRange;
		int longRange = mapCombatRange.LongRange;
		List<CombatRangeCategory> list = new List<CombatRangeCategory>(3);
		if (num > 0)
		{
			list.Add(CombatRangeCategory.Close);
			num--;
		}
		if (num2 > 0)
		{
			list.Add(CombatRangeCategory.Medium);
			num2--;
		}
		if (longRange > 0)
		{
			list.Add(CombatRangeCategory.Far);
			longRange = 0;
		}
		int num3 = 3 - list.Count;
		for (int i = 0; i < num3; i++)
		{
			if (num > num2)
			{
				list.Add(CombatRangeCategory.Close);
				num--;
			}
			else
			{
				list.Add(CombatRangeCategory.Medium);
				num2 = Mathf.Max(num2 - 1, 0);
			}
		}
		list.Sort((CombatRangeCategory range, CombatRangeCategory j) => mapCombatRange.GetTierForRange(j).CompareTo(mapCombatRange.GetTierForRange(range)));
		return list;
	}

	private static IUnityItem GetBestItemForRange(CombatRangeCategory range, IEnumerable<IUnityItem> items)
	{
		IUnityItem unityItem = null;
		int num = -1;
		foreach (IUnityItem item in items)
		{
			UberStrikeItemWeaponView uberStrikeItemWeaponView = item.View as UberStrikeItemWeaponView;
			if (((uint)uberStrikeItemWeaponView.CombatRange & (uint)range) != 0 && (unityItem == null || uberStrikeItemWeaponView.Tier > num))
			{
				unityItem = item;
				num = uberStrikeItemWeaponView.Tier;
			}
		}
		return unityItem;
	}

	public static LoadoutSlotType FindBestSlotToEquipWeapon(IUnityItem weapon, List<CombatRangeCategory> ranges)
	{
		if (weapon != null)
		{
			if (weapon.View.ItemClass == UberstrikeItemClass.WeaponMelee)
			{
				return LoadoutSlotType.WeaponMelee;
			}
			Dictionary<LoadoutSlotType, IUnityItem> dictionary = new Dictionary<LoadoutSlotType, IUnityItem>();
			dictionary.Add(LoadoutSlotType.WeaponPrimary, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponPrimary));
			dictionary.Add(LoadoutSlotType.WeaponSecondary, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponSecondary));
			dictionary.Add(LoadoutSlotType.WeaponTertiary, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponTertiary));
			Dictionary<LoadoutSlotType, IUnityItem> dictionary2 = dictionary;
			foreach (KeyValuePair<LoadoutSlotType, IUnityItem> item in dictionary2)
			{
				if (item.Value != null && item.Value.View.ItemClass == weapon.View.ItemClass)
				{
					return item.Key;
				}
			}
			KeyValuePair<LoadoutSlotType, IUnityItem> i;
			foreach (KeyValuePair<LoadoutSlotType, IUnityItem> item2 in dictionary2)
			{
				i = item2;
				if (i.Value == null)
				{
					return i.Key;
				}
				if (ranges.TrueForAll((CombatRangeCategory r) => ((uint)r & (uint)((UberStrikeItemWeaponView)i.Value.View).CombatRange) == 0))
				{
					return i.Key;
				}
			}
			KeyValuePair<LoadoutSlotType, IUnityItem> i2;
			foreach (KeyValuePair<LoadoutSlotType, IUnityItem> item3 in dictionary2)
			{
				i2 = item3;
				if (i2.Value != null)
				{
					CombatRangeCategory combatRangeCategory = ranges.Find((CombatRangeCategory r) => r == (CombatRangeCategory)((UberStrikeItemWeaponView)i2.Value.View).CombatRange);
					if (combatRangeCategory == (CombatRangeCategory)0)
					{
						return i2.Key;
					}
					ranges.Remove(combatRangeCategory);
				}
			}
		}
		return LoadoutSlotType.None;
	}

	private static WeaponRecommendation CheckMyLoadout(CombatRangeTier mapCombatRange, List<IUnityItem> loadout, List<IUnityItem> inventory)
	{
		loadout.RemoveAll((IUnityItem w) => w == null);
		WeaponRecommendation weaponRecommendation = new WeaponRecommendation();
		weaponRecommendation.Debug += "[LC: ";
		Dictionary<UberstrikeItemClass, IUnityItem> dictionary = new Dictionary<UberstrikeItemClass, IUnityItem>();
		foreach (IUnityItem item in inventory)
		{
			IUnityItem value;
			if (dictionary.TryGetValue(item.View.ItemClass, out value))
			{
				UberStrikeItemWeaponView uberStrikeItemWeaponView = value.View as UberStrikeItemWeaponView;
				UberStrikeItemWeaponView uberStrikeItemWeaponView2 = value.View as UberStrikeItemWeaponView;
				if (uberStrikeItemWeaponView.Tier < uberStrikeItemWeaponView2.Tier || (uberStrikeItemWeaponView.Tier == uberStrikeItemWeaponView2.Tier && uberStrikeItemWeaponView.DamagePerSecond < uberStrikeItemWeaponView2.DamagePerSecond))
				{
					dictionary[item.View.ItemClass] = item;
				}
			}
			else
			{
				dictionary[item.View.ItemClass] = item;
			}
		}
		HashSet<UberstrikeItemClass> equippedItemClasses = new HashSet<UberstrikeItemClass>();
		foreach (IUnityItem item2 in loadout)
		{
			equippedItemClasses.Add(item2.View.ItemClass);
		}
		if (!equippedItemClasses.Contains(UberstrikeItemClass.WeaponMelee))
		{
			weaponRecommendation.ItemWeapon = dictionary[UberstrikeItemClass.WeaponMelee];
			weaponRecommendation.CombatRange = CombatRangeCategory.Close;
			weaponRecommendation.LoadoutSlot = LoadoutSlotType.WeaponMelee;
			weaponRecommendation.Debug += " No Melee] ";
			return weaponRecommendation;
		}
		List<int> processedItemIds = new List<int>();
		IUnityItem unityItem = loadout.Find((IUnityItem w) => w.View.ItemClass == UberstrikeItemClass.WeaponMelee);
		if (unityItem != null)
		{
			processedItemIds.Add(unityItem.View.ID);
		}
		List<CombatRangeCategory> categoriesForCombatRange = GetCategoriesForCombatRange(mapCombatRange);
		foreach (CombatRangeCategory range in categoriesForCombatRange)
		{
			IUnityItem unityItem2 = loadout.Find((IUnityItem w) => ((uint)((UberStrikeItemWeaponView)w.View).CombatRange & (uint)range) == (uint)((UberStrikeItemWeaponView)w.View).CombatRange && !processedItemIds.Contains(w.View.ID));
			if (unityItem2 == null)
			{
				unityItem2 = loadout.Find((IUnityItem w) => ((uint)((UberStrikeItemWeaponView)w.View).CombatRange & (uint)range) != 0 && !processedItemIds.Contains(w.View.ID));
			}
			if (unityItem2 != null)
			{
				processedItemIds.Add(unityItem2.View.ID);
				continue;
			}
			List<IUnityItem> list = new List<IUnityItem>(dictionary.Values);
			list.RemoveAll((IUnityItem w) => equippedItemClasses.Contains(w.View.ItemClass));
			IUnityItem weapon = (weaponRecommendation.ItemWeapon = GetBestItemForRange(range, list));
			weaponRecommendation.CombatRange = range;
			weaponRecommendation.LoadoutSlot = FindBestSlotToEquipWeapon(weapon, categoriesForCombatRange);
			weaponRecommendation.Debug += " Uncovered Range] ";
			return weaponRecommendation;
		}
		foreach (IUnityItem item3 in loadout)
		{
			IUnityItem value2;
			if (dictionary.TryGetValue(item3.View.ItemClass, out value2) && ((UberStrikeItemWeaponView)value2.View).Tier > ((UberStrikeItemWeaponView)item3.View).Tier)
			{
				weaponRecommendation.ItemWeapon = value2;
				weaponRecommendation.CombatRange = (CombatRangeCategory)((UberStrikeItemWeaponView)value2.View).CombatRange;
				weaponRecommendation.LoadoutSlot = FindBestSlotToEquipWeapon(value2, categoriesForCombatRange);
				weaponRecommendation.Debug += " Better Inventory] ";
				return weaponRecommendation;
			}
		}
		weaponRecommendation.LoadoutSlot = LoadoutSlotType.None;
		weaponRecommendation.IsComplete = true;
		weaponRecommendation.Debug += " OK] ";
		return weaponRecommendation;
	}

	private static List<IUnityItem> GetWeakestItemsInLoadout(List<IUnityItem> loadout, CombatRangeTier combatRange)
	{
		List<IUnityItem> list = new List<IUnityItem>(loadout);
		list.Sort(delegate(IUnityItem i, IUnityItem j)
		{
			int num = ((UberStrikeItemWeaponView)i.View).Tier.CompareTo(((UberStrikeItemWeaponView)j.View).Tier);
			if (num == 0)
			{
				num = combatRange.GetTierForRange((CombatRangeCategory)((UberStrikeItemWeaponView)j.View).CombatRange).CompareTo(combatRange.GetTierForRange((CombatRangeCategory)((UberStrikeItemWeaponView)i.View).CombatRange));
			}
			return num;
		});
		return list;
	}

	private static KeyValuePair<IUnityItem, ItemPrice> GetAdditionalWeapon(CombatRangeCategory range, int playerLevel, List<IUnityItem> inventory, List<IUnityItem> loadout)
	{
		HashSet<UberstrikeItemClass> hashSet = new HashSet<UberstrikeItemClass>();
		foreach (IUnityItem item in loadout)
		{
			hashSet.Add(item.View.ItemClass);
		}
		playerLevel = Mathf.Max(2, playerLevel);
		List<KeyValuePair<IUnityItem, ItemPrice>> list = new List<KeyValuePair<IUnityItem, ItemPrice>>();
		IUnityItem w;
		foreach (IUnityItem shopItem in Singleton<ItemManager>.Instance.GetShopItems(UberstrikeItemType.Weapon, BuyingMarketType.Shop))
		{
			w = shopItem;
			UberStrikeItemWeaponView uberStrikeItemWeaponView = w.View as UberStrikeItemWeaponView;
			if (((uint)uberStrikeItemWeaponView.CombatRange & (uint)range) != 0 && inventory != null && !inventory.Exists((IUnityItem wp) => wp.View.ID == w.View.ID) && !hashSet.Contains(w.View.ItemClass))
			{
				ItemPrice lowestPrice = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Credits);
				ItemPrice lowestPrice2 = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Points);
				if (lowestPrice2 != null && lowestPrice2.Price > 0 && lowestPrice2.Price <= PlayerDataManager.Points && w.View.LevelLock <= playerLevel)
				{
					list.Add(new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice2));
				}
				else if (lowestPrice != null && lowestPrice.Price > 0 && lowestPrice.Price <= PlayerDataManager.Credits)
				{
					list.Add(new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice));
				}
			}
		}
		if (list.Count > 0)
		{
			list.Sort(new ShopUtils.PriceComparer<IUnityItem>());
			return list[0];
		}
		return GetNextBestWeapon(range, inventory);
	}

	private static KeyValuePair<IUnityItem, ItemPrice> GetNextBestAffordableWeapon(IUnityItem weakestLink, int playerLevel, List<IUnityItem> inventory)
	{
		playerLevel = Mathf.Max(2, playerLevel);
		List<KeyValuePair<IUnityItem, ItemPrice>> list = new List<KeyValuePair<IUnityItem, ItemPrice>>();
		IUnityItem w;
		foreach (IUnityItem shopItem in Singleton<ItemManager>.Instance.GetShopItems(UberstrikeItemType.Weapon, BuyingMarketType.Shop))
		{
			w = shopItem;
			UberStrikeItemWeaponView uberStrikeItemWeaponView = w.View as UberStrikeItemWeaponView;
			if (w.View.ItemClass == weakestLink.View.ItemClass && !inventory.Exists((IUnityItem wp) => wp.View.ID == w.View.ID) && (uberStrikeItemWeaponView.Tier > ((UberStrikeItemWeaponView)weakestLink.View).Tier || (uberStrikeItemWeaponView.Tier == ((UberStrikeItemWeaponView)weakestLink.View).Tier && uberStrikeItemWeaponView.DamagePerSecond > ((UberStrikeItemWeaponView)weakestLink.View).DamagePerSecond)))
			{
				ItemPrice lowestPrice = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Credits);
				ItemPrice lowestPrice2 = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Points);
				if (lowestPrice2 != null && lowestPrice2.Price > 0 && lowestPrice2.Price <= PlayerDataManager.Points && w.View.LevelLock <= playerLevel)
				{
					list.Add(new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice2));
				}
				else if (lowestPrice != null && lowestPrice.Price > 0 && lowestPrice.Price <= PlayerDataManager.Credits)
				{
					list.Add(new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice));
				}
			}
		}
		if (list.Count > 0)
		{
			list.Sort(new ShopUtils.PriceComparer<IUnityItem>());
			return list[0];
		}
		return default(KeyValuePair<IUnityItem, ItemPrice>);
	}

	private static KeyValuePair<IUnityItem, ItemPrice> GetNextBestWeapon(IUnityItem weakestLink, List<IUnityItem> inventory)
	{
		KeyValuePair<IUnityItem, ItemPrice> result = default(KeyValuePair<IUnityItem, ItemPrice>);
		IUnityItem w;
		foreach (IUnityItem shopItem in Singleton<ItemManager>.Instance.GetShopItems(UberstrikeItemType.Weapon, BuyingMarketType.Shop))
		{
			w = shopItem;
			UberStrikeItemWeaponView uberStrikeItemWeaponView = w.View as UberStrikeItemWeaponView;
			if (w.View.ItemClass == weakestLink.View.ItemClass && !inventory.Exists((IUnityItem wp) => wp.View.ID == w.View.ID) && (uberStrikeItemWeaponView.Tier > ((UberStrikeItemWeaponView)weakestLink.View).Tier || (uberStrikeItemWeaponView.Tier == ((UberStrikeItemWeaponView)weakestLink.View).Tier && uberStrikeItemWeaponView.DamagePerSecond > ((UberStrikeItemWeaponView)weakestLink.View).DamagePerSecond)))
			{
				ItemPrice lowestPrice = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Credits);
				ItemPrice lowestPrice2 = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Points);
				if (lowestPrice2 != null && lowestPrice2.Price > 0 && (result.Key == null || ((UberStrikeItemWeaponView)result.Key.View).DamagePerSecond > uberStrikeItemWeaponView.DamagePerSecond))
				{
					result = new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice2);
				}
				else if (lowestPrice != null && lowestPrice.Price > 0 && (result.Key == null || ((UberStrikeItemWeaponView)result.Key.View).DamagePerSecond > uberStrikeItemWeaponView.DamagePerSecond))
				{
					result = new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice);
				}
			}
		}
		return result;
	}

	private static KeyValuePair<IUnityItem, ItemPrice> GetNextBestWeapon(CombatRangeCategory range, List<IUnityItem> inventory)
	{
		KeyValuePair<IUnityItem, ItemPrice> result = default(KeyValuePair<IUnityItem, ItemPrice>);
		IUnityItem w;
		foreach (IUnityItem shopItem in Singleton<ItemManager>.Instance.GetShopItems(UberstrikeItemType.Weapon, BuyingMarketType.Shop))
		{
			w = shopItem;
			UberStrikeItemWeaponView uberStrikeItemWeaponView = w.View as UberStrikeItemWeaponView;
			if (((uint)uberStrikeItemWeaponView.CombatRange & (uint)range) != 0 && inventory != null && !inventory.Exists((IUnityItem wp) => wp.View.ID == w.View.ID))
			{
				ItemPrice lowestPrice = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Credits);
				ItemPrice lowestPrice2 = ShopUtils.GetLowestPrice(w, UberStrikeCurrencyType.Points);
				if (lowestPrice2 != null && lowestPrice2.Price > 0 && (result.Key == null || ((UberStrikeItemWeaponView)result.Key.View).DamagePerSecond > uberStrikeItemWeaponView.DamagePerSecond))
				{
					result = new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice2);
				}
				else if (lowestPrice != null && lowestPrice.Price > 0 && (result.Key == null || ((UberStrikeItemWeaponView)result.Key.View).DamagePerSecond > uberStrikeItemWeaponView.DamagePerSecond))
				{
					result = new KeyValuePair<IUnityItem, ItemPrice>(w, lowestPrice);
				}
			}
		}
		return result;
	}
}
