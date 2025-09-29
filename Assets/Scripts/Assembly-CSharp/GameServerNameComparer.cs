using System.Collections.Generic;

public class GameServerNameComparer : IComparer<GameServerView>
{
	public int Compare(GameServerView a, GameServerView b)
	{
		return StaticCompare(a, b);
	}

	public static int StaticCompare(GameServerView a, GameServerView b)
	{
		return string.Compare(b.Name, a.Name);
	}
}
