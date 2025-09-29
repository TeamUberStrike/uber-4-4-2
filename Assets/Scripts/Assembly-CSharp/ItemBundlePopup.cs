using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class ItemBundlePopup : LotteryPopupDialog
{
	private BundleUnityView _bundleUnityView;

	private ShopItemGrid _lotteryItemGrid;

	public ItemBundlePopup(BundleUnityView bundleUnityView)
	{
		_bundleUnityView = bundleUnityView;
		Title = bundleUnityView.BundleView.Name;
		Text = bundleUnityView.BundleView.Description;
		Width = 388;
		Height = 560 - GlobalUIRibbon.Instance.Height() - 10;
		_lotteryItemGrid = new ShopItemGrid(bundleUnityView.BundleView.BundleItemViews);
	}

	protected override void DrawPlayGUI(Rect rect)
	{
		GUI.color = ColorScheme.HudTeamBlue;
		float num = BlueStonez.label_interparkbold_18pt.CalcSize(new GUIContent(Title)).x * 2.5f;
		GUI.DrawTexture(new Rect((rect.width - num) * 0.5f, -29f, num, 100f), HudTextures.WhiteBlur128);
		GUI.color = Color.white;
		GUITools.OutlineLabel(new Rect(0f, 10f, rect.width, 30f), Title, BlueStonez.label_interparkbold_18pt, 1, Color.white, ColorScheme.GuiTeamBlue.SetAlpha(0.5f));
		GUI.Label(new Rect(30f, 35f, rect.width - 60f, 40f), Text, BlueStonez.label_interparkbold_13pt);
		int num2 = 288;
		int num3 = (Width - num2 - 6) / 2;
		int num4 = 323;
		GUI.BeginGroup(new Rect(num3, 75f, num2, num4), BlueStonez.item_slot_large);
		Rect rect2 = new Rect((num2 - 282) / 2, (num4 - 317) / 2, 282f, 317f);
		_bundleUnityView.Image.Draw(rect2);
		_lotteryItemGrid.Show = rect2.Contains(Event.current.mousePosition) || ApplicationDataManager.IsMobile;
		_lotteryItemGrid.Draw(new Rect(0f, 0f, num2, num4));
		GUI.EndGroup();
		if (GUI.Button(new Rect(rect.width * 0.5f - 95f, rect.height - 42f, 20f, 20f), GUIContent.none, BlueStonez.button_left))
		{
			SfxManager.Play2dAudioClip(GameAudio.ButtonClick);
			PopupSystem.HideMessage(this);
			BundleUnityView previousItem = Singleton<BundleManager>.Instance.GetPreviousItem(_bundleUnityView);
			if (previousItem != null)
			{
				PopupSystem.Show(new ItemBundlePopup(previousItem));
			}
		}
		if (GUI.Button(new Rect(rect.width * 0.5f + 75f, rect.height - 42f, 20f, 20f), GUIContent.none, BlueStonez.button_right))
		{
			SfxManager.Play2dAudioClip(GameAudio.ButtonClick);
			PopupSystem.HideMessage(this);
			BundleUnityView nextItem = Singleton<BundleManager>.Instance.GetNextItem(_bundleUnityView);
			if (nextItem != null)
			{
				PopupSystem.Show(new ItemBundlePopup(nextItem));
			}
		}
		GUI.enabled = !_bundleUnityView.IsOwned && _bundleUnityView.IsValid && GUITools.SaveClickIn(1f);
		BuyButton(rect, _bundleUnityView);
		GUI.enabled = true;
	}

	private void BuyButton(Rect position, BundleUnityView bundleUnityView)
	{
		switch (ApplicationDataManager.Channel)
		{
		case ChannelType.MacAppStore:
		case ChannelType.IPad:
		case ChannelType.Android:
			BuyStoreKitButton(position, bundleUnityView);
			break;
		case ChannelType.WebFacebook:
			BuyFBCButton(position, bundleUnityView);
			break;
		}
	}

	private void BuyFBCButton(Rect position, BundleUnityView bundleUnityView)
	{
		if (GUI.Button(new Rect(position.width * 0.5f - 70f, position.height - 47f, 140f, 30f), (!_bundleUnityView.IsOwned) ? new GUIContent(bundleUnityView.Price, UberstrikeIcons.FacebookCreditsIcon, "Buy the " + bundleUnityView.BundleView.Name + " pack.") : new GUIContent("Purchased"), BlueStonez.buttongold_large_price))
		{
			PopupSystem.HideMessage(this);
			GUITools.Clicked();
			if (ScreenResolutionManager.IsFullScreen)
			{
				ScreenResolutionManager.IsFullScreen = false;
			}
			Singleton<BundleManager>.Instance.BuyFacebookBundle(bundleUnityView.BundleView.Id);
		}
	}

	private void BuyStoreKitButton(Rect position, BundleUnityView bundleUnityView)
	{
		if (!GUI.Button(new Rect(position.width * 0.5f - 70f, position.height - 47f, 140f, 30f), (!_bundleUnityView.IsOwned) ? new GUIContent(bundleUnityView.CurrencySymbol + bundleUnityView.Price, "Buy the " + bundleUnityView.BundleView.Name + " pack.") : new GUIContent("Purchased"), BlueStonez.buttongold_large_price))
		{
			return;
		}
		PopupSystem.HideMessage(this);
		if (Singleton<BundleManager>.Instance.CanMakeMasPayments)
		{
			GUITools.Clicked();
			if (ScreenResolutionManager.IsFullScreen)
			{
				ScreenResolutionManager.IsFullScreen = false;
			}
			Singleton<BundleManager>.Instance.BuyStoreKitItem(bundleUnityView);
		}
		else
		{
			PopupSystem.ShowMessage(LocalizedStrings.Error, "Sorry, it appears you are unable to make In App purchases at this time.", PopupSystem.AlertType.OK);
		}
	}

	public override LotteryWinningPopup ShowReward()
	{
		return null;
	}
}
