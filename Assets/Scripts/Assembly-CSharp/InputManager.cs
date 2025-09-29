using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class InputManager : AutoMonoBehaviour<InputManager>
{
	private bool _inputEnabled;

	private bool _unassignedKeyMappings;

	private bool _isGamepadEnabled;

	private Dictionary<int, UserInputMap> _keyMapping = new Dictionary<int, UserInputMap>();

	public bool IsGamepadEnabled
	{
		get
		{
			return _isGamepadEnabled;
		}
		set
		{
			_isGamepadEnabled = value;
			if (_isGamepadEnabled)
			{
				KeyMapping[1].Channel = new AxisInputChannel("RS X", 0f);
				KeyMapping[2].Channel = new AxisInputChannel("RS Y", 0f);
			}
		}
	}

	public Dictionary<int, UserInputMap> KeyMapping
	{
		get
		{
			return _keyMapping;
		}
	}

	public bool IsAnyDown
	{
		get
		{
			if (IsInputEnabled)
			{
				foreach (UserInputMap value in _keyMapping.Values)
				{
					if (value.Value != 0f)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public bool IsInputEnabled
	{
		get
		{
			return _inputEnabled;
		}
		set
		{
			_inputEnabled = value;
			if (_inputEnabled)
			{
				return;
			}
			foreach (UserInputMap value2 in _keyMapping.Values)
			{
				if (value2 != null && value2.Channel != null)
				{
					value2.Channel.Reset();
					if (value2.IsEventSender && value2.Channel.IsChanged)
					{
						CmuneEventHandler.Route(new InputChangeEvent(value2.Slot, value2.Channel.Value));
					}
				}
			}
		}
	}

	public bool IsSettingKeymap { get; private set; }

	public bool HasUnassignedKeyMappings
	{
		get
		{
			return _unassignedKeyMappings;
		}
	}

	private void Awake()
	{
		SetDefaultKeyMapping();
	}

	private void Update()
	{
		KeyInput.Update();
		if (IsInputEnabled)
		{
			foreach (UserInputMap value in _keyMapping.Values)
			{
				if (value != null && value.Channel != null)
				{
					value.Channel.Listen();
					if (value.IsEventSender && value.Channel.IsChanged)
					{
						CmuneEventHandler.Route(new InputChangeEvent(value.Slot, value.Channel.Value));
					}
				}
			}
		}
		if (RawValue(GameInputKey.Fullscreen) != 0f && GUITools.SaveClickIn(0.2f))
		{
			GUITools.Clicked();
			ScreenResolutionManager.IsFullScreen = !Screen.fullScreen;
		}
		if (RawValue(GameInputKey.SendScreenshotToFacebook) != 0f && GUITools.SaveClickIn(0.2f))
		{
			GUITools.Clicked();
			AutoMonoBehaviour<FacebookInterface>.Instance.ShareScreenShot();
		}
	}

	private void OnGUI()
	{
		KeyInput.OnGUI();
		Singleton<MouseInput>.Instance.OnGUI();
		if (Event.current.shift && Event.current.type == EventType.ScrollWheel)
		{
			if (Event.current.delta.x > 0f)
			{
				CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.PrevWeapon, Event.current.delta.x));
			}
			if (Event.current.delta.x < 0f)
			{
				CmuneEventHandler.Route(new InputChangeEvent(GameInputKey.NextWeapon, Event.current.delta.x));
			}
		}
	}

	public static bool GetMouseButtonDown(int button)
	{
		return (Event.current == null || Event.current.type == EventType.Layout) && Input.GetMouseButtonDown(button);
	}

	public bool ListenForNewKeyAssignment(UserInputMap map)
	{
		if (Event.current.keyCode == KeyCode.Escape)
		{
			IsSettingKeymap = false;
			return true;
		}
		if (Event.current.keyCode != KeyCode.None)
		{
			map.Channel = new KeyInputChannel(Event.current.keyCode);
		}
		else if (Event.current.shift)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				map.Channel = new KeyInputChannel(KeyCode.LeftShift);
			}
			if (Input.GetKey(KeyCode.RightShift))
			{
				map.Channel = new KeyInputChannel(KeyCode.RightShift);
			}
		}
		else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(3) || Input.GetMouseButtonDown(4))
		{
			map.Channel = new MouseInputChannel(Event.current.button);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("Mouse ScrollWheel")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("Mouse ScrollWheel", 0.1f, (Input.GetAxisRaw("Mouse ScrollWheel") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("LS X")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("LS X", 0.1f, (Input.GetAxisRaw("LS X") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("LS Y")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("LS Y", 0.1f, (Input.GetAxisRaw("LS Y") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("RS X")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("RS X", 0.1f, (Input.GetAxisRaw("RS X") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("RS Y")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("RS Y", 0.1f, (Input.GetAxisRaw("RS Y") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("DPad X")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("DPad X", 0.1f, (Input.GetAxisRaw("DPad X") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Mathf.Abs(Input.GetAxisRaw("DPad Y")) > 0.1f)
		{
			map.Channel = new AxisInputChannel("DPad Y", 0.1f, (Input.GetAxisRaw("DPad Y") > 0f) ? AxisInputChannel.AxisReadingMethod.PositiveOnly : AxisInputChannel.AxisReadingMethod.NegativeOnly);
		}
		else if (Input.GetAxisRaw("LT") > 0.1f)
		{
			map.Channel = new AxisInputChannel("LT", 0.1f);
		}
		else if (Input.GetAxisRaw("RT") > 0.1f)
		{
			map.Channel = new AxisInputChannel("RT", 0.1f);
		}
		else if (Input.GetButton("A"))
		{
			map.Channel = new ButtonInputChannel("A");
		}
		else if (Input.GetButton("B"))
		{
			map.Channel = new ButtonInputChannel("B");
		}
		else if (Input.GetButton("X"))
		{
			map.Channel = new ButtonInputChannel("X");
		}
		else if (Input.GetButton("Y"))
		{
			map.Channel = new ButtonInputChannel("Y");
		}
		else if (Input.GetButton("LB"))
		{
			map.Channel = new ButtonInputChannel("LB");
		}
		else if (Input.GetButton("RB"))
		{
			map.Channel = new ButtonInputChannel("RB");
		}
		else if (Input.GetButton("Start"))
		{
			map.Channel = new ButtonInputChannel("Start");
		}
		else if (Input.GetButton("Back"))
		{
			map.Channel = new ButtonInputChannel("Back");
		}
		else if (Input.GetButton("GamePadButton10"))
		{
			map.Channel = new ButtonInputChannel("GamePadButton10");
		}
		else
		{
			if (!Input.GetButton("GamePadButton11"))
			{
				IsSettingKeymap = true;
				return false;
			}
			map.Channel = new ButtonInputChannel("GamePadButton11");
		}
		CmuneEventHandler.Route(new InputAssignmentEvent());
		Event.current.Use();
		ResolveMultipleAssignment(map);
		WriteAllKeyMappings();
		IsSettingKeymap = false;
		return true;
	}

	public void Reset()
	{
		_keyMapping.Clear();
		SetDefaultKeyMapping();
		IsGamepadEnabled = false;
		WriteAllKeyMappings();
	}

	public float RawValue(GameInputKey slot)
	{
		UserInputMap value;
		if (!IsSettingKeymap && _keyMapping.TryGetValue((int)slot, out value))
		{
			return value.RawValue();
		}
		return 0f;
	}

	public float GetValue(GameInputKey slot)
	{
		UserInputMap value;
		if (!IsSettingKeymap && IsInputEnabled && _keyMapping.TryGetValue((int)slot, out value))
		{
			return value.Value;
		}
		return 0f;
	}

	public bool IsDown(GameInputKey slot)
	{
		UserInputMap value;
		if (!IsSettingKeymap && _keyMapping.TryGetValue((int)slot, out value))
		{
			return value.Value != 0f;
		}
		return false;
	}

	public string GetKeyAssignmentString(GameInputKey slot)
	{
		UserInputMap value;
		if (_keyMapping.TryGetValue((int)slot, out value) && value != null)
		{
			return value.Assignment;
		}
		return "Not set";
	}

	public string GetSlotName(GameInputKey slot)
	{
		switch (slot)
		{
		case GameInputKey.Backward:
			return "Backward";
		case GameInputKey.Chat:
			return "Chat";
		case GameInputKey.Crouch:
			return "Crouch";
		case GameInputKey.ChangeTeam:
			return "Change Team";
		case GameInputKey.Forward:
			return "Forward";
		case GameInputKey.Fullscreen:
			return "Fullscreen";
		case GameInputKey.UseQuickItem:
			return "Use QuickItem";
		case GameInputKey.NextQuickItem:
			return "Cycle QuickItems";
		case GameInputKey.HorizontalLook:
			return "HorizontalLook";
		case GameInputKey.Loadout:
			return "Loadout";
		case GameInputKey.Jump:
			return "Jump";
		case GameInputKey.Left:
			return "Left";
		case GameInputKey.NextWeapon:
			return "Next Weapon / Zoom In";
		case GameInputKey.None:
			return "None";
		case GameInputKey.Pause:
			return "Pause";
		case GameInputKey.PrevWeapon:
			return "Prev Weapon / Zoom Out";
		case GameInputKey.PrimaryFire:
			return "Primary Fire";
		case GameInputKey.QuickItem1:
			return "Quick Item 1";
		case GameInputKey.QuickItem2:
			return "Quick Item 2";
		case GameInputKey.QuickItem3:
			return "Quick Item 3";
		case GameInputKey.Right:
			return "Right";
		case GameInputKey.SecondaryFire:
			return "Secondary Fire";
		case GameInputKey.Tabscreen:
			return "Tabscreen";
		case GameInputKey.VerticalLook:
			return "VerticalLook";
		case GameInputKey.Weapon1:
			return "Primary Weapon";
		case GameInputKey.Weapon2:
			return "Secondary Weapon";
		case GameInputKey.Weapon3:
			return "Tertiary Weapon";
		case GameInputKey.Weapon4:
			return "Pickup Weapon";
		case GameInputKey.WeaponMelee:
			return "Melee Weapon";
		case GameInputKey.SendScreenshotToFacebook:
			return "Send Screenshot to Facebook";
		default:
			return "No Name";
		}
	}

	private void ResolveMultipleAssignment(UserInputMap map)
	{
		foreach (UserInputMap value in _keyMapping.Values)
		{
			if (value != map && value.Channel != null && value.Channel.ChannelType == map.Channel.ChannelType && map.Assignment == value.Assignment)
			{
				value.Channel = null;
				break;
			}
		}
	}

	private bool IsChannelTaken(IInputChannel channel)
	{
		bool result = false;
		foreach (UserInputMap value in _keyMapping.Values)
		{
			if (value.Channel.Equals(channel))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void SetDefaultKeyMapping()
	{
		_keyMapping[1] = new UserInputMap(GetSlotName(GameInputKey.HorizontalLook), GameInputKey.HorizontalLook, new AxisInputChannel("GameStopLook X", 0f), false, false);
		_keyMapping[2] = new UserInputMap(GetSlotName(GameInputKey.VerticalLook), GameInputKey.VerticalLook, new AxisInputChannel("GameStopLook Y", 1f), false, false);
		_keyMapping[21] = new UserInputMap(GetSlotName(GameInputKey.Pause), GameInputKey.Pause, new KeyInputChannel(KeyCode.Escape));
		_keyMapping[3] = new UserInputMap(GetSlotName(GameInputKey.Forward), GameInputKey.Forward, new KeyInputChannel(KeyCode.UpArrow));
		_keyMapping[5] = new UserInputMap(GetSlotName(GameInputKey.Left), GameInputKey.Left, new KeyInputChannel(KeyCode.LeftArrow));
		_keyMapping[4] = new UserInputMap(GetSlotName(GameInputKey.Backward), GameInputKey.Backward, new KeyInputChannel(KeyCode.DownArrow));
		_keyMapping[6] = new UserInputMap(GetSlotName(GameInputKey.Right), GameInputKey.Right, new KeyInputChannel(KeyCode.RightArrow));
		_keyMapping[7] = new UserInputMap(GetSlotName(GameInputKey.Jump), GameInputKey.Jump, new KeyInputChannel(KeyCode.Alpha6));
		_keyMapping[8] = new UserInputMap(GetSlotName(GameInputKey.Crouch), GameInputKey.Crouch, new KeyInputChannel(KeyCode.Alpha8));
		_keyMapping[9] = new UserInputMap(GetSlotName(GameInputKey.PrimaryFire), GameInputKey.PrimaryFire, new KeyInputChannel(KeyCode.Alpha1));
		_keyMapping[10] = new UserInputMap(GetSlotName(GameInputKey.SecondaryFire), GameInputKey.SecondaryFire, new KeyInputChannel(KeyCode.Alpha2));
		_keyMapping[19] = new UserInputMap(GetSlotName(GameInputKey.NextWeapon), GameInputKey.NextWeapon, new KeyInputChannel(KeyCode.Alpha7));
		_keyMapping[20] = new UserInputMap(GetSlotName(GameInputKey.PrevWeapon), GameInputKey.PrevWeapon, new KeyInputChannel(KeyCode.Alpha5));
		_keyMapping[26] = new UserInputMap(GetSlotName(GameInputKey.UseQuickItem), GameInputKey.UseQuickItem, new KeyInputChannel(KeyCode.Alpha3));
		_keyMapping[28] = new UserInputMap(GetSlotName(GameInputKey.NextQuickItem), GameInputKey.NextQuickItem, new KeyInputChannel(KeyCode.Alpha4));
	}

	private static CmunePrefs.Key GetPrefsKeyForSlot(int slot)
	{
		switch (slot)
		{
		case 4:
			return CmunePrefs.Key.Keymap_Backward;
		case 24:
			return CmunePrefs.Key.Keymap_Chat;
		case 8:
			return CmunePrefs.Key.Keymap_Crouch;
		case 27:
			return CmunePrefs.Key.Keymap_ChangeTeam;
		case 3:
			return CmunePrefs.Key.Keymap_Forward;
		case 22:
			return CmunePrefs.Key.Keymap_Fullscreen;
		case 26:
			return CmunePrefs.Key.Keymap_UseQuickItem;
		case 28:
			return CmunePrefs.Key.Keymap_NextQuickItem;
		case 1:
			return CmunePrefs.Key.Keymap_HorizontalLook;
		case 25:
			return CmunePrefs.Key.Keymap_Inventory;
		case 7:
			return CmunePrefs.Key.Keymap_Jump;
		case 5:
			return CmunePrefs.Key.Keymap_Left;
		case 19:
			return CmunePrefs.Key.Keymap_NextWeapon;
		case 0:
			return CmunePrefs.Key.Keymap_None;
		case 21:
			return CmunePrefs.Key.Keymap_Pause;
		case 20:
			return CmunePrefs.Key.Keymap_PrevWeapon;
		case 9:
			return CmunePrefs.Key.Keymap_PrimaryFire;
		case 16:
			return CmunePrefs.Key.Keymap_QuickItem1;
		case 17:
			return CmunePrefs.Key.Keymap_QuickItem2;
		case 18:
			return CmunePrefs.Key.Keymap_QuickItem3;
		case 6:
			return CmunePrefs.Key.Keymap_Right;
		case 10:
			return CmunePrefs.Key.Keymap_SecondaryFire;
		case 23:
			return CmunePrefs.Key.Keymap_Tabscreen;
		case 2:
			return CmunePrefs.Key.Keymap_VerticalLook;
		case 11:
			return CmunePrefs.Key.Keymap_Weapon1;
		case 12:
			return CmunePrefs.Key.Keymap_Weapon2;
		case 13:
			return CmunePrefs.Key.Keymap_Weapon3;
		case 14:
			return CmunePrefs.Key.Keymap_Weapon4;
		case 15:
			return CmunePrefs.Key.Keymap_WeaponMelee;
		case 30:
			return CmunePrefs.Key.Keymap_SendScreenshotToFacebook;
		default:
			return CmunePrefs.Key.Keymap_None;
		}
	}

	private void WriteAllKeyMappings()
	{
		_unassignedKeyMappings = false;
		foreach (KeyValuePair<int, UserInputMap> item in _keyMapping)
		{
			if (item.Value.IsConfigurable)
			{
				string prefString = item.Value.GetPrefString();
				CmunePrefs.WriteKey(GetPrefsKeyForSlot(item.Key), prefString);
				if (item.Value.Channel == null)
				{
					_unassignedKeyMappings = true;
				}
			}
		}
	}

	public void ReadAllKeyMappings()
	{
		_unassignedKeyMappings = false;
		foreach (KeyValuePair<int, UserInputMap> item in _keyMapping)
		{
			string value;
			if (item.Value.IsConfigurable && CmunePrefs.TryGetKey<string>(GetPrefsKeyForSlot(item.Key), out value))
			{
				item.Value.FromPrefString(value);
				if (item.Value.Channel == null)
				{
					_unassignedKeyMappings = true;
				}
			}
		}
	}

	public void SetKeyboardKeyMappingAndroid()
	{
		_keyMapping[1] = new UserInputMap(GetSlotName(GameInputKey.HorizontalLook), GameInputKey.HorizontalLook, new AxisInputChannel("GameStopLook X", 0f), false, false);
		_keyMapping[2] = new UserInputMap(GetSlotName(GameInputKey.VerticalLook), GameInputKey.VerticalLook, new AxisInputChannel("GameStopLook Y", 1f), false, false);
		_keyMapping[21] = new UserInputMap(GetSlotName(GameInputKey.Pause), GameInputKey.Pause, new KeyInputChannel(KeyCode.Escape));
		_keyMapping[3] = new UserInputMap(GetSlotName(GameInputKey.Forward), GameInputKey.Forward, new KeyInputChannel(KeyCode.UpArrow));
		_keyMapping[5] = new UserInputMap(GetSlotName(GameInputKey.Left), GameInputKey.Left, new KeyInputChannel(KeyCode.LeftArrow));
		_keyMapping[4] = new UserInputMap(GetSlotName(GameInputKey.Backward), GameInputKey.Backward, new KeyInputChannel(KeyCode.DownArrow));
		_keyMapping[6] = new UserInputMap(GetSlotName(GameInputKey.Right), GameInputKey.Right, new KeyInputChannel(KeyCode.RightArrow));
		_keyMapping[7] = new UserInputMap(GetSlotName(GameInputKey.Jump), GameInputKey.Jump, new KeyInputChannel(KeyCode.Alpha6));
		_keyMapping[8] = new UserInputMap(GetSlotName(GameInputKey.Crouch), GameInputKey.Crouch, new KeyInputChannel(KeyCode.Alpha8));
		_keyMapping[9] = new UserInputMap(GetSlotName(GameInputKey.PrimaryFire), GameInputKey.PrimaryFire, new KeyInputChannel(KeyCode.Alpha1));
		_keyMapping[10] = new UserInputMap(GetSlotName(GameInputKey.SecondaryFire), GameInputKey.SecondaryFire, new KeyInputChannel(KeyCode.Alpha2));
		_keyMapping[19] = new UserInputMap(GetSlotName(GameInputKey.NextWeapon), GameInputKey.NextWeapon, new KeyInputChannel(KeyCode.Alpha7));
		_keyMapping[20] = new UserInputMap(GetSlotName(GameInputKey.PrevWeapon), GameInputKey.PrevWeapon, new KeyInputChannel(KeyCode.Alpha5));
		_keyMapping[26] = new UserInputMap(GetSlotName(GameInputKey.UseQuickItem), GameInputKey.UseQuickItem, new KeyInputChannel(KeyCode.Alpha3));
		_keyMapping[28] = new UserInputMap(GetSlotName(GameInputKey.NextQuickItem), GameInputKey.NextQuickItem, new KeyInputChannel(KeyCode.Alpha4));
	}

	public string InputChannelForSlot(GameInputKey keySlot)
	{
		UserInputMap value;
		if (KeyMapping.TryGetValue((int)keySlot, out value))
		{
			return value.Assignment;
		}
		return "None";
	}
}
