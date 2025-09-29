using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Mobile Bloom (Simple)")]
[ExecuteInEditMode]
public class MobileBloomSimple : MonoBehaviour
{
	[SerializeField]
	private float _intensity = 0.7f;

	[SerializeField]
	private float _threshhold = 0.75f;

	[SerializeField]
	private float _blurWidth = 1f;

	[SerializeField]
	private bool _extraBlurry;

	[SerializeField]
	private Shader _bloomShader;

	[SerializeField]
	private Material _bloomMaterial;

	[SerializeField]
	private bool _supported;

	private RenderTexture _tempRtA;

	private RenderTexture _tempRtB;

	private void Start()
	{
		CreateMaterials();
		CheckSupport();
	}

	private void CreateMaterials()
	{
		if (!_bloomShader)
		{
			_bloomShader = Shader.Find("Cross Platform Shaders/Mobile Bloom Simple");
		}
		if (!_bloomMaterial)
		{
			_bloomMaterial = new Material(_bloomShader);
			_bloomMaterial.hideFlags = HideFlags.DontSave;
		}
	}

	private bool CheckSupport()
	{
		if (_supported)
		{
			return true;
		}
		// SystemInfo.supportsImageEffects and supportsRenderTextures always return true in Unity 5.0+
		_supported = _bloomMaterial && _bloomMaterial.shader && _bloomMaterial.shader.isSupported;
		return _supported;
	}

	private void CreateBuffers()
	{
		if (!_tempRtA)
		{
			_tempRtA = new RenderTexture(Screen.width / 4, Screen.height / 4, 0);
			_tempRtA.hideFlags = HideFlags.DontSave;
		}
		if (!_tempRtB)
		{
			_tempRtB = new RenderTexture(Screen.width / 4, Screen.height / 4, 0);
			_tempRtB.hideFlags = HideFlags.DontSave;
		}
	}

	private void OnDisable()
	{
		if ((bool)_tempRtA)
		{
			Object.DestroyImmediate(_tempRtA);
			_tempRtA = null;
		}
		if ((bool)_tempRtB)
		{
			Object.DestroyImmediate(_tempRtB);
			_tempRtB = null;
		}
		if ((bool)_bloomMaterial)
		{
			Object.DestroyImmediate(_bloomMaterial);
			_bloomMaterial = null;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		CreateBuffers();
		_bloomMaterial.SetVector("_Parameter", new Vector4(0f, 0f, _threshhold, _intensity / (1f - _threshhold)));
		float num = 1f / ((float)source.width * 1f);
		float num2 = 1f / ((float)source.height * 1f);
		_bloomMaterial.SetVector("_OffsetsA", new Vector4(1.5f * num, 1.5f * num2, -1.5f * num, 1.5f * num2));
		_bloomMaterial.SetVector("_OffsetsB", new Vector4(-1.5f * num, -1.5f * num2, 1.5f * num, -1.5f * num2));
		Graphics.Blit(source, _tempRtB, _bloomMaterial, 1);
		num *= 4f * _blurWidth;
		num2 *= 4f * _blurWidth;
		_bloomMaterial.SetVector("_OffsetsA", new Vector4(1.5f * num, 0f, -1.5f * num, 0f));
		_bloomMaterial.SetVector("_OffsetsB", new Vector4(0.5f * num, 0f, -0.5f * num, 0f));
		Graphics.Blit(_tempRtB, _tempRtA, _bloomMaterial, 2);
		_bloomMaterial.SetVector("_OffsetsA", new Vector4(0f, 1.5f * num2, 0f, -1.5f * num2));
		_bloomMaterial.SetVector("_OffsetsB", new Vector4(0f, 0.5f * num2, 0f, -0.5f * num2));
		Graphics.Blit(_tempRtA, _tempRtB, _bloomMaterial, 2);
		if (_extraBlurry)
		{
			_bloomMaterial.SetVector("_OffsetsA", new Vector4(1.5f * num, 0f, -1.5f * num, 0f));
			_bloomMaterial.SetVector("_OffsetsB", new Vector4(0.5f * num, 0f, -0.5f * num, 0f));
			Graphics.Blit(_tempRtB, _tempRtA, _bloomMaterial, 2);
			_bloomMaterial.SetVector("_OffsetsA", new Vector4(0f, 1.5f * num2, 0f, -1.5f * num2));
			_bloomMaterial.SetVector("_OffsetsB", new Vector4(0f, 0.5f * num2, 0f, -0.5f * num2));
			Graphics.Blit(_tempRtA, _tempRtB, _bloomMaterial, 2);
		}
		_bloomMaterial.SetTexture("_Bloom", _tempRtB);
		Graphics.Blit(source, destination, _bloomMaterial, 0);
	}
}
