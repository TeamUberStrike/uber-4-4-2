using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class FastSecureInteger
{
	private const float syncFrequency = 1f;

	private SecureMemory<int> secureValue;

	private int deltaValue;

	private float nextUpdate;

	public int Value
	{
		get
		{
			if (deltaValue != 0 && nextUpdate < Time.time)
			{
				Value = secureValue.ReadData(true) + deltaValue;
			}
			return secureValue.ReadData(false) + deltaValue;
		}
		set
		{
			secureValue.WriteData(value);
			deltaValue = 0;
			nextUpdate = Time.time + 1f;
		}
	}

	public FastSecureInteger(int value)
	{
		secureValue = new SecureMemory<int>(0, true, ApplicationDataManager.Channel == ChannelType.IPad || ApplicationDataManager.Channel == ChannelType.IPhone);
		secureValue.WriteData(value);
		deltaValue = 0;
	}

	public void Decrement(int value)
	{
		deltaValue -= value;
	}

	public void Increment(int value)
	{
		deltaValue += value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
