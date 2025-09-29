using System;
using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameConnectionManager : AutoMonoBehaviour<GameConnectionManager>
{
	private float _syncTime;

	private float _joinFullHackTime;

	private bool _reponseArrived;

	private bool _isConnectionStarted;

	private PhotonClient _client;

	private GameMetaData _gameRoom;

	private GameMetaData _requestedGameData = GameMetaData.Empty;

	public static bool IsConnected
	{
		get
		{
			return AutoMonoBehaviour<GameConnectionManager>.Instance._client.ConnectionState == PhotonClient.ConnectionStatus.RUNNING;
		}
	}

	public static RemoteMethodInterface Rmi
	{
		get
		{
			return AutoMonoBehaviour<GameConnectionManager>.Instance._client.Rmi;
		}
	}

	public static PhotonClient Client
	{
		get
		{
			return AutoMonoBehaviour<GameConnectionManager>.Instance._client;
		}
	}

	public static string CurrentRoomID
	{
		get
		{
			return AutoMonoBehaviour<GameConnectionManager>.Instance._client.PeerListener.CurrentRoom.ID;
		}
	}

	public static int CurrentPlayerID
	{
		get
		{
			return AutoMonoBehaviour<GameConnectionManager>.Instance._client.PeerListener.ActorId;
		}
	}

	public string GameID { get; set; }

	public string MatchID { get; set; }

	public int RoundsPlayed { get; set; }

	private void Awake()
	{
		_client = new PhotonClient(true);
		_client.PeerListener.SubscribeToEvents(OnEventCallback);
	}

	private void Update()
	{
		if (_client != null && _syncTime <= Time.realtimeSinceStartup)
		{
			_syncTime = Time.realtimeSinceStartup + 0.02f;
			_client.Update();
		}
	}

	private void OnGUI()
	{
		if (!GameState.HasCurrentGame || GameState.CurrentGame.NetworkID == -1 || Client == null || Client.ConnectionState == PhotonClient.ConnectionStatus.RUNNING)
		{
			return;
		}
		Rect position = new Rect((float)(Screen.width - 320) * 0.5f, (float)(Screen.height - 240 - 56) * 0.5f, 320f, 240f);
		GUI.BeginGroup(position, GUIContent.none, BlueStonez.window_standard_grey38);
		GUI.Label(new Rect(0f, 0f, 320f, 56f), LocalizedStrings.PleaseWait, BlueStonez.tab_strip);
		if (Client.ConnectionState == PhotonClient.ConnectionStatus.STARTING)
		{
			GUI.Button(new Rect(17f, 55f, 286f, 140f), LocalizedStrings.ConnectingToServer, BlueStonez.label_interparkbold_11pt);
		}
		else if (Client.ConnectionState == PhotonClient.ConnectionStatus.STOPPED)
		{
			GUI.Button(new Rect(17f, 55f, 286f, 140f), LocalizedStrings.ServerError, BlueStonez.label_interparkbold_11pt);
			if (GUITools.Button(new Rect(100f, 200f, 120f, 32f), new GUIContent(LocalizedStrings.OkCaps), BlueStonez.button))
			{
				Singleton<GameStateController>.Instance.UnloadGameMode();
			}
		}
		GUI.EndGroup();
	}

	private void Reconnect()
	{
		StartCoroutine(StartReconnectionInSeconds(1));
	}

	private void CloseGame()
	{
		Singleton<GameStateController>.Instance.LeaveGame();
	}

	private void CloseGameAndBackToServerSelection()
	{
		Singleton<GameStateController>.Instance.LeaveGame();
	}

	private void OnRequestRoomMetaDataCallback(int result, object[] table)
	{
		_reponseArrived = true;
		if (table.Length > 0)
		{
			_requestedGameData = (GameMetaData)table[0];
		}
	}

	private void OnEventCallback(PhotonPeerListener.ConnectionEvent ev)
	{
		switch (ev.Type)
		{
		case PhotonPeerListener.ConnectionEventType.JoinFailed:
			_joinFullHackTime = Time.time + 2f;
			switch (ev.ErrorCode)
			{
			case 3:
				ApplicationDataManager.EventsSystem.SendJoinFailed(PlayerDataManager.CmidSecure, ev.Guid, "player-banned");
				PopupSystem.ShowMessage("Connection Rejected", "You are not currently permitted to join this game.", PopupSystem.AlertType.OK, CloseGameAndBackToServerSelection);
				break;
			case 4:
				ApplicationDataManager.EventsSystem.SendJoinFailed(PlayerDataManager.CmidSecure, ev.Guid, "server-full");
				PopupSystem.ShowMessage("Server Full", "The server is currently full!\nDo you want to try again?", PopupSystem.AlertType.OKCancel, Reconnect, CloseGame);
				break;
			default:
				ApplicationDataManager.EventsSystem.SendJoinFailed(PlayerDataManager.CmidSecure, ev.Guid, "game-full");
				PopupSystem.ShowMessage("Game Full", "The game is currently full!\nDo you want to try again?", PopupSystem.AlertType.OKCancel, Reconnect, CloseGame);
				break;
			}
			break;
		case PhotonPeerListener.ConnectionEventType.Disconnected:
			if (_isConnectionStarted && _joinFullHackTime < Time.time)
			{
				ApplicationDataManager.EventsSystem.SendGameDisconnected(PlayerDataManager.CmidSecure, ev.Guid, "disconnected");
				PopupSystem.ShowMessage("Connection Error", "You lost the connection to our server!\nDo you want to reconnect?", PopupSystem.AlertType.OKCancel, Reconnect, CloseGameAndBackToServerSelection);
				Screen.lockCursor = false;
			}
			break;
		case PhotonPeerListener.ConnectionEventType.JoinedRoom:
			if (_gameRoom != null)
			{
				RoundsPlayed = 0;
				_gameRoom.RoomID = ev.Room;
				GameID = ev.Guid;
				ApplicationDataManager.EventsSystem.SendJoinGame(PlayerDataManager.CmidSecure, ev.Guid, _gameRoom.MapID, _gameRoom.RoundTime, _gameRoom.GameMode, _gameRoom.SplatLimit);
			}
			break;
		case PhotonPeerListener.ConnectionEventType.LeftRoom:
		case PhotonPeerListener.ConnectionEventType.OtherJoined:
		case PhotonPeerListener.ConnectionEventType.OtherLeft:
			break;
		}
	}

	protected void OnApplicationQuit()
	{
		if (_client != null)
		{
			_client.ShutDown();
		}
	}

	private IEnumerator StartReconnectionInSeconds(int seconds)
	{
		yield return new WaitForSeconds(seconds);
		if (_gameRoom != null)
		{
			_client.ConnectToRoom(_gameRoom, PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
		}
		else
		{
			Debug.LogError("Failed to reconnect because GameRoom is null!");
		}
	}

	private IEnumerator StartRequestRoomMetaData(CmuneRoomID room, Action<int, GameMetaData> action)
	{
		_gameRoom = null;
		_isConnectionStarted = false;
		if (GameState.HasCurrentGame)
		{
			GameState.CurrentGame.Leave();
		}
		yield return Client.Disconnect();
		yield return new WaitForEndOfFrame();
		_isConnectionStarted = true;
		yield return Client.ConnectToServer(room.Server, PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
		if (Client.IsConnected)
		{
			Client.Rmi.Messenger.SendOperationToServerApplication(OnRequestRoomMetaDataCallback, 21, room.Number);
			float timeout = 5f;
			_reponseArrived = false;
			while (!_reponseArrived && timeout > 0f)
			{
				yield return new WaitForSeconds(0.1f);
				timeout -= 0.1f;
			}
			if (_reponseArrived && !_requestedGameData.RoomID.IsEmpty)
			{
				if (action != null)
				{
					action(0, _requestedGameData);
				}
			}
			else if (action != null)
			{
				action(1, GameMetaData.Empty);
			}
		}
		else if (action != null)
		{
			action(2, GameMetaData.Empty);
		}
	}

	public static void Start(GameMetaData game)
	{
		AutoMonoBehaviour<GameConnectionManager>.Instance._isConnectionStarted = true;
		AutoMonoBehaviour<GameConnectionManager>.Instance._gameRoom = game;
		AutoMonoBehaviour<GameConnectionManager>.Instance._client.ConnectToRoom(game, PlayerDataManager.AuthToken, ApplicationDataManager.EventsSystem.SessionId);
	}

	public static void Stop()
	{
		if (GameState.HasCurrentGame)
		{
			GameState.CurrentGame.Leave();
		}
		AutoMonoBehaviour<GameConnectionManager>.Instance._gameRoom = null;
		AutoMonoBehaviour<GameConnectionManager>.Instance._isConnectionStarted = false;
		AutoMonoBehaviour<GameConnectionManager>.Instance._client.Disconnect();
	}

	public static void RequestRoomMetaData(CmuneRoomID room, Action<int, GameMetaData> action)
	{
		AutoMonoBehaviour<GameConnectionManager>.Instance.StartCoroutine(AutoMonoBehaviour<GameConnectionManager>.Instance.StartRequestRoomMetaData(room, action));
	}

	public bool IsConnectedToServer(string server)
	{
		return _client.IsConnected && _client.CurrentConnection == server;
	}
}
