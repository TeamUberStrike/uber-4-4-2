using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : Singleton<ProjectileManager>
{
	private Dictionary<int, IProjectile> _projectiles;

	private List<int> _limitedProjectiles;

	public static GameObject ProjectileContainer { get; set; }

	public IEnumerable<KeyValuePair<int, IProjectile>> AllProjectiles
	{
		get
		{
			return _projectiles;
		}
	}

	public IEnumerable<int> LimitedProjectiles
	{
		get
		{
			return _limitedProjectiles;
		}
	}

	private ProjectileManager()
	{
		_projectiles = new Dictionary<int, IProjectile>();
		_limitedProjectiles = new List<int>();
	}

	public static void CreateContainer()
	{
		if ((bool)ProjectileContainer)
		{
			DestroyContainer();
		}
		ProjectileContainer = new GameObject();
		ProjectileContainer.name = "Projectiles";
	}

	public static void DestroyContainer()
	{
		UnityEngine.Object.Destroy(ProjectileContainer);
	}

	public void AddProjectile(IProjectile p, int id)
	{
		if (p != null)
		{
			p.ID = id;
			_projectiles.Add(p.ID, p);
		}
	}

	public void AddLimitedProjectile(IProjectile p, int id, int count)
	{
		if (p != null)
		{
			p.ID = id;
			_projectiles.Add(p.ID, p);
			_limitedProjectiles.Add(p.ID);
			CheckLimitedProjectiles(count);
		}
	}

	private void CheckLimitedProjectiles(int count)
	{
		int[] array = _limitedProjectiles.ToArray();
		for (int i = 0; i < _limitedProjectiles.Count - count; i++)
		{
			RemoveProjectile(array[i]);
			GameState.CurrentGame.RemoveProjectile(array[i], true);
		}
	}

	public void RemoveAllLimitedProjectiles(bool explode = true)
	{
		int[] array = _limitedProjectiles.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			RemoveProjectile(array[i], explode);
			GameState.CurrentGame.RemoveProjectile(array[i], explode);
		}
	}

	public void RemoveProjectile(int id, bool explode = true)
	{
		try
		{
			IProjectile value;
			if (_projectiles.TryGetValue(id, out value))
			{
				if (explode)
				{
					value.Explode();
				}
				else
				{
					value.Destroy();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
		finally
		{
			_limitedProjectiles.RemoveAll((int i) => i == id);
			_projectiles.Remove(id);
		}
	}

	public void RemoveAllProjectilesFromPlayer(byte playerNumber)
	{
		int[] array = _projectiles.KeyArray();
		foreach (int num in array)
		{
			if ((num & 0xFF) == playerNumber)
			{
				RemoveProjectile(num, false);
			}
		}
	}

	public void ClearAll()
	{
		try
		{
			foreach (KeyValuePair<int, IProjectile> projectile in _projectiles)
			{
				if (projectile.Value != null)
				{
					projectile.Value.Destroy();
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
		}
		finally
		{
			_projectiles.Clear();
		}
	}

	public static int CreateGlobalProjectileID(byte playerNumber, int localProjectileId)
	{
		return (localProjectileId << 8) + playerNumber;
	}

	public static string PrintID(int id)
	{
		return GetPlayerId(id) + "/" + (id >> 8);
	}

	private static int GetPlayerId(int projectileId)
	{
		return projectileId & 0xFF;
	}
}
