using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class HudUtil : Singleton<HudUtil>
{
	private int _lastScreenWidth;

	private int _lastScreenHeight;

	private HudUtil()
	{
	}

	public void ClearAllHud()
	{
		CleanAllTemporaryHud();
		Singleton<ScreenshotHud>.Instance.Enable = false;
		Singleton<FrameRateHud>.Instance.Enable = false;
	}

	public void ClearAllFeedbackHud()
	{
		Singleton<EventStreamHud>.Instance.ClearAllEvents();
		Singleton<EventFeedbackHud>.Instance.ClearAll();
		Singleton<InGameFeatHud>.Instance.AnimationScheduler.ClearAll();
		Singleton<DamageFeedbackHud>.Instance.ClearAll();
		Singleton<PlayerStateMsgHud>.Instance.DisplayNone();
	}

	public void SetPlayerTeam(TeamID teamId)
	{
		OnSetPlayerTeamEvent onSetPlayerTeamEvent = new OnSetPlayerTeamEvent();
		onSetPlayerTeamEvent.TeamId = teamId;
		CmuneEventHandler.Route(onSetPlayerTeamEvent);
	}

	public void ShowContinueButton()
	{
		Singleton<PlayerStateMsgHud>.Instance.ButtonEnabled = true;
		Singleton<PlayerStateMsgHud>.Instance.ButtonCaption = LocalizedStrings.Continue;
		Singleton<PlayerStateMsgHud>.Instance.OnButtonClicked = OnContinueButtonClicked;
	}

	public void ShowRespawnButton()
	{
		Singleton<PlayerStateMsgHud>.Instance.ButtonEnabled = true;
		Singleton<PlayerStateMsgHud>.Instance.TemporaryMsgEnabled = false;
		Singleton<PlayerStateMsgHud>.Instance.ButtonCaption = LocalizedStrings.Respawn;
		Singleton<PlayerStateMsgHud>.Instance.OnButtonClicked = OnRespawnButtonClicked;
	}

	public void ShowClickToRespawnText(FpsGameMode fpsGameMode)
	{
		Singleton<PlayerStateMsgHud>.Instance.DisplayClickToRespawnMsg();
		if (Input.GetMouseButtonDown(0))
		{
			fpsGameMode.RespawnPlayer();
		}
	}

	public void ShowRespawnFrozenTimeText(int spawnFrozenTime)
	{
		Singleton<PlayerStateMsgHud>.Instance.DisplayRespawnTimeMsg(spawnFrozenTime);
	}

	public void ShowTimeOutText(FpsGameMode fpsGameMode, int timeout)
	{
		Singleton<PlayerStateMsgHud>.Instance.DisplayDisconnectionTimeoutMsg(timeout);
	}

	public void SetTeamScore(int blueScore, int redScore)
	{
		Singleton<MatchStatusHud>.Instance.BlueTeamScore = blueScore;
		Singleton<MatchStatusHud>.Instance.RedTeamScore = redScore;
		TabScreenPanelGUI.Instance.SetTeamSplats(blueScore, redScore);
	}

	public void AddInGameEvent(string subjective, string objective, UberstrikeItemClass weaponClass, InGameEventFeedbackType eventType, TeamID sourceTeam, TeamID destinationTeam)
	{
		Singleton<EventStreamHud>.Instance.AddEventText(subjective, sourceTeam, GetEventTypeMessage(eventType), objective, destinationTeam);
	}

	public void AddInGameEvent(string sourcePlayer, string message)
	{
		AddInGameEvent(sourcePlayer, message, UberstrikeItemClass.FunctionalGeneral, InGameEventFeedbackType.CustomMessage, TeamID.NONE, TeamID.NONE);
	}

	private void OnRespawnButtonClicked()
	{
		GamePageManager.Instance.UnloadCurrentPage();
		GameState.LocalPlayer.UnPausePlayer();
		GameState.CurrentGame.RespawnPlayer();
	}

	private void OnContinueButtonClicked()
	{
		GamePageManager.Instance.UnloadCurrentPage();
		GameState.LocalPlayer.UnPausePlayer();
	}

	private void CleanAllTemporaryHud()
	{
		Singleton<InGameChatHud>.Instance.ClearAll();
		ClearAllFeedbackHud();
	}

	private string GetEventTypeMessage(InGameEventFeedbackType eventType)
	{
		switch (eventType)
		{
		case InGameEventFeedbackType.HeadShot:
			return "headshot";
		case InGameEventFeedbackType.Humiliation:
			return "smacked";
		case InGameEventFeedbackType.NutShot:
			return "nutshot";
		case InGameEventFeedbackType.None:
			return "killed";
		default:
			return string.Empty;
		}
	}

	private bool IsScreenResolutionChanged()
	{
		if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
		{
			_lastScreenWidth = Screen.width;
			_lastScreenHeight = Screen.height;
			return true;
		}
		return false;
	}
}
