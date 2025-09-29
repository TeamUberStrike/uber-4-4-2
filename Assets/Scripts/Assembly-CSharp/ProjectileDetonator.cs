using UberStrike.Core.Types;
using UnityEngine;

public class ProjectileDetonator
{
	public float Radius { get; private set; }

	public float Damage { get; private set; }

	public int Force { get; private set; }

	public Vector3 Direction { get; set; }

	public int WeaponID { get; private set; }

	public UberstrikeItemClass WeaponClass { get; private set; }

	public int ProjectileID { get; private set; }

	public DamageEffectType DamageEffectFlag { get; private set; }

	public float DamageEffectValue { get; private set; }

	public ProjectileDetonator(float radius, float damage, int force, Vector3 direction, int projectileId, int weaponId, UberstrikeItemClass weaponClass, DamageEffectType damageEffectFlag, float damageEffectValue)
	{
		Radius = radius;
		Damage = damage;
		Force = force;
		Direction = direction;
		ProjectileID = projectileId;
		WeaponID = weaponId;
		WeaponClass = weaponClass;
		DamageEffectFlag = damageEffectFlag;
		DamageEffectValue = damageEffectValue;
	}

	public void Explode(Vector3 position)
	{
		Explode(position, ProjectileID, Damage, Direction, Radius, Force, WeaponID, WeaponClass, DamageEffectFlag, DamageEffectValue);
	}

	public static void Explode(Vector3 position, int projectileId, float damage, Vector3 dir, float radius, int force, int weaponId, UberstrikeItemClass weaponClass, DamageEffectType damageEffectFlag = DamageEffectType.None, float damageEffectValue = 0f)
	{
		Collider[] array = Physics.OverlapSphere(position, radius, UberstrikeLayerMasks.ExplosionMask);
		int num = 1;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			BaseGameProp component = collider.transform.GetComponent<BaseGameProp>();
			RaycastHit hitInfo;
			if (component != null && component.RecieveProjectileDamage && (!Physics.Linecast(position, collider.bounds.center, out hitInfo, UberstrikeLayerMasks.ProtectionMask) || hitInfo.transform == component.transform || hitInfo.transform.GetComponent<BaseGameProp>() != null))
			{
				Vector3 hitpoint = collider.ClosestPointOnBounds(position);
				float num2 = 1f;
				Vector3 vector = collider.transform.position - position;
				if (vector.sqrMagnitude < 0.01f)
				{
					vector = dir;
				}
				else
				{
					if (radius > 1f)
					{
						float num3 = radius - Mathf.Clamp(vector.magnitude, 0f, radius);
						num2 = Mathf.Clamp(num3 / radius, 0f, 0.6f) + 0.4f;
					}
					vector = vector.normalized;
				}
				short num4 = (short)Mathf.CeilToInt(damage * num2);
				if (component.IsLocal)
				{
					num4 /= 2;
				}
				if (num4 <= 0)
				{
					continue;
				}
				component.ApplyDamage(new DamageInfo(num4)
				{
					Force = vector * force,
					Hitpoint = hitpoint,
					ProjectileID = projectileId,
					WeaponID = weaponId,
					WeaponClass = weaponClass,
					DamageEffectFlag = damageEffectFlag,
					DamageEffectValue = damageEffectValue
				});
			}
			num++;
		}
	}
}
