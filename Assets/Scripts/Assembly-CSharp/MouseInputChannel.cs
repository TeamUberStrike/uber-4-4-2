using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class MouseInputChannel : IByteArray, IInputChannel
{
	private bool _wasDown;

	private bool _isDown;

	private int _button;

	public int Button
	{
		get
		{
			return _button;
		}
	}

	public string Name
	{
		get
		{
			return ConvertMouseButtonName(_button);
		}
	}

	public InputChannelType ChannelType
	{
		get
		{
			return InputChannelType.Mouse;
		}
	}

	public float Value
	{
		get
		{
			return _isDown ? 1 : 0;
		}
		set
		{
			_isDown = (_wasDown = ((value != 0f) ? true : false));
		}
	}

	public bool IsChanged
	{
		get
		{
			return _isDown != _wasDown;
		}
	}

	public MouseInputChannel(byte[] bytes, ref int idx)
	{
		idx = FromBytes(bytes, idx);
	}

	public MouseInputChannel(int button)
	{
		_button = button;
	}

	public void Listen()
	{
		_wasDown = _isDown;
		_isDown = Input.GetMouseButton(_button);
	}

	public float RawValue()
	{
		return Input.GetMouseButton(_button) ? 1 : 0;
	}

	public void Reset()
	{
		_wasDown = false;
		_isDown = false;
	}

	public override string ToString()
	{
		return string.Format("Mouse {0}", _button);
	}

	public override bool Equals(object obj)
	{
		if (obj is MouseInputChannel)
		{
			MouseInputChannel mouseInputChannel = obj as MouseInputChannel;
			if (mouseInputChannel.Button == Button)
			{
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private string ConvertMouseButtonName(int _button)
	{
		switch (_button)
		{
		case 0:
			return "Left Mousebutton";
		case 1:
			return "Right Mousebutton";
		default:
			return string.Format("Mouse {0}", _button);
		}
	}

	public int FromBytes(byte[] bytes, int idx)
	{
		_button = DefaultByteConverter.ToInt(bytes, ref idx);
		return idx;
	}

	public byte[] GetBytes()
	{
		List<byte> bytes = new List<byte>(4);
		DefaultByteConverter.FromInt(_button, ref bytes);
		return bytes.ToArray();
	}
}
