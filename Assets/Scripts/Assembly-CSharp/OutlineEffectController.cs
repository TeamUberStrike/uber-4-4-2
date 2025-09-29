using System.Collections.Generic;
using UnityEngine;

public class OutlineEffectController : MonoBehaviour
{
	private class OutlineProperty
	{
		private List<Material> _materialGroup;

		public Color OutlineColor { get; set; }

		public float OutlineSize { get; set; }

		public List<Material> MaterialGroup
		{
			get
			{
				return _materialGroup;
			}
		}

		public OutlineProperty(Color outlineColor, float outlineSize, List<Material> materialGroup)
		{
			OutlineColor = outlineColor;
			OutlineSize = outlineSize;
			_materialGroup = materialGroup;
		}
	}

	private Dictionary<GameObject, OutlineProperty> _outlinedGroup;

	private int _distanceToAttenuateOutline = 3;

	private int _distanceToHideOutline = 60;

	[SerializeField]
	private Material _outlineMaterial;

	public static OutlineEffectController Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	private OutlineEffectController()
	{
		_outlinedGroup = new Dictionary<GameObject, OutlineProperty>();
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		Camera main = Camera.main;
		if (main == null)
		{
			return;
		}
		foreach (KeyValuePair<GameObject, OutlineProperty> item in _outlinedGroup)
		{
			GameObject key = item.Key;
			if (key == null)
			{
				_outlinedGroup.Remove(key);
				break;
			}
			OutlineProperty value = item.Value;
			float f = 1f - Mathf.Clamp((main.WorldToScreenPoint(key.transform.position).z - (float)_distanceToAttenuateOutline) / (float)(_distanceToHideOutline - _distanceToAttenuateOutline), 0f, 1f);
			f = Mathf.Pow(f, 3f);
			f *= main.fieldOfView / 60f;
			foreach (Material item2 in value.MaterialGroup)
			{
				item2.SetFloat("_Outline_Size", value.OutlineSize * f);
			}
		}
	}

	public void AddOutlineObject(GameObject gameObject, List<Material> materialGroup, Color outlineColor, float outlineSize = 0.01f)
	{
		if (!_outlinedGroup.ContainsKey(gameObject) && !(_outlineMaterial == null))
		{
			OutlineProperty outlineProperty = new OutlineProperty(outlineColor, outlineSize, materialGroup);
			_outlinedGroup.Add(gameObject, outlineProperty);
			SetOutlineMaterial(outlineProperty);
		}
	}

	public void RemoveOutlineObject(GameObject gameObject)
	{
		OutlineProperty value = null;
		_outlinedGroup.TryGetValue(gameObject, out value);
		if (value != null)
		{
			SetDefaultMaterial(value.MaterialGroup);
			_outlinedGroup.Remove(gameObject);
		}
	}

	private void SetOutlineMaterial(OutlineProperty outlineProp)
	{
		if (_outlineMaterial == null)
		{
			return;
		}
		foreach (Material item in outlineProp.MaterialGroup)
		{
			item.shader = _outlineMaterial.shader;
			item.SetFloat("_Outline_Size", outlineProp.OutlineSize);
			item.SetColor("_Outline_Color", outlineProp.OutlineColor);
		}
	}

	private void SetOutlineSize(OutlineProperty outlineProp, float size)
	{
		foreach (Material item in outlineProp.MaterialGroup)
		{
			item.shader = _outlineMaterial.shader;
			item.SetFloat("_Outline_Size", outlineProp.OutlineSize);
		}
	}

	private void SetDefaultMaterial(List<Material> materialGroup)
	{
		foreach (Material item in materialGroup)
		{
			item.shader = Shader.Find("Diffuse");
		}
	}
}
