using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public static class UserInput
{
	public static float ZoomSpeed;

	public static Vector2 TouchLookSensitivity;

	public static Vector2 Mouse;

	public static Vector3 VerticalDirection;

	public static Vector3 HorizontalDirection;

	public static bool IsWalking
	{
		get
		{
			return (GameState.LocalCharacter.Keys & KeyState.Walking) != KeyState.Still && (GameState.LocalCharacter.Keys ^ KeyState.Horizontal) != KeyState.Still && (GameState.LocalCharacter.Keys ^ KeyState.Vertical) != KeyState.Still;
		}
	}

	public static bool IsMouseLooking
	{
		get
		{
			return AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.HorizontalLook) != 0f || AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.VerticalLook) != 0f;
		}
	}

	public static bool IsMovingVertically
	{
		get
		{
			return (GameState.LocalCharacter.Keys & (KeyState.Jump | KeyState.Crouch)) != 0;
		}
	}

	public static bool IsMovingUp
	{
		get
		{
			return (GameState.LocalCharacter.Keys & KeyState.Jump) != 0;
		}
	}

	public static bool IsMovingDown
	{
		get
		{
			return (GameState.LocalCharacter.Keys & KeyState.Crouch) != 0;
		}
	}

	public static Quaternion Rotation { get; private set; }

	static UserInput()
	{
		ZoomSpeed = 1f;
		TouchLookSensitivity = new Vector2(1f, 0.5f);
		Reset();
	}

	public static void Reset()
	{
		Mouse = new Vector2(0f, 0f);
		VerticalDirection = Vector3.zero;
		HorizontalDirection = Vector3.zero;
		Rotation = Quaternion.identity;
	}

	public static void UpdateDirections()
	{
		ResetDirection();
		if (TouchInput.WishJump || AutoMonoBehaviour<InputManager>.Instance.GetValue(GameInputKey.Jump) > 0.1f)
		{
			GameState.LocalCharacter.Keys |= KeyState.Jump;
		}
		else
		{
			GameState.LocalCharacter.Keys &= ~KeyState.Jump;
		}
		if (TouchInput.WishCrouch || AutoMonoBehaviour<InputManager>.Instance.GetValue(GameInputKey.Crouch) > 0.1f)
		{
			GameState.LocalCharacter.Keys |= KeyState.Crouch;
		}
		else
		{
			GameState.LocalCharacter.Keys &= ~KeyState.Crouch;
		}
		if (TouchInput.WishDirection.x > 0.1f || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.Right) > 0.1f)
		{
			GameState.LocalCharacter.Keys |= KeyState.Right;
		}
		else
		{
			GameState.LocalCharacter.Keys &= ~KeyState.Right;
		}
		if (TouchInput.WishDirection.x < -0.1f || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.Left) < -0.1f)
		{
			GameState.LocalCharacter.Keys |= KeyState.Left;
		}
		else
		{
			GameState.LocalCharacter.Keys &= ~KeyState.Left;
		}
		if (TouchInput.WishDirection.y > 0.1f || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.Forward) > 0.1f)
		{
			GameState.LocalCharacter.Keys |= KeyState.Forward;
		}
		else
		{
			GameState.LocalCharacter.Keys &= ~KeyState.Forward;
		}
		if (TouchInput.WishDirection.y < -0.1f || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || AutoMonoBehaviour<InputManager>.Instance.RawValue(GameInputKey.Backward) < -0.1f)
		{
			GameState.LocalCharacter.Keys |= KeyState.Backward;
		}
		else
		{
			GameState.LocalCharacter.Keys &= ~KeyState.Backward;
		}
		if ((GameState.LocalCharacter.Keys & KeyState.Left) != KeyState.Still)
		{
			HorizontalDirection.x -= 127f;
		}
		if ((GameState.LocalCharacter.Keys & KeyState.Right) != KeyState.Still)
		{
			HorizontalDirection.x += 127f;
		}
		if ((GameState.LocalCharacter.Keys & KeyState.Forward) != KeyState.Still)
		{
			HorizontalDirection.z += 127f;
		}
		if ((GameState.LocalCharacter.Keys & KeyState.Backward) != KeyState.Still)
		{
			HorizontalDirection.z -= 127f;
		}
		HorizontalDirection += new Vector3(TouchInput.WishDirection.x, 0f, TouchInput.WishDirection.y);
		if (HorizontalDirection.sqrMagnitude > 1f)
		{
			HorizontalDirection.Normalize();
		}
		VerticalDirection += new Vector3(0f, TouchInput.WishCrouch ? (-1) : (TouchInput.WishJump ? 1 : 0), 0f);
		VerticalDirection.Normalize();
	}

	public static void ResetDirection()
	{
		HorizontalDirection = Vector3.zero;
		VerticalDirection = Vector3.zero;
	}

	public static KeyState GetkeyState(GameInputKey slot)
	{
		switch (slot)
		{
		case GameInputKey.Forward:
			return KeyState.Forward;
		case GameInputKey.Backward:
			return KeyState.Backward;
		case GameInputKey.Right:
			return KeyState.Right;
		case GameInputKey.Left:
			return KeyState.Left;
		case GameInputKey.Crouch:
			return KeyState.Crouch;
		case GameInputKey.Jump:
			return KeyState.Jump;
		default:
			return KeyState.Still;
		}
	}

	public static void SetRotation(float hAngle = 0f, float vAngle = 0f)
	{
		Mouse = new Vector2(hAngle, 0f - vAngle);
		UpdateMouse();
		UpdateDirections();
	}

	public static void UpdateMouse()
	{
		if (Camera.main != null)
		{
			float num = Mathf.Pow(Camera.main.fieldOfView / ApplicationDataManager.ApplicationOptions.CameraFovMax, 1.1f);
			if (AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled)
			{
				Mouse.x += Input.GetAxisRaw("RS X") * ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity * num;
			}
			else
			{
				Mouse.x += Input.GetAxisRaw("GameStopLook X") * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * num;
			}
			Mouse.x += TouchInput.WishLook.x * TouchLookSensitivity.x * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * num;
			Mouse.x = ClampAngle(Mouse.x, -360f, 360f);
			int num2 = ((!ApplicationDataManager.ApplicationOptions.InputInvertMouse) ? 1 : (-1));
			if (AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled)
			{
				Mouse.y += Input.GetAxisRaw("RS Y") * ApplicationDataManager.ApplicationOptions.InputXMouseSensitivity * num;
			}
			else
			{
				Mouse.y += Input.GetAxisRaw("GameStopLook Y") * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * (float)num2 * num;
			}
			Mouse.y += TouchInput.WishLook.y * TouchLookSensitivity.y * ApplicationDataManager.ApplicationOptions.TouchLookSensitivity * (float)num2 * num;
			Mouse.y = ClampAngle(Mouse.y, -88f, 88f);
		}
		Rotation = Quaternion.AngleAxis(Mouse.x, Vector3.up) * Quaternion.AngleAxis(Mouse.y, Vector3.left);
	}

	public static bool IsPressed(KeyState k)
	{
		return (GameState.LocalCharacter.Keys & k) != 0;
	}

	private static float ClampAngle(float angle, float min, float max)
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
