using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;

public class GameListManager : Singleton<GameListManager>
{
	private Dictionary<CmuneRoomID, GameMetaData> _gameList = new Dictionary<CmuneRoomID, GameMetaData>();

	public int PlayersCount { get; private set; }

	public IEnumerable<GameMetaData> GameList
	{
		get
		{
			return _gameList.Values;
		}
	}

	public int GamesCount
	{
		get
		{
			return _gameList.Count;
		}
	}

	private GameListManager()
	{
		CmuneEventHandler.AddListener<RoomListUpdatedEvent>(OnGameListUpdated);
	}

	public void Init()
	{
	}

	private void OnGameListUpdated(RoomListUpdatedEvent ev)
	{
		PlayersCount = 0;
		_gameList.Clear();
		foreach (GameMetaData room in ev.Rooms)
		{
			if (room != null)
			{
				PlayersCount += room.ConnectedPlayers;
				room.Latency = Singleton<GameServerManager>.Instance.GetServerLatency(room.ServerConnection);
				_gameList[room.RoomID] = room;
			}
		}
		if (ev.IsInitialList && PlayPageGUI.Instance != null && ((MenuPageManager.Instance != null) & MenuPageManager.Instance.IsCurrentPage(PageType.Play)))
		{
			PlayPageGUI.Instance.RefreshGameList();
		}
	}

	public void UpdateServerLatency(string serverConnection)
	{
		foreach (GameMetaData value in _gameList.Values)
		{
			if (value.ServerConnection == serverConnection)
			{
				value.Latency = Singleton<GameServerManager>.Instance.GetServerLatency(serverConnection);
			}
		}
	}

	public void ClearGameList()
	{
		_gameList.Clear();
	}

	public bool TryGetGame(CmuneRoomID id, out GameMetaData game)
	{
		return _gameList.TryGetValue(id, out game);
	}
}
