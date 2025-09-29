using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;

public abstract class LotteryShopItem
{
	public int Id { get; set; }

	public string Name { get; set; }

	public DynamicTexture Icon { get; set; }

	public ItemPrice Price { get; set; }

	public BundleCategoryType Category { get; set; }

	public abstract string Description { get; }

	public abstract string IconUrl { get; }

	public abstract void Use();
}
