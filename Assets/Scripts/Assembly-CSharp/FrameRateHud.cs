using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class FrameRateHud : Singleton<FrameRateHud>
{
	private MeshGUIText _frameRateText;

	public bool Enable
	{
		get
		{
			return _frameRateText.IsVisible;
		}
		set
		{
			if (ApplicationDataManager.IsMobile || !ApplicationDataManager.ApplicationOptions.VideoShowFps)
			{
				value = false;
			}
			if (value)
			{
				_frameRateText.Show();
			}
			else
			{
				_frameRateText.Hide();
			}
		}
	}

	private FrameRateHud()
	{
		_frameRateText = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.UpperRight);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_frameRateText);
		ResetTransform();
		Enable = false;
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
	}

	public void Draw()
	{
		string frameRate = ApplicationDataManager.FrameRate;
		if (_frameRateText.Text != frameRate)
		{
			_frameRateText.Text = frameRate;
		}
		_frameRateText.Draw();
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		float num = 0.2f;
		_frameRateText.Scale = new Vector2(num, num);
		_frameRateText.Position = new Vector2((float)Screen.width * 0.99f, (float)Screen.height * 0.01f);
	}
}
