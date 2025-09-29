using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UserInputMap
{
	private KeyCode _prefix;

	public GameInputKey Slot { get; private set; }

	public string Description { get; private set; }

	public string Assignment
	{
		get
		{
			if (Channel == null)
			{
				return "None";
			}
			return (_prefix == KeyCode.None) ? Channel.Name : string.Format("{0} + {1}", PrintKeyCode(_prefix), Channel.Name);
		}
	}

	public IInputChannel Channel { get; set; }

	public bool IsConfigurable { get; set; }

	public float Value
	{
		get
		{
			if (Channel != null)
			{
				return (_prefix != KeyCode.None && !Input.GetKey(_prefix)) ? 0f : Channel.Value;
			}
			return 0f;
		}
	}

	public bool IsEventSender { get; private set; }

	public UserInputMap(string description, GameInputKey s, IInputChannel channel = null, bool isConfigurable = true, bool isEventSender = true, KeyCode prefix = KeyCode.None)
	{
		_prefix = prefix;
		IsConfigurable = isConfigurable;
		IsEventSender = isEventSender;
		Channel = channel;
		Slot = s;
		Description = description;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(Description);
		stringBuilder.AppendFormat(": {0}", Channel);
		return stringBuilder.ToString();
	}

	public int SetChannel(byte[] bytes, int idx)
	{
		byte b = bytes[idx++];
		switch (b)
		{
		case 0:
			Channel = new KeyInputChannel(bytes, ref idx);
			break;
		case 1:
			Channel = new MouseInputChannel(bytes, ref idx);
			break;
		case 2:
			Channel = new AxisInputChannel(bytes, ref idx);
			break;
		case 3:
			if (b == 3)
			{
				Channel = new ButtonInputChannel(bytes, ref idx);
			}
			break;
		default:
			Debug.LogError("KeyMap deserialization failed");
			break;
		}
		return idx;
	}

	public byte[] GetChannel()
	{
		List<byte> list = new List<byte>();
		if (Channel is KeyInputChannel)
		{
			list.Add(0);
		}
		else if (Channel is MouseInputChannel)
		{
			list.Add(1);
		}
		else if (Channel is AxisInputChannel)
		{
			list.Add(2);
		}
		else if (Channel is ButtonInputChannel)
		{
			list.Add(3);
		}
		else
		{
			list.Add(byte.MaxValue);
		}
		if (Channel != null)
		{
			list.AddRange(Channel.GetBytes());
		}
		return list.ToArray();
	}

	public string GetPrefString()
	{
		return WWW.EscapeURL(Encoding.ASCII.GetString(GetChannel()), Encoding.ASCII);
	}

	public void FromPrefString(string pref)
	{
		SetChannel(Encoding.ASCII.GetBytes(WWW.UnEscapeURL(pref, Encoding.ASCII)), 0);
	}

	private string PrintKeyCode(KeyCode keyCode)
	{
		switch (keyCode)
		{
		case KeyCode.LeftAlt:
			return "Left Alt";
		case KeyCode.LeftControl:
			return "Left Ctrl";
		case KeyCode.LeftShift:
			return "Left Shift";
		case KeyCode.RightAlt:
			return "Right Alt";
		case KeyCode.RightControl:
			return "Right Ctrl";
		case KeyCode.RightShift:
			return "Right Shift";
		default:
			return keyCode.ToString();
		}
	}

	public float RawValue()
	{
		if (Channel != null && (_prefix == KeyCode.None || Input.GetKey(_prefix)))
		{
			return Channel.RawValue();
		}
		return 0f;
	}
}
