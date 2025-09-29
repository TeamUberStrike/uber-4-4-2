using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class HudStyleUtility : Singleton<HudStyleUtility>
{
	public static Color DEFAULT_BLUE_COLOR = new Color(16f / 85f, 49f / 85f, 37f / 51f);

	public static Color DEFAULT_RED_COLOR = new Color(57f / 85f, 0.18039216f, 7f / 51f);

	public static float BLUR_WIDTH_SCALE_FACTOR = 3f;

	public static float BLUR_HEIGHT_SCALE_FACTOR = 3.2f;

	public static float XP_BAR_WIDTH_PROPORTION_IN_SCREEN = 0.15f;

	public static float ACRONYM_TEXT_SCALE = 0.25f;

	public static float BIGGER_DIGITS_SCALE = 1f;

	public static float SMALLER_DIGITS_SCALE = 0.7f;

	public static float GAP_BETWEEN_TEXT = 3f;

	public static Color GLOW_BLUR_BLUE_COLOR = new Color(0f, 53f / 85f, 1f);

	public static Color GLOW_BLUR_RED_COLOR = new Color(0.72156864f, 10f / 51f, 0.16862746f);

	public Color TeamTextColor { get; private set; }

	public Color TeamGlowColor { get; private set; }

	private HudStyleUtility()
	{
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
	}

	public void SetTeamStyle(MeshGUIText meshText3D)
	{
		SetDefaultStyle(meshText3D);
		meshText3D.BitmapMeshText.ShadowColor = TeamTextColor;
	}

	public void SetDefaultStyle(MeshGUIText meshText3D)
	{
		meshText3D.Color = Color.white;
		meshText3D.BitmapMeshText.ShadowColor = DEFAULT_BLUE_COLOR;
		meshText3D.BitmapMeshText.AlphaMin = 0.45f;
		meshText3D.BitmapMeshText.AlphaMax = 0.62f;
		meshText3D.BitmapMeshText.ShadowAlphaMin = 0.2f;
		meshText3D.BitmapMeshText.ShadowAlphaMax = 0.45f;
		meshText3D.BitmapMeshText.ShadowOffsetU = 0f;
		meshText3D.BitmapMeshText.ShadowOffsetV = 0f;
	}

	public void SetSamllTextStyle(MeshGUIText meshText3D)
	{
		meshText3D.Color = Color.white;
		meshText3D.BitmapMeshText.ShadowColor = DEFAULT_BLUE_COLOR;
		meshText3D.BitmapMeshText.AlphaMin = 0.4f;
		meshText3D.BitmapMeshText.AlphaMax = 0.62f;
		meshText3D.BitmapMeshText.ShadowAlphaMin = 0.2f;
		meshText3D.BitmapMeshText.ShadowAlphaMax = 0.45f;
		meshText3D.BitmapMeshText.ShadowOffsetU = 0f;
		meshText3D.BitmapMeshText.ShadowOffsetV = 0f;
	}

	public void SetBlueStyle(MeshGUIText meshText3D)
	{
		SetDefaultStyle(meshText3D);
	}

	public void SetRedStyle(MeshGUIText meshText3D)
	{
		SetDefaultStyle(meshText3D);
		meshText3D.BitmapMeshText.ShadowColor = DEFAULT_RED_COLOR;
	}

	public void SetNoShadowStyle(MeshGUIText meshText3D)
	{
		meshText3D.Color = Color.white;
		meshText3D.BitmapMeshText.ShadowColor = new Color(1f, 1f, 1f, 0f);
		meshText3D.BitmapMeshText.AlphaMin = 0.18f;
		meshText3D.BitmapMeshText.AlphaMax = 0.62f;
		meshText3D.BitmapMeshText.ShadowAlphaMin = 0.18f;
		meshText3D.BitmapMeshText.ShadowAlphaMax = 0.39f;
		meshText3D.BitmapMeshText.ShadowOffsetU = 0f;
		meshText3D.BitmapMeshText.ShadowOffsetV = 0f;
	}

	public void SetBlackStyle(MeshGUIText meshText3D)
	{
		meshText3D.Color = Color.white;
		meshText3D.BitmapMeshText.ShadowColor = Color.black;
		meshText3D.BitmapMeshText.AlphaMin = 0.45f;
		meshText3D.BitmapMeshText.AlphaMax = 0.62f;
		meshText3D.BitmapMeshText.ShadowAlphaMin = 0.2f;
		meshText3D.BitmapMeshText.ShadowAlphaMax = 0.45f;
		meshText3D.BitmapMeshText.ShadowOffsetU = 0f;
		meshText3D.BitmapMeshText.ShadowOffsetV = 0f;
	}

	public void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		TeamTextColor = Color.white;
		switch (ev.TeamId)
		{
		case TeamID.NONE:
		case TeamID.BLUE:
			TeamTextColor = DEFAULT_BLUE_COLOR;
			TeamGlowColor = GLOW_BLUR_BLUE_COLOR;
			break;
		case TeamID.RED:
			TeamTextColor = DEFAULT_RED_COLOR;
			TeamGlowColor = GLOW_BLUR_RED_COLOR;
			break;
		}
	}

	public void ResetOverlayBoxTransform(Sprite2DGUI boxOverlay, Rect rect, Vector2 scaleFactor)
	{
		float num = rect.width * scaleFactor.x;
		float num2 = rect.height * scaleFactor.y;
		Vector2 scale = new Vector2(num / boxOverlay.GUIBounds.x, num2 / boxOverlay.GUIBounds.y);
		boxOverlay.Scale = scale;
		boxOverlay.Position = new Vector2(rect.x - num / 2f, rect.y - num2 / 2f);
	}
}
