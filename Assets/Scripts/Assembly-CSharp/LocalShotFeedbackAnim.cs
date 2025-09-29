using UnityEngine;

internal class LocalShotFeedbackAnim : AbstractAnim
{
	private Animatable2DGroup _textGroup;

	private MeshGUIText _text;

	private float _displayTime;

	private float _fadeOutAnimTime;

	private bool _isFading;

	private AudioClip _sound;

	public LocalShotFeedbackAnim(Animatable2DGroup textGroup, MeshGUIText meshText, float displayTime, float fadeOutAnimTime, AudioClip sound)
	{
		_textGroup = textGroup;
		_text = meshText;
		_displayTime = displayTime;
		_fadeOutAnimTime = fadeOutAnimTime;
		_sound = sound;
		Duration = _displayTime + _fadeOutAnimTime;
	}

	protected override void OnStart()
	{
		_textGroup.Group.Add(_text);
		_text.FadeAlphaTo(1f);
		SfxManager.Play2dAudioClip(_sound);
	}

	protected override void OnStop()
	{
		_text.FadeAlphaTo(0f);
		_text.StopFading();
		_isFading = false;
		_textGroup.RemoveAndFree(_text);
	}

	protected override void OnUpdate()
	{
		if (IsAnimating && Time.time > StartTime + _displayTime && !_isFading)
		{
			DoFadeoutAnim();
		}
		_text.ShadowColorAnim.Alpha = 0f;
	}

	private void DoFadeoutAnim()
	{
		if (IsAnimating)
		{
			_isFading = true;
			_text.FadeAlphaTo(0f, _fadeOutAnimTime, EaseType.Out);
		}
	}
}
