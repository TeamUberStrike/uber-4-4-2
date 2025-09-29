using UberStrike.Core.Models.Views;
using UnityEngine;

public class ArmorItemDetailGUI : IBaseItemDetailGUI
{
	private UberStrikeItemGearView _item;

	private Texture2D _armorPointsIcon;

	public ArmorItemDetailGUI(UberStrikeItemGearView item, Texture2D armorPointsIcon)
	{
		_item = item;
		_armorPointsIcon = armorPointsIcon;
	}

	public void Draw()
	{
		float percentage = (float)_item.ArmorAbsorptionPercent / 100f;
		GUI.DrawTexture(new Rect(48f, 89f, 32f, 32f), _armorPointsIcon);
		GUI.contentColor = Color.black;
		GUI.Label(new Rect(48f, 89f, 32f, 32f), _item.ArmorPoints.ToString(), BlueStonez.label_interparkbold_16pt);
		GUI.contentColor = Color.white;
		GUI.Label(new Rect(80f, 89f, 32f, 32f), "AP", BlueStonez.label_interparkbold_18pt_left);
		GUITools.ProgressBar(new Rect(120f, 95f, 160f, 12f), LocalizedStrings.Absorption, percentage, ColorScheme.ProgressBar, 64);
	}
}
