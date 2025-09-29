using System;
using UnityEngine;

public static class Texture2DExtension
{
	public static Texture2D ChangeHSV(this Texture2D texture, float hue, float saturation = 0f, float value = 0f)
	{
		if (IsSupported(texture.format))
		{
			try
			{
				Texture2D texture2D = new Texture2D(texture.width, texture.height, texture.format, false);
				Color[] pixels = texture.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i] = HsvColor.FromColor(pixels[i]).Add(hue, saturation, value).ToColor();
				}
				texture2D.SetPixels(pixels);
				texture2D.Apply();
				return texture2D;
			}
			catch (Exception ex)
			{
				Debug.LogError("ChangeHue failed on '" + texture.name + "' with Exception: " + ex.Message);
				return texture;
			}
		}
		Debug.LogError("ChangeHue failed on '" + texture.name + "' because texture format not supported: " + texture.format);
		return texture;
	}

	private static bool IsSupported(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
		case TextureFormat.RGB24:
		case TextureFormat.RGBA32:
		case TextureFormat.ARGB32:
			return true;
		default:
			return false;
		}
	}
}
