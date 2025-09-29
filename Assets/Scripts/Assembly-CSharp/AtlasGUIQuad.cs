using UnityEngine;

public class AtlasGUIQuad : MeshGUIBase
{
	public float Width
	{
		get
		{
			return (float)AtlasMesh.Material.mainTexture.width * AtlasMesh.UVOffset.width;
		}
	}

	public float Height
	{
		get
		{
			return (float)AtlasMesh.Material.mainTexture.height * AtlasMesh.UVOffset.height;
		}
	}

	public AtlasMesh AtlasMesh
	{
		get
		{
			return _customMesh as AtlasMesh;
		}
	}

	public AtlasGUIQuad(Material mat, Rect uvOffset, TextAnchor anchor = TextAnchor.UpperLeft, GameObject parentObject = null)
		: base(parentObject)
	{
		AtlasMesh.UVOffset = uvOffset;
		AtlasMesh.Material = mat;
		AtlasMesh.Anchor = anchor;
		base.Scale = Vector2.one;
	}

	public void UpdateUVOffset(Rect uvOffset)
	{
		AtlasMesh.UVOffset = uvOffset;
	}

	public override void FreeObject()
	{
		MeshGUIManager.Instance.FreeAtlasMesh(_meshObject);
	}

	public override Vector2 GetOriginalBounds()
	{
		return new Vector2(Width, Height);
	}

	protected override Vector2 GetAdjustScale()
	{
		return MeshGUIManager.Instance.TransformSizeFromScreenToWorld(new Vector2(Width, Height));
	}

	protected override CustomMesh GetCustomMesh()
	{
		if (_meshObject != null)
		{
			return _meshObject.GetComponent<AtlasMesh>();
		}
		return null;
	}

	protected override GameObject AllocObject(GameObject parentObject)
	{
		return MeshGUIManager.Instance.CreateAtlasMesh(parentObject);
	}

	protected override void UpdateRect()
	{
		Vector2 vector = MeshGUIManager.Instance.TransformSizeFromWorldToScreen(AtlasMesh.OffsetToUpperLeft);
		_rect.x = base.Position.x - vector.x;
		_rect.y = base.Position.y - vector.y;
		_rect.width = base.Size.x;
		_rect.height = base.Size.y;
	}
}
