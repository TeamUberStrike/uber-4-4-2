using UberStrike.Realtime.UnitySdk;

[NetworkClass(101)]
public class DeathMatchGameMode : FpsGameMode
{
	public DeathMatchGameMode(GameMetaData gameData)
		: base(GameConnectionManager.Rmi, gameData)
	{
		Singleton<PlayerLeadStatus>.Instance.ResetPlayerLead();
	}

	[NetworkMethod(79)]
	protected void OnUpdateSplatCount(short myKills, short otherKills, bool isLeading)
	{
		GameState.LocalCharacter.Kills = myKills;
		OnUpdateDeathMatchScoreEvent onUpdateDeathMatchScoreEvent = new OnUpdateDeathMatchScoreEvent();
		onUpdateDeathMatchScoreEvent.MyScore = myKills;
		onUpdateDeathMatchScoreEvent.OtherPlayerScore = otherKills;
		onUpdateDeathMatchScoreEvent.IsLeading = isLeading;
		CmuneEventHandler.Route(onUpdateDeathMatchScoreEvent);
	}

	protected override void OnEndOfMatch()
	{
		base.IsWaitingForSpawn = false;
		base.IsMatchRunning = false;
		_stateInterpolator.Pause();
		GameState.LocalPlayer.Pause();
		GameState.IsReadyForNextGame = false;
		HideRemotePlayerHudFeedback();
	}
}
