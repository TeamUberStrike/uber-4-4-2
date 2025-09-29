using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ItemLoader : Singleton<ItemLoader>
{
	private bool _isWorking;

	private Dictionary<string, ItemLoadingTask> allTasks;

	private List<ItemLoadingTask> highPriorityTasks;

	private List<ItemLoadingTask> lowPriorityTasks;

	public bool Initialized { get; private set; }

	public bool Blocked { get; private set; }

	public bool Paused { get; set; }

	private bool HasTask
	{
		get
		{
			return highPriorityTasks.Count > 0 || lowPriorityTasks.Count > 0;
		}
	}

	private bool Sorted { get; set; }

	private ItemLoader()
	{
		highPriorityTasks = new List<ItemLoadingTask>();
		lowPriorityTasks = new List<ItemLoadingTask>();
		allTasks = new Dictionary<string, ItemLoadingTask>();
	}

	public void AddTask(string prefabName, string version, Action<AssetBundle> OnLoaded = null)
	{
		string key = prefabName + version;
		ItemLoadingTask value;
		if (allTasks.TryGetValue(key, out value))
		{
			if (OnLoaded != null)
			{
				value.OnLoaded += OnLoaded;
			}
			return;
		}
		value = new ItemLoadingTask(prefabName, version, OnLoaded);
		allTasks.Add(key, value);
		lowPriorityTasks.Add(value);
		if (!_isWorking)
		{
			MonoRoutine.Start(StartLoading());
		}
	}

	public void SetMustLoadItems(List<string> prefabNames)
	{
		Sorted = false;
		string name;
		foreach (string prefabName in prefabNames)
		{
			name = prefabName;
			int num = lowPriorityTasks.FindIndex((ItemLoadingTask task) => task.PrefabName.Equals(name));
			if (num >= 0)
			{
				highPriorityTasks.Add(lowPriorityTasks[num]);
				lowPriorityTasks.RemoveAt(num);
			}
		}
		Sorted = true;
		Blocked = highPriorityTasks.Count > 0;
	}

	public void SetFirstToLoadItems(List<string> prefabNames)
	{
		Sorted = false;
		if (allTasks.Count == 0)
		{
			return;
		}
		string name;
		foreach (string prefabName in prefabNames)
		{
			name = prefabName;
			if (!string.IsNullOrEmpty(name))
			{
				int num = lowPriorityTasks.FindIndex((ItemLoadingTask task) => task.PrefabName.Equals(name));
				if (num >= 0)
				{
					ItemLoadingTask item = lowPriorityTasks[num];
					lowPriorityTasks.RemoveAt(num);
					lowPriorityTasks.Insert(0, item);
				}
			}
		}
		Sorted = true;
	}

	public void UnloadAll()
	{
		foreach (IUnityItem allShopItem in Singleton<ItemManager>.Instance.GetAllShopItems())
		{
			if (!Singleton<InventoryManager>.Instance.Contains(allShopItem.View.ID))
			{
				allShopItem.Unload();
			}
		}
	}

	private IEnumerator StartLoading()
	{
		Sorted = true;
		Initialized = true;
		_isWorking = true;
		while (HasTask)
		{
			if (highPriorityTasks.Count > 0)
			{
				ItemLoadingTask task = highPriorityTasks[0];
				highPriorityTasks.RemoveAt(0);
				yield return MonoRoutine.Start(task.StartLoading());
				allTasks.Remove(task.PrefabName);
				if (highPriorityTasks.Count == 0)
				{
					Blocked = false;
				}
			}
			else if (Paused)
			{
				yield return new WaitForEndOfFrame();
			}
			else if (lowPriorityTasks.Count > 0)
			{
				if (Sorted)
				{
					ItemLoadingTask task2 = lowPriorityTasks[0];
					lowPriorityTasks.RemoveAt(0);
					yield return MonoRoutine.Start(task2.StartLoading());
					allTasks.Remove(task2.PrefabName);
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		_isWorking = false;
		allTasks.Clear();
	}
}
