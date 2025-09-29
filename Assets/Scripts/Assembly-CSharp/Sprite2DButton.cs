using System;
using UnityEngine;

public class Sprite2DButton : IAnimatable2D
{
	private ColorAnim _colorAnim;

	private Vector2Anim _positionAnim;

	private FlickerAnim _flickerAnim;

	private Vector2Anim _scaleAnim;

	private GUIContent _content;

	private GUIStyle _style;

	private Vector2 _size;

	private Vector2 _center;

	private Vector2 _guiBounds;

	private Rect _rect;

	private bool _visible;

	private bool _isScaleAnimAroundPivot;

	private Vector2 _scaleAnimPivot;

	public GUIStyle Style
	{
		get
		{
			return _style;
		}
		set
		{
			_style = value;
		}
	}

	public GUIContent Content
	{
		get
		{
			return _content;
		}
		set
		{
			_content = value;
		}
	}

	public bool IsUsingGuiContentBounds { get; set; }

	public Action OnClick { get; set; }

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

	public Vector2 Position
	{
		get
		{
			return _positionAnim.Vec2;
		}
		set
		{
			_positionAnim.Vec2 = value;
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

	public Vector2 GUIBounds
	{
		get
		{
			if (IsUsingGuiContentBounds)
			{
				return _style.CalcSize(_content);
			}
			return _guiBounds;
		}
		set
		{
			_guiBounds = value;
		}
	}

	public Vector2 Size
	{
		get
		{
			_size.x = Scale.x * GUIBounds.x;
			_size.y = Scale.y * GUIBounds.y;
			return _size;
		}
	}

	public Vector2 Center
	{
		get
		{
			_center = Position + Size / 2f;
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

	public bool Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			_visible = value;
		}
	}

	public Sprite2DButton(GUIContent content, GUIStyle style)
	{
		IsUsingGuiContentBounds = true;
		_content = content;
		_style = style;
		_positionAnim = new Vector2Anim();
		_scaleAnim = new Vector2Anim(OnScaleChange);
		_colorAnim = new ColorAnim();
		_colorAnim.Color = Color.white;
		_flickerAnim = new FlickerAnim();
		_size = GUIBounds;
		_rect = default(Rect);
		_visible = true;
	}

	public void Draw(float offsetX = 0f, float offsetY = 0f)
	{
		bool num;
		if (_flickerAnim.IsAnimating)
		{
			if (!_visible)
			{
				goto IL_00a2;
			}
			num = _flickerAnim.IsFlickerVisible;
		}
		else
		{
			num = _visible;
		}
		if (num)
		{
			GUITools.BeginGUIColor(_colorAnim.Color);
			Rect rect = Rect;
			rect.x += offsetX;
			rect.y += offsetY;
			if (GUI.Button(rect, _content, _style) && OnClick != null)
			{
				OnClick();
			}
			GUITools.EndGUIColor();
		}
		goto IL_00a2;
		IL_00a2:
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

	public void Show()
	{
		Visible = true;
	}

	public void Hide()
	{
		Visible = false;
	}

	public void FreeObject()
	{
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

	private void UpdateRect()
	{
		_rect.x = Position.x - (float)Screen.width * (1f - AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth) / 2f;
		_rect.y = Position.y;
		_rect.width = Size.x;
		_rect.height = Size.y;
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
	}
}
