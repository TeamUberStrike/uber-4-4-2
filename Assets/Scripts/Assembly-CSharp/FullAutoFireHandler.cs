public class FullAutoFireHandler : IWeaponFireHandler
{
	private bool _isShooting;

	private bool _isTriggerPulled;

	private BaseWeaponDecorator _weapon;

	public FullAutoFireHandler(BaseWeaponDecorator weapon)
	{
		_weapon = weapon;
	}

	public void OnTriggerPulled(bool pulled)
	{
		_isTriggerPulled = pulled;
	}

	public void Update()
	{
		if (_isTriggerPulled && Singleton<WeaponController>.Instance.Shoot())
		{
			_isShooting = true;
		}
		if (_isShooting && !_isTriggerPulled)
		{
			_isShooting = false;
			if ((bool)_weapon)
			{
				_weapon.PostShoot();
			}
		}
	}

	public void Stop()
	{
		_isTriggerPulled = false;
	}
}
