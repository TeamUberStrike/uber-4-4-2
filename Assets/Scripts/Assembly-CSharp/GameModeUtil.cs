using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal static class GameModeUtil
{
	private const int DisconnectionTimeout = 120;

	private const int DisconnectionTimeoutAdmin = 1200;

	public static void OnEnterGameMode(FpsGameMode gameMode)
	{
		GameState.CurrentGame = gameMode;
		TabScreenPanelGUI.Instance.SetGameName(gameMode.GameData.RoomName);
		TabScreenPanelGUI.Instance.SetServerName(Singleton<GameServerManager>.Instance.GetServerName(gameMode.GameData.ServerConnection));
		LevelCamera.Instance.SetLevelCamera(GameState.CurrentSpace.Camera, GameState.CurrentSpace.DefaultViewPoint.position, GameState.CurrentSpace.DefaultViewPoint.rotation);
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		GameState.LocalPlayer.SetEnabled(true);
		ProjectileManager.CreateContainer();
	}

	public static void OnExitGameMode()
	{
		GameConnectionManager.Stop();
		LevelCamera.Instance.ReleaseCamera();
		Singleton<WeaponController>.Instance.StopInputHandler();
		Singleton<ProjectileManager>.Instance.ClearAll();
		ProjectileManager.DestroyContainer();
		GameState.CurrentGame.UnloadAllPlayers();
		GameState.CurrentGame = null;
		GameState.LocalPlayer.SetEnabled(false);
		Singleton<HudUtil>.Instance.ClearAllHud();
		GameState.LocalPlayer.SetPlayerControlState(LocalPlayer.PlayerState.None);
		if ((bool)GameState.LocalAvatar.Decorator)
		{
			if (!GameState.LocalAvatar.Decorator.gameObject.activeSelf)
			{
				GameState.LocalAvatar.EnableDecorator();
			}
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = true;
			GameState.LocalAvatar.Decorator.HudInformation.enabled = true;
		}
	}

	public static void OnPlayerDamage(OnPlayerDamageEvent ev)
	{
		Singleton<DamageFeedbackHud>.Instance.AddDamageMark(Mathf.Clamp01(ev.DamageValue / 50f), ev.Angle);
		if (GameState.LocalCharacter.Armor.ArmorPoints > 0)
		{
			SfxManager.Play2dAudioClip(GameAudio.LocalPlayerHitArmorRemaining);
		}
		else
		{
			SfxManager.Play2dAudioClip(GameAudio.LocalPlayerHitNoArmor);
		}
	}

	public static void OnPlayerKillEnemy(OnPlayerKillEnemyEvent ev)
	{
		ApplicationDataManager.EventsSystem.SendPlayerKill(PlayerDataManager.CmidSecure, AutoMonoBehaviour<GameConnectionManager>.Instance.GameID, AutoMonoBehaviour<GameConnectionManager>.Instance.MatchID, ev.EmemyInfo.Cmid, ev.EmemyInfo.CurrentWeaponID);
		InGameEventFeedbackType inGameEventFeedbackType = InGameEventFeedbackType.None;
		if (ev.WeaponCategory == UberstrikeItemClass.WeaponMelee)
		{
			inGameEventFeedbackType = InGameEventFeedbackType.Humiliation;
			Singleton<LocalShotFeedbackHud>.Instance.DisplayLocalShotFeedback(inGameEventFeedbackType);
			SfxManager.Play2dAudioClip(GameAudio.KilledBySplatbat);
		}
		else if (ev.BodyHitPart == BodyPart.Head)
		{
			inGameEventFeedbackType = InGameEventFeedbackType.HeadShot;
			Singleton<LocalShotFeedbackHud>.Instance.DisplayLocalShotFeedback(inGameEventFeedbackType);
			SfxManager.Play2dAudioClip(GameAudio.GotHeadshotKill);
		}
		else if (ev.BodyHitPart == BodyPart.Nuts)
		{
			inGameEventFeedbackType = InGameEventFeedbackType.NutShot;
			Singleton<LocalShotFeedbackHud>.Instance.DisplayLocalShotFeedback(inGameEventFeedbackType);
			SfxManager.Play2dAudioClip(GameAudio.GotNutshotKill);
		}
		else
		{
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.YouKilledN, ev.EmemyInfo.PlayerName));
		}
		Singleton<HudUtil>.Instance.AddInGameEvent(GameState.LocalCharacter.PlayerName, ev.EmemyInfo.PlayerName, ev.WeaponCategory, inGameEventFeedbackType, GameState.LocalCharacter.TeamID, ev.EmemyInfo.TeamID);
	}

	public static void OnPlayerKilled(OnPlayerKilledEvent ev)
	{
		ApplicationDataManager.EventsSystem.SendPlayerDied(PlayerDataManager.CmidSecure, AutoMonoBehaviour<GameConnectionManager>.Instance.GameID, AutoMonoBehaviour<GameConnectionManager>.Instance.MatchID, ev.ShooterInfo.Cmid, ev.ShooterInfo.CurrentWeaponID);
		InGameEventFeedbackType eventType = InGameEventFeedbackType.None;
		if (ev.WeaponCategory == UberstrikeItemClass.WeaponMelee)
		{
			eventType = InGameEventFeedbackType.Humiliation;
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.SmackdownFromN, ev.ShooterInfo.PlayerName));
			if (LevelCamera.Instance.IsZoomedIn)
			{
				LevelCamera.Instance.DoZoomOut(60f, 10f);
			}
		}
		else if (ev.BodyHitPart == BodyPart.Head)
		{
			eventType = InGameEventFeedbackType.HeadShot;
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.HeadshotFromN, ev.ShooterInfo.PlayerName), 6f);
		}
		else if (ev.BodyHitPart == BodyPart.Nuts)
		{
			eventType = InGameEventFeedbackType.NutShot;
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.NutshotFromN, ev.ShooterInfo.PlayerName), 6f);
		}
		else
		{
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, string.Format(LocalizedStrings.KilledByN, ev.ShooterInfo.PlayerName), 6f);
		}
		Singleton<HudUtil>.Instance.AddInGameEvent(ev.ShooterInfo.PlayerName, GameState.LocalCharacter.PlayerName, ev.WeaponCategory, eventType, ev.ShooterInfo.TeamID, GameState.LocalCharacter.TeamID);
	}

	public static void OnPlayerSuicide(OnPlayerSuicideEvent ev)
	{
		ApplicationDataManager.EventsSystem.SendPlayerSuicide(PlayerDataManager.CmidSecure, AutoMonoBehaviour<GameConnectionManager>.Instance.GameID, AutoMonoBehaviour<GameConnectionManager>.Instance.MatchID);
		if (ev.PlayerInfo.ActorId == GameState.CurrentPlayerID)
		{
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, LocalizedStrings.CongratulationsYouKilledYourself);
		}
		Singleton<HudUtil>.Instance.AddInGameEvent(ev.PlayerInfo.PlayerName, LocalizedStrings.NKilledThemself, UberstrikeItemClass.FunctionalGeneral, InGameEventFeedbackType.CustomMessage, ev.PlayerInfo.TeamID, TeamID.NONE);
	}

	public static void UpdatePlayerStateMsg(FpsGameMode gameMode, bool checkTimeout)
	{
		if (gameMode.IsWaitingForSpawn)
		{
			UpdateWaitingForSpawnMsg(gameMode, checkTimeout);
		}
		else if (gameMode.IsWaitingForPlayers)
		{
			Singleton<PlayerStateMsgHud>.Instance.DisplayWaitingForOtherPlayerMsg();
		}
		else
		{
			Singleton<PlayerStateMsgHud>.Instance.DisplayNone();
		}
	}

	public static void UpdateWaitingForSpawnMsg(FpsGameMode gameMode, bool checkTimeout)
	{
		int num = Mathf.CeilToInt(gameMode.NextSpawnTime - Time.time);
		if (num > 0)
		{
			Singleton<HudUtil>.Instance.ShowRespawnFrozenTimeText(num);
			return;
		}
		int num2 = Mathf.CeilToInt(gameMode.NextSpawnTime - Time.time + (float)((PlayerDataManager.AccessLevel != MemberAccessLevel.Default) ? 1200 : 120));
		if (!checkTimeout || num2 > 0)
		{
			Singleton<HudUtil>.Instance.ShowRespawnButton();
			if (checkTimeout && num2 < 10)
			{
				Singleton<HudUtil>.Instance.ShowTimeOutText(gameMode, num2);
			}
		}
		else if (checkTimeout && !gameMode.IsGameClosed)
		{
			gameMode.IsGameClosed = true;
			GameState.LocalAvatar.EnableDecorator();
			Singleton<GameStateController>.Instance.LeaveGame();
			PopupSystem.ShowMessage("Wake up!", "It looks like you were asleep.The server disconnected you because you were idle for more than one minute while waiting to respawn.");
		}
	}
}
