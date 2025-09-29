using System.Collections.Generic;
using UnityEngine;

public class BitmapMeshText : CustomMesh
{
	private string _text;

	private Vector3 _offset;

	private Color _mainColor;

	private Color _shadowColor;

	private float _alphaMin;

	private float _alphaMax;

	private float _shadowAlphaMin;

	private float _shadowAlphaMax;

	private float _shadowOffsetU;

	private float _shadowOffsetV;

	private Vector3[] quadVerts = new Vector3[4]
	{
		new Vector3(0f, 0f),
		new Vector3(0f, 1f),
		new Vector3(1f, 1f),
		new Vector3(1f, 0f)
	};

	private Vector2[] quadUvs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f)
	};

	private int[] quadTriangles = new int[6] { 0, 1, 2, 2, 3, 0 };

	public BitmapFont Font { get; set; }

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			Reset();
		}
	}

	public TextAnchor Anchor { get; set; }

	public override Color Color
	{
		get
		{
			return MainColor;
		}
		set
		{
			MainColor = value;
		}
	}

	public override float Alpha
	{
		get
		{
			return MainColor.a;
		}
		set
		{
			Color mainColor = MainColor;
			mainColor.a = value;
			MainColor = mainColor;
			mainColor = ShadowColor;
			mainColor.a = value;
			ShadowColor = mainColor;
		}
	}

	public Vector2 OffsetToUpperLeft { get; private set; }

	public Color MainColor
	{
		get
		{
			return _mainColor;
		}
		set
		{
			_mainColor = value;
			if (_meshRenderer != null)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetColor("_Color", value);
				}
			}
		}
	}

	public Color ShadowColor
	{
		get
		{
			return _shadowColor;
		}
		set
		{
			_shadowColor = value;
			if (_meshRenderer != null)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetColor("_ShadowColor", value);
				}
			}
		}
	}

	public float AlphaMin
	{
		get
		{
			return _alphaMin;
		}
		set
		{
			_alphaMin = value;
			if (_meshRenderer != null)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_AlphaMin", value);
				}
			}
		}
	}

	public float AlphaMax
	{
		get
		{
			return _alphaMax;
		}
		set
		{
			_alphaMax = value;
			if (_meshRenderer != null)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_AlphaMax", value);
				}
			}
		}
	}

	public float ShadowAlphaMin
	{
		get
		{
			return _shadowAlphaMin;
		}
		set
		{
			_shadowAlphaMin = value;
			if ((bool)_meshRenderer)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_ShadowAlphaMin", value);
				}
			}
		}
	}

	public float ShadowAlphaMax
	{
		get
		{
			return _shadowAlphaMax;
		}
		set
		{
			_shadowAlphaMax = value;
			if ((bool)_meshRenderer)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_ShadowAlphaMax", value);
				}
			}
		}
	}

	public float ShadowOffsetU
	{
		get
		{
			return _shadowOffsetU;
		}
		set
		{
			_shadowOffsetU = value;
			if ((bool)_meshRenderer)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_ShadowOffsetU", value);
				}
			}
		}
	}

	public float ShadowOffsetV
	{
		get
		{
			return _shadowOffsetV;
		}
		set
		{
			_shadowOffsetV = value;
			if ((bool)_meshRenderer)
			{
				Material[] materials = _meshRenderer.materials;
				foreach (Material material in materials)
				{
					material.SetFloat("_ShadowOffsetV", value);
				}
			}
		}
	}

	protected override void Reset()
	{
		if (Font == null)
		{
			return;
		}
		if (_meshRenderer == null)
		{
			_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (_meshFilter == null)
		{
			_meshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		Vector3 vector = new Vector3(1f, 1f, 1f);
		Vector2 renderSize = new Vector2(vector.x, vector.y);
		_bounds = Font.CalculateSize(Text, renderSize);
		_offset = new Vector3(0f, 0f);
		if (Anchor == TextAnchor.UpperCenter || Anchor == TextAnchor.UpperLeft || Anchor == TextAnchor.UpperRight)
		{
			_offset.y = _bounds.y;
		}
		if (Anchor == TextAnchor.MiddleCenter || Anchor == TextAnchor.MiddleLeft || Anchor == TextAnchor.MiddleRight)
		{
			_offset.y = _bounds.y / 2f;
		}
		if (Anchor == TextAnchor.UpperRight || Anchor == TextAnchor.MiddleRight || Anchor == TextAnchor.LowerRight)
		{
			_offset.x = _bounds.x;
		}
		if (Anchor == TextAnchor.UpperCenter || Anchor == TextAnchor.MiddleCenter || Anchor == TextAnchor.LowerCenter)
		{
			_offset.x = _bounds.x / 2f;
		}
		OffsetToUpperLeft = new Vector2(_offset.x, _bounds.y - _offset.y);
		Mesh mesh = GenerateMesh();
		if (_meshFilter.sharedMesh != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(_meshFilter.sharedMesh);
			}
			else
			{
				Object.DestroyImmediate(_meshFilter.sharedMesh);
			}
		}
		_meshFilter.mesh = mesh;
		Material[] array = new Material[mesh.subMeshCount];
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			array[i] = Font.GetPageMaterial(i, _shader);
		}
		Material[] materials = _meshRenderer.materials;
		foreach (Material obj in materials)
		{
			Object.Destroy(obj);
		}
		_meshRenderer.materials = array;
		ResetMaterial();
	}

	protected override Mesh GenerateMesh()
	{
		Vector3 vector = new Vector3(1f, 1f, 1f);
		string text = Text;
		Vector3 vector2 = new Vector3(0f, 0f, 0f) - _offset;
		List<int> list = new List<int>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		Vector3 vector3 = vector2;
		Vector3 vector4 = vector / Font.Size;
		int num = 0;
		while (!string.IsNullOrEmpty(text) && num < text.Length)
		{
			char c = text[num];
			BitmapChar bitmapChar = Font.GetBitmapChar(c);
			int count = list2.Count;
			Rect uVRect = Font.GetUVRect(bitmapChar);
			Vector2 b = new Vector2(uVRect.width, uVRect.height);
			Vector2 vector5 = new Vector2(uVRect.x, uVRect.y);
			for (int i = 0; i < quadUvs.Length; i++)
			{
				list3.Add(Vector2.Scale(quadUvs[i], b) + vector5);
			}
			Vector3 b2 = Vector2.Scale(bitmapChar.Size, vector4);
			Vector3 vector6 = Vector2.Scale(bitmapChar.Offset, vector4);
			vector6.y = vector.y - (vector6.y + b2.y);
			for (int j = 0; j < quadVerts.Length; j++)
			{
				Vector3 item = Vector3.Scale(quadVerts[j], b2) + vector3 + vector6;
				list2.Add(item);
			}
			for (int k = 0; k < quadTriangles.Length; k++)
			{
				list.Add(quadTriangles[k] + count);
			}
			float num2 = 0f;
			if (num < Text.Length - 1)
			{
				num2 = Font.GetKerning(c, Text[num + 1]);
			}
			vector3.x += (bitmapChar.XAdvance + num2) * vector4.x;
			num++;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.subMeshCount = 1;
		mesh.SetTriangles(list.ToArray(), 0);
		return mesh;
	}

	private void ResetMaterial()
	{
		if (_meshRenderer != null)
		{
			Material[] materials = _meshRenderer.materials;
			foreach (Material material in materials)
			{
				material.SetColor("_Color", _mainColor);
				material.SetColor("_ShadowColor", _shadowColor);
				material.SetFloat("_AlphaMin", _alphaMin);
				material.SetFloat("_AlphaMax", _alphaMax);
				material.SetFloat("_ShadowAlphaMin", _shadowAlphaMin);
				material.SetFloat("_ShadowAlphaMax", _shadowAlphaMax);
				material.SetFloat("_ShadowOffsetU", _shadowOffsetU);
				material.SetFloat("_ShadowOffsetV", _shadowOffsetV);
			}
		}
	}
}
