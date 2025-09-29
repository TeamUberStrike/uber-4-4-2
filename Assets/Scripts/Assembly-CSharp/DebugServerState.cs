using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugServerState : IDebugPage
{
	public string Title
	{
		get
		{
			return "Servers";
		}
	}

	public void Draw()
	{
		GUILayout.Label("ALL SERVERS");
		foreach (GameServerView photonServer in Singleton<GameServerManager>.Instance.PhotonServerList)
		{
			GUILayout.Label("  " + photonServer.ConnectionString + " " + photonServer.Latency);
		}
		if (Singleton<GameServerController>.Instance.SelectedServer != null)
		{
			GUILayout.Space(10f);
			GUILayout.Label(string.Format("GAMESERVER: {0}, isValid: {1}", Singleton<GameServerController>.Instance.SelectedServer.ConnectionString, Singleton<GameServerController>.Instance.SelectedServer.IsValid));
			GUILayout.Label("  Room ID: " + GameConnectionManager.CurrentRoomID);
			GUILayout.Label("  Player ID: " + GameConnectionManager.CurrentPlayerID);
			GUILayout.Label("  Network Time: " + GameConnectionManager.Client.PeerListener.ServerTimeTicks);
			GUILayout.Label("  KBytes IN: " + ConvertBytes.ToKiloBytes(GameConnectionManager.Client.PeerListener.IncomingBytes).ToString("f2"));
			GUILayout.Label("  KBytes OUT: " + ConvertBytes.ToKiloBytes(GameConnectionManager.Client.PeerListener.OutgoingBytes).ToString("f2"));
		}
		if (CmuneNetworkManager.CurrentLobbyServer != null)
		{
			GUILayout.Space(10f);
			GUILayout.Label(string.Format("LOBBYSERVER: {0}, isValid: {1}", CmuneNetworkManager.CurrentLobbyServer.ConnectionString, CmuneNetworkManager.CurrentLobbyServer.IsValid));
			GUILayout.Label("  Player ID: " + LobbyConnectionManager.CurrentPlayerID);
			GUILayout.Label("  Network Time: " + LobbyConnectionManager.Client.PeerListener.ServerTimeTicks);
			GUILayout.Label("  KBytes IN: " + ConvertBytes.ToKiloBytes(LobbyConnectionManager.Client.PeerListener.IncomingBytes).ToString("f2"));
			GUILayout.Label("  KBytes OUT: " + ConvertBytes.ToKiloBytes(LobbyConnectionManager.Client.PeerListener.OutgoingBytes).ToString("f2"));
		}
		if (CmuneNetworkManager.CurrentCommServer != null)
		{
			GUILayout.Space(10f);
			GUILayout.Label(string.Format("COMMSERVER: {0}, isValid: {1}", CmuneNetworkManager.CurrentCommServer.ConnectionString, CmuneNetworkManager.CurrentCommServer.IsValid));
			GUILayout.Label("  Player ID: " + CommConnectionManager.CurrentPlayerID);
			GUILayout.Label("  Network Time: " + CommConnectionManager.Client.PeerListener.ServerTimeTicks);
			GUILayout.Label("  KBytes IN: " + ConvertBytes.ToKiloBytes(CommConnectionManager.Client.PeerListener.IncomingBytes).ToString("f2"));
			GUILayout.Label("  KBytes OUT: " + ConvertBytes.ToKiloBytes(CommConnectionManager.Client.PeerListener.OutgoingBytes).ToString("f2"));
		}
	}
}
