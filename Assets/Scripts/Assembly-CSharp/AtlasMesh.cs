using System.Collections.Generic;
using UnityEngine;

public class AtlasMesh : CustomMesh
{
	private Vector3[] quadVerts = new Vector3[4]
	{
		new Vector3(0f, -1f),
		new Vector3(0f, 0f),
		new Vector3(1f, 0f),
		new Vector3(1f, -1f)
	};

	private Vector2[] quadUvs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f)
	};

	private int[] quadTriangles = new int[6] { 0, 1, 2, 2, 3, 0 };

	private Rect _uvOffset;

	private TextAnchor _anchor;

	private Vector2 _offset;

	public Rect UVOffset
	{
		get
		{
			return _uvOffset;
		}
		set
		{
			_uvOffset = value;
			quadUvs = new Vector2[4]
			{
				new Vector2(value.x, value.y),
				new Vector2(value.x, value.y + value.height),
				new Vector2(value.x + value.width, value.y + value.height),
				new Vector2(value.x + value.width, value.y)
			};
			Reset();
		}
	}

	public Material Material
	{
		get
		{
			return _meshRenderer.sharedMaterial;
		}
		set
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.sharedMaterial = value;
			}
		}
	}

	public TextAnchor Anchor
	{
		get
		{
			return _anchor;
		}
		set
		{
			_anchor = value;
			Reset();
		}
	}

	public Vector2 OffsetToUpperLeft { get; private set; }

	protected override Mesh GenerateMesh()
	{
		_bounds = Vector2.one;
		CalculateOffset();
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < quadVerts.Length; i++)
		{
			list.Add(new Vector3
			{
				x = quadVerts[i].x,
				y = quadVerts[i].y
			});
		}
		mesh.vertices = list.ToArray();
		mesh.uv = quadUvs;
		mesh.subMeshCount = 1;
		mesh.SetTriangles(quadTriangles, 0);
		return mesh;
	}

	private void Awake()
	{
		Reset();
	}

	private void CalculateOffset()
	{
		_offset = Vector2.zero;
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
	}

	protected override void Reset()
	{
		if (_meshRenderer == null)
		{
			_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (_meshFilter == null)
		{
			_meshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
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
		if (!_isMaterialInitialized)
		{
			_meshRenderer.sharedMaterial = MobileIcons.TextureAtlas;
			_meshRenderer.enabled = base.IsVisible;
			_isMaterialInitialized = true;
		}
	}
}
