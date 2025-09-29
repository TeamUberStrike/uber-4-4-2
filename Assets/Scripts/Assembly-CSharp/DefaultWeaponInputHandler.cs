public class DefaultWeaponInputHandler : WeaponInputHandler
{
	private IWeaponFireHandler _primaryFireHandler;

	private IWeaponFireHandler _secondaryFireHandler;

	public DefaultWeaponInputHandler(IWeaponLogic logic, bool isLocal, bool autoFire, IWeaponFireHandler secondaryFireHandler = null)
		: base(logic, isLocal)
	{
		if (autoFire)
		{
			_primaryFireHandler = new FullAutoFireHandler(logic.Decorator);
		}
		else
		{
			_primaryFireHandler = new SemiAutoFireHandler(logic.Decorator);
		}
		_secondaryFireHandler = secondaryFireHandler;
	}

	public override void OnPrimaryFire(bool pressed)
	{
		_primaryFireHandler.OnTriggerPulled(pressed);
	}

	public override void OnSecondaryFire(bool pressed)
	{
		if (_secondaryFireHandler != null)
		{
			_secondaryFireHandler.OnTriggerPulled(pressed);
		}
	}

	public override void OnPrevWeapon()
	{
	}

	public override void OnNextWeapon()
	{
	}

	public override void Update()
	{
		_primaryFireHandler.Update();
	}

	public override bool CanChangeWeapon()
	{
		return true;
	}

	public override void Stop()
	{
		_primaryFireHandler.Stop();
	}
}
