using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class TutorialArmoryPickup : PickupItem
{
	private string _weaponName = string.Empty;

	[SerializeField]
	private UberstrikeItemClass _weaponType;

	[SerializeField]
	private TutorialWaypoint _waypoint;

	private void Start()
	{
		IUnityItem item;
		if (Singleton<ItemManager>.Instance.TryGetDefaultItem(_weaponType, out item))
		{
			_weaponName = item.Name;
			GameObject gameObject = item.Create(base.transform.position, base.transform.rotation);
			if ((bool)gameObject)
			{
				BaseWeaponDecorator component = gameObject.GetComponent<BaseWeaponDecorator>();
				component.transform.parent = _pickupItem;
				component.transform.localPosition = new Vector3(0f, 0f, -0.3f);
				component.transform.localRotation = Quaternion.identity;
				component.transform.localScale = Vector3.one;
				LayerUtil.SetLayerRecursively(component.transform, UberstrikeLayer.GloballyLit);
				_renderers = component.GetComponentsInChildren<MeshRenderer>(true);
			}
		}
		else
		{
			Debug.LogError("No Default Weapon found for Class " + _weaponType);
		}
	}

	protected override bool OnPlayerPickup()
	{
		base.IsAvailable = false;
		bool result = false;
		bool flag = false;
		IUnityItem item;
		if (Singleton<ItemManager>.Instance.TryGetDefaultItem(_weaponType, out item))
		{
			result = true;
			flag = Singleton<WeaponController>.Instance.HasWeaponOfClass(_weaponType);
			Singleton<WeaponController>.Instance.SetPickupWeapon(item.View.ID);
			if ((bool)_pickupItem)
			{
				_pickupItem.gameObject.SetActive(false);
			}
			if (GameState.HasCurrentGame && GameState.CurrentGame is TutorialGameMode)
			{
				TutorialGameMode tutorialGameMode = GameState.CurrentGame as TutorialGameMode;
				tutorialGameMode.OnArmoryPickupMG();
				ParticleEffectController.ShowPickUpEffect(_pickupItem.position, 100);
			}
			if (GameState.HasCurrentGame)
			{
				GameState.CurrentGame.PickupPowerup(base.PickupID, PickupItemType.Weapon, 0);
				if (flag)
				{
					switch (_weaponType)
					{
					case UberstrikeItemClass.WeaponCannon:
						Singleton<PickupNameHud>.Instance.DisplayPickupName("Cannon Rockets", PickUpMessageType.AmmoCannon);
						break;
					case UberstrikeItemClass.WeaponLauncher:
						Singleton<PickupNameHud>.Instance.DisplayPickupName("Launcher Grenades", PickUpMessageType.AmmoLauncher);
						break;
					case UberstrikeItemClass.WeaponMachinegun:
						Singleton<PickupNameHud>.Instance.DisplayPickupName("Machinegun Ammo", PickUpMessageType.AmmoMachinegun);
						break;
					case UberstrikeItemClass.WeaponShotgun:
						Singleton<PickupNameHud>.Instance.DisplayPickupName("Shotgun Shells", PickUpMessageType.AmmoShotgun);
						break;
					case UberstrikeItemClass.WeaponSniperRifle:
						Singleton<PickupNameHud>.Instance.DisplayPickupName("Sniper Bullets", PickUpMessageType.AmmoSnipergun);
						break;
					case UberstrikeItemClass.WeaponSplattergun:
						Singleton<PickupNameHud>.Instance.DisplayPickupName("Splattergun Cells", PickUpMessageType.AmmoSplattergun);
						break;
					}
				}
				else
				{
					Singleton<PickupNameHud>.Instance.DisplayPickupName(_weaponName, PickUpMessageType.ChangeWeapon);
					Singleton<WeaponController>.Instance.ResetPickupWeaponSlotInSeconds(0);
				}
			}
		}
		else
		{
			Debug.LogError("Cannot get default item of type: " + _weaponType);
		}
		return result;
	}

	public void Show(bool show)
	{
		SetItemAvailable(show);
		if ((bool)_waypoint)
		{
			_waypoint.CanShow = show;
		}
		if (show && (bool)_pickupItem)
		{
			_pickupItem.gameObject.SetActive(true);
		}
	}
}
