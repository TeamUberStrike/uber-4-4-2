using UnityEngine;

public class MouseInput : Singleton<MouseInput>
{
	private struct Click
	{
		public float Time;

		public Vector2 Point;
	}

	public const float DoubleClickInterval = 0.3f;

	private Click Current;

	private Click Previous;

	private MouseInput()
	{
	}

	public bool DoubleClick(Rect rect)
	{
		return Time.time - Previous.Time < 0.3f && GUITools.ToGlobal(rect).Contains(Current.Point);
	}

	public void OnGUI()
	{
		if (Event.current.type == EventType.MouseDown)
		{
			Previous = Current;
			Current.Time = Time.time;
			Current.Point = Event.current.mousePosition;
		}
	}
}
