using UnityEngine;

public abstract class CustomMesh : MonoBehaviour
{
	protected bool _isMaterialInitialized;

	protected Vector2 _bounds;

	protected MeshFilter _meshFilter;

	protected MeshRenderer _meshRenderer;

	protected Color _color;

	protected Texture _texture;

	[SerializeField]
	protected Shader _shader;

	public Vector2 Bounds
	{
		get
		{
			return _bounds;
		}
	}

	public bool IsVisible
	{
		get
		{
			return _meshRenderer != null && _meshRenderer.enabled;
		}
		set
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.enabled = value;
			}
		}
	}

	public virtual Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
			if (_meshRenderer != null)
			{
				_meshRenderer.material.SetColor("_Color", value);
			}
		}
	}

	public virtual float Alpha
	{
		get
		{
			return _color.a;
		}
		set
		{
			_color.a = value;
			if (_meshRenderer != null)
			{
				_meshRenderer.material.SetColor("_Color", _color);
			}
		}
	}

	public Texture Texture
	{
		get
		{
			return _texture;
		}
		set
		{
			_texture = value;
			if (_meshRenderer != null)
			{
				_meshRenderer.material.SetTexture("_MainTex", value);
			}
		}
	}

	protected virtual void Reset()
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
			_meshRenderer.material = new Material(_shader);
			_meshRenderer.enabled = IsVisible;
			_meshRenderer.material.SetColor("_Color", _color);
			_meshRenderer.material.SetTexture("_MainTex", _texture);
			_isMaterialInitialized = true;
		}
	}

	protected abstract Mesh GenerateMesh();
}
