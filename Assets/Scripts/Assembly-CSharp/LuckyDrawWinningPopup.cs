using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class LuckyDrawWinningPopup : LotteryWinningPopup
{
	private ShopItemGrid _grid;

	public LuckyDrawWinningPopup(string text, DynamicTexture image, LotteryShopItem item, LuckyDrawSetUnityView luckyDrawSet)
		: base(image, item)
	{
		_grid = new ShopItemGrid(luckyDrawSet.LuckyDrawSetItems, luckyDrawSet.CreditsAttributed, luckyDrawSet.PointsAttributed);
		Text = LocalizedStrings.LuckyDrawWinningsInInventory;
	}

	protected override void DrawItemGrid(Rect rect, bool showItems)
	{
		_grid.Show = showItems;
		_grid.Draw(rect);
	}
}
