using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class AmmoPickupItem : PickupItem
{
	[SerializeField]
	private AmmoType _ammoType;

	protected override bool CanPlayerPickup
	{
		get
		{
			return AmmoDepot.CanAddAmmo(_ammoType);
		}
	}

	protected override bool OnPlayerPickup()
	{
		bool flag = false;
		flag = AmmoDepot.CanAddAmmo(_ammoType);
		if (flag)
		{
			AmmoDepot.AddDefaultAmmoOfType(_ammoType);
			Singleton<WeaponController>.Instance.UpdateAmmoHUD();
			switch (_ammoType)
			{
			case AmmoType.Cannon:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Cannon Rockets", PickUpMessageType.AmmoCannon);
				break;
			case AmmoType.Handgun:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Handgun Rounds", PickUpMessageType.AmmoHandgun);
				break;
			case AmmoType.Launcher:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Launcher Grenades", PickUpMessageType.AmmoLauncher);
				break;
			case AmmoType.Machinegun:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Machinegun Ammo", PickUpMessageType.AmmoMachinegun);
				break;
			case AmmoType.Shotgun:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Shotgun Shells", PickUpMessageType.AmmoShotgun);
				break;
			case AmmoType.Snipergun:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Sniper Bullets", PickUpMessageType.AmmoSnipergun);
				break;
			case AmmoType.Splattergun:
				Singleton<PickupNameHud>.Instance.DisplayPickupName("Splattergun Cells", PickUpMessageType.AmmoSplattergun);
				break;
			}
			PlayLocalPickupSound(GameAudio.AmmoPickup2D);
			if (GameState.HasCurrentGame)
			{
				GameState.CurrentGame.PickupPowerup(base.PickupID, PickupItemType.Ammo, 0);
				if (GameState.IsSinglePlayer)
				{
					StartCoroutine(StartHidingPickupForSeconds(_respawnTime));
				}
			}
		}
		return flag;
	}

	protected override void OnRemotePickup()
	{
		PlayRemotePickupSound(GameAudio.AmmoPickup, base.transform.position);
	}
}
