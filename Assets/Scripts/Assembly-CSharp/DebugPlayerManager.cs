using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugPlayerManager : IDebugPage
{
	private Vector2 v1;

	public string Title
	{
		get
		{
			return "Players";
		}
	}

	public void Draw()
	{
		if (!GameState.HasCurrentGame)
		{
			return;
		}
		v1 = GUILayout.BeginScrollView(v1, GUILayout.MinHeight(200f));
		GUILayout.BeginHorizontal();
		foreach (UberStrike.Realtime.UnitySdk.CharacterInfo value in GameState.CurrentGame.Players.Values)
		{
			GUILayout.Label(value.PlayerName + " " + value.Ping);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}
}
