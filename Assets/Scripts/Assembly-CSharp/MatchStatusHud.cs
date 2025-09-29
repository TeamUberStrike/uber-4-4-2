using System.Collections;
using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class MatchStatusHud : Singleton<MatchStatusHud>
{
	private float _textScale;

	private MeshGUIText _teamScoreLeft;

	private Sprite2DGUI _boxOverlayRed;

	private Animatable2DGroup _leftScoreGroup;

	private MeshGUIText _teamScoreRight;

	private Sprite2DGUI _boxOverlayBlue;

	private Animatable2DGroup _rightScoreGroup;

	private Animatable2DGroup _scoreGroup;

	private MeshGUIText _remainingKillText;

	private MeshGUIText _clockText;

	private Animatable2DGroup _entireGroup;

	private int _curLeftTeamScore;

	private int _curRightTeamScore;

	private int _remainingSeconds;

	private int _remainingKills;

	private float normalHalfClockHeight;

	private Dictionary<int, AudioClip> _killLeftAudioMap;

	private static int WARNING_TIME_LOW_VALUE = 30;

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

	public bool IsScoreEnabled
	{
		get
		{
			return _scoreGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_scoreGroup.Show();
			}
			else
			{
				_scoreGroup.Hide();
			}
		}
	}

	public bool IsClockEnabled
	{
		get
		{
			return _clockText.IsVisible;
		}
		set
		{
			if (value)
			{
				_clockText.Show();
			}
			else
			{
				_clockText.Hide();
			}
		}
	}

	public bool IsRemainingKillEnabled
	{
		get
		{
			return _remainingKillText.IsVisible;
		}
		set
		{
			if (value)
			{
				_remainingKillText.Show();
			}
			else
			{
				_remainingKillText.Hide();
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
			if (_remainingSeconds != value)
			{
				_remainingSeconds = ((value > 0) ? value : 0);
				_clockText.Text = GetClockString(_remainingSeconds);
				OnUpdateRemainingSeconds();
			}
		}
	}

	public int RemainingKills
	{
		get
		{
			return _remainingKills;
		}
		set
		{
			if (_remainingKills != value)
			{
				_remainingKills = ((value > 0) ? value : 0);
				_remainingKillText.Text = GetRemainingKillString(_remainingKills);
			}
		}
	}

	public string RemainingRoundsText
	{
		set
		{
			_remainingKillText.Text = value;
		}
	}

	public int BlueTeamScore
	{
		get
		{
			return _curLeftTeamScore;
		}
		set
		{
			_curLeftTeamScore = value;
			_teamScoreLeft.Text = _curLeftTeamScore.ToString();
			ResetTeamScoreGroupTransform();
		}
	}

	public int RedTeamScore
	{
		get
		{
			return _curRightTeamScore;
		}
		set
		{
			_curRightTeamScore = value;
			_teamScoreRight.Text = _curRightTeamScore.ToString();
			ResetTeamScoreGroupTransform();
		}
	}

	private MatchStatusHud()
	{
		_remainingSeconds = 0;
		_teamScoreLeft = new MeshGUIText("0", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_teamScoreLeft.NamePrefix = "TeamScore";
		_boxOverlayBlue = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_leftScoreGroup = new Animatable2DGroup();
		_leftScoreGroup.Group.Add(_teamScoreLeft);
		_leftScoreGroup.Group.Add(_boxOverlayBlue);
		_teamScoreRight = new MeshGUIText("0", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_teamScoreRight.NamePrefix = "TeamScore";
		_boxOverlayRed = new Sprite2DGUI(new GUIContent(), StormFront.RedBox);
		_rightScoreGroup = new Animatable2DGroup();
		_rightScoreGroup.Group.Add(_teamScoreRight);
		_rightScoreGroup.Group.Add(_boxOverlayRed);
		_scoreGroup = new Animatable2DGroup();
		_scoreGroup.Group.Add(_leftScoreGroup);
		_scoreGroup.Group.Add(_rightScoreGroup);
		_clockText = new MeshGUIText(GetClockString(_remainingSeconds), HudAssets.Instance.InterparkBitmapFont, TextAnchor.UpperCenter);
		_clockText.NamePrefix = "Clock";
		_remainingKillText = new MeshGUIText(GetRemainingKillString(_remainingKills), HudAssets.Instance.InterparkBitmapFont, TextAnchor.UpperCenter);
		_entireGroup = new Animatable2DGroup();
		_entireGroup.Group.Add(_scoreGroup);
		_entireGroup.Group.Add(_clockText);
		_entireGroup.Group.Add(_remainingKillText);
		_killLeftAudioMap = new Dictionary<int, AudioClip>(5);
		ResetHud();
		Enabled = true;
		IsClockEnabled = false;
		IsScoreEnabled = false;
		IsRemainingKillEnabled = false;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
		CmuneEventHandler.AddListener<OnGlobalUIRibbonChanged>(OnGlobalUIRibbonChange);
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

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_teamScoreLeft);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_teamScoreRight);
		_teamScoreRight.BitmapMeshText.ShadowColor = HudStyleUtility.DEFAULT_RED_COLOR;
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_clockText);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_remainingKillText);
		_remainingKillText.BitmapMeshText.ShadowColor = Color.black;
		_remainingKillText.BitmapMeshText.AlphaMin = 0.1f;
		_remainingKillText.BitmapMeshText.AlphaMax = 0.6f;
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		if (ev.TeamId == TeamID.RED)
		{
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_clockText);
		}
		else
		{
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_clockText);
		}
	}

	private void OnGlobalUIRibbonChange(OnGlobalUIRibbonChanged ev)
	{
		ResetTransform();
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		_textScale = 0.45f;
		ResetClockText();
		ResetRemainingKillText();
		ResetTeamScoreGroupTransform();
		float num = (float)Screen.height * 0.02f;
		if (GlobalUIRibbon.Instance.IsEnabled)
		{
			num += 44f;
		}
		_entireGroup.Position = new Vector2(Screen.width / 2, num);
	}

	private void ResetClockText()
	{
		_clockText.Scale = new Vector2(_textScale, _textScale);
		_clockText.Position = Vector2.zero;
		normalHalfClockHeight = _clockText.Rect.height / 2f;
	}

	private void ResetRemainingKillText()
	{
		_remainingKillText.Scale = new Vector2(_textScale * 0.4f, _textScale * 0.4f);
		_remainingKillText.Position = new Vector2(_clockText.Position.x, _clockText.Position.y + _clockText.Size.y);
	}

	private void ResetTeamScoreGroupTransform()
	{
		MeshGUIText teamScoreLeft = _teamScoreLeft;
		Vector2 scale = new Vector2(_textScale * 1.2f, _textScale * 1.2f);
		_teamScoreRight.Scale = scale;
		teamScoreLeft.Scale = scale;
		int num = ((_teamScoreLeft.Text.Length <= _teamScoreRight.Text.Length) ? _teamScoreRight.Text.Length : _teamScoreLeft.Text.Length);
		float y = _teamScoreLeft.Size.y;
		float num2 = y * 1.2f;
		num2 = ((num > 2) ? (y * (float)num * 0.5f) : num2);
		float num3 = 1.3f;
		_boxOverlayBlue.Scale = new Vector2(num2 / _boxOverlayBlue.GUIBounds.x, y / _boxOverlayBlue.GUIBounds.y) * num3;
		_boxOverlayRed.Scale = new Vector2(num2 / _boxOverlayRed.GUIBounds.x, y / _boxOverlayRed.GUIBounds.y) * num3;
		_boxOverlayRed.Position = -_boxOverlayRed.Size / 2f;
		_boxOverlayBlue.Position = -_boxOverlayBlue.Size / 2f;
		_leftScoreGroup.Position = new Vector2((0f - num2) / 2f - (float)Screen.height * 0.09f, normalHalfClockHeight * 1.5f);
		_rightScoreGroup.Position = new Vector2(num2 / 2f + (float)Screen.height * 0.09f, normalHalfClockHeight * 1.5f);
	}

	private IEnumerator PulseClock()
	{
		int emissionId = Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(PulseClock);
		ResetClockText();
		_clockText.StopScaling();
		float sizeIncreaseTime = 0.1f;
		float sizeDecreaseTime = 0.5f;
		float sizeIncreaseScale = 1.2f;
		Vector2 pivot = _clockText.Center;
		_clockText.ScaleAroundPivot(new Vector2(sizeIncreaseScale, sizeIncreaseScale), pivot, sizeIncreaseTime);
		yield return new WaitForSeconds(sizeIncreaseTime);
		if (Singleton<PreemptiveCoroutineManager>.Instance.IsCurrent(PulseClock, emissionId))
		{
			_clockText.ScaleAroundPivot(new Vector2(1f / sizeIncreaseScale, 1f / sizeIncreaseScale), pivot, sizeDecreaseTime);
		}
	}

	private void OnUpdateRemainingSeconds()
	{
		if (_remainingSeconds <= WARNING_TIME_LOW_VALUE)
		{
			if (_remainingSeconds != 0)
			{
				MonoRoutine.Start(PulseClock());
			}
			else
			{
				StopClockPulsing();
			}
		}
		switch (_remainingSeconds)
		{
		case 5:
			SfxManager.Play2dAudioClip(GameAudio.MatchEndingCountdown5);
			break;
		case 4:
			SfxManager.Play2dAudioClip(GameAudio.MatchEndingCountdown4);
			break;
		case 3:
			SfxManager.Play2dAudioClip(GameAudio.MatchEndingCountdown3);
			break;
		case 2:
			SfxManager.Play2dAudioClip(GameAudio.MatchEndingCountdown2);
			break;
		case 1:
			SfxManager.Play2dAudioClip(GameAudio.MatchEndingCountdown1);
			break;
		}
	}

	private void StopClockPulsing()
	{
		Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(PulseClock);
		_clockText.StopScaling();
		ResetClockText();
	}

	public void PlayKillsLeftAudio(int killsLeft)
	{
		AudioClip value;
		if (_killLeftAudioMap.TryGetValue(killsLeft, out value))
		{
			SfxManager.Play2dAudioClip(value, 2f);
			_killLeftAudioMap.Remove(killsLeft);
		}
	}

	public void ResetKillsLeftAudio()
	{
		AudioClip[] array = new AudioClip[5]
		{
			GameAudio.KillLeft1,
			GameAudio.KillLeft2,
			GameAudio.KillLeft3,
			GameAudio.KillLeft4,
			GameAudio.KillLeft5
		};
		for (int i = 0; i < array.Length; i++)
		{
			_killLeftAudioMap[i + 1] = array[i];
		}
	}

	private string GetClockString(int remainingSeconds)
	{
		int num = remainingSeconds / 60;
		int num2 = remainingSeconds % 60;
		string text = ((num < 10) ? ("0" + num) : num.ToString());
		string text2 = ((num2 < 10) ? ("0" + num2) : num2.ToString());
		return text + ":" + text2;
	}

	private string GetRemainingKillString(int remainingKills)
	{
		if (remainingKills > 1)
		{
			return remainingKills + " Kills Left";
		}
		return remainingKills + " Kill Left";
	}
}
