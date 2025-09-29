using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UnityEngine;

public static class Conversions
{
	public static MysteryBoxShopItem ToUnityItem(this MysteryBoxUnityView mysteryBox)
	{
		List<IUnityItem> list = new List<IUnityItem>();
		if (mysteryBox.ExposeItemsToPlayers)
		{
			if (mysteryBox.PointsAttributed > 0)
			{
				list.Add(new PointsUnityItem(mysteryBox.PointsAttributed));
			}
			if (mysteryBox.CreditsAttributed > 0)
			{
				list.Add(new CreditsUnityItem(mysteryBox.CreditsAttributed));
			}
			foreach (BundleItemView mysteryBoxItem in mysteryBox.MysteryBoxItems)
			{
				list.Add(Singleton<ItemManager>.Instance.GetItemInShop(mysteryBoxItem.ItemId));
			}
		}
		MysteryBoxShopItem mysteryBoxShopItem = new MysteryBoxShopItem(mysteryBox);
		mysteryBoxShopItem.Name = mysteryBox.Name;
		mysteryBoxShopItem.Id = mysteryBox.Id;
		mysteryBoxShopItem.Price = new ItemPrice
		{
			Price = mysteryBox.Price,
			Currency = mysteryBox.UberStrikeCurrencyType
		};
		mysteryBoxShopItem.Icon = new DynamicTexture(mysteryBox.IconUrl);
		mysteryBoxShopItem.Image = new DynamicTexture(mysteryBox.ImageUrl);
		mysteryBoxShopItem.Items = list;
		mysteryBoxShopItem.Category = mysteryBox.Category;
		return mysteryBoxShopItem;
	}

	public static GUIContent PriceTag(this ItemPrice price, bool printCurrency = false, string tooltip = "")
	{
		switch (price.Currency)
		{
		case UberStrikeCurrencyType.Points:
			return new GUIContent(price.Price.ToString("N0") + ((!printCurrency) ? string.Empty : "Points"), ShopIcons.IconPoints20x20, tooltip);
		case UberStrikeCurrencyType.Credits:
			return new GUIContent(price.Price.ToString("N0") + ((!printCurrency) ? string.Empty : "Credits"), ShopIcons.IconCredits20x20, tooltip);
		default:
			return new GUIContent("N/A");
		}
	}

	public static LuckyDrawShopItem ToUnityItem(this LuckyDrawUnityView luckyDraw)
	{
		LuckyDrawShopItem luckyDrawShopItem = new LuckyDrawShopItem(luckyDraw);
		luckyDrawShopItem.Name = luckyDraw.Name;
		luckyDrawShopItem.Id = luckyDraw.Id;
		luckyDrawShopItem.Price = new ItemPrice
		{
			Price = luckyDraw.Price,
			Currency = luckyDraw.UberStrikeCurrencyType
		};
		luckyDrawShopItem.Icon = new DynamicTexture(luckyDraw.IconUrl);
		luckyDrawShopItem.Category = luckyDraw.Category;
		LuckyDrawShopItem luckyDrawShopItem2 = luckyDrawShopItem;
		List<LuckyDrawShopItem.LuckyDrawSet> list = new List<LuckyDrawShopItem.LuckyDrawSet>();
		foreach (LuckyDrawSetUnityView luckyDrawSet3 in luckyDraw.LuckyDrawSets)
		{
			LuckyDrawShopItem.LuckyDrawSet luckyDrawSet = new LuckyDrawShopItem.LuckyDrawSet();
			luckyDrawSet.Id = luckyDrawSet3.Id;
			luckyDrawSet.Items = new List<IUnityItem>();
			luckyDrawSet.Image = new DynamicTexture(luckyDrawSet3.ImageUrl);
			luckyDrawSet.View = luckyDrawSet3;
			luckyDrawSet.Parent = luckyDrawShopItem2;
			LuckyDrawShopItem.LuckyDrawSet luckyDrawSet2 = luckyDrawSet;
			if (luckyDrawSet3.ExposeItemsToPlayers)
			{
				if (luckyDrawSet3.PointsAttributed > 0)
				{
					luckyDrawSet2.Items.Add(new PointsUnityItem(luckyDrawSet3.PointsAttributed));
				}
				if (luckyDrawSet3.CreditsAttributed > 0)
				{
					luckyDrawSet2.Items.Add(new CreditsUnityItem(luckyDrawSet3.CreditsAttributed));
				}
				foreach (BundleItemView luckyDrawSetItem in luckyDrawSet3.LuckyDrawSetItems)
				{
					luckyDrawSet2.Items.Add(Singleton<ItemManager>.Instance.GetItemInShop(luckyDrawSetItem.ItemId));
				}
			}
			list.Add(luckyDrawSet2);
		}
		luckyDrawShopItem2.Sets = list;
		return luckyDrawShopItem2;
	}
}
