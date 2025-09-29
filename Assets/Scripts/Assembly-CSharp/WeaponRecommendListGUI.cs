using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class WeaponRecommendListGUI
{
	private bool _enabled;

	private GUIStyle _selectionStyle;

	private GUIStyle _normalStyle;

	private List<KeyValuePair<RecommendType, BaseItemGUI>> _recommendedItemList;

	private IUnityItem _selectedItem;

	private BuyingLocationType _location;

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (value == _enabled)
			{
				return;
			}
			_enabled = value;
			if (value)
			{
				if (_selectedItem == null && _recommendedItemList.Count > 0)
				{
					SetSelection(_recommendedItemList[0].Value.Item);
				}
				if (_selectionStyle == null)
				{
					_selectionStyle = new GUIStyle(StormFront.GrayPanelBox);
					_selectionStyle.overflow.left = 6;
				}
				if (_normalStyle == null)
				{
					_normalStyle = new GUIStyle(StormFront.GrayPanelBlankBox);
					_normalStyle.overflow.left = 5;
				}
				CmuneEventHandler.AddListener<SelectShopItemEvent>(OnSelectItem);
			}
			else
			{
				CmuneEventHandler.RemoveListener<SelectShopItemEvent>(OnSelectItem);
				ClearSelection();
			}
		}
	}

	public Action<IUnityItem, RecommendType> OnSelectionChange { get; set; }

	public IUnityItem SelectedItem
	{
		get
		{
			return _selectedItem;
		}
	}

	public WeaponRecommendListGUI(BuyingLocationType location)
	{
		_recommendedItemList = new List<KeyValuePair<RecommendType, BaseItemGUI>>();
		_location = location;
	}

	public void ClearSelection()
	{
		SetSelection(null);
	}

	public void Draw(Rect rect)
	{
		if (Enabled)
		{
			DrawRecommendList(rect);
		}
	}

	public void UpdateRecommendedList(IEnumerable<KeyValuePair<RecommendType, IUnityItem>> recomendations)
	{
		_recommendedItemList.Clear();
		foreach (KeyValuePair<RecommendType, IUnityItem> recomendation in recomendations)
		{
			try
			{
				_recommendedItemList.Add(new KeyValuePair<RecommendType, BaseItemGUI>(recomendation.Key, new InGameItemGUI(recomendation.Value, ShopUtils.GetRecommendationString(recomendation.Key), _location, (recomendation.Key == RecommendType.StaffPick) ? BuyingRecommendationType.Manual : BuyingRecommendationType.Behavior)));
			}
			catch (Exception ex)
			{
				Debug.LogError("Couldn't add item to recommendation list, it was null.\n\n" + ex.Message);
			}
		}
	}

	private void OnSelectItem(SelectShopItemEvent ev)
	{
		if (_selectedItem != ev.Item)
		{
			SetSelection(ev.Item);
		}
	}

	private void SetSelection(IUnityItem item)
	{
		_selectedItem = item;
		foreach (KeyValuePair<RecommendType, BaseItemGUI> recommendedItem in _recommendedItemList)
		{
			if (recommendedItem.Value.Item == _selectedItem && OnSelectionChange != null)
			{
				OnSelectionChange(_selectedItem, recommendedItem.Key);
				break;
			}
		}
	}

	private void DrawRecommendList(Rect rect)
	{
		if (_recommendedItemList.Count <= 0)
		{
			GUI.Label(rect, "Nothing to recommend", BlueStonez.label_interparkbold_11pt);
			return;
		}
		GUI.BeginGroup(rect);
		float num = rect.height / (float)_recommendedItemList.Count;
		Rect rect2 = new Rect(5f, 0f, rect.width - 10f, num);
		for (int i = 0; i < _recommendedItemList.Count; i++)
		{
			IUnityItem item = _recommendedItemList[i].Value.Item;
			if (_selectedItem == item)
			{
				GUI.Label(new Rect(rect2.x, rect2.y, rect.width - 5f, num), GUIContent.none, _selectionStyle);
			}
			else
			{
				GUI.Label(new Rect(rect2.x, rect2.y, rect.width - 5f, num), GUIContent.none, _normalStyle);
			}
			_recommendedItemList[i].Value.Draw(rect2, false);
			rect2.y += num - 1f;
		}
		GUI.EndGroup();
	}
}
