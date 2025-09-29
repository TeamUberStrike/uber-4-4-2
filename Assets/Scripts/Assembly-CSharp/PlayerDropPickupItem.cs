using System.Collections;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PlayerDropPickupItem : PickupItem
{
	public int WeaponItemId;

	private UberstrikeItemClass _weaponType = UberstrikeItemClass.WeaponMelee;

	private float _timeout;

	private IEnumerator Start()
	{
		if (WeaponItemId != 0)
		{
			IUnityItem item = Singleton<ItemManager>.Instance.GetItemInShop(WeaponItemId);
			if (item != null)
			{
				base.IsAvailable = false;
				_weaponType = item.View.ItemClass;
				base.gameObject.tag = "DynamicProp";
				GameObject obj = item.Create(base.transform.position, base.transform.rotation);
				if ((bool)obj)
				{
					BaseWeaponDecorator decorator = obj.GetComponent<BaseWeaponDecorator>();
					decorator.transform.parent = _pickupItem;
					decorator.transform.localPosition = new Vector3(0f, 0f, -0.3f);
					decorator.transform.localRotation = Quaternion.identity;
					decorator.transform.localScale = Vector3.one;
					LayerUtil.SetLayerRecursively(decorator.transform, UberstrikeLayer.Props);
					_renderers = decorator.GetComponentsInChildren<MeshRenderer>(true);
					MeshRenderer[] renderers = _renderers;
					foreach (Renderer r in renderers)
					{
						r.enabled = false;
					}
					yield return new WaitForSeconds(0.5f);
					SetItemAvailable(true);
					BaseWeaponEffect[] effects = decorator.GetComponentsInChildren<BaseWeaponEffect>();
					BaseWeaponEffect[] array = effects;
					foreach (BaseWeaponEffect ef in array)
					{
						ef.Hide();
					}
				}
			}
		}
		else
		{
			Debug.LogError("No Pickup Weapon assigned: " + WeaponItemId);
		}
		Vector3 oldpos = base.transform.position;
		Vector3 newpos = oldpos;
		RaycastHit hit;
		if (Physics.Raycast(oldpos + Vector3.up, Vector3.down, out hit, 100f, UberstrikeLayerMasks.ProtectionMask) && oldpos.y > hit.point.y + 1f)
		{
			newpos = hit.point + Vector3.up;
		}
		_timeout = Time.time + 10f;
		float time = 0f;
		while (_timeout > Time.time)
		{
			yield return new WaitForEndOfFrame();
			time += Time.deltaTime;
			base.transform.position = Vector3.Lerp(oldpos, newpos, time);
		}
		base.IsAvailable = false;
		yield return new WaitForSeconds(0.2f);
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if ((bool)_pickupItem)
		{
			_pickupItem.Rotate(Vector3.up, 150f * Time.deltaTime, Space.Self);
		}
	}

	protected override bool OnPlayerPickup()
	{
		bool flag = false;
		if (ApplicationDataManager.ApplicationOptions.GameplayAutoPickupEnabled)
		{
			flag = Singleton<WeaponController>.Instance.HasWeaponOfClass(_weaponType);
			Singleton<WeaponController>.Instance.SetPickupWeapon(WeaponItemId, false, false);
			if (Singleton<WeaponController>.Instance.CheckPlayerWeaponInPickupSlot(WeaponItemId))
			{
				Singleton<WeaponController>.Instance.ResetPickupWeaponSlotInSeconds(10);
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
					PlayLocalPickupSound(GameAudio.WeaponPickup2D);
					StartCoroutine(StartHidingPickupForSeconds(0));
				}
			}
			return true;
		}
		return false;
	}

	protected override void OnRemotePickup()
	{
		PlayRemotePickupSound(GameAudio.WeaponPickup, base.transform.position);
	}
}
