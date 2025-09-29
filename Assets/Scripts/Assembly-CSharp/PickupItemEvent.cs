public class PickupItemEvent
{
	public int PickupID { get; private set; }

	public bool ShowItem { get; private set; }

	public PickupItemEvent(int pickupID, bool showItem)
	{
		ShowItem = showItem;
		PickupID = pickupID;
	}
}
