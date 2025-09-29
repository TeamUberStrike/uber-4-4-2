using System;

[Flags]
public enum HudDrawFlags
{
	None = 0,
	Score = 1,
	HealthArmor = 2,
	Ammo = 4,
	Weapons = 8,
	Reticle = 0x10,
	RoundTime = 0x20,
	XpPoints = 0x40,
	InGameHelp = 0x80,
	EventStream = 0x100,
	RemainingKill = 0x200,
	InGameChat = 0x400,
	StateMsg = 0x800
}
