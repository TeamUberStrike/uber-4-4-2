using UberStrike.Realtime.UnitySdk;

public static class CmuneNetworkManager
{
	public static bool UseLocalCommServer;

	public static GameServerView CurrentLobbyServer;

	public static GameServerView CurrentCommServer;

	static CmuneNetworkManager()
	{
		UseLocalCommServer = true;
		CurrentLobbyServer = GameServerView.Empty;
		CurrentCommServer = GameServerView.Empty;
		RealtimeSerialization.Converter = new UberStrikeByteConverter();
	}
}
