using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataMapComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int num = a.MapID - b.MapID;
		return (num == 0) ? GameDataNameComparer.StaticCompare(a, b) : ((!GameDataComparer.SortAscending) ? (-num) : num);
	}
}
