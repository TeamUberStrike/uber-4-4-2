using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameServerLatencyComparer : IComparer<GameServerView>
{
	public int Compare(GameServerView a, GameServerView b)
	{
		return StaticCompare(a, b);
	}

	public static int StaticCompare(GameServerView a, GameServerView b)
	{
		int num = 1;
		int num2 = ((a.Data.State != ServerLoadData.Status.Alive) ? 1000 : a.Latency);
		int num3 = ((b.Data.State != ServerLoadData.Status.Alive) ? 1000 : b.Latency);
		if (a.Latency == b.Latency)
		{
			return string.Compare(b.Name, a.Name);
		}
		return (num2 <= num3) ? (num * -1) : num;
	}
}
