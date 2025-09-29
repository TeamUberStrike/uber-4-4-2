using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoaderProfiler : MonoBehaviour
{
	private static int proxyItemsCount;

	private static Dictionary<string, double> loadTime;

	private static bool waitingForAllItems;

	private static DateTime beginTime;

	private static TimeSpan spentTime;

	private static ItemLoaderProfiler instance;

	static ItemLoaderProfiler()
	{
		loadTime = new Dictionary<string, double>();
	}

	public static void Record(string name, double timeMs)
	{
		if (!instance)
		{
			GameObject gameObject = new GameObject("Profiler");
			instance = gameObject.AddComponent<ItemLoaderProfiler>();
		}
		loadTime[name] = timeMs;
		if (loadTime.Count == proxyItemsCount)
		{
			waitingForAllItems = false;
			spentTime = DateTime.Now.Subtract(beginTime);
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnGUI()
	{
		Rect position = new Rect(20f, 100f, 200f, 60f);
		if (waitingForAllItems)
		{
			GUI.Label(position, string.Format("{0}/{1}", loadTime.Count, proxyItemsCount));
			if (GUI.Button(position, "Export"))
			{
				Export();
			}
		}
		else if (loadTime.Count != proxyItemsCount && GUI.Button(position, "Load All Items"))
		{
			LoadAll();
			waitingForAllItems = true;
		}
	}

	private void LoadAll()
	{
		beginTime = DateTime.Now;
		foreach (IUnityItem shopItem in Singleton<ItemManager>.Instance.ShopItems)
		{
			if (shopItem is ProxyItem)
			{
				shopItem.Create(Vector3.zero, Quaternion.identity);
				proxyItemsCount++;
			}
		}
	}

	private void Export()
	{
	}
}
