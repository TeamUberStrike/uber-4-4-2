using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class ArmorPickupItem : PickupItem
{
	public enum Category
	{
		Gold = 0,
		Silver = 1,
		Bronze = 2
	}

	[SerializeField]
	private Category _armorPoints;

	protected override bool CanPlayerPickup
	{
		get
		{
			return GameState.HasCurrentPlayer && GameState.LocalCharacter.Armor.ArmorPoints < 200;
		}
	}

	private AudioClip Get2DAudioClip()
	{
		switch (_armorPoints)
		{
		case Category.Gold:
			return GameAudio.GoldArmor2D;
		case Category.Silver:
			return GameAudio.SilverArmor2D;
		case Category.Bronze:
			return GameAudio.ArmorShard2D;
		default:
			return GameAudio.ArmorShard2D;
		}
	}

	private AudioClip Get3DAudioClip()
	{
		switch (_armorPoints)
		{
		case Category.Gold:
			return GameAudio.GoldArmor;
		case Category.Silver:
			return GameAudio.SilverArmor;
		case Category.Bronze:
			return GameAudio.ArmorShard;
		default:
			return GameAudio.ArmorShard;
		}
	}

	protected override bool OnPlayerPickup()
	{
		if (CanPlayerPickup)
		{
			int num = 0;
			switch (_armorPoints)
			{
			case Category.Gold:
				num = 100;
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Uber Armor", PickUpMessageType.Armor100);
				break;
			case Category.Silver:
				num = 50;
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Big Armor", PickUpMessageType.Armor50);
				break;
			case Category.Bronze:
				num = 5;
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Mini Armor", PickUpMessageType.Armor5);
				break;
			}
			Singleton<HpApHud>.Instance.AP = Mathf.Clamp(GameState.LocalCharacter.Armor.ArmorPoints + num, 0, 200);
			PlayLocalPickupSound(Get2DAudioClip());
			if (GameState.HasCurrentGame)
			{
				GameState.CurrentGame.PickupPowerup(base.PickupID, PickupItemType.Armor, num);
				if (GameState.IsSinglePlayer)
				{
					StartCoroutine(StartHidingPickupForSeconds(_respawnTime));
				}
			}
			return true;
		}
		return false;
	}

	protected override void OnRemotePickup()
	{
		PlayRemotePickupSound(Get3DAudioClip(), base.transform.position);
	}
}
