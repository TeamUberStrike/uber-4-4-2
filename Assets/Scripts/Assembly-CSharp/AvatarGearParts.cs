using System.Collections.Generic;
using UnityEngine;

public class AvatarGearParts
{
	public GameObject Base { get; set; }

	public List<GameObject> Parts { get; private set; }

	public AvatarGearParts()
	{
		Parts = new List<GameObject>();
	}
}
