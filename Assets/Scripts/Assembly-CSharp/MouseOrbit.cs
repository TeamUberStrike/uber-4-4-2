using System;
using System.Collections;
using UnityEngine;

public class MouseOrbit : MonoBehaviour
{
	private const float zoomSpeedFactor = 15f;

	private const float zoomTouchSpeedFactor = 0.001f;

	private const float flingSpeedFactor = 0.1f;

	private const float orbitSpeedFactor = 3f;

	private const float panSpeedFactor = 0.001f;

	[SerializeField]
	private Transform target;

	private Vector2 zoomMax = new Vector2(1.3f, 5f);

	private Vector2 panMax = new Vector2(-0.5f, 0.5f);

	private Vector2 panTotalMax = new Vector2(-1f, 1f);

	public Vector3 OrbitConfig;

	public float yPanningOffset;

	private float zoomDistance = 5f;

	public int MaxX;

	private Vector2 mouseAxisSpin;

	private Queue lastTouchDeltas = new Queue();

	private Queue lastDragDiffs = new Queue();

	public static MouseOrbit Instance { get; private set; }

	public Vector3 OrbitOffset { get; set; }

	public static bool Disable { get; set; }

	private void Awake()
	{
		Instance = this;
		base.enabled = false;
		Disable = false;
		if (target == null)
		{
			throw new NullReferenceException("MouseOrbit.target not set");
		}
	}

	private void Start()
	{
		mouseAxisSpin = Vector2.zero;
		Vector3 eulerAngles = base.transform.eulerAngles;
		OrbitConfig.x = eulerAngles.y;
		OrbitConfig.y = eulerAngles.x;
		MaxX = Screen.width;
	}

	private void OnEnable()
	{
		zoomDistance = (OrbitConfig.z = Mathf.Clamp(Vector3.Distance(base.transform.position, target.position), zoomMax[0], zoomMax[1]));
		OrbitConfig.x = base.transform.rotation.eulerAngles.y;
		OrbitConfig.y = base.transform.rotation.eulerAngles.x;
	}

	private void LateUpdate()
	{
		if (!PopupSystem.IsAnyPopupOpen && !PanelManager.IsAnyPanelOpen && !Disable && Input.touchCount > 0)
		{
			Touch touch = Input.touches[0];
			if (Input.touchCount > 1)
			{
				Touch touch2 = Input.touches[1];
				if (touch2.phase == TouchPhase.Began)
				{
					lastDragDiffs.Clear();
				}
				if (lastDragDiffs.Count > 9)
				{
					lastDragDiffs.Dequeue();
				}
				Vector2 vector = touch.position - touch.deltaPosition - (touch2.position - touch2.deltaPosition);
				Vector2 vector2 = touch.position - touch2.position;
				lastDragDiffs.Enqueue(vector2.sqrMagnitude - vector.sqrMagnitude);
				float num = 0f;
				foreach (float lastDragDiff in lastDragDiffs)
				{
					num += lastDragDiff;
				}
				num /= (float)lastDragDiffs.Count;
				float num3 = num * 0.001f;
				OrbitConfig.z = Mathf.Clamp(zoomDistance - num3, zoomMax[0], zoomMax[1]);
			}
			else if (touch.phase == TouchPhase.Began)
			{
				if (GameState.HasCurrentSpace && GameState.CurrentSpace.Camera.pixelRect.ContainsTouch(touch.position) && touch.position.x < (float)MaxX && touch.position.y < (float)(Screen.height - GlobalUIRibbon.Instance.Height()))
				{
					lastTouchDeltas.Clear();
					mouseAxisSpin = Vector2.zero;
				}
			}
			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled || touch.position.x >= (float)MaxX || touch.position.y >= (float)(Screen.height - GlobalUIRibbon.Instance.Height()))
			{
				Vector2 zero = Vector2.zero;
				foreach (Vector2 lastTouchDelta in lastTouchDeltas)
				{
					zero += lastTouchDelta;
				}
				lastTouchDeltas.Clear();
				float num4 = Mathf.Clamp(zero.magnitude, 0f, 100f);
				mouseAxisSpin = zero.normalized * num4;
			}
			else if (touch.position.x < (float)MaxX && touch.position.y < (float)(Screen.height - GlobalUIRibbon.Instance.Height()))
			{
				if (lastTouchDeltas.Count > 4)
				{
					lastTouchDeltas.Dequeue();
				}
				lastTouchDeltas.Enqueue(touch.deltaPosition);
				OrbitConfig.x += touch.deltaPosition.x;
				yPanningOffset -= touch.deltaPosition.y * 0.001f * ((!IsValueWithin(yPanningOffset, panMax[0], panMax[1])) ? 0.3f : 1f);
			}
		}
		else if (mouseAxisSpin.sqrMagnitude > 0.010000001f)
		{
			mouseAxisSpin = Vector2.Lerp(mouseAxisSpin, Vector2.zero, Time.deltaTime * 2f);
			OrbitConfig.x += mouseAxisSpin.x * 0.1f;
			yPanningOffset -= mouseAxisSpin.y * 0.1f * 0.001f * ((!IsValueWithin(yPanningOffset, panMax[0], panMax[1])) ? 0.3f : 1f);
		}
		else
		{
			mouseAxisSpin = Vector2.zero;
		}
		if (Input.touchCount == 0)
		{
			yPanningOffset = Mathf.Lerp(yPanningOffset, Mathf.Clamp(yPanningOffset, panMax[0], panMax[1]), Time.deltaTime * 10f);
		}
		yPanningOffset = Mathf.Clamp(yPanningOffset, panTotalMax[0], panTotalMax[1]);
		zoomDistance = Mathf.Lerp(zoomDistance, Mathf.Clamp(OrbitConfig.z, zoomMax[0], zoomMax[1]), Time.deltaTime * 5f);
		Transform(base.transform);
	}

	public void Transform(Transform transform)
	{
		Vector3 position;
		Quaternion rotation;
		Transform(out position, out rotation);
		transform.position = position;
		transform.rotation = rotation;
	}

	public void Transform(out Vector3 position, out Quaternion rotation2)
	{
		float num = 1f - Mathf.Clamp01(zoomDistance / zoomMax[1]);
		Quaternion quaternion = (rotation2 = Quaternion.Euler(OrbitConfig.y, OrbitConfig.x, 0f));
		position = target.position + quaternion * new Vector3(0f, 0f, 0f - zoomDistance) + quaternion * (OrbitOffset + new Vector3(0f, yPanningOffset * num, 0f)) * zoomDistance;
	}

	public void SetTarget(Transform t)
	{
		target = t;
	}

	private static bool IsValueWithin(float value, float min, float max)
	{
		return value >= min && value <= max;
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
