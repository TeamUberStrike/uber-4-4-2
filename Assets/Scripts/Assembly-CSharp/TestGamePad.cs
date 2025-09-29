using UnityEngine;

internal class TestGamePad : MonoBehaviour
{
	private UserInputMap _targetMap;

	private void Start()
	{
		AutoMonoBehaviour<InputManager>.Instance.IsGamepadEnabled = true;
		string[] joystickNames = Input.GetJoystickNames();
		foreach (string text in joystickNames)
		{
			Debug.Log("Joystick " + text);
		}
	}

	private void OnGUI()
	{
		int num = 0;
		foreach (UserInputMap value in AutoMonoBehaviour<InputManager>.Instance.KeyMapping.Values)
		{
			bool flag = value == _targetMap;
			GUI.Label(new Rect(20f, 35 + num * 20, 140f, 20f), value.Description);
			if (value.IsConfigurable && GUI.Toggle(new Rect(180f, 35 + num * 20, 20f, 20f), flag, string.Empty))
			{
				_targetMap = value;
				Screen.lockCursor = true;
			}
			if (flag)
			{
				GUI.enabled = true;
				GUI.TextField(new Rect(220f, 35 + num * 20, 100f, 20f), string.Empty);
				GUI.enabled = false;
			}
			else
			{
				GUI.contentColor = ((value.Channel == null) ? Color.red : Color.white);
				GUI.Label(new Rect(220f, 35 + num * 20, 150f, 20f), value.Assignment);
				GUI.contentColor = Color.white;
			}
			num++;
		}
		if (_targetMap != null && Event.current.type == EventType.Layout && AutoMonoBehaviour<InputManager>.Instance.ListenForNewKeyAssignment(_targetMap))
		{
			_targetMap = null;
			Screen.lockCursor = false;
			Event.current.Use();
		}
		if (_targetMap != null && Event.current.type == EventType.Layout && AutoMonoBehaviour<InputManager>.Instance.ListenForNewKeyAssignment(_targetMap))
		{
			_targetMap = null;
			Screen.lockCursor = false;
			Event.current.Use();
		}
	}
}
