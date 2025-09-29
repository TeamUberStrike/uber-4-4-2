using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugSpawnPoints : IDebugPage
{
	private Vector2 scroll1;

	private Vector2 scroll2;

	private Vector2 scroll3;

	private GameMode gameMode;

	public string Title
	{
		get
		{
			return "Spawn";
		}
	}

	public void Draw()
	{
		GUILayout.BeginHorizontal();
		foreach (int value in Enum.GetValues(typeof(GameMode)))
		{
			if (GUILayout.Button(((GameMode)value).ToString()))
			{
				gameMode = (GameMode)value;
			}
		}
		GUILayout.EndHorizontal();
		if (GameState.HasCurrentGame)
		{
			GUILayout.Label(string.Concat("CurrentSpawnPoint ", GameState.CurrentGame.GameMode, " - ", GameState.CurrentGame.CurrentSpawnPoint));
		}
		else
		{
			GUILayout.Label("CurrentSpawnPoint - no game mode running");
		}
		GUILayout.BeginHorizontal();
		scroll1 = GUILayout.BeginScrollView(scroll1);
		GUILayout.Label(TeamID.NONE.ToString());
		Vector3 position;
		Quaternion rotation;
		for (int i = 0; i < Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(gameMode, TeamID.NONE); i++)
		{
			Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(i, gameMode, TeamID.NONE, out position, out rotation);
			GUILayout.Label(i + ": " + position);
		}
		GUILayout.EndScrollView();
		scroll2 = GUILayout.BeginScrollView(scroll2);
		GUILayout.Label(TeamID.BLUE.ToString());
		for (int j = 0; j < Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(gameMode, TeamID.BLUE); j++)
		{
			Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(j, gameMode, TeamID.BLUE, out position, out rotation);
			GUILayout.Label(j + ": " + position);
		}
		GUILayout.EndScrollView();
		scroll3 = GUILayout.BeginScrollView(scroll3);
		GUILayout.Label(TeamID.RED.ToString());
		for (int k = 0; k < Singleton<SpawnPointManager>.Instance.GetSpawnPointCount(gameMode, TeamID.RED); k++)
		{
			Singleton<SpawnPointManager>.Instance.GetSpawnPointAt(k, gameMode, TeamID.RED, out position, out rotation);
			GUILayout.Label(k + ": " + position);
		}
		GUILayout.EndScrollView();
		GUILayout.EndHorizontal();
	}
}
