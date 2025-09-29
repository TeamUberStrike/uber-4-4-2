using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGearLoadout
{
	event Action OnGearUpdated;

	bool Contains(string gearPrefabName);

	Dictionary<LoadoutSlotType, GameObject> GetPrefabs();
}
