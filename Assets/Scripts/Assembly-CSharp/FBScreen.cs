using UnityEngine;

public class FBScreen
{
	private static bool resizable;

	public static bool FullScreen
	{
		get
		{
			return Screen.fullScreen;
		}
		set
		{
			Screen.fullScreen = value;
		}
	}

	public static bool Resizable
	{
		get
		{
			return resizable;
		}
	}

	public static int Width
	{
		get
		{
			return Screen.width;
		}
	}

	public static int Height
	{
		get
		{
			return Screen.height;
		}
	}

	public static void SetResolution(int width, int height, bool fullscreen, int preferredRefreshRate = 0)
	{
		Screen.SetResolution(width, height, fullscreen, preferredRefreshRate);
	}

	public static void SetAspectRatio(int width, int height)
	{
		int width2 = Screen.height / height * width;
		Screen.SetResolution(width2, Screen.height, Screen.fullScreen);
	}
}
