using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

internal static class TabScreenPlayerSorter
{
	private class PlayerSplatSorter : Comparer<CharacterInfo>
	{
		public override int Compare(CharacterInfo x, CharacterInfo y)
		{
			return y.Kills - x.Kills;
		}
	}

	public static void SortDeathMatchPlayers(IEnumerable<CharacterInfo> toBeSortedPlayers)
	{
		List<CharacterInfo> list = new List<CharacterInfo>(toBeSortedPlayers);
		list.Sort(new PlayerSplatSorter());
		TabScreenPanelGUI.Instance.SetPlayerListAll(list);
	}

	public static void SortTeamMatchPlayers(IEnumerable<CharacterInfo> toBeSortedPlayers)
	{
		List<CharacterInfo> list = new List<CharacterInfo>();
		List<CharacterInfo> list2 = new List<CharacterInfo>();
		foreach (CharacterInfo toBeSortedPlayer in toBeSortedPlayers)
		{
			if (toBeSortedPlayer.TeamID == TeamID.BLUE)
			{
				list.Add(toBeSortedPlayer);
			}
			else if (toBeSortedPlayer.TeamID == TeamID.RED)
			{
				list2.Add(toBeSortedPlayer);
			}
		}
		list.Sort(new PlayerSplatSorter());
		list2.Sort(new PlayerSplatSorter());
		TabScreenPanelGUI.Instance.SetPlayerListBlue(list);
		TabScreenPanelGUI.Instance.SetPlayerListRed(list2);
	}
}
