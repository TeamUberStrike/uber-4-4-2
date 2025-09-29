using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PregameLoadoutPageGUI : PageGUI
{
	private WeaponDetailGUI _weaponDetailGui;

	private WeaponRecommendListGUI _weaponRecomGui;

	private static int _itemSlotButtonHash = "Button".GetHashCode();

	private bool _doDragZoom;

	private float _zoomMultiplier = 1f;

	private bool _isDropZoomAnimating;

	private bool _showDragZoomAnimation;

	private bool _dropZoomAnimating;

	private float _alphaValue = 1f;

	private IUnityItem _activeDragItem;

	private bool _activeDragItemEquipped;

	private LoadoutSlotType _activeDragItemLoadoutSlot;

	private int _draggedControlID;

	private Rect _draggedControlRect;

	private LoadoutSlotType _lastSelectedSlot = LoadoutSlotType.Inventory;

	private LoadoutSlotType _weaponTakenFromSlot = LoadoutSlotType.None;

	private Vector2 _dragScalePivot;

	private Vector2 _dropTargetPositon = Vector2.zero;

	private string ModeName
	{
		get
		{
			return GameModes.GetModeName(GameState.CurrentGameMode);
		}
	}

	private void Awake()
	{
		_weaponDetailGui = new WeaponDetailGUI();
		_weaponRecomGui = new WeaponRecommendListGUI(BuyingLocationType.PreGame);
		_weaponRecomGui.OnSelectionChange = _weaponDetailGui.SetWeaponItem;
	}

	private void OnEnable()
	{
		OnUpdateRecommendationEvent(new UpdateRecommendationEvent());
		_weaponRecomGui.Enabled = true;
		CmuneEventHandler.AddListener<UpdateRecommendationEvent>(OnUpdateRecommendationEvent);
	}

	private void OnDisable()
	{
		_weaponRecomGui.Enabled = false;
		CmuneEventHandler.RemoveListener<UpdateRecommendationEvent>(OnUpdateRecommendationEvent);
	}

	private void OnUpdateRecommendationEvent(UpdateRecommendationEvent ev)
	{
		List<KeyValuePair<RecommendType, IUnityItem>> list = new List<KeyValuePair<RecommendType, IUnityItem>>(3);
		list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.StaffPick, Singleton<MapManager>.Instance.GetRecommendedItem(GameState.CurrentSpace.SceneName)));
		RecommendationUtils.WeaponRecommendation recommendedWeapon = RecommendationUtils.GetRecommendedWeapon(PlayerDataManager.PlayerLevelSecure, GameState.CurrentSpace.CombatRangeTiers);
		list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.RecommendedWeapon, recommendedWeapon.ItemWeapon ?? RecommendationUtils.FallBackWeapon));
		list.Add(new KeyValuePair<RecommendType, IUnityItem>(RecommendType.RecommendedArmor, ShopUtils.GetRecommendedArmor(PlayerDataManager.PlayerLevelSecure, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearHolo), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearUpperBody), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearLowerBody))));
		_weaponRecomGui.UpdateRecommendedList(list);
	}

	public override void DrawGUI(Rect rect)
	{
		GUI.skin = BlueStonez.Skin;
		GUI.BeginGroup(rect);
		DrawPanel(rect);
		GUI.EndGroup();
		DoDragControls();
	}

	public void EquipBoughtWeapon(IUnityItem baseItem)
	{
		if (baseItem != null)
		{
			switch (_lastSelectedSlot)
			{
			case LoadoutSlotType.WeaponMelee:
			case LoadoutSlotType.WeaponPrimary:
			case LoadoutSlotType.WeaponSecondary:
			case LoadoutSlotType.WeaponTertiary:
				Singleton<LoadoutManager>.Instance.SetLoadoutItem(_lastSelectedSlot, baseItem);
				Singleton<LoadoutManager>.Instance.EquipWeapon(_lastSelectedSlot, baseItem);
				break;
			default:
				Debug.LogError("Item not equipped because slot type not correct: " + _lastSelectedSlot);
				break;
			}
		}
	}

	private void DrawPanel(Rect panelRect)
	{
		GUI.BeginGroup(new Rect(1f, 0f, panelRect.width - 2f, panelRect.height));
		Rect position = new Rect(0f, 0f, panelRect.width - 2f, 242f);
		GUI.BeginGroup(position);
		_weaponDetailGui.Draw(new Rect(0f, 0f, 200f, position.height));
		_weaponRecomGui.Draw(new Rect(199f, 0f, position.width - 200f + 1f, position.height));
		GUI.EndGroup();
		Rect position2 = new Rect(0f, 241f, panelRect.width - 2f, 167f);
		GUI.BeginGroup(position2);
		DrawQuickItemLoadout(new Rect(0f, 5f, position2.width * 0.2f, position2.height));
		DrawWeaponLoadout(new Rect(position2.width * 0.2f, 0f, position2.width * 0.4f + 1f, position2.height));
		DrawGearLoadout(new Rect(position2.width * 0.6f, 0f, position2.width * 0.4f, position2.height));
		GUI.EndGroup();
		GUI.EndGroup();
	}

	private void DrawQuickItemLoadout(Rect rect)
	{
		GUI.BeginGroup(rect);
		DrawLoadoutItem(LocalizedStrings.QuickItem, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.QuickUseItem1), new Rect(rect.width / 2f - 24f, rect.height / 2f - 80f, 48f, 48f), LoadoutSlotType.QuickUseItem1, string.Empty, true);
		DrawLoadoutItem(LocalizedStrings.QuickItem, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.QuickUseItem2), new Rect(rect.width / 2f - 24f, rect.height / 2f - 24f, 48f, 48f), LoadoutSlotType.QuickUseItem2, string.Empty, true);
		DrawLoadoutItem(LocalizedStrings.QuickItem, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.QuickUseItem3), new Rect(rect.width / 2f - 24f, rect.height / 2f + 32f, 48f, 48f), LoadoutSlotType.QuickUseItem3, string.Empty, true);
		GUI.EndGroup();
	}

	private void DrawWeaponLoadout(Rect rect)
	{
		GUI.BeginGroup(rect);
		DrawGroupControl(new Rect(0f, 10f, rect.width, rect.height - 10f), "WEAPONS", BlueStonez.label_group_interparkbold_18pt);
		DrawWeaponLoadoutRangeIcon(new Rect(rect.width / 2f - 65f, rect.height / 2f - 85f, 128f, 128f));
		float num = (rect.width - 192f - 24f) / 3f;
		DrawLoadoutItem(LocalizedStrings.Melee, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponMelee), new Rect(12f, rect.height - 56f, 48f, 48f), LoadoutSlotType.WeaponMelee, AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString(GameInputKey.WeaponMelee), true);
		DrawLoadoutItem(LocalizedStrings.PrimaryWeapon, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponPrimary), new Rect(60f + num, rect.height - 56f, 48f, 48f), LoadoutSlotType.WeaponPrimary, AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString(GameInputKey.Weapon1), true);
		DrawLoadoutItem(LocalizedStrings.SecondaryWeapon, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponSecondary), new Rect(12f + (48f + num) * 2f, rect.height - 56f, 48f, 48f), LoadoutSlotType.WeaponSecondary, AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString(GameInputKey.Weapon2), true);
		DrawLoadoutItem(LocalizedStrings.TertiaryWeapon, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponTertiary), new Rect(12f + (48f + num) * 3f, rect.height - 56f, 48f, 48f), LoadoutSlotType.WeaponTertiary, AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString(GameInputKey.Weapon3), true);
		GUI.EndGroup();
	}

	private void DrawWeaponLoadoutRangeIcon(Rect rect)
	{
		DrawCombatRangeIconUtil.DrawWeaponRangeIcon2(rect, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponMelee), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponPrimary), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponSecondary), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponTertiary));
		DrawCombatRangeIconUtil.DrawRecommendedCombatRange(rect, GameState.CurrentSpace.CombatRangeTiers, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponMelee), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponPrimary), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponSecondary), Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.WeaponTertiary));
	}

	private void DrawGearLoadout(Rect rect)
	{
		GUI.BeginGroup(rect);
		DrawGroupControl(new Rect(0f, 10f, rect.width, rect.height - 10f), "ARMOR", BlueStonez.label_group_interparkbold_18pt);
		float num = (rect.width - 144f - 44f) / 2f;
		DrawLoadoutItem(LocalizedStrings.UpperBody, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearUpperBody), new Rect(22f, rect.height - 56f, 48f, 48f), LoadoutSlotType.GearUpperBody, "UB", false);
		DrawLoadoutItem(LocalizedStrings.LowerBody, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearLowerBody), new Rect(70f + num, rect.height - 56f, 48f, 48f), LoadoutSlotType.GearLowerBody, "LB", false);
		DrawLoadoutItem(LocalizedStrings.UpperBody, Singleton<LoadoutManager>.Instance.GetItemOnSlot(LoadoutSlotType.GearHolo), new Rect(22f + (48f + num) * 2f, rect.height - 56f, 48f, 48f), LoadoutSlotType.GearHolo, "HO", false);
		int armorPoints = 0;
		int absorbtionRatio = 0;
		Singleton<LoadoutManager>.Instance.GetArmorValues(out armorPoints, out absorbtionRatio);
		DrawArmorPowerIconUtil.DrawArmorPower(new Rect(rect.width / 2f - 45f, rect.height / 2f - 70f, 90f, 90f), armorPoints, absorbtionRatio);
		GUI.EndGroup();
	}

	private void DrawLoadoutItem(string slotName, IUnityItem item, Rect rect, LoadoutSlotType loadoutSlotType, string slotTag, bool supportDrag)
	{
		if (item != null)
		{
			item.DrawIcon(new Rect(rect.x, rect.y, 48f, 48f));
			if (new Rect(rect.x, rect.y, 48f, 48f).Contains(Event.current.mousePosition) && !PanelManager.IsAnyPanelOpen && !PopupSystem.IsAnyPopupOpen)
			{
				AutoMonoBehaviour<ItemToolTip>.Instance.SetItem(item, new Rect(rect.x, rect.y, 48f, 48f), PopupViewSide.Left);
			}
		}
		else
		{
			GUI.Label(new Rect(rect.x, rect.y, 48f, 48f), new GUIContent(string.Empty, LocalizedStrings.Empty), BlueStonez.item_slot_large);
		}
		if (!string.IsNullOrEmpty(slotTag) && !ApplicationDataManager.IsMobile)
		{
			DrawSlotTag(rect, slotTag);
		}
		if (supportDrag)
		{
			WeaponEquipArea(rect, GUIContent.none, item, loadoutSlotType, BlueStonez.loadoutdropslot);
		}
	}

	private void DrawSlotTag(Rect rect, string slotTag)
	{
		GUI.color = Color.black;
		GUI.Label(new Rect(rect.x + 3f, rect.y + rect.height - 19f, rect.width, 18f), slotTag, BlueStonez.label_interparkbold_18pt_left);
		GUI.color = Color.white;
		GUI.Label(new Rect(rect.x + 2f, rect.y + rect.height - 18f, rect.width, 18f), slotTag, BlueStonez.label_interparkbold_18pt_left);
	}

	private bool WeaponEquipArea(Rect position, GUIContent guiContent, IUnityItem baseItem, LoadoutSlotType loadoutSlotType, GUIStyle guiStyle)
	{
		bool result = false;
		int controlID = GUIUtility.GetControlID(_itemSlotButtonHash, FocusType.Passive);
		switch (Event.current.GetTypeForControl(controlID))
		{
		case EventType.MouseDown:
			if (position.Contains(Event.current.mousePosition) && !_isDropZoomAnimating)
			{
				GUIUtility.hotControl = controlID;
				Event.current.Use();
			}
			break;
		case EventType.MouseUp:
			if (GUIUtility.hotControl == controlID && !_isDropZoomAnimating)
			{
				GUIUtility.hotControl = 0;
				Event.current.Use();
				result = position.Contains(Event.current.mousePosition);
			}
			else
			{
				if (!position.Contains(Event.current.mousePosition) || _activeDragItem == null)
				{
					break;
				}
				if (!_activeDragItemEquipped)
				{
					LoadoutSlotType activeDragItemLoadoutSlot = _activeDragItemLoadoutSlot;
					if (activeDragItemLoadoutSlot != LoadoutSlotType.Inventory)
					{
						break;
					}
					switch (loadoutSlotType)
					{
					case LoadoutSlotType.WeaponMelee:
						if (_activeDragItem.View.ItemClass == UberstrikeItemClass.WeaponMelee)
						{
							_lastSelectedSlot = LoadoutSlotType.WeaponMelee;
							Singleton<LoadoutManager>.Instance.SetLoadoutItem(loadoutSlotType, _activeDragItem);
						}
						break;
					case LoadoutSlotType.WeaponPrimary:
						if (_activeDragItem.View.ItemType == UberstrikeItemType.Weapon && _activeDragItem.View.ItemClass != UberstrikeItemClass.WeaponMelee)
						{
							_lastSelectedSlot = LoadoutSlotType.WeaponPrimary;
							Singleton<LoadoutManager>.Instance.SetLoadoutItem(loadoutSlotType, _activeDragItem);
						}
						break;
					case LoadoutSlotType.WeaponSecondary:
						if (_activeDragItem.View.ItemType == UberstrikeItemType.Weapon && _activeDragItem.View.ItemClass != UberstrikeItemClass.WeaponMelee)
						{
							_lastSelectedSlot = LoadoutSlotType.WeaponSecondary;
							Singleton<LoadoutManager>.Instance.SetLoadoutItem(loadoutSlotType, _activeDragItem);
						}
						break;
					case LoadoutSlotType.WeaponTertiary:
						if (_activeDragItem.View.ItemType == UberstrikeItemType.Weapon && _activeDragItem.View.ItemClass != UberstrikeItemClass.WeaponMelee)
						{
							_lastSelectedSlot = LoadoutSlotType.WeaponTertiary;
							Singleton<LoadoutManager>.Instance.SetLoadoutItem(loadoutSlotType, _activeDragItem);
						}
						break;
					case LoadoutSlotType.QuickUseItem1:
					case LoadoutSlotType.QuickUseItem2:
					case LoadoutSlotType.QuickUseItem3:
						if (_activeDragItem.View.ItemType == UberstrikeItemType.QuickUse)
						{
							Singleton<LoadoutManager>.Instance.SetLoadoutItem(loadoutSlotType, _activeDragItem);
						}
						break;
					}
				}
				else if (loadoutSlotType != LoadoutSlotType.WeaponMelee && _weaponTakenFromSlot != LoadoutSlotType.WeaponMelee)
				{
					Singleton<LoadoutManager>.Instance.SwitchItemInSlot(loadoutSlotType, _weaponTakenFromSlot);
					_weaponTakenFromSlot = LoadoutSlotType.None;
				}
			}
			break;
		case EventType.MouseDrag:
			if (GUIUtility.hotControl == controlID && !_isDropZoomAnimating)
			{
				_draggedControlID = GUIUtility.hotControl;
				Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(position.x, position.y));
				_draggedControlRect = new Rect(vector.x, vector.y, position.width, position.height);
				_activeDragItem = baseItem;
				_activeDragItemEquipped = true;
				_activeDragItemLoadoutSlot = loadoutSlotType;
				if (baseItem != null)
				{
					_weaponTakenFromSlot = loadoutSlotType;
				}
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
			break;
		case EventType.Repaint:
			guiStyle.Draw(position, guiContent, controlID);
			break;
		}
		return result;
	}

	private void DrawGroupControl(Rect rect, string title, GUIStyle textStyle)
	{
		GUI.BeginGroup(rect, string.Empty, BlueStonez.group_grey81);
		GUI.EndGroup();
		GUI.Label(new Rect(rect.x + 18f, rect.y - 8f, textStyle.CalcSize(new GUIContent(title)).x + 10f, 16f), title, textStyle);
	}

	private void DoDragControls()
	{
		if (Event.current.type == EventType.MouseUp)
		{
			_draggedControlID = 0;
			_doDragZoom = false;
			_activeDragItem = null;
			_activeDragItemEquipped = false;
			_activeDragItemLoadoutSlot = LoadoutSlotType.Shop;
		}
		if (_draggedControlID > 0 && _activeDragItem != null)
		{
			if (!_doDragZoom)
			{
				_doDragZoom = true;
				StartCoroutine(StartDragZoom(0f, 1f, 1.25f, 0.1f, 0.8f));
				return;
			}
			if (!_showDragZoomAnimation)
			{
				Vector2 screenPoint = new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
				screenPoint = GUIUtility.ScreenToGUIPoint(screenPoint);
				_dragScalePivot = new Vector2(screenPoint.x, screenPoint.y);
			}
			GUIUtility.ScaleAroundPivot(new Vector2(_zoomMultiplier, _zoomMultiplier), _dragScalePivot);
			GUI.backgroundColor = new Color(1f, 1f, 1f, _alphaValue);
			_activeDragItem.DrawIcon(new Rect(_dragScalePivot.x - 24f, _dragScalePivot.y - 24f, 48f, 48f));
		}
		else if (_dropZoomAnimating)
		{
			GUI.color = new Color(1f, 1f, 1f, _alphaValue);
			_activeDragItem.DrawIcon(new Rect(_draggedControlRect.xMin, _draggedControlRect.yMin, 48f, 48f));
			GUIUtility.ScaleAroundPivot(new Vector2(_zoomMultiplier, _zoomMultiplier), new Vector2(_dropTargetPositon.x + 32f, _dropTargetPositon.y + 32f));
			_activeDragItem.DrawIcon(new Rect(_dropTargetPositon.x, _dropTargetPositon.y, 48f, 48f));
		}
	}

	private IEnumerator StartDragZoom(float startTime, float startZoom, float endZoom, float startAlpha, float endAlpha)
	{
		_showDragZoomAnimation = true;
		float time = 0f;
		float DragScalePivotMultiplierX = 0f;
		float DragScalePivotMultiplierY = 0f;
		_dragScalePivot = new Vector2(_draggedControlRect.xMin + 32f, _draggedControlRect.yMin + 32f);
		while (time < startTime)
		{
			_alphaValue = Mathf.Lerp(startAlpha, endAlpha, time / startTime);
			_zoomMultiplier = Mathfx.Berp(startZoom, endZoom, time / startTime);
			Vector2 currentMousePos = new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y);
			currentMousePos = GUIUtility.ScreenToGUIPoint(currentMousePos);
			DragScalePivotMultiplierX = Mathf.Lerp(_draggedControlRect.xMin + 32f, currentMousePos.x, time / startTime);
			DragScalePivotMultiplierY = Mathf.Lerp(_draggedControlRect.yMin + 32f, currentMousePos.y, time / startTime);
			_dragScalePivot = new Vector2(DragScalePivotMultiplierX, DragScalePivotMultiplierY);
			time += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		_alphaValue = endAlpha;
		_zoomMultiplier = endZoom;
		_showDragZoomAnimation = false;
	}
}
