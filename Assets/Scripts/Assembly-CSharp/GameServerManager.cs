using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.Core.Models.Views;
using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class GameServerManager : Singleton<GameServerManager>
{
	private const int ServerUpdateCycle = 30;

	private Dictionary<int, GameServerView> _gameServers = new Dictionary<int, GameServerView>();

	private List<GameServerView> _sortedServers = new List<GameServerView>();

	private IComparer<GameServerView> _comparer;

	private bool _reverseSorting;

	private Dictionary<int, ServerLoadRequest> _loadRequests = new Dictionary<int, ServerLoadRequest>();

	public int PhotonServerCount
	{
		get
		{
			return _gameServers.Count;
		}
	}

	public int AllPlayersCount { get; private set; }

	public int AllGamesCount { get; private set; }

	public IEnumerable<GameServerView> PhotonServerList
	{
		get
		{
			return _sortedServers;
		}
	}

	public IEnumerable<ServerLoadRequest> ServerRequests
	{
		get
		{
			return _loadRequests.Values;
		}
	}

	private GameServerManager()
	{
	}

	public void SortServers()
	{
		if (_comparer != null)
		{
			_sortedServers.Sort(_comparer);
			if (_reverseSorting)
			{
				_sortedServers.Reverse();
			}
		}
	}

	public GameServerView GetBestServer()
	{
		GameServerView bestServer = GetBestServer(ApplicationDataManager.IsMobile);
		if (ApplicationDataManager.IsMobile && bestServer == null)
		{
			bestServer = GetBestServer(false);
		}
		return bestServer;
	}

	private GameServerView GetBestServer(bool doMobileFilter)
	{
		List<GameServerView> list = new List<GameServerView>(_gameServers.Values);
		list.Sort((GameServerView s, GameServerView t) => s.Latency - t.Latency);
		GameServerView gameServerView = null;
		for (int num = 0; num < list.Count; num++)
		{
			GameServerView gameServerView2 = list[num];
			if (gameServerView2.Latency != 0 && (!doMobileFilter || gameServerView2.UsageType == PhotonUsageType.Mobile))
			{
				if (gameServerView == null && gameServerView2.CheckLatency())
				{
					gameServerView = gameServerView2;
				}
				else if (gameServerView2.CheckLatency() && gameServerView2.Latency < 200 && gameServerView.Data.PlayersConnected < gameServerView2.Data.PlayersConnected)
				{
					gameServerView = gameServerView2;
				}
			}
		}
		return gameServerView;
	}

	internal string GetServerName(string connection)
	{
		string result = string.Empty;
		foreach (GameServerView value in _gameServers.Values)
		{
			if (value.ConnectionString == connection)
			{
				result = value.Name;
				break;
			}
		}
		return result;
	}

	public void SortServers(IComparer<GameServerView> comparer, bool reverse = false)
	{
		_comparer = comparer;
		_reverseSorting = reverse;
		lock (_sortedServers)
		{
			_sortedServers.Clear();
			_sortedServers.AddRange(_gameServers.Values);
		}
		SortServers();
	}

	public void AddPhotonGameServer(PhotonView view)
	{
		_gameServers[view.PhotonId] = new GameServerView(view);
		if (view.MinLatency > 0)
		{
			view.Name = view.Name + " - " + view.MinLatency + "ms";
		}
		SortServers();
	}

	public void AddPhotonGameServers(List<PhotonView> servers)
	{
		foreach (PhotonView server in servers)
		{
			AddPhotonGameServer(server);
		}
	}

	public int GetServerLatency(string connection)
	{
		foreach (GameServerView value in _gameServers.Values)
		{
			if (value.ConnectionString == connection)
			{
				return value.Latency;
			}
		}
		return 0;
	}

	public IEnumerator StartUpdatingServerLoads()
	{
		foreach (GameServerView server in _gameServers.Values)
		{
			ServerLoadRequest request;
			if (!_loadRequests.TryGetValue(server.Id, out request))
			{
				request = ServerLoadRequest.Run(server, delegate
				{
					UpdateGamesAndPlayerCount();
					Singleton<GameListManager>.Instance.UpdateServerLatency(server.ConnectionString);
				});
				_loadRequests.Add(server.Id, request);
			}
			if (request.RequestState != ServerLoadRequest.RequestStateType.Waiting)
			{
				request.RunAgain();
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	public IEnumerator StartUpdatingLatency(Action<float> progressCallback)
	{
		yield return MonoRoutine.Start(StartUpdatingServerLoads());
		float minTimeout = Time.time + 4f;
		float maxTimeout = Time.time + 10f;
		int count = 0;
		while (count != _loadRequests.Count)
		{
			yield return new WaitForSeconds(1f);
			count = 0;
			foreach (ServerLoadRequest r in _loadRequests.Values)
			{
				if (r.RequestState != ServerLoadRequest.RequestStateType.Waiting)
				{
					count++;
				}
			}
			progressCallback((float)count / (float)_loadRequests.Count);
			if ((count > 0 && Time.time > minTimeout) || Time.time > maxTimeout)
			{
				break;
			}
		}
	}

	private void UpdateGamesAndPlayerCount()
	{
		AllPlayersCount = 0;
		AllGamesCount = 0;
		foreach (GameServerView value in _gameServers.Values)
		{
			AllPlayersCount += value.Data.PlayersConnected;
			AllGamesCount += value.Data.RoomsCreated;
		}
		SortServers();
	}
}
