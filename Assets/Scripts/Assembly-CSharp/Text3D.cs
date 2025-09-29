using UnityEngine;

public class Text3D : BitmapMeshText
{
	public enum StyleType
	{
		Default = 0,
		Custom = 1,
		NoBorder = 2
	}

	[SerializeField]
	private string _textContent = "Cmune";

	[SerializeField]
	private BitmapFont _font;

	[SerializeField]
	private Color _innerColor = Color.black;

	[SerializeField]
	private Color _borderColor = Color.white;

	[SerializeField]
	private StyleType _textStyle;

	private void Awake()
	{
		base.Font = _font;
		base.Anchor = TextAnchor.MiddleCenter;
		base.Text = _textContent;
		switch (_textStyle)
		{
		case StyleType.Default:
			SetDefaultStyle();
			break;
		case StyleType.NoBorder:
			SetNoShadowStyle();
			break;
		case StyleType.Custom:
			SetCustomStyle();
			break;
		}
	}

	private void SetCustomStyle()
	{
		Color = _innerColor;
		base.ShadowColor = _borderColor;
		base.AlphaMin = 0.45f;
		base.AlphaMax = 0.62f;
		base.ShadowAlphaMin = 0.2f;
		base.ShadowAlphaMax = 0.45f;
		base.ShadowOffsetU = 0f;
		base.ShadowOffsetV = 0f;
	}

	private void SetDefaultStyle()
	{
		Color = Color.white;
		base.ShadowColor = HudStyleUtility.DEFAULT_BLUE_COLOR;
		base.AlphaMin = 0.45f;
		base.AlphaMax = 0.62f;
		base.ShadowAlphaMin = 0.2f;
		base.ShadowAlphaMax = 0.45f;
		base.ShadowOffsetU = 0f;
		base.ShadowOffsetV = 0f;
	}

	private void SetNoShadowStyle()
	{
		Color = _innerColor;
		base.ShadowColor = new Color(1f, 1f, 1f, 0f);
		base.AlphaMin = 0.18f;
		base.AlphaMax = 0.62f;
		base.ShadowAlphaMin = 0.18f;
		base.ShadowAlphaMax = 0.39f;
		base.ShadowOffsetU = 0f;
		base.ShadowOffsetV = 0f;
	}
}
