using UnityEngine;

[AddComponentMenu("Image Effects/Silhouette Outlined")]
[ExecuteInEditMode]
public class SilhouetteOutlined : ImageEffectBase
{
	[SerializeField]
	private Shader generateGlowTextureShader;

	[SerializeField]
	private Shader gaussianBlurShader;

	[SerializeField]
	private Shader objectMaskShader;

	[SerializeField]
	private Color globalOutlineColor;

	[SerializeField]
	private bool isUseGlobalColor;

	private Material _gaussianBlurMaterial;

	private RenderTexture _glowTexture;

	private RenderTexture _maskTexture;

	private GameObject _shaderCamera;

	private int _glowTexWidth;

	private int _glowTexHeight;

	protected Material GaussianBlurMaterial
	{
		get
		{
			if (_gaussianBlurMaterial == null)
			{
				_gaussianBlurMaterial = new Material(gaussianBlurShader);
				_gaussianBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return _gaussianBlurMaterial;
		}
	}

	protected GameObject ShaderCamera
	{
		get
		{
			if (!_shaderCamera)
			{
				_shaderCamera = new GameObject("ShaderCamera", typeof(Camera));
				_shaderCamera.GetComponent<Camera>().enabled = false;
				_shaderCamera.hideFlags = HideFlags.HideAndDontSave;
			}
			return _shaderCamera;
		}
	}

	private new void OnDisable()
	{
		base.OnDisable();
		Object.DestroyImmediate(_shaderCamera);
		if (_glowTexture != null)
		{
			RenderTexture.ReleaseTemporary(_glowTexture);
			_glowTexture = null;
		}
	}

	private void OnPreRender()
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			CleanRenderTextures();
			Camera camera = ShaderCamera.GetComponent<Camera>();
			camera.CopyFrom(base.GetComponent<Camera>());
			camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			camera.clearFlags = CameraClearFlags.Color;
			_maskTexture = RenderTexture.GetTemporary((int)base.GetComponent<Camera>().pixelWidth, (int)base.GetComponent<Camera>().pixelHeight, 16);
			camera.targetTexture = _maskTexture;
			camera.RenderWithShader(objectMaskShader, "Outline");
			UpdateGlowTextureSize(base.GetComponent<Camera>().pixelWidth, base.GetComponent<Camera>().pixelHeight);
			_glowTexture = RenderTexture.GetTemporary(_glowTexWidth, _glowTexHeight, 16);
			camera.targetTexture = _glowTexture;
			camera.RenderWithShader(generateGlowTextureShader, "Outline");
		}
	}

	private void UpdateGlowTextureSize(float cameraWidth, float cameraHeight)
	{
		_glowTexWidth = (int)cameraWidth;
		_glowTexHeight = (int)cameraHeight;
		float num = cameraWidth / cameraHeight;
		if (cameraWidth > 256f && cameraWidth < 512f)
		{
			_glowTexWidth = 256;
			_glowTexHeight = (int)((float)_glowTexWidth / num);
		}
		if (cameraWidth > 512f)
		{
			_glowTexWidth = 512;
			_glowTexHeight = (int)((float)_glowTexWidth / num);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		GaussianBlur(source, destination);
		CleanRenderTextures();
	}

	private void GaussianBlur(RenderTexture source, RenderTexture dest)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
		GaussianBlurMaterial.SetFloat("_TexWidth", _glowTexWidth);
		GaussianBlurMaterial.SetFloat("_TexHeight", _glowTexHeight);
		Graphics.Blit(_glowTexture, temporary, GaussianBlurMaterial, 0);
		Graphics.Blit(temporary, _glowTexture, GaussianBlurMaterial, 1);
		RenderTexture.ReleaseTemporary(temporary);
		base.material.SetTexture("_GlowTex", _glowTexture);
		base.material.SetTexture("_MaskTex", _maskTexture);
		base.material.SetFloat("_IsUseGlobalColor", (!isUseGlobalColor) ? 0f : 1f);
		base.material.SetColor("_GlobalOutlineColor", globalOutlineColor);
		Graphics.Blit(source, dest, base.material);
	}

	private void NoBlur(RenderTexture source, RenderTexture dest)
	{
		base.material.SetTexture("_GlowTex", _glowTexture);
		base.material.SetTexture("_MaskTex", _maskTexture);
		base.material.SetFloat("_IsUseGlobalColor", (!isUseGlobalColor) ? 0f : 1f);
		base.material.SetColor("_GlobalOutlineColor", globalOutlineColor);
		Graphics.Blit(source, dest, base.material);
	}

	private void CleanRenderTextures()
	{
		if (_glowTexture != null)
		{
			RenderTexture.ReleaseTemporary(_glowTexture);
			_glowTexture = null;
		}
		if (_maskTexture != null)
		{
			RenderTexture.ReleaseTemporary(_maskTexture);
			_maskTexture = null;
		}
	}
}
