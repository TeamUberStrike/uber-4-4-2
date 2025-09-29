using UnityEngine;

public class MeshGUIQuad : MeshGUIBase
{
	public Texture Texture
	{
		get
		{
			return QuadMesh.Texture;
		}
		set
		{
			QuadMesh.Texture = value;
		}
	}

	public QuadMesh QuadMesh
	{
		get
		{
			return _customMesh as QuadMesh;
		}
	}

	public MeshGUIQuad(Texture tex, TextAnchor anchor = TextAnchor.UpperLeft, GameObject parentObject = null)
		: base(parentObject)
	{
		Texture = tex;
		QuadMesh quadMesh = QuadMesh;
		if (quadMesh != null)
		{
			quadMesh.Anchor = anchor;
		}
	}

	public override void FreeObject()
	{
		MeshGUIManager.Instance.FreeQuadMesh(_meshObject);
	}

	public override Vector2 GetOriginalBounds()
	{
		if (Texture != null)
		{
			return new Vector2(Texture.width, Texture.height);
		}
		return Vector2.zero;
	}

	protected override Vector2 GetAdjustScale()
	{
		if (Texture != null)
		{
			return MeshGUIManager.Instance.TransformSizeFromScreenToWorld(new Vector2(Texture.width, Texture.height));
		}
		return Vector2.zero;
	}

	protected override CustomMesh GetCustomMesh()
	{
		if (_meshObject != null)
		{
			return _meshObject.GetComponent<QuadMesh>();
		}
		return null;
	}

	protected override GameObject AllocObject(GameObject parentObject)
	{
		return MeshGUIManager.Instance.CreateQuadMesh(parentObject);
	}

	protected override void UpdateRect()
	{
		Vector2 vector = MeshGUIManager.Instance.TransformSizeFromWorldToScreen(QuadMesh.OffsetToUpperLeft);
		_rect.x = base.Position.x - vector.x * base.Scale.x;
		_rect.y = base.Position.y - vector.y * base.Scale.y;
		_rect.width = base.Size.x;
		_rect.height = base.Size.y;
	}
}
