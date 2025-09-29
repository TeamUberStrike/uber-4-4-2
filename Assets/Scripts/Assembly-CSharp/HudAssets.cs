using UnityEngine;

public class HudAssets : MonoBehaviour
{
	[SerializeField]
	private BitmapFont _interparkBitmapFont;

	[SerializeField]
	private BitmapFont _helveticaBitmapFont;

	public static HudAssets Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public BitmapFont InterparkBitmapFont
	{
		get
		{
			return _interparkBitmapFont;
		}
	}

	public BitmapFont HelveticaBitmapFont
	{
		get
		{
			return _helveticaBitmapFont;
		}
	}

	private void Awake()
	{
		Instance = this;
	}
}
