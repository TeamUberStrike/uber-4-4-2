using UnityEngine;

public abstract class MeshGUIBase : IAnimatable2D
{
	protected GameObject _meshObject;

	protected CustomMesh _customMesh;

	protected ColorAnim _colorAnim;

	protected Vector2Anim _positionAnim;

	protected FlickerAnim _flickerAnim;

	protected Vector2Anim _scaleAnim;

	protected Rect _rect;

	private float _depth;

	private Vector2 _size;

	private Vector2 _center;

	private Vector2 _parentPosition;

	private Vector2 _scaleAnimPivot;

	private bool _isEnabled = true;

	private bool _isScaleAnimAroundPivot;

	public string Name
	{
		get
		{
			return _customMesh.name;
		}
		set
		{
			_customMesh.name = value;
		}
	}

	public Color Color
	{
		get
		{
			return _colorAnim.Color;
		}
		set
		{
			_colorAnim.Color = value;
		}
	}

	public float Alpha
	{
		get
		{
			return _colorAnim.Alpha;
		}
		set
		{
			_colorAnim.Alpha = value;
		}
	}

	public Vector2 ParentPosition
	{
		get
		{
			return _parentPosition;
		}
		set
		{
			_parentPosition = value;
			UpdatePosition();
		}
	}

	public Vector2 Position
	{
		get
		{
			return _positionAnim.Vec2;
		}
		set
		{
			_positionAnim.Vec2 = value;
			UpdatePosition();
		}
	}

	public float Depth
	{
		get
		{
			return _depth;
		}
		set
		{
			_depth = value;
			UpdatePosition();
		}
	}

	public Vector2 Scale
	{
		get
		{
			return _scaleAnim.Vec2;
		}
		set
		{
			_scaleAnim.Vec2 = value;
		}
	}

	public Vector2 Size
	{
		get
		{
			Vector2 originalBounds = GetOriginalBounds();
			_size.x = Scale.x * originalBounds.x;
			_size.y = Scale.y * originalBounds.y;
			return _size;
		}
	}

	public Vector2 Center
	{
		get
		{
			UpdateRect();
			_center.x = _rect.x + Size.x / 2f;
			_center.y = _rect.y + Size.y / 2f;
			return _center;
		}
	}

	public Rect Rect
	{
		get
		{
			UpdateRect();
			return _rect;
		}
	}

	public float Rotation
	{
		get
		{
			return _meshObject.transform.localRotation.eulerAngles.z;
		}
		set
		{
			_meshObject.transform.localRotation = Quaternion.Euler(0f, 0f, value);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			_isEnabled = value;
			if (_isEnabled)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	public bool IsVisible { get; private set; }

	public MeshGUIBase(GameObject parentObject)
	{
		if (MeshGUIManager.Exists)
		{
			_meshObject = AllocObject(parentObject);
			_customMesh = GetCustomMesh();
			_positionAnim = new Vector2Anim(OnPositionChange);
			_scaleAnim = new Vector2Anim(OnScaleChange);
			_scaleAnim.Vec2 = Vector2.one;
			_colorAnim = new ColorAnim(OnColorChange);
			_flickerAnim = new FlickerAnim(UpdateVisible);
			ResetGUI();
		}
	}

	public abstract void FreeObject();

	public abstract Vector2 GetOriginalBounds();

	protected abstract GameObject AllocObject(GameObject parentObject);

	protected abstract CustomMesh GetCustomMesh();

	protected abstract Vector2 GetAdjustScale();

	protected abstract void UpdateRect();

	public virtual void Draw(float offsetX = 0f, float offsetY = 0f)
	{
		if (_parentPosition.x != offsetX || _parentPosition.y != offsetY)
		{
			ParentPosition = new Vector2(offsetX, offsetY);
		}
		_flickerAnim.Update();
		_colorAnim.Update();
		_positionAnim.Update();
		_scaleAnim.Update();
	}

	public Vector2 GetPosition()
	{
		return Position;
	}

	public Vector2 GetCenter()
	{
		return Center;
	}

	public Rect GetRect()
	{
		return Rect;
	}

	public void SetVisible(bool show)
	{
		if (show)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Show()
	{
		if (IsEnabled)
		{
			IsVisible = true;
			if ((bool)_customMesh)
			{
				_customMesh.IsVisible = true;
			}
		}
	}

	public void Hide()
	{
		IsVisible = false;
		if ((bool)_customMesh)
		{
			_customMesh.IsVisible = false;
		}
	}

	public void StopFading()
	{
		_colorAnim.StopFading();
	}

	public void StopMoving()
	{
		_positionAnim.StopAnim();
	}

	public void StopScaling()
	{
		_scaleAnim.StopAnim();
	}

	public void StopFlickering()
	{
		_flickerAnim.StopAnim();
	}

	public void FadeColorTo(Color destColor, float time = 0f, EaseType easeType = EaseType.None)
	{
		_colorAnim.FadeColorTo(destColor, time, easeType);
	}

	public void FadeColor(Color deltaColor, float time = 0f, EaseType easeType = EaseType.None)
	{
		_colorAnim.FadeColor(deltaColor, time, easeType);
	}

	public void FadeAlphaTo(float destAlpha, float time = 0f, EaseType easeType = EaseType.None)
	{
		_colorAnim.FadeAlphaTo(destAlpha, time, easeType);
	}

	public void FadeAlpha(float deltaAlpha, float time = 0f, EaseType easeType = EaseType.None)
	{
		_colorAnim.FadeAlpha(deltaAlpha, time, easeType);
	}

	public void MoveTo(Vector2 destPosition, float time = 0f, EaseType easeType = EaseType.None, float startDelay = 0f)
	{
		_positionAnim.AnimTo(destPosition, time, easeType, startDelay);
	}

	public void Move(Vector2 deltaPosition, float time = 0f, EaseType easeType = EaseType.None)
	{
		_positionAnim.AnimBy(deltaPosition, time, easeType);
	}

	public void ScaleTo(Vector2 destScale, float time = 0f, EaseType easeType = EaseType.None)
	{
		_scaleAnim.AnimTo(destScale, time, easeType, 0f);
	}

	public void ScaleDelta(Vector2 scaleFactor, float time = 0f, EaseType easeType = EaseType.None)
	{
		_scaleAnim.AnimBy(scaleFactor, time, easeType);
	}

	public void ScaleToAroundPivot(Vector2 destScale, Vector2 pivot, float time = 0f, EaseType easeType = EaseType.None)
	{
		ScaleTo(destScale, time, easeType);
		_isScaleAnimAroundPivot = true;
		_scaleAnimPivot = pivot;
	}

	public void ScaleAroundPivot(Vector2 scaleFactor, Vector2 pivot, float time = 0f, EaseType easeType = EaseType.None)
	{
		Vector2 destScale = default(Vector2);
		destScale.x = Scale.x * scaleFactor.x;
		destScale.y = Scale.y * scaleFactor.y;
		ScaleToAroundPivot(destScale, pivot, time, easeType);
	}

	public void Flicker(float time, float flickerInterval = 0.02f)
	{
		_flickerAnim.Flicker(time, flickerInterval);
	}

	public bool IsFlickering()
	{
		return _flickerAnim.IsAnimating;
	}

	protected void ResetGUI()
	{
		_parentPosition = Vector2.zero;
		_depth = 0f;
		Color = Color.white;
		Position = Vector2.zero;
		Scale = Vector2.one;
		Show();
		UpdateRect();
	}

	private void OnColorChange(Color oldColor, Color newColor)
	{
		_customMesh.Color = _colorAnim.Color;
		_customMesh.Alpha = _colorAnim.Alpha;
	}

	private void OnScaleChange(Vector2 oldScale, Vector2 newScale)
	{
		if (_scaleAnim.IsAnimating && _isScaleAnimAroundPivot)
		{
			Vector2 position = default(Vector2);
			if (oldScale.x > 0f)
			{
				position.x = (Position.x - _scaleAnimPivot.x) * newScale.x / oldScale.x + _scaleAnimPivot.x;
			}
			else
			{
				position.x = Position.x - Size.x / 2f;
			}
			if (oldScale.y > 0f)
			{
				position.y = (Position.y - _scaleAnimPivot.y) * newScale.y / oldScale.y + _scaleAnimPivot.y;
			}
			else
			{
				position.y = Position.y - Size.y / 2f;
			}
			Position = position;
		}
		Vector2 adjustScale = GetAdjustScale();
		_meshObject.transform.localScale = new Vector3(newScale.x * adjustScale.x, newScale.y * adjustScale.y, 1f);
		UpdateRect();
	}

	private void OnPositionChange(Vector2 oldPosition, Vector2 newPosition)
	{
		UpdatePosition();
	}

	private void UpdateVisible(FlickerAnim animation)
	{
		if (!(_customMesh == null))
		{
			if (animation.IsAnimating)
			{
				_customMesh.IsVisible = IsVisible && animation.IsFlickerVisible;
			}
			else
			{
				_customMesh.IsVisible = IsVisible;
			}
		}
	}

	private void UpdatePosition()
	{
		Vector3 position = MeshGUIManager.Instance.TransformPosFromScreenToWorld(_parentPosition + _positionAnim.Vec2);
		position.z = _depth;
		_meshObject.transform.position = position;
		UpdateRect();
	}
}
