using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class AmmoHud : Singleton<AmmoHud>
{
	private float _curScaleFactor;

	private MeshGUIText _ammoDigits;

	private MeshGUIQuad _ammoIcon;

	private MeshGUIQuad _glowBlur;

	private Animatable2DGroup _ammoGroup;

	private Animatable2DGroup _entireGroup;

	private int _curAmmo;

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (_entireGroup.IsVisible != value)
			{
				if (value)
				{
					_entireGroup.Show();
					CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
					CmuneEventHandler.AddListener<CameraWidthChangeEvent>(OnCameraWidthChange);
				}
				else
				{
					_entireGroup.Hide();
					CmuneEventHandler.RemoveListener<ScreenResolutionEvent>(OnScreenResolutionChange);
					CmuneEventHandler.RemoveListener<CameraWidthChangeEvent>(OnCameraWidthChange);
					Singleton<TemporaryWeaponHud>.Instance.Enabled = false;
				}
			}
		}
	}

	public int Ammo
	{
		get
		{
			return _curAmmo;
		}
		set
		{
			SetRemainingAmmo(value);
		}
	}

	private AmmoHud()
	{
		if (HudAssets.Exists)
		{
			_curAmmo = 0;
			_ammoDigits = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.LowerRight);
			_ammoDigits.NamePrefix = "AM";
			_ammoIcon = new MeshGUIQuad(HudTextures.AmmoBlue);
			_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
			_glowBlur.Name = "AmmoHudGlow";
			_glowBlur.Depth = 1f;
			_ammoGroup = new Animatable2DGroup();
			_entireGroup = new Animatable2DGroup();
			_ammoGroup.Group.Add(_ammoDigits);
			_ammoGroup.Group.Add(_ammoIcon);
			_entireGroup.Group.Add(_ammoGroup);
			_entireGroup.Group.Add(_glowBlur);
			ResetHud();
			Enabled = false;
			CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
			CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
			CmuneEventHandler.AddListener<OnGlobalUIRibbonChanged>(OnGlobalUIRibbonChange);
		}
	}

	public void Draw()
	{
		if (Singleton<WeaponController>.Instance.HasAnyWeapon)
		{
			_entireGroup.Draw();
		}
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(_ammoDigits);
		_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		if (ev.TeamId == TeamID.RED)
		{
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_ammoDigits);
			_ammoIcon.Texture = HudTextures.AmmoRed;
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_RED_COLOR;
			ResetTransform();
		}
		else
		{
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_ammoDigits);
			_ammoIcon.Texture = HudTextures.AmmoBlue;
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
			ResetTransform();
		}
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void OnCameraWidthChange(CameraWidthChangeEvent ev)
	{
		ResetTransform();
	}

	private void OnGlobalUIRibbonChange(OnGlobalUIRibbonChanged ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		_curScaleFactor = 0.65f;
		ResetAmmoTransform();
		ResetBlurTransform();
		float num = (float)Screen.height * 0.07f;
		if (GlobalUIRibbon.Instance.IsEnabled)
		{
			num += 44f;
		}
		_entireGroup.Position = new Vector2((float)Screen.width * 0.98f, num);
	}

	private void ResetAmmoTransform()
	{
		float num = 0.07f;
		float num2 = (float)Screen.height * 0.03f / (float)_ammoIcon.Texture.height;
		_ammoIcon.Scale = new Vector2(num2, num2);
		_ammoIcon.Position = new Vector2(0f - _ammoIcon.Size.x, (0f - _ammoIcon.Size.y) * 0.8f);
		_ammoDigits.Text = _curAmmo.ToString();
		_ammoDigits.Scale = new Vector2(HudStyleUtility.SMALLER_DIGITS_SCALE * _curScaleFactor, HudStyleUtility.SMALLER_DIGITS_SCALE * _curScaleFactor);
		_ammoDigits.Position = new Vector2(0f - _ammoIcon.Size.x - HudStyleUtility.GAP_BETWEEN_TEXT, num * _ammoDigits.Size.y);
	}

	private void ResetBlurTransform()
	{
		float num = _ammoGroup.Rect.width * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR;
		float num2 = _ammoGroup.Rect.height * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_glowBlur.Scale = new Vector2(num / (float)HudTextures.WhiteBlur128.width, num2 / (float)HudTextures.WhiteBlur128.height);
		_glowBlur.Position = new Vector2((0f - num - _ammoGroup.Rect.width) / 2f, (0f - num2 - _ammoGroup.Rect.height) / 2f);
	}

	private void SetRemainingAmmo(int ammo)
	{
		bool flag = ammo > _curAmmo;
		_curAmmo = ammo;
		ResetTransform();
		if (flag)
		{
			_ammoDigits.Flicker(0.1f);
		}
	}
}
