using System.Collections;
using UnityEngine;

public class LevelUpHud : Singleton<LevelUpHud>
{
	private MeshGUIText _levelUpText;

	private MeshGUIQuad _glowBlur;

	private Animatable2DGroup _entireGroup;

	private Vector2 _spawnPosition;

	private Vector2 _destBlurScale;

	private Vector2 _destTextScale;

	private Color _color;

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_entireGroup.Show();
			}
			else
			{
				_entireGroup.Hide();
			}
		}
	}

	private LevelUpHud()
	{
		_color = new Color(78f / 85f, 0.78039217f, 39f / 85f);
		_levelUpText = new MeshGUIText("LEVEL UP", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_glowBlur = new MeshGUIQuad(HudTextures.WhiteBlur128);
		_glowBlur.Name = "LevelUpGlow";
		_glowBlur.Color = _color;
		_glowBlur.Depth = 1f;
		_entireGroup = new Animatable2DGroup();
		_entireGroup.Group.Add(_levelUpText);
		_entireGroup.Group.Add(_glowBlur);
		ResetHud();
		Enabled = true;
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	public void PopupLevelUp()
	{
		ResetTransform();
		MonoRoutine.Start(EmitLevelUp());
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetDefaultStyle(_levelUpText);
		_levelUpText.BitmapMeshText.ShadowColor = _color;
	}

	private void ResetTransform()
	{
		_spawnPosition = new Vector2(Screen.width / 2, (float)(Screen.height / 2) - 40f);
		ResetTextTransform();
		ResetBlurTransform();
		_levelUpText.Scale = Vector2.zero;
	}

	private void ResetTextTransform()
	{
		_destTextScale = new Vector2(1.5f, 1.5f);
		_levelUpText.Position = _spawnPosition;
		_levelUpText.Scale = _destTextScale;
		_levelUpText.Alpha = 0f;
	}

	private void ResetBlurTransform()
	{
		float num = _levelUpText.Size.x * HudStyleUtility.BLUR_WIDTH_SCALE_FACTOR;
		float num2 = _levelUpText.Size.y * HudStyleUtility.BLUR_HEIGHT_SCALE_FACTOR;
		_destBlurScale = new Vector2(num / (float)HudTextures.WhiteBlur128.width, num2 / (float)HudTextures.WhiteBlur128.height);
		_glowBlur.Position = _spawnPosition;
		_glowBlur.Scale = Vector2.zero;
		_glowBlur.Alpha = 0f;
	}

	private IEnumerator EmitLevelUp()
	{
		float popupTime = 0.1f;
		float sizeIncreaseTime = 5f;
		float zoomTime = 0.1f;
		float sizeIncreaseScale = 1.1f;
		_entireGroup.FadeAlphaTo(1f, popupTime, EaseType.Berp);
		_levelUpText.ScaleTo(_destTextScale, popupTime, EaseType.Berp);
		_glowBlur.ScaleToAroundPivot(_destBlurScale, _spawnPosition, popupTime, EaseType.Berp);
		yield return new WaitForSeconds(popupTime);
		_entireGroup.ScaleAroundPivot(new Vector2(sizeIncreaseScale, sizeIncreaseScale), _spawnPosition, sizeIncreaseTime, EaseType.Out);
		yield return new WaitForSeconds(sizeIncreaseTime);
		_entireGroup.ScaleAroundPivot(new Vector2(5f, 5f), _spawnPosition, zoomTime);
		yield return new WaitForSeconds(zoomTime);
		_entireGroup.FadeAlphaTo(0f, 0.5f, EaseType.Out);
	}
}
