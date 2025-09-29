using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AvatarAnimationController
{
	private Animation _animation;

	private Dictionary<int, AnimationInfo> _animations;

	private Dictionary<int, Transform> _bones;

	public ICollection<AnimationInfo> Animations
	{
		get
		{
			return _animations.Values;
		}
	}

	public Animation Animation
	{
		get
		{
			return _animation;
		}
	}

	public AvatarAnimationController(Animation animation)
	{
		_bones = new Dictionary<int, Transform>();
		_animations = new Dictionary<int, AnimationInfo>();
		_animation = animation;
		InitBones(animation.transform);
		InitAnimations();
	}

	public void UpdateAnimation()
	{
		foreach (AnimationInfo animation in Animations)
		{
			if (animation.EndTime >= Time.time)
			{
				if (animation.State.weight == 0f)
				{
					animation.State.time = 0f;
				}
				animation.State.speed = animation.Speed;
				animation.CurrentTimePlayed = animation.State.normalizedTime;
				Animation.Blend(animation.Name, 1f, 0f);
			}
			else
			{
				animation.State.speed = 0f;
				animation.CurrentTimePlayed = animation.State.normalizedTime;
				Animation.Blend(animation.Name, 0f, 0.3f);
			}
		}
	}

	public void PlayAnimation(AnimationIndex id)
	{
		PlayAnimation(id, 1f);
	}

	public bool PlayAnimation(AnimationIndex id, float speed)
	{
		return PlayAnimation(id, speed, Time.deltaTime);
	}

	public bool PlayAnimation(AnimationIndex id, float speed, float runtime)
	{
		AnimationInfo value;
		if (_animations.TryGetValue((int)id, out value))
		{
			value.Speed = speed;
			value.EndTime = Time.time + runtime;
		}
		return value != null;
	}

	public void TriggerAnimation(AnimationIndex id)
	{
		TriggerAnimation(id, 1f, false);
	}

	public void TriggerAnimation(AnimationIndex id, bool stopAll)
	{
		TriggerAnimation(id, 1f, stopAll);
	}

	public void TriggerAnimation(AnimationIndex id, float speed, bool stopAll)
	{
		AnimationInfo value;
		if (_animations.TryGetValue((int)id, out value))
		{
			if (stopAll)
			{
				ResetAllAnimations();
			}
			value.State.time = 0f;
			PlayAnimation(id, speed, value.State.length / speed);
		}
	}

	public void RewindAnimation(AnimationIndex id)
	{
		AnimationInfo value;
		if (_animations.TryGetValue((int)id, out value))
		{
			value.State.speed = 0f;
			value.State.time = 0f;
			value.EndTime = 0f;
		}
	}

	public void ResetAllAnimations()
	{
		foreach (int key in _animations.Keys)
		{
			RewindAnimation((AnimationIndex)key);
		}
	}

	public string GetDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (AnimationInfo value in _animations.Values)
		{
			AnimationState state = value.State;
			stringBuilder.Append(state.name);
			stringBuilder.Append(", weight: ");
			stringBuilder.Append(state.weight.ToString("N2"));
			stringBuilder.Append(", time/runtime: ");
			stringBuilder.Append(state.time.ToString("N2"));
			stringBuilder.Append("/");
			stringBuilder.Append(value.EndTime.ToString("N2"));
			stringBuilder.Append(", length ");
			stringBuilder.Append(state.length.ToString("N2"));
			stringBuilder.Append("\n");
		}
		return stringBuilder.ToString();
	}

	public bool IsPlaying(AnimationIndex idx)
	{
		AnimationInfo value;
		if (_animations.TryGetValue((int)idx, out value))
		{
			return value.State.weight > 0f;
		}
		return false;
	}

	public void SetAnimationTimeNormalized(AnimationIndex idx, float time)
	{
		AnimationInfo value;
		if (_animations.TryGetValue((int)idx, out value))
		{
			value.State.normalizedTime = Mathf.Lerp(value.State.normalizedTime, Mathf.Clamp01(time), Time.deltaTime * 10f);
			PlayAnimation(idx, 0f);
		}
	}

	public Transform GetBoneTransform(BoneIndex i)
	{
		Transform value = null;
		_bones.TryGetValue((int)i, out value);
		return value;
	}

	public bool TryGetAnimationInfo(AnimationIndex id, out AnimationInfo info)
	{
		_animations.TryGetValue((int)id, out info);
		return info != null;
	}

	private void InitBones(Transform rigging)
	{
		Transform[] componentsInChildren = rigging.GetComponentsInChildren<Transform>(true);
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>(componentsInChildren.Length);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			dictionary[transform.name] = transform;
		}
		foreach (int value2 in Enum.GetValues(typeof(BoneIndex)))
		{
			string name = Enum.GetName(typeof(BoneIndex), (BoneIndex)value2);
			Transform value;
			if (dictionary.TryGetValue(name, out value))
			{
				_bones.Add(value2, value);
			}
		}
	}

	private void InitAnimations()
	{
		foreach (int value2 in Enum.GetValues(typeof(AnimationIndex)))
		{
			AnimationState animationState = _animation[Enum.GetName(typeof(AnimationIndex), (AnimationIndex)value2)];
			if (animationState != null)
			{
				_animations.Add(value2, new AnimationInfo((AnimationIndex)value2, animationState));
				Transform value;
				if (_bones.TryGetValue(GetMixingTransformBoneIndex((AnimationIndex)value2), out value))
				{
					animationState.AddMixingTransform(value);
				}
				animationState.wrapMode = GetAnimationWrapMode((AnimationIndex)value2);
				animationState.blendMode = GetAnimationBlendMode((AnimationIndex)value2);
				animationState.layer = GetAnimationLayer((AnimationIndex)value2);
			}
		}
	}

	private WrapMode GetAnimationWrapMode(AnimationIndex id)
	{
		switch (id)
		{
		case AnimationIndex.idle:
		case AnimationIndex.run:
		case AnimationIndex.lightGunBreathe:
		case AnimationIndex.heavyGunBreathe:
		case AnimationIndex.swimLoop:
		case AnimationIndex.walk:
		case AnimationIndex.crouch:
		case AnimationIndex.HomeNoWeaponIdle:
		case AnimationIndex.HomeNoWeaponnLookAround:
		case AnimationIndex.HomeNoWeaponRelaxNeck:
		case AnimationIndex.HomeMeleeIdle:
		case AnimationIndex.HomeMeleeLookAround:
		case AnimationIndex.HomeMeleeRelaxNeck:
		case AnimationIndex.HomeMeleeCheckWeapon:
		case AnimationIndex.HomeSmallGunIdle:
		case AnimationIndex.HomeSmallGunLookAround:
		case AnimationIndex.HomeSmallGunRelaxNeck:
		case AnimationIndex.HomeSmallGunCheckWeapon:
		case AnimationIndex.HomeMediumGunIdle:
		case AnimationIndex.HomeMediumGunLookAround:
		case AnimationIndex.HomeMediumGunRelaxNeck:
		case AnimationIndex.HomeMediumGunCheckWeapon:
		case AnimationIndex.HomeLargeGunIdle:
		case AnimationIndex.HomeLargeGunLookAround:
		case AnimationIndex.HomeLargeGunRelaxNeck:
		case AnimationIndex.HomeLargeGunCheckWeapon:
		case AnimationIndex.HomeLargeGunShakeWeapon:
		case AnimationIndex.ShopMeleeAimIdle:
		case AnimationIndex.ShopSmallGunAimIdle:
		case AnimationIndex.ShopSmallGunShoot:
		case AnimationIndex.ShopLargeGunAimIdle:
		case AnimationIndex.ShopLargeGunShoot:
		case AnimationIndex.ShopNewGloves:
		case AnimationIndex.ShopNewUpperBody:
		case AnimationIndex.ShopNewBoots:
		case AnimationIndex.ShopNewLowerBody:
		case AnimationIndex.ShopNewHead:
		case AnimationIndex.idleWalk:
		case AnimationIndex.TutorialGuideWalk:
		case AnimationIndex.TutorialGuideIdle:
			return WrapMode.Loop;
		case AnimationIndex.shootLightGun:
		case AnimationIndex.shootHeavyGun:
		case AnimationIndex.swimStart:
		case AnimationIndex.gotHit:
		case AnimationIndex.meleeSwingRightToLeft:
		case AnimationIndex.meleSwingLeftToRight:
			return WrapMode.Once;
		case AnimationIndex.jumpUp:
		case AnimationIndex.die1:
		case AnimationIndex.squat:
		case AnimationIndex.lightGunUpDown:
		case AnimationIndex.heavyGunUpDown:
		case AnimationIndex.snipeUpDown:
		case AnimationIndex.ShopMeleeTakeOut:
		case AnimationIndex.ShopSmallGunTakeOut:
		case AnimationIndex.ShopLargeGunTakeOut:
		case AnimationIndex.ShopHideGun:
		case AnimationIndex.ShopHideMelee:
			return WrapMode.ClampForever;
		default:
			return WrapMode.Default;
		}
	}

	private AnimationBlendMode GetAnimationBlendMode(AnimationIndex id)
	{
		if (id == AnimationIndex.shootLightGun || id == AnimationIndex.shootHeavyGun)
		{
			return AnimationBlendMode.Additive;
		}
		return AnimationBlendMode.Blend;
	}

	private int GetAnimationLayer(AnimationIndex id)
	{
		switch (id)
		{
		case AnimationIndex.swimLoop:
		case AnimationIndex.lightGunUpDown:
		case AnimationIndex.heavyGunUpDown:
		case AnimationIndex.snipeUpDown:
		case AnimationIndex.ShopSmallGunAimIdle:
			return 1;
		case AnimationIndex.meleeSwingRightToLeft:
		case AnimationIndex.meleSwingLeftToRight:
			return 2;
		case AnimationIndex.die1:
			return 5;
		default:
			return 0;
		}
	}

	private int GetMixingTransformBoneIndex(AnimationIndex id)
	{
		switch (id)
		{
		case AnimationIndex.shootLightGun:
		case AnimationIndex.shootHeavyGun:
		case AnimationIndex.lightGunUpDown:
		case AnimationIndex.heavyGunUpDown:
		case AnimationIndex.snipeUpDown:
		case AnimationIndex.meleeSwingRightToLeft:
		case AnimationIndex.meleSwingLeftToRight:
			return 9;
		default:
			return 0;
		}
	}
}
