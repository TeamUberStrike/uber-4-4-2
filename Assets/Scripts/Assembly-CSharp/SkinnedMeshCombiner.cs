using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshCombiner
{
	public static void Combine(GameObject target, List<GameObject> objects)
	{
		if (!target || objects == null)
		{
			return;
		}
		CopyComponents(target, objects);
		List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
		foreach (GameObject @object in objects)
		{
			if (@object != null)
			{
				list.AddRange(@object.GetComponentsInChildren<SkinnedMeshRenderer>(true));
			}
		}
		SuperCombineCreate(target, list);
	}

	public static void Update(GameObject target, List<GameObject> objects)
	{
		if (!target || objects == null)
		{
			return;
		}
		CopyComponents(target, objects);
		List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
		foreach (GameObject @object in objects)
		{
			if ((bool)@object)
			{
				list.AddRange(@object.GetComponentsInChildren<SkinnedMeshRenderer>(true));
			}
		}
		SuperCombineUpdate(target, list);
	}

	private static void CopyComponents(GameObject target, List<GameObject> objects)
	{
		List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
		renderers.AddRange(target.GetComponentsInChildren<SkinnedMeshRenderer>(true));
		objects.ForEach(delegate(GameObject obj)
		{
			renderers.AddRange(obj.GetComponentsInChildren<SkinnedMeshRenderer>(true));
		});
		CopyAnimation(target, renderers);
		List<AudioSource> audioSources = new List<AudioSource>();
		objects.ForEach(delegate(GameObject obj)
		{
			audioSources.AddRange(obj.GetComponentsInChildren<AudioSource>(true));
		});
		CopyAudio(target, audioSources);
		List<ParticleSystem> particles = new List<ParticleSystem>();
		objects.ForEach(delegate(GameObject obj)
		{
			particles.Add(obj.GetComponentInChildren<ParticleSystem>());
		});
		CopyParticleSystems(target, particles);
	}

	private static void CopyAnimation(GameObject target, List<SkinnedMeshRenderer> renderers)
	{
		foreach (SkinnedMeshRenderer renderer in renderers)
		{
			if (!(renderer.transform != target.transform) || !renderer.animation)
			{
				continue;
			}
			Animation animation = renderer.animation;
			if (animation.enabled && (bool)animation.clip)
			{
				Animation animation2 = target.animation;
				if (!animation2)
				{
					animation2 = target.AddComponent<Animation>();
				}
				string text = string.Format("{0}-{1}", renderer.name, animation.clip.name);
				if (!animation2.GetClip(text))
				{
					animation2.AddClip(animation.clip, text);
				}
				animation2[text].wrapMode = animation[animation.clip.name].wrapMode;
				animation2[text].enabled = true;
				animation2[text].weight = 1f;
			}
		}
	}

	private static void CopyAudio(GameObject target, List<AudioSource> audio)
	{
		foreach (AudioSource item in audio)
		{
			item.transform.parent = target.transform;
		}
	}

	private static void CopyParticleSystems(GameObject target, List<ParticleSystem> particleSystems)
	{
		AvatarDecoratorConfig component = target.GetComponent<AvatarDecoratorConfig>();
		if (!component)
		{
			return;
		}
		List<string> list = new List<string>(Enum.GetNames(typeof(BoneIndex)));
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			if (!particleSystem)
			{
				continue;
			}
			Transform transform = particleSystem.transform;
			Transform particleParent = transform.parent;
			Vector3 localPosition = transform.localPosition;
			while ((bool)particleParent)
			{
				string value = list.Find((string value2) => particleParent.name.Equals(value2));
				if (!string.IsNullOrEmpty(value))
				{
					Transform bone = component.GetBone((BoneIndex)(int)Enum.Parse(typeof(BoneIndex), value));
					transform.parent = bone;
					transform.localPosition = localPosition;
					break;
				}
				transform = particleParent;
				particleParent = particleParent.parent;
			}
		}
	}

	private static GameObject SuperCombineCreate(GameObject sourceGameObject, List<SkinnedMeshRenderer> otherGear)
	{
		foreach (SkinnedMeshRenderer item in otherGear)
		{
			if (item.sharedMesh == null)
			{
				Debug.LogError(item.name + "'s sharedMesh is null!");
			}
		}
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		List<Transform> list3 = new List<Transform>();
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Transform[] componentsInChildren = sourceGameObject.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			dictionary.Add(transform.name, transform.transform);
		}
		SkinnedMeshRenderer[] componentsInChildren2 = sourceGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
		{
			list2.AddRange(skinnedMeshRenderer.sharedMaterials);
			for (int k = 0; k < skinnedMeshRenderer.sharedMesh.subMeshCount; k++)
			{
				list.Add(new CombineInstance
				{
					mesh = skinnedMeshRenderer.sharedMesh,
					subMeshIndex = k
				});
				list3.AddRange(skinnedMeshRenderer.bones);
			}
			UnityEngine.Object.Destroy(skinnedMeshRenderer);
		}
		if (otherGear != null && otherGear.Count > 0)
		{
			foreach (SkinnedMeshRenderer item2 in otherGear)
			{
				list2.AddRange(item2.sharedMaterials);
				if (item2.sharedMesh == null)
				{
					continue;
				}
				for (int l = 0; l < item2.sharedMesh.subMeshCount; l++)
				{
					list.Add(new CombineInstance
					{
						mesh = item2.sharedMesh,
						subMeshIndex = l
					});
					Transform[] bones = item2.bones;
					foreach (Transform transform2 in bones)
					{
						if (dictionary.ContainsKey(transform2.name))
						{
							list3.Add(dictionary[transform2.name]);
						}
						else
						{
							Debug.LogError("I couldn't find a matching bone transform in the gameobject you're trying to add this skinned mesh to! " + transform2.name);
						}
					}
				}
			}
		}
		SkinnedMeshRenderer skinnedMeshRenderer2 = sourceGameObject.AddComponent<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer2.sharedMesh == null)
		{
			skinnedMeshRenderer2.sharedMesh = new Mesh();
		}
		skinnedMeshRenderer2.sharedMesh.Clear();
		skinnedMeshRenderer2.sharedMesh.name = "CombinedMesh";
		skinnedMeshRenderer2.sharedMesh.CombineMeshes(list.ToArray(), false, false);
		skinnedMeshRenderer2.bones = list3.ToArray();
		Material[] materials = skinnedMeshRenderer2.materials;
		foreach (Material obj in materials)
		{
			UnityEngine.Object.Destroy(obj);
		}
		skinnedMeshRenderer2.materials = list2.ToArray();
		Animation component = sourceGameObject.GetComponent<Animation>();
		if ((bool)component)
		{
			component.cullingType = AnimationCullingType.AlwaysAnimate;
		}
		return sourceGameObject;
	}

	private static GameObject SuperCombineUpdate(GameObject sourceGameObject, List<SkinnedMeshRenderer> otherGear)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		List<Transform> list3 = new List<Transform>();
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Transform[] componentsInChildren = sourceGameObject.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!dictionary.ContainsKey(transform.name))
			{
				dictionary.Add(transform.name, transform.transform);
			}
		}
		if (otherGear != null && otherGear.Count > 0)
		{
			foreach (SkinnedMeshRenderer item in otherGear)
			{
				list2.AddRange(item.sharedMaterials);
				if (item.sharedMesh == null)
				{
					Debug.Log("No shared mesh in " + item.name);
					continue;
				}
				for (int j = 0; j < item.sharedMesh.subMeshCount; j++)
				{
					list.Add(new CombineInstance
					{
						mesh = item.sharedMesh,
						subMeshIndex = j
					});
					Transform[] bones = item.bones;
					foreach (Transform transform2 in bones)
					{
						if (dictionary.ContainsKey(transform2.name))
						{
							Transform transform3 = dictionary[transform2.name];
							transform3.localPosition = transform2.localPosition;
							list3.Add(transform3);
						}
						else
						{
							Debug.LogError("I couldn't find a matching bone transform in the gameobject you're trying to add this skinned mesh to! " + transform2.name);
						}
					}
				}
			}
		}
		else
		{
			Debug.LogError("Gear array contains no Skinned Meshes! Trying to go naked?");
		}
		SkinnedMeshRenderer component = sourceGameObject.GetComponent<SkinnedMeshRenderer>();
		if ((bool)component)
		{
			if (component.sharedMesh == null)
			{
				component.sharedMesh = new Mesh();
			}
			component.sharedMesh.Clear();
			component.sharedMesh.name = "CombinedMesh";
			component.sharedMesh.CombineMeshes(list.ToArray(), false, false);
			component.bones = list3.ToArray();
			Material[] materials = component.materials;
			foreach (Material obj in materials)
			{
				UnityEngine.Object.Destroy(obj);
			}
			component.materials = list2.ToArray();
		}
		else
		{
			Debug.LogError("There is no SkinnedMeshRenderer on " + sourceGameObject.name);
		}
		Animation component2 = sourceGameObject.GetComponent<Animation>();
		if ((bool)component2)
		{
			component2.cullingType = AnimationCullingType.AlwaysAnimate;
		}
		return sourceGameObject;
	}
}
