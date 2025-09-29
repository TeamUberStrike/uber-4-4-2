using UnityEngine;

public class MinigunInputHandler : WeaponInputHandler
{
	protected bool _isGunWarm;

	protected bool _isWarmupPlayed;

	protected float _warmTime;

	private MinigunWeaponDecorator _decorator;

	public MinigunInputHandler(IWeaponLogic logic, bool isLocal, MinigunWeaponDecorator decorator)
		: base(logic, isLocal)
	{
		_decorator = decorator;
	}

	public override void Update()
	{
		if (!_decorator)
		{
			return;
		}
		if (_warmTime < _decorator.MaxWarmUpTime)
		{
			if (_isGunWarm || _isTriggerPulled)
			{
				if (!_isWarmupPlayed)
				{
					_isWarmupPlayed = true;
					_decorator.PlayWindUpSound(_warmTime);
				}
				_warmTime += Time.deltaTime;
				if (_warmTime >= _decorator.MaxWarmUpTime)
				{
					_decorator.PlayDuringSound();
				}
				_decorator.SpinWeaponHead();
			}
		}
		else if (_isTriggerPulled)
		{
			Singleton<WeaponController>.Instance.Shoot();
		}
		else if (_isGunWarm)
		{
			_decorator.SpinWeaponHead();
		}
		if (_isGunWarm || _isTriggerPulled)
		{
			return;
		}
		if (_warmTime > 0f)
		{
			_warmTime -= Time.deltaTime;
			if (_warmTime < 0f)
			{
				_warmTime = 0f;
			}
			if (_isWarmupPlayed)
			{
				_decorator.PlayWindDownSound((1f - _warmTime / _decorator.MaxWarmUpTime) * _decorator.MaxWarmDownTime);
			}
		}
		_isWarmupPlayed = false;
	}

	public override void OnSecondaryFire(bool pressed)
	{
		_isGunWarm = pressed;
	}

	public override bool CanChangeWeapon()
	{
		return !_isGunWarm;
	}

	public override void Stop()
	{
		_warmTime = 0f;
		_isGunWarm = false;
		_isWarmupPlayed = false;
		_isTriggerPulled = false;
		if ((bool)_decorator)
		{
			_decorator.StopSound();
		}
	}

	public override void OnPrimaryFire(bool pressed)
	{
		_isTriggerPulled = pressed;
	}

	public override void OnPrevWeapon()
	{
	}

	public override void OnNextWeapon()
	{
	}
}
