using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataLatencyComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int num = a.Latency - b.Latency;
		return (!GameDataComparer.SortAscending) ? (-num) : num;
	}
}
