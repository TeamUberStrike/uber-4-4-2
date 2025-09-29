using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class HealthPickupItem : PickupItem
{
	public enum Category
	{
		HP_100 = 0,
		HP_50 = 1,
		HP_25 = 2,
		HP_5 = 3
	}

	[SerializeField]
	private Category _healthPoints;

	protected override bool CanPlayerPickup
	{
		get
		{
			return GameState.LocalCharacter.Health < 100;
		}
	}

	private AudioClip Get3DAudioClip()
	{
		switch (_healthPoints)
		{
		case Category.HP_100:
			return GameAudio.MegaHealth2D;
		case Category.HP_50:
			return GameAudio.BigHealth2D;
		case Category.HP_25:
			return GameAudio.MediumHealth2D;
		case Category.HP_5:
			return GameAudio.SmallHealth2D;
		default:
			return GameAudio.SmallHealth2D;
		}
	}

	private AudioClip Get2DAudioClip()
	{
		switch (_healthPoints)
		{
		case Category.HP_100:
			return GameAudio.MegaHealth;
		case Category.HP_50:
			return GameAudio.BigHealth;
		case Category.HP_25:
			return GameAudio.MediumHealth;
		case Category.HP_5:
			return GameAudio.SmallHealth;
		default:
			return GameAudio.SmallHealth;
		}
	}

	protected override bool OnPlayerPickup()
	{
		int num = 0;
		int num2 = 100;
		switch (_healthPoints)
		{
		case Category.HP_5:
			num = 5;
			num2 = 200;
			break;
		case Category.HP_25:
			num = 25;
			num2 = 100;
			break;
		case Category.HP_50:
			num = 50;
			num2 = 100;
			break;
		case Category.HP_100:
			num = 100;
			num2 = 200;
			break;
		default:
			num = 0;
			num2 = 100;
			break;
		}
		if (GameState.HasCurrentPlayer && GameState.HasCurrentGame && GameState.LocalCharacter.Health < num2)
		{
			Singleton<HpApHud>.Instance.HP = Mathf.Clamp(GameState.LocalCharacter.Health + num, 0, num2);
			GameState.CurrentGame.PickupPowerup(base.PickupID, PickupItemType.Health, num);
			switch (_healthPoints)
			{
			case Category.HP_5:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Mini Health", PickUpMessageType.Health5);
				break;
			case Category.HP_25:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Medium Health", PickUpMessageType.Health25);
				break;
			case Category.HP_50:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Big Health", PickUpMessageType.Health50);
				break;
			case Category.HP_100:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Uber Health", PickUpMessageType.Health100);
				break;
			}
			PlayLocalPickupSound(Get2DAudioClip());
			if (GameState.IsSinglePlayer)
			{
				StartCoroutine(StartHidingPickupForSeconds(_respawnTime));
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
