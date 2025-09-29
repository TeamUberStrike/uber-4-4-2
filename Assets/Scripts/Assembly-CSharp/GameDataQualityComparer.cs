using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataQualityComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int num = (int)((a.RoomJoinValue - b.RoomJoinValue) * 10f);
		return (!GameDataComparer.SortAscending) ? (-num) : num;
	}
}
