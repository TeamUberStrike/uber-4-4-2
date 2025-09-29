using UnityEngine;

public class ObjectiveTick
{
	private bool _completed;

	private Texture _bk;

	private Texture _tip;

	public ObjectiveTick(Texture bk, Texture tip)
	{
		_bk = bk;
		_tip = tip;
	}

	public void Draw(Vector2 position, float scale)
	{
		int num = 78;
		GUIUtility.ScaleAroundPivot(pivotPoint: new Vector2(position.x, position.y + (float)num * scale / 2f), scale: new Vector2(scale, scale));
		GUI.BeginGroup(new Rect(position.x, position.y, num, num));
		GUI.Label(new Rect(0f, 0f, 78f, 78f), _bk);
		if (_completed)
		{
			GUI.DrawTexture(new Rect(4f, 16f, 62f, 49f), _tip);
		}
		GUI.EndGroup();
		GUI.matrix = Matrix4x4.identity;
	}

	public void Complete()
	{
		_completed = true;
	}
}
