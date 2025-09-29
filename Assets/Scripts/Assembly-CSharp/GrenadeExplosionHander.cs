public class GrenadeExplosionHander : IWeaponFireHandler
{
	public void OnTriggerPulled(bool pulled)
	{
		if (pulled)
		{
			Singleton<ProjectileManager>.Instance.RemoveAllLimitedProjectiles();
		}
	}

	public void Update()
	{
	}

	public void Stop()
	{
	}
}
