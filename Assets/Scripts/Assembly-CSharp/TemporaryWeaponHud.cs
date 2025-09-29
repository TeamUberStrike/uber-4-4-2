using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class TemporaryWeaponHud : Singleton<TemporaryWeaponHud>
{
	private Animatable2DGroup _entireGroup;

	private MeshGUICircle _circleBackground;

	private MeshGUICircle _circleForeground;

	private MeshGUIText _remainingSecondsText;

	private int _totalSeconds;

	private int _remainingSeconds;

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_entireGroup.Show();
			}
			else
			{
				_entireGroup.Hide();
			}
		}
	}

	public int RemainingSeconds
	{
		get
		{
			return _remainingSeconds;
		}
		set
		{
			if (value != _remainingSeconds)
			{
				_remainingSeconds = ((value > 0) ? value : 0);
				_remainingSecondsText.Text = _remainingSeconds.ToString();
				OnUpdateRemainingSeconds();
			}
		}
	}

	private TemporaryWeaponHud()
	{
		_entireGroup = new Animatable2DGroup();
		_circleBackground = new MeshGUICircle(HudTextures.QIBlue1);
		_circleBackground.Name = "Temporary Weapon Background";
		_circleBackground.Angle = 360f;
		_circleBackground.Depth = 2f;
		_circleForeground = new MeshGUICircle(HudTextures.QIBlue3);
		_circleForeground.Name = "Temporary Weapon Foreground";
		_circleForeground.Depth = 1f;
		_remainingSecondsText = new MeshGUIText(_remainingSeconds.ToString(), HudAssets.Instance.HelveticaBitmapFont, TextAnchor.MiddleCenter);
		_entireGroup.Group.Add(_circleBackground);
		_entireGroup.Group.Add(_circleForeground);
		_entireGroup.Group.Add(_remainingSecondsText);
		ResetHud();
		Enabled = false;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
	}

	public void StartCounting(int totalSeconds)
	{
		_totalSeconds = totalSeconds;
		RemainingSeconds = totalSeconds;
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		_remainingSecondsText.Scale = new Vector2(HudStyleUtility.SMALLER_DIGITS_SCALE * 0.65f, HudStyleUtility.SMALLER_DIGITS_SCALE * 0.65f);
		float y = _remainingSecondsText.Size.y;
		float num = y / _circleForeground.GetOriginalBounds().x;
		MeshGUICircle circleBackground = _circleBackground;
		Vector2 scale = new Vector2(num, num);
		_circleForeground.Scale = scale;
		circleBackground.Scale = scale;
		_remainingSecondsText.Scale = new Vector2(_circleBackground.Size.x / 2f / _remainingSecondsText.TextBounds.y, _circleBackground.Size.x / 2f / _remainingSecondsText.TextBounds.y);
		_entireGroup.Position = new Vector2((float)Screen.width * 0.89f - _circleBackground.Size.x / 2f, (float)Screen.height * 0.95f - _circleBackground.Size.y / 2f);
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_remainingSecondsText);
		_remainingSecondsText.BitmapMeshText.AlphaMin = 0.4f;
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		if (ev.TeamId == TeamID.RED)
		{
			_circleBackground.Texture = HudTextures.QIRed1;
			_circleForeground.Texture = HudTextures.QIRed3;
			_remainingSecondsText.ShadowColorAnim.Color = HudStyleUtility.DEFAULT_RED_COLOR;
		}
		else
		{
			_circleBackground.Texture = HudTextures.QIBlue1;
			_circleForeground.Texture = HudTextures.QIBlue3;
			_remainingSecondsText.ShadowColorAnim.Color = HudStyleUtility.DEFAULT_BLUE_COLOR;
		}
		_remainingSecondsText.BitmapMeshText.AlphaMin = 0.4f;
	}

	private void OnUpdateRemainingSeconds()
	{
		if (_totalSeconds != 0)
		{
			_circleForeground.Angle = 360f * (float)(-_remainingSeconds) / (float)_totalSeconds;
		}
	}
}
