using System;
using System.Collections.Generic;
using UnityEngine;

public class CallUtil : Singleton<CallUtil>, IUpdatable
{
	private class ScheduledFunction
	{
		public EventFunction Function { get; private set; }

		public float DelayedTime { get; private set; }

		public float CallTime { get; set; }

		public ScheduledFunction(EventFunction function, float time)
		{
			Function = function;
			DelayedTime = time;
			CallTime = Time.time + time;
		}
	}

	private Dictionary<string, ScheduledFunction> _timeoutGroup;

	private Dictionary<string, ScheduledFunction> _intervalGroup;

	private CallUtil()
	{
		_timeoutGroup = new Dictionary<string, ScheduledFunction>();
		_intervalGroup = new Dictionary<string, ScheduledFunction>();
	}

	public string SetTimeout(float time, EventDelegate func, params object[] args)
	{
		string text = Guid.NewGuid().ToString();
		ScheduledFunction value = new ScheduledFunction(new EventFunction(func, args), time);
		_timeoutGroup.Add(text, value);
		return text;
	}

	public string SetInterval(EventDelegate func, float time, params object[] args)
	{
		string text = Guid.NewGuid().ToString();
		ScheduledFunction value = new ScheduledFunction(new EventFunction(func, args), time);
		_intervalGroup.Add(text, value);
		return text;
	}

	public void ClearTimeout(string timeoutId)
	{
		if (_timeoutGroup.ContainsKey(timeoutId))
		{
			_timeoutGroup.Remove(timeoutId);
			return;
		}
		throw new Exception("ClearTimeout - timeout id [" + timeoutId + "] doesn't exist.");
	}

	public void ClearInterval(string intervalId)
	{
		if (_intervalGroup.ContainsKey(intervalId))
		{
			_intervalGroup.Remove(intervalId);
			return;
		}
		throw new Exception("ClearInterval - interval id [" + intervalId + "] doesn't exist.");
	}

	public void Update()
	{
		UpdateTimeoutGroup();
		UpdateIntervalGroup();
	}

	private void UpdateTimeoutGroup()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, ScheduledFunction> item in _timeoutGroup)
		{
			ScheduledFunction value = item.Value;
			if (Time.time > value.CallTime)
			{
				value.Function.Execute();
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			_timeoutGroup.Remove(item2);
		}
	}

	private void UpdateIntervalGroup()
	{
		foreach (ScheduledFunction value in _intervalGroup.Values)
		{
			if (Time.time > value.CallTime)
			{
				value.Function.Execute();
				value.CallTime = Time.time + value.DelayedTime;
			}
		}
	}
}
