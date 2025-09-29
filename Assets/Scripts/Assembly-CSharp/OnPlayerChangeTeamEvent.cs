using UberStrike.Realtime.UnitySdk;

public class OnPlayerChangeTeamEvent
{
	public int PlayerID { get; set; }

	public CharacterInfo PlayerInfo { get; set; }

	public TeamID TargetTeamID { get; set; }
}
