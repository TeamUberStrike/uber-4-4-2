using System.Collections;
using Cmune.DataCenter.Common.Entities;
using UberStrike.DataCenter.UnitySdk;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugApplication : IDebugPage
{
	private string config = "No config file loaded";

	public string Title
	{
		get
		{
			return "App";
		}
	}

	public void Draw()
	{
		GUILayout.Label("Channel: " + ApplicationDataManager.Channel);
		GUILayout.Label("Version: 4.4.2m");
		GUILayout.Label("BuildNumber: " + ApplicationDataManager.BuildNumber);
		GUILayout.Label("BuildType: " + ApplicationDataManager.BuildType);
		GUILayout.Label("Source: " + Application.srcValue);
		GUILayout.Label("WS API: " + UberStrike.DataCenter.UnitySdk.ApiVersion.Current);
		GUILayout.Label("RT API: " + UberStrike.Realtime.UnitySdk.ApiVersion.Current);
		if (PlayerDataManager.AccessLevel > MemberAccessLevel.Default)
		{
			GUILayout.Label("Member Name: " + PlayerDataManager.Name);
			GUILayout.Label("Member Cmid: " + PlayerDataManager.Cmid);
			GUILayout.Label("Member Access: " + PlayerDataManager.AccessLevel);
			GUILayout.Label("Member Tag: " + PlayerDataManager.ClanTag);
			foreach (GameServerView photonServer in Singleton<GameServerManager>.Instance.PhotonServerList)
			{
				GUILayout.Label("Game Server: " + photonServer.Name + " [" + photonServer.MinLatency + "] " + photonServer.Data.PeersConnected + "/" + photonServer.Data.PlayersConnected);
			}
		}
		GUILayout.Space(10f);
		GUILayout.Label("Path: " + ApplicationDataManager.ConfigPath);
		if (GUILayout.Button("Update"))
		{
			MonoRoutine.Start(LoadConfig());
		}
		GUILayout.TextArea(config);
	}

	private IEnumerator LoadConfig()
	{
		string path = ApplicationDataManager.ConfigPath;
		if (!string.IsNullOrEmpty(path))
		{
			WWW www = new WWW(path);
			yield return www;
			config = www.text;
		}
	}
}
