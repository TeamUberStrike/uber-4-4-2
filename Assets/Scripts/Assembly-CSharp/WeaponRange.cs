using System;
using UnityEngine;

public static class WeaponRange
{
	public static Texture2D CombatRangeBackground { get; private set; }

	public static Texture2D CombatRangeClose { get; private set; }

	public static Texture2D CombatRangeMedium { get; private set; }

	public static Texture2D CombatRangeFar { get; private set; }

	public static Texture2D CombatRangeMiniClose { get; private set; }

	public static Texture2D CombatRangeMiniCloseMed { get; private set; }

	public static Texture2D CombatRangeMiniFar { get; private set; }

	public static Texture2D CombatRangeMiniMed { get; private set; }

	public static Texture2D CombatRangeMiniMedFar { get; private set; }

	public static Texture2D IconRange02 { get; private set; }

	public static Texture2D IconRange03 { get; private set; }

	public static Texture2D IconRange04 { get; private set; }

	public static Texture2D IconRange05 { get; private set; }

	public static Texture2D IconRange06 { get; private set; }

	public static Texture2D IconRange07 { get; private set; }

	public static Texture2D IconRange08 { get; private set; }

	public static Texture2D IconRange09 { get; private set; }

	public static Texture2D IconRange10 { get; private set; }

	public static Texture2D IconRange11 { get; private set; }

	public static Texture2D IconRange12 { get; private set; }

	public static Texture2D IconRange13 { get; private set; }

	public static Texture2D ArmorIndicatorBackground { get; private set; }

	public static Texture2D ArmorIndicatorForeground { get; private set; }

	static WeaponRange()
	{
		Texture2DConfigurator texture2DConfigurator = null;
		UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(Texture2DConfigurator));
		for (int i = 0; i < array.Length; i++)
		{
			Texture2DConfigurator texture2DConfigurator2 = (Texture2DConfigurator)array[i];
			if (texture2DConfigurator2.name == "WeaponRange")
			{
				texture2DConfigurator = texture2DConfigurator2;
				break;
			}
		}
		if (texture2DConfigurator == null)
		{
			throw new Exception("Missing instance of the prefab with name: WeaponRange!");
		}
		CombatRangeBackground = texture2DConfigurator.Assets[0];
		CombatRangeClose = texture2DConfigurator.Assets[1];
		CombatRangeMedium = texture2DConfigurator.Assets[2];
		CombatRangeFar = texture2DConfigurator.Assets[3];
		CombatRangeMiniClose = texture2DConfigurator.Assets[4];
		CombatRangeMiniCloseMed = texture2DConfigurator.Assets[5];
		CombatRangeMiniFar = texture2DConfigurator.Assets[6];
		CombatRangeMiniMed = texture2DConfigurator.Assets[7];
		CombatRangeMiniMedFar = texture2DConfigurator.Assets[8];
		IconRange02 = texture2DConfigurator.Assets[9];
		IconRange03 = texture2DConfigurator.Assets[10];
		IconRange04 = texture2DConfigurator.Assets[11];
		IconRange05 = texture2DConfigurator.Assets[12];
		IconRange06 = texture2DConfigurator.Assets[13];
		IconRange07 = texture2DConfigurator.Assets[14];
		IconRange08 = texture2DConfigurator.Assets[15];
		IconRange09 = texture2DConfigurator.Assets[16];
		IconRange10 = texture2DConfigurator.Assets[17];
		IconRange11 = texture2DConfigurator.Assets[18];
		IconRange12 = texture2DConfigurator.Assets[19];
		IconRange13 = texture2DConfigurator.Assets[20];
		ArmorIndicatorBackground = texture2DConfigurator.Assets[21];
		ArmorIndicatorForeground = texture2DConfigurator.Assets[22];
	}
}
