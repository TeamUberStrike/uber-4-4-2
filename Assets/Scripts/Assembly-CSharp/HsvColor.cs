using UnityEngine;

public struct HsvColor
{
	public float h;

	public float s;

	public float v;

	public float a;

	public HsvColor(float h, float s, float b, float a)
	{
		this.h = h;
		this.s = s;
		v = b;
		this.a = a;
	}

	public HsvColor(float h, float s, float b)
	{
		this.h = h;
		this.s = s;
		v = b;
		a = 1f;
	}

	public HsvColor(Color col)
	{
		HsvColor hsvColor = ColorConverter.RgbToHsv(col);
		h = hsvColor.h;
		s = hsvColor.s;
		v = hsvColor.v;
		a = hsvColor.a;
	}

	public static HsvColor FromColor(Color color)
	{
		return ColorConverter.RgbToHsv(color);
	}

	public Color ToColor()
	{
		return ColorConverter.HsvToRgb(this);
	}

	public HsvColor Add(float hue, float saturation, float value)
	{
		h += hue;
		s += saturation;
		v += value;
		while (h > 1f)
		{
			h -= 1f;
		}
		while (h < 0f)
		{
			h += 1f;
		}
		return this;
	}

	public HsvColor AddHue(float hue)
	{
		h += hue;
		while (h > 1f)
		{
			h -= 1f;
		}
		while (h < 0f)
		{
			h += 1f;
		}
		return this;
	}

	public HsvColor AddSaturation(float saturation)
	{
		s += saturation;
		return this;
	}

	public HsvColor AddValue(float value)
	{
		v += value;
		return this;
	}
}
