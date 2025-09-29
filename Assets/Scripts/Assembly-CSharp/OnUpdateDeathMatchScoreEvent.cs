public class OnUpdateDeathMatchScoreEvent
{
	public int MyScore { get; set; }

	public int OtherPlayerScore { get; set; }

	public bool IsLeading { get; set; }
}
