using UnityEngine;

public abstract class TouchBaseControl
{
	public class TouchFinger
	{
		public Vector2 StartPos;

		public Vector2 LastPos;

		public float StartTouchTime;

		public int FingerId;

		public TouchFinger()
		{
			Reset();
		}

		public void Reset()
		{
			StartPos = Vector2.zero;
			LastPos = Vector2.zero;
			StartTouchTime = 0f;
			FingerId = -1;
		}
	}

	public virtual bool Enabled { get; set; }

	public virtual Rect Boundary { get; set; }

	public TouchBaseControl()
	{
		Singleton<TouchController>.Instance.AddControl(this);
	}

	public virtual void FirstUpdate()
	{
	}

	public virtual void UpdateTouches(Touch touch)
	{
	}

	public virtual void FinalUpdate()
	{
	}

	public virtual void Draw()
	{
	}

	~TouchBaseControl()
	{
		Singleton<TouchController>.Instance.RemoveControl(this);
	}
}
