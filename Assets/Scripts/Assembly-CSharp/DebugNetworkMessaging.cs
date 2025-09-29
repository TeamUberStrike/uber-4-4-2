using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class DebugNetworkMessaging : IDebugPage
{
	private class MessageInfo
	{
		public DateTime timestamp;

		public int number;

		public MessageInfo(int num)
		{
			number = num;
			timestamp = DateTime.Now;
		}

		public override string ToString()
		{
			return timestamp.ToLongTimeString() + " Messages " + number;
		}
	}

	private Vector2 v1;

	private Queue _outMessageQueue;

	private int _outMaxPackPerSec;

	private float _outAvgPackPerSec;

	private int _outTotalPackages;

	private Queue _inMessageQueue;

	private int _inMaxPackPerSec;

	private float _inAvgPackPerSec;

	private int _inTotalPackages;

	public string Title
	{
		get
		{
			return "Traffic";
		}
	}

	public DebugNetworkMessaging()
	{
		_outMessageQueue = new Queue();
		_inMessageQueue = new Queue();
	}

	private IEnumerator countIncomingMessages()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			int inPerSec = CmuneNetworkState.IncomingMessagesCount - _inTotalPackages;
			_inTotalPackages = CmuneNetworkState.IncomingMessagesCount;
			_inMessageQueue.Enqueue(new MessageInfo(inPerSec));
			if (_inMessageQueue.Count > 10)
			{
				_inMessageQueue.Dequeue();
			}
			_inAvgPackPerSec = 0f;
			foreach (MessageInfo i in _inMessageQueue)
			{
				_inAvgPackPerSec += i.number;
			}
			_inAvgPackPerSec /= _inMessageQueue.Count;
			if (_inMaxPackPerSec < inPerSec)
			{
				_inMaxPackPerSec = inPerSec;
			}
		}
	}

	private IEnumerator countOutgoingMessages()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			int outPerSec = CmuneNetworkState.OutgoingMessagesCount - _outTotalPackages;
			_outTotalPackages = CmuneNetworkState.OutgoingMessagesCount;
			_outMessageQueue.Enqueue(new MessageInfo(outPerSec));
			if (_outMessageQueue.Count > 10)
			{
				_outMessageQueue.Dequeue();
			}
			_outAvgPackPerSec = 0f;
			foreach (MessageInfo i in _outMessageQueue)
			{
				_outAvgPackPerSec += i.number;
			}
			_outAvgPackPerSec /= _outMessageQueue.Count;
			if (_outMaxPackPerSec < outPerSec)
			{
				_outMaxPackPerSec = outPerSec;
			}
		}
	}

	public void Draw()
	{
		GUILayout.Label("OUT (tot):" + _outTotalPackages);
		GUILayout.Label("OUT (avg):" + _outAvgPackPerSec);
		GUILayout.Label("OUT (max):" + _outMaxPackPerSec);
		GUILayout.Label("IN (tot):" + _inTotalPackages);
		GUILayout.Label("IN (avg):" + _inAvgPackPerSec);
		GUILayout.Label("IN (max):" + _inMaxPackPerSec);
		if (GUILayout.Button("Dump To File"))
		{
			FileStream fileStream = new FileStream("NetworkMessages.txt", FileMode.OpenOrCreate, FileAccess.Write);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			try
			{
				foreach (KeyValuePair<short, NetworkMessenger.NetworkClassInfo> callStatistic in LobbyConnectionManager.Rmi.Messenger.CallStatistics)
				{
					foreach (KeyValuePair<byte, int> functionCall in callStatistic.Value._functionCalls)
					{
						string value = string.Format("{0}\t{1}\t{2}\t{3}", functionCall.Value, LobbyConnectionManager.Rmi.GetAddress(callStatistic.Key, functionCall.Key), callStatistic.Value.GetTotalExecutionTime(functionCall.Key), callStatistic.Value.GetAvarageExecutionTime(functionCall.Key));
						streamWriter.WriteLine(value);
					}
				}
			}
			finally
			{
				streamWriter.Close();
				fileStream.Close();
			}
		}
		v1 = GUILayout.BeginScrollView(v1);
		foreach (KeyValuePair<short, NetworkMessenger.NetworkClassInfo> callStatistic2 in LobbyConnectionManager.Rmi.Messenger.CallStatistics)
		{
			foreach (KeyValuePair<byte, int> functionCall2 in callStatistic2.Value._functionCalls)
			{
				string text = string.Format("{0} {1}: [{2}ms /{3}ms]", functionCall2.Value, LobbyConnectionManager.Rmi.GetAddress(callStatistic2.Key, functionCall2.Key), callStatistic2.Value.GetTotalExecutionTime(functionCall2.Key), callStatistic2.Value.GetAvarageExecutionTime(functionCall2.Key));
				GUILayout.Label(text);
			}
		}
		GUILayout.EndScrollView();
	}
}
