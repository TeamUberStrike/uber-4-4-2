using UnityEngine;

public class PopupAnim : AbstractAnim
{
	private Animatable2DGroup _popupGroup;

	private MeshGUIQuad _glowBlur;

	protected MeshGUIText _popupText;

	private Vector2 _spawnPosition;

	private Vector2 _destBlurScale;

	private Vector2 _destTextScale;

	private AudioClip _sound;

	private float _berpTime;

	private float _displayTime;

	private float _fadeOutAnimTime;

	private bool _isFading;

	public PopupAnim(Animatable2DGroup popupGroup, MeshGUIQuad glowBlur, MeshGUIText multiKillText, Vector2 spawnPosition, Vector2 destBlurScale, Vector2 destMultiKillScale, float displayTime, float fadeOutTime, AudioClip sound)
	{
		_popupGroup = popupGroup;
		_glowBlur = glowBlur;
		_popupText = multiKillText;
		_spawnPosition = spawnPosition;
		_destBlurScale = destBlurScale;
		_destTextScale = destMultiKillScale;
		_berpTime = 0.1f;
		_displayTime = displayTime;
		_fadeOutAnimTime = fadeOutTime;
		_sound = sound;
		Duration = _berpTime + _displayTime + _fadeOutAnimTime;
	}

	protected override void OnStart()
	{
		DoBerpAnim();
		if (_sound != null)
		{
			SfxManager.Play2dAudioClip(_sound);
		}
	}

	protected override void OnStop()
	{
		_popupGroup.RemoveAndFree(_glowBlur);
		_popupGroup.RemoveAndFree(_popupText);
		_isFading = false;
	}

	protected override void OnUpdate()
	{
		if (IsAnimating && Time.time > StartTime + _berpTime + _displayTime && !_isFading)
		{
			DoFadeOutAnim();
		}
	}

	private void DoBerpAnim()
	{
		_popupText.ScaleTo(_destTextScale, _berpTime, EaseType.Berp);
		_popupText.FadeAlphaTo(1f, _berpTime, EaseType.Berp);
		_glowBlur.FadeAlphaTo(1f, _berpTime, EaseType.Berp);
		_glowBlur.ScaleToAroundPivot(_destBlurScale, _spawnPosition, _berpTime, EaseType.Berp);
	}

	private void DoFadeOutAnim()
	{
		if (IsAnimating)
		{
			_isFading = true;
			_popupText.FadeAlphaTo(0f, _fadeOutAnimTime, EaseType.Out);
			_glowBlur.FadeAlphaTo(0f, _fadeOutAnimTime, EaseType.Out);
		}
	}
}
