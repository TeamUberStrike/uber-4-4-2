using UberStrike.Core.Models.Views;
using UnityEngine;

public class WeaponsHud : Singleton<WeaponsHud>
{
	public QuickItemGroupHud QuickItems { get; private set; }

	public WeaponSelectorHud Weapons { get; private set; }

	private WeaponsHud()
	{
		Weapons = new WeaponSelectorHud();
		QuickItems = new QuickItemGroupHud();
		SetEnabled(false);
	}

	public void SetEnabled(bool enabled)
	{
		Weapons.Enabled = enabled;
		QuickItems.Enabled = enabled;
	}

	public void Draw()
	{
		Weapons.Draw();
		QuickItems.Draw();
	}

	public void Update()
	{
		Weapons.Update();
	}

	public void SetActiveLoadout(LoadoutSlotType loadoutSlotType)
	{
		switch (loadoutSlotType)
		{
		case LoadoutSlotType.WeaponMelee:
		case LoadoutSlotType.WeaponPrimary:
		case LoadoutSlotType.WeaponSecondary:
		case LoadoutSlotType.WeaponTertiary:
		case LoadoutSlotType.WeaponPickup:
			SetActiveWeaponLoadout(loadoutSlotType);
			break;
		default:
			Debug.LogError("You passed in an invalid LoadoutSlotType!");
			break;
		}
	}

	public void ResetActiveWeapon()
	{
		SetActiveLoadout(LoadoutSlotType.WeaponPrimary);
	}

	public void SetQuickItemCurrentAmount(int slot, int amount)
	{
		QuickItemHud loadoutQuickItemHud = QuickItems.GetLoadoutQuickItemHud(slot);
		if (loadoutQuickItemHud != null)
		{
			loadoutQuickItemHud.Amount = amount;
		}
	}

	public void SetQuickItemCooldown(int slot, float cooldown)
	{
		QuickItemHud loadoutQuickItemHud = QuickItems.GetLoadoutQuickItemHud(slot);
		if (loadoutQuickItemHud != null)
		{
			loadoutQuickItemHud.Cooldown = cooldown;
		}
	}

	public void SetQuickItemCooldownMax(int slot, float cooldownMax)
	{
		QuickItemHud loadoutQuickItemHud = QuickItems.GetLoadoutQuickItemHud(slot);
		if (loadoutQuickItemHud != null)
		{
			loadoutQuickItemHud.CooldownMax = cooldownMax;
		}
	}

	public void SetQuickItemRecharging(int slot, float recharging)
	{
		QuickItemHud loadoutQuickItemHud = QuickItems.GetLoadoutQuickItemHud(slot);
		if (loadoutQuickItemHud != null)
		{
			loadoutQuickItemHud.Recharging = recharging;
		}
	}

	public void SetQuickItemRechargingMax(int slot, float rechargingMax)
	{
		QuickItemHud loadoutQuickItemHud = QuickItems.GetLoadoutQuickItemHud(slot);
		if (loadoutQuickItemHud != null)
		{
			loadoutQuickItemHud.RechargingMax = rechargingMax;
		}
	}

	protected override void OnDispose()
	{
	}

	private void SetActiveWeaponLoadout(LoadoutSlotType loadoutSlotType)
	{
		if (HudAssets.Exists)
		{
			IUnityItem loadoutWeapon = Weapons.GetLoadoutWeapon(loadoutSlotType);
			if (loadoutWeapon != null)
			{
				Weapons.SetActiveWeaponLoadout(loadoutSlotType);
				Singleton<ReticleHud>.Instance.ConfigureReticle(loadoutWeapon.View as UberStrikeItemWeaponView);
			}
		}
	}
}
