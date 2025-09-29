using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public abstract class LotteryWinningPopup : IPopupDialog
{
	private int Width;

	private int Height;

	private float _deltaY;

	private DynamicTexture _bkImage;

	private LotteryShopItem _shopItem;

	public string Text { get; set; }

	public string Title { get; set; }

	public GuiDepth Depth
	{
		get
		{
			return GuiDepth.Event;
		}
	}

	public LotteryWinningPopup(DynamicTexture image, LotteryShopItem shopItem)
	{
		Height = 560 - GlobalUIRibbon.Instance.Height() - 10;
		Width = 388;
		Title = LocalizedStrings.Congratulations.ToUpper();
		_bkImage = image;
		_shopItem = shopItem;
		AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.MysteryBoxMusic);
	}

	public void OnGUI()
	{
		Rect position = GetPosition();
		GUI.Box(position, GUIContent.none, BlueStonez.window);
		GUI.BeginGroup(position);
		if (GUI.Button(new Rect(position.width - 20f, 0f, 20f, 20f), "X", BlueStonez.friends_hidden_button))
		{
			PopupSystem.HideMessage(this);
		}
		GUI.color = ColorScheme.HudTeamBlue;
		float num = BlueStonez.label_interparkbold_18pt.CalcSize(new GUIContent(Title)).x * 2.5f;
		GUI.DrawTexture(new Rect((position.width - num) * 0.5f, -29f, num, 100f), HudTextures.WhiteBlur128);
		GUI.color = Color.white;
		GUITools.OutlineLabel(new Rect(0f, 15f, position.width, 30f), Title, BlueStonez.label_interparkbold_32pt, 1, Color.white, ColorScheme.GuiTeamBlue);
		GUI.Label(new Rect(30f, 40f, position.width - 60f, 40f), Text, BlueStonez.label_interparkbold_16pt);
		int num2 = 288;
		int num3 = (Width - num2 - 6) / 2;
		int num4 = 323;
		GUI.BeginGroup(new Rect(num3, 75f, num2, num4), BlueStonez.item_slot_large);
		_bkImage.Draw(new Rect((num2 - 282) / 2, (num4 - 317) / 2, 282f, 317f));
		Rect rect = new Rect(0f, 0f, num2, num4);
		DrawItemGrid(rect, true);
		GUI.EndGroup();
		if (GUI.Button(new Rect(num3, position.height - 55f, 120f, 32f), LocalizedStrings.PlayAgainCaps, BlueStonez.button_green))
		{
			PopupSystem.HideMessage(this);
			if (_shopItem != null)
			{
				if (_shopItem.Category == BundleCategoryType.Login || _shopItem.Category == BundleCategoryType.Signup)
				{
					Singleton<LotteryManager>.Instance.ShowNextItem(_shopItem);
				}
				else
				{
					_shopItem.Use();
				}
			}
		}
		if (GUI.Button(new Rect(position.width - 126f - (float)num3, position.height - 55f, 120f, 32f), LocalizedStrings.DoneCaps, BlueStonez.button))
		{
			PopupSystem.HideMessage(this);
			SelectShopAreaEvent selectShopAreaEvent = new SelectShopAreaEvent();
			selectShopAreaEvent.ShopArea = ShopArea.Inventory;
			CmuneEventHandler.Route(selectShopAreaEvent);
		}
		GUI.EndGroup();
	}

	public void OnHide()
	{
		if (GameState.HasCurrentGame)
		{
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
		}
		else
		{
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.SeletronRadioShort);
		}
	}

	public void SetYOffset(float offset)
	{
		_deltaY = offset;
	}

	protected abstract void DrawItemGrid(Rect rect, bool showItems);

	private Rect GetPosition()
	{
		float left = (float)(Screen.width - Width) * 0.5f;
		float num = (float)GlobalUIRibbon.Instance.Height() + (float)(Screen.height - GlobalUIRibbon.Instance.Height() - Height) * 0.5f;
		return new Rect(left, num - _deltaY, Width, Height);
	}
}
