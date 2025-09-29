using UnityEngine;

public class LotteryEffect : MonoBehaviour
{
	private const float MAX_DURATION = 2f;

	private const float FADE_TIME = 1.5f;

	private float _time;

	private float _alpha = 1f;

	private float _cameraAlpha;

	private RenderTexture _renderTexture;

	[SerializeField]
	private Camera _renderCamera;

	private void Awake()
	{
		if ((bool)_renderCamera)
		{
			_renderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
			_renderCamera.targetTexture = _renderTexture;
			_cameraAlpha = _renderCamera.backgroundColor.a;
		}
	}

	private void Update()
	{
		if (_time > 1.5f)
		{
			_alpha = Mathf.Clamp01(_alpha - Time.deltaTime);
			_renderCamera.backgroundColor.SetAlpha(Mathf.Min(_cameraAlpha, _alpha));
		}
		if (_time > 2f)
		{
			Object.Destroy(base.gameObject);
			if ((bool)_renderTexture)
			{
				_renderTexture.Release();
			}
		}
		_time += Time.deltaTime;
	}

	private void OnGUI()
	{
		if ((bool)_renderTexture)
		{
			GUI.depth = -1;
			GUI.color = new Color(1f, 1f, 1f, _alpha);
			Rect position = new Rect((Screen.width - _renderTexture.width) / 2, (Screen.height - _renderTexture.height) / 2, _renderTexture.width, _renderTexture.height);
			GUI.DrawTexture(position, _renderTexture);
			GUI.color = Color.white;
		}
	}
}
