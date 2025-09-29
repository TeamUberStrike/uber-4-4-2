using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class EventFeedbackHud : Singleton<EventFeedbackHud>
{
	private class FeedbackMessage
	{
		public InGameEventFeedbackType Type { get; private set; }

		public string Message { get; private set; }

		public float Time { get; private set; }

		public FeedbackMessage(string msg, InGameEventFeedbackType type, float time)
		{
			Message = msg;
			Type = type;
			Time = time;
		}
	}

	private float _textScale;

	private Queue<FeedbackMessage> _eventFeedbackQueue;

	private FeedbackMessage _curShownMessage;

	private MeshGUITextFormat _feedbackText;

	private float _defaultTextDisplayTime;

	private float _animFadeOutTime;

	private float _lastEventShownTime;

	private bool _isFadingOutText;

	public bool Enabled
	{
		get
		{
			return _feedbackText.IsVisible;
		}
		set
		{
			if (value)
			{
				_feedbackText.Show();
			}
			else
			{
				_feedbackText.Hide();
			}
		}
	}

	private FeedbackMessage CurrentFeedbackMessage
	{
		get
		{
			return _curShownMessage;
		}
		set
		{
			_curShownMessage = value;
			if (_curShownMessage != null)
			{
				_feedbackText.FormatText = _curShownMessage.Message;
				ResetHud();
			}
		}
	}

	private EventFeedbackHud()
	{
		_animFadeOutTime = 1f;
		_defaultTextDisplayTime = 2f;
		_eventFeedbackQueue = new Queue<FeedbackMessage>();
		CurrentFeedbackMessage = null;
		_feedbackText = new MeshGUITextFormat(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAlignment.Center);
		ResetHud();
		Enabled = true;
		CmuneEventHandler.AddListener<CameraWidthChangeEvent>(OnCameraRectChange);
	}

	public void Draw()
	{
		if (Enabled)
		{
			ScheduleEventFeedback();
			DrawFeedbackMessage();
		}
	}

	public void EnqueueFeedback(InGameEventFeedbackType eventFeedbackType, string customMessage)
	{
		EnqueueFeedback(eventFeedbackType, customMessage, _defaultTextDisplayTime);
	}

	public void EnqueueFeedback(InGameEventFeedbackType eventFeedbackType, string customMessage, float time)
	{
		_eventFeedbackQueue.Enqueue(new FeedbackMessage(customMessage, eventFeedbackType, time));
	}

	public void ClearQueue()
	{
		_eventFeedbackQueue.Clear();
	}

	public void ClearAll()
	{
		_eventFeedbackQueue.Clear();
		_feedbackText.FormatText = string.Empty;
	}

	private void OnCameraRectChange(CameraWidthChangeEvent ev)
	{
		ResetTransform();
	}

	private void ScheduleEventFeedback()
	{
		if (_eventFeedbackQueue.Count > 0 && CurrentFeedbackMessage == null)
		{
			CurrentFeedbackMessage = _eventFeedbackQueue.Dequeue();
			_lastEventShownTime = Time.time;
			_isFadingOutText = false;
		}
	}

	private void DrawFeedbackMessage()
	{
		if (CurrentFeedbackMessage != null)
		{
			if (Time.time < _lastEventShownTime + CurrentFeedbackMessage.Time)
			{
				if (Time.time > _lastEventShownTime + CurrentFeedbackMessage.Time - _animFadeOutTime && !_isFadingOutText)
				{
					FadeOutFeedbackText();
				}
			}
			else
			{
				CurrentFeedbackMessage = null;
			}
		}
		_feedbackText.Draw();
		_feedbackText.ShadowColorAnim.Alpha = 0f;
	}

	private void FadeOutFeedbackText()
	{
		_feedbackText.FadeAlphaTo(0f, _animFadeOutTime, EaseType.Out);
		_isFadingOutText = true;
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		foreach (IAnimatable2D item in _feedbackText.Group)
		{
			MeshGUIText noShadowStyle = item as MeshGUIText;
			Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(noShadowStyle);
		}
	}

	private void ResetTransform()
	{
		_textScale = 0.45f;
		_feedbackText.Scale = new Vector2(_textScale, _textScale);
		_feedbackText.LineGap = _feedbackText.LineHeight * _textScale * _textScale;
		_feedbackText.Position = new Vector2(Screen.width / 2, (float)Screen.height * 0.3f + (float)Screen.height * 0.65f * (1f - AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth));
	}
}
