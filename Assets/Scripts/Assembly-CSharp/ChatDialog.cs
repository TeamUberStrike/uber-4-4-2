using System.Collections.Generic;
using System.Text;
using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public class ChatDialog
{
	public delegate bool CanShowMessage(ChatContext c);

	public CanShowMessage CanShow;

	public List<float> _msgHeight;

	public Queue<InstantMessage> _msgQueue;

	private bool _reset;

	private string _title;

	private Vector2 _scroll;

	public Vector2 _frameSize;

	public Vector2 _contentSize;

	public float _heightCache;

	public bool CanChat
	{
		get
		{
			return UserCmid == 0 || CommConnectionManager.IsPlayerOnline(UserCmid);
		}
	}

	public string Title
	{
		get
		{
			if (UserCmid > 0)
			{
				return (!CommConnectionManager.IsPlayerOnline(UserCmid)) ? (UserName + " is offline") : ("Chat with " + UserName);
			}
			return _title;
		}
		set
		{
			_title = value;
		}
	}

	public string UserName { get; private set; }

	public int UserCmid { get; private set; }

	public UserGroups Group { get; set; }

	public bool HasUnreadMessage { get; set; }

	public ICollection<InstantMessage> AllMessages
	{
		get
		{
			return new List<InstantMessage>(_msgQueue.ToArray());
		}
	}

	public ChatDialog(string title)
	{
		Title = title;
		UserName = string.Empty;
		_msgHeight = new List<float>();
		_msgQueue = new Queue<InstantMessage>();
		AddMessage(new InstantMessage(0, 0, "Disclaimer", "Do not share your password or any other confidential information with anybody. The members of Cmune and the Uberstrike Moderators will never ask you to provide such information.", MemberAccessLevel.Admin));
	}

	public ChatDialog(CommUser user, UserGroups group)
		: this(string.Empty)
	{
		Group = group;
		if (user != null)
		{
			UserName = user.ShortName;
			UserCmid = user.Cmid;
		}
	}

	public void AddMessage(InstantMessage msg)
	{
		_reset = true;
		while (_msgQueue.Count > 200)
		{
			_msgQueue.Dequeue();
		}
		_msgQueue.Enqueue(msg);
		if (!Input.GetMouseButton(0))
		{
			_scroll.y = float.MaxValue;
		}
	}

	public void ScrollToEnd()
	{
		_scroll.y = float.PositiveInfinity;
	}

	public void Clear()
	{
		_msgQueue.Clear();
	}

	public void CheckSize(Rect rect)
	{
		if (!_reset && rect.width == _frameSize.x && rect.height == _frameSize.y)
		{
			return;
		}
		float num = 0f;
		_reset = false;
		_frameSize.x = rect.width;
		_frameSize.y = rect.height;
		_contentSize.x = rect.width;
		_contentSize.y = rect.height;
		_msgHeight.Clear();
		foreach (InstantMessage item in _msgQueue)
		{
			float num2 = BlueStonez.label_interparkbold_11pt_left_wrap.CalcHeight(new GUIContent(item.MessageText), _contentSize.x - 8f);
			num2 += 24f;
			_msgHeight.Add(num2);
			num += num2;
		}
		if (!(num > rect.height))
		{
			return;
		}
		num = 0f;
		_msgHeight.Clear();
		_contentSize.x = rect.width - 17f;
		foreach (InstantMessage item2 in _msgQueue)
		{
			float num3 = BlueStonez.label_interparkbold_11pt_left_wrap.CalcHeight(new GUIContent(item2.MessageText), _contentSize.x - 8f);
			num3 += 24f;
			_msgHeight.Add(num3);
			num += num3;
		}
		_contentSize.y = num;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Title: " + Title);
		stringBuilder.AppendLine("Group: " + Group);
		stringBuilder.AppendLine("User: " + UserName + " " + UserCmid);
		stringBuilder.AppendLine("CanChat: " + CanChat);
		return stringBuilder.ToString();
	}
}
