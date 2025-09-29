using System;
using UnityEngine;

[Serializable]
public class CombatRangeTier
{
	public int CloseRange = 1;

	public int MediumRange = 1;

	public int LongRange = 1;

	public CombatRangeCategory RangeCategory
	{
		get
		{
			int num = Mathf.Max(CloseRange, MediumRange, LongRange);
			return (CombatRangeCategory)(((CloseRange == num) ? 1 : 0) | ((MediumRange == num) ? 2 : 0) | ((LongRange == num) ? 4 : 0));
		}
	}

	public int GetTierForRange(CombatRangeCategory range)
	{
		switch (range)
		{
		case CombatRangeCategory.Close:
			return CloseRange;
		case CombatRangeCategory.Medium:
			return MediumRange;
		case CombatRangeCategory.Far:
			return LongRange;
		case CombatRangeCategory.CloseMedium:
			return Mathf.RoundToInt((float)(CloseRange + MediumRange) / 2f);
		case CombatRangeCategory.MediumFar:
			return Mathf.RoundToInt((float)(MediumRange + LongRange) / 2f);
		default:
			return Mathf.RoundToInt((float)(CloseRange + MediumRange + LongRange) / 3f);
		}
	}
}
