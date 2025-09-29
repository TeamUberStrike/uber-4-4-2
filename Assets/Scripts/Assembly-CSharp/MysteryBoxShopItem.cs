using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;

public class MysteryBoxShopItem : LotteryShopItem
{
	public DynamicTexture Image { get; set; }

	public MysteryBoxUnityView View { get; private set; }

	public List<IUnityItem> Items { get; set; }

	public override string Description
	{
		get
		{
			return View.Description;
		}
	}

	public override string IconUrl
	{
		get
		{
			return View.IconUrl;
		}
	}

	public MysteryBoxShopItem(MysteryBoxUnityView view)
	{
		View = view;
	}

	public override void Use()
	{
		Singleton<LotteryManager>.Instance.RunMysteryBox(this);
	}
}
