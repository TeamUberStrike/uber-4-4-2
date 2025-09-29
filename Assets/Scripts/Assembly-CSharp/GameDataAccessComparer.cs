using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameDataAccessComparer : IComparer<GameMetaData>
{
	public int Compare(GameMetaData a, GameMetaData b)
	{
		int result = 0;
		if (GameDataComparer.SortAscending)
		{
			if (a.IsPublic && b.IsPublic)
			{
				result = 2;
			}
			else if (a.IsPublic)
			{
				result = 1;
			}
			else if (b.IsPublic)
			{
				result = -1;
			}
		}
		else if (!a.IsPublic && !b.IsPublic)
		{
			result = 2;
		}
		else if (!a.IsPublic)
		{
			result = 1;
		}
		else if (!b.IsPublic)
		{
			result = -1;
		}
		return result;
	}
}
