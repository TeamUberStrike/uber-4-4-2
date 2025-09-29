using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

public class LuckyDrawPopup : LotteryPopupDialog
{
	private const int spacing = 20;

	private const int offset = 30;

	private LuckyDrawShopItem luckyDraw;

	private List<ShopItemGrid> _itemGrids;

	private Vector2Anim scroll = new Vector2Anim();

	private int _luckyDrawResult = -1;

	public bool ShowNavigationArrows { get; set; }

	public string HelpText { get; set; }

	public event Action OnLuckyDrawCompleted;

	public LuckyDrawPopup(LuckyDrawShopItem luckyDraw)
	{
		this.luckyDraw = luckyDraw;
		Title = luckyDraw.Name;
		Text = luckyDraw.View.Description;
		Width = 60 + luckyDraw.Sets.Count * 308 - 20;
		Height = 560 - GlobalUIRibbon.Instance.Height() - 10;
		ShowNavigationArrows = true;
		HelpText = LocalizedStrings.LuckyDrawHelpText;
		_itemGrids = new List<ShopItemGrid>(luckyDraw.Sets.Count);
		foreach (LuckyDrawSetUnityView luckyDrawSet in luckyDraw.View.LuckyDrawSets)
		{
			ShopItemGrid item = new ShopItemGrid(luckyDrawSet.LuckyDrawSetItems, luckyDrawSet.CreditsAttributed, luckyDrawSet.PointsAttributed);
			_itemGrids.Add(item);
		}
		_showExitButton = luckyDraw.View.Category != BundleCategoryType.Login && luckyDraw.View.Category != BundleCategoryType.Signup;
		base.IsVisible = true;
		if (_showExitButton)
		{
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.MysteryBoxMusic);
		}
	}

	protected override void DrawPlayGUI(Rect rect)
	{
		Width = 60 + luckyDraw.Sets.Count * 308 - 20;
		GUI.color = ColorScheme.HudTeamBlue;
		float num = BlueStonez.label_interparkbold_18pt.CalcSize(new GUIContent(Title)).x * 2.5f;
		GUI.DrawTexture(new Rect((rect.width - num) * 0.5f, -29f, num, 100f), HudTextures.WhiteBlur128);
		GUI.color = Color.white;
		GUITools.OutlineLabel(new Rect(0f, 10f, rect.width, 30f), Title, BlueStonez.label_interparkbold_18pt, 1, Color.white, ColorScheme.GuiTeamBlue.SetAlpha(0.5f));
		GUI.Label(new Rect(30f, 35f, rect.width - 60f, 40f), Text, BlueStonez.label_interparkbold_16pt);
		int num2 = 288;
		int num3 = 30;
		int num4 = 323;
		for (int i = 0; i < luckyDraw.Sets.Count; i++)
		{
			LuckyDrawShopItem.LuckyDrawSet luckyDrawSet = luckyDraw.Sets[i];
			GUI.BeginGroup(new Rect(num3, 75f, num2, num4), BlueStonez.item_slot_large);
			Rect rect2 = new Rect((num2 - 282) / 2, (num4 - 317) / 2, 282f, 317f);
			luckyDrawSet.Image.Draw(rect2);
			_itemGrids[i].Show = (rect2.Contains(Event.current.mousePosition) || ApplicationDataManager.IsMobile) && !base.IsUIDisabled;
			if (luckyDrawSet.View.ExposeItemsToPlayers)
			{
				_itemGrids[i].Draw(new Rect(0f, 0f, num2, num4));
			}
			num3 += num2 + 20;
			GUI.EndGroup();
		}
		if (luckyDraw.Price.Price > 0)
		{
			if (GUI.Button(new Rect(rect.width * 0.5f - 70f, rect.height - 47f, 140f, 30f), luckyDraw.Price.PriceTag(false, string.Empty), BlueStonez.buttongold_large_price))
			{
				Play();
			}
		}
		else if (GUI.Button(new Rect(rect.width * 0.5f - 70f, rect.height - 47f, 140f, 30f), LocalizedStrings.PlayCaps, BlueStonez.buttongold_large))
		{
			Play();
		}
		if (ShowNavigationArrows)
		{
			DrawNaviArrows(rect, luckyDraw);
		}
	}

	public override void OnAfterGUI()
	{
		scroll.Update();
	}

	public override LotteryWinningPopup ShowReward()
	{
		LuckyDrawShopItem.LuckyDrawSet luckyDrawSet = luckyDraw.Sets.Find((LuckyDrawShopItem.LuckyDrawSet s) => s.Id == _luckyDrawResult);
		if (luckyDrawSet != null)
		{
			return new LuckyDrawWinningPopup(Text, luckyDrawSet.Image, luckyDraw, luckyDrawSet.View);
		}
		return null;
	}

	private void Play()
	{
		if (luckyDraw.Price.Currency == UberStrikeCurrencyType.Credits && luckyDraw.Price.Price > PlayerDataManager.CreditsSecure)
		{
			PopupSystem.ShowMessage(LocalizedStrings.ProblemBuyingItem, LocalizedStrings.YouNeedMoreCreditsToBuyThisItem, PopupSystem.AlertType.OKCancel, ApplicationDataManager.OpenBuyCredits, LocalizedStrings.BuyCreditsCaps, null, LocalizedStrings.CancelCaps, PopupSystem.ActionType.Positive);
		}
		else if (luckyDraw.Price.Currency == UberStrikeCurrencyType.Points && luckyDraw.Price.Price > PlayerDataManager.PointsSecure)
		{
			PopupSystem.ShowMessage(LocalizedStrings.ProblemBuyingItem, LocalizedStrings.YouNeedToEarnMorePointsToBuyThisItem, PopupSystem.AlertType.OK, LocalizedStrings.OkCaps, null);
		}
		else
		{
			RollLuckyDraw();
		}
	}

	private void RollLuckyDraw()
	{
		if (_onLotteryRolled != null)
		{
			_onLotteryRolled();
		}
		ShopWebServiceClient.RollLuckyDraw(PlayerDataManager.AuthToken, luckyDraw.View.Id, ApplicationDataManager.Channel, OnLuckyDrawReturn, delegate(Exception ex)
		{
			base.ReturnedState = MyState.Failed;
			Debug.LogError("ERROR IN StartPlaying: " + ex.Message);
			PopupSystem.ShowMessage("Server Error", "There was a problem. Please check your internet connection and try again.");
		});
	}

	private void OnLuckyDrawReturn(int result)
	{
		base.ReturnedState = MyState.Success;
		_luckyDrawResult = result;
		if (this.OnLuckyDrawCompleted != null)
		{
			this.OnLuckyDrawCompleted();
		}
	}
}
