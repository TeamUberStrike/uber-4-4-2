using System.Collections.Generic;
using UnityEngine;

public class MysteryBoxWinningPopup : LotteryWinningPopup
{
	private ShopItemGrid _grid;

	public MysteryBoxWinningPopup(DynamicTexture image, MysteryBoxShopItem item, List<bool> highlight)
		: base(image, item)
	{
		_grid = new ShopItemGrid(item.View.MysteryBoxItems, item.View.CreditsAttributed, item.View.PointsAttributed);
		_grid.HighlightState = highlight;
		Text = "You find your winnings in the inventory!";
	}

	protected override void DrawItemGrid(Rect rect, bool showItems)
	{
		_grid.Show = showItems;
		_grid.Draw(rect);
	}
}
