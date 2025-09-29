using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class WeaponPickupItem : PickupItem
{
	private string _weaponName = string.Empty;

	[SerializeField]
	private UberstrikeItemClass _weaponType;

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
				LayerUtil.SetLayerRecursively(component.transform, UberstrikeLayer.Default);
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
		if (ApplicationDataManager.ApplicationOptions.GameplayAutoPickupEnabled)
		{
			bool flag = false;
			IUnityItem item;
			if (Singleton<ItemManager>.Instance.TryGetDefaultItem(_weaponType, out item))
			{
				flag = Singleton<WeaponController>.Instance.HasWeaponOfClass(_weaponType);
				Singleton<WeaponController>.Instance.SetPickupWeapon(item.View.ID);
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
					PlayLocalPickupSound(GameAudio.WeaponPickup2D);
					if (GameState.IsSinglePlayer)
					{
						StartCoroutine(StartHidingPickupForSeconds(_respawnTime));
					}
				}
				return true;
			}
			return false;
		}
		return false;
	}

	protected override void OnRemotePickup()
	{
		PlayRemotePickupSound(GameAudio.WeaponPickup, base.transform.position);
	}

	private void Update()
	{
		if ((bool)_pickupItem)
		{
			_pickupItem.Rotate(Vector3.up, 150f * Time.deltaTime, Space.Self);
		}
	}
}
