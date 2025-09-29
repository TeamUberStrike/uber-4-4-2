using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class AvatarHudInformation : MonoBehaviour
{
	public enum Mode
	{
		None = 0,
		Robot = 1
	}

	private const int ImageSize = 64;

	private const int PixelOffset = 1;

	[SerializeField]
	private Mode _mode;

	[SerializeField]
	private bool _isTeamDebug = true;

	[SerializeField]
	private string _text = "text";

	[SerializeField]
	public Vector2 _barOffset = new Vector2(0f, 0f);

	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private Vector3 _offset = new Vector3(0f, 2f, 0f);

	[SerializeField]
	private Texture _healthBarImage;

	private float _distanceCapMax = 50f;

	private float _distanceCapMin = 3f;

	[SerializeField]
	private float _timeCap;

	private float _maxTime;

	private InGameEventFeedbackType _currentFeedback;

	[SerializeField]
	private Transform _target;

	private Transform _transform;

	private Vector3 _screenPosition;

	[SerializeField]
	private float _barValue;

	private Vector2 _textSize;

	private bool _isVisible = true;

	private bool _showBar = true;

	public float _feedbackTimer;

	private UberStrike.Realtime.UnitySdk.CharacterInfo _info;

	private GUIStyle _nameTextStyle;

	private bool _isInViewport;

	private bool _needFadeIn;

	private float _fadeInAlpha;

	[SerializeField]
	private bool _forceShowInformation;

	public float Visibility
	{
		get
		{
			return _color.a;
		}
		set
		{
			_color.a = value;
		}
	}

	public UberStrike.Realtime.UnitySdk.CharacterInfo Info
	{
		get
		{
			return _info;
		}
		set
		{
			_info = value;
		}
	}

	public bool IsEnemy
	{
		get
		{
			return _info != null && GameState.HasCurrentPlayer && (_info.TeamID == TeamID.NONE || _info.TeamID != GameState.LocalCharacter.TeamID);
		}
	}

	public bool IsFriend
	{
		get
		{
			return !IsEnemy && !IsMe;
		}
	}

	public bool IsMe
	{
		get
		{
			return !GameState.HasCurrentGame || !GameState.HasCurrentPlayer || _info == null || GameState.CurrentPlayerID == _info.ActorId;
		}
	}

	public bool IsBarVisible
	{
		get
		{
			return _showBar;
		}
		set
		{
			_showBar = value;
		}
	}

	public float DistanceCap
	{
		get
		{
			return _distanceCapMax;
		}
		set
		{
			_distanceCapMax = value;
		}
	}

	public InGameEventFeedbackType ActiveFeedbackType
	{
		get
		{
			return _currentFeedback;
		}
	}

	public bool ForceShowInformation
	{
		get
		{
			return _forceShowInformation;
		}
		set
		{
			_forceShowInformation = value;
		}
	}

	private void Start()
	{
		base.useGUILayout = false;
		_transform = base.transform;
		if (_mode == Mode.Robot)
		{
			SetAvatarLabel(_text);
		}
		_nameTextStyle = BlueStonez.label_interparkbold_11pt;
	}

	private void OnGUI()
	{
		if (BlueStonez.Skin == null)
		{
			return;
		}
		GUI.depth = 100;
		Rect position = new Rect(_screenPosition.x - 50f, (float)Screen.height - _screenPosition.y + _barOffset.y, 100f, 6f);
		Rect position2 = new Rect(position.xMin, (float)Screen.height - _screenPosition.y - _textSize.y - 6f, _textSize.x, _textSize.y);
		Rect position3 = new Rect(_screenPosition.x - _textSize.x * 0.5f, (float)Screen.height - _screenPosition.y - _textSize.y - 6f, _textSize.x, _textSize.y);
		if (!_isInViewport)
		{
			return;
		}
		if (Application.isEditor && _isTeamDebug && _info != null)
		{
			GUI.Label(new Rect(_screenPosition.x, (float)Screen.height - _screenPosition.y, 50f, 20f), _info.TeamID.ToString());
		}
		if (IsEnemy && GameState.HasCurrentGame)
		{
			if (_isVisible || _forceShowInformation)
			{
				DrawName(position3);
			}
		}
		else if (((IsFriend || (_mode == Mode.Robot && _barValue > 0f)) && GameState.HasCurrentGame) || _forceShowInformation)
		{
			DrawBar(position, 100);
			DrawName(position2);
		}
		else if (!GameState.HasCurrentGame && _mode != Mode.Robot)
		{
			DrawName(position3);
		}
	}

	private void LateUpdate()
	{
		if ((bool)Camera.main && (bool)_target)
		{
			Vector3 rhs = _target.position + _offset - Camera.main.transform.position;
			Camera.main.ResetWorldToCameraMatrix();
			_screenPosition = Camera.main.WorldToScreenPoint(_target.position + _offset);
			bool isInViewport = _isInViewport;
			_isInViewport = Vector3.Dot(Camera.main.transform.forward, rhs) > 0f && _screenPosition.x >= 0f && _screenPosition.x <= (float)Screen.width && _screenPosition.y >= 0f && _screenPosition.y <= (float)Screen.height;
			if (_isInViewport && !isInViewport)
			{
				_needFadeIn = true;
			}
			if (!_isInViewport && _timeCap > 0f)
			{
				_timeCap = 0f;
				Visibility = 0f;
				_isVisible = false;
				_currentFeedback = InGameEventFeedbackType.None;
			}
			if (GameState.HasCurrentGame || _forceShowInformation)
			{
				if (_mode == Mode.Robot)
				{
					if (_isInViewport)
					{
						Visibility = 1f - (_screenPosition.z - _distanceCapMin) / (_distanceCapMax - _distanceCapMin) * Camera.main.fieldOfView / 60f;
					}
					else
					{
						Visibility = 0f;
					}
				}
				else if (IsEnemy && _isVisible)
				{
					if (_timeCap > 0f)
					{
						_timeCap -= Time.deltaTime;
						Visibility = Mathf.Clamp01(_timeCap / _maxTime);
					}
					else
					{
						Visibility = 0f;
						_isVisible = false;
						_currentFeedback = InGameEventFeedbackType.None;
					}
				}
				else if (IsFriend)
				{
					if (_isInViewport)
					{
						if (_timeCap > 0f)
						{
							_timeCap -= Time.deltaTime;
							float num = Mathf.Clamp01(_timeCap / _maxTime);
							float num2 = 1f - (_screenPosition.z - _distanceCapMin) / (_distanceCapMax - _distanceCapMin) * Camera.main.fieldOfView / 60f;
							Visibility = ((!(num > num2)) ? num2 : num);
						}
						else
						{
							Visibility = 1f - (_screenPosition.z - _distanceCapMin) / (_distanceCapMax - _distanceCapMin) * Camera.main.fieldOfView / 60f;
						}
					}
					else
					{
						Visibility = 0f;
					}
				}
				if (_barValue == 0f)
				{
					Visibility = 0f;
				}
			}
			else
			{
				Visibility = 1f;
			}
			if (_isInViewport)
			{
				FadeInAlphaCorrection();
			}
			_transform.position = _target.position;
		}
		if (_feedbackTimer >= 0f)
		{
			_feedbackTimer -= Time.deltaTime;
		}
	}

	public void Show(int seconds)
	{
		_needFadeIn = true;
		_fadeInAlpha = 1f;
		_isVisible = true;
		_timeCap = (float)seconds + 0.3f;
		_maxTime = seconds;
	}

	public void Hide()
	{
		_isVisible = false;
		_timeCap = 0f;
		_feedbackTimer = 0f;
		_currentFeedback = InGameEventFeedbackType.None;
	}

	private void DrawName(Rect position)
	{
		if (_forceShowInformation)
		{
			Visibility = 1f;
		}
		if (Visibility > 0f)
		{
			GUI.color = new Color(0f, 0f, 0f, Visibility);
			GUI.Label(new Rect(position.x + 1f, position.y + 1f, position.width, position.height), _text, _nameTextStyle);
			GUI.color = _color;
			GUI.Label(position, _text, _nameTextStyle);
			GUI.color = Color.white;
		}
	}

	private void DrawBar(Rect position, int barWidth)
	{
		if (Visibility > 0f)
		{
			GUI.color = new Color(1f, 1f, 1f, Visibility);
			GUI.DrawTexture(position, _healthBarImage);
			Color color = ColorConverter.HsvToRgb(_barValue / 3f, 1f, 0.9f);
			GUI.color = new Color(color.r, color.g, color.b, Visibility);
			GUI.DrawTexture(new Rect(position.xMin + 1f, position.yMin + 1f, (position.width - 2f) * _barValue, position.height - 2f), _healthBarImage);
			GUI.color = Color.white;
		}
	}

	private void DrawInGameEvent(Rect position, Texture image)
	{
		if (!(image == null))
		{
			GUI.DrawTexture(position, image);
		}
	}

	public void SetAvatarLabel(string name)
	{
		_text = name;
		if (BlueStonez.label_interparkbold_11pt != null)
		{
			_textSize = BlueStonez.label_interparkbold_11pt.CalcSize(new GUIContent(_text));
		}
		else
		{
			_textSize = new Vector2(name.Length * 10, 20f);
		}
	}

	public void SetHealthBarValue(float value)
	{
		_barValue = Mathf.Clamp01(value);
	}

	public void SetInGameFeedback(InGameEventFeedbackType feedbackType)
	{
		_currentFeedback = feedbackType;
		_feedbackTimer = 3f;
	}

	public void SetCharacterInfo(UberStrike.Realtime.UnitySdk.CharacterInfo info)
	{
		if (info != null)
		{
			SetAvatarLabel(info.PlayerName);
			_info = info;
		}
	}

	public void SetTarget(Transform target)
	{
		if ((bool)target)
		{
			_target = target;
		}
	}

	private void FadeInAlphaCorrection()
	{
		if (_needFadeIn)
		{
			_fadeInAlpha += Time.deltaTime;
			if (_fadeInAlpha >= Visibility)
			{
				_needFadeIn = false;
				_fadeInAlpha = 0f;
			}
			else
			{
				Visibility = _fadeInAlpha;
			}
		}
	}
}
