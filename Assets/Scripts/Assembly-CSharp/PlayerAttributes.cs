using System;

[Serializable]
public class PlayerAttributes
{
	public const float HEIGHT_NORMAL = 1.6f;

	public const float HEIGHT_DUCKED = 0.9f;

	public const float CENTER_OFFSET_DUCKED = -0.4f;

	public const float CENTER_OFFSET_NORMAL = -0.1f;

	public float Speed
	{
		get
		{
			return 7f;
		}
	}

	public float JumpForce
	{
		get
		{
			return 15f;
		}
	}
}
