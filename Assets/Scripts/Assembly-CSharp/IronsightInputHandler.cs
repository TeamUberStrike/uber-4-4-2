using UnityEngine;

public class IronsightInputHandler : WeaponInputHandler
{
	protected bool _isIronsight;

	protected float _ironSightDelay;

	private IWeaponFireHandler _fireHandler;

	public IronsightInputHandler(IWeaponLogic logic, bool isLocal, ZoomInfo zoomInfo, bool autoFire)
		: base(logic, isLocal)
	{
		_zoomInfo = zoomInfo;
		if (autoFire)
		{
			_fireHandler = new FullAutoFireHandler(logic.Decorator);
		}
		else
		{
			_fireHandler = new SemiAutoFireHandler(logic.Decorator);
		}
	}

	public override void OnSecondaryFire(bool pressed)
	{
		_isIronsight = pressed;
	}

	public override void Update()
	{
		_fireHandler.Update();
		UpdateIronsight();
		if (_isIronsight)
		{
			if (!LevelCamera.Instance.IsZoomedIn)
			{
				WeaponInputHandler.ZoomIn(_zoomInfo, _weaponLogic.Decorator, 0f);
			}
		}
		else if (LevelCamera.Instance.IsZoomedIn)
		{
			WeaponInputHandler.ZoomOut(_zoomInfo, _weaponLogic.Decorator);
		}
		if (!_isIronsight && _ironSightDelay > 0f)
		{
			_ironSightDelay -= Time.deltaTime;
		}
	}

	public override void Stop()
	{
		_fireHandler.Stop();
		if (_isIronsight)
		{
			_isIronsight = false;
			if (_isLocal)
			{
				LevelCamera.Instance.ResetZoom();
			}
			if (WeaponFeedbackManager.Instance.IsIronSighted)
			{
				WeaponFeedbackManager.Instance.ResetIronSight();
			}
		}
	}

	public override bool CanChangeWeapon()
	{
		return !_isIronsight && _ironSightDelay <= 0f;
	}

	private void UpdateIronsight()
	{
		if (_isIronsight)
		{
			if (!WeaponFeedbackManager.Instance.IsIronSighted)
			{
				WeaponFeedbackManager.Instance.BeginIronSight();
			}
		}
		else if (WeaponFeedbackManager.Instance.IsIronSighted)
		{
			WeaponFeedbackManager.Instance.EndIronSight();
		}
	}

	public override void OnPrimaryFire(bool pressed)
	{
		_fireHandler.OnTriggerPulled(pressed);
	}

	public override void OnPrevWeapon()
	{
	}

	public override void OnNextWeapon()
	{
	}
}
