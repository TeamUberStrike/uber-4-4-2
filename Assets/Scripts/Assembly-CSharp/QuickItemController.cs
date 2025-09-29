using System;
using UberStrike.Core.Models.Views;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;

public class QuickItemController : Singleton<QuickItemController>
{
	private const float CooldownTime = 0.5f;

	private QuickItem[] _quickItems;

	private bool _isEnabled;

	public bool IsQuickItemMobilePushed;

	public bool IsEnabled
	{
		get
		{
			return _isEnabled && !GameState.CurrentGame.IsWaitingForPlayers && GameState.LocalCharacter.IsAlive;
		}
		set
		{
			_isEnabled = value;
		}
	}

	public bool IsCharging { get; set; }

	public bool IsConsumptionEnabled { get; set; }

	public int CurrentSlotIndex { get; private set; }

	public float NextCooldownFinishTime { get; set; }

	public QuickItemRestriction Restriction { get; private set; }

	private QuickItemController()
	{
		_quickItems = new QuickItem[LoadoutManager.QuickSlots.Length];
		Restriction = new QuickItemRestriction();
		Singleton<QuickItemEventListener>.Instance.Initialize();
		CmuneEventHandler.AddListener<InputChangeEvent>(OnInputChanged);
	}

	public void Initialize()
	{
		Clear();
		UpdateHudSlot(GameState.LocalCharacter.TeamID);
		for (int i = 0; i < LoadoutManager.QuickSlots.Length; i++)
		{
			LoadoutSlotType slot = LoadoutManager.QuickSlots[i];
			InventoryItem inventoryItem;
			if (Singleton<LoadoutManager>.Instance.TryGetItemInSlot(slot, out inventoryItem))
			{
				GameObject gameObject = inventoryItem.Item.Create(Vector3.zero, Quaternion.identity);
				if ((bool)gameObject)
				{
					InitializeQuickItem(gameObject, slot, inventoryItem);
					continue;
				}
				inventoryItem.Item.OnPrefabLoaded += delegate(IUnityItem item)
				{
					GameObject quickItemObject = item.Create(Vector3.zero, Quaternion.identity);
					InitializeQuickItem(quickItemObject, slot, inventoryItem);
				};
			}
			else
			{
				Restriction.InitializeSlot(i);
			}
		}
		ResetSlotSelection();
		Singleton<WeaponsHud>.Instance.QuickItems.Collapse();
	}

	public void ResetSlotSelection()
	{
		if (_quickItems.Length > 0)
		{
			CurrentSlotIndex = 0;
			if (!IsSlotAvailable(CurrentSlotIndex))
			{
				CurrentSlotIndex = GetNextAvailableSlotIndex(CurrentSlotIndex);
			}
		}
		Singleton<WeaponsHud>.Instance.QuickItems.SetSelected(CurrentSlotIndex);
	}

	public void UpdateQuickSlotAmount()
	{
		for (int i = 0; i < _quickItems.Length; i++)
		{
			if (_quickItems[i] != null)
			{
				Singleton<WeaponsHud>.Instance.SetQuickItemCurrentAmount(i, _quickItems[i].Behaviour.CurrentAmount);
			}
		}
	}

	public void UseQuickItem(LoadoutSlotType slot)
	{
		UseQuickItem(GetSlotIndex(slot));
	}

	private void UseQuickItem(int index)
	{
		if (!IsEnabled || IsCharging || Time.time < NextCooldownFinishTime)
		{
			return;
		}
		if (_quickItems != null && index >= 0 && _quickItems[index] != null)
		{
			if (_quickItems[index].Behaviour.Run() && GameState.LocalPlayer.Character != null)
			{
				SfxManager.Play2dAudioClip(GameAudio.WeaponSwitch);
			}
		}
		else
		{
			Debug.LogError("The QuickItem has no Behaviour: " + index);
		}
	}

	public void Update()
	{
		if (_quickItems == null)
		{
			return;
		}
		for (int i = 0; i < _quickItems.Length; i++)
		{
			if (_quickItems[i] != null)
			{
				Singleton<WeaponsHud>.Instance.SetQuickItemCooldown(i, _quickItems[i].Behaviour.CoolDownTimeRemaining);
				Singleton<WeaponsHud>.Instance.SetQuickItemRecharging(i, _quickItems[i].Behaviour.ChargingTimeRemaining);
			}
		}
	}

	private void OnInputChanged(InputChangeEvent ev)
	{
		if (ev.IsDown && !LevelCamera.Instance.IsZoomedIn && IsEnabled)
		{
			switch (ev.Key)
			{
			case GameInputKey.QuickItem1:
				UseQuickItem(LoadoutSlotType.QuickUseItem1);
				break;
			case GameInputKey.QuickItem2:
				UseQuickItem(LoadoutSlotType.QuickUseItem2);
				break;
			case GameInputKey.QuickItem3:
				UseQuickItem(LoadoutSlotType.QuickUseItem3);
				break;
			case GameInputKey.NextQuickItem:
				if (_quickItems.Length > 0)
				{
					CurrentSlotIndex = GetNextAvailableSlotIndex(CurrentSlotIndex);
					Singleton<WeaponsHud>.Instance.QuickItems.SetSelected(CurrentSlotIndex);
				}
				break;
			case GameInputKey.PrevQuickItem:
				if (_quickItems.Length > 0)
				{
					CurrentSlotIndex = GetPrevAvailableSlotIndex(CurrentSlotIndex);
					Singleton<WeaponsHud>.Instance.QuickItems.SetSelected(CurrentSlotIndex, false);
				}
				break;
			case GameInputKey.UseQuickItem:
				UseQuickItem(CurrentSlotIndex);
				break;
			}
		}
		if (ev.Key == GameInputKey.UseQuickItem && !LevelCamera.Instance.IsZoomedIn && IsEnabled)
		{
			IsQuickItemMobilePushed = ev.IsDown;
		}
	}

	private int GetNextAvailableSlotIndex(int currentSlot)
	{
		for (int num = (currentSlot + 1) % _quickItems.Length; num != currentSlot; num = (num + 1) % _quickItems.Length)
		{
			if (!Singleton<WeaponsHud>.Instance.QuickItems.GetLoadoutQuickItemHud(num).IsEmpty)
			{
				return num;
			}
		}
		return currentSlot;
	}

	private int GetPrevAvailableSlotIndex(int currentSlot)
	{
		for (int num = (currentSlot - 1) % _quickItems.Length; num != currentSlot; num = (num - 1) % _quickItems.Length)
		{
			if (num < 0)
			{
				num = _quickItems.Length - 1;
			}
			if (!Singleton<WeaponsHud>.Instance.QuickItems.GetLoadoutQuickItemHud(num).IsEmpty)
			{
				return num;
			}
		}
		return currentSlot;
	}

	private void UpdateHudSlot(TeamID teamId)
	{
		for (int i = 0; i < _quickItems.Length; i++)
		{
			QuickItem quickItem = _quickItems[i];
			Singleton<WeaponsHud>.Instance.QuickItems.ConfigureQuickItem(i, quickItem, teamId);
		}
		Singleton<WeaponsHud>.Instance.QuickItems.SetSelected(CurrentSlotIndex);
	}

	private bool IsSlotAvailable(int slotIndex)
	{
		if (slotIndex >= 0 && slotIndex < _quickItems.Length)
		{
			QuickItem quickItem = _quickItems[slotIndex];
			return quickItem != null;
		}
		return false;
	}

	private void UseConsumableItem(InventoryItem inventoryItem)
	{
		if (IsConsumptionEnabled)
		{
			ShopWebServiceClient.UseConsumableItem(PlayerDataManager.AuthToken, inventoryItem.Item.View.ID, null, null);
			inventoryItem.AmountRemaining--;
			if (inventoryItem.AmountRemaining == 0)
			{
				MonoRoutine.Start(Singleton<ItemManager>.Instance.StartGetInventory(false));
			}
		}
	}

	private LoadoutSlotType GetSlotType(int index)
	{
		return (LoadoutSlotType)(12 + index);
	}

	private GameInputKey GetFocusKey(LoadoutSlotType slot)
	{
		switch (slot)
		{
		case LoadoutSlotType.QuickUseItem1:
			return GameInputKey.QuickItem1;
		case LoadoutSlotType.QuickUseItem2:
			return GameInputKey.QuickItem2;
		case LoadoutSlotType.QuickUseItem3:
			return GameInputKey.QuickItem3;
		default:
			return GameInputKey.None;
		}
	}

	private int GetSlotIndex(LoadoutSlotType slot)
	{
		switch (slot)
		{
		case LoadoutSlotType.QuickUseItem1:
			return 0;
		case LoadoutSlotType.QuickUseItem2:
			return 1;
		case LoadoutSlotType.QuickUseItem3:
			return 2;
		default:
			return -1;
		}
	}

	private void InitializeQuickItem(GameObject quickItemObject, LoadoutSlotType slot, InventoryItem inventoryItem)
	{
		int slotIndex = GetSlotIndex(slot);
		QuickItem component = quickItemObject.GetComponent<QuickItem>();
		if ((bool)component)
		{
			component.gameObject.SetActive(true);
			for (int i = 0; i < component.gameObject.transform.childCount; i++)
			{
				component.gameObject.transform.GetChild(i).gameObject.SetActive(false);
			}
			component.gameObject.name = "QI - " + inventoryItem.Item.Name;
			component.transform.parent = GameState.LocalPlayer.WeaponAttachPoint;
			ItemConfigurationUtil.CopyProperties((UberStrikeItemQuickView)component.Configuration, inventoryItem.Item.View);
			ItemConfigurationUtil.CopyCustomProperties(inventoryItem.Item.View, component.Configuration);
			if (component.Configuration.RechargeTime <= 0)
			{
				int index = slotIndex;
				QuickItemBehaviour behaviour = component.Behaviour;
				behaviour.OnActivated = (Action)Delegate.Combine(behaviour.OnActivated, (Action)delegate
				{
					UseConsumableItem(inventoryItem);
					Restriction.DecreaseUse(index);
					NextCooldownFinishTime = Time.time + 0.5f;
				});
				Restriction.InitializeSlot(slotIndex, component, inventoryItem.AmountRemaining);
			}
			else
			{
				component.Behaviour.CurrentAmount = component.Configuration.AmountRemaining;
			}
			component.Behaviour.FocusKey = GetFocusKey(slot);
			Singleton<WeaponsHud>.Instance.SetQuickItemCurrentAmount(slotIndex, component.Behaviour.CurrentAmount);
			Singleton<WeaponsHud>.Instance.SetQuickItemCooldownMax(slotIndex, component.Behaviour.CoolDownTimeTotal);
			Singleton<WeaponsHud>.Instance.SetQuickItemRechargingMax(slotIndex, component.Behaviour.ChargingTimeTotal);
			if (component.Behaviour.CurrentAmount > 0)
			{
				Singleton<WeaponsHud>.Instance.QuickItems.ConfigureQuickItem(slotIndex, component, GameState.LocalCharacter.TeamID);
			}
			Singleton<WeaponsHud>.Instance.QuickItems.SetSelected(CurrentSlotIndex);
			IGrenadeProjectile grenadeProjectile = component as IGrenadeProjectile;
			if (grenadeProjectile != null)
			{
				grenadeProjectile.OnProjectileEmitted += delegate(IGrenadeProjectile p)
				{
					Singleton<ProjectileManager>.Instance.AddProjectile(p, Singleton<WeaponController>.Instance.NextProjectileId());
					GameState.CurrentGame.EmitQuickItem(p.Position, p.Velocity, inventoryItem.Item.View.ID, GameState.LocalCharacter.PlayerNumber, p.ID);
				};
			}
			if ((bool)_quickItems[slotIndex])
			{
				UnityEngine.Object.Destroy(_quickItems[slotIndex].gameObject);
			}
			_quickItems[slotIndex] = component;
		}
		else
		{
			Debug.LogError("Failed to initialize QuickItem");
		}
	}

	internal void Reset()
	{
	}

	internal void Clear()
	{
		for (int i = 0; i < _quickItems.Length; i++)
		{
			if (_quickItems[i] != null)
			{
				UnityEngine.Object.Destroy(_quickItems[i].gameObject);
			}
			_quickItems[i] = null;
		}
	}
}
