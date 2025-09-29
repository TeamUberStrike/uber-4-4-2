using UnityEngine;

[RequireComponent(typeof(ParticleRenderer))]
public class MuzzleSmoke : BaseWeaponEffect
{
	private ParticleEmitter _particleEmitter;

	private void Awake()
	{
		_particleEmitter = GetComponentInChildren<ParticleEmitter>();
	}

	public override void OnShoot()
	{
		if ((bool)_particleEmitter)
		{
			base.gameObject.SetActive(true);
			_particleEmitter.Emit();
		}
	}

	public override void OnPostShoot()
	{
	}

	public override void OnHits(RaycastHit[] hits)
	{
	}

	public override void Hide()
	{
	}
}
