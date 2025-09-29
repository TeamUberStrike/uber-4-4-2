using UnityEngine;

public class MeshGUICircle : MeshGUIBase
{
	private FloatAnim _angleAnim;

	public Texture Texture
	{
		get
		{
			return CircleMesh.Texture;
		}
		set
		{
			CircleMesh.Texture = value;
		}
	}

	public CircleMesh CircleMesh
	{
		get
		{
			return _customMesh as CircleMesh;
		}
	}

	public float Angle
	{
		get
		{
			return CircleMesh.FillAngle;
		}
		set
		{
			CircleMesh.FillAngle = value;
		}
	}

	public MeshGUICircle(Texture tex, GameObject parentObject = null)
		: base(parentObject)
	{
		Texture = tex;
		_angleAnim = new FloatAnim(OnAngleChange);
	}

	public override void FreeObject()
	{
		MeshGUIManager.Instance.FreeCircleMesh(_meshObject);
	}

	public override void Draw(float offsetX = 0f, float offsetY = 0f)
	{
		base.Draw(offsetX, offsetY);
		_angleAnim.Update();
	}

	public override Vector2 GetOriginalBounds()
	{
		if (Texture != null)
		{
			return new Vector2(Texture.width, Texture.height);
		}
		return Vector2.zero;
	}

	public void AnimAngleTo(float destAngle, float time = 0f, EaseType easeType = EaseType.None)
	{
		_angleAnim.AnimTo(destAngle, time, easeType);
	}

	public void AnimAngleDelta(float deltaAngle, float time = 0f, EaseType easeType = EaseType.None)
	{
		_angleAnim.AnimBy(deltaAngle, time, easeType);
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
			return _meshObject.GetComponent<CircleMesh>();
		}
		return null;
	}

	protected override GameObject AllocObject(GameObject parentObject)
	{
		return MeshGUIManager.Instance.CreateCircleMesh(parentObject);
	}

	protected override void UpdateRect()
	{
		_rect.x = base.Position.x - base.Size.x / 2f;
		_rect.y = base.Position.y - base.Size.x / 2f;
		_rect.width = base.Size.x;
		_rect.height = base.Size.y;
	}

	private void OnAngleChange(float oldAngle, float newAngle)
	{
		CircleMesh.FillAngle = newAngle;
	}
}
