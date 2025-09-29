using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DamageInfo
{
	public short Damage { get; set; }

	public Vector3 Force { get; set; }

	public Vector3 Hitpoint { get; set; }

	public BodyPart BodyPart { get; set; }

	public int ProjectileID { get; set; }

	public int ShotCount { get; set; }

	public int WeaponID { get; set; }

	public UberstrikeItemClass WeaponClass { get; set; }

	public float CriticalStrikeBonus { get; set; }

	public DamageEffectType DamageEffectFlag { get; set; }

	public float DamageEffectValue { get; set; }

	public DamageInfo(short damage)
	{
		Damage = damage;
		Force = Vector3.zero;
	}
}
