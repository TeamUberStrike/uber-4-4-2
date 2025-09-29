using UnityEngine;
using UnityEngine.UI;

public class GuiText : MonoBehaviour
{
	[SerializeField]
	private Font _font;

	[SerializeField]
	private string _text;

	[SerializeField]
	private Color _color;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private Transform _target;

	[SerializeField]
	private bool _hasTimeLimit;

	[SerializeField]
	private float _distanceCap = -1f;

	private Text _uiText;

	private Canvas _canvas;

	private Transform _transform;

	private RectTransform _rectTransform;

	private Material _material;

	private float _visibleTime;

	private bool _isVisible = true;

	public bool IsTextVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				if (_uiText != null)
					_uiText.enabled = value;
			}
		}
	}

	private void Awake()
	{
		_transform = base.transform;
	}

	private void Start()
	{
		// Create a Canvas for UI Text
		GameObject canvasObject = new GameObject("UICanvas");
		canvasObject.transform.SetParent(transform);
		_canvas = canvasObject.AddComponent<Canvas>();
		_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		
		// Add CanvasScaler for proper scaling
		CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		
		// Add GraphicRaycaster for UI interactions
		canvasObject.AddComponent<GraphicRaycaster>();
		
		// Create the Text UI element
		GameObject textObject = new GameObject("UIText");
		textObject.transform.SetParent(canvasObject.transform);
		_uiText = textObject.AddComponent<Text>();
		_rectTransform = textObject.GetComponent<RectTransform>();
		
		// Configure Text properties
		_uiText.alignment = TextAnchor.MiddleCenter;
		_uiText.font = _font;
		_uiText.text = _text;
		_uiText.color = _color;
		_material = _uiText.material;
		
		// Set initial size and position
		_rectTransform.sizeDelta = new Vector2(200, 50);
		_rectTransform.anchorMin = Vector2.zero;
		_rectTransform.anchorMax = Vector2.zero;
	}

	private void LateUpdate()
	{
		if (!(Camera.main != null) || !_isVisible)
		{
			return;
		}
		Vector3 position = Camera.main.WorldToViewportPoint(_target.localPosition + _offset);
		
		// Convert viewport coordinates to screen coordinates for UI
		Vector2 screenPosition = new Vector2(position.x * Screen.width, position.y * Screen.height);
		_rectTransform.position = screenPosition;
		
		if (_hasTimeLimit)
		{
			_visibleTime -= Time.deltaTime;
			if (_visibleTime > 0f)
			{
				_color.a = _visibleTime;
				_uiText.color = _color;
			}
			else
			{
				_uiText.enabled = false;
			}
		}
		else
		{
			if (_distanceCap > 0f)
			{
				float a = 1f - Mathf.Clamp01(position.z / _distanceCap);
				_color.a = a;
			}
			_uiText.color = _color;
		}
	}

	public void ShowText(int seconds)
	{
		_visibleTime = seconds;
	}

	public void ShowText()
	{
		ShowText(5);
	}
}
