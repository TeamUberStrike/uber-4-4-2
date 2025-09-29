using UnityEngine;

[ExecuteInEditMode]
public class TestAnyGuiStyle : MonoBehaviour
{
	public GUISkin skin;

	public string style = "label";

	public Vector2 size;

	public string text = string.Empty;

	private void OnGUI()
	{
		GUI.skin = skin;
		GUI.Button(new Rect(((float)Screen.width - size.x) / 2f, ((float)Screen.height - size.y) / 2f, size.x, size.y), text, style);
	}
}
