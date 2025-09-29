using System.Text;
using UnityEngine;

public static class DrawArmorPowerIconUtil
{
	private static float MaxArmorPoints = 100f;

	public static void DrawArmorPower(Rect rect, int armorPoints, int absortionPoints)
	{
		if (rect.Contains(Event.current.mousePosition))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(armorPoints + " Armor Points");
			stringBuilder.AppendLine((50 + absortionPoints).ToString("N0") + "% Defense");
			GUI.tooltip = stringBuilder.ToString();
		}
		float num = (float)armorPoints / MaxArmorPoints;
		float num2 = (float)(50 + absortionPoints) / 100f;
		GUI.DrawTexture(rect, WeaponRange.ArmorIndicatorBackground);
		GUI.color = Color.white;
		GUI.BeginGroup(new Rect(rect.x, rect.y + rect.height * (1f - num), rect.width / 2f, rect.height * num));
		GUI.DrawTexture(new Rect(0f, (0f - rect.height) * (1f - num), rect.width, rect.height), WeaponRange.ArmorIndicatorForeground);
		GUI.EndGroup();
		GUI.BeginGroup(new Rect(rect.x + rect.width / 2f, rect.y + rect.height * (1f - num2), rect.width / 2f, rect.height * num2));
		GUI.DrawTexture(new Rect((0f - rect.width) / 2f, (0f - rect.height) * (1f - num2), rect.width, rect.height), WeaponRange.ArmorIndicatorForeground);
		GUI.EndGroup();
	}
}
