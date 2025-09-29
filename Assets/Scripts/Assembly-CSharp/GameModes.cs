public static class GameModes
{
	public static string GetModeName(int gameMode)
	{
		return GetModeName((GameMode)gameMode);
	}

	public static string GetModeName(GameMode gameMode)
	{
		switch (gameMode)
		{
		case GameMode.DeathMatch:
			return LocalizedStrings.DeathMatch;
		case GameMode.TeamDeathMatch:
			return LocalizedStrings.TeamDeathMatch;
		case GameMode.Training:
			return LocalizedStrings.TrainingCaps;
		default:
			return LocalizedStrings.None;
		}
	}
}
