using System.Collections.Generic;
using UnityEngine;

public class GamePageManager : MonoBehaviour
{
	private static IDictionary<PageType, PageScene> _pageByPageType;

	private static PageType _currentPageType;

	public static GamePageManager Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public bool HasPage
	{
		get
		{
			return _currentPageType != PageType.None;
		}
	}

	private void Awake()
	{
		Instance = this;
		_pageByPageType = new Dictionary<PageType, PageScene>();
	}

	private void Start()
	{
		PageScene[] componentsInChildren = GetComponentsInChildren<PageScene>(true);
		foreach (PageScene pageScene in componentsInChildren)
		{
			_pageByPageType[pageScene.PageType] = pageScene;
		}
	}

	public static bool IsCurrentPage(PageType type)
	{
		return _currentPageType == type;
	}

	public PageScene GetCurrentPage()
	{
		PageScene value;
		_pageByPageType.TryGetValue(_currentPageType, out value);
		return value;
	}

	public void UnloadCurrentPage()
	{
		PageScene currentPage = GetCurrentPage();
		if ((bool)currentPage)
		{
			currentPage.Unload();
			_currentPageType = PageType.None;
		}
	}

	public void LoadPage(PageType pageType)
	{
		if (pageType == _currentPageType)
		{
			return;
		}
		PageScene value = null;
		if (_pageByPageType.TryGetValue(pageType, out value))
		{
			PageScene value2 = null;
			_pageByPageType.TryGetValue(_currentPageType, out value2);
			if ((bool)value2)
			{
				value2.Unload();
			}
			_currentPageType = pageType;
			value.Load();
		}
	}
}
