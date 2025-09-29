using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

public class LotteryManager : Singleton<LotteryManager>
{
	public const int IMG_WIDTH = 282;

	public const int IMG_HEIGHT = 317;

	private Dictionary<BundleCategoryType, List<LotteryShopItem>> _lotteryItems;

	private List<LuckyDrawShopItem> _luckyDrawItems;

	private LotteryManager()
	{
		_luckyDrawItems = new List<LuckyDrawShopItem>();
		_lotteryItems = new Dictionary<BundleCategoryType, List<LotteryShopItem>>();
	}

	private IEnumerable<LotteryShopItem> GetAllItemsOfType(Type type)
	{
		foreach (List<LotteryShopItem> list in _lotteryItems.Values)
		{
			foreach (LotteryShopItem item in list)
			{
				if (item.GetType() == type)
				{
					yield return item;
				}
			}
		}
	}

	public void RefreshLotteryItems()
	{
		GetAllLuckyDraws();
		GetAllMysteryBoxes();
	}

	public bool TryGetBundle(BundleCategoryType bundle, out List<LotteryShopItem> items)
	{
		return _lotteryItems.TryGetValue(bundle, out items);
	}

	public void ShowNextItem(LotteryShopItem currentItem)
	{
		List<LotteryShopItem> list = new List<LotteryShopItem>(GetAllItemsOfType(currentItem.GetType()));
		if (list.Count > 0)
		{
			int num = list.FindIndex((LotteryShopItem i) => i == currentItem);
			if (num < 0)
			{
				list[UnityEngine.Random.Range(0, list.Count)].Use();
				return;
			}
			int index = (num + 1) % list.Count;
			list[index].Use();
		}
	}

	public void ShowPreviousItem(LotteryShopItem currentItem)
	{
		List<LotteryShopItem> list = new List<LotteryShopItem>(GetAllItemsOfType(currentItem.GetType()));
		if (list.Count > 0)
		{
			int num = list.FindIndex((LotteryShopItem i) => i == currentItem);
			if (num < 0)
			{
				list[UnityEngine.Random.Range(0, list.Count)].Use();
				return;
			}
			int index = (num - 1 + list.Count) % list.Count;
			list[index].Use();
		}
	}

	public LuckyDrawPopup RunLuckyDraw(LuckyDrawShopItem item)
	{
		LuckyDrawPopup luckyDrawPopup = new LuckyDrawPopup(item);
		StartTask(luckyDrawPopup);
		return luckyDrawPopup;
	}

	public void RunMysteryBox(MysteryBoxShopItem item)
	{
		StartTask(new MysteryBoxPopup(item));
	}

	private void StartTask(LotteryPopupDialog dialog)
	{
		new LotteryPopupTask(dialog);
		PopupSystem.Show(dialog);
	}

	private void GetAllMysteryBoxes()
	{
		ShopWebServiceClient.GetAllMysteryBoxs(delegate(List<MysteryBoxUnityView> list)
		{
			foreach (MysteryBoxUnityView item2 in list)
			{
				List<LotteryShopItem> value;
				if (!_lotteryItems.TryGetValue(item2.Category, out value))
				{
					value = new List<LotteryShopItem>();
					_lotteryItems[item2.Category] = value;
				}
				MysteryBoxShopItem item = item2.ToUnityItem();
				value.Add(item);
			}
		}, delegate(Exception ex)
		{
			Debug.LogError("MysteryBoxManager failed with: " + ex.Message);
		});
	}

	private void GetAllLuckyDraws()
	{
		ShopWebServiceClient.GetAllLuckyDraws(delegate(List<LuckyDrawUnityView> list)
		{
			foreach (LuckyDrawUnityView item2 in list)
			{
				List<LotteryShopItem> value;
				if (!_lotteryItems.TryGetValue(item2.Category, out value))
				{
					value = new List<LotteryShopItem>();
					_lotteryItems[item2.Category] = value;
				}
				LuckyDrawShopItem item = item2.ToUnityItem();
				value.Add(item);
				_luckyDrawItems.Add(item);
			}
		}, delegate(Exception ex)
		{
			Debug.LogError("MysteryBoxManager failed with: " + ex.Message);
		});
	}
}
