using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class WeaponDetailGUI
{
	private IUnityItem _selectedItem;

	private Texture2D _curBadge;

	private RecommendType _curRecomType;

	public void SetWeaponItem(IUnityItem item, RecommendType type)
	{
		_selectedItem = item;
		_curRecomType = type;
		_curBadge = UberstrikeIconsHelper.GetRecommendBadgeTexture(type);
	}

	public void Draw(Rect rect)
	{
		Rect position = new Rect(rect.x, rect.y, rect.width, rect.height - 2f);
		GUI.BeginGroup(position, GUIContent.none, StormFront.GrayPanelBox);
		if (_selectedItem != null)
		{
			DrawWeaponBadge(new Rect((rect.width - 180f) / 2f, 15f, 180f, 125f));
			DrawWeaponCaption(new Rect(0f, 2f, rect.width, 20f));
			DrawWeaponIcons(new Rect(25f, 140f, rect.width - 50f, 30f));
			DrawWeaponPropertyBars(new Rect(-25f, 175f, rect.width, rect.height - 60f));
		}
		GUI.EndGroup();
	}

	private void DrawWeaponCaption(Rect rect)
	{
		GUI.color = ColorConverter.HexToColor("ffc41b");
		GUI.Label(rect, ShopUtils.GetRecommendationString(_curRecomType), BlueStonez.label_interparkbold_16pt);
		GUI.color = Color.white;
	}

	private void DrawWeaponBadge(Rect rect)
	{
		if (_curBadge != null)
		{
			GUI.DrawTexture(rect, _curBadge);
		}
	}

	private void DrawWeaponIcons(Rect rect)
	{
		GUI.BeginGroup(rect);
		if (_selectedItem.View != null)
		{
			switch (_selectedItem.View.ItemType)
			{
			case UberstrikeItemType.Weapon:
				DrawWeaponIcon(new Rect(0f, 0f, 50f, rect.height), UberstrikeIconsHelper.GetIconForItemClass(_selectedItem.View.ItemClass));
				GUI.Label(new Rect(49f, 0f, 2f, rect.height), GUIContent.none, BlueStonez.vertical_line_grey95);
				GUI.DrawTexture(new Rect(60f, rect.height / 2f - 16f, 32f, 32f), DrawCombatRangeIconUtil.GetIconByRange((CombatRangeCategory)((UberStrikeItemWeaponView)_selectedItem.View).CombatRange));
				GUI.Label(new Rect(99f, 0f, 2f, rect.height), GUIContent.none, BlueStonez.vertical_line_grey95);
				DrawWeaponIcon(new Rect(100f, 0f, 50f, rect.height), ShopIcons.BlankItemFrame);
				GUI.Label(new Rect(98f, 12f, 50f, 16f), _selectedItem.View.LevelLock.ToString(), BlueStonez.label_interparkbold_13pt);
				break;
			case UberstrikeItemType.Gear:
				DrawWeaponIcon(new Rect(25f, 0f, 50f, rect.height), UberstrikeIconsHelper.GetIconForItemClass(_selectedItem.View.ItemClass));
				GUI.Label(new Rect(74f, 0f, 2f, rect.height), GUIContent.none, BlueStonez.vertical_line_grey95);
				DrawWeaponIcon(new Rect(75f, 0f, 50f, rect.height), ShopIcons.BlankItemFrame);
				GUI.Label(new Rect(73f, 12f, 50f, 16f), _selectedItem.View.LevelLock.ToString(), BlueStonez.label_interparkbold_13pt);
				break;
			}
		}
		GUI.EndGroup();
	}

	private void DrawWeaponIcon(Rect rect, Texture iconTexture)
	{
		GUI.Label(new Rect(rect.x + rect.width / 2f - 13f, rect.y + rect.height / 2f - 14f, 35f, rect.height), iconTexture);
	}

	private void DrawWeaponPropertyBars(Rect rect)
	{
		int barWidth = 60;
		float num = 12f;
		float num2 = 2f;
		GUI.BeginGroup(rect);
		if (_selectedItem.View != null)
		{
			switch (_selectedItem.View.ItemType)
			{
			case UberstrikeItemType.Weapon:
				GUITools.ProgressBar(new Rect(0f, (num + num2) * 2f, rect.width, num), LocalizedStrings.Damage, WeaponConfigurationHelper.GetDamageNormalized((UberStrikeItemWeaponView)_selectedItem.View), ColorScheme.ProgressBar, barWidth);
				GUITools.ProgressBar(new Rect(0f, (num + num2) * 3f, rect.width, num), LocalizedStrings.RateOfFire, WeaponConfigurationHelper.GetRateOfFireNormalized((UberStrikeItemWeaponView)_selectedItem.View), ColorScheme.ProgressBar, barWidth);
				if (_selectedItem.View.ItemClass == UberstrikeItemClass.WeaponCannon || _selectedItem.View.ItemClass == UberstrikeItemClass.WeaponLauncher || _selectedItem.View.ItemClass == UberstrikeItemClass.WeaponSplattergun)
				{
					GUITools.ProgressBar(new Rect(0f, 0f, rect.width, num), LocalizedStrings.Velocity, WeaponConfigurationHelper.GetProjectileSpeedNormalized((UberStrikeItemWeaponView)_selectedItem.View), ColorScheme.ProgressBar, barWidth);
					GUITools.ProgressBar(new Rect(0f, num + num2, rect.width, num), LocalizedStrings.Impact, WeaponConfigurationHelper.GetSplashRadiusNormalized((UberStrikeItemWeaponView)_selectedItem.View), ColorScheme.ProgressBar, barWidth);
				}
				else if (_selectedItem.View.ItemClass == UberstrikeItemClass.WeaponMelee)
				{
					bool enabled = GUI.enabled;
					GUI.enabled = false;
					GUITools.ProgressBar(new Rect(0f, 0f, rect.width, num), LocalizedStrings.Accuracy, 0f, ColorScheme.ProgressBar, barWidth);
					GUITools.ProgressBar(new Rect(0f, num + num2, rect.width, num), LocalizedStrings.Recoil, 0f, ColorScheme.ProgressBar, barWidth);
					GUI.enabled = enabled;
				}
				else
				{
					GUITools.ProgressBar(new Rect(0f, 0f, rect.width, num), LocalizedStrings.Accuracy, WeaponConfigurationHelper.GetAccuracySpreadNormalized((UberStrikeItemWeaponView)_selectedItem.View), ColorScheme.ProgressBar, barWidth);
				}
				break;
			case UberstrikeItemType.Gear:
			{
				float percentage = (float)((UberStrikeItemGearView)_selectedItem.View).ArmorAbsorptionPercent / 100f;
				GUI.DrawTexture(new Rect(50f, 0f, 32f, 32f), ShopIcons.ItemarmorpointsIcon);
				GUI.contentColor = Color.black;
				GUI.Label(new Rect(50f, 0f, 32f, 32f), ((UberStrikeItemGearView)_selectedItem.View).ArmorPoints.ToString(), BlueStonez.label_interparkbold_16pt);
				GUI.contentColor = Color.white;
				GUI.Label(new Rect(87f, 0f, 32f, 32f), "AP", BlueStonez.label_interparkbold_18pt_left);
				GUITools.ProgressBar(new Rect(0f, 37f, rect.width, 15f), LocalizedStrings.Absorption, percentage, ColorScheme.ProgressBar, barWidth);
				break;
			}
			}
		}
		GUI.EndGroup();
	}

	private void OnSelectionChange(IUnityItem item)
	{
		_curBadge = UberstrikeIconsHelper.GetAchievementBadgeTexture(AchievementType.CostEffective);
	}
}
