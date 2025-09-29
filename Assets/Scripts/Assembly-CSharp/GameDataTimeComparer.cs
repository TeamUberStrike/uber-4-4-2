using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataTimeComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int num = a.RoundTime - b.RoundTime;
		return (!GameDataComparer.SortAscending) ? (-num) : num;
	}
}
