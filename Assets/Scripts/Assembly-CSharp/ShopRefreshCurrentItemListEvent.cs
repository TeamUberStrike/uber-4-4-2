using UberStrike.Core.Types;

public class ShopRefreshCurrentItemListEvent
{
	public bool UseCurrentSelection { get; private set; }

	public UberstrikeItemClass ItemClass { get; private set; }

	public UberstrikeItemType ItemType { get; private set; }

	public ShopRefreshCurrentItemListEvent()
	{
		UseCurrentSelection = true;
	}

	public ShopRefreshCurrentItemListEvent(UberstrikeItemClass itemClass, UberstrikeItemType itemType)
	{
		UseCurrentSelection = false;
		ItemClass = itemClass;
		ItemType = itemType;
	}
}
