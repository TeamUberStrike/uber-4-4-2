using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class WeaponSimulator : IWeaponController
{
	private bool _isFullSimulation = true;

	private AvatarDecorator _avatar;

	private CharacterConfig _config;

	private float _nextShootTime;

	private WeaponSlot _currentSlot;

	private WeaponSlot[] _weaponSlots;

	private int _projectileId;

	public int CurrentSlotIndex { get; private set; }

	public byte PlayerNumber
	{
		get
		{
			return _config.State.PlayerNumber;
		}
	}

	public Vector3 ShootingPoint
	{
		get
		{
			return _config.State.ShootingPoint;
		}
	}

	public Vector3 ShootingDirection
	{
		get
		{
			return _config.State.ShootingDirection;
		}
	}

	public bool IsLocal
	{
		get
		{
			return false;
		}
	}

	public WeaponSimulator(CharacterConfig config)
	{
		_config = config;
		_weaponSlots = new WeaponSlot[5];
		CurrentSlotIndex = -1;
	}

	public void Update(UberStrike.Realtime.UnitySdk.CharacterInfo state, bool isLocal)
	{
		if (_avatar != null && state != null && state.IsAlive && !isLocal && state.IsFiring)
		{
			Shoot(state);
		}
	}

	public void Shoot(UberStrike.Realtime.UnitySdk.CharacterInfo state)
	{
		if (state == null || !(_nextShootTime < Time.time))
		{
			return;
		}
		if (_currentSlot != null)
		{
			_nextShootTime = Time.time + WeaponConfigurationHelper.GetRateOfFire(_currentSlot.View);
			if (_isFullSimulation)
			{
				BeginShooting();
				CmunePairList<BaseGameProp, ShotPoint> hits;
				_currentSlot.Logic.Shoot(new Ray(state.ShootingPoint + LocalPlayer.EyePosition, state.ShootingDirection), out hits);
				EndShooting();
			}
		}
		else
		{
			Debug.LogWarning("Current weapon is null!");
		}
	}

	public IProjectile EmitProjectile(int actorID, byte playerNumber, Vector3 origin, Vector3 direction, LoadoutSlotType slot, int projectileId, bool explode)
	{
		IProjectile result = null;
		if (_isFullSimulation)
		{
			BeginShooting();
			switch (slot)
			{
			case LoadoutSlotType.WeaponPrimary:
				result = ShootProjectileFromSlot(1, origin, direction, projectileId, explode, actorID);
				break;
			case LoadoutSlotType.WeaponSecondary:
				result = ShootProjectileFromSlot(2, origin, direction, projectileId, explode, actorID);
				break;
			case LoadoutSlotType.WeaponTertiary:
				result = ShootProjectileFromSlot(3, origin, direction, projectileId, explode, actorID);
				break;
			case LoadoutSlotType.WeaponPickup:
				result = ShootProjectileFromSlot(4, origin, direction, projectileId, explode, actorID);
				break;
			}
			EndShooting();
		}
		return result;
	}

	private void BeginShooting()
	{
		CharacterHitArea[] hitAreas = _avatar.HitAreas;
		foreach (CharacterHitArea characterHitArea in hitAreas)
		{
			characterHitArea.gameObject.layer = 2;
		}
	}

	private void EndShooting()
	{
		CharacterHitArea[] hitAreas = _avatar.HitAreas;
		foreach (CharacterHitArea characterHitArea in hitAreas)
		{
			characterHitArea.gameObject.layer = _avatar.gameObject.layer;
		}
	}

	private IProjectile ShootProjectileFromSlot(int slot, Vector3 origin, Vector3 direction, int projectileID, bool explode, int actorID)
	{
		if (_weaponSlots.Length > slot && _weaponSlots[slot] != null)
		{
			ProjectileWeapon projectileWeapon = _weaponSlots[slot].Logic as ProjectileWeapon;
			if (projectileWeapon != null)
			{
				projectileWeapon.Decorator.PlayShootSound();
				if (!explode)
				{
					return projectileWeapon.EmitProjectile(new Ray(origin, direction), projectileID, actorID);
				}
				projectileWeapon.ShowExplosionEffect(origin, Vector3.up, direction, projectileID);
			}
		}
		return null;
	}

	public void UpdateWeaponSlot(int slotIndex, bool isLocal)
	{
		CurrentSlotIndex = slotIndex;
		switch (slotIndex)
		{
		case 0:
			_currentSlot = _weaponSlots[0];
			if (!isLocal)
			{
				_avatar.ShowWeapon(LoadoutSlotType.WeaponMelee);
			}
			break;
		case 1:
			_currentSlot = _weaponSlots[1];
			if (!isLocal)
			{
				_avatar.ShowWeapon(LoadoutSlotType.WeaponPrimary);
			}
			break;
		case 2:
			_currentSlot = _weaponSlots[2];
			if (!isLocal)
			{
				_avatar.ShowWeapon(LoadoutSlotType.WeaponSecondary);
			}
			break;
		case 3:
			_currentSlot = _weaponSlots[3];
			if (!isLocal)
			{
				_avatar.ShowWeapon(LoadoutSlotType.WeaponTertiary);
			}
			break;
		case 4:
			_currentSlot = _weaponSlots[4];
			if (!isLocal)
			{
				_avatar.ShowWeapon(LoadoutSlotType.WeaponPickup);
			}
			break;
		}
	}

	public void UpdateWeapons(int currentWeaponSlot, IList<int> weaponItemIds, IList<int> quickItemIds)
	{
		if (!(_avatar != null))
		{
			return;
		}
		IUnityItem[] array = new IUnityItem[5]
		{
			Singleton<ItemManager>.Instance.GetItemInShop((weaponItemIds != null && weaponItemIds.Count > 0) ? weaponItemIds[0] : 0),
			Singleton<ItemManager>.Instance.GetItemInShop((weaponItemIds != null && weaponItemIds.Count > 1) ? weaponItemIds[1] : 0),
			Singleton<ItemManager>.Instance.GetItemInShop((weaponItemIds != null && weaponItemIds.Count > 2) ? weaponItemIds[2] : 0),
			Singleton<ItemManager>.Instance.GetItemInShop((weaponItemIds != null && weaponItemIds.Count > 3) ? weaponItemIds[3] : 0),
			Singleton<ItemManager>.Instance.GetItemInShop((weaponItemIds != null && weaponItemIds.Count > 4) ? weaponItemIds[4] : 0)
		};
		LoadoutSlotType[] array2 = new LoadoutSlotType[5]
		{
			LoadoutSlotType.WeaponMelee,
			LoadoutSlotType.WeaponPrimary,
			LoadoutSlotType.WeaponSecondary,
			LoadoutSlotType.WeaponTertiary,
			LoadoutSlotType.WeaponPickup
		};
		int num = -1;
		for (int i = 0; i < _weaponSlots.Length; i++)
		{
			if (_weaponSlots[i] != null && _weaponSlots[i].Decorator != null)
			{
				Object.Destroy(_weaponSlots[i].Decorator.gameObject);
			}
			if (array[i] != null && (bool)_avatar.WeaponAttachPoint)
			{
				WeaponSlot weaponSlot = new WeaponSlot(array2[i], array[i], _avatar.WeaponAttachPoint, this);
				if ((bool)weaponSlot.Decorator)
				{
					if (num < 0)
					{
						num = i;
					}
					_avatar.AssignWeapon(array2[i], weaponSlot.Decorator);
				}
				else
				{
					Debug.LogError("WeaponDecorator is NULL!");
				}
				_weaponSlots[i] = weaponSlot;
			}
			else
			{
				_weaponSlots[i] = null;
			}
		}
		if (CurrentSlotIndex >= 0 && _weaponSlots[CurrentSlotIndex] != null && _weaponSlots[CurrentSlotIndex].Decorator != null)
		{
			_weaponSlots[CurrentSlotIndex].Decorator.IsEnabled = true;
		}
	}

	public void SetAvatarDecorator(AvatarDecorator decorator)
	{
		_avatar = decorator;
	}

	public void UpdateWeaponDecorator(IUnityItem item)
	{
		UpdateWeapons(_config.State.CurrentWeaponSlot, _config.State.Weapons.ItemIDs, _config.State.QuickItems);
	}

	public int NextProjectileId()
	{
		return ProjectileManager.CreateGlobalProjectileID(PlayerNumber, ++_projectileId);
	}
}
