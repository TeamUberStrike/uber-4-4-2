public class DoorOpenedEvent
{
	private int _doorID;

	public int DoorID
	{
		get
		{
			return _doorID;
		}
		protected set
		{
			_doorID = value;
		}
	}

	public DoorOpenedEvent(int doorID)
	{
		DoorID = doorID;
	}
}
