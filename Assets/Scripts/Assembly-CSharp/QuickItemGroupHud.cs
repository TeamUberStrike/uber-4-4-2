using System.Collections.Generic;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class QuickItemGroupHud
{
	private Animatable2DGroup _quickItemsGroup;

	private List<QuickItemHud> _quickItemSlots;

	public Animatable2DGroup Group
	{
		get
		{
			return _quickItemsGroup;
		}
	}

	public bool Enabled
	{
		get
		{
			return _quickItemsGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_quickItemsGroup.Show();
				ResetQuickItemVisibility();
			}
			else
			{
				_quickItemsGroup.Hide();
			}
		}
	}

	public QuickItemGroupHud()
	{
		if (!HudAssets.Exists)
		{
			return;
		}
		_quickItemSlots = new List<QuickItemHud>(3);
		_quickItemsGroup = new Animatable2DGroup();
		_quickItemSlots.Add(new QuickItemHud("Slot A-"));
		_quickItemSlots.Add(new QuickItemHud("Slot B-"));
		_quickItemSlots.Add(new QuickItemHud("Slot C-"));
		foreach (QuickItemHud quickItemSlot in _quickItemSlots)
		{
			_quickItemsGroup.Group.Add(quickItemSlot.Group);
		}
		ResetQuickItemsTransform();
		ResetQuickItemVisibility();
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
		CmuneEventHandler.AddListener<InputAssignmentEvent>(OnInputAssignmentChange);
	}

	public void SetSelected(int slotIndex, bool moveNext = true)
	{
		for (int i = 0; i < _quickItemSlots.Count; i++)
		{
			_quickItemSlots[i].SetSelected(slotIndex == i, moveNext);
		}
	}

	public void Draw()
	{
		_quickItemsGroup.Draw();
	}

	public void ConfigureQuickItem(int slot, QuickItem quickItem, TeamID team = TeamID.NONE)
	{
		if (_quickItemSlots.Count > slot && slot >= 0)
		{
			QuickItemHud quickItemHud = _quickItemSlots[slot];
			if (quickItem != null)
			{
				quickItemHud.SetRechargeBarVisible(quickItem.Configuration.RechargeTime > 0);
				quickItemHud.SetKeyBinding(AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString((GameInputKey)(16 + slot)));
				if (team == TeamID.RED)
				{
					quickItemHud.ConfigureSlot(HudStyleUtility.DEFAULT_RED_COLOR, ConsumableHudTextures.CircleRed, ConsumableHudTextures.CircleWhite, ConsumableHudTextures.CircleRed, ConsumableHudTextures.CircleRed, GetIconRed(quickItem));
				}
				else
				{
					quickItemHud.ConfigureSlot(HudStyleUtility.DEFAULT_BLUE_COLOR, ConsumableHudTextures.CircleBlue, ConsumableHudTextures.CircleWhite, ConsumableHudTextures.CircleBlue, ConsumableHudTextures.CircleBlue, GetIconBlue(quickItem));
				}
			}
			else
			{
				quickItemHud.ConfigureEmptySlot();
			}
		}
		ResetQuickItemsTransform();
	}

	public QuickItemHud GetLoadoutQuickItemHud(int slot)
	{
		if (_quickItemSlots.Count > slot && slot >= 0)
		{
			return _quickItemSlots[slot];
		}
		return null;
	}

	public void Expand()
	{
		if (ApplicationDataManager.IsMobile)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < _quickItemSlots.Count; i++)
		{
			if (!_quickItemSlots[i].IsEmpty)
			{
				_quickItemSlots[i].Expand(new Vector2(0f, _quickItemSlots[i].ExpandedHeight * (float)(num - 3)), (float)num * 0.1f);
				num++;
			}
		}
	}

	public void Collapse()
	{
		if (ApplicationDataManager.IsMobile)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < _quickItemSlots.Count; i++)
		{
			if (!_quickItemSlots[i].IsEmpty)
			{
				_quickItemSlots[i].Collapse(new Vector2(0f, _quickItemSlots[i].CollapsedHeight * (float)(num - 3)), (float)num * 0.1f);
				num++;
			}
		}
	}

	private Texture2D GetIconBlue(QuickItem config)
	{
		switch (config.Logic)
		{
		case QuickItemLogic.AmmoPack:
			return ConsumableHudTextures.AmmoBlue;
		case QuickItemLogic.ArmorPack:
			return ConsumableHudTextures.ArmorBlue;
		case QuickItemLogic.HealthPack:
			return ConsumableHudTextures.HealthBlue;
		case QuickItemLogic.SpringGrenade:
			return ConsumableHudTextures.SpringGrenadeBlue;
		case QuickItemLogic.ExplosiveGrenade:
			return ConsumableHudTextures.OffensiveGrenadeBlue;
		default:
			return ConsumableHudTextures.AmmoBlue;
		}
	}

	private Texture2D GetIconRed(QuickItem config)
	{
		switch (config.Logic)
		{
		case QuickItemLogic.AmmoPack:
			return ConsumableHudTextures.AmmoRed;
		case QuickItemLogic.ArmorPack:
			return ConsumableHudTextures.ArmorRed;
		case QuickItemLogic.HealthPack:
			return ConsumableHudTextures.HealthRed;
		case QuickItemLogic.SpringGrenade:
			return ConsumableHudTextures.SpringGrenadeRed;
		case QuickItemLogic.ExplosiveGrenade:
			return ConsumableHudTextures.OffensiveGrenadeRed;
		default:
			return ConsumableHudTextures.AmmoRed;
		}
	}

	private void ResetQuickItemVisibility()
	{
		if (_quickItemSlots.Count == 0)
		{
			_quickItemsGroup.Hide();
			return;
		}
		_quickItemsGroup.Show();
		foreach (QuickItemHud quickItemSlot in _quickItemSlots)
		{
			if (quickItemSlot.IsEmpty)
			{
				quickItemSlot.ConfigureEmptySlot();
			}
		}
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetQuickItemsTransform();
	}

	private void OnInputAssignmentChange(InputAssignmentEvent ev)
	{
		for (int i = 0; i < _quickItemSlots.Count; i++)
		{
			QuickItemHud quickItemHud = _quickItemSlots[i];
			if (!quickItemHud.IsEmpty)
			{
				quickItemHud.SetKeyBinding(AutoMonoBehaviour<InputManager>.Instance.GetKeyAssignmentString((GameInputKey)(16 + i)));
			}
		}
	}

	private void ResetQuickItemsTransform()
	{
		QuickItemHud quickItemHud = _quickItemSlots[0];
		foreach (QuickItemHud quickItemSlot in _quickItemSlots)
		{
			quickItemSlot.ResetHud();
		}
		if (quickItemHud.IsExpanded)
		{
			Expand();
		}
		else
		{
			Collapse();
		}
		float y = (float)Screen.height * 0.9f - 10f;
		if (ApplicationDataManager.IsMobile)
		{
			y = 160f;
		}
		_quickItemsGroup.Position = new Vector2((float)Screen.width * 0.95f - quickItemHud.Group.Rect.width / 2f, y);
	}
}
