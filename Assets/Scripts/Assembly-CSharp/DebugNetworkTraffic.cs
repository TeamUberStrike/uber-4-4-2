using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugNetworkTraffic : IDebugPage
{
	private Vector2 scroller;

	public string Title
	{
		get
		{
			return "Network";
		}
	}

	public void Draw()
	{
		scroller = GUILayout.BeginScrollView(scroller);
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Label("IN (" + NetworkStatistics.TotalBytesIn + ")");
		foreach (KeyValuePair<string, NetworkStatistics.Statistics> item in NetworkStatistics.Incoming)
		{
			GUILayout.Label(item.Key + ": " + item.Value);
		}
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		GUILayout.Label("OUT (" + NetworkStatistics.TotalBytesOut + ")");
		foreach (KeyValuePair<string, NetworkStatistics.Statistics> item2 in NetworkStatistics.Outgoing)
		{
			GUILayout.Label(item2.Key + ": " + item2.Value);
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}
}
