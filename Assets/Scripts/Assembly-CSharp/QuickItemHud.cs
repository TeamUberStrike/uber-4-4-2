using System.Collections;
using UnityEngine;

public class QuickItemHud
{
	private const float EmptyAlpha = 0.3f;

	private const float BackgroundAlpha = 0.3f;

	private const float CooldownAlpha = 1f;

	private const float RechargeAlpha = 0.5f;

	private const int AngleOffset = 0;

	private const int TotalFillAngle = 360;

	private MeshGUICircle _recharge;

	private MeshGUICircle _cooldown;

	private MeshGUIQuad _cooldownFlash;

	private MeshGUIQuad _background;

	private MeshGUIQuad _selection;

	private MeshGUIQuad _icon;

	private MeshGUIText _countText;

	private MeshGUIText _descriptionText;

	private Animatable2DGroup _quickItemGroup;

	private int _amount;

	private float _cooldownTime;

	private float _cooldownMax;

	private float _rechargeTime;

	private float _rechargeTimeMax;

	private bool _isAnimating;

	private bool _isCooliingDown;

	private Vector2 _expandGroupPos;

	private Vector2 _collapseGroupPos;

	public string Name { get; private set; }

	public bool IsEmpty { get; private set; }

	public int Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			bool isDecreasing = value < _amount;
			_amount = ((value > 0) ? value : 0);
			UpdateSpringCountText(isDecreasing);
		}
	}

	public float Cooldown
	{
		set
		{
			if (_cooldownTime != value)
			{
				_cooldownTime = value;
				UpdateCooldownAngle();
			}
		}
	}

	public float CooldownMax
	{
		set
		{
			_cooldownMax = value;
			UpdateCooldownAngle();
		}
	}

	public float Recharging
	{
		set
		{
			if (_rechargeTime != value)
			{
				_rechargeTime = value;
				UpdateRechargingAngle();
			}
		}
	}

	public float RechargingMax
	{
		set
		{
			_rechargeTimeMax = value;
			UpdateRechargingAngle();
		}
	}

	public Animatable2DGroup Group
	{
		get
		{
			return _quickItemGroup;
		}
	}

	public float ExpandedHeight { get; private set; }

	public float CollapsedHeight { get; private set; }

	public bool IsExpanded { get; private set; }

	public QuickItemHud(string name)
	{
		Name = name;
		_amount = 0;
		_recharge = new MeshGUICircle(ConsumableHudTextures.CircleBlue)
		{
			Name = name + "Recharging",
			Alpha = 0.5f,
			Depth = 2f
		};
		_recharge.CircleMesh.StartAngle = 0f;
		_cooldown = new MeshGUICircle(ConsumableHudTextures.CircleBlue)
		{
			Name = name + "Cooldown",
			Alpha = 1f,
			Depth = 3f
		};
		_cooldown.CircleMesh.StartAngle = 0f;
		_cooldownFlash = new MeshGUIQuad(ConsumableHudTextures.CircleWhite, TextAnchor.MiddleCenter)
		{
			Name = name + "Flash",
			Alpha = 0f,
			Depth = 3f
		};
		_background = new MeshGUIQuad(ConsumableHudTextures.CircleBlue, TextAnchor.MiddleCenter)
		{
			Name = name + "CooldownBackground",
			Alpha = 0.3f,
			Depth = 6f
		};
		_countText = new MeshGUIText(_amount.ToString(), HudAssets.Instance.HelveticaBitmapFont, TextAnchor.LowerCenter)
		{
			NamePrefix = name,
			Depth = 7f
		};
		if (!ApplicationDataManager.IsMobile)
		{
			_descriptionText = new MeshGUIText("Key: 7", HudAssets.Instance.HelveticaBitmapFont, TextAnchor.LowerCenter)
			{
				NamePrefix = name,
				Depth = 8f
			};
			_selection = new MeshGUIQuad(ConsumableHudTextures.CircleBlue, TextAnchor.MiddleCenter)
			{
				Name = name + "Selection",
				Depth = 4f
			};
		}
		_icon = new MeshGUIQuad(ConsumableHudTextures.AmmoBlue, TextAnchor.MiddleCenter)
		{
			Name = name + "Icon",
			Depth = 1f,
			Alpha = 0.3f
		};
		_quickItemGroup = new Animatable2DGroup();
		_quickItemGroup.Group.Add(_recharge);
		_quickItemGroup.Group.Add(_cooldown);
		_quickItemGroup.Group.Add(_cooldownFlash);
		_quickItemGroup.Group.Add(_background);
		_quickItemGroup.Group.Add(_icon);
		_quickItemGroup.Group.Add(_countText);
		if (!ApplicationDataManager.IsMobile)
		{
			_quickItemGroup.Group.Add(_descriptionText);
			_quickItemGroup.Group.Add(_selection);
		}
	}

	public void SetKeyBinding(string binding)
	{
		if (!ApplicationDataManager.IsMobile)
		{
			_descriptionText.Text = "Key: " + binding;
		}
	}

	public void SetRechargeBarVisible(bool isVisible)
	{
		_recharge.IsEnabled = isVisible;
	}

	public void Expand(Vector2 destPos, float delay = 0f)
	{
		ResetHud();
		_expandGroupPos = destPos;
		Vector2 vector = new Vector2(_background.Size.x / 2f, _background.Size.y + _countText.Size.y * 1.5f);
		if (_quickItemGroup.IsVisible)
		{
			_quickItemGroup.MoveTo(_expandGroupPos, 0.3f, EaseType.Berp, delay);
			_descriptionText.FadeAlphaTo(1f, 0.3f, EaseType.In);
			_descriptionText.MoveTo(vector, 0.3f, EaseType.Berp, delay);
		}
		else
		{
			_quickItemGroup.Position = _expandGroupPos;
			_descriptionText.Alpha = 1f;
			_descriptionText.Position = vector;
		}
		IsExpanded = true;
	}

	public void Expand(bool moveNext = true)
	{
		ResetHud();
		float x = 40f;
		if (moveNext)
		{
			x = -40f;
		}
		_quickItemGroup.Position = new Vector2(x, CollapsedHeight);
		_quickItemGroup.MoveTo(new Vector2(0f, CollapsedHeight), 0.3f, EaseType.Berp, 0f);
		_quickItemGroup.FadeAlphaTo(1f, 0.3f);
		_cooldown.StopFading();
		_cooldown.Alpha = 0f;
		_cooldownFlash.StopFading();
		_cooldownFlash.Alpha = 0f;
		IsExpanded = true;
	}

	public void Collapse(Vector2 destPos, float delay = 0f)
	{
		ResetHud();
		_collapseGroupPos = destPos;
		Vector2 vector = new Vector2(_background.Size.x / 2f, _background.Size.y + 5f);
		if (_quickItemGroup.IsVisible)
		{
			_quickItemGroup.MoveTo(_collapseGroupPos, 0.3f, EaseType.Berp, delay);
			_descriptionText.FadeAlphaTo(0f, 0.3f, EaseType.Out);
			_descriptionText.MoveTo(vector, 0.3f, EaseType.Berp, delay);
		}
		else
		{
			_quickItemGroup.Position = _collapseGroupPos;
			_descriptionText.Alpha = 0f;
			_descriptionText.Position = vector;
		}
		IsExpanded = false;
	}

	public void Collapse(bool moveNext = true)
	{
		ResetHud();
		float x = -40f;
		if (moveNext)
		{
			x = 40f;
		}
		_quickItemGroup.Position = new Vector2(0f, CollapsedHeight);
		_quickItemGroup.MoveTo(new Vector2(x, CollapsedHeight), 0.3f, EaseType.Berp, 0f);
		_quickItemGroup.FadeAlphaTo(0f, 0.3f);
		IsExpanded = false;
	}

	public void SetSelected(bool selected, bool moveNext = true)
	{
		if (IsEmpty)
		{
			selected = false;
		}
		if (ApplicationDataManager.IsMobile)
		{
			if (!selected)
			{
				Collapse(moveNext);
			}
			else
			{
				Expand(moveNext);
			}
		}
		else
		{
			_selection.IsEnabled = selected;
		}
	}

	public void ConfigureEmptySlot()
	{
		_quickItemGroup.Hide();
		IsEmpty = true;
		ResetHud();
	}

	public void ConfigureSlot(Color textColor, Texture rechargingTexture, Texture cooldownTexture, Texture backgroundTexture, Texture selectionTexture, Texture2D icon)
	{
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_countText);
		if (!ApplicationDataManager.IsMobile)
		{
			Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_descriptionText);
		}
		_recharge.Texture = rechargingTexture;
		_cooldown.Texture = cooldownTexture;
		_cooldown.Alpha = 0f;
		_cooldownFlash.Texture = cooldownTexture;
		_cooldownFlash.Alpha = 0f;
		_background.Texture = backgroundTexture;
		_background.Alpha = 0.3f;
		_icon.Texture = icon;
		_countText.BitmapMeshText.ShadowColor = textColor;
		_countText.BitmapMeshText.AlphaMin = 0.4f;
		if (!ApplicationDataManager.IsMobile)
		{
			_selection.Texture = selectionTexture;
			_descriptionText.BitmapMeshText.ShadowColor = textColor;
			_descriptionText.BitmapMeshText.AlphaMin = 0.4f;
			_descriptionText.BitmapMeshText.MainColor = _descriptionText.BitmapMeshText.MainColor.SetAlpha(0.5f);
		}
		_quickItemGroup.Show();
		IsEmpty = false;
		ResetHud();
	}

	public void ResetHud()
	{
		_quickItemGroup.StopScaling();
		_quickItemGroup.StopMoving();
		_cooldownFlash.StopScaling();
		_cooldownFlash.StopFading();
		_isAnimating = false;
		ResetTransform();
	}

	private void ResetTransform()
	{
		Vector2 vector = Vector2.one * 0.8f;
		_background.Scale = vector;
		_icon.Scale = vector * 0.9f;
		_recharge.Scale = vector * 0.8f;
		_cooldown.Scale = vector;
		_cooldownFlash.Scale = vector;
		_countText.Scale = Vector2.one * 0.25f;
		if (!ApplicationDataManager.IsMobile)
		{
			_selection.Scale = vector;
			_descriptionText.Scale = Vector2.one * 0.25f;
		}
		Vector2 position = _background.Size / 2f;
		_icon.Position = position;
		_recharge.Position = _background.Size / 2f;
		_cooldown.Position = _background.Size / 2f;
		_cooldownFlash.Position = _background.Size / 2f;
		_background.Position = _background.Size / 2f;
		_countText.Position = new Vector2(_background.Size.x * 0.95f, _background.Size.y + _countText.Size.y * 0.5f);
		if (!ApplicationDataManager.IsMobile)
		{
			_selection.Position = _selection.Size / 2f;
		}
		UpdateExpandedHeight();
	}

	private void UpdateCollapsedHeight()
	{
		if (!ApplicationDataManager.IsMobile)
		{
			Vector2 position = _descriptionText.Position;
			_descriptionText.Position = new Vector2(_background.Size.x / 2f, _background.Size.y + 5f);
			float height = Group.Rect.height;
			_descriptionText.Position = position;
			CollapsedHeight = height;
		}
	}

	private void UpdateExpandedHeight()
	{
		UpdateCollapsedHeight();
		ExpandedHeight = CollapsedHeight + _countText.Size.y;
	}

	private void UpdateSpringCountText(bool isDecreasing)
	{
		if (_amount <= 0)
		{
			_icon.Alpha = 0.3f;
			_cooldown.Alpha = 0f;
		}
		else if (!ApplicationDataManager.IsMobile || IsExpanded)
		{
			_icon.Alpha = 1f;
		}
		else
		{
			_icon.Alpha = 0f;
		}
		_countText.Text = _amount.ToString();
		if (isDecreasing)
		{
			MonoRoutine.Start(OnQuickItemDecrement());
		}
	}

	private IEnumerator OnQuickItemDecrement()
	{
		ResetHud();
		_isAnimating = true;
		float sizeIncreaseTime = 0.05f;
		float sizeIncreaseScale = 1.2f;
		float waitTime = 0.3f;
		float sizeDecreaseTime = 0.05f;
		Vector2 pivot = _quickItemGroup.Center;
		_quickItemGroup.ScaleAroundPivot(new Vector2(sizeIncreaseScale, sizeIncreaseScale), pivot, sizeIncreaseTime);
		yield return new WaitForSeconds(sizeIncreaseTime + waitTime);
		if (_isAnimating)
		{
			_quickItemGroup.ScaleAroundPivot(new Vector2(1f / sizeIncreaseScale, 1f / sizeIncreaseScale), pivot, sizeDecreaseTime);
			yield return new WaitForSeconds(sizeDecreaseTime);
		}
		_isAnimating = false;
	}

	private void UpdateCooldownAngle()
	{
		float num = ((_cooldownMax != 0f) ? ((_cooldownMax - _cooldownTime) * 360f / _cooldownMax) : 360f);
		if (num < 360f)
		{
			if (_cooldown.Angle == 360f)
			{
				OnCooldownStart();
			}
			if (_isCooliingDown && _amount > 0 && (!ApplicationDataManager.IsMobile || IsExpanded))
			{
				_cooldown.Alpha = 1f;
			}
		}
		else
		{
			if (_cooldown.Angle < 360f && _amount > 0)
			{
				OnCooldownFinish();
			}
			if (!_isCooliingDown)
			{
				_cooldown.Alpha = 0f;
			}
		}
		_cooldown.Angle = num;
	}

	private void OnCooldownStart()
	{
		_isCooliingDown = true;
	}

	private void OnCooldownFinish()
	{
		_isCooliingDown = false;
		MonoRoutine.Start(DoFlashAnim());
	}

	private void UpdateRechargingAngle()
	{
		float angle = ((_rechargeTimeMax != 0f) ? ((_rechargeTimeMax - _rechargeTime) * 360f / _rechargeTimeMax) : 360f);
		_recharge.Angle = angle;
	}

	private IEnumerator DoFlashAnim()
	{
		ResetHud();
		_isAnimating = true;
		_cooldownFlash.StopScaling();
		_cooldownFlash.StopFading();
		_cooldownFlash.StopFading();
		ResetCooldownFlashView(1f);
		float animTime = 0.2f;
		_cooldownFlash.ScaleTo(Vector2.one * 1.5f, animTime);
		_cooldownFlash.Flicker(animTime);
		_cooldownFlash.FadeAlphaTo(0f, animTime);
		yield return new WaitForSeconds(animTime + 0.1f);
		if (_isAnimating)
		{
			ResetCooldownFlashView(0f);
		}
	}

	private void ResetCooldownFlashView(float alpha)
	{
		if (ApplicationDataManager.IsMobile && !IsExpanded)
		{
			alpha = 0f;
		}
		_cooldownFlash.Alpha = alpha;
		_cooldownFlash.Scale = Vector2.one * 0.8f;
		_cooldownFlash.Position = _background.Size / 2f;
	}
}
