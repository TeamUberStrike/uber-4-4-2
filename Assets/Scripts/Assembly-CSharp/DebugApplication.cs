using System.Collections;
using Cmune.DataCenter.Common.Entities;
using UberStrike.DataCenter.UnitySdk;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;
using UnityEngine.Networking;

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
		// Application.srcValue is obsolete, show URL query string for WebGL
		string sourceValue = "";
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			string url = Application.absoluteURL;
			int queryIndex = url.IndexOf('?');
			if (queryIndex >= 0)
			{
				sourceValue = url.Substring(queryIndex + 1);
			}
		}
		GUILayout.Label("Source: " + sourceValue);
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
			UnityWebRequest www = UnityWebRequest.Get(path);
			yield return www.SendWebRequest();
			if (www.result == UnityWebRequest.Result.Success)
			{
				config = www.downloadHandler.text;
			}
			else
			{
				config = "Error loading config: " + www.error;
			}
		}
	}
}
