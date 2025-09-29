using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

public class MysteryBoxPopup : LotteryPopupDialog
{
	private MysteryBoxShopItem mysteryBox;

	private Vector2Anim scroll = new Vector2Anim();

	private ShopItemGrid _lotteryItemGrid;

	private List<bool> _rewardHighlight;

	public MysteryBoxPopup(MysteryBoxShopItem mysteryBox)
	{
		this.mysteryBox = mysteryBox;
		Title = mysteryBox.Name;
		Text = mysteryBox.View.Description;
		Width = 388;
		Height = 560 - GlobalUIRibbon.Instance.Height() - 10;
		_lotteryItemGrid = new ShopItemGrid(mysteryBox.View.MysteryBoxItems, mysteryBox.View.CreditsAttributed, mysteryBox.View.PointsAttributed);
		base.IsVisible = true;
		if (mysteryBox.Category != BundleCategoryType.Login && mysteryBox.Category != BundleCategoryType.Signup)
		{
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.MysteryBoxMusic);
		}
	}

	protected override void DrawPlayGUI(Rect rect)
	{
		GUI.color = ColorScheme.HudTeamBlue;
		float num = BlueStonez.label_interparkbold_18pt.CalcSize(new GUIContent(Title)).x * 2.5f;
		GUI.DrawTexture(new Rect((rect.width - num) * 0.5f, -29f, num, 100f), HudTextures.WhiteBlur128);
		GUI.color = Color.white;
		GUITools.OutlineLabel(new Rect(0f, 10f, rect.width, 30f), Title, BlueStonez.label_interparkbold_18pt, 1, Color.white, ColorScheme.GuiTeamBlue.SetAlpha(0.5f));
		GUI.Label(new Rect(30f, 35f, rect.width - 60f, 40f), Text, BlueStonez.label_interparkbold_16pt);
		int num2 = 288;
		int num3 = (Width - num2 - 6) / 2;
		int num4 = 323;
		GUI.BeginGroup(new Rect(num3, 75f, num2, num4), BlueStonez.item_slot_large);
		Rect rect2 = new Rect((num2 - 282) / 2, (num4 - 317) / 2, 282f, 317f);
		mysteryBox.Image.Draw(rect2);
		_lotteryItemGrid.Show = (rect2.Contains(Event.current.mousePosition) || ApplicationDataManager.IsMobile) && !base.IsUIDisabled;
		if (mysteryBox.View.ExposeItemsToPlayers)
		{
			_lotteryItemGrid.Draw(new Rect(0f, 0f, num2, num4));
		}
		GUI.EndGroup();
		if (GUI.Button(new Rect(rect.width * 0.5f - 70f, rect.height - 47f, 140f, 30f), mysteryBox.Price.PriceTag(false, string.Empty), BlueStonez.buttongold_large_price))
		{
			Play();
		}
		DrawNaviArrows(rect, mysteryBox);
	}

	public override void OnAfterGUI()
	{
		scroll.Update();
	}

	public override LotteryWinningPopup ShowReward()
	{
		return new MysteryBoxWinningPopup(mysteryBox.Image, mysteryBox, _rewardHighlight);
	}

	private void Play()
	{
		if (mysteryBox.Price.Currency == UberStrikeCurrencyType.Credits && mysteryBox.Price.Price > PlayerDataManager.CreditsSecure)
		{
			PopupSystem.ShowMessage(LocalizedStrings.ProblemBuyingItem, LocalizedStrings.YouNeedMoreCreditsToBuyThisItem, PopupSystem.AlertType.OKCancel, base.OpenGetCredits, "GET CREDITS", null, LocalizedStrings.CancelCaps, PopupSystem.ActionType.Positive);
		}
		else if (mysteryBox.Price.Currency == UberStrikeCurrencyType.Points && mysteryBox.Price.Price > PlayerDataManager.PointsSecure)
		{
			PopupSystem.ShowMessage(LocalizedStrings.ProblemBuyingItem, LocalizedStrings.YouNeedToEarnMorePointsToBuyThisItem, PopupSystem.AlertType.OK, LocalizedStrings.OkCaps, null);
		}
		else
		{
			RollMysteryBox();
		}
	}

	private void RollMysteryBox()
	{
		if (_onLotteryRolled != null)
		{
			_onLotteryRolled();
		}
		ShopWebServiceClient.RollMysteryBox(PlayerDataManager.AuthToken, mysteryBox.View.Id, ApplicationDataManager.Channel, OnMysteryBoxReturned, delegate(Exception ex)
		{
			base.ReturnedState = MyState.Failed;
			Debug.LogError("ERROR IN StartPlaying: " + ex.Message);
			PopupSystem.ShowMessage("Server Error", "There was a problem. Please check your internet connection and try again.");
		});
	}

	private void OnMysteryBoxReturned(List<MysteryBoxWonItemUnityView> items)
	{
		base.ReturnedState = MyState.Success;
		_rewardHighlight = new List<bool>(_lotteryItemGrid.Items.Count);
		for (int i = 0; i < _lotteryItemGrid.Items.Count; i++)
		{
			_rewardHighlight.Add(false);
		}
		MysteryBoxWonItemUnityView view;
		foreach (MysteryBoxWonItemUnityView item in items)
		{
			view = item;
			int num = _lotteryItemGrid.Items.FindIndex((ShopItemView t) => (t.ItemId > 0 && t.ItemId == view.ItemIdWon) || (t.ItemId == 0 && (t.Credits == view.CreditWon || t.Points == view.PointWon)));
			if (num >= 0)
			{
				_rewardHighlight[num] = true;
			}
		}
	}
}
