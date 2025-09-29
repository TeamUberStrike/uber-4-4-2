using System.Linq;
using UberStrike.Core.Models.Views;

public static class WeaponConfigurationHelper
{
	public static float MaxAmmo { get; private set; }

	public static float MaxDamage { get; private set; }

	public static float MaxAccuracySpread { get; private set; }

	public static float MaxProjectileSpeed { get; private set; }

	public static float MaxRateOfFire { get; private set; }

	public static float MaxRecoilKickback { get; private set; }

	public static float MaxSplashRadius { get; private set; }

	static WeaponConfigurationHelper()
	{
		MaxSplashRadius = 1f;
		MaxRecoilKickback = 1f;
		MaxRateOfFire = 1f;
		MaxProjectileSpeed = 1f;
		MaxAccuracySpread = 1f;
		MaxDamage = 1f;
		MaxAmmo = 1f;
	}

	public static void UpdateWeaponStatistics(UberStrikeItemShopClientView shopView)
	{
		if (shopView != null && shopView.WeaponItems.Count > 0)
		{
			MaxAmmo = shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.MaxAmmo).First().MaxAmmo;
			MaxSplashRadius = (float)shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.SplashRadius).First().SplashRadius / 100f;
			MaxRecoilKickback = shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.RecoilKickback).First().RecoilKickback;
			MaxRateOfFire = (float)shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.RateOfFire).First().RateOfFire / 1000f;
			MaxProjectileSpeed = shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.ProjectileSpeed).First().ProjectileSpeed;
			MaxAccuracySpread = shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.AccuracySpread).First().AccuracySpread / 10;
			MaxDamage = shopView.WeaponItems.OrderByDescending((UberStrikeItemWeaponView item) => item.DamagePerProjectile).First().DamagePerProjectile;
		}
	}

	public static float GetAmmoNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.MaxAmmo / MaxAmmo);
	}

	public static float GetDamageNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)(view.DamagePerProjectile * view.ProjectilesPerShot) / MaxDamage);
	}

	public static float GetAccuracySpreadNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.AccuracySpread / 10f / MaxAccuracySpread);
	}

	public static float GetProjectileSpeedNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.ProjectileSpeed / MaxProjectileSpeed);
	}

	public static float GetRateOfFireNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.RateOfFire / 1000f / MaxRateOfFire);
	}

	public static float GetRecoilKickbackNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.RecoilKickback / MaxRecoilKickback);
	}

	public static float GetSplashRadiusNormalized(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.SplashRadius / 100f / MaxSplashRadius);
	}

	public static float GetAmmo(UberStrikeItemWeaponView view)
	{
		return (view != null) ? view.MaxAmmo : 0;
	}

	public static float GetDamage(UberStrikeItemWeaponView view)
	{
		return (view != null) ? (view.DamagePerProjectile * view.ProjectilesPerShot) : 0;
	}

	public static float GetAccuracySpread(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.AccuracySpread / 10f);
	}

	public static float GetProjectileSpeed(UberStrikeItemWeaponView view)
	{
		return (view != null) ? view.ProjectileSpeed : 0;
	}

	public static float GetRateOfFire(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 1f : ((float)view.RateOfFire / 1000f);
	}

	public static float GetRecoilKickback(UberStrikeItemWeaponView view)
	{
		return (view != null) ? view.RecoilKickback : 0;
	}

	public static float GetRecoilMovement(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.RecoilMovement / 100f);
	}

	public static float GetSplashRadius(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.SplashRadius / 100f);
	}

	public static float GetCriticalStrikeBonus(UberStrikeItemWeaponView view)
	{
		return (view == null) ? 0f : ((float)view.CriticalStrikeBonus / 100f);
	}
}
