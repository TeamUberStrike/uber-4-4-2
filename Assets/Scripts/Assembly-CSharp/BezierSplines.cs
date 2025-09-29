using System;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class BezierSplines
{
	[Serializable]
	public struct Packet
	{
		public int Time;

		public Vector3 Pos;

		public Vector3 S1;

		public Vector3 S2;

		public float GameTime;

		public Packet(int t, Vector3 p, Vector3 prev)
		{
			Time = t;
			Pos = p;
			S1 = prev * 0.3f;
			S2 = -prev * 0.3f;
			GameTime = UnityEngine.Time.realtimeSinceStartup;
		}
	}

	public readonly LimitedQueue<Packet> Packets = new LimitedQueue<Packet>(10);

	public Packet PreviousPacket;

	public Packet LastPacket;

	public Vector3 _velocity;

	public Vector3 _checkVelocity;

	private Vector3 _lastPosition;

	private int debugCounter;

	public void AddSample(int serverTime, Vector3 pos)
	{
		if (LastPacket.Time < serverTime)
		{
			PreviousPacket = LastPacket;
			LastPacket = new Packet(serverTime, pos, PreviousPacket.Pos - pos);
			Packets.Enqueue(LastPacket);
		}
	}

	public int ReadPosition(int time, out Vector3 oPos)
	{
		int num = 0;
		if (Packets.Count > 0)
		{
			Packet? packet = null;
			Packet? packet2 = null;
			Packet? packet3 = null;
			for (num = Packets.Count - 1; num > 0; num--)
			{
				Packet value = Packets[num];
				packet2 = packet;
				packet = value;
				if (value.Time < time)
				{
					if (!packet2.HasValue && num > 1)
					{
						packet3 = Packets[num - 1];
					}
					break;
				}
			}
			debugCounter = num;
			if (packet.HasValue && packet2.HasValue)
			{
				float num2 = packet2.Value.Time - packet.Value.Time;
				float num3 = time - packet.Value.Time;
				float t = Mathf.Clamp01(Mathf.Abs(num3 / num2));
				oPos = InterpolateBetween(t, packet.Value, packet2.Value);
				_lastPosition = oPos;
			}
			else if (packet.HasValue && packet3.HasValue)
			{
				oPos = packet.Value.Pos + GetVelocity(packet.Value, packet3.Value) * Mathf.Clamp(time - packet.Value.Time, 0, 100);
				_lastPosition = oPos;
			}
			else
			{
				num = 0;
				oPos = _lastPosition;
			}
		}
		else
		{
			oPos = _lastPosition;
		}
		return num;
	}

	private static Vector3 GetVelocity(Packet a, Packet b)
	{
		return (a.Pos - b.Pos) / Mathf.Abs(a.Time - b.Time);
	}

	public Vector3 LatestPosition()
	{
		if (Packets.Count > 0)
		{
			return Packets.LastItem.Pos;
		}
		return Vector3.zero;
	}

	private Vector3 InterpolateBetween(float t, Packet a, Packet b)
	{
		return Vector3.Lerp(a.Pos, b.Pos, t);
	}

	public static Vector3 GetBSplineAt(float t, Vector3 p1, Vector3 ss1, Vector3 ss2, Vector3 p2)
	{
		Vector3 vector = p2 - 3f * ss2 + 3f * ss1 - p1;
		Vector3 vector2 = 3f * ss2 - 6f * ss1 + 3f * p1;
		Vector3 vector3 = 3f * ss1 - 3f * p1;
		return vector * Mathf.Pow(t, 3f) + vector2 * Mathf.Pow(t, 2f) + vector3 * t + p1;
	}
}
