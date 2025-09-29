using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class InGameItemGUI : BaseItemGUI
{
	private string _promotionalText = string.Empty;

	public InGameItemGUI(IUnityItem item, string promotionalText, BuyingLocationType location, BuyingRecommendationType recommendation)
		: base(item, location, recommendation)
	{
		_promotionalText = promotionalText;
	}

	public override void Draw(Rect rect, bool selected)
	{
		GUI.BeginGroup(rect);
		DrawIcon(new Rect(4f, (rect.height - 48f) / 2f, 48f, 48f));
		GUI.contentColor = ColorScheme.UberStrikeYellow;
		GUI.Label(new Rect(60f, rect.height / 2f - 18f, rect.width, 18f), _promotionalText, BlueStonez.label_interparkbold_16pt_left);
		GUI.contentColor = Color.white;
		GUI.Label(new Rect(60f, rect.height / 2f + 2f, rect.width, 16f), Item.Name, BlueStonez.label_interparkbold_16pt_left);
		InventoryItem item;
		if (Item.View != null && Singleton<InventoryManager>.Instance.TryGetInventoryItem(Item.View.ID, out item))
		{
			if (Singleton<LoadoutManager>.Instance.IsItemEquipped(Item.View.ID))
			{
				GUI.Label(new Rect(rect.width - 80f, rect.height / 2f - 25f, 80f, 22f), new GUIContent("EQUIPPED", ShopIcons.CheckMark), BlueStonez.label_interparkbold_11pt_left);
			}
			else
			{
				DrawEquipButton(new Rect(rect.width - 80f, rect.height / 2f - 25f, 80f, 22f), "EQUIP NOW");
			}
		}
		else
		{
			DrawBuyButton(new Rect(rect.width - 80f, rect.height / 2f - 25f, 80f, 22f), "BUY NOW");
		}
		GUI.EndGroup();
		if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
		{
			SelectShopItemEvent selectShopItemEvent = new SelectShopItemEvent();
			selectShopItemEvent.Item = Item;
			CmuneEventHandler.Route(selectShopItemEvent);
		}
	}
}
