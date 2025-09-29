using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CharacterStateAnimationController
{
	private AnimationIndex _aimingMode = AnimationIndex.heavyGunUpDown;

	private bool _isJumping;

	public bool IsCinematic { get; set; }

	public void Update(UberStrike.Realtime.UnitySdk.CharacterInfo state, AvatarAnimationController animation)
	{
		if (animation != null && state != null)
		{
			RunPlayerConditions(state, animation);
			animation.UpdateAnimation();
		}
	}

	private void RunPlayerConditions(UberStrike.Realtime.UnitySdk.CharacterInfo state, AvatarAnimationController animation)
	{
		if (IsCinematic || animation == null)
		{
			return;
		}
		if (state.Is(PlayerStates.PAUSED))
		{
			if (state.Is(PlayerStates.DUCKED))
			{
				animation.PlayAnimation(AnimationIndex.squat);
			}
			else if (state.CurrentWeaponID == 0)
			{
				animation.PlayAnimation(AnimationIndex.idle);
			}
			else if (state.CurrentWeaponSlot == 0)
			{
				animation.PlayAnimation(AnimationIndex.ShopSmallGunAimIdle);
			}
			else
			{
				animation.PlayAnimation(AnimationIndex.heavyGunBreathe);
			}
			return;
		}
		float num = 1f;
		if (state.Is(PlayerStates.DIVING))
		{
			animation.PlayAnimation(AnimationIndex.swimLoop, num * 0.5f);
		}
		else if (state.Is(PlayerStates.SWIMMING))
		{
			if (state.Is(PlayerStates.GROUNDED))
			{
				if (state.Keys == KeyState.Still)
				{
					if (state.CurrentWeaponSlot == 0)
					{
						animation.PlayAnimation(AnimationIndex.ShopSmallGunAimIdle);
					}
					else
					{
						animation.PlayAnimation(AnimationIndex.heavyGunBreathe);
					}
				}
				else
				{
					animation.PlayAnimation(AnimationIndex.walk, num);
				}
			}
			else
			{
				animation.PlayAnimation(AnimationIndex.swimLoop, num);
			}
			_isJumping = false;
		}
		else
		{
			if ((double)state.Distance > 0.05 && state.Is(PlayerStates.GROUNDED))
			{
				if (state.Is(PlayerStates.DUCKED))
				{
					animation.PlayAnimation(AnimationIndex.crouch, num);
				}
				else
				{
					animation.PlayAnimation(AnimationIndex.run, num);
				}
			}
			else if (state.Is(PlayerStates.JUMPING))
			{
				animation.PlayAnimation(AnimationIndex.jumpUp);
				_isJumping = true;
			}
			else if (state.Is(PlayerStates.DUCKED))
			{
				animation.PlayAnimation(AnimationIndex.squat);
				_isJumping = false;
			}
			else
			{
				if (_isJumping)
				{
					animation.TriggerAnimation(AnimationIndex.jumpLand);
				}
				if (state.CurrentWeaponSlot == 0)
				{
					animation.PlayAnimation(AnimationIndex.ShopSmallGunAimIdle);
				}
				else
				{
					animation.PlayAnimation(AnimationIndex.heavyGunBreathe);
				}
				_isJumping = false;
			}
			if (state.IsFiring && state.CurrentWeaponCategory == UberstrikeItemClass.WeaponMelee && !IsPlayingMelee(animation))
			{
				if (Random.Range(0, 2) == 0)
				{
					animation.TriggerAnimation(AnimationIndex.meleeSwingRightToLeft, 1.5f, false);
				}
				else
				{
					animation.TriggerAnimation(AnimationIndex.meleSwingLeftToRight, 1.5f, false);
				}
			}
		}
		UpdateAimingMode(state, animation);
	}

	private bool IsPlayingMelee(AvatarAnimationController animation)
	{
		return animation.IsPlaying(AnimationIndex.meleeSwingRightToLeft) || animation.IsPlaying(AnimationIndex.meleSwingLeftToRight);
	}

	private void UpdateAimingMode(UberStrike.Realtime.UnitySdk.CharacterInfo state, AvatarAnimationController animation)
	{
		if (state.CurrentFiringMode == FireMode.Secondary)
		{
			_aimingMode = AnimationIndex.snipeUpDown;
		}
		else if (state.CurrentFiringMode == FireMode.Primary)
		{
			_aimingMode = AnimationIndex.heavyGunUpDown;
		}
		animation.SetAnimationTimeNormalized(_aimingMode, state.VerticalRotation);
	}
}
