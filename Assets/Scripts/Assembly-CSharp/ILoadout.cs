using System;

public interface ILoadout
{
	event Action OnGearChanged;

	event Action<LoadoutSlotType> OnWeaponChanged;

	bool Contains(string prefabName);

	AvatarGearParts GetAvatarGear();

	AvatarGearParts GetRagdollGear();

	bool TryGetItem(LoadoutSlotType slot, out IUnityItem item);

	void SetSlot(LoadoutSlotType slotType, IUnityItem item);

	void ClearSlot(LoadoutSlotType slot);

	void ClearAllSlots();
}
