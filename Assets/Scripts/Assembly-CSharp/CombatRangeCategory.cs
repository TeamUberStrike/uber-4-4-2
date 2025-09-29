using System;

[Flags]
public enum CombatRangeCategory
{
	Close = 1,
	Medium = 2,
	Far = 4,
	CloseMedium = 3,
	MediumFar = 6,
	CloseMediumFar = 7,
	CloseFar = 5
}
