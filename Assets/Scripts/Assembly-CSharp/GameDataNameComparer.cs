using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataNameComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		return StaticCompare(a, b);
	}

	public static int StaticCompare(GameMetaData a, GameMetaData b)
	{
		return (!GameDataComparer.SortAscending) ? string.Compare(a.RoomName, b.RoomName) : string.Compare(b.RoomName, a.RoomName);
	}
}
