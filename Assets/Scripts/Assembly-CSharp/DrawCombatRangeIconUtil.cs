using System.Collections.Generic;
using System.Text;
using UberStrike.Core.Models.Views;
using UnityEngine;

public static class DrawCombatRangeIconUtil
{
	private static Texture2D[] _closeRange;

	private static Texture2D[] _midRange;

	private static Texture2D[] _farRange;

	public static int _warningRange;

	static DrawCombatRangeIconUtil()
	{
		_closeRange = new Texture2D[5]
		{
			WeaponRange.IconRange02,
			WeaponRange.IconRange05,
			WeaponRange.IconRange08,
			WeaponRange.IconRange11,
			WeaponRange.CombatRangeClose
		};
		_midRange = new Texture2D[5]
		{
			WeaponRange.IconRange03,
			WeaponRange.IconRange06,
			WeaponRange.IconRange09,
			WeaponRange.IconRange12,
			WeaponRange.CombatRangeMedium
		};
		_farRange = new Texture2D[5]
		{
			WeaponRange.IconRange04,
			WeaponRange.IconRange07,
			WeaponRange.IconRange10,
			WeaponRange.IconRange13,
			WeaponRange.CombatRangeFar
		};
	}

	public static void DrawWeaponRangeIcon2(Rect rect, params IUnityItem[] weapons)
	{
		int close;
		int medium;
		int far;
		GetBestTiersPerCombatRange(out close, out medium, out far, weapons);
		GUI.color = new Color(1f, 0f, 0f, 0.25f * (float)close);
		GUI.DrawTexture(rect, WeaponRange.CombatRangeClose);
		GUI.color = new Color(1f, 0f, 0f, 0.25f * (float)medium);
		GUI.DrawTexture(rect, WeaponRange.CombatRangeMedium);
		GUI.color = new Color(1f, 0f, 0f, 0.25f * (float)far);
		GUI.DrawTexture(rect, WeaponRange.CombatRangeFar);
		GUI.color = Color.white;
		GUI.DrawTexture(rect, WeaponRange.CombatRangeBackground);
	}

	public static void DrawRecommendedCombatRange(Rect rect, CombatRangeTier mapRange, params IUnityItem[] weapons)
	{
		int close;
		int medium;
		int far;
		GetBestTiersPerCombatRange(out close, out medium, out far, weapons);
		if (rect.Contains(Event.current.mousePosition))
		{
			string value = CreateToolTip(close, "You need a shotgun or a pistol here !");
			string value2 = CreateToolTip(medium, "You need a splatter, a canon or a machinegun here !");
			string value3 = CreateToolTip(far, "You need a sniper rifle here !");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Combat Efficiency");
			stringBuilder.Append("Long: ").AppendLine(value3);
			stringBuilder.Append("Medium: ").AppendLine(value2);
			stringBuilder.Append("Close: ").AppendLine(value);
			GUI.tooltip = stringBuilder.ToString();
		}
		CombatRangeCategory combatRangeCategory = (CombatRangeCategory)0;
		if (close - mapRange.CloseRange < 0)
		{
			combatRangeCategory |= CombatRangeCategory.Close;
		}
		if (medium - mapRange.MediumRange < 0)
		{
			combatRangeCategory |= CombatRangeCategory.Medium;
		}
		if (far - mapRange.LongRange < 0)
		{
			combatRangeCategory |= CombatRangeCategory.Far;
		}
		if (combatRangeCategory != 0)
		{
			float alpha = (Mathf.Sin(Time.time * 9f) + 1f) * 0.3f;
			GUI.color = Color.white.SetAlpha(alpha);
			foreach (Texture2D rangeTexture in GetRangeTextures(combatRangeCategory))
			{
				GUI.DrawTexture(rect, rangeTexture);
			}
			GUI.color = Color.white;
		}
		_warningRange = (int)combatRangeCategory;
	}

	private static string CreateToolTip(int tier, string defaultText)
	{
		switch (tier)
		{
		case 0:
			return "Weak";
		case 1:
			return "Average";
		case 2:
			return "Good";
		case 3:
			return "Excellent";
		default:
			return defaultText;
		}
	}

	private static void GetBestTiersPerCombatRange(out int close, out int medium, out int far, IUnityItem[] weapons)
	{
		close = 0;
		medium = 0;
		far = 0;
		foreach (IUnityItem unityItem in weapons)
		{
			if (unityItem != null)
			{
				UberStrikeItemWeaponView uberStrikeItemWeaponView = unityItem.View as UberStrikeItemWeaponView;
				if (uberStrikeItemWeaponView != null && IsCloseRange((CombatRangeCategory)uberStrikeItemWeaponView.CombatRange))
				{
					close = Mathf.Max(close, uberStrikeItemWeaponView.Tier);
				}
				if (uberStrikeItemWeaponView != null && IsMidRange((CombatRangeCategory)uberStrikeItemWeaponView.CombatRange))
				{
					medium = Mathf.Max(medium, uberStrikeItemWeaponView.Tier);
				}
				if (uberStrikeItemWeaponView != null && IsFarRange((CombatRangeCategory)uberStrikeItemWeaponView.CombatRange))
				{
					far = Mathf.Max(far, uberStrikeItemWeaponView.Tier);
				}
			}
		}
		close = Mathf.Min(close, 3);
		medium = Mathf.Min(medium, 3);
		far = Mathf.Min(far, 3);
	}

	private static IEnumerable<Texture2D> GetRangeTextures(CombatRangeCategory range)
	{
		switch (range)
		{
		case CombatRangeCategory.Close:
			return new Texture2D[1] { _closeRange[4] };
		case CombatRangeCategory.Medium:
			return new Texture2D[1] { _midRange[4] };
		case CombatRangeCategory.Far:
			return new Texture2D[1] { _farRange[4] };
		case CombatRangeCategory.MediumFar:
			return new Texture2D[2]
			{
				_midRange[4],
				_farRange[4]
			};
		case CombatRangeCategory.CloseMedium:
			return new Texture2D[2]
			{
				_closeRange[4],
				_midRange[4]
			};
		case CombatRangeCategory.CloseMediumFar:
			return new Texture2D[3]
			{
				_closeRange[4],
				_midRange[4],
				_farRange[4]
			};
		case CombatRangeCategory.CloseFar:
			return new Texture2D[2]
			{
				_closeRange[4],
				_farRange[4]
			};
		default:
			return new Texture2D[0];
		}
	}

	public static bool IsCloseRange(CombatRangeCategory range)
	{
		return (range & CombatRangeCategory.Close) == CombatRangeCategory.Close;
	}

	public static bool IsMidRange(CombatRangeCategory range)
	{
		return (range & CombatRangeCategory.Medium) == CombatRangeCategory.Medium;
	}

	public static bool IsFarRange(CombatRangeCategory range)
	{
		return (range & CombatRangeCategory.Far) == CombatRangeCategory.Far;
	}

	public static Texture2D GetIconByRange(CombatRangeCategory range)
	{
		switch (range)
		{
		case CombatRangeCategory.Close:
			return WeaponRange.CombatRangeMiniClose;
		case CombatRangeCategory.CloseMedium:
			return WeaponRange.CombatRangeMiniCloseMed;
		case CombatRangeCategory.Far:
			return WeaponRange.CombatRangeMiniFar;
		case CombatRangeCategory.Medium:
			return WeaponRange.CombatRangeMiniMed;
		case CombatRangeCategory.MediumFar:
			return WeaponRange.CombatRangeMiniMedFar;
		default:
			Debug.LogWarning(string.Concat("Cannot find corresponding icon for range - [", range, "]."));
			return WeaponRange.CombatRangeMiniMedFar;
		}
	}
}
