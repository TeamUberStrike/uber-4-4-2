using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class AxisInputChannel : IByteArray, IInputChannel
{
	public enum AxisReadingMethod
	{
		All = 0,
		PositiveOnly = 1,
		NegativeOnly = 2
	}

	private string _axis = string.Empty;

	private string _axisName = string.Empty;

	private float _value;

	private float _lastValue;

	private float _deadRange = 0.1f;

	private AxisReadingMethod _axisReading;

	public string Axis
	{
		get
		{
			return _axis;
		}
	}

	public string Name
	{
		get
		{
			return _axisName;
		}
	}

	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = (_lastValue = value);
		}
	}

	public InputChannelType ChannelType
	{
		get
		{
			return InputChannelType.Axis;
		}
	}

	public bool IsPressed
	{
		get
		{
			return _value != 0f;
		}
	}

	public bool IsChanged
	{
		get
		{
			return _lastValue != _value;
		}
	}

	public AxisInputChannel(byte[] bytes, ref int idx)
	{
		idx = FromBytes(bytes, idx);
	}

	public AxisInputChannel(string axis)
	{
		_axis = axis;
		_axisName = _axis;
	}

	public AxisInputChannel(string axis, float deadRange)
		: this(axis)
	{
		_deadRange = deadRange;
	}

	public AxisInputChannel(string axis, float deadRange, AxisReadingMethod method)
		: this(axis, deadRange)
	{
		_axisReading = method;
		string[] array = _axisName.Split(' ');
		switch (method)
		{
		case AxisReadingMethod.NegativeOnly:
			if (array.Length > 1)
			{
				if (array[1].Equals("X"))
				{
					_axisName = string.Format("{0} L", array[0]);
				}
				else if (array[1].Equals("Y"))
				{
					_axisName = string.Format("{0} D", array[0]);
				}
			}
			break;
		case AxisReadingMethod.PositiveOnly:
			if (array.Length > 1)
			{
				if (array[1].Equals("X"))
				{
					_axisName = string.Format("{0} R", array[0]);
				}
				else if (array[1].Equals("Y"))
				{
					_axisName = string.Format("{0} U", array[0]);
				}
			}
			break;
		}
	}

	public void Listen()
	{
		_lastValue = _value;
		_value = RawValue();
		switch (_axisReading)
		{
		case AxisReadingMethod.NegativeOnly:
			if (_value > 0f)
			{
				_value = 0f;
			}
			break;
		case AxisReadingMethod.PositiveOnly:
			if (_value < 0f)
			{
				_value = 0f;
			}
			break;
		}
		if (Mathf.Abs(_value) < _deadRange)
		{
			_value = 0f;
		}
	}

	public void Reset()
	{
		_value = 0f;
		_lastValue = 0f;
	}

	public float RawValue()
	{
		return Input.GetAxis(_axis);
	}

	public override string ToString()
	{
		return _axis;
	}

	public override bool Equals(object obj)
	{
		if (obj is AxisInputChannel)
		{
			AxisInputChannel axisInputChannel = obj as AxisInputChannel;
			if (axisInputChannel.Axis == Axis)
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

	public int FromBytes(byte[] bytes, int idx)
	{
		_axis = DefaultByteConverter.ToString(bytes, ref idx);
		_deadRange = DefaultByteConverter.ToFloat(bytes, ref idx);
		_axisReading = (AxisReadingMethod)DefaultByteConverter.ToInt(bytes, ref idx);
		switch (_axisReading)
		{
		case AxisReadingMethod.NegativeOnly:
			_axisName = _axis + " Up";
			break;
		case AxisReadingMethod.PositiveOnly:
			_axisName = _axis + " Down";
			break;
		default:
			_axisName = _axis;
			break;
		}
		return idx;
	}

	public byte[] GetBytes()
	{
		List<byte> bytes = new List<byte>();
		DefaultByteConverter.FromString(_axis, ref bytes);
		DefaultByteConverter.FromFloat(_deadRange, ref bytes);
		DefaultByteConverter.FromInt((int)_axisReading, ref bytes);
		return bytes.ToArray();
	}
}
