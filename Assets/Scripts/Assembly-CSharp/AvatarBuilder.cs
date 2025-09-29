using System;
using UnityEngine;

public class AvatarBuilder : Singleton<AvatarBuilder>
{
	private AvatarBuilder()
	{
	}

	public static void Destroy(GameObject obj)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Material[] materials = renderer.materials;
			foreach (Material obj2 in materials)
			{
				UnityEngine.Object.Destroy(obj2);
			}
		}
		SkinnedMeshRenderer componentInChildren = obj.GetComponentInChildren<SkinnedMeshRenderer>();
		if ((bool)componentInChildren)
		{
			UnityEngine.Object.Destroy(componentInChildren.sharedMesh);
		}
		UnityEngine.Object.Destroy(obj);
	}

	public AvatarDecorator CreateLocalAvatar()
	{
		return CreateLocalAvatar(Singleton<LoadoutManager>.Instance.GearLoadout.GetAvatarGear());
	}

	public AvatarDecorator CreateLocalAvatar(AvatarGearParts gear)
	{
		AvatarDecorator avatarDecorator = CreateAvatarMesh(gear);
		SetupLocalAvatar(avatarDecorator);
		return avatarDecorator;
	}

	public AvatarDecorator CreateRemoteAvatar(AvatarGearParts gear, Color skinColor)
	{
		AvatarDecorator avatarDecorator = CreateAvatarMesh(gear);
		SetupRemoteAvatar(avatarDecorator, skinColor);
		return avatarDecorator;
	}

	public AvatarDecoratorConfig CreateRagdoll(AvatarGearParts gear, Color skinColor)
	{
		SkinnedMeshCombiner.Combine(gear.Base, gear.Parts);
		LayerUtil.SetLayerRecursively(gear.Base.transform, UberstrikeLayer.Ragdoll);
		SkinnedMeshRenderer[] componentsInChildren = gear.Base.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			skinnedMeshRenderer.updateWhenOffscreen = true;
		}
		gear.Parts.ForEach(delegate(GameObject obj)
		{
			UnityEngine.Object.Destroy(obj);
		});
		AvatarDecoratorConfig component = gear.Base.GetComponent<AvatarDecoratorConfig>();
		if ((bool)component)
		{
			component.SkinColor = skinColor;
		}
		return component;
	}

	public void UpdateLocalAvatar(AvatarGearParts gear)
	{
		if ((bool)GameState.LocalAvatar.Decorator)
		{
			UpdateAvatarMesh(GameState.LocalAvatar.Decorator, gear);
			SetupLocalAvatar(GameState.LocalAvatar.Decorator);
		}
		else
		{
			Debug.LogError("No local Player created yet! Call 'CreateLocalPlayerAvatar' first!");
		}
	}

	public void UpdateRemoteAvatar(AvatarDecorator decorator, AvatarGearParts gear, Color skinColor)
	{
		UpdateAvatarMesh(decorator, gear);
		SetupRemoteAvatar(decorator, skinColor);
	}

	private void UpdateAvatarMesh(AvatarDecorator avatar, AvatarGearParts gear)
	{
		if (!avatar)
		{
			Debug.LogError("AvatarDecorator is null!");
			return;
		}
		gear.Parts.Add(gear.Base);
		foreach (int value in Enum.GetValues(typeof(BoneIndex)))
		{
			Transform bone2 = avatar.GetBone((BoneIndex)value);
			for (int i = 0; i < bone2.childCount; i++)
			{
				Transform child = bone2.GetChild(i);
				if ((bool)child)
				{
					ParticleSystem component = child.GetComponent<ParticleSystem>();
					if ((bool)component)
					{
						UnityEngine.Object.Destroy(component.gameObject);
					}
				}
			}
		}
		SkinnedMeshCombiner.Update(avatar.gameObject, gear.Parts);
		avatar.MeshRenderer.receiveShadows = false;
		gear.Parts.ForEach(delegate(GameObject obj)
		{
			UnityEngine.Object.Destroy(obj);
		});
	}

	private AvatarDecorator CreateAvatarMesh(AvatarGearParts gear)
	{
		UnityEngine.Object.DontDestroyOnLoad(gear.Base);
		SkinnedMeshCombiner.Combine(gear.Base, gear.Parts);
		gear.Parts.ForEach(delegate(GameObject obj)
		{
			UnityEngine.Object.Destroy(obj);
		});
		return gear.Base.GetComponent<AvatarDecorator>();
	}

	private void SetupLocalAvatar(AvatarDecorator avatar)
	{
		if ((bool)avatar)
		{
			avatar.UpdateLayers();
			avatar.HudInformation.DistanceCap = 100f;
			avatar.SetSkinColor(PlayerDataManager.SkinColor);
			avatar.HudInformation.SetAvatarLabel(PlayerDataManager.NameAndTag);
			SkinnedMeshRenderer componentInChildren = avatar.GetComponentInChildren<SkinnedMeshRenderer>();
			if ((bool)componentInChildren)
			{
				componentInChildren.castShadows = true;
				componentInChildren.receiveShadows = false;
			}
			LoadoutSlotType[] weaponSlots = LoadoutManager.WeaponSlots;
			foreach (LoadoutSlotType slot in weaponSlots)
			{
				InventoryItem item;
				if (Singleton<LoadoutManager>.Instance.TryGetItemInSlot(slot, out item))
				{
					GameObject gameObject = item.Item.Create(avatar.WeaponAttachPoint.position, avatar.WeaponAttachPoint.rotation);
					avatar.AssignWeapon(slot, gameObject.GetComponent<BaseWeaponDecorator>());
				}
			}
			avatar.ShowWeapon(LoadoutSlotType.None);
		}
		else
		{
			Debug.LogError("No AvatarDecorator to setup!");
		}
	}

	private void SetupRemoteAvatar(AvatarDecorator avatar, Color skinColor)
	{
		if ((bool)avatar)
		{
			avatar.SetLayers(UberstrikeLayer.RemotePlayer);
			avatar.SetSkinColor(skinColor);
			avatar.HudInformation.SetTarget(avatar.GetBone(BoneIndex.HeadTop));
			avatar.ShowWeapon(avatar.CurrentWeaponSlot);
		}
	}
}
