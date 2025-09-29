using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameServerPlayerCountComparer : IComparer<GameServerView>
{
	public int Compare(GameServerView a, GameServerView b)
	{
		return StaticCompare(a, b);
	}

	public static int StaticCompare(GameServerView a, GameServerView b)
	{
		int num = 1;
		if (a.Data.PlayersConnected == b.Data.PlayersConnected)
		{
			return string.Compare(b.Name, a.Name);
		}
		return (((a.Data.State != ServerLoadData.Status.Alive) ? 1000 : a.Data.PlayersConnected) <= ((b.Data.State != ServerLoadData.Status.Alive) ? 1000 : b.Data.PlayersConnected)) ? (num * -1) : num;
	}
}
