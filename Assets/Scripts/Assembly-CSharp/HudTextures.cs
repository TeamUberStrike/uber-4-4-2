using System;
using UnityEngine;

public static class HudTextures
{
	public static Texture2D AmmoBlue { get; private set; }

	public static Texture2D AmmoRed { get; private set; }

	public static Texture2D WhiteBlur128 { get; private set; }

	public static Texture2D XPBarEmptyBlue { get; private set; }

	public static Texture2D XPBarEmptyRed { get; private set; }

	public static Texture2D XPBarFull { get; private set; }

	public static Texture2D DeathScreenTab { get; private set; }

	public static Texture2D DamageFeedbackMark { get; private set; }

	public static Texture2D LevelUp { get; private set; }

	public static Texture2D MGScale { get; private set; }

	public static Texture2D MGTranslate { get; private set; }

	public static Texture2D CNRotate { get; private set; }

	public static Texture2D CNScale { get; private set; }

	public static Texture2D HGTraslate { get; private set; }

	public static Texture2D LRScale { get; private set; }

	public static Texture2D LRTranslate { get; private set; }

	public static Texture2D MWTranslate { get; private set; }

	public static Texture2D SGScaleInside { get; private set; }

	public static Texture2D SGScaleOutside { get; private set; }

	public static Texture2D SPScale { get; private set; }

	public static Texture2D SPTranslate { get; private set; }

	public static Texture2D SRScale { get; private set; }

	public static Texture2D SRTranslate { get; private set; }

	public static Texture2D TargetCircle { get; private set; }

	public static Texture2D ReticleSRZoom { get; private set; }

	public static Texture2D QIBlue3 { get; private set; }

	public static Texture2D QIBlue1 { get; private set; }

	public static Texture2D QIRed3 { get; private set; }

	public static Texture2D QIRed1 { get; private set; }

	static HudTextures()
	{
		Texture2DConfigurator component = GameObject.Find("HudTextures").GetComponent<Texture2DConfigurator>();
		if (component == null)
		{
			throw new Exception("Missing instance of the prefab with name: HudTextures!");
		}
		AmmoBlue = component.Assets[0];
		AmmoRed = component.Assets[1];
		WhiteBlur128 = component.Assets[2];
		XPBarEmptyBlue = component.Assets[3];
		XPBarEmptyRed = component.Assets[4];
		XPBarFull = component.Assets[5];
		DeathScreenTab = component.Assets[6];
		DamageFeedbackMark = component.Assets[7];
		LevelUp = component.Assets[8];
		MGScale = component.Assets[9];
		MGTranslate = component.Assets[10];
		CNRotate = component.Assets[11];
		CNScale = component.Assets[12];
		HGTraslate = component.Assets[13];
		LRScale = component.Assets[14];
		LRTranslate = component.Assets[15];
		MWTranslate = component.Assets[16];
		SGScaleInside = component.Assets[17];
		SGScaleOutside = component.Assets[18];
		SPScale = component.Assets[19];
		SPTranslate = component.Assets[20];
		SRScale = component.Assets[21];
		SRTranslate = component.Assets[22];
		TargetCircle = component.Assets[23];
		ReticleSRZoom = component.Assets[24];
		QIBlue3 = component.Assets[25];
		QIBlue1 = component.Assets[26];
		QIRed3 = component.Assets[27];
		QIRed1 = component.Assets[28];
	}
}
