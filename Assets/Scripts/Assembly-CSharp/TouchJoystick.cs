using System;
using UnityEngine;

public class TouchJoystick : TouchBaseControl
{
	public AtlasGUIQuad JoystickQuad;

	public AtlasGUIQuad BackgroundQuad;

	public Vector2 MoveInteriaRolloff = new Vector2(6f, 5f);

	public float MinGUIAlpha = 0.4f;

	private bool enabled;

	private TouchFinger _finger;

	private Rect _joystickBoundary;

	private Vector2 _joystickPos;

	public override bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (value == enabled)
			{
				return;
			}
			enabled = value;
			if (!enabled)
			{
				JoystickQuad.Hide();
				BackgroundQuad.Hide();
				if (_finger.FingerId != -1 && this.OnJoystickStopped != null)
				{
					this.OnJoystickStopped();
				}
				_finger.Reset();
			}
		}
	}

	public event Action<Vector2> OnJoystickMoved;

	public event Action OnJoystickStopped;

	public TouchJoystick()
	{
		_finger = new TouchFinger();
		JoystickQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchMoveInner, TextAnchor.MiddleCenter);
		JoystickQuad.Hide();
		JoystickQuad.Name = "Joystick";
		BackgroundQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, MobileIcons.TouchMoveOuter, TextAnchor.MiddleCenter);
		BackgroundQuad.Hide();
		BackgroundQuad.Name = "JoystickBackground";
	}

	public override void UpdateTouches(Touch touch)
	{
		if (!enabled)
		{
			return;
		}
		if (touch.phase == TouchPhase.Began && _finger.FingerId == -1 && Boundary.ContainsTouch(touch.position))
		{
			_finger = new TouchFinger
			{
				StartPos = new Vector2(touch.position.x, (float)Screen.height - touch.position.y),
				StartTouchTime = Time.time,
				FingerId = touch.fingerId
			};
			_joystickBoundary = new Rect(touch.position.x - JoystickQuad.Width / 2f, (float)Screen.height - touch.position.y - JoystickQuad.Height / 2f, JoystickQuad.Width, JoystickQuad.Height);
			BackgroundQuad.Position = GetPositionAtTopLeft(touch.position.x, (float)Screen.height - touch.position.y, BackgroundQuad.Width, BackgroundQuad.Height);
			BackgroundQuad.Show();
			JoystickQuad.Position = GetPositionAtTopLeft(touch.position.x, (float)Screen.height - touch.position.y, JoystickQuad.Width, JoystickQuad.Height);
			JoystickQuad.Show();
		}
		else
		{
			if (_finger.FingerId != touch.fingerId)
			{
				return;
			}
			if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
			{
				_joystickPos.x = Mathf.Clamp(touch.position.x, _joystickBoundary.x, _joystickBoundary.x + _joystickBoundary.width);
				_joystickPos.y = Mathf.Clamp((float)Screen.height - touch.position.y, _joystickBoundary.y, _joystickBoundary.y + _joystickBoundary.height);
				JoystickQuad.Position = GetPositionAtTopLeft(_joystickPos.x, _joystickPos.y, JoystickQuad.Width, JoystickQuad.Height);
				Vector2 zero = Vector2.zero;
				zero.x = (_joystickPos.x - _finger.StartPos.x) * 2f / _joystickBoundary.width;
				zero.y = (_finger.StartPos.y - _joystickPos.y) * 2f / _joystickBoundary.height;
				zero *= ApplicationDataManager.ApplicationOptions.TouchMoveSensitivity;
				if (touch.phase == TouchPhase.Moved && this.OnJoystickMoved != null)
				{
					this.OnJoystickMoved(zero);
				}
			}
			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				if (this.OnJoystickStopped != null)
				{
					JoystickQuad.Hide();
					BackgroundQuad.Hide();
					this.OnJoystickStopped();
				}
				_finger.Reset();
			}
		}
	}

	private Vector2 GetPositionAtTopLeft(float x, float y, float width, float height)
	{
		return new Vector2(x - width / 2f, y - height / 2f);
	}
}
