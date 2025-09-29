using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;

internal class JoinGameUtil
{
	private static bool IsAccessAllowed
	{
		get
		{
			return PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA;
		}
	}

	private static bool IsGameFull(GameMetaData data)
	{
		return data.ConnectedPlayers >= data.MaxPlayers;
	}

	public static bool IsMobileChannel(ChannelType channel)
	{
		return channel == ChannelType.Android || channel == ChannelType.IPad || channel == ChannelType.IPhone;
	}

	public static bool CanJoinGame(FpsGameMode game)
	{
		return game.IsInitialized && (IsAccessAllowed || !IsGameFull(game.GameData));
	}

	public static bool CanJoinBlueTeam(TeamDeathMatchGameMode game)
	{
		return game.IsInitialized && (IsAccessAllowed || (!IsGameFull(game.GameData) && game.CanJoinBlueTeam));
	}

	public static bool CanJoinRedTeam(TeamDeathMatchGameMode game)
	{
		return game.IsInitialized && (IsAccessAllowed || (!IsGameFull(game.GameData) && game.CanJoinRedTeam));
	}
}
