using UnityEngine;

public interface IWeaponController
{
	byte PlayerNumber { get; }

	bool IsLocal { get; }

	Vector3 ShootingPoint { get; }

	Vector3 ShootingDirection { get; }

	int NextProjectileId();

	void UpdateWeaponDecorator(IUnityItem item);
}
