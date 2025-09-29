using System;
using System.Collections;
using System.Collections.Generic;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class WeaponController : Singleton<WeaponController>, IWeaponController
{
	private class InputEventHandler
	{
		public LoadoutSlotType SlotType { get; private set; }

		public Action<InputChangeEvent, LoadoutSlotType> Callback { get; private set; }

		public InputEventHandler(LoadoutSlotType slotType, Action<InputChangeEvent, LoadoutSlotType> callback)
		{
			SlotType = slotType;
			Callback = callback;
		}
	}

	private WeaponSlot[] _weapons;

	private WeaponSlot _weapon;

	private CircularInteger _currentSlotID;

	private bool _isWeaponControlEnabled = true;

	private float _weaponSwitchTimeout;

	private int _pickupWeaponEventCount;

	private float _pickUpWeaponAutoRemovalTime;

	private int _projectileId;

	private LoadoutSlotType _lastLoadoutType = LoadoutSlotType.WeaponPrimary;

	private readonly LoadoutSlotType[] _slotTypes = new LoadoutSlotType[5]
	{
		LoadoutSlotType.WeaponMelee,
		LoadoutSlotType.WeaponPrimary,
		LoadoutSlotType.WeaponSecondary,
		LoadoutSlotType.WeaponTertiary,
		LoadoutSlotType.WeaponPickup
	};

	private Dictionary<GameInputKey, InputEventHandler> _gameInputHandlers = new Dictionary<GameInputKey, InputEventHandler>();

	private WeaponShotCounter _shotCounter;

	public WeaponShotCounter ShotCounter
	{
		get
		{
			return _shotCounter;
		}
	}

	public byte PlayerNumber
	{
		get
		{
			return GameState.LocalCharacter.PlayerNumber;
		}
	}

	public bool IsLocal
	{
		get
		{
			return true;
		}
	}

	public Vector3 ShootingPoint
	{
		get
		{
			return GameState.LocalCharacter.ShootingPoint;
		}
	}

	public Vector3 ShootingDirection
	{
		get
		{
			return GameState.LocalCharacter.ShootingDirection;
		}
	}

	public bool HasAnyWeapon
	{
		get
		{
			WeaponSlot[] weapons = _weapons;
			foreach (WeaponSlot weaponSlot in weapons)
			{
				if (weaponSlot != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public BaseWeaponDecorator CurrentDecorator
	{
		get
		{
			if (IsWeaponValid)
			{
				return _weapon.Decorator;
			}
			return null;
		}
	}

	public bool IsWeaponValid
	{
		get
		{
			return _weapon != null && _weapon.Logic != null && _weapon.Decorator != null;
		}
	}

	public bool IsWeaponReady
	{
		get
		{
			return IsWeaponValid && _weapon.NextShootTime < Time.time && _weapon.Logic.IsWeaponActive;
		}
	}

	public bool IsSecondaryAction
	{
		get
		{
			return _weapon != null && !_weapon.InputHandler.CanChangeWeapon();
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _isWeaponControlEnabled && AutoMonoBehaviour<InputManager>.Instance.IsInputEnabled;
		}
		set
		{
			_isWeaponControlEnabled = value;
		}
	}

	public int TimeLeftForPickUpWeapon
	{
		get
		{
			if (_pickUpWeaponAutoRemovalTime > Time.time)
			{
				return Mathf.RoundToInt(_pickUpWeaponAutoRemovalTime - Time.time);
			}
			return -1;
		}
	}

	public LoadoutSlotType CurrentSlot
	{
		get
		{
			return (_weapon == null) ? LoadoutSlotType.None : _weapon.Slot;
		}
	}

	private bool CanPlayerShoot
	{
		get
		{
			return GameState.HasCurrentPlayer && IsEnabled && GameState.LocalCharacter.IsAlive && _weaponSwitchTimeout < Time.time;
		}
	}

	private WeaponController()
	{
		_weapons = new WeaponSlot[5];
		_shotCounter = new WeaponShotCounter();
		_currentSlotID = new CircularInteger(0, 3);
		InitInputEventHandlers();
		CmuneEventHandler.AddListener<InputChangeEvent>(OnInputChanged);
	}

	public void NextWeapon()
	{
		if (HasAnyWeapon)
		{
			if (_weapon != null && _weapon.InputHandler != null)
			{
				_weapon.InputHandler.Stop();
				_lastLoadoutType = _weapon.Slot;
				_weapon = null;
			}
			int next = _currentSlotID.Next;
			while (_weapons[next] == null)
			{
				next = _currentSlotID.Next;
			}
			ShowWeapon(_slotTypes[next]);
		}
	}

	public void PrevWeapon()
	{
		if (HasAnyWeapon)
		{
			if (_weapon != null && _weapon.InputHandler != null)
			{
				_weapon.InputHandler.Stop();
				_lastLoadoutType = _weapon.Slot;
				_weapon = null;
			}
			int prev = _currentSlotID.Prev;
			while (_weapons[prev] == null)
			{
				prev = _currentSlotID.Prev;
			}
			ShowWeapon(_slotTypes[prev]);
		}
	}

	public void ShowFirstWeapon()
	{
		_currentSlotID.Reset();
		NextWeapon();
	}

	public bool ShowWeapon(LoadoutSlotType slot)
	{
		return ShowWeapon(slot, false);
	}

	public bool ShowWeapon(LoadoutSlotType slot, bool force)
	{
		if (HudAssets.Exists)
		{
			Singleton<TemporaryWeaponHud>.Instance.Enabled = false;
		}
		if (force || _weapon == null || _weapon.Slot != slot)
		{
			WeaponSlot weaponSlot = null;
			switch (slot)
			{
			case LoadoutSlotType.WeaponMelee:
				weaponSlot = _weapons[0];
				if (weaponSlot != null)
				{
					_currentSlotID.Current = 0;
					Singleton<WeaponsHud>.Instance.SetActiveLoadout(LoadoutSlotType.WeaponMelee);
					if (GameState.HasCurrentPlayer)
					{
						GameState.LocalCharacter.CurrentWeaponSlot = 0;
					}
				}
				break;
			case LoadoutSlotType.WeaponPrimary:
				weaponSlot = _weapons[1];
				if (weaponSlot != null)
				{
					_currentSlotID.Current = 1;
					Singleton<WeaponsHud>.Instance.SetActiveLoadout(LoadoutSlotType.WeaponPrimary);
					if (GameState.HasCurrentPlayer)
					{
						GameState.LocalCharacter.CurrentWeaponSlot = 1;
					}
				}
				break;
			case LoadoutSlotType.WeaponSecondary:
				weaponSlot = _weapons[2];
				if (weaponSlot != null)
				{
					_currentSlotID.Current = 2;
					Singleton<WeaponsHud>.Instance.SetActiveLoadout(LoadoutSlotType.WeaponSecondary);
					if (GameState.HasCurrentPlayer)
					{
						GameState.LocalCharacter.CurrentWeaponSlot = 2;
					}
				}
				break;
			case LoadoutSlotType.WeaponTertiary:
				weaponSlot = _weapons[3];
				if (weaponSlot != null)
				{
					_currentSlotID.Current = 3;
					Singleton<WeaponsHud>.Instance.SetActiveLoadout(LoadoutSlotType.WeaponTertiary);
					if (GameState.HasCurrentPlayer)
					{
						GameState.LocalCharacter.CurrentWeaponSlot = 3;
					}
				}
				break;
			case LoadoutSlotType.WeaponPickup:
				weaponSlot = _weapons[4];
				if (weaponSlot != null)
				{
					_currentSlotID.Current = 4;
					Singleton<WeaponsHud>.Instance.SetActiveLoadout(LoadoutSlotType.WeaponPickup);
					if (GameState.HasCurrentPlayer)
					{
						GameState.LocalCharacter.CurrentWeaponSlot = 4;
					}
					if (TimeLeftForPickUpWeapon > 0 && HudAssets.Exists)
					{
						Singleton<TemporaryWeaponHud>.Instance.Enabled = true;
						Singleton<TemporaryWeaponHud>.Instance.StartCounting(30);
						Singleton<TemporaryWeaponHud>.Instance.RemainingSeconds = TimeLeftForPickUpWeapon;
					}
				}
				break;
			default:
				weaponSlot = _weapons[1];
				if (weaponSlot != null)
				{
					_currentSlotID.Current = 1;
					Singleton<WeaponsHud>.Instance.SetActiveLoadout(LoadoutSlotType.WeaponPrimary);
					if (GameState.HasCurrentPlayer)
					{
						GameState.LocalCharacter.CurrentWeaponSlot = 1;
					}
				}
				break;
			}
			if (weaponSlot != null)
			{
				_weaponSwitchTimeout = Time.time + 0.2f;
				_weapon = weaponSlot;
				UpdateAmmoHUD();
				if (_weapon.Logic != null && _weapon.Decorator != null)
				{
					WeaponFeedbackManager.Instance.PickUp(_weapon);
					_weapon.Decorator.PlayEquipSound();
				}
				else
				{
					Debug.LogError("Failed to show weapon: logic is null = " + (_weapon.Logic == null) + " decorator is null = " + (_weapon.Decorator == null));
				}
				return true;
			}
			if (!HasAnyWeapon)
			{
				return false;
			}
			return false;
		}
		return false;
	}

	public void UpdateAmmoHUD()
	{
		if (_weapon != null && HudAssets.Exists)
		{
			Singleton<AmmoHud>.Instance.Ammo = AmmoDepot.AmmoOfClass(_weapon.View.ItemClass);
		}
	}

	public void PutdownCurrentWeapon()
	{
		WeaponFeedbackManager.Instance.PutDown();
	}

	public void PickupCurrentWeapon()
	{
		if (_weapon != null)
		{
			WeaponFeedbackManager.Instance.PickUp(_weapon);
		}
	}

	public bool Shoot()
	{
		bool result = false;
		if (IsWeaponReady && GameState.HasCurrentPlayer)
		{
			_weapon.NextShootTime = Time.time + WeaponConfigurationHelper.GetRateOfFire(_weapon.View);
			if (AmmoDepot.HasAmmoOfClass(_weapon.View.ItemClass))
			{
				Ray ray = new Ray(GameState.LocalCharacter.ShootingPoint + LocalPlayer.EyePosition, GameState.LocalCharacter.ShootingDirection);
				ShotCounter.IncreaseShotCount(_weapon.View.ItemClass);
				CmunePairList<BaseGameProp, ShotPoint> hits;
				_weapon.Logic.Shoot(ray, out hits);
				if (!_weapon.Decorator.HasShootAnimation)
				{
					WeaponFeedbackManager.Instance.Fire();
				}
				AmmoDepot.UseAmmoOfClass(_weapon.View.ItemClass);
				UpdateAmmoHUD();
				if (HudAssets.Exists)
				{
					Singleton<ReticleHud>.Instance.TriggerReticle(_weapon.View.ItemClass);
				}
				result = true;
			}
			else
			{
				_weapon.Decorator.PlayOutOfAmmoSound();
				GameState.LocalCharacter.IsFiring = false;
			}
		}
		return result;
	}

	public WeaponSlot GetPrimaryWeapon()
	{
		return _weapons[1];
	}

	public WeaponSlot GetCurrentWeapon()
	{
		return _weapon;
	}

	public WeaponSlot GetPickupWeapon()
	{
		return _weapons[4];
	}

	public void InitializeAllWeapons(Transform attachPoint)
	{
		for (int i = 0; i < _weapons.Length; i++)
		{
			if (_weapons[i] != null && _weapons[i].Decorator != null)
			{
				UnityEngine.Object.Destroy(_weapons[i].Decorator.gameObject);
			}
			_weapons[i] = null;
		}
		WeaponInfo.SlotType[] array = new WeaponInfo.SlotType[4]
		{
			WeaponInfo.SlotType.Melee,
			WeaponInfo.SlotType.Primary,
			WeaponInfo.SlotType.Secondary,
			WeaponInfo.SlotType.Tertiary
		};
		for (int j = 0; j < LoadoutManager.WeaponSlots.Length; j++)
		{
			LoadoutSlotType slot = LoadoutManager.WeaponSlots[j];
			InventoryItem item;
			if (Singleton<LoadoutManager>.Instance.TryGetItemInSlot(slot, out item))
			{
				WeaponSlot weaponSlot = new WeaponSlot(slot, item.Item, attachPoint, this);
				AddGameLogicToWeapon(weaponSlot);
				_weapons[j] = weaponSlot;
				AmmoDepot.SetMaxAmmoForType(item.Item.View.ItemClass, ((UberStrikeItemWeaponView)item.Item.View).MaxAmmo);
				AmmoDepot.SetStartAmmoForType(item.Item.View.ItemClass, ((UberStrikeItemWeaponView)item.Item.View).StartAmmo);
				if (HudAssets.Exists)
				{
					Singleton<WeaponsHud>.Instance.Weapons.SetSlotWeapon(slot, item.Item);
				}
			}
			else if (GameState.HasCurrentPlayer)
			{
				GameState.LocalCharacter.Weapons.SetWeaponSlot(array[j], 0, (UberstrikeItemClass)0);
				if (HudAssets.Exists)
				{
					Singleton<WeaponsHud>.Instance.Weapons.SetSlotWeapon(slot, null);
				}
			}
		}
		if (GameState.HasCurrentPlayer)
		{
			GameState.LocalCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Pickup, 0, (UberstrikeItemClass)0);
		}
		Singleton<QuickItemController>.Instance.Initialize();
		if (HudAssets.Exists)
		{
			Singleton<WeaponsHud>.Instance.Weapons.SetSlotWeapon(LoadoutSlotType.WeaponPickup, null);
		}
		Reset();
	}

	public void ResetPickupWeaponSlotInSeconds(int seconds)
	{
		if (seconds <= 0)
		{
			_pickUpWeaponAutoRemovalTime = 0f;
		}
		else
		{
			_pickUpWeaponAutoRemovalTime = Time.time + (float)seconds;
		}
	}

	public void Reset()
	{
		AmmoDepot.Reset();
		_currentSlotID.SetRange(0, 3);
		_weapon = null;
		_shotCounter.Reset();
		ShowFirstWeapon();
		ResetPickupWeaponSlotInSeconds(0);
	}

	public void SetPickupWeapon(int weaponID)
	{
		SetPickupWeapon(weaponID, true, false);
	}

	public void SetPickupWeapon(int weaponID, bool uniqueWeaponClass, bool forceAutoEquip)
	{
		_pickupWeaponEventCount++;
		IUnityItem itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(weaponID);
		if (itemInShop != null)
		{
			if (GameState.HasCurrentPlayer)
			{
				if (!GameState.LocalCharacter.Weapons.ItemIDs.Contains(weaponID))
				{
					bool flag = true;
					for (int i = 0; i < 4; i++)
					{
						if (_weapons[i] != null && _weapons[i].View.ItemClass == itemInShop.View.ItemClass)
						{
							flag = false;
						}
					}
					if (flag || !uniqueWeaponClass)
					{
						if (_weapons[4] != null && _weapons[4].Decorator != null)
						{
							_weapons[4].InputHandler.Stop();
							UnityEngine.Object.Destroy(_weapons[4].Decorator.gameObject);
						}
						WeaponSlot weaponSlot = new WeaponSlot(LoadoutSlotType.WeaponPickup, itemInShop, GameState.LocalPlayer.WeaponAttachPoint, this);
						AddGameLogicToWeapon(weaponSlot);
						int current = _currentSlotID.Current;
						_currentSlotID.SetRange(0, 4);
						_currentSlotID.Current = current;
						if (HudAssets.Exists)
						{
							Singleton<WeaponsHud>.Instance.Weapons.SetSlotWeapon(LoadoutSlotType.WeaponPickup, itemInShop);
							Singleton<AmmoHud>.Instance.Ammo = AmmoDepot.AmmoOfClass(itemInShop.View.ItemClass);
						}
						Singleton<LoadoutManager>.Instance.EquipWeapon(LoadoutSlotType.WeaponPickup, itemInShop);
						GameState.LocalCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Pickup, itemInShop.View.ID, itemInShop.View.ItemClass);
						AmmoDepot.SetMaxAmmoForType(weaponSlot.View.ItemClass, weaponSlot.View.MaxAmmo);
						AmmoDepot.SetStartAmmoForType(weaponSlot.View.ItemClass, weaponSlot.View.StartAmmo);
						_weapons[4] = weaponSlot;
						if (_weapon == null || forceAutoEquip || ApplicationDataManager.ApplicationOptions.GameplayAutoEquipEnabled || _currentSlotID.Current == 4)
						{
							ShowWeapon(LoadoutSlotType.WeaponPickup, true);
						}
					}
				}
			}
			else
			{
				Debug.LogError("SetPickupWeapon failed because no player defined yet");
			}
			AmmoDepot.AddAmmoOfClass(itemInShop.View.ItemClass);
			UpdateAmmoHUD();
		}
		else
		{
			ResetPickupSlot();
		}
	}

	public void UpdateWeaponDecorator(IUnityItem item)
	{
		LoadoutSlotType loadoutSlotType = LoadoutSlotType.GearHead;
		for (int i = 0; i < 4; i++)
		{
			LoadoutSlotType loadoutSlotType2 = (LoadoutSlotType)(7 + i);
			if (_weapons[i] != null)
			{
				if (_weapons[i] == _weapon)
				{
					loadoutSlotType = loadoutSlotType2;
				}
				WeaponSlot weaponSlot = new WeaponSlot(loadoutSlotType2, _weapons[i].UnityItem, GameState.LocalPlayer.WeaponAttachPoint, this);
				AddGameLogicToWeapon(weaponSlot);
				UnityEngine.Object.Destroy(_weapons[i].Decorator.gameObject);
				_weapons[i] = weaponSlot;
			}
		}
		if (loadoutSlotType > LoadoutSlotType.GearHead)
		{
			ShowWeapon(loadoutSlotType, true);
		}
	}

	public void ResetPickupSlot()
	{
		if (HudAssets.Exists)
		{
			Singleton<TemporaryWeaponHud>.Instance.Enabled = false;
		}
		if (_weapons[4] == null || !(_weapons[4].Decorator != null))
		{
			return;
		}
		_weapons[4].InputHandler.Stop();
		UberStrikeItemWeaponView sameClassWeapon = null;
		if (GetPlayerWeaponOfPickupClass(out sameClassWeapon))
		{
			AmmoDepot.SetMaxAmmoForType(sameClassWeapon.ItemClass, sameClassWeapon.MaxAmmo);
			AmmoDepot.RemoveExtraAmmoOfType(sameClassWeapon.ItemClass);
			UpdateAmmoHUD();
		}
		MonoRoutine.Start(StartHidingWeapon(_weapons[4].Decorator.gameObject, true));
		if (_weapon != null && _weapon.Slot == LoadoutSlotType.WeaponPickup)
		{
			WeaponFeedbackManager.Instance.PutDown();
		}
		int current = _currentSlotID.Current;
		_currentSlotID.SetRange(0, 3);
		if (current != 4)
		{
			_currentSlotID.Current = current;
		}
		_weapons[4] = null;
		if (HudAssets.Exists)
		{
			Singleton<WeaponsHud>.Instance.Weapons.SetSlotWeapon(LoadoutSlotType.WeaponPickup, null);
		}
		if (current == 4)
		{
			if (HudAssets.Exists)
			{
				Singleton<WeaponsHud>.Instance.ResetActiveWeapon();
			}
			ShowWeapon(_lastLoadoutType);
		}
	}

	public bool HasWeaponOfClass(UberstrikeItemClass itemClass)
	{
		for (int i = 0; i < 4; i++)
		{
			WeaponSlot weaponSlot = _weapons[i];
			if (weaponSlot != null && weaponSlot.HasWeapon && weaponSlot.View.ItemClass == itemClass)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckPlayerWeaponInPickupSlot(int id)
	{
		if (_weapons[4] != null && _weapons[4].HasWeapon && _weapons[4].View.ID == id)
		{
			return true;
		}
		return false;
	}

	public void StopInputHandler()
	{
		if (_weapon != null)
		{
			_weapon.InputHandler.Stop();
		}
	}

	public int NextProjectileId()
	{
		return ProjectileManager.CreateGlobalProjectileID(PlayerNumber, ++_projectileId);
	}

	private void OnInputChanged(InputChangeEvent ev)
	{
		InputEventHandler value;
		if (GameState.HasCurrentPlayer && IsEnabled && GameState.LocalCharacter.IsAlive && _gameInputHandlers.TryGetValue(ev.Key, out value))
		{
			value.Callback(ev, value.SlotType);
		}
	}

	private void InitInputEventHandlers()
	{
		_gameInputHandlers.Add(GameInputKey.WeaponMelee, new InputEventHandler(LoadoutSlotType.WeaponMelee, SelectWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.Weapon1, new InputEventHandler(LoadoutSlotType.WeaponPrimary, SelectWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.Weapon2, new InputEventHandler(LoadoutSlotType.WeaponSecondary, SelectWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.Weapon3, new InputEventHandler(LoadoutSlotType.WeaponTertiary, SelectWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.Weapon4, new InputEventHandler(LoadoutSlotType.WeaponPickup, SelectWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.PrevWeapon, new InputEventHandler(LoadoutSlotType.None, PrevWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.NextWeapon, new InputEventHandler(LoadoutSlotType.None, NextWeaponCallback));
		_gameInputHandlers.Add(GameInputKey.PrimaryFire, new InputEventHandler(LoadoutSlotType.None, PrimaryFireCallback));
		_gameInputHandlers.Add(GameInputKey.SecondaryFire, new InputEventHandler(LoadoutSlotType.None, SecondaryFireCallback));
	}

	private void SelectWeaponCallback(InputChangeEvent ev, LoadoutSlotType slotType)
	{
		if (ev.IsDown && !LevelCamera.Instance.IsZoomedIn)
		{
			ShowWeapon(slotType);
		}
	}

	private void PrevWeaponCallback(InputChangeEvent ev, LoadoutSlotType slotType)
	{
		if ((_weapon == null || (ev.IsDown && _weapon.InputHandler.CanChangeWeapon())) && GUITools.SaveClickIn(0.2f))
		{
			GUITools.Clicked();
			NextWeapon();
		}
		else if (_weapon != null && ev.IsDown)
		{
			_weapon.InputHandler.OnPrevWeapon();
		}
	}

	private void NextWeaponCallback(InputChangeEvent ev, LoadoutSlotType slotType)
	{
		if ((_weapon == null || (ev.IsDown && _weapon.InputHandler.CanChangeWeapon())) && GUITools.SaveClickIn(0.2f))
		{
			GUITools.Clicked();
			PrevWeapon();
		}
		else if (_weapon != null && ev.IsDown)
		{
			_weapon.InputHandler.OnNextWeapon();
		}
	}

	private void PrimaryFireCallback(InputChangeEvent ev, LoadoutSlotType slotType)
	{
		if (ev.IsDown && CanPlayerShoot)
		{
			if (_weapon != null && _weapon.HasWeapon)
			{
				_weapon.InputHandler.OnPrimaryFire(true);
			}
		}
		else if (_weapon != null)
		{
			GameState.LocalCharacter.IsFiring = false;
			_weapon.InputHandler.OnPrimaryFire(false);
		}
	}

	private void SecondaryFireCallback(InputChangeEvent ev, LoadoutSlotType slotType)
	{
		if (GameState.HasCurrentPlayer && GameState.LocalCharacter.IsAlive && IsEnabled && _weapon != null && _weapon.HasWeapon)
		{
			_weapon.InputHandler.OnSecondaryFire(ev.IsDown);
		}
	}

	public void LateUpdate()
	{
		if (CanPlayerShoot)
		{
			if (_weapon != null && _weapon.HasWeapon && _weaponSwitchTimeout < Time.time)
			{
				_weapon.InputHandler.Update();
			}
			if (_pickUpWeaponAutoRemovalTime > 0f && _pickUpWeaponAutoRemovalTime < Time.time)
			{
				_pickUpWeaponAutoRemovalTime = 0f;
				if (GameState.HasCurrentPlayer)
				{
					GameState.LocalCharacter.Weapons.SetWeaponSlot(WeaponInfo.SlotType.Pickup, 0, (UberstrikeItemClass)0);
				}
				ResetPickupSlot();
				LevelCamera.Instance.ResetZoom();
			}
			if (HudAssets.Exists)
			{
				Singleton<TemporaryWeaponHud>.Instance.RemainingSeconds = TimeLeftForPickUpWeapon;
			}
		}
		else
		{
			if (GameState.HasCurrentPlayer)
			{
				GameState.LocalCharacter.IsFiring = false;
			}
			if (_weapon != null && _weapon.InputHandler != null)
			{
				_weapon.InputHandler.Stop();
			}
		}
	}

	private IEnumerator StartHidingWeapon(GameObject weapon, bool destroy)
	{
		for (float time = 0f; time < 2f; time += Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
		}
		if (destroy)
		{
			UnityEngine.Object.Destroy(weapon);
		}
	}

	private IEnumerator StartApplyDamage(float delay, CmunePairList<BaseGameProp, ShotPoint> hits)
	{
		yield return new WaitForSeconds(delay);
		ApplyDamage(hits);
	}

	private void ApplyDamage(CmunePairList<BaseGameProp, ShotPoint> hits)
	{
		foreach (KeyValuePair<BaseGameProp, ShotPoint> hit in hits)
		{
			int shotCount = _shotCounter.GetShotCount(_weapon.View.ItemClass);
			DamageInfo damageInfo = new DamageInfo((short)(_weapon.View.DamagePerProjectile * hit.Value.Count));
			damageInfo.Force = GameState.LocalPlayer.WeaponCamera.transform.forward * _weapon.View.DamageKnockback;
			damageInfo.Hitpoint = hit.Value.MidPoint;
			damageInfo.ProjectileID = hit.Value.ProjectileId;
			damageInfo.ShotCount = shotCount;
			damageInfo.WeaponID = _weapon.View.ID;
			damageInfo.WeaponClass = _weapon.View.ItemClass;
			damageInfo.DamageEffectFlag = _weapon.Logic.Config.DamageEffectFlag;
			damageInfo.DamageEffectValue = _weapon.Logic.Config.DamageEffectValue;
			damageInfo.CriticalStrikeBonus = (float)_weapon.Logic.Config.CriticalStrikeBonus / 100f;
			DamageInfo damageInfo2 = damageInfo;
			UberstrikeItemClass weaponClass = damageInfo2.WeaponClass;
			if (weaponClass == UberstrikeItemClass.WeaponSniperRifle && damageInfo2.CriticalStrikeBonus == 0f)
			{
				damageInfo2.CriticalStrikeBonus = 0.5f;
			}
			if (hit.Key != null)
			{
				hit.Key.ApplyDamage(damageInfo2);
			}
		}
	}

	private void AddGameLogicToWeapon(WeaponSlot weapon)
	{
		float movement = WeaponConfigurationHelper.GetRecoilMovement(weapon.View);
		float kickback = WeaponConfigurationHelper.GetRecoilKickback(weapon.View);
		LoadoutSlotType slot = weapon.Slot;
		if (weapon.Logic is ProjectileWeapon)
		{
			ProjectileWeapon w = weapon.Logic as ProjectileWeapon;
			w.OnProjectileShoot += delegate(ProjectileInfo p)
			{
				ProjectileDetonator projectileDetonator = new ProjectileDetonator(WeaponConfigurationHelper.GetSplashRadius(weapon.View), weapon.View.DamagePerProjectile, weapon.View.DamageKnockback, p.Direction, p.Id, weapon.View.ID, weapon.View.ItemClass, w.Config.DamageEffectFlag, w.Config.DamageEffectValue);
				if (p.Projectile != null)
				{
					p.Projectile.Detonator = projectileDetonator;
					if (weapon.View.ItemClass != UberstrikeItemClass.WeaponSplattergun)
					{
						GameState.CurrentGame.EmitProjectile(p.Position, p.Direction, slot, p.Id, false);
					}
				}
				else
				{
					projectileDetonator.Explode(p.Position);
					GameState.CurrentGame.EmitProjectile(p.Position, p.Direction, slot, p.Id, true);
				}
				if (weapon.View.ItemClass == UberstrikeItemClass.WeaponSplattergun)
				{
					GameState.LocalCharacter.IsFiring = true;
				}
				else if (w.HasProjectileLimit)
				{
					Singleton<ProjectileManager>.Instance.AddLimitedProjectile(p.Projectile, p.Id, w.MaxConcurrentProjectiles);
				}
				else
				{
					Singleton<ProjectileManager>.Instance.AddProjectile(p.Projectile, p.Id);
				}
				LevelCamera.Instance.DoFeedback(LevelCamera.FeedbackType.ShootWeapon, Vector3.back, 0f, movement / 8f, 0.1f, 0.3f, kickback / 3f, Vector3.left);
			};
			return;
		}
		if (weapon.Logic is MeleeWeapon)
		{
			float delay = weapon.Logic.HitDelay;
			weapon.Logic.OnTargetHit += delegate(CmunePairList<BaseGameProp, ShotPoint> h)
			{
				if (GameState.LocalCharacter != null)
				{
					if (weapon.View.HasAutomaticFire)
					{
						GameState.LocalCharacter.IsFiring = true;
					}
					else
					{
						GameState.CurrentGame.SingleBulletFire();
					}
				}
				if (h != null)
				{
					MonoRoutine.Start(StartApplyDamage(delay, h));
				}
				LevelCamera.Instance.DoFeedback(LevelCamera.FeedbackType.ShootWeapon, Vector3.back, 0f, movement / 8f, 0.1f, 0.3f, kickback / 3f, Vector3.left);
			};
			return;
		}
		weapon.Logic.OnTargetHit += delegate(CmunePairList<BaseGameProp, ShotPoint> h)
		{
			if (GameState.LocalCharacter != null)
			{
				if (weapon.View.HasAutomaticFire)
				{
					GameState.LocalCharacter.IsFiring = true;
				}
				else
				{
					GameState.CurrentGame.SingleBulletFire();
				}
			}
			if (h != null)
			{
				ApplyDamage(h);
			}
			LevelCamera.Instance.DoFeedback(LevelCamera.FeedbackType.ShootWeapon, Vector3.back, 0f, movement / 8f, 0.1f, 0.3f, kickback / 3f, Vector3.left);
		};
	}

	private bool GetPlayerWeaponOfPickupClass(out UberStrikeItemWeaponView sameClassWeapon)
	{
		sameClassWeapon = null;
		if (_weapons[4] != null && _weapons[4].HasWeapon)
		{
			for (int i = 0; i < 4; i++)
			{
				WeaponSlot weaponSlot = _weapons[i];
				if (weaponSlot != null && weaponSlot.HasWeapon && weaponSlot.View.ItemClass == _weapons[4].View.ItemClass)
				{
					sameClassWeapon = weaponSlot.View;
					return true;
				}
			}
		}
		return false;
	}
}
