using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataRuleComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int num = a.GameMode - b.GameMode;
		return (num == 0) ? GameDataNameComparer.StaticCompare(a, b) : ((!GameDataComparer.SortAscending) ? (-num) : num);
	}
}
