using System.Collections.Generic;
using UnityEngine;

public class MeshGUITextFormat : Animatable2DGroup
{
	public delegate void SetStyle(MeshGUIText meshText);

	private string _formatText;

	private BitmapFont _bitmapFont;

	private TextAnchor _textAnchor;

	private Vector2Anim _scaleAnim;

	private ColorAnim _shadowColorAnim;

	private float _verticalGapBetweenLines;

	private SetStyle _setStyleFunc;

	public string FormatText
	{
		get
		{
			return _formatText;
		}
		set
		{
			_formatText = value;
			UpdateMeshTextGroup();
		}
	}

	public Vector2 Scale
	{
		get
		{
			return _scaleAnim.Vec2;
		}
		set
		{
			_scaleAnim.Vec2 = value;
		}
	}

	public float LineGap
	{
		get
		{
			return _verticalGapBetweenLines;
		}
		set
		{
			_verticalGapBetweenLines = value;
			ResetTransform();
		}
	}

	public float LineHeight
	{
		get
		{
			if (base.Group.Count > 0)
			{
				return (base.Group[0] as MeshGUIText).Size.y;
			}
			return 0f;
		}
	}

	public ColorAnim ShadowColorAnim
	{
		get
		{
			if (_shadowColorAnim == null)
			{
				_shadowColorAnim = new ColorAnim(OnShadowColorChange);
			}
			return _shadowColorAnim;
		}
	}

	public SetStyle SetStyleFunc
	{
		set
		{
			_setStyleFunc = value;
		}
	}

	public MeshGUITextFormat(string formatText, BitmapFont bitmapFont, TextAlignment textAlignment = TextAlignment.Left, SetStyle setTyleFunc = null)
	{
		_bitmapFont = bitmapFont;
		_setStyleFunc = setTyleFunc;
		SetTextAlignment(textAlignment);
		_scaleAnim = new Vector2Anim(OnScaleChange);
		_scaleAnim.Vec2 = Vector2.one;
		_setStyleFunc = setTyleFunc;
		FormatText = formatText;
	}

	public void UpdateStyle()
	{
		for (int i = 0; i < base.Group.Count; i++)
		{
			MeshGUIText meshGUIText = base.Group[i] as MeshGUIText;
			if (meshGUIText != null && _setStyleFunc != null)
			{
				_setStyleFunc(meshGUIText);
			}
		}
	}

	private void SetTextAlignment(TextAlignment textAlignment)
	{
		switch (textAlignment)
		{
		case TextAlignment.Left:
			_textAnchor = TextAnchor.UpperLeft;
			break;
		case TextAlignment.Center:
			_textAnchor = TextAnchor.UpperCenter;
			break;
		case TextAlignment.Right:
			_textAnchor = TextAnchor.UpperRight;
			break;
		}
	}

	private void UpdateMeshTextGroup()
	{
		ClearAndFree();
		List<string> stringArray = GetStringArray(_formatText);
		foreach (string item in stringArray)
		{
			MeshGUIText meshGUIText = new MeshGUIText(item, _bitmapFont, _textAnchor);
			if (_setStyleFunc != null)
			{
				_setStyleFunc(meshGUIText);
			}
			base.Group.Add(meshGUIText);
		}
		ResetTransform();
	}

	private List<string> GetStringArray(string formatStr)
	{
		List<string> list = new List<string>();
		string text = formatStr;
		while (true)
		{
			int num = text.IndexOf('\n');
			if (num == -1)
			{
				break;
			}
			list.Add(text.Substring(0, num));
			text = text.Substring(num + 1);
		}
		if (!string.IsNullOrEmpty(text))
		{
			list.Add(text);
		}
		return list;
	}

	private void OnShadowColorChange(Color oldColor, Color newColor)
	{
		foreach (IAnimatable2D item in base.Group)
		{
			MeshGUIText meshGUIText = item as MeshGUIText;
			meshGUIText.ShadowColorAnim.Color = newColor;
		}
	}

	private void OnScaleChange(Vector2 oldScale, Vector2 newScale)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < base.Group.Count; i++)
		{
			MeshGUIText meshGUIText = base.Group[i] as MeshGUIText;
			if (meshGUIText != null)
			{
				meshGUIText.Scale = _scaleAnim.Vec2;
				meshGUIText.Position = zero;
				zero += new Vector2(0f, meshGUIText.Size.y + _verticalGapBetweenLines);
			}
		}
	}
}
