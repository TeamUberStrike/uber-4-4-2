using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;

public class LuckyDrawShopItem : LotteryShopItem
{
	public class LuckyDrawSet
	{
		public int Id { get; set; }

		public DynamicTexture Image { get; set; }

		public List<IUnityItem> Items { get; set; }

		public LuckyDrawShopItem Parent { get; set; }

		public LuckyDrawSetUnityView View { get; set; }
	}

	public LuckyDrawUnityView View { get; private set; }

	public List<LuckyDrawSet> Sets { get; set; }

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

	public LuckyDrawShopItem(LuckyDrawUnityView view)
	{
		View = view;
	}

	public override void Use()
	{
		Singleton<LotteryManager>.Instance.RunLuckyDraw(this);
	}
}
