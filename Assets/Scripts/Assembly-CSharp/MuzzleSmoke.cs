using UnityEngine;

public class MuzzleSmoke : BaseWeaponEffect
{
	private ParticleSystem _particleEmitter;

	private void Awake()
	{
		_particleEmitter = GetComponentInChildren<ParticleSystem>();
	}

	public override void OnShoot()
	{
		if (_particleEmitter != null)
		{
			base.gameObject.SetActive(true);
			_particleEmitter.Emit(10); // Emit 10 particles for muzzle smoke effect
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
