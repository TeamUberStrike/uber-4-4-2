using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CustomWeaponPickupItem : PickupItem
{
	private string _weaponName = string.Empty;

	[SerializeField]
	private int _weaponId;

	private void Start()
	{
		IUnityItem itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(_weaponId);
		if (itemInShop != null)
		{
			CreateWeaponDecorator(itemInShop);
			itemInShop.OnPrefabLoaded += CreateWeaponDecorator;
		}
	}

	private void OnDestroy()
	{
		IUnityItem itemInShop = Singleton<ItemManager>.Instance.GetItemInShop(_weaponId);
		if (itemInShop != null)
		{
			itemInShop.OnPrefabLoaded -= CreateWeaponDecorator;
		}
	}

	private void CreateWeaponDecorator(IUnityItem item)
	{
		for (int i = 0; i < _pickupItem.childCount; i++)
		{
			Object.Destroy(_pickupItem.GetChild(i).gameObject);
		}
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
		_weaponName = item.Name;
	}

	protected override bool OnPlayerPickup()
	{
		if (ApplicationDataManager.ApplicationOptions.GameplayAutoPickupEnabled)
		{
			Singleton<WeaponController>.Instance.SetPickupWeapon(_weaponId);
			if (GameState.HasCurrentGame)
			{
				GameState.CurrentGame.PickupPowerup(base.PickupID, PickupItemType.Weapon, 0);
				Singleton<PickupNameHud>.Instance.DisplayPickupName(_weaponName, PickUpMessageType.ChangeWeapon);
				Singleton<WeaponController>.Instance.ResetPickupWeaponSlotInSeconds(0);
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
