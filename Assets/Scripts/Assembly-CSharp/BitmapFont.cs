using System.Collections.Generic;
using UnityEngine;

public class BitmapFont : MonoBehaviour
{
	public Color Color = Color.white;

	public float AlphaMin = 0.49f;

	public float AlphaMax = 0.51f;

	public Color ShadowColor = Color.black;

	public float ShadowAlphaMin = 0.28f;

	public float ShadowAlphaMax = 0.49f;

	public Vector2 ShadowOffset = new Vector2(-0.05f, 0.08f);

	public float Size;

	public float LineHeight;

	public float Base;

	public float ScaleW;

	public float ScaleH;

	public BitmapChar[] Chars;

	public Rect[] PageOffsets;

	public Texture2D PageAtlas;

	public BitmapCharKerning[] Kernings;

	private Material pageMaterial;

	private Dictionary<int, Material> fontMaterials = new Dictionary<int, Material>();

	public BitmapChar GetBitmapChar(int c)
	{
		BitmapChar[] chars = Chars;
		foreach (BitmapChar bitmapChar in chars)
		{
			if (c == bitmapChar.Id)
			{
				return bitmapChar;
			}
		}
		return Chars[0];
	}

	public Rect GetUVRect(int c)
	{
		BitmapChar bitmapChar = GetBitmapChar(c);
		return GetUVRect(bitmapChar);
	}

	public Rect GetUVRect(BitmapChar bitmapChar)
	{
		Vector2 vector = new Vector2(bitmapChar.Size.x / ScaleW, bitmapChar.Size.y / ScaleH);
		Vector2 vector2 = new Vector2(bitmapChar.Position.x / ScaleW, bitmapChar.Position.y / ScaleH);
		Vector2 vector3 = new Vector2(vector2.x, 1f - (vector2.y + vector.y));
		Rect rect = PageOffsets[bitmapChar.Page];
		vector3 = new Vector2(vector3.x * rect.width + rect.xMin, vector3.y * rect.height + rect.yMin);
		vector = new Vector2(vector.x * rect.width, vector.y * rect.height);
		return new Rect(vector3.x, vector3.y, vector.x, vector.y);
	}

	public Material CreateFontMaterial(Shader shader)
	{
		return new Material(shader);
	}

	public void UpdateFontMaterial(Material fontMaterial)
	{
		fontMaterial.color = Color;
		fontMaterial.mainTexture = PageAtlas;
		fontMaterial.SetFloat("_AlphaMin", AlphaMin);
		fontMaterial.SetFloat("_AlphaMax", AlphaMax);
		fontMaterial.SetColor("_ShadowColor", ShadowColor);
		fontMaterial.SetFloat("_ShadowAlphaMin", ShadowAlphaMin);
		fontMaterial.SetFloat("_ShadowAlphaMax", ShadowAlphaMax);
		fontMaterial.SetFloat("_ShadowOffsetU", ShadowOffset.x);
		fontMaterial.SetFloat("_ShadowOffsetV", ShadowOffset.y);
	}

	public Material GetPageMaterial(int page, Shader shader)
	{
		if (pageMaterial == null)
		{
			pageMaterial = CreateFontMaterial(shader);
		}
		UpdateFontMaterial(pageMaterial);
		return pageMaterial;
	}

	public Material GetCharacterMaterial(int c, Shader shader)
	{
		if (!fontMaterials.ContainsKey(c) || fontMaterials[c] == null)
		{
			Material material = CreateFontMaterial(shader);
			BitmapChar bitmapChar = GetBitmapChar(c);
			Rect uVRect = GetUVRect(bitmapChar);
			material.mainTextureScale = new Vector2(uVRect.width, uVRect.height);
			material.mainTextureOffset = new Vector2(uVRect.xMin, uVRect.yMin);
			fontMaterials[c] = material;
		}
		Material material2 = fontMaterials[c];
		UpdateFontMaterial(material2);
		return material2;
	}

	public float GetKerning(char first, char second)
	{
		if (Kernings != null)
		{
			BitmapCharKerning[] kernings = Kernings;
			foreach (BitmapCharKerning bitmapCharKerning in kernings)
			{
				if (bitmapCharKerning.FirstChar == first && bitmapCharKerning.SecondChar == second)
				{
					return bitmapCharKerning.Amount;
				}
			}
		}
		return 0f;
	}

	public Vector2 CalculateSize(string str, Vector2 renderSize)
	{
		Vector2 result = new Vector2(0f, renderSize.y);
		Vector2 vector = renderSize / Size;
		int num = 0;
		while (!string.IsNullOrEmpty(str) && num < str.Length)
		{
			char c = str[num];
			BitmapChar bitmapChar = GetBitmapChar(c);
			float num2 = 0f;
			if (num < str.Length - 1)
			{
				num2 = GetKerning(c, str[num + 1]);
			}
			result.x += (bitmapChar.XAdvance + num2) * vector.x;
			num++;
		}
		return result;
	}

	public Vector2 CalculateSize(string str, float renderSize)
	{
		return CalculateSize(str, new Vector2(renderSize, renderSize));
	}
}
