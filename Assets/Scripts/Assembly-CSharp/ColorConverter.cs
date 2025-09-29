using System.Globalization;
using UnityEngine;

public static class ColorConverter
{
	public static float GetHue(Color c)
	{
		float num = 0f;
		if (c.r == 0f)
		{
			num = 2f;
			return num + ((!(c.b < 1f)) ? (2f - c.g) : c.b);
		}
		if (c.g == 0f)
		{
			num = 4f;
			return num + ((!(c.r < 1f)) ? (2f - c.b) : c.r);
		}
		num = 0f;
		return num + ((!(c.g < 1f)) ? (2f - c.r) : c.g);
	}

	public static Color GetColor(float hue)
	{
		hue %= 6f;
		Color white = Color.white;
		return (hue < 1f) ? new Color(1f, hue, 0f) : ((hue < 2f) ? new Color(2f - hue, 1f, 0f) : ((hue < 3f) ? new Color(0f, 1f, hue - 2f) : ((hue < 4f) ? new Color(0f, 4f - hue, 1f) : ((!(hue < 5f)) ? new Color(1f, 0f, 6f - hue) : new Color(hue - 4f, 0f, 1f)))));
	}

	public static Color HexToColor(string hexString)
	{
		int num;
		try
		{
			num = int.Parse(hexString.Substring(0, 2), NumberStyles.HexNumber);
		}
		catch
		{
			num = 255;
		}
		int num2;
		try
		{
			num2 = int.Parse(hexString.Substring(2, 2), NumberStyles.HexNumber);
		}
		catch
		{
			num2 = 255;
		}
		int num3;
		try
		{
			num3 = int.Parse(hexString.Substring(4, 2), NumberStyles.HexNumber);
		}
		catch
		{
			num3 = 255;
		}
		return new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f);
	}

	public static string ColorToHex(Color color)
	{
		string text = ((int)(color.r * 255f)).ToString("X2");
		string text2 = ((int)(color.g * 255f)).ToString("X2");
		string text3 = ((int)(color.b * 255f)).ToString("X2");
		return text + text2 + text3;
	}

	public static Color RgbToColor(float r, float g, float b)
	{
		return new Color(r / 255f, g / 255f, b / 255f);
	}

	public static Color RgbaToColor(float r, float g, float b, float a)
	{
		return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static HsvColor RgbToHsv(Color color)
	{
		HsvColor result = new HsvColor(0f, 0f, 0f, color.a);
		float r = color.r;
		float g = color.g;
		float b = color.b;
		float num = Mathf.Max(r, Mathf.Max(g, b));
		if (num <= 0f)
		{
			return result;
		}
		float num2 = Mathf.Min(r, Mathf.Min(g, b));
		float num3 = num - num2;
		if (num > num2)
		{
			if (g == num)
			{
				result.h = (b - r) / num3 * 60f + 120f;
			}
			else if (b == num)
			{
				result.h = (r - g) / num3 * 60f + 240f;
			}
			else if (b > g)
			{
				result.h = (g - b) / num3 * 60f + 360f;
			}
			else
			{
				result.h = (g - b) / num3 * 60f;
			}
			if (result.h < 0f)
			{
				result.h += 360f;
			}
		}
		else
		{
			result.h = 0f;
		}
		result.h *= 0.0027777778f;
		result.s = num3 / num * 1f;
		result.v = num;
		return result;
	}

	public static Color HsvToRgb(HsvColor color)
	{
		return HsvToRgb(color.h, color.s, color.v, color.a);
	}

	public static Color HsvToRgb(float hue, float saturation, float value)
	{
		return HsvToRgb(hue, saturation, value, 1f);
	}

	public static Color HsvToRgb(float hue, float saturation, float value, float alpha)
	{
		float value2 = value;
		float value3 = value;
		float value4 = value;
		if (saturation != 0f)
		{
			float num = value * saturation;
			float num2 = value - num;
			float num3 = hue * 360f;
			if (num3 < 60f)
			{
				value2 = value;
				value3 = num3 * num / 60f + num2;
				value4 = num2;
			}
			else if (num3 < 120f)
			{
				value2 = (0f - (num3 - 120f)) * num / 60f + num2;
				value3 = value;
				value4 = num2;
			}
			else if (num3 < 180f)
			{
				value2 = num2;
				value3 = value;
				value4 = (num3 - 120f) * num / 60f + num2;
			}
			else if (num3 < 240f)
			{
				value2 = num2;
				value3 = (0f - (num3 - 240f)) * num / 60f + num2;
				value4 = value;
			}
			else if (num3 < 300f)
			{
				value2 = (num3 - 240f) * num / 60f + num2;
				value3 = num2;
				value4 = value;
			}
			else if (num3 <= 360f)
			{
				value2 = value;
				value3 = num2;
				value4 = (0f - (num3 - 360f)) * num / 60f + num2;
			}
			else
			{
				value2 = 0f;
				value3 = 0f;
				value4 = 0f;
			}
		}
		return new Color(Mathf.Clamp01(value2), Mathf.Clamp01(value3), Mathf.Clamp01(value4), alpha);
	}
}
