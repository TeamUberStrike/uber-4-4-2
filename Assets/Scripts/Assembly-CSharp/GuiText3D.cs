using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GuiText3D : MonoBehaviour
{
	public Font mFont;

	public string mText;

	public Camera mCamera;

	public Transform mTarget;

	public float mMaxDistance = 20f;

	public float mLifeTime = 5f;

	public Color mColor = Color.black;

	public bool mFadeOut = true;

	public Vector3 mFadeDirection = Vector2.up;

	private Text _uiText;

	private Canvas _canvas;

	private Transform _transform;

	private RectTransform _rectTransform;

	private Material _material;

	private Vector3 _viewportPosition;

	private float time;

	private Vector3 fadeDir = Vector3.zero;

	private Color startColor;

	private Color finalColor;

	private void Awake()
	{
		_transform = base.transform;
	}

	private void Start()
	{
		// Create a Canvas for UI Text
		GameObject canvasObject = new GameObject("UI3DCanvas");
		canvasObject.transform.SetParent(transform);
		_canvas = canvasObject.AddComponent<Canvas>();
		_canvas.renderMode = RenderMode.WorldSpace;
		_canvas.worldCamera = mCamera;
		
		// Add CanvasScaler for proper scaling
		CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.dynamicPixelsPerUnit = 10f;
		
		// Create the Text UI element
		GameObject textObject = new GameObject("UIText");
		textObject.transform.SetParent(canvasObject.transform);
		_uiText = textObject.AddComponent<Text>();
		_rectTransform = textObject.GetComponent<RectTransform>();
		
		// Configure Text properties
		_uiText.alignment = TextAnchor.MiddleCenter;
		_uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
		_uiText.verticalOverflow = VerticalWrapMode.Overflow;
		
		if (mCamera == null || mTarget == null || mFont == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		
		_uiText.font = mFont;
		_uiText.text = mText;
		_uiText.color = mColor;
		_material = _uiText.material;
		
		// Set initial size and position
		_rectTransform.sizeDelta = new Vector2(200, 50);
		_rectTransform.localScale = Vector3.one * 0.01f; // Scale down for world space
		
		startColor = _uiText.color;
		finalColor = _uiText.color;
		if (mFadeOut)
		{
			finalColor.a = 0f;
		}
	}

	private void LateUpdate()
	{
		if (mCamera != null && mTarget != null && (mLifeTime < 0f || mLifeTime > time))
		{
			time += Time.deltaTime;
			_viewportPosition = mCamera.WorldToViewportPoint(mTarget.localPosition);
			if (mFadeOut && mLifeTime > 0f)
			{
				_uiText.color = Color.Lerp(startColor, finalColor, time / mLifeTime);
			}
			else
			{
				float t = Mathf.Clamp01(_viewportPosition.z / mMaxDistance);
				_uiText.color = Color.Lerp(startColor, finalColor, t);
			}
			fadeDir += Time.deltaTime * mFadeDirection;
			
			// Convert viewport position to world space for UI Canvas
			Vector3 worldPos = mCamera.ViewportToWorldPoint(new Vector3(_viewportPosition.x, _viewportPosition.y, mCamera.nearClipPlane + 1f));
			_canvas.transform.position = worldPos + fadeDir;
			_canvas.transform.LookAt(_canvas.transform.position + mCamera.transform.rotation * Vector3.forward,
									 mCamera.transform.rotation * Vector3.up);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator startShowGuiText(float mLifeTime)
	{
		float time = 0f;
		Vector3 fadeDir = Vector3.zero;
		Color startColor = _uiText.color;
		Color finalColor = _uiText.color;
		if (mFadeOut)
		{
			finalColor.a = 0f;
		}
		while (mCamera != null && mTarget != null && (mLifeTime < 0f || mLifeTime > time))
		{
			time += Time.deltaTime;
			_viewportPosition = mCamera.WorldToViewportPoint(mTarget.localPosition);
			if (mFadeOut && mLifeTime > 0f)
			{
				_uiText.color = Color.Lerp(startColor, finalColor, time / mLifeTime);
			}
			else
			{
				float dist = Mathf.Clamp01(_viewportPosition.z / mMaxDistance);
				_uiText.color = Color.Lerp(startColor, finalColor, dist);
			}
			fadeDir += Time.deltaTime * mFadeDirection;
			
			// Convert viewport position to world space for UI Canvas
			Vector3 worldPos = mCamera.ViewportToWorldPoint(new Vector3(_viewportPosition.x, _viewportPosition.y, mCamera.nearClipPlane + 1f));
			_canvas.transform.position = worldPos + fadeDir;
			_canvas.transform.LookAt(_canvas.transform.position + mCamera.transform.rotation * Vector3.forward,
									 mCamera.transform.rotation * Vector3.up);
			yield return new WaitForEndOfFrame();
		}
		Object.Destroy(base.gameObject);
	}
}
