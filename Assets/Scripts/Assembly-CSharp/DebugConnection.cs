using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugConnection : IDebugPage
{
	public string Title
	{
		get
		{
			return "Connection";
		}
	}

	public void Draw()
	{
		if (GUI.Button(new Rect(20f, 70f, 150f, 20f), "Comm Disconnect"))
		{
			CommConnectionManager.Stop();
		}
		if (GUI.Button(new Rect(180f, 70f, 150f, 20f), "Comm Connect"))
		{
			CommConnectionManager.Client.ConnectToRoom(new RoomMetaData(88, string.Empty, CmuneNetworkManager.CurrentCommServer.ConnectionString), PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
		}
		GUI.Label(new Rect(360f, 70f, 600f, 20f), CommConnectionManager.Client.Debug);
		if (GUI.Button(new Rect(20f, 100f, 150f, 20f), "Lobby Disconnect"))
		{
			LobbyConnectionManager.Stop();
		}
		if (GUI.Button(new Rect(180f, 100f, 150f, 20f), "Lobby Connect"))
		{
			LobbyConnectionManager.Client.ConnectToRoom(new RoomMetaData(66, string.Empty, CmuneNetworkManager.CurrentLobbyServer.ConnectionString), PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
		}
		GUI.Label(new Rect(360f, 100f, 600f, 20f), LobbyConnectionManager.Client.Debug);
		if (GUI.Button(new Rect(20f, 130f, 150f, 20f), "Game Disconnect"))
		{
			GameConnectionManager.Stop();
		}
		if (GUI.Button(new Rect(180f, 130f, 150f, 20f), "Game Connect"))
		{
			GameConnectionManager.Client.ConnectToRoom(new GameMetaData(0, string.Empty, Singleton<GameServerController>.Instance.SelectedServer.ConnectionString), PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
		}
		GUI.Label(new Rect(360f, 130f, 600f, 20f), GameConnectionManager.Client.Debug);
	}
}
