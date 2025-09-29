using System.Collections.Generic;
using UnityEngine;

public class TouchDPad : TouchBaseControl
{
	private bool enabled;

	public Vector2 TapDelay = new Vector2(0.2f, 0.2f);

	public Vector2 MoveInteriaRolloff = new Vector2(24f, 20f);

	private float _rotation;

	public TouchButton JumpButton;

	public TouchButton CrouchButton;

	private AtlasGUIQuad DpadQuad;

	private Vector2 _topLeftPosition;

	public float MinGUIAlpha = 0.3f;

	private Rect _leftRect;

	private Rect _rightRect;

	private Rect _forwardRect;

	private Rect _backwardRect;

	private Dictionary<int, TouchFinger> _fingers;

	private Vector2 _lastDirection;

	public Vector2 TopLeftPosition
	{
		set
		{
			DpadQuad.Position = new Vector2(value.x, value.y);
			_leftRect = new Rect(value.x, value.y, 104f, 209f);
			_forwardRect = new Rect(value.x + 104f, value.y, 104f, 104f);
			_backwardRect = new Rect(value.x + 104f, value.y + 104f, 104f, 106f);
			_rightRect = new Rect(value.x + 207f, value.y + 103f, 104f, 106f);
			_topLeftPosition = value;
			if (CrouchButton != null)
			{
				Rect boundary = new Rect(value.x + 311f, value.y + 103f, 88f, 106f);
				CrouchButton.Boundary = boundary;
			}
			if (JumpButton != null)
			{
				Rect boundary2 = new Rect(value.x + 207f, value.y, 192f, 104f);
				JumpButton.Boundary = boundary2;
			}
			Boundary = new Rect(value.x, value.y, value.x + 311f + 88f, value.y + 104f + 106f);
		}
	}

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
				DpadQuad.SetVisible(value);
				if (JumpButton != null)
				{
					JumpButton.Enabled = value;
				}
				if (CrouchButton != null)
				{
					CrouchButton.Enabled = value;
				}
				_lastDirection = Vector2.zero;
				Direction = Vector2.zero;
				if (!enabled)
				{
					_fingers.Clear();
					Moving = false;
				}
			}
		}
	}

	public float Rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
			DpadQuad.Rotation = value;
			DpadQuad.Name = "Dpad";
			JumpButton.SetRotation(value, _topLeftPosition);
			CrouchButton.SetRotation(value, _topLeftPosition);
		}
	}

	public Vector2 Direction { get; private set; }

	public bool Moving { get; private set; }

	public TouchDPad()
	{
		_fingers = new Dictionary<int, TouchFinger>();
		_lastDirection = Vector2.zero;
		Moving = false;
	}

	public TouchDPad(Rect rect)
		: this()
	{
		DpadQuad = new AtlasGUIQuad(MobileIcons.TextureAtlas, rect);
		DpadQuad.Hide();
		JumpButton = new TouchButton();
		CrouchButton = new TouchButton();
	}

	public bool InsideBoundary(Vector2 position)
	{
		return _forwardRect.ContainsTouch(position) || _leftRect.ContainsTouch(position) || _rightRect.ContainsTouch(position) || _backwardRect.ContainsTouch(position);
	}

	public override void UpdateTouches(Touch touch)
	{
		Vector2 vector = Mathfx.RotateVector2AboutPoint(touch.position, new Vector2(_topLeftPosition.x, (float)Screen.height - _topLeftPosition.y), 0f - _rotation);
		if (touch.phase == TouchPhase.Began && InsideBoundary(vector))
		{
			_fingers.Remove(touch.fingerId);
			_fingers.Add(touch.fingerId, new TouchFinger
			{
				StartPos = vector,
				StartTouchTime = Time.time,
				LastPos = vector,
				FingerId = touch.fingerId
			});
		}
		else if (touch.phase == TouchPhase.Moved)
		{
			if (_fingers.ContainsKey(touch.fingerId))
			{
				_fingers[touch.fingerId].LastPos = vector;
			}
		}
		else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
		{
			_fingers.Remove(touch.fingerId);
		}
	}

	public override void FinalUpdate()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		foreach (TouchFinger value in _fingers.Values)
		{
			if (_leftRect.ContainsTouch(value.LastPos))
			{
				flag = true;
			}
			else if (_rightRect.ContainsTouch(value.LastPos))
			{
				flag2 = true;
			}
			else if (_forwardRect.ContainsTouch(value.LastPos))
			{
				flag3 = true;
			}
			else if (_backwardRect.ContainsTouch(value.LastPos))
			{
				flag4 = true;
			}
		}
		Vector2 zero = Vector2.zero;
		if (flag)
		{
			zero += new Vector2(-1f, 0f);
		}
		if (flag2)
		{
			zero += new Vector2(1f, 0f);
		}
		if (flag3)
		{
			zero += new Vector2(0f, 1f);
		}
		if (flag4)
		{
			zero += new Vector2(0f, -1f);
		}
		if (flag || flag2 || flag3 || flag4)
		{
			Moving = true;
		}
		else
		{
			Moving = false;
		}
		if (zero.y == 0f)
		{
			zero.y = Mathf.Lerp(_lastDirection.y, zero.y, Time.deltaTime * MoveInteriaRolloff.y);
		}
		if (zero.x == 0f)
		{
			zero.x = Mathf.Lerp(_lastDirection.x, zero.x, Time.deltaTime * MoveInteriaRolloff.x);
		}
		_lastDirection = Direction;
		Direction = zero;
	}
}
