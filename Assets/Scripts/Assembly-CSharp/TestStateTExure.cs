using UnityEngine;

public class TestStateTExure : MonoBehaviour
{
	public Texture[] textures;

	private StateTexture2D texture;

	private int index;

	private void Awake()
	{
		texture = new StateTexture2D(textures);
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(100f, 100f, 100f, 20f), "Change"))
		{
			texture.ChangeState(++index % textures.Length);
		}
		GUI.DrawTexture(new Rect(100f, 150f, 100f, 100f), texture.Current);
	}
}
