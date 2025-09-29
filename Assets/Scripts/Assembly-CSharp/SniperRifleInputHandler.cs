using UberStrike.Realtime.UnitySdk;

public class SniperRifleInputHandler : WeaponInputHandler
{
	protected const float ZOOM = 4f;

	protected bool _scopeOpen;

	protected float _zoom;

	private IWeaponFireHandler _fireHandler;

	public SniperRifleInputHandler(IWeaponLogic logic, bool isLocal, ZoomInfo zoomInfo, bool autoFire)
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
		_scopeOpen = pressed;
		Update();
	}

	public override void OnPrevWeapon()
	{
		_zoom = -4f;
	}

	public override void OnNextWeapon()
	{
		_zoom = 4f;
	}

	public override void Update()
	{
		_fireHandler.Update();
		if (_scopeOpen)
		{
			if (!LevelCamera.Instance.IsZoomedIn || _zoom != 0f)
			{
				WeaponInputHandler.ZoomIn(_zoomInfo, _weaponLogic.Decorator, _zoom);
				_zoom = 0f;
				CmuneEventHandler.Route(new OnCameraZoomInEvent());
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(false);
			}
		}
		else if (LevelCamera.Instance.IsZoomedIn)
		{
			GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(true);
			WeaponInputHandler.ZoomOut(_zoomInfo, _weaponLogic.Decorator);
		}
	}

	public override bool CanChangeWeapon()
	{
		return !_scopeOpen;
	}

	public override void Stop()
	{
		_fireHandler.Stop();
		if (!_scopeOpen)
		{
			return;
		}
		_scopeOpen = false;
		if (_isLocal)
		{
			LevelCamera.Instance.ResetZoom();
			if (GameState.LocalCharacter.IsAlive)
			{
				GameState.LocalPlayer.WeaponCamera.SetCameraEnabled(true);
			}
		}
	}

	public override void OnPrimaryFire(bool pressed)
	{
		_fireHandler.OnTriggerPulled(pressed);
	}
}
