using System.Collections.Generic;
using UnityEngine;

public class Animatable2DGroup : IAnimatable2D
{
	private List<IAnimatable2D> _group;

	private Vector2 _center;

	private Rect _rect;

	private Vector2 _position;

	private bool _isEnabled = true;

	private bool _isAnimatingPosition;

	private Vector2 _positionAnimSrc;

	private Vector2 _positionAnimDest;

	private float _positionAnimTime;

	private float _positionAnimStartTime;

	private EaseType _positionAnimEaseType;

	public List<IAnimatable2D> Group
	{
		get
		{
			return _group;
		}
		set
		{
			_group = value;
		}
	}

	public Vector2 Center
	{
		get
		{
			UpdateCenter();
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

	public Vector2 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
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

	public Animatable2DGroup()
	{
		_group = new List<IAnimatable2D>();
	}

	public void Draw(float offsetX = 0f, float offsetY = 0f)
	{
		AnimPosition();
		foreach (IAnimatable2D item in _group)
		{
			item.Draw(_position.x + offsetX, _position.y + offsetY);
		}
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
		if (!IsEnabled)
		{
			return;
		}
		foreach (IAnimatable2D item in _group)
		{
			item.Show();
		}
		IsVisible = true;
	}

	public void Hide()
	{
		foreach (IAnimatable2D item in _group)
		{
			item.Hide();
		}
		IsVisible = false;
	}

	public void FadeColorTo(Color destColor, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.FadeColorTo(destColor, time, easeType);
		}
	}

	public void FadeColor(Color deltaColor, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.FadeColorTo(deltaColor, time, easeType);
		}
	}

	public void FadeAlphaTo(float destAlpha, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.FadeAlphaTo(destAlpha, time, easeType);
		}
	}

	public void FadeAlpha(float deltaAlpha, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.FadeAlpha(deltaAlpha, time, easeType);
		}
	}

	public void MoveTo(Vector2 destPosition, float time = 0f, EaseType easeType = EaseType.None, float startDelay = 0f)
	{
		if (time <= 0f)
		{
			_position = destPosition;
			UpdateRect();
			return;
		}
		_isAnimatingPosition = true;
		_positionAnimSrc = _position;
		_positionAnimDest = destPosition;
		_positionAnimTime = time;
		_positionAnimEaseType = easeType;
		_positionAnimStartTime = Time.time + startDelay;
	}

	public void Move(Vector2 deltaPosition, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.Move(deltaPosition, time, easeType);
		}
	}

	public void ScaleTo(Vector2 destScale, float time = 0f, EaseType easeType = EaseType.None)
	{
		ScaleDelta(destScale, time, easeType);
	}

	public void ScaleDelta(Vector2 scaleFactor, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.ScaleDelta(scaleFactor, time, easeType);
		}
	}

	public void ScaleToAroundPivot(Vector2 destScale, Vector2 pivot, float time = 0f, EaseType easeType = EaseType.None)
	{
		ScaleAroundPivot(destScale, pivot, time, easeType);
	}

	public void ScaleAroundPivot(Vector2 scaleFactor, Vector2 pivot, float time = 0f, EaseType easeType = EaseType.None)
	{
		foreach (IAnimatable2D item in _group)
		{
			if (item is Animatable2DGroup)
			{
				item.ScaleAroundPivot(scaleFactor, pivot - item.GetPosition(), time, easeType);
			}
			else
			{
				item.ScaleAroundPivot(scaleFactor, pivot, time, easeType);
			}
		}
	}

	public void Flicker(float time, float flickerInterval = 0.02f)
	{
		foreach (IAnimatable2D item in _group)
		{
			item.Flicker(time, flickerInterval);
		}
	}

	public void StopFading()
	{
		foreach (IAnimatable2D item in Group)
		{
			item.StopFading();
		}
	}

	public void StopMoving()
	{
		_isAnimatingPosition = false;
		foreach (IAnimatable2D item in Group)
		{
			item.StopMoving();
		}
	}

	public void StopScaling()
	{
		foreach (IAnimatable2D item in Group)
		{
			item.StopScaling();
		}
	}

	public void StopFlickering()
	{
		foreach (IAnimatable2D item in Group)
		{
			item.StopFlickering();
		}
	}

	public void RemoveAndFree(int index)
	{
		if (index >= 0 && index < _group.Count)
		{
			IAnimatable2D animatable2D = _group[index];
			animatable2D.FreeObject();
			_group.RemoveAt(index);
		}
	}

	public void RemoveAndFree(IAnimatable2D animatable)
	{
		if (_group.Contains(animatable))
		{
			animatable.FreeObject();
			_group.Remove(animatable);
		}
	}

	public void ClearAndFree()
	{
		FreeObject();
		_group.Clear();
	}

	public void UpdateMeshGUIPosition(float offsetX = 0f, float offsetY = 0f)
	{
		foreach (IAnimatable2D item in _group)
		{
			if (item is MeshGUIBase)
			{
				(item as MeshGUIBase).ParentPosition = _position + new Vector2(offsetX, offsetY);
			}
			else if (item is Animatable2DGroup)
			{
				(item as Animatable2DGroup).UpdateMeshGUIPosition(_position.x + offsetX, _position.y + offsetY);
			}
		}
	}

	public void FreeObject()
	{
		foreach (IAnimatable2D item in _group)
		{
			item.FreeObject();
		}
	}

	private void AnimPosition()
	{
		if (_isAnimatingPosition)
		{
			float num = Time.time - _positionAnimStartTime;
			if (num <= _positionAnimTime)
			{
				float t = Mathf.Clamp01(num * (1f / _positionAnimTime));
				_position = Vector2.Lerp(_positionAnimSrc, _positionAnimDest, Mathfx.Ease(t, _positionAnimEaseType));
				UpdateRect();
			}
			else
			{
				_isAnimatingPosition = false;
				_position = _positionAnimDest;
			}
		}
	}

	private void UpdateCenter()
	{
		if (_group.Count <= 0)
		{
			return;
		}
		_center = Vector2.zero;
		foreach (IAnimatable2D item in _group)
		{
			_center += item.GetCenter();
		}
		_center /= (float)_group.Count;
	}

	private void UpdateRect()
	{
		if (_group.Count > 0)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			foreach (IAnimatable2D item in _group)
			{
				Rect rect = item.GetRect();
				Vector2 vector = new Vector2(rect.x + rect.width, rect.y + rect.height);
				zero.x = ((!(zero.x < rect.x)) ? rect.x : zero.x);
				zero.y = ((!(zero.y < rect.y)) ? rect.y : zero.y);
				zero2.x = ((!(zero2.x > vector.x)) ? vector.x : zero2.x);
				zero2.y = ((!(zero2.y > vector.y)) ? vector.y : zero2.y);
			}
			_rect.x = zero.x;
			_rect.y = zero.y;
			_rect.width = zero2.x - zero.x;
			_rect.height = zero2.y - zero.y;
			_rect.x += Position.x;
			_rect.y += Position.y;
		}
		else
		{
			_rect.x = _position.x;
			_rect.y = _position.y;
			Animatable2DGroup animatable2DGroup = this;
			float num = 0f;
			_rect.height = num;
			animatable2DGroup._rect.width = num;
		}
	}
}
