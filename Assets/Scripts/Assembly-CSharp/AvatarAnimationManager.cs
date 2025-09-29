using System.Collections.Generic;
using UberStrike.Core.Types;
using UnityEngine;

public class AvatarAnimationManager : AutoMonoBehaviour<AvatarAnimationManager>
{
	private enum AnimationState
	{
		None = 0,
		Idle = 1,
		Melee = 2,
		SmallGun = 3,
		MediumGun = 4,
		HeavyGun = 5
	}

	private Dictionary<AnimationState, AnimationIndex[]> _homeAnimations = new Dictionary<AnimationState, AnimationIndex[]>();

	private Dictionary<AnimationState, AnimationIndex[]> _shopAnimations = new Dictionary<AnimationState, AnimationIndex[]>();

	private float _nextAnimationTime;

	private Dictionary<AnimationState, AnimationIndex[]> _currentSet;

	private AnimationState _currentState;

	private void Awake()
	{
		AnimationIndex[] value = new AnimationIndex[3]
		{
			AnimationIndex.HomeNoWeaponIdle,
			AnimationIndex.HomeNoWeaponnLookAround,
			AnimationIndex.HomeNoWeaponRelaxNeck
		};
		AnimationIndex[] value2 = new AnimationIndex[4]
		{
			AnimationIndex.HomeMeleeIdle,
			AnimationIndex.HomeMeleeCheckWeapon,
			AnimationIndex.HomeMeleeLookAround,
			AnimationIndex.HomeMeleeRelaxNeck
		};
		AnimationIndex[] value3 = new AnimationIndex[4]
		{
			AnimationIndex.HomeSmallGunIdle,
			AnimationIndex.HomeSmallGunCheckWeapon,
			AnimationIndex.HomeSmallGunLookAround,
			AnimationIndex.HomeSmallGunRelaxNeck
		};
		AnimationIndex[] value4 = new AnimationIndex[4]
		{
			AnimationIndex.HomeMediumGunIdle,
			AnimationIndex.HomeMediumGunCheckWeapon,
			AnimationIndex.HomeMediumGunLookAround,
			AnimationIndex.HomeMediumGunRelaxNeck
		};
		AnimationIndex[] value5 = new AnimationIndex[5]
		{
			AnimationIndex.HomeLargeGunIdle,
			AnimationIndex.HomeLargeGunCheckWeapon,
			AnimationIndex.HomeLargeGunLookAround,
			AnimationIndex.HomeLargeGunRelaxNeck,
			AnimationIndex.HomeLargeGunShakeWeapon
		};
		_homeAnimations.Add(AnimationState.Idle, value);
		_homeAnimations.Add(AnimationState.Melee, value2);
		_homeAnimations.Add(AnimationState.SmallGun, value3);
		_homeAnimations.Add(AnimationState.MediumGun, value4);
		_homeAnimations.Add(AnimationState.HeavyGun, value5);
		_shopAnimations.Add(AnimationState.Idle, value);
		_shopAnimations.Add(AnimationState.Melee, new AnimationIndex[1] { AnimationIndex.ShopMeleeAimIdle });
		_shopAnimations.Add(AnimationState.SmallGun, new AnimationIndex[2]
		{
			AnimationIndex.ShopSmallGunAimIdle,
			AnimationIndex.ShopSmallGunShoot
		});
		_shopAnimations.Add(AnimationState.MediumGun, new AnimationIndex[2]
		{
			AnimationIndex.ShopLargeGunAimIdle,
			AnimationIndex.ShopLargeGunShoot
		});
		_shopAnimations.Add(AnimationState.HeavyGun, new AnimationIndex[2]
		{
			AnimationIndex.ShopLargeGunAimIdle,
			AnimationIndex.ShopLargeGunShoot
		});
	}

	private void Update()
	{
		if (_currentState != AnimationState.None && !GameState.HasCurrentGame)
		{
			if (_nextAnimationTime < Time.time)
			{
				PlayAnimation(GetNextAnimation(), _currentState);
			}
			if (GameState.LocalAvatar.Decorator != null && GameState.LocalAvatar.Decorator.AnimationController != null)
			{
				GameState.LocalAvatar.Decorator.AnimationController.UpdateAnimation();
			}
		}
	}

	private AnimationIndex GetNextAnimation()
	{
		AnimationIndex[] array = _currentSet[_currentState];
		return array[Random.Range(0, array.Length)];
	}

	private void PlayAnimation(AnimationIndex nextAnimation, AnimationState state, bool resetAnimations = false)
	{
		if (GameState.LocalAvatar.Decorator != null && GameState.LocalAvatar.Decorator.AnimationController != null)
		{
			AnimationInfo info;
			if (GameState.LocalAvatar.Decorator.AnimationController.TryGetAnimationInfo(nextAnimation, out info))
			{
				GameState.LocalAvatar.Decorator.AnimationController.TriggerAnimation(info.Index, resetAnimations || _currentState != state);
				_nextAnimationTime = Time.time + info.State.length - 0.1f;
			}
			else
			{
				_nextAnimationTime = Time.time + 1f;
			}
		}
		else
		{
			_nextAnimationTime = Time.time + 0.01f;
		}
		_currentState = state;
	}

	public void ResetAnimationState(PageType page)
	{
		SetAnimationState(page, (UberstrikeItemClass)0, true);
	}

	public void SetAnimationState(PageType page, UberstrikeItemClass type, bool resetAnimations = false)
	{
		if (page == PageType.Shop)
		{
			_currentSet = _shopAnimations;
			switch (type)
			{
			case UberstrikeItemClass.WeaponMelee:
				PlayAnimation(AnimationIndex.ShopMeleeTakeOut, AnimationState.Melee, resetAnimations);
				return;
			case UberstrikeItemClass.WeaponMachinegun:
			case UberstrikeItemClass.WeaponShotgun:
			case UberstrikeItemClass.WeaponSniperRifle:
			case UberstrikeItemClass.WeaponCannon:
			case UberstrikeItemClass.WeaponSplattergun:
			case UberstrikeItemClass.WeaponLauncher:
				PlayAnimation(AnimationIndex.ShopLargeGunTakeOut, AnimationState.HeavyGun, resetAnimations);
				return;
			case UberstrikeItemClass.GearBoots:
				PlayAnimation(AnimationIndex.ShopNewBoots, AnimationState.Idle, resetAnimations);
				return;
			case UberstrikeItemClass.GearHead:
			case UberstrikeItemClass.GearFace:
				PlayAnimation(AnimationIndex.ShopNewHead, AnimationState.Idle, resetAnimations);
				return;
			case UberstrikeItemClass.GearGloves:
				PlayAnimation(AnimationIndex.ShopNewGloves, AnimationState.Idle, resetAnimations);
				return;
			case UberstrikeItemClass.GearLowerBody:
				PlayAnimation(AnimationIndex.ShopNewLowerBody, AnimationState.Idle, resetAnimations);
				return;
			case UberstrikeItemClass.GearUpperBody:
				PlayAnimation(AnimationIndex.ShopNewUpperBody, AnimationState.Idle, resetAnimations);
				return;
			case UberstrikeItemClass.GearHolo:
				PlayAnimation(AnimationIndex.ShopNewUpperBody, AnimationState.Idle, resetAnimations);
				return;
			}
			if (_currentState == AnimationState.Melee)
			{
				PlayAnimation(AnimationIndex.ShopHideMelee, AnimationState.Idle, resetAnimations);
			}
			else if (_currentState == AnimationState.SmallGun || _currentState == AnimationState.MediumGun || _currentState == AnimationState.HeavyGun)
			{
				PlayAnimation(AnimationIndex.ShopHideGun, AnimationState.Idle, resetAnimations);
			}
			else
			{
				_nextAnimationTime = 0f;
			}
			_currentState = AnimationState.Idle;
		}
		else
		{
			_currentSet = _homeAnimations;
			_nextAnimationTime = 0f;
			switch (type)
			{
			case UberstrikeItemClass.WeaponMelee:
				_currentState = AnimationState.Melee;
				break;
			case UberstrikeItemClass.WeaponMachinegun:
			case UberstrikeItemClass.WeaponShotgun:
			case UberstrikeItemClass.WeaponSniperRifle:
			case UberstrikeItemClass.WeaponCannon:
			case UberstrikeItemClass.WeaponSplattergun:
			case UberstrikeItemClass.WeaponLauncher:
				_currentState = AnimationState.HeavyGun;
				break;
			default:
				_currentState = AnimationState.Idle;
				PlayAnimation(AnimationIndex.HomeNoWeaponIdle, AnimationState.Idle, resetAnimations);
				break;
			}
		}
	}
}
