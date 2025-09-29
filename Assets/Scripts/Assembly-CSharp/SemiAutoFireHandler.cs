public class SemiAutoFireHandler : IWeaponFireHandler
{
	private bool _isTriggerPulled;

	private BaseWeaponDecorator _weapon;

	public SemiAutoFireHandler(BaseWeaponDecorator weapon)
	{
		_weapon = weapon;
		_isTriggerPulled = false;
	}

	public void OnTriggerPulled(bool pulled)
	{
		if (pulled && !_isTriggerPulled && Singleton<WeaponController>.Instance.Shoot())
		{
			_weapon.PostShoot();
		}
		_isTriggerPulled = pulled;
	}

	public void Update()
	{
	}

	public void Stop()
	{
	}
}
