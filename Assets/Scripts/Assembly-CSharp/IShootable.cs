using UnityEngine;

public interface IShootable
{
	bool IsVulnerable { get; }

	bool IsLocal { get; }

	void ApplyDamage(DamageInfo shot);

	void ApplyForce(Vector3 position, Vector3 force);
}
