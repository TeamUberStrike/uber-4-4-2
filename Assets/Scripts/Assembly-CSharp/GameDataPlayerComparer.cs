using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataPlayerComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int num = a.ConnectedPlayers - b.ConnectedPlayers;
		return (num == 0) ? GameDataNameComparer.StaticCompare(a, b) : ((!GameDataComparer.SortAscending) ? (-num) : num);
	}
}
