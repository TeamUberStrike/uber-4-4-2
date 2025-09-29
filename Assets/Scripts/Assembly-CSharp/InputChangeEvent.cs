public class InputChangeEvent
{
	private GameInputKey _key;

	private float _value;

	public GameInputKey Key
	{
		get
		{
			return _key;
		}
	}

	public bool IsDown
	{
		get
		{
			return _value != 0f;
		}
	}

	public float Value
	{
		get
		{
			return _value;
		}
	}

	public InputChangeEvent(GameInputKey key, float value)
	{
		_key = key;
		_value = value;
	}
}
