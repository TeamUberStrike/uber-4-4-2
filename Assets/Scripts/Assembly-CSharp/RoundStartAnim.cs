using UnityEngine;

public class RoundStartAnim : PopupAnim
{
	private bool _countdown5;

	private bool _countdown4;

	private bool _countdown3;

	private bool _countdown2;

	private bool _countdown1;

	private bool _countdown0;

	public RoundStartAnim(Animatable2DGroup popupGroup, MeshGUIQuad glowBlur, MeshGUIText multiKillText, Vector2 spawnPosition, Vector2 destBlurScale, Vector2 destMultiKillScale, float displayTime, float fadeOutTime, AudioClip sound)
		: base(popupGroup, glowBlur, multiKillText, spawnPosition, destBlurScale, destMultiKillScale, displayTime, fadeOutTime, sound)
	{
	}

	protected override void OnStart()
	{
		base.OnStart();
		_countdown5 = false;
		_countdown4 = false;
		_countdown3 = false;
		_countdown2 = false;
		_countdown1 = false;
		_countdown0 = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (IsAnimating)
		{
			int num = 6 - Mathf.CeilToInt(Time.time - StartTime);
			if (num == 5 && !_countdown5)
			{
				OnRoundStartCountdown("Round Starts In 5", GameAudio.MatchEndingCountdown5);
				_countdown5 = true;
			}
			else if (num == 4 && !_countdown4)
			{
				OnRoundStartCountdown("Round Starts In 4", GameAudio.MatchEndingCountdown4);
				_countdown4 = true;
			}
			else if (num == 3 && !_countdown3)
			{
				OnRoundStartCountdown("Round Starts In 3", GameAudio.MatchEndingCountdown3);
				_countdown3 = true;
			}
			else if (num == 2 && !_countdown2)
			{
				OnRoundStartCountdown("Round Starts In 2", GameAudio.MatchEndingCountdown2);
				_countdown2 = true;
			}
			else if (num == 1 && !_countdown1)
			{
				OnRoundStartCountdown("Round Starts In 1", GameAudio.MatchEndingCountdown1);
				_countdown1 = true;
			}
			else if (num == 0 && !_countdown0)
			{
				OnRoundStartCountdown("Fight", GameAudio.Fight);
				_countdown0 = true;
			}
		}
	}

	private void OnRoundStartCountdown(string textStr, AudioClip sound)
	{
		_popupText.Text = textStr;
		SfxManager.Play2dAudioClip(sound);
	}
}
