using UnityEngine;

public abstract class WeaponInputHandler
{
	protected bool _isLocal;

	protected IWeaponLogic _weaponLogic;

	protected bool _isTriggerPulled;

	protected ZoomInfo _zoomInfo;

	protected WeaponInputHandler(IWeaponLogic logic, bool isLocal)
	{
		_isLocal = isLocal;
		_weaponLogic = logic;
		_isTriggerPulled = false;
	}

	protected static void ZoomIn(ZoomInfo zoomInfo, BaseWeaponDecorator weapon, float zoom)
	{
		if ((bool)weapon)
		{
			if (!LevelCamera.Instance.IsZoomedIn)
			{
				SfxManager.Play3dAudioClip(GameAudio.SniperScopeIn, weapon.transform.position);
			}
			else if (zoom < 0f && zoomInfo.CurrentMultiplier != zoomInfo.MinMultiplier)
			{
				SfxManager.Play3dAudioClip(GameAudio.SniperZoomIn, weapon.transform.position);
			}
			else if (zoom > 0f && zoomInfo.CurrentMultiplier != zoomInfo.MaxMultiplier)
			{
				SfxManager.Play3dAudioClip(GameAudio.SniperZoomOut, weapon.transform.position);
			}
			zoomInfo.CurrentMultiplier = Mathf.Clamp(zoomInfo.CurrentMultiplier + zoom, zoomInfo.MinMultiplier, zoomInfo.MaxMultiplier);
			LevelCamera.Instance.DoZoomIn(60f / zoomInfo.CurrentMultiplier, 20f);
			UserInput.ZoomSpeed = 0.5f;
		}
	}

	protected static void ZoomOut(ZoomInfo zoomInfo, BaseWeaponDecorator weapon)
	{
		if (LevelCamera.Instance != null)
		{
			LevelCamera.Instance.DoZoomOut(60f, 10f);
		}
		UserInput.ZoomSpeed = 1f;
		if (zoomInfo != null)
		{
			zoomInfo.CurrentMultiplier = zoomInfo.DefaultMultiplier;
		}
		if ((bool)weapon)
		{
			SfxManager.Play3dAudioClip(GameAudio.SniperScopeOut, weapon.transform.position);
		}
	}

	public abstract void OnPrimaryFire(bool pressed);

	public abstract void OnSecondaryFire(bool pressed);

	public abstract void OnPrevWeapon();

	public abstract void OnNextWeapon();

	public abstract void Update();

	public abstract bool CanChangeWeapon();

	public virtual void Stop()
	{
	}
}
