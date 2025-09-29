using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class QuickItemEventListener : Singleton<QuickItemEventListener>
{
	private QuickItemEventListener()
	{
		CmuneEventHandler.AddListener<QuickItemAmountChangedEvent>(delegate
		{
			Singleton<QuickItemController>.Instance.UpdateQuickSlotAmount();
		});
		CmuneEventHandler.AddListener<HealthIncreaseEvent>(OnHealthIncrease);
		CmuneEventHandler.AddListener<ArmorIncreaseEvent>(OnArmorIncrease);
		CmuneEventHandler.AddListener<AddAmmoIncreaseEvent>(OnAmmoIncrease);
		CmuneEventHandler.AddListener<AmmoAddStartEvent>(OnAddStartAmmo);
		CmuneEventHandler.AddListener<AmmoAddMaxEvent>(OnAddMaxAmmo);
	}

	public void Initialize()
	{
	}

	private void OnHealthIncrease(HealthIncreaseEvent ev)
	{
		if (GameState.LocalCharacter != null && GameState.CurrentGame != null)
		{
			GameState.CurrentGame.IncreaseHealthAndArmor(ev.Health, 0);
			Singleton<HpApHud>.Instance.HP = Mathf.Clamp(GameState.LocalCharacter.Health + ev.Health, 0, 200);
		}
	}

	private void OnArmorIncrease(ArmorIncreaseEvent ev)
	{
		GameState.CurrentGame.IncreaseHealthAndArmor(0, ev.Armor);
		Singleton<HpApHud>.Instance.AP = Mathf.Clamp(GameState.LocalCharacter.Armor.ArmorPoints + ev.Armor, 0, 200);
	}

	private void OnAmmoIncrease(AddAmmoIncreaseEvent ev)
	{
		foreach (int value in Enum.GetValues(typeof(AmmoType)))
		{
			AmmoDepot.AddAmmoOfType((AmmoType)value, ev.Amount);
		}
		Singleton<WeaponController>.Instance.UpdateAmmoHUD();
	}

	private void OnAddMaxAmmo(AmmoAddMaxEvent ev)
	{
		foreach (int value in Enum.GetValues(typeof(AmmoType)))
		{
			AmmoDepot.AddMaxAmmoOfType((AmmoType)value, (float)ev.Percent / 100f);
		}
		Singleton<WeaponController>.Instance.UpdateAmmoHUD();
	}

	private void OnAddStartAmmo(AmmoAddStartEvent ev)
	{
		foreach (int value in Enum.GetValues(typeof(AmmoType)))
		{
			AmmoDepot.AddStartAmmoOfType((AmmoType)value, (float)ev.Percent / 100f);
		}
		Singleton<WeaponController>.Instance.UpdateAmmoHUD();
	}
}
