using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class XpPtsHud
{
	private float _screenYOffset;

	private MeshGUIText _xpDigits;

	private MeshGUIText _ptsDigits;

	private MeshGUIText _xpText;

	private MeshGUIText _ptsText;

	private MeshGUIText _curLevelText;

	private MeshGUIText _nextLevelText;

	private MeshGUIQuad _xpBarEmptySprite;

	private MeshGUIQuad _xpBarFullSprite;

	private MeshGUIQuad _glowBlur;

	private Animatable2DGroup _xpGroup;

	private Animatable2DGroup _ptsGroup;

	private Animatable2DGroup _textGroup;

	private Animatable2DGroup _xpBarGroup;

	private Animatable2DGroup _entireGroup;

	private Vector2 _oriBlurPos;

	private Vector2 _oriBlurScale;

	private Vector2 _oriXpBarPos;

	private Vector2 _oriXpBarEmptyScale;

	private Vector2 _oriXpBarFillScale;

	private int _curLevel;

	private int _curXp;

	private int _curPts;

	private float _curXpPercentage;

	private Vector2 _groupPosition;

	public Vector2 ScreenPosition = new Vector2(0.5f, 0.97f);

	public float ScaleFactor = 0.65f;

	private Vector2 _xpPtsSpawnPoint;

	private Vector2 _xpPtsDiePoint;

	private AnimationScheduler _animScheduler;

	private bool _isOnScreen;

	private float _translationDistance;

	private float _xpBarHideTime;

	private bool _isTemporaryDisplay;

	private float _curScreenWidth;

	private float _curScreenHeight;

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
				if (IsXpPtsTextVisible)
				{
					_textGroup.Show();
				}
				else
				{
					_textGroup.Hide();
				}
				if (IsNextLevelVisible)
				{
					_nextLevelText.Show();
				}
				else
				{
					_nextLevelText.Hide();
				}
			}
			else
			{
				_entireGroup.Hide();
			}
		}
	}

	public bool IsXpPtsTextVisible { get; set; }

	public bool IsNextLevelVisible { get; set; }

	private int CurrentLevel
	{
		get
		{
			return _curLevel;
		}
		set
		{
			_curLevel = Mathf.Clamp(value, 1, XpPointsUtil.MaxPlayerLevel);
			int minXp;
			int maxXp;
			XpPointsUtil.GetXpRangeForLevel(_curLevel, out minXp, out maxXp);
			CurrentLevelMinXp = minXp;
			CurrentLevelMaxXp = maxXp;
			UpdateXpPercentage();
		}
	}

	private int CurrentLevelMinXp { get; set; }

	private int CurrentLevelMaxXp { get; set; }

	private int TotalXpOnGameStart { get; set; }

	public XpPtsHud()
	{
		TextAnchor textAnchor = TextAnchor.UpperLeft;
		_xpDigits = new MeshGUIText(_curXp.ToString(), HudAssets.Instance.InterparkBitmapFont, textAnchor);
		_xpDigits.NamePrefix = "XP";
		_xpText = new MeshGUIText("XP", HudAssets.Instance.HelveticaBitmapFont, textAnchor);
		_ptsDigits = new MeshGUIText(_curPts.ToString(), HudAssets.Instance.InterparkBitmapFont, textAnchor);
		_ptsDigits.NamePrefix = "PTS";
		_ptsText = new MeshGUIText("PTS", HudAssets.Instance.HelveticaBitmapFont, textAnchor);
		_xpBarEmptySprite = new MeshGUIQuad(HudTextures.XPBarEmptyBlue);
		_xpBarFullSprite = new MeshGUIQuad(HudTextures.XPBarFull);
		_curLevelText = new MeshGUIText("Lvl 5", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleRight);
		_nextLevelText = new MeshGUIText("Lvl 6", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleLeft);
		_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
		_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
		_glowBlur.Name = "XpPtsHudGlow";
		_glowBlur.Depth = 1f;
		_xpGroup = new Animatable2DGroup();
		_ptsGroup = new Animatable2DGroup();
		_textGroup = new Animatable2DGroup();
		_xpBarGroup = new Animatable2DGroup();
		_entireGroup = new Animatable2DGroup();
		_xpGroup.Group.Add(_xpDigits);
		_xpGroup.Group.Add(_xpText);
		_ptsGroup.Group.Add(_ptsDigits);
		_ptsGroup.Group.Add(_ptsText);
		_textGroup.Group.Add(_xpGroup);
		_textGroup.Group.Add(_ptsGroup);
		_xpBarGroup.Group.Add(_curLevelText);
		_xpBarGroup.Group.Add(_nextLevelText);
		_xpBarGroup.Group.Add(_xpBarEmptySprite);
		_xpBarGroup.Group.Add(_xpBarFullSprite);
		_entireGroup.Group.Add(_glowBlur);
		_entireGroup.Group.Add(_textGroup);
		_entireGroup.Group.Add(_xpBarGroup);
		_animScheduler = new AnimationScheduler();
		_translationDistance = 100f;
		_isOnScreen = false;
		ResetHud();
		Enabled = false;
	}

	public void OnGameStart()
	{
		IsXpPtsTextVisible = true;
		PopdownOffScreen();
		ResetXp();
	}

	public void ResetXp()
	{
		SetXp(0);
		SetPts(0);
		TotalXpOnGameStart = PlayerDataManager.PlayerExperienceSecure;
		CurrentLevel = PlayerDataManager.PlayerLevelSecure;
	}

	public void GainXp(int gainXp)
	{
		if (gainXp != 0)
		{
			SetXp(_curXp + gainXp);
			SfxManager.Play2dAudioClip(GameAudio.GetXP);
		}
	}

	public void GainPoints(int gainPts)
	{
		if (gainPts != 0)
		{
			SetPts(_curPts + gainPts);
			SfxManager.Play2dAudioClip(GameAudio.GetPoints);
		}
	}

	public void Update()
	{
		if (_isOnScreen && _isTemporaryDisplay && Time.time > _xpBarHideTime)
		{
			PopdownOffScreen();
		}
		_animScheduler.Draw();
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	public void DisplayPermanently()
	{
		_isTemporaryDisplay = false;
		_textGroup.Hide();
		_screenYOffset = 50f;
		_entireGroup.MoveTo(_groupPosition, 0f, EaseType.None, 0f);
		_isOnScreen = true;
	}

	public void PopupTemporarily()
	{
		_isTemporaryDisplay = true;
		_screenYOffset = 0f;
		_entireGroup.MoveTo(_groupPosition, 0.1f, EaseType.InOut, 0f);
		_isOnScreen = true;
		float num = 3f;
		_xpBarHideTime = Time.time + num;
	}

	public void PopdownOffScreen(float animTime = 0.1f)
	{
		_entireGroup.MoveTo(new Vector2(_groupPosition.x, _groupPosition.y + _translationDistance), animTime, EaseType.InOut, 0f);
		_isOnScreen = false;
	}

	private void ResetHud()
	{
		OnScreenResolutionChange(new ScreenResolutionEvent());
		OnTeamChange(new OnSetPlayerTeamEvent
		{
			TeamId = TeamID.NONE
		});
		ResetTransform();
	}

	public void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		switch (ev.TeamId)
		{
		case TeamID.NONE:
		case TeamID.BLUE:
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_xpDigits);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_ptsDigits);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_xpText);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_ptsText);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_curLevelText);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_nextLevelText);
			_xpBarEmptySprite.Texture = HudTextures.XPBarEmptyBlue;
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
			break;
		case TeamID.RED:
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_xpDigits);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_ptsDigits);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_xpText);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_ptsText);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_curLevelText);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_nextLevelText);
			_xpBarEmptySprite.Texture = HudTextures.XPBarEmptyRed;
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_RED_COLOR;
			break;
		}
		_xpText.BitmapMeshText.AlphaMin = 0.4f;
		_ptsText.BitmapMeshText.AlphaMin = 0.4f;
	}

	public void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		_curScreenWidth = Screen.width;
		_curScreenHeight = Screen.height;
		ResetTransform();
	}

	private Animatable2DGroup GenerateXp(int xp)
	{
		float num = 0.3f;
		float num2 = 10f;
		Animatable2DGroup animatable2DGroup = new Animatable2DGroup();
		BitmapFont interparkBitmapFont = HudAssets.Instance.InterparkBitmapFont;
		TextAnchor textAnchor = TextAnchor.UpperLeft;
		MeshGUIText meshGUIText = new MeshGUIText((xp <= 0) ? string.Empty : "+", interparkBitmapFont, textAnchor);
		MeshGUIText meshGUIText2 = new MeshGUIText(xp + "XP", interparkBitmapFont, textAnchor);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(meshGUIText);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(meshGUIText2);
		animatable2DGroup.Group.Add(meshGUIText);
		animatable2DGroup.Group.Add(meshGUIText2);
		meshGUIText.Position = new Vector2(0f, 0f);
		meshGUIText.Scale = new Vector2(num, num);
		meshGUIText2.Position = new Vector2(meshGUIText.Size.x + num2, 0f);
		meshGUIText2.Scale = new Vector2(num, num);
		animatable2DGroup.Position = new Vector2(_xpPtsSpawnPoint.x - animatable2DGroup.GetRect().width / 2f, _xpPtsSpawnPoint.y);
		animatable2DGroup.UpdateMeshGUIPosition();
		return animatable2DGroup;
	}

	private Animatable2DGroup GeneratePts(int pts)
	{
		float num = 0.3f;
		float num2 = 10f;
		Animatable2DGroup animatable2DGroup = new Animatable2DGroup();
		BitmapFont interparkBitmapFont = HudAssets.Instance.InterparkBitmapFont;
		TextAnchor textAnchor = TextAnchor.UpperLeft;
		MeshGUIText meshGUIText = new MeshGUIText((pts <= 0) ? string.Empty : "+", interparkBitmapFont, textAnchor);
		MeshGUIText meshGUIText2 = new MeshGUIText(pts + "PTS", interparkBitmapFont, textAnchor);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(meshGUIText);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(meshGUIText2);
		animatable2DGroup.Group.Add(meshGUIText);
		animatable2DGroup.Group.Add(meshGUIText2);
		meshGUIText.Position = new Vector2(0f, 0f);
		meshGUIText.Scale = new Vector2(num, num);
		meshGUIText2.Position = new Vector2(meshGUIText.Size.x + num2, 0f);
		meshGUIText2.Scale = new Vector2(num, num);
		animatable2DGroup.Position = new Vector2(_xpPtsSpawnPoint.x - animatable2DGroup.GetRect().width / 2f, _xpPtsSpawnPoint.y);
		animatable2DGroup.UpdateMeshGUIPosition();
		return animatable2DGroup;
	}

	private IEnumerator EmitSprite(IAnimatable2D animatable, string[] args)
	{
		PopupTemporarily();
		float flickerTime = 0.5f;
		float fallDownTime = 0.3f;
		animatable.Flicker(flickerTime);
		animatable.ScaleAroundPivot(new Vector2(2f, 2f), animatable.GetCenter(), flickerTime, EaseType.Berp);
		yield return new WaitForSeconds(flickerTime);
		animatable.Move(new Vector2(0f, _xpPtsDiePoint.y - _xpPtsSpawnPoint.y), fallDownTime, EaseType.In);
		animatable.FadeAlphaTo(0f, fallDownTime, EaseType.In);
		yield return new WaitForSeconds(fallDownTime);
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(_xpDigits);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(_ptsDigits);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(_xpText);
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(_ptsText);
		_xpText.BitmapMeshText.AlphaMin = 0.4f;
		_ptsText.BitmapMeshText.AlphaMin = 0.4f;
	}

	public void ResetTransform()
	{
		ResetXpPtsTransform();
		ResetXpBarTransform();
		if (CurrentLevel != 0)
		{
			ResetLevelTxtTransform();
		}
		ResetBlurTransform();
		ResetGroupTransform();
		_xpPtsSpawnPoint = new Vector2(_curScreenWidth / 2f, _curScreenHeight / 2f);
		_xpPtsDiePoint = new Vector2(_curScreenWidth * ScreenPosition.x, _curScreenHeight * ScreenPosition.y);
	}

	private void ResetXpPtsTransform()
	{
		_xpDigits.Text = _curXp.ToString();
		_xpDigits.Position = new Vector2(0f, 0f);
		_xpDigits.Scale = new Vector2(0.7f * ScaleFactor, 0.7f * ScaleFactor);
		float num = 3f;
		float num2 = 0f;
		num2 += _xpDigits.Size.x + num;
		_xpText.Scale = new Vector2(0.35f * ScaleFactor, 0.35f * ScaleFactor);
		_xpText.Position = new Vector2(num2, _xpDigits.Size.y - _xpText.Size.y * 1.2f);
		num2 += _xpText.Size.x + num;
		_ptsDigits.Text = _curPts.ToString();
		_ptsDigits.Scale = new Vector2(0.7f * ScaleFactor, 0.7f * ScaleFactor);
		_ptsDigits.Position = new Vector2(num2, 0f);
		num2 += _ptsDigits.Size.x + num;
		_ptsText.Scale = new Vector2(0.35f * ScaleFactor, 0.35f * ScaleFactor);
		_ptsText.Position = new Vector2(num2, _xpText.Position.y);
	}

	private void ResetXpBarTransform()
	{
		float num = (float)Screen.width * HudStyleUtility.XP_BAR_WIDTH_PROPORTION_IN_SCREEN;
		_oriXpBarEmptyScale.x = (_oriXpBarEmptyScale.y = num / (float)HudTextures.XPBarEmptyBlue.width);
		_oriXpBarFillScale.y = _oriXpBarEmptyScale.y;
		_oriXpBarFillScale.x = _oriXpBarFillScale.y * _curXpPercentage;
		_oriXpBarPos.x = (0f - (num - _textGroup.Rect.width)) / 2f;
		_oriXpBarPos.y = _xpDigits.Size.y;
		float num2 = 8.5f;
		_xpBarEmptySprite.Scale = _oriXpBarEmptyScale;
		_xpBarFullSprite.Scale = _oriXpBarFillScale;
		_xpBarEmptySprite.Position = _oriXpBarPos;
		_xpBarFullSprite.Position = _oriXpBarPos;
		_xpBarFullSprite.Scale = new Vector2(_xpBarFullSprite.Scale.y * (512f - num2 * 2f) / 512f * _curXpPercentage, _xpBarFullSprite.Scale.y * 0.95f);
		Vector2 position = _xpBarEmptySprite.Position;
		position.x += num2 * _xpBarEmptySprite.Scale.x;
		position.y += num2 * _xpBarEmptySprite.Scale.x;
		_xpBarFullSprite.Position = position;
	}

	private void ResetLevelTxtTransform()
	{
		float num = 0.5f;
		_curLevelText.Text = "Lvl " + CurrentLevel;
		_curLevelText.Position = new Vector2(_xpBarEmptySprite.Position.x - 20f, _xpBarEmptySprite.Position.y);
		_curLevelText.Scale = new Vector2(num * ScaleFactor, num * ScaleFactor);
		if (CurrentLevel < XpPointsUtil.MaxPlayerLevel)
		{
			IsNextLevelVisible = true;
			int num2 = CurrentLevel + 1;
			_nextLevelText.Text = "Lvl " + num2;
			_nextLevelText.Position = new Vector2(_xpBarEmptySprite.Position.x + _xpBarEmptySprite.Rect.width + 20f, _xpBarEmptySprite.Position.y);
			_nextLevelText.Scale = new Vector2(num * ScaleFactor, num * ScaleFactor);
		}
		else
		{
			IsNextLevelVisible = false;
		}
	}

	private void ResetBlurTransform()
	{
		float num = _xpBarEmptySprite.Rect.width * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR;
		float num2 = _xpBarGroup.Rect.height * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_oriBlurScale.x = num / (float)HudTextures.WhiteBlur128.width;
		_oriBlurScale.y = num2 / (float)HudTextures.WhiteBlur128.height;
		_oriBlurPos.x = (_textGroup.Rect.width - num) / 2f;
		_oriBlurPos.y = (_textGroup.Rect.height - num2) / 2f;
		if (!IsXpPtsTextVisible)
		{
			_oriBlurPos.y += num2 * 0.1f;
		}
		_glowBlur.Scale = _oriBlurScale;
		_glowBlur.Position = _oriBlurPos;
	}

	private void ResetGroupTransform()
	{
		_translationDistance = (_entireGroup.Rect.height + _screenYOffset) * 1.5f;
		_groupPosition = new Vector2(_curScreenWidth * ScreenPosition.x - _textGroup.Rect.width / 2f, _curScreenHeight * ScreenPosition.y - _textGroup.Rect.height - _xpBarEmptySprite.Rect.height - _screenYOffset);
		if (!_isOnScreen)
		{
			_entireGroup.Position = new Vector2(_groupPosition.x, _groupPosition.y + _translationDistance);
		}
		else
		{
			_entireGroup.Position = _groupPosition;
		}
	}

	private IEnumerator OnXpIncrease()
	{
		float scaleFactor = 1.2f;
		float scaleUpTime = 0.02f;
		float flickerTime = 0.1f;
		float scaleDownTime = 0.1f;
		Vector2 pivot = _xpDigits.Center;
		_xpDigits.ScaleAroundPivot(new Vector2(scaleFactor, scaleFactor), pivot, scaleUpTime);
		yield return new WaitForSeconds(scaleUpTime);
		_xpDigits.Flicker(flickerTime);
		yield return new WaitForSeconds(flickerTime);
		_xpDigits.ScaleAroundPivot(new Vector2(1f / scaleFactor, 1f / scaleFactor), pivot, scaleDownTime);
	}

	private void UpdateXpPercentage()
	{
		if (CurrentLevel == XpPointsUtil.MaxPlayerLevel)
		{
			SetXpPercentage(1f);
			return;
		}
		if (TotalXpOnGameStart + _curXp > CurrentLevelMaxXp)
		{
			CurrentLevel++;
		}
		int num = CurrentLevelMaxXp - CurrentLevelMinXp;
		if (num != 0)
		{
			SetXpPercentage((float)(TotalXpOnGameStart + _curXp - CurrentLevelMinXp) / (float)num);
		}
	}

	private void SetXp(int xp)
	{
		bool flag = xp > _curXp;
		_curXp = ((xp >= 0) ? xp : 0);
		UpdateXpPercentage();
		ResetTransform();
		if (flag)
		{
			MonoRoutine.Start(OnXpIncrease());
		}
	}

	private void SetPts(int pts)
	{
		_curPts = ((pts >= 0) ? pts : 0);
		ResetTransform();
	}

	private void SetXpPercentage(float xpPercentage)
	{
		_curXpPercentage = Mathf.Clamp01(xpPercentage);
		ResetXpBarTransform();
	}
}
