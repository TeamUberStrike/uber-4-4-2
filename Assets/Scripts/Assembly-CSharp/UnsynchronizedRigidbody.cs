using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnsynchronizedRigidbody : BaseGameProp
{
	public override void ApplyDamage(DamageInfo d)
	{
		base.Rigidbody.AddForceAtPosition(d.Force, d.Hitpoint);
	}
}
