using UberStrike.Core.Types;

public static class EnumConversion
{
	public static short GetGameModeID(this GameModeType mode)
	{
		switch (mode)
		{
		case GameModeType.DeathMatch:
			return 101;
		case GameModeType.TeamDeathMatch:
			return 100;
		default:
			return 109;
		}
	}
}
