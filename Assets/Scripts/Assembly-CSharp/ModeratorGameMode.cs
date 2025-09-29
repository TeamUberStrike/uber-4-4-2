using UberStrike.Realtime.UnitySdk;

[NetworkClass(106)]
public class ModeratorGameMode : ClientNetworkClass
{
	private static ModeratorGameMode _moderator;

	private ModeratorGameMode(GameMetaData data)
		: base(GameConnectionManager.Rmi)
	{
	}

	public static void ModerateGameMode(FpsGameMode mode)
	{
		if (_moderator != null)
		{
			_moderator.Dispose();
		}
		_moderator = new ModeratorGameMode(mode.GameData);
		mode.InitializeMode(TeamID.NONE, true);
		Singleton<PlayerSpectatorControl>.Instance.IsEnabled = true;
	}

	protected override void Dispose(bool dispose)
	{
		Singleton<PlayerSpectatorControl>.Instance.IsEnabled = false;
		base.Dispose(dispose);
		_moderator = null;
	}
}
