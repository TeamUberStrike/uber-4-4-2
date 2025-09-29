using System.Collections;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class HpApHud : Singleton<HpApHud>
{
	private float _curScaleFactor;

	private MeshGUIText _hpDigitsText;

	private MeshGUIText _apDigitsText;

	private MeshGUIText _hpText;

	private MeshGUIText _apText;

	private Animatable2DGroup _hpApGroup;

	private MeshGUIQuad _glowBlur;

	private Animatable2DGroup _entireGroup;

	private int _curHP;

	private int _curAP;

	private static int WARNING_HEALTH_LOW_VALUE = 25;

	private static float WARNING_HEALTH_LOW_INTERVAL = 0.8f;

	private bool _isLowHealth;

	private float _nextWarningTime;

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_entireGroup.Show();
			}
			else
			{
				_entireGroup.Hide();
			}
		}
	}

	public int HP
	{
		get
		{
			return _curHP;
		}
		set
		{
			SetHealthPoint(value);
		}
	}

	public int AP
	{
		get
		{
			return _curAP;
		}
		set
		{
			SetArmorPoint(value);
		}
	}

	private HpApHud()
	{
		_curAP = 0;
		_curHP = 0;
		TextAnchor textAnchor = TextAnchor.LowerLeft;
		_hpDigitsText = new MeshGUIText(_curHP.ToString(), HudAssets.Instance.InterparkBitmapFont, textAnchor);
		_hpDigitsText.NamePrefix = "HP";
		_apDigitsText = new MeshGUIText(_curAP.ToString(), HudAssets.Instance.InterparkBitmapFont, textAnchor);
		_apDigitsText.NamePrefix = "AP";
		_hpText = new MeshGUIText("HP", HudAssets.Instance.HelveticaBitmapFont, textAnchor);
		_apText = new MeshGUIText("AP", HudAssets.Instance.HelveticaBitmapFont, textAnchor);
		_hpApGroup = new Animatable2DGroup();
		_hpApGroup.Group.Add(_hpDigitsText);
		_hpApGroup.Group.Add(_apDigitsText);
		_hpApGroup.Group.Add(_hpText);
		_hpApGroup.Group.Add(_apText);
		_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
		_glowBlur.Name = "HpApHudGlow";
		_glowBlur.Depth = 1f;
		_entireGroup = new Animatable2DGroup();
		_entireGroup.Group.Add(_hpApGroup);
		_entireGroup.Group.Add(_glowBlur);
		ResetHud();
		Enabled = false;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
		CmuneEventHandler.AddListener<OnGlobalUIRibbonChanged>(OnGlobalUIRibbonChange);
	}

	public void Update()
	{
		_entireGroup.Draw();
		if (_isLowHealth && _curHP != 0 && _nextWarningTime > 0f && Time.time > _nextWarningTime)
		{
			MonoRoutine.Start(OnHealthLow());
			_nextWarningTime = -1f;
		}
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_hpDigitsText);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_apDigitsText);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_hpText);
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_apText);
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		if (ev.TeamId == TeamID.RED)
		{
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_hpDigitsText);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_apDigitsText);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_hpText);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_apText);
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_RED_COLOR;
		}
		else
		{
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_hpDigitsText);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_apDigitsText);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_hpText);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_apText);
			_glowBlur.Color = HudStyleUtility.GLOW_BLUR_BLUE_COLOR;
		}
		_hpText.BitmapMeshText.AlphaMin = 0.4f;
		_apText.BitmapMeshText.AlphaMin = 0.4f;
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
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
		ResetHpApTransform();
		ResetBlurTransform();
		float num = (float)Screen.height * 0.08f;
		if (GlobalUIRibbon.Instance.IsEnabled)
		{
			num += 44f;
		}
		_entireGroup.Position = new Vector2((float)Screen.width * 0.04f, num);
		_entireGroup.UpdateMeshGUIPosition();
	}

	private void ResetHpApTransform()
	{
		float num = 0.07f;
		_hpDigitsText.Text = _curHP.ToString();
		_hpDigitsText.Scale = new Vector2(1f * _curScaleFactor, 1f * _curScaleFactor);
		_hpDigitsText.Position = new Vector2(0f, num * _hpDigitsText.Size.y);
		float num2 = 3f;
		float num3 = 0f;
		num3 += _hpDigitsText.Size.x + num2;
		_hpText.Scale = new Vector2(0.35f * _curScaleFactor, 0.35f * _curScaleFactor);
		_hpText.Position = new Vector2(num3, 0f);
		num3 += (float)Screen.width * 0.03f;
		_apDigitsText.Text = _curAP.ToString();
		_apDigitsText.Scale = new Vector2(0.7f * _curScaleFactor, 0.7f * _curScaleFactor);
		_apDigitsText.Position = new Vector2(num3, num * _apDigitsText.Size.y);
		num3 += _apDigitsText.Size.x + num2;
		_apText.Scale = new Vector2(0.35f * _curScaleFactor, 0.35f * _curScaleFactor);
		_apText.Position = new Vector2(num3, 0f);
	}

	private void ResetBlurTransform()
	{
		float num = _hpApGroup.Rect.width * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR;
		float num2 = _hpApGroup.Rect.height * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_glowBlur.Scale = new Vector2(num / (float)HudTextures.WhiteBlur128.width, num2 / (float)HudTextures.WhiteBlur128.height);
		_glowBlur.Position = new Vector2((_hpApGroup.Rect.width - num) / 2f, (0f - _hpApGroup.Rect.height - num2) / 2f);
	}

	private void SetHealthPoint(int hp)
	{
		bool flag = hp > _curHP;
		bool flag2 = hp < _curHP;
		_curHP = ((hp >= 0) ? hp : 0);
		ResetTransform();
		if (flag)
		{
			OnHealthIncrease();
		}
		if (flag2)
		{
			MonoRoutine.Start(OnHealthDecrease());
		}
		bool flag3 = _curHP < WARNING_HEALTH_LOW_VALUE;
		if (flag3 != _isLowHealth)
		{
			_isLowHealth = flag3;
			_nextWarningTime = Time.time + WARNING_HEALTH_LOW_INTERVAL;
		}
	}

	private void SetArmorPoint(int ap)
	{
		bool flag = ap < _curAP;
		_curAP = ((ap >= 0) ? ap : 0);
		ResetTransform();
		if (flag)
		{
			MonoRoutine.Start(OnArmorDecrease());
		}
	}

	private void OnHealthIncrease()
	{
		float time = 0.1f;
		_hpDigitsText.Flicker(time);
		_apDigitsText.Flicker(time);
	}

	private IEnumerator OnHealthDecrease()
	{
		int berpAnimId = Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(OnHealthDecrease);
		float berpTime = 0.1f;
		float berpScale = 0.8f;
		Vector2 pivot = _hpDigitsText.Center;
		_hpDigitsText.ScaleAroundPivot(new Vector2(berpScale, berpScale), pivot, berpTime, EaseType.Berp);
		yield return new WaitForSeconds(berpTime);
		if (Singleton<PreemptiveCoroutineManager>.Instance.IsCurrent(OnHealthDecrease, berpAnimId))
		{
			_hpDigitsText.ScaleAroundPivot(new Vector2(1f / berpScale, 1f / berpScale), pivot, berpTime, EaseType.Berp);
		}
	}

	private IEnumerator OnHealthLow()
	{
		int berpAnimId = Singleton<PreemptiveCoroutineManager>.Instance.IncrementId(OnHealthDecrease);
		float sizeIncreaseTime = 0.02f;
		float sizeDecreaseTime = 0.2f;
		float sizeIncreaseScale = 0.8f;
		Vector2 pivot = _hpDigitsText.Center;
		Vector2 oriScale = _hpDigitsText.Scale;
		_hpDigitsText.ScaleAroundPivot(new Vector2(sizeIncreaseScale, sizeIncreaseScale), pivot, sizeIncreaseTime);
		yield return new WaitForSeconds(sizeIncreaseTime);
		if (!Singleton<PreemptiveCoroutineManager>.Instance.IsCurrent(OnHealthDecrease, berpAnimId))
		{
			_nextWarningTime = Time.time + WARNING_HEALTH_LOW_INTERVAL;
			yield break;
		}
		_hpDigitsText.ScaleToAroundPivot(oriScale, pivot, sizeDecreaseTime);
		yield return new WaitForSeconds(sizeDecreaseTime);
		_nextWarningTime = Time.time + WARNING_HEALTH_LOW_INTERVAL;
	}

	private IEnumerator OnArmorDecrease()
	{
		float berpTime = 0.05f;
		float berpScale = 0.8f;
		Vector2 pivot = _apDigitsText.Center;
		_apDigitsText.ScaleAroundPivot(new Vector2(berpScale, berpScale), pivot, berpTime, EaseType.Berp);
		yield return new WaitForSeconds(berpTime);
		_apDigitsText.ScaleAroundPivot(new Vector2(1f / berpScale, 1f / berpScale), pivot, berpTime, EaseType.Berp);
	}
}
