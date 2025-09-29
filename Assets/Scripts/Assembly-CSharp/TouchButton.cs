using System;
using UnityEngine;

public class TouchButton : TouchControl
{
	public const float LongPressTime = 0.4f;

	public GUIContent Content;

	public AtlasGUIQuad Quad;

	public GUIStyle Style;

	public float MinGUIAlpha;

	private bool _touchStarted;

	private bool _touchSent;

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
			if (Quad != null)
			{
				Quad.SetVisible(value);
			}
		}
	}

	public event Action OnPushed;

	public event Action OnLongPress;

	public TouchButton()
	{
		base.OnTouchBegan += OnTouchButtonBegan;
		base.OnTouchEnded += OnTouchButtonEnded;
	}

	public TouchButton(string title, GUIStyle style)
		: this()
	{
		Content = new GUIContent(title);
		Style = style;
	}

	public TouchButton(Rect rect)
		: this()
	{
		Quad = new AtlasGUIQuad(MobileIcons.TextureAtlas, rect);
		Quad.Hide();
		Quad.Name = "Button";
	}

	~TouchButton()
	{
		base.OnTouchBegan -= OnTouchButtonBegan;
		base.OnTouchEnded -= OnTouchButtonEnded;
	}

	public override void UpdateTouches(Touch touch)
	{
		base.UpdateTouches(touch);
		if (_touchStarted && !_touchSent && finger.StartTouchTime + 0.4f < Time.time)
		{
			if (this.OnLongPress != null)
			{
				this.OnLongPress();
			}
			else if (this.OnPushed != null)
			{
				this.OnPushed();
			}
			_touchSent = true;
		}
	}

	public override void Draw()
	{
		if (Content != null)
		{
			GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp(Singleton<TouchController>.Instance.GUIAlpha, MinGUIAlpha, 1f));
			if (_rotationAngle != 0f)
			{
				GUIUtility.RotateAroundPivot(_rotationAngle, _rotationPoint);
			}
			if (Style != null)
			{
				GUI.Label(Boundary, Content, Style);
			}
			else
			{
				GUI.Label(Boundary, Content);
			}
			if (_rotationAngle != 0f)
			{
				GUI.matrix = Matrix4x4.identity;
			}
			GUI.color = Color.white;
		}
	}

	private void OnTouchButtonEnded(Vector2 pos)
	{
		if (!_touchSent && this.OnPushed != null)
		{
			this.OnPushed();
		}
	}

	private void OnTouchButtonBegan(Vector2 pos)
	{
		_touchSent = false;
		_touchStarted = true;
	}

	protected override void ResetTouch()
	{
		base.ResetTouch();
		_touchStarted = false;
		_touchSent = false;
	}
}
