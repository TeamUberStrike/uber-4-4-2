using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class EndOfRoundPageGUI : PageGUI
{
	private const float WeaponRecommendHeight = 265f;

	private WeaponDetailGUI _weaponDetailGui;

	private ValuablePlayerDetailGUI _playerDetailGui;

	private ValuablePlayerListGUI _playerListGui;

	private WeaponRecommendListGUI _weaponRecomGui;

	public override void DrawGUI(Rect rect)
	{
		float height = Mathf.Min(_playerListGui.Height, rect.height - 265f) - 2f;
		float num = Mathf.Min(_playerListGui.Height, rect.height - 265f);
		GUI.BeginGroup(rect, GUIContent.none, BlueStonez.window_standard_grey38);
		_playerListGui.Draw(new Rect(2f, 2f, rect.width - 4f, height));
		DrawWeaponRecommend(new Rect(2f, 2f + num, rect.width - 4f, 265f));
		GUI.EndGroup();
	}

	private void Awake()
	{
		_weaponRecomGui = new WeaponRecommendListGUI(BuyingLocationType.EndOfRound);
		_weaponDetailGui = new WeaponDetailGUI();
		_playerListGui = new ValuablePlayerListGUI();
		_playerDetailGui = new ValuablePlayerDetailGUI();
		_playerListGui.OnSelectionChange = OnValuablePlayerListSelectionChange;
		_weaponRecomGui.OnSelectionChange = OnRecomListSelectionChange;
	}

	private void OnEnable()
	{
		OnUpdateRecommendationEvent(null);
		CmuneEventHandler.AddListener<UpdateRecommendationEvent>(OnUpdateRecommendationEvent);
		if (Singleton<EndOfMatchStats>.Instance.Data.MostValuablePlayers.Count > 0)
		{
			_playerListGui.SetSelection(0);
		}
		else
		{
			_playerDetailGui.SetValuablePlayer(null);
		}
		_weaponRecomGui.Enabled = true;
		_playerListGui.Enabled = true;
	}

	private void OnDisabled()
	{
		_weaponRecomGui.Enabled = false;
		_playerListGui.Enabled = false;
		_playerListGui.ClearSelection();
		_playerDetailGui.StopBadgeShow();
		CmuneEventHandler.RemoveListener<UpdateRecommendationEvent>(OnUpdateRecommendationEvent);
	}

	private void OnUpdateRecommendationEvent(UpdateRecommendationEvent ev)
	{
		List<KeyValuePair<RecommendType, IUnityItem>> list = new List<KeyValuePair<RecommendType, IUnityItem>>(3);
		list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.StaffPick, Singleton<MapManager>.Instance.GetRecommendedItem(GameState.CurrentSpace.SceneName)));
		list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.RecommendedArmor, ShopUtils.GetRecommendedArmor(PlayerDataManager.PlayerLevelSecure, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearHolo), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearUpperBody), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearLowerBody))));
		IUnityItem itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(Singleton<EndOfMatchStats>.Instance.Data.MostEffecientWeaponId);
		if (itemInShop == null)
		{
			RecommendationUtils.WeaponRecommendation recommendedWeapon = RecommendationUtils.GetRecommendedWeapon(PlayerDataManager.PlayerLevelSecure, GameState.CurrentSpace.CombatRangeTiers);
			itemInShop = recommendedWeapon.ItemWeapon ?? RecommendationUtils.FallBackWeapon;
			list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.RecommendedWeapon, itemInShop));
		}
		else
		{
			list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.MostEfficient, itemInShop));
		}
		_weaponRecomGui.UpdateRecommendedList(list);
	}

	private void DrawWeaponRecommend(Rect rect)
	{
		GUI.BeginGroup(rect);
		GUI.Label(new Rect(0f, 2f, rect.width, 20f), LocalizedStrings.RecommendedLoadoutCaps, BlueStonez.label_interparkbold_18pt);
		DrawRecommendContent(new Rect(0f, 25f, rect.width, rect.height - 25f));
		GUI.EndGroup();
	}

	private void DrawRecommendContent(Rect rect)
	{
		GUI.BeginGroup(rect);
		if (_weaponRecomGui.SelectedItem != null)
		{
			_weaponDetailGui.Draw(new Rect(0f, 0f, 200f, rect.height));
		}
		else
		{
			_playerDetailGui.Draw(new Rect(0f, 0f, 200f, rect.height));
		}
		_weaponRecomGui.Draw(new Rect(199f, 0f, rect.width - 200f + 1f, rect.height));
		GUI.EndGroup();
	}

	private void OnRecomListSelectionChange(IUnityItem item, RecommendType type)
	{
		_playerListGui.ClearSelection();
		_playerDetailGui.StopBadgeShow();
		_weaponDetailGui.SetWeaponItem(item, type);
	}

	private void OnValuablePlayerListSelectionChange(StatsSummary playerStats)
	{
		_weaponRecomGui.ClearSelection();
		_playerDetailGui.SetValuablePlayer(playerStats);
	}
}
