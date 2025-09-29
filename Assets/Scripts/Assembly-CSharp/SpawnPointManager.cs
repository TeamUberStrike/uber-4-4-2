using System;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class SpawnPointManager : Singleton<SpawnPointManager>
{
	private static readonly Vector3 DefaultSpawnPoint = new Vector3(0f, 6f, 0f);

	private IDictionary<GameMode, IDictionary<TeamID, IList<SpawnPoint>>> _spawnPointsDictionary;

	private SpawnPointManager()
	{
		_spawnPointsDictionary = new Dictionary<GameMode, IDictionary<TeamID, IList<SpawnPoint>>>();
		foreach (int value in Enum.GetValues(typeof(GameMode)))
		{
			_spawnPointsDictionary[(GameMode)value] = new Dictionary<TeamID, IList<SpawnPoint>>
			{
				{
					TeamID.BLUE,
					new List<SpawnPoint>()
				},
				{
					TeamID.RED,
					new List<SpawnPoint>()
				},
				{
					TeamID.NONE,
					new List<SpawnPoint>()
				}
			};
		}
	}

	private void Clear()
	{
		foreach (int value in Enum.GetValues(typeof(GameMode)))
		{
			_spawnPointsDictionary[(GameMode)value][TeamID.NONE].Clear();
			_spawnPointsDictionary[(GameMode)value][TeamID.BLUE].Clear();
			_spawnPointsDictionary[(GameMode)value][TeamID.RED].Clear();
		}
	}

	private bool TryGetSpawnPointAt(int index, GameMode gameMode, TeamID teamID, out SpawnPoint point)
	{
		point = ((index >= GetSpawnPointList(gameMode, teamID).Count) ? null : GetSpawnPointList(gameMode, teamID)[index]);
		return point != null;
	}

	private bool TryGetRandomSpawnPoint(GameMode gameMode, TeamID teamID, out SpawnPoint point)
	{
		IList<SpawnPoint> spawnPointList = GetSpawnPointList(gameMode, teamID);
		point = ((spawnPointList.Count <= 0) ? null : spawnPointList[UnityEngine.Random.Range(0, spawnPointList.Count)]);
		return point != null;
	}

	private IList<SpawnPoint> GetSpawnPointList(GameMode gameMode, TeamID team)
	{
		if (gameMode == GameMode.Training)
		{
			return _spawnPointsDictionary[GameMode.DeathMatch][TeamID.NONE];
		}
		return _spawnPointsDictionary[gameMode][team];
	}

	public void ConfigureSpawnPoints(SpawnPoint[] points)
	{
		Clear();
		foreach (SpawnPoint spawnPoint in points)
		{
			if (_spawnPointsDictionary.ContainsKey(spawnPoint.GameMode))
			{
				_spawnPointsDictionary[spawnPoint.GameMode][spawnPoint.TeamId].Add(spawnPoint);
			}
		}
	}

	public int GetSpawnPointCount(GameMode gameMode, TeamID team)
	{
		return GetSpawnPointList(gameMode, team).Count;
	}

	public void GetSpawnPointAt(int index, GameMode gameMode, TeamID team, out Vector3 position, out Quaternion rotation)
	{
		SpawnPoint point;
		if (TryGetSpawnPointAt(index, gameMode, team, out point))
		{
			position = point.transform.position;
			rotation = point.transform.rotation;
			return;
		}
		Debug.LogError("No spawnpoints found at " + index + " int list of length " + GetSpawnPointCount(gameMode, team));
		position = DefaultSpawnPoint;
		rotation = Quaternion.identity;
	}

	public void GetRandomSpawnPoint(out Vector3 position, out Quaternion rotation)
	{
		IList<SpawnPoint> list = _spawnPointsDictionary[GameMode.DeathMatch][TeamID.NONE];
		SpawnPoint spawnPoint = list[UnityEngine.Random.Range(0, list.Count)];
		position = spawnPoint.transform.position;
		rotation = spawnPoint.transform.rotation;
	}

	public List<SpawnPoint> GetAllSpawnPoints()
	{
		List<SpawnPoint> list = new List<SpawnPoint>();
		foreach (IDictionary<TeamID, IList<SpawnPoint>> value in _spawnPointsDictionary.Values)
		{
			foreach (IList<SpawnPoint> value2 in value.Values)
			{
				list.AddRange(value2);
			}
		}
		return list;
	}
}
