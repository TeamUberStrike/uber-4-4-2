using UnityEngine;

public class LevelEnviroment : MonoBehaviour
{
	public const float MovementSpeed = 1f;

	public const float Modifier = 0.035f;

	public const float PlayerWalkSpeed = 7f;

	public const float PlayerJumpSpeed = 15f;

	public EnviromentSettings Settings;

	public static LevelEnviroment Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	private void Awake()
	{
		Instance = this;
	}
}
