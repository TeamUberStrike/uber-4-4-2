using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class LotteryShopGUI
{
	private int scrollHeight;

	private Vector2 scroll = Vector2.zero;

	private Dictionary<int, float> _alpha = new Dictionary<int, float>();

	public void Draw(Rect position)
	{
		List<string> list = new List<string>();
		float height = Mathf.Max(position.height, scrollHeight);
		scroll = GUITools.BeginScrollView(position, scroll, new Rect(0f, 0f, position.width - 17f, height), false, true);
		int num = 4;
		foreach (int value in Enum.GetValues(typeof(BundleCategoryType)))
		{
			if (value == 0)
			{
				continue;
			}
			int num2 = 0;
			List<LotteryShopItem> items;
			if (Singleton<LotteryManager>.Instance.TryGetBundle((BundleCategoryType)value, out items))
			{
				GUI.Label(new Rect(4f, num + 4, position.width - 20f, 20f), ((BundleCategoryType)value).ToString(), BlueStonez.label_interparkbold_18pt_left);
				num += 30;
				foreach (LotteryShopItem item in items)
				{
					int num3 = ((num2 % 2 == 1) ? 187 : 0);
					DrawLotterySlot(new Rect(num3, num, 188f, 95f), item);
					list.Add(item.IconUrl);
					num += ((num2 % 2 == 1) ? 94 : 0);
					num2++;
				}
			}
			if (num2 > 0)
			{
				if (num2 % 2 == 1)
				{
					num += 94;
				}
				GUI.Label(new Rect(4f, num, position.width - 8f, 1f), GUIContent.none, BlueStonez.horizontal_line_grey95);
				num += 4;
			}
		}
		scrollHeight = num;
		GUITools.EndScrollView();
		if (list.Count > 0 && !GuiLockController.IsLocked(GuiDepth.Page))
		{
			list.Reverse();
			AutoMonoBehaviour<TextureLoader>.Instance.SetFirstToLoadImages(list);
		}
	}

	private void DrawLotterySlot(Rect position, LotteryShopItem item)
	{
		bool flag = position.Contains(Event.current.mousePosition);
		if (!_alpha.ContainsKey(item.Id))
		{
			_alpha[item.Id] = 0f;
		}
		_alpha[item.Id] = Mathf.Lerp(_alpha[item.Id], flag ? 1 : 0, Time.deltaTime * (float)((!flag) ? 10 : 3));
		GUI.BeginGroup(position);
		GUI.color = new Color(1f, 1f, 1f, _alpha[item.Id]);
		if (GUI.Button(new Rect(2f, 2f, position.width - 4f, 79f), GUIContent.none, BlueStonez.gray_background))
		{
			UseLotteryItem(item);
		}
		GUI.color = Color.white;
		item.Icon.Draw(new Rect(4f, 4f, 75f, 75f));
		GUI.Label(new Rect(81f, 0f, position.width - 80f, 44f), item.Name, BlueStonez.label_interparkbold_13pt_left);
		if (GUI.Button(new Rect(81f, 51f, position.width - 110f, 20f), item.Price.PriceTag(false, item.Description), BlueStonez.buttongold_medium))
		{
			UseLotteryItem(item);
		}
		GUI.EndGroup();
	}

	private void UseLotteryItem(LotteryShopItem item)
	{
		item.Use();
	}
}
