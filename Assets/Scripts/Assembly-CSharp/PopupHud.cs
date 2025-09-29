using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PopupHud : Singleton<PopupHud>
{
	private Color _doubleKillColor;

	private Color _tripleKillColor;

	private Color _quadKillColor;

	private Color _megaKillColor;

	private Color _uberKillColor;

	private Color _defaultColor;

	private MeshGUIQuad _glowBlur;

	private MeshGUIText _popupText;

	private Animatable2DGroup _popupGroup;

	private Vector2 _spawnPosition;

	private float _scaleEnlargeFactor;

	private Vector2 _doubleKillScale;

	private Vector2 _tripleKillScale;

	private Vector2 _quadKillScale;

	private Vector2 _megaKillScale;

	private Vector2 _uberKillScale;

	private Vector2 _defaultScale;

	private Vector2 _destBlurScale;

	private Vector2 _destTextScale;

	private float _displayTime;

	private float _fadeOutTime;

	private AudioClip _sound;

	public bool Enabled
	{
		get
		{
			return _popupGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_popupGroup.Show();
			}
			else
			{
				_popupGroup.Hide();
			}
		}
	}

	private PopupHud()
	{
		_spawnPosition = new Vector2(Screen.width / 2, 200f);
		_doubleKillColor = new Color(0.78039217f, 31f / 51f, 0f);
		_tripleKillColor = new Color(64f / 85f, 0.5882353f, 0f);
		_quadKillColor = new Color(64f / 85f, 0.4627451f, 0f);
		_megaKillColor = new Color(64f / 85f, 0.3372549f, 0f);
		_uberKillColor = new Color(64f / 85f, 18f / 85f, 0f);
		_defaultColor = Color.white;
		_scaleEnlargeFactor = 1.1f;
		_doubleKillScale = new Vector2(0.8f, 0.8f);
		_tripleKillScale = _doubleKillScale * _scaleEnlargeFactor;
		_quadKillScale = _tripleKillScale * _scaleEnlargeFactor;
		_megaKillScale = _quadKillScale * _scaleEnlargeFactor;
		_uberKillScale = _megaKillScale * _scaleEnlargeFactor;
		_defaultScale = _doubleKillScale;
		_popupGroup = new Animatable2DGroup();
	}

	public void Draw()
	{
		_popupGroup.Draw();
	}

	public void PopupMultiKill(int killCount)
	{
		if (killCount >= 2 && killCount <= 6)
		{
			DoPopup((PopupType)killCount);
		}
	}

	public void PopupRoundStart()
	{
		PopupType type = PopupType.RoundStart;
		DoPopup(type);
	}

	public void PopupWinTeam(TeamID teamId)
	{
		PopupType popupType = PopupType.None;
		switch (teamId)
		{
		case TeamID.BLUE:
			popupType = PopupType.BlueTeamWins;
			SfxManager.Play2dAudioClip(GameAudio.BlueWins);
			break;
		case TeamID.RED:
			popupType = PopupType.RedTeamWins;
			SfxManager.Play2dAudioClip(GameAudio.RedWins);
			break;
		default:
			popupType = PopupType.Draw;
			SfxManager.Play2dAudioClip(GameAudio.Draw);
			break;
		}
		DoPopup(popupType);
	}

	public void PopupMatchOver()
	{
		PopupType type = PopupType.GameOver;
		DoPopup(type);
	}

	private void DoPopup(PopupType type)
	{
		CreateNewPopupGroup();
		ResetStyleAndTransform(type);
		IAnim anim = ((type != PopupType.RoundStart) ? new PopupAnim(_popupGroup, _glowBlur, _popupText, _spawnPosition, _destBlurScale, _destTextScale, _displayTime, _fadeOutTime, _sound) : new RoundStartAnim(_popupGroup, _glowBlur, _popupText, _spawnPosition, _destBlurScale, _destTextScale, _displayTime, _fadeOutTime, _sound));
		if (type >= PopupType.RoundStart)
		{
			Singleton<InGameFeatHud>.Instance.AnimationScheduler.ClearAll();
		}
		Singleton<InGameFeatHud>.Instance.AnimationScheduler.EnqueueAnim(anim);
	}

	private void ResetStyleAndTransform(PopupType type)
	{
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_popupText);
		UpdateScales();
		_spawnPosition = Singleton<InGameFeatHud>.Instance.AnchorPoint;
		_displayTime = 1f;
		_fadeOutTime = 1f;
		_popupText.BitmapMeshText.ShadowColor = Color.white;
		_destTextScale = _defaultScale;
		_glowBlur.Color = _defaultColor;
		_sound = null;
		switch (type)
		{
		case PopupType.DoubleKill:
			_popupText.Text = "DOUBLE KILL";
			_popupText.BitmapMeshText.ShadowColor = _doubleKillColor;
			_destTextScale = _doubleKillScale;
			_glowBlur.Color = _doubleKillColor;
			_sound = GameAudio.DoubleKill;
			break;
		case PopupType.TripleKill:
			_popupText.Text = "TRIPLE KILL";
			_popupText.BitmapMeshText.ShadowColor = _tripleKillColor;
			_destTextScale = _tripleKillScale;
			_glowBlur.Color = _tripleKillColor;
			_sound = GameAudio.TripleKill;
			break;
		case PopupType.QuadKill:
			_popupText.Text = "QUAD KILL";
			_popupText.BitmapMeshText.ShadowColor = _quadKillColor;
			_destTextScale = _quadKillScale;
			_glowBlur.Color = _quadKillColor;
			_sound = GameAudio.QuadKill;
			break;
		case PopupType.MegaKill:
			_popupText.Text = "MEGA KILL";
			_popupText.BitmapMeshText.ShadowColor = _megaKillColor;
			_destTextScale = _megaKillScale;
			_glowBlur.Color = _megaKillColor;
			_sound = GameAudio.MegaKill;
			break;
		case PopupType.UberKill:
			_popupText.Text = "UBER KILL";
			_popupText.BitmapMeshText.ShadowColor = _uberKillColor;
			_destTextScale = _uberKillScale;
			_glowBlur.Color = _uberKillColor;
			_sound = GameAudio.UberKill;
			_fadeOutTime = 5f;
			break;
		case PopupType.RoundStart:
			_popupText.Text = "Round Starts In";
			_displayTime = 5f;
			break;
		case PopupType.GameOver:
			_popupText.Text = "Game Over";
			_displayTime = 2f;
			_spawnPosition += new Vector2(0f, (float)Screen.height * 0.3f);
			break;
		case PopupType.RedTeamWins:
			_popupText.Text = "Red Team Wins";
			_popupText.BitmapMeshText.ShadowColor = HudStyleUtility.DEFAULT_RED_COLOR;
			_glowBlur.Color = HudStyleUtility.DEFAULT_RED_COLOR;
			_displayTime = 2f;
			_spawnPosition += new Vector2(0f, (float)Screen.height * 0.3f);
			break;
		case PopupType.BlueTeamWins:
			_popupText.Text = "Blue Team Wins";
			_popupText.BitmapMeshText.ShadowColor = HudStyleUtility.DEFAULT_BLUE_COLOR;
			_glowBlur.Color = HudStyleUtility.DEFAULT_BLUE_COLOR;
			_displayTime = 2f;
			_spawnPosition += new Vector2(0f, (float)Screen.height * 0.3f);
			break;
		case PopupType.Draw:
			_popupText.Text = "Draw!";
			_displayTime = 2f;
			_spawnPosition += new Vector2(0f, (float)Screen.height * 0.3f);
			break;
		}
		_popupText.Scale = _destTextScale;
		float num = _popupText.Size.x * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR;
		float num2 = _popupText.Size.y * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_destBlurScale = new Vector2(num / (float)HudTextures.WhiteBlur128.width, num2 / (float)HudTextures.WhiteBlur128.height);
		_glowBlur.Scale = Vector2.zero;
		_glowBlur.Position = _spawnPosition;
		_popupText.Position = _spawnPosition;
		_popupText.Alpha = 0f;
		_popupText.Scale = Vector2.zero;
	}

	private IEnumerator EmitPopup()
	{
		int emissionId = Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(EmitPopup);
		float berpTime = 0.1f;
		float displayTime = 1f;
		_popupText.ScaleTo(_destTextScale, berpTime, EaseType.Berp);
		_popupText.FadeAlphaTo(1f, berpTime, EaseType.Berp);
		_glowBlur.FadeAlphaTo(1f, berpTime, EaseType.Berp);
		_glowBlur.ScaleToAroundPivot(_destBlurScale, _spawnPosition, berpTime, EaseType.Berp);
		yield return new WaitForSeconds(displayTime);
		if (Singleton<PreemptiveCoroutineManager>.Instance.IsCurrent(EmitPopup, emissionId))
		{
			_popupText.FadeAlphaTo(0f, _fadeOutTime, EaseType.Out);
			_glowBlur.FadeAlphaTo(0f, _fadeOutTime, EaseType.Out);
			yield return new WaitForSeconds(_fadeOutTime);
		}
	}

	private void CreateNewPopupGroup()
	{
		_popupText = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_popupText.NamePrefix = "Popup";
		_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
		_glowBlur.Name = "PopupHudGlow";
		_popupGroup.Group.Add(_popupText);
		_popupGroup.Group.Add(_glowBlur);
	}

	private void UpdateScales()
	{
		float num = Singleton<InGameFeatHud>.Instance.TextHeight / _popupText.TextBounds.y;
		_doubleKillScale = new Vector2(num, num);
		_tripleKillScale = _doubleKillScale * _scaleEnlargeFactor;
		_quadKillScale = _tripleKillScale * _scaleEnlargeFactor;
		_megaKillScale = _quadKillScale * _scaleEnlargeFactor;
		_uberKillScale = _megaKillScale * _scaleEnlargeFactor;
	}
}
