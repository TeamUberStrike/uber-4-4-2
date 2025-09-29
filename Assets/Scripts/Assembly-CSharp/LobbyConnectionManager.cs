using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class LobbyConnectionManager : AutoMonoBehaviour<LobbyConnectionManager>
{
	private float _syncTime;

	private PhotonClient _client;

	private ClientLobbyCenter _lobbyCenter;

	public static RemoteMethodInterface Rmi
	{
		get
		{
			return AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.Rmi;
		}
	}

	public static PhotonClient Client
	{
		get
		{
			return AutoMonoBehaviour<LobbyConnectionManager>.Instance._client;
		}
	}

	public static int CurrentPlayerID
	{
		get
		{
			return AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.PeerListener.ActorId;
		}
	}

	public static bool IsConnected
	{
		get
		{
			return AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.IsConnected;
		}
	}

	public static bool IsConnecting
	{
		get
		{
			return AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.ConnectionState == PhotonClient.ConnectionStatus.STARTING;
		}
	}

	public static bool IsInLobby
	{
		get
		{
			return AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.PeerListener.HasJoinedRoom;
		}
	}

	private void Awake()
	{
		_client = new PhotonClient(true);
		_lobbyCenter = new ClientLobbyCenter(_client.Rmi);
		_client.PeerListener.SubscribeToEvents(OnEventCallback);
	}

	private void OnEventCallback(PhotonPeerListener.ConnectionEvent ev)
	{
		PhotonPeerListener.ConnectionEventType type = ev.Type;
		if (type == PhotonPeerListener.ConnectionEventType.Disconnected)
		{
			Singleton<GameListManager>.Instance.ClearGameList();
		}
	}

	public static void StartConnection()
	{
		if (!AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.IsConnected && CmuneNetworkManager.CurrentLobbyServer.IsValid)
		{
			RoomMetaData room = new RoomMetaData(66, "The Lobby", CmuneNetworkManager.CurrentLobbyServer.ConnectionString);
			AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.ConnectToRoom(room, PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
		}
	}

	public static void Reconnect()
	{
		Stop();
		AutoMonoBehaviour<LobbyConnectionManager>.Instance.Awake();
	}

	public static void Stop()
	{
		if (AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.IsConnected)
		{
			AutoMonoBehaviour<LobbyConnectionManager>.Instance._lobbyCenter.Leave();
			AutoMonoBehaviour<LobbyConnectionManager>.Instance._client.Disconnect();
		}
	}

	private void Update()
	{
		if (_client != null && _syncTime <= Time.time)
		{
			_syncTime = Time.time + 0.02f;
			_client.Update();
		}
	}

	protected void OnApplicationQuit()
	{
		if (_client != null)
		{
			_client.ShutDown();
		}
	}
}
