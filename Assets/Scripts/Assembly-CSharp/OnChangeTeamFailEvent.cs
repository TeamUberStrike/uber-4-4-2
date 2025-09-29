public class OnChangeTeamFailEvent
{
	public enum FailReason
	{
		None = 0,
		CannotChangeToATeamWithEqual = 1,
		OnlyOneTeamChangePerLife = 2
	}

	public FailReason Reason { get; set; }
}
