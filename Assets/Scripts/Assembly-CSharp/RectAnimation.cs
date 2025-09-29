using UnityEngine;

public class RectAnimation
{
	public delegate void OnValueChange(Rect oldValue, Rect newValue);

	private Rect _rect;

	private OnValueChange _onVec2Change;

	private bool _isAnimating;

	private Rect _animSrc;

	private float _animTime;

	private float _animStartTime;

	private EaseType _animEaseType;

	public Rect FinalRect { get; private set; }

	public Rect Rect
	{
		get
		{
			return _rect;
		}
		set
		{
			Rect rect = _rect;
			_rect = value;
			if (_onVec2Change != null)
			{
				_onVec2Change(rect, _rect);
			}
		}
	}

	public bool IsAnimating
	{
		get
		{
			return _isAnimating;
		}
	}

	public RectAnimation(Rect initialRect, OnValueChange onVec2Change = null)
	{
		_animSrc = initialRect;
		_rect = initialRect;
		_isAnimating = false;
		if (onVec2Change != null)
		{
			_onVec2Change = onVec2Change;
		}
	}

	public void Update()
	{
		if (_isAnimating)
		{
			float num = Time.time - _animStartTime;
			if (num <= _animTime)
			{
				float t = Mathf.Clamp01(num * (1f / _animTime));
				Rect = new Rect(Mathf.Lerp(_animSrc.x, FinalRect.x, Mathfx.Ease(t, _animEaseType)), Mathf.Lerp(_animSrc.y, FinalRect.y, Mathfx.Ease(t, _animEaseType)), Mathf.Lerp(_animSrc.width, FinalRect.width, Mathfx.Ease(t, _animEaseType)), Mathf.Lerp(_animSrc.height, FinalRect.height, Mathfx.Ease(t, _animEaseType)));
			}
			else
			{
				Rect = FinalRect;
				_isAnimating = false;
			}
		}
	}

	public void AnimTo(Rect destPosition, float time = 0.5f, EaseType easeType = EaseType.In, float startDelay = 0f)
	{
		if (time <= 0f)
		{
			Rect = destPosition;
			return;
		}
		_isAnimating = true;
		_animSrc = Rect;
		FinalRect = destPosition;
		_animTime = time;
		_animEaseType = easeType;
		_animStartTime = Time.time + startDelay;
	}

	public void StopAnim()
	{
		_isAnimating = false;
	}
}
