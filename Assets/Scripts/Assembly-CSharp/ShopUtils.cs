using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UnityEngine;

public static class ShopUtils
{
	public class PriceComparer<T> : IComparer<KeyValuePair<T, ItemPrice>>
	{
		public int Compare(KeyValuePair<T, ItemPrice> x, KeyValuePair<T, ItemPrice> y)
		{
			int value = x.Value.Price + ((x.Value.Currency == UberStrikeCurrencyType.Credits) ? 1000000 : 0);
			return (y.Value.Price + ((y.Value.Currency == UberStrikeCurrencyType.Credits) ? 1000000 : 0)).CompareTo(value);
		}
	}

	private class DescendedComparer : IComparer<int>
	{
		public int Compare(int x, int y)
		{
			return y - x;
		}
	}

	public static ItemPrice GetLowestPrice(IUnityItem item, UberStrikeCurrencyType currency = UberStrikeCurrencyType.None)
	{
		ItemPrice itemPrice = null;
		if (item != null && item.View != null && item.View.Prices != null)
		{
			foreach (ItemPrice price in item.View.Prices)
			{
				if ((currency == UberStrikeCurrencyType.None || price.Currency == currency) && (itemPrice == null || itemPrice.Price > price.Price))
				{
					itemPrice = price;
				}
			}
		}
		return itemPrice;
	}

	public static string PrintDuration(BuyingDurationType duration)
	{
		switch (duration)
		{
		case BuyingDurationType.Permanent:
			return " " + LocalizedStrings.Permanent;
		case BuyingDurationType.OneDay:
			return " 1 " + LocalizedStrings.Day;
		case BuyingDurationType.SevenDays:
			return " 1 " + LocalizedStrings.Week;
		case BuyingDurationType.ThirtyDays:
			return " 1 " + LocalizedStrings.Month;
		case BuyingDurationType.NinetyDays:
			return " " + LocalizedStrings.ThreeMonths;
		default:
			return string.Empty;
		}
	}

	public static Texture2D CurrencyIcon(UberStrikeCurrencyType currency)
	{
		switch (currency)
		{
		case UberStrikeCurrencyType.Credits:
			return ShopIcons.IconCredits20x20;
		case UberStrikeCurrencyType.Points:
			return ShopIcons.IconPoints20x20;
		default:
			return null;
		}
	}

	public static IUnityItem GetRecommendedArmor(int playerLevel, IUnityItem holo, IUnityItem upper, IUnityItem lower)
	{
		int num = ((holo != null) ? ((UberStrikeItemGearView)holo.View).ArmorPoints : 0);
		int num2 = ((upper != null) ? ((UberStrikeItemGearView)upper.View).ArmorPoints : 0);
		int num3 = ((lower != null) ? ((UberStrikeItemGearView)lower.View).ArmorPoints : 0);
		playerLevel = Mathf.Max(2, playerLevel);
		List<KeyValuePair<IUnityItem, ItemPrice>> list = new List<KeyValuePair<IUnityItem, ItemPrice>>();
		KeyValuePair<IUnityItem, ItemPrice> keyValuePair = new KeyValuePair<IUnityItem, ItemPrice>(null, new ItemPrice
		{
			Price = int.MaxValue
		});
		foreach (IUnityItem shopItem in Singleton<ItemManager>.Instance.GetShopItems(UberstrikeItemType.Gear, BuyingMarketType.Shop))
		{
			if (shopItem == null || shopItem.View.ItemType != UberstrikeItemType.Gear)
			{
				continue;
			}
			UberStrikeItemGearView uberStrikeItemGearView = shopItem.View as UberStrikeItemGearView;
			if (!shopItem.View.IsForSale)
			{
				continue;
			}
			bool flag = (shopItem.View.ItemClass == UberstrikeItemClass.GearHolo && uberStrikeItemGearView.ArmorPoints >= num && shopItem != holo) || (shopItem.View.ItemClass == UberstrikeItemClass.GearUpperBody && uberStrikeItemGearView.ArmorPoints >= num2 && shopItem != upper) || (shopItem.View.ItemClass == UberstrikeItemClass.GearLowerBody && uberStrikeItemGearView.ArmorPoints >= num3 && shopItem != lower);
			if (uberStrikeItemGearView.ArmorPoints <= 0 || !flag)
			{
				continue;
			}
			ItemPrice lowestPrice = GetLowestPrice(shopItem);
			if (shopItem.View.LevelLock <= playerLevel && keyValuePair.Value.Price > lowestPrice.Price)
			{
				keyValuePair = new KeyValuePair<IUnityItem, ItemPrice>(shopItem, lowestPrice);
			}
			if (shopItem.View.LevelLock <= playerLevel && lowestPrice.Currency == UberStrikeCurrencyType.Points && lowestPrice.Price <= PlayerDataManager.Points)
			{
				if (flag)
				{
					list.Add(new KeyValuePair<IUnityItem, ItemPrice>(shopItem, lowestPrice));
				}
			}
			else if (lowestPrice.Currency == UberStrikeCurrencyType.Credits && lowestPrice.Price <= PlayerDataManager.Credits && flag)
			{
				list.Add(new KeyValuePair<IUnityItem, ItemPrice>(shopItem, lowestPrice));
			}
		}
		if (list.Count == 0)
		{
			if (keyValuePair.Key != null)
			{
				return keyValuePair.Key;
			}
			if (Singleton<LoadoutManager>.Instance.HasLoadoutItem(LoadoutSlotType.GearUpperBody))
			{
				return Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearUpperBody);
			}
			return Singleton<ItemManager>.Instance.GetItemInShop(1087);
		}
		try
		{
			list.Sort(new PriceComparer<IUnityItem>());
		}
		catch
		{
			throw;
		}
		return list[0].Key;
	}

	public static bool IsMeleeWeapon(IUnityItem view)
	{
		if (view != null && view.View != null)
		{
			return view.View.ItemClass == UberstrikeItemClass.WeaponMelee;
		}
		return false;
	}

	public static bool IsInstantHitWeapon(IUnityItem view)
	{
		if (view != null && view.View != null)
		{
			return view.View.ItemClass == UberstrikeItemClass.WeaponMachinegun || view.View.ItemClass == UberstrikeItemClass.WeaponShotgun || view.View.ItemClass == UberstrikeItemClass.WeaponSniperRifle;
		}
		return false;
	}

	public static bool IsProjectileWeapon(IUnityItem view)
	{
		if (view != null && view.View != null)
		{
			return view.View.ItemClass == UberstrikeItemClass.WeaponCannon || view.View.ItemClass == UberstrikeItemClass.WeaponLauncher || view.View.ItemClass == UberstrikeItemClass.WeaponSplattergun;
		}
		return false;
	}

	public static string GetRecommendationString(RecommendType recommendation)
	{
		switch (recommendation)
		{
		case RecommendType.MostEfficient:
			return LocalizedStrings.MostEfficientWeaponCaps;
		case RecommendType.RecommendedArmor:
			return LocalizedStrings.RecommendedArmorCaps;
		case RecommendType.StaffPick:
			return LocalizedStrings.StaffPickCaps;
		case RecommendType.RecommendedWeapon:
			return LocalizedStrings.RecommendedWeaponCaps;
		default:
			return string.Empty;
		}
	}
}
