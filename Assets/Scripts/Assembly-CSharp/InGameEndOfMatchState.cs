using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class InGameEndOfMatchState : IState
{
	private float _nextRoundStartTime;

	private int _endOfMatchCountdown;

	public void OnEnter()
	{
		Singleton<QuickItemController>.Instance.Restriction.RenewGameUses();
		Singleton<QuickItemController>.Instance.IsEnabled = false;
		GamePageManager.Instance.LoadPage(PageType.EndOfMatch);
		SfxManager.Play2dAudioClip(GameAudio.EndOfRound);
		Singleton<PopupHud>.Instance.PopupMatchOver();
		Singleton<PlayerStateMsgHud>.Instance.ButtonEnabled = false;
		Screen.lockCursor = false;
		Singleton<HudDrawFlagGroup>.Instance.BaseDrawFlag = HudDrawFlags.XpPoints | HudDrawFlags.InGameChat;
		HudController.Instance.XpPtsHud.DisplayPermanently();
		CmuneEventHandler.AddListener<OnSetEndOfMatchCountdownEvent>(OnStartCountdown);
	}

	public void OnExit()
	{
		_endOfMatchCountdown = 0;
		SfxManager.Play2dAudioClip(GameAudio.CountdownTonal2);
		MonoRoutine.Start(StartPlayFightAudio());
		GamePageManager.Instance.UnloadCurrentPage();
		CmuneEventHandler.RemoveListener<OnSetEndOfMatchCountdownEvent>(OnStartCountdown);
	}

	public void OnUpdate()
	{
		if (!(_nextRoundStartTime >= Time.time))
		{
			return;
		}
		int num = Mathf.CeilToInt(Mathf.Max(_nextRoundStartTime - Time.time, 0f));
		if (_endOfMatchCountdown != num)
		{
			if (num <= 3 && num >= 1)
			{
				SfxManager.Play2dAudioClip(GameAudio.CountdownTonal1);
			}
			GameState.CurrentGame.UpdatePlayerReadyForNextRound();
			_endOfMatchCountdown = num;
			EndOfMatchCountdownEvent endOfMatchCountdownEvent = new EndOfMatchCountdownEvent();
			endOfMatchCountdownEvent.Countdown = _endOfMatchCountdown;
			CmuneEventHandler.Route(endOfMatchCountdownEvent);
		}
	}

	public void OnGUI()
	{
	}

	private void OnStartCountdown(OnSetEndOfMatchCountdownEvent ev)
	{
		_nextRoundStartTime = Time.time + (float)ev.SecondsUntilNextMatch;
		_endOfMatchCountdown = 0;
	}

	private IEnumerator StartPlayFightAudio()
	{
		yield return new WaitForSeconds(0.2f);
		if (GameState.HasCurrentGame)
		{
			SfxManager.Play2dAudioClip(GameAudio.Fight);
		}
	}
}
