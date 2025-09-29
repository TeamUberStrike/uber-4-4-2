using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class AvatarDecorator : MonoBehaviour
{
	public const float DiveFootstep = 3.5f;

	public const float SwimFootstep = 1.7f;

	public const float WaterFootstep = 1.4f;

	private Transform _transform;

	private Animation _animation;

	[SerializeField]
	private CharacterHitArea[] _hitAreas;

	[SerializeField]
	private Transform _weaponAttachPoint;

	private FootStepSoundType _footStep;

	private LoadoutSlotType _currentWeaponSlot;

	private UberstrikeLayer _layer;

	private float _nextFootStepTime;

	private Dictionary<LoadoutSlotType, BaseWeaponDecorator> _weapons;

	private AvatarDecoratorConfig _configuration;

	public AvatarDecoratorConfig Configuration
	{
		get
		{
			if (_configuration == null)
			{
				_configuration = GetComponent<AvatarDecoratorConfig>();
			}
			return _configuration;
		}
	}

	public CharacterHitArea[] HitAreas
	{
		get
		{
			return _hitAreas;
		}
		set
		{
			_hitAreas = value;
		}
	}

	public Renderer MeshRenderer
	{
		get
		{
			return base.renderer;
		}
	}

	public Animation Animation
	{
		get
		{
			return _animation;
		}
	}

	public Transform WeaponAttachPoint
	{
		get
		{
			return _weaponAttachPoint;
		}
		set
		{
			_weaponAttachPoint = value;
		}
	}

	public LoadoutSlotType CurrentWeaponSlot
	{
		get
		{
			return _currentWeaponSlot;
		}
	}

	public AvatarHudInformation HudInformation { get; private set; }

	public AvatarAnimationController AnimationController { get; private set; }

	public bool CanPlayFootSound
	{
		get
		{
			return _nextFootStepTime < Time.time;
		}
	}

	private void Awake()
	{
		_transform = base.transform;
		_currentWeaponSlot = LoadoutSlotType.None;
		_weapons = new Dictionary<LoadoutSlotType, BaseWeaponDecorator>();
		_animation = GetComponent<Animation>();
		if ((bool)_animation)
		{
			AnimationController = new AvatarAnimationController(_animation);
		}
		HudInformation = GetComponentInChildren<AvatarHudInformation>();
	}

	public void SetSkinColor(Color color)
	{
		Configuration.SkinColor = color;
	}

	public void EnableOutline(bool showOutline)
	{
		if (OutlineEffectController.Exists)
		{
			if (showOutline)
			{
				OutlineEffectController.Instance.AddOutlineObject(base.gameObject, Configuration.MaterialGroup, ColorScheme.TeamOutline);
			}
			else
			{
				OutlineEffectController.Instance.RemoveOutlineObject(base.gameObject);
			}
		}
	}

	public void SetShotFeedback(BodyPart bodyPart)
	{
		if ((bool)HudInformation)
		{
			switch (bodyPart)
			{
			case BodyPart.Head:
				HudInformation.SetInGameFeedback(InGameEventFeedbackType.HeadShot);
				break;
			case BodyPart.Nuts:
				HudInformation.SetInGameFeedback(InGameEventFeedbackType.NutShot);
				break;
			case BodyPart.Body | BodyPart.Head:
				break;
			}
		}
	}

	public void AssignWeapon(LoadoutSlotType slot, BaseWeaponDecorator decorator)
	{
		if ((bool)decorator)
		{
			BaseWeaponDecorator value;
			if (_weapons.TryGetValue(slot, out value) && (bool)value)
			{
				Object.Destroy(value.gameObject);
			}
			_weapons[slot] = decorator;
			decorator.transform.parent = _weaponAttachPoint;
			LayerUtil.SetLayerRecursively(decorator.gameObject.transform, _layer);
			decorator.transform.localPosition = Vector3.zero;
			decorator.transform.localRotation = Quaternion.identity;
			decorator.IsEnabled = slot == _currentWeaponSlot;
		}
		else
		{
			UnassignWeapon(slot);
		}
	}

	public void UnassignWeapon(LoadoutSlotType slot)
	{
		BaseWeaponDecorator value;
		if (_weapons.TryGetValue(slot, out value) && (bool)value)
		{
			Object.Destroy(value.gameObject);
		}
		_weapons.Remove(slot);
	}

	public void SetActiveWeaponSlot(LoadoutSlotType slot)
	{
		_currentWeaponSlot = slot;
	}

	public void ShowWeapon(LoadoutSlotType slot)
	{
		_currentWeaponSlot = slot;
		foreach (KeyValuePair<LoadoutSlotType, BaseWeaponDecorator> weapon in _weapons)
		{
			if ((bool)weapon.Value)
			{
				weapon.Value.IsEnabled = slot == weapon.Key;
			}
		}
	}

	public void HideWeapons()
	{
		foreach (BaseWeaponDecorator value in _weapons.Values)
		{
			if ((bool)value)
			{
				value.IsEnabled = false;
			}
		}
	}

	public void SetLayers(UberstrikeLayer layer)
	{
		_layer = layer;
		UpdateLayers();
	}

	public void UpdateLayers()
	{
		LayerUtil.SetLayerRecursively(base.transform, _layer);
	}

	public Transform GetBone(BoneIndex bone)
	{
		return Configuration.GetBone(bone);
	}

	public void SetPosition(Vector3 position, Quaternion rotation)
	{
		base.transform.localPosition = position;
		base.transform.localRotation = rotation;
	}

	public void SetFootStep(FootStepSoundType sound)
	{
		_footStep = sound;
	}

	public void PlayFootSound(float length)
	{
		if (CanPlayFootSound)
		{
			PlayFootSound(_footStep, length);
		}
	}

	public void PlayFootSound(FootStepSoundType sound, float length)
	{
		switch (sound)
		{
		case FootStepSoundType.Dive:
			length *= 3.5f;
			break;
		case FootStepSoundType.Swim:
			length *= 1.7f;
			break;
		case FootStepSoundType.Water:
			length *= 1.4f;
			break;
		}
		_nextFootStepTime = Time.time + length;
		AutoMonoBehaviour<SfxManager>.Instance.PlayFootStepAudioClip(sound, _transform.position);
	}

	public void PlayDieSound()
	{
		int num = Random.Range(0, 3);
		AudioClip audioClip = GameAudio.NormalKill1;
		switch (num)
		{
		case 0:
			audioClip = GameAudio.NormalKill1;
			break;
		case 1:
			audioClip = GameAudio.NormalKill2;
			break;
		case 3:
			audioClip = GameAudio.NormalKill3;
			break;
		}
		SfxManager.Play3dAudioClip(audioClip, _transform.position);
	}
}
