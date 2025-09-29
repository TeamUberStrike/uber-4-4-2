using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CameraRectController : AutoMonoBehaviour<CameraRectController>
{
	private float lastWidth = 1f;

	private Vector2 screenSize;

	public float PixelWidth
	{
		get
		{
			if (GameState.CurrentSpace != null && GameState.CurrentSpace.Camera != null)
			{
				return GameState.CurrentSpace.Camera.pixelWidth;
			}
			return Screen.width;
		}
	}

	public float NormalizedWidth
	{
		get
		{
			return PixelWidth / (float)Screen.width;
		}
	}

	private void LateUpdate()
	{
		if ((float)Screen.width != screenSize.x || (float)Screen.height != screenSize.y)
		{
			screenSize.x = Screen.width;
			screenSize.y = Screen.height;
			CmuneEventHandler.Route(new ScreenResolutionEvent());
		}
		if (GameState.CurrentSpace != null && GameState.CurrentSpace.Camera != null && GameState.CurrentSpace.Camera.pixelWidth != lastWidth)
		{
			lastWidth = GameState.CurrentSpace.Camera.pixelWidth;
			CmuneEventHandler.Route(new CameraWidthChangeEvent());
		}
	}

	public void SetAbsoluteWidth(float width)
	{
		SetNormalizedWidth(width / (float)Screen.width);
	}

	public void SetNormalizedWidth(float width)
	{
		width = Mathf.Clamp(width, 0f, 1f);
		if (GameState.CurrentSpace != null && GameState.CurrentSpace.Camera != null)
		{
			GameState.CurrentSpace.Camera.rect = new Rect(0f, 0f, width, 1f);
		}
	}
}
