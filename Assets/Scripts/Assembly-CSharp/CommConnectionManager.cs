using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CommConnectionManager : AutoMonoBehaviour<CommConnectionManager>
{
	private PhotonClient _client;

	private ClientCommCenter _commCenter;

	private float _syncTime;

	private float _pollFriendsOnlineStatus;

	public static RemoteMethodInterface Rmi
	{
		get
		{
			return AutoMonoBehaviour<CommConnectionManager>.Instance._client.Rmi;
		}
	}

	public static PhotonClient Client
	{
		get
		{
			return AutoMonoBehaviour<CommConnectionManager>.Instance._client;
		}
	}

	public static int CurrentPlayerID
	{
		get
		{
			return AutoMonoBehaviour<CommConnectionManager>.Instance._client.PeerListener.ActorId;
		}
	}

	public static CmuneRoomID CurrentRoomID
	{
		get
		{
			return AutoMonoBehaviour<CommConnectionManager>.Instance._client.PeerListener.CurrentRoom;
		}
	}

	public static ClientCommCenter CommCenter
	{
		get
		{
			return AutoMonoBehaviour<CommConnectionManager>.Instance._commCenter;
		}
	}

	public static bool IsConnected
	{
		get
		{
			return AutoMonoBehaviour<CommConnectionManager>.Instance._client.IsConnected;
		}
	}

	private void Awake()
	{
		_client = new PhotonClient(true);
		_commCenter = new ClientCommCenter(_client.Rmi);
	}

	private new void Start()
	{
		GameConnectionManager.Client.PeerListener.SubscribeToEvents(OnGameConnectionChange);
		StartCoroutine(StartCheckingCommServerConnection());
		CmuneEventHandler.AddListener<LoginEvent>(OnLoginEvent);
	}

	private void OnLoginEvent(LoginEvent ev)
	{
		_commCenter.Login();
	}

	private void Update()
	{
		if (_client != null && _syncTime <= Time.time)
		{
			_syncTime = Time.time + 0.02f;
			_client.Update();
		}
		if (_pollFriendsOnlineStatus < Time.time)
		{
			_pollFriendsOnlineStatus = Time.time + 30f;
			if (MenuPageManager.Instance != null && (MenuPageManager.Instance.IsCurrentPage(PageType.Chat) || MenuPageManager.Instance.IsCurrentPage(PageType.Inbox) || MenuPageManager.Instance.IsCurrentPage(PageType.Clans) || MenuPageManager.Instance.IsCurrentPage(PageType.Home)))
			{
				CommCenter.UpdateContacts();
			}
		}
	}

	public static bool TryGetActor(int cmid, out CommActorInfo actor)
	{
		if (cmid > 0 && CommCenter != null)
		{
			return CommCenter.TryGetActorWithCmid(cmid, out actor) && actor != null;
		}
		actor = null;
		return false;
	}

	public static bool IsPlayerOnline(int cmid)
	{
		if (cmid > 0 && CommCenter != null)
		{
			return CommCenter.HasActorWithCmid(cmid);
		}
		return false;
	}

	public static void Reconnect()
	{
		Stop();
		AutoMonoBehaviour<CommConnectionManager>.Instance.Awake();
	}

	protected void OnApplicationQuit()
	{
		if (_client != null)
		{
			_client.ShutDown();
		}
	}

	private IEnumerator StartCheckingCommServerConnection()
	{
		while (true)
		{
			yield return new WaitForSeconds(5f);
			if (_client.ConnectionState == PhotonClient.ConnectionStatus.STOPPED && CmuneNetworkManager.CurrentCommServer.IsValid && PlayerDataManager.IsPlayerLoggedIn)
			{
				_client.ConnectToRoom(new RoomMetaData(88, "The CommServer", CmuneNetworkManager.CurrentCommServer.ConnectionString), PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
			}
		}
	}

	public static void Stop()
	{
		if (AutoMonoBehaviour<CommConnectionManager>.Instance._client != null)
		{
			AutoMonoBehaviour<CommConnectionManager>.Instance._commCenter.Leave();
			AutoMonoBehaviour<CommConnectionManager>.Instance._client.Disconnect();
		}
	}

	private void OnGameConnectionChange(PhotonPeerListener.ConnectionEvent ev)
	{
		if (_commCenter.IsInitialized)
		{
			switch (ev.Type)
			{
			case PhotonPeerListener.ConnectionEventType.JoinedRoom:
				_commCenter.UpdatePlayerRoom(ev.Room);
				break;
			case PhotonPeerListener.ConnectionEventType.LeftRoom:
				_commCenter.ResetPlayerRoom();
				break;
			}
		}
	}
}
