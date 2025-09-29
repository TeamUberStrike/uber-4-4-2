using UnityEngine;

public class Reticle
{
	private const int STATE_NORMAL = 0;

	private const int STATE_BIG = 1;

	private const int STATE_SMALL = 2;

	private const float DURATION = 1f;

	private Texture _texRotate;

	private Texture _texScale1;

	private Texture _texScale2;

	private Texture _texTranslate;

	private float _innerScaleRatio;

	private float _outterScaleRatio;

	private float _translateDistance;

	private float _currentAngle;

	private float _currentDistance;

	private float _currentInnerRatio;

	private float _currentOutterRatio;

	private int _currentState;

	private float _timer;

	public Reticle()
	{
		_texRotate = null;
		_texScale1 = null;
		_texScale2 = null;
		_texTranslate = null;
		_innerScaleRatio = 0f;
		_outterScaleRatio = 0f;
		_translateDistance = 0f;
	}

	public void SetRotate(Texture image, float angle)
	{
		_texRotate = image;
	}

	public void SetInnerScale(Texture image, float ratio)
	{
		_texScale1 = image;
		_innerScaleRatio = ratio;
	}

	public void SetOutterScale(Texture image, float ratio)
	{
		_texScale2 = image;
		_outterScaleRatio = ratio;
	}

	public void SetTranslate(Texture image, float distance)
	{
		_texTranslate = image;
		_translateDistance = distance;
	}

	public void Update()
	{
		switch (_currentState)
		{
		case 0:
			_timer = Mathf.Lerp(_timer, 0f, Time.deltaTime);
			_currentDistance = Mathf.Lerp(_currentDistance, 0f, Time.deltaTime);
			_currentInnerRatio = Mathf.Lerp(_currentInnerRatio, 1f, Time.deltaTime);
			_currentOutterRatio = Mathf.Lerp(_currentOutterRatio, 1f, Time.deltaTime);
			break;
		case 1:
			if (_timer < 1f)
			{
				_timer += Time.deltaTime * 5f;
				_currentAngle = Mathf.Lerp(0f, 60f, _timer / 1f);
				_currentDistance = _translateDistance * _timer / 1f;
				_currentInnerRatio = Mathf.Lerp(1f, _innerScaleRatio, _timer / 1f);
				_currentOutterRatio = Mathf.Lerp(1f, _outterScaleRatio, _timer / 1f);
			}
			else
			{
				_currentState = 2;
			}
			break;
		case 2:
			if (_timer > 0f)
			{
				_timer -= Time.deltaTime * 5f;
				_currentDistance = _translateDistance * _timer / 1f;
				_currentInnerRatio = Mathf.Lerp(1f, _innerScaleRatio, _timer / 1f);
				_currentOutterRatio = Mathf.Lerp(1f, _outterScaleRatio, _timer / 1f);
			}
			else
			{
				_currentState = 0;
			}
			break;
		}
	}

	public void Trigger()
	{
		_currentState = 1;
	}

	public void Draw(Rect position)
	{
		Vector2 pivotPoint = new Vector2(position.x + position.width * 0.5f, position.y + position.height * 0.5f);
		if ((bool)_texRotate)
		{
			GUIUtility.RotateAroundPivot(_currentAngle, pivotPoint);
			GUI.DrawTexture(position, _texRotate);
			GUI.matrix = Matrix4x4.identity;
		}
		if ((bool)_texScale1)
		{
			GUIUtility.ScaleAroundPivot(new Vector2(_currentInnerRatio, _currentInnerRatio), pivotPoint);
			GUI.DrawTexture(position, _texScale1);
			GUI.matrix = Matrix4x4.identity;
		}
		if ((bool)_texScale2)
		{
			GUIUtility.ScaleAroundPivot(new Vector2(_currentOutterRatio, _currentOutterRatio), pivotPoint);
			GUI.DrawTexture(position, _texScale2);
			GUI.matrix = Matrix4x4.identity;
		}
		if ((bool)_texTranslate)
		{
			position.x += _currentDistance;
			GUI.DrawTexture(position, _texTranslate);
			GUIUtility.RotateAroundPivot(-90f, pivotPoint);
			GUI.DrawTexture(position, _texTranslate);
			GUIUtility.RotateAroundPivot(-90f, pivotPoint);
			GUI.DrawTexture(position, _texTranslate);
			GUIUtility.RotateAroundPivot(-90f, pivotPoint);
			GUI.DrawTexture(position, _texTranslate);
			GUIUtility.RotateAroundPivot(-90f, pivotPoint);
			GUI.matrix = Matrix4x4.identity;
		}
	}
}
