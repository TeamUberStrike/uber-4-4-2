using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal static class TeamGameModeUtil
{
	public static void OnChangeTeamSuccess(OnChangeTeamSuccessEvent ev)
	{
		switch (ev.CurrentTeamID)
		{
		case TeamID.BLUE:
			Singleton<TeamChangeWarningHud>.Instance.DisplayWarningMsg(LocalizedStrings.ChangingToRedTeam, ColorScheme.HudTeamRed);
			Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.RED);
			break;
		case TeamID.RED:
			Singleton<TeamChangeWarningHud>.Instance.DisplayWarningMsg(LocalizedStrings.ChangingToBlueTeam, ColorScheme.HudTeamBlue);
			Singleton<HudUtil>.Instance.SetPlayerTeam(TeamID.BLUE);
			break;
		}
	}

	public static void OnChangeTeamFail(OnChangeTeamFailEvent ev)
	{
		switch (ev.Reason)
		{
		case OnChangeTeamFailEvent.FailReason.CannotChangeToATeamWithEqual:
			Singleton<TeamChangeWarningHud>.Instance.DisplayWarningMsg(LocalizedStrings.YouCannotChangeToATeamWithEqual, Color.white);
			break;
		case OnChangeTeamFailEvent.FailReason.OnlyOneTeamChangePerLife:
			Singleton<TeamChangeWarningHud>.Instance.DisplayWarningMsg(LocalizedStrings.OnlyOneTeamChangePerLife, Color.white);
			break;
		}
	}

	public static void OnPlayerChangeTeam(TeamDeathMatchGameMode gameMode, int playerId, UberStrike.Realtime.UnitySdk.CharacterInfo playerInfo, TeamID targetTeamID)
	{
		if (playerInfo != null && gameMode != null)
		{
			string arg = ((targetTeamID != TeamID.BLUE) ? LocalizedStrings.Red : LocalizedStrings.Blue);
			Singleton<EventStreamHud>.Instance.AddEventText(playerInfo.PlayerName, playerInfo.TeamID, string.Format(LocalizedStrings.ChangingToTeamN, arg), string.Empty);
			if (playerInfo.ActorId == GameState.CurrentPlayerID)
			{
				Singleton<ReticleHud>.Instance.FocusCharacter(TeamID.NONE);
				gameMode.ChangeAllPlayerOutline(GameState.LocalCharacter.TeamID);
			}
			else
			{
				gameMode.ChangePlayerOutlineById(playerId);
			}
		}
	}

	public static void DetectTeamChange(TeamDeathMatchGameMode gameMode)
	{
		if ((!KeyInput.AltPressed || !KeyInput.CtrlPressed) && gameMode.IsGameStarted && !Singleton<InGameChatHud>.Instance.CanInput && !ChatPageGUI.IsChatActive && !PanelManager.Instance.IsPanelOpen(PanelType.Options) && AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.ChangeTeam) > 0f && GUITools.SaveClickIn(1f))
		{
			GUITools.Clicked();
			gameMode.ChangeTeam();
		}
	}
}
