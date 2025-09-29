using UnityEngine;

public class PickupNameHud : Singleton<PickupNameHud>
{
	private float _curScaleFactor;

	private MeshGUIText _pickUpText;

	private PickUpMessageType _lastPickUpType;

	private int _samePickUpCount;

	private TemporaryDisplayAnim _displayAnim;

	public bool Enabled
	{
		get
		{
			return _pickUpText.IsVisible;
		}
		set
		{
			if (_pickUpText.IsVisible != value)
			{
				if (value)
				{
					_pickUpText.Show();
				}
				else
				{
					_pickUpText.Hide();
				}
				_displayAnim.Stop();
			}
		}
	}

	private PickupNameHud()
	{
		_pickUpText = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_pickUpText.NamePrefix = "Pickup";
		_displayAnim = new TemporaryDisplayAnim(_pickUpText, 2f, 1f);
		ResetHud();
		Enabled = true;
	}

	public void Draw()
	{
	}

	public void Update()
	{
		_displayAnim.Update();
		_pickUpText.Draw();
		_pickUpText.ShadowColorAnim.Alpha = 0f;
	}

	public void DisplayPickupName(string itemName, PickUpMessageType pickupItem)
	{
		if (IsSupportedPickupType(pickupItem))
		{
			OnPickupItem(itemName, pickupItem);
		}
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_pickUpText);
		_pickUpText.Color = ColorConverter.RgbToColor(255f, 248f, 192f);
		_pickUpText.ShadowColorAnim.Alpha = 0f;
	}

	private void ResetTransform()
	{
		_curScaleFactor = 0.45f;
		_pickUpText.Scale = new Vector2(_curScaleFactor, _curScaleFactor);
		_pickUpText.Position = new Vector2(Screen.width / 2, (float)Screen.height * 0.6f);
	}

	private bool IsSupportedPickupType(PickUpMessageType selectedItem)
	{
		return selectedItem != PickUpMessageType.None;
	}

	private void OnPickupItem(string itemName, PickUpMessageType pickupItem)
	{
		if (IsComboPickup(pickupItem))
		{
			_samePickUpCount++;
			_pickUpText.Text = GetComboPickupString(itemName);
		}
		else
		{
			_pickUpText.Text = itemName;
			_samePickUpCount = 1;
		}
		_lastPickUpType = pickupItem;
		if (_displayAnim.IsAnimating)
		{
			_displayAnim.Stop();
		}
		ResetStyle();
		_displayAnim.Start();
	}

	private string GetComboPickupString(string itemName)
	{
		return string.Format("{0} x {1}", itemName, _samePickUpCount.ToString());
	}

	private bool IsComboPickup(PickUpMessageType selectedItem)
	{
		return _lastPickUpType == selectedItem && _displayAnim.IsAnimating && _samePickUpCount > 0;
	}
}
