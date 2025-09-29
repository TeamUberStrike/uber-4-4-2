using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class ScreenshotHud : Singleton<ScreenshotHud>
{
	private MeshGUITextFormat _helpText;

	public bool Enable
	{
		get
		{
			return _helpText.IsVisible;
		}
		set
		{
			if (value)
			{
				_helpText.Show();
			}
			else
			{
				_helpText.Hide();
			}
		}
	}

	private ScreenshotHud()
	{
		_helpText = new MeshGUITextFormat("Screenshot mode\nPress [P] to enable HUD", HudAssets.Instance.InterparkBitmapFont, TextAlignment.Right, Singleton<HudStyleUtility>.Instance.SetNoShadowStyle);
		ResetTransform();
		Enable = false;
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
	}

	public void Draw()
	{
		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.P && GameState.CurrentGame.IsMatchRunning && GameState.LocalCharacter.IsAlive)
		{
			Singleton<HudDrawFlagGroup>.Instance.IsScreenshotMode = !Singleton<HudDrawFlagGroup>.Instance.IsScreenshotMode;
		}
		if (Enable != Singleton<HudDrawFlagGroup>.Instance.IsScreenshotMode)
		{
			Enable = Singleton<HudDrawFlagGroup>.Instance.IsScreenshotMode;
		}
		_helpText.Draw();
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		float num = 0.2f;
		_helpText.Scale = new Vector2(num, num);
		_helpText.LineGap = _helpText.Rect.height * 0.1f;
		_helpText.Position = new Vector2((float)Screen.width * 0.99f, (float)Screen.height * 0.99f - _helpText.Rect.height);
	}
}
