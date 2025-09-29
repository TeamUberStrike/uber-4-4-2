using UnityEngine;

public class Avatar
{
	private bool _isLocal;

	private ILoadout _loadout;

	public ILoadout Loadout
	{
		get
		{
			return _loadout;
		}
		set
		{
			_loadout = value;
			_loadout.OnGearChanged += RebuildDecorator;
			_loadout.OnWeaponChanged += UpdateWeapon;
			RebuildDecorator();
		}
	}

	public AvatarDecorator Decorator { get; set; }

	public AvatarDecoratorConfig Ragdoll { get; private set; }

	public Avatar(ILoadout loadout, bool local)
	{
		_isLocal = local;
		Loadout = loadout;
	}

	public void RebuildDecorator()
	{
		if ((bool)Decorator)
		{
			AvatarGearParts avatarGear = Loadout.GetAvatarGear();
			if (_isLocal)
			{
				Singleton<AvatarBuilder>.Instance.UpdateLocalAvatar(avatarGear);
			}
			else
			{
				Singleton<AvatarBuilder>.Instance.UpdateRemoteAvatar(Decorator, avatarGear, Color.white);
			}
		}
	}

	public void EnableDecorator()
	{
		DestroyRagdoll();
		Decorator.gameObject.SetActive(true);
	}

	public void SpawnRagdoll(Vector3 force)
	{
		Ragdoll = Singleton<AvatarBuilder>.Instance.CreateRagdoll(_loadout.GetRagdollGear(), Color.white);
		if (!Ragdoll || !Decorator)
		{
			return;
		}
		Ragdoll.SkinColor = Decorator.Configuration.SkinColor;
		Ragdoll.transform.position = Decorator.transform.position;
		Ragdoll.transform.rotation = Decorator.transform.rotation;
		AvatarDecoratorConfig.CopyBones(Decorator.Configuration, Ragdoll);
		ArrowProjectile[] componentsInChildren = Decorator.GetComponentsInChildren<ArrowProjectile>(true);
		foreach (ArrowProjectile arrowProjectile in componentsInChildren)
		{
			Vector3 localPosition = arrowProjectile.transform.localPosition;
			Quaternion localRotation = arrowProjectile.transform.localRotation;
			arrowProjectile.transform.parent = Ragdoll.GetBone(BoneIndex.Hips);
			arrowProjectile.transform.localPosition = localPosition;
			arrowProjectile.transform.localRotation = localRotation;
		}
		foreach (AvatarBone bone in Ragdoll.Bones)
		{
			if ((bool)bone.Rigidbody)
			{
				bone.Rigidbody.isKinematic = false;
				if (bone.Bone == BoneIndex.Hips)
				{
					bone.Rigidbody.AddForce(force / 100f);
				}
				else
				{
					bone.Rigidbody.AddForce(force / 300f, ForceMode.VelocityChange);
				}
				if (GameState.IsRagdollShootable)
				{
					bone.Transform.gameObject.layer = 21;
				}
			}
		}
		Decorator.gameObject.SetActive(false);
	}

	private void DestroyRagdoll()
	{
		if ((bool)Ragdoll)
		{
			AvatarBuilder.Destroy(Ragdoll.gameObject);
			Ragdoll = null;
		}
	}

	private void UpdateWeapon(LoadoutSlotType slot)
	{
		IUnityItem item;
		if (_loadout.TryGetItem(slot, out item) && (bool)Decorator && (bool)Decorator.WeaponAttachPoint)
		{
			GameObject gameObject = item.Create(Decorator.WeaponAttachPoint.position, Decorator.WeaponAttachPoint.rotation);
			Decorator.AssignWeapon(slot, gameObject.GetComponent<BaseWeaponDecorator>());
		}
	}
}
