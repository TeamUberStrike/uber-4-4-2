using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupMenu
{
	private class MenuItem
	{
		public string Item;

		public Action<CommUser> Callback;

		public IsEnabledForUser CheckItem;

		public bool Enabled;
	}

	public delegate bool IsEnabledForUser(CommUser user);

	private const int Height = 24;

	private const int Width = 105;

	private Rect _position;

	private List<MenuItem> _items;

	private CommUser _selectedUser;

	public CommUser SelectedUser
	{
		get
		{
			return _selectedUser;
		}
	}

	public static PopupMenu Current { get; private set; }

	public static bool IsEnabled
	{
		get
		{
			return Current != null;
		}
	}

	public PopupMenu()
	{
		_items = new List<MenuItem>();
	}

	public void AddMenuItem(string item, Action<CommUser> action, IsEnabledForUser isEnabledForUser)
	{
		MenuItem menuItem = new MenuItem();
		menuItem.Item = item;
		menuItem.Callback = action;
		menuItem.CheckItem = isEnabledForUser;
		menuItem.Enabled = false;
		_items.Add(menuItem);
	}

	private void Configure()
	{
		for (int i = 0; i < _items.Count; i++)
		{
			_items[i].Enabled = _items[i].CheckItem(_selectedUser);
		}
	}

	public static void Hide()
	{
		Current = null;
	}

	public void Show(Vector2 screenPos, CommUser user)
	{
		Show(screenPos, user, this);
	}

	public static void Show(Vector2 screenPos, CommUser user, PopupMenu menu)
	{
		if (menu != null)
		{
			menu._selectedUser = user;
			menu.Configure();
			menu._position.height = 24 * menu._items.Count;
			menu._position.width = 105f;
			menu._position.x = screenPos.x - 1f;
			if (screenPos.y + menu._position.height > (float)Screen.height)
			{
				menu._position.y = screenPos.y - menu._position.height + 1f;
			}
			else
			{
				menu._position.y = screenPos.y - 1f;
			}
			Current = menu;
		}
	}

	public void Draw()
	{
		GUI.BeginGroup(new Rect(_position.x, _position.y, _position.width, _position.height + 6f), BlueStonez.window);
		GUI.Label(new Rect(1f, 1f, _position.width - 2f, _position.height + 4f), GUIContent.none, BlueStonez.box_grey50);
		GUI.Label(new Rect(0f, 0f, _position.width, _position.height + 6f), GUIContent.none, BlueStonez.box_grey50);
		for (int i = 0; i < _items.Count; i++)
		{
			GUITools.PushGUIState();
			GUI.enabled = _items[i].Enabled;
			GUI.Label(new Rect(8f, 8 + i * 24, _position.width - 8f, 24f), _items[i].Item, BlueStonez.label_interparkmed_11pt_left);
			if (GUI.Button(new Rect(2f, 3 + i * 24, _position.width - 4f, 24f), GUIContent.none, BlueStonez.dropdown_list))
			{
				Current = null;
				_items[i].Callback(_selectedUser);
			}
			GUITools.PopGUIState();
		}
		GUI.EndGroup();
		if (Event.current.type == EventType.MouseUp && !_position.Contains(Event.current.mousePosition))
		{
			Current = null;
		}
	}
}
