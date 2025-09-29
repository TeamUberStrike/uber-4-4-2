using System;
using UnityEngine;

public class TouchSwipeBar : TouchControl
{
	private bool enabled;

	public int SwipeThreshold = 60;

	private AtlasGUIQuad _quad;

	private Vector2 _touchStartPos;

	public override bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (value != enabled)
			{
				enabled = value;
				_quad.SetVisible(value);
				if (!enabled)
				{
					finger.Reset();
				}
			}
		}
	}

	public bool Active
	{
		get
		{
			return Enabled && finger.FingerId != -1;
		}
	}

	public override Rect Boundary
	{
		get
		{
			return base.Boundary;
		}
		set
		{
			base.Boundary = value;
			_quad.Position = new Vector2(value.x, value.y);
		}
	}

	public event Action OnSwipeUp;

	public event Action OnSwipeDown;

	public TouchSwipeBar()
	{
		base.OnTouchBegan += OnSwipeBarTouchBegan;
		base.OnTouchMoved += OnSwipeBarTouchMoved;
		_quad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchZoomScrollbar);
		_quad.SetVisible(false);
	}

	private void OnSwipeBarTouchBegan(Vector2 obj)
	{
		_touchStartPos = finger.StartPos;
	}

	private void OnSwipeBarTouchMoved(Vector2 pos, Vector2 delta)
	{
		if (_touchStartPos.y - pos.y > (float)SwipeThreshold)
		{
			_touchStartPos = pos;
			if (this.OnSwipeDown != null)
			{
				this.OnSwipeDown();
			}
		}
		else if (_touchStartPos.y - pos.y < (float)(-SwipeThreshold))
		{
			_touchStartPos = pos;
			if (this.OnSwipeUp != null)
			{
				this.OnSwipeUp();
			}
		}
	}
}
