using System.Collections.Generic;
using UnityEngine;

public class AvatarDecoratorConfig : MonoBehaviour
{
	[SerializeField]
	private AvatarBone[] _avatarBones;

	private Color _skinColor;

	private List<Material> _materials;

	public List<Material> MaterialGroup
	{
		get
		{
			return _materials;
		}
	}

	public Color SkinColor
	{
		get
		{
			return _skinColor;
		}
		set
		{
			_skinColor = value;
			UpdateMaterials();
			foreach (Material material in _materials)
			{
				if ((bool)material && material.name.Contains("Skin"))
				{
					material.color = _skinColor;
				}
			}
		}
	}

	public IEnumerable<AvatarBone> Bones
	{
		get
		{
			return _avatarBones;
		}
	}

	private void Awake()
	{
		_materials = new List<Material>();
		AvatarBone[] avatarBones = _avatarBones;
		foreach (AvatarBone avatarBone in avatarBones)
		{
			avatarBone.Collider = avatarBone.Transform.GetComponent<Collider>();
			avatarBone.Rigidbody = avatarBone.Transform.GetComponent<Rigidbody>();
			avatarBone.OriginalPosition = avatarBone.Transform.localPosition;
			avatarBone.OriginalRotation = avatarBone.Transform.localRotation;
		}
	}

	public Transform GetBone(BoneIndex bone)
	{
		AvatarBone[] avatarBones = _avatarBones;
		foreach (AvatarBone avatarBone in avatarBones)
		{
			if (avatarBone.Bone == bone)
			{
				return avatarBone.Transform;
			}
		}
		return base.transform;
	}

	public void SetBones(List<AvatarBone> bones)
	{
		_avatarBones = bones.ToArray();
	}

	public void UpdateMaterials()
	{
		SkinnedMeshRenderer componentInChildren = GetComponentInChildren<SkinnedMeshRenderer>();
		if ((bool)componentInChildren)
		{
			_materials.Clear();
			Material[] materials = componentInChildren.materials;
			foreach (Material item in materials)
			{
				_materials.Add(item);
			}
		}
	}

	public static void CopyBones(AvatarDecoratorConfig srcAvatar, AvatarDecoratorConfig dstAvatar)
	{
		AvatarBone[] avatarBones = srcAvatar._avatarBones;
		foreach (AvatarBone avatarBone in avatarBones)
		{
			Transform bone = dstAvatar.GetBone(avatarBone.Bone);
			if ((bool)bone)
			{
				bone.position = avatarBone.Transform.position;
				bone.rotation = avatarBone.Transform.rotation;
			}
		}
	}
}
