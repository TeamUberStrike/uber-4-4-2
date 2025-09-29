using System;
using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public abstract class LotteryPopupDialog : IPopupDialog
{
	protected enum State
	{
		Normal = 0,
		Rolled = 1
	}

	public enum MyState
	{
		Waiting = 0,
		Success = 1,
		Failed = 2
	}

	private const float BerpSpeed = 2.5f;

	protected int Width = 650;

	protected int Height = 330;

	public bool ClickAnywhereToExit = true;

	protected State _state;

	protected Action _onLotteryRolled;

	protected Action _onLotteryReturned;

	protected bool _showExitButton = true;

	public string Text { get; set; }

	public string Title { get; set; }

	public MyState ReturnedState { get; protected set; }

	public bool IsVisible { get; set; }

	public bool IsUIDisabled { get; set; }

	public bool IsWaiting { get; set; }

	public GuiDepth Depth
	{
		get
		{
			return GuiDepth.Event;
		}
	}

	public void OnGUI()
	{
		Rect position = GetPosition();
		GUI.Box(position, GUIContent.none, BlueStonez.window);
		GUITools.PushGUIState();
		GUI.enabled = !IsUIDisabled;
		GUI.BeginGroup(position);
		if (_showExitButton && GUI.Button(new Rect(position.width - 45f, 0f, 45f, 45f), "X", BlueStonez.friends_hidden_button))
		{
			PopupSystem.HideMessage(this);
		}
		DrawPlayGUI(position);
		GUI.EndGroup();
		GUITools.PopGUIState();
		if (IsWaiting)
		{
			WaitingTexture.Draw(position.center);
		}
		if (ClickAnywhereToExit && Event.current.type == EventType.MouseDown && !position.Contains(Event.current.mousePosition))
		{
			ClosePopup();
			Event.current.Use();
		}
		OnAfterGUI();
	}

	public virtual void OnAfterGUI()
	{
	}

	public void OnHide()
	{
		if (GameState.HasCurrentGame)
		{
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Stop();
		}
		else
		{
			AutoMonoBehaviour<BackgroundMusicPlayer>.Instance.Play(GameAudio.SeletronRadioShort);
		}
	}

	public void SetRollCallback(Action onLotteryRolled)
	{
		_onLotteryRolled = onLotteryRolled;
	}

	public void SetLotteryReturnedCallback(Action onLotteryReturned)
	{
		_onLotteryReturned = onLotteryReturned;
	}

	public abstract LotteryWinningPopup ShowReward();

	protected abstract void DrawPlayGUI(Rect rect);

	protected void DrawNaviArrows(Rect rect, LotteryShopItem item)
	{
		if (GUI.Button(new Rect(rect.width * 0.5f - 95f, rect.height - 42f, 20f, 20f), GUIContent.none, BlueStonez.button_left))
		{
			PopupSystem.HideMessage(this);
			Singleton<LotteryManager>.Instance.ShowPreviousItem(item);
			SfxManager.Play2dAudioClip(GameAudio.ButtonClick);
		}
		if (GUI.Button(new Rect(rect.width * 0.5f + 75f, rect.height - 42f, 20f, 20f), GUIContent.none, BlueStonez.button_right))
		{
			PopupSystem.HideMessage(this);
			Singleton<LotteryManager>.Instance.ShowNextItem(item);
			SfxManager.Play2dAudioClip(GameAudio.ButtonClick);
		}
	}

	protected void ClosePopup()
	{
		PopupSystem.HideMessage(this);
	}

	protected void OpenGetCredits()
	{
		ApplicationDataManager.OpenBuyCredits();
		if (ApplicationDataManager.Channel == ChannelType.MacAppStore)
		{
			PopupSystem.HideMessage(this);
		}
	}

	private Rect GetPosition()
	{
		float left = (float)(Screen.width - Width) * 0.5f;
		float top = (float)GlobalUIRibbon.Instance.Height() + (float)(Screen.height - GlobalUIRibbon.Instance.Height() - Height) * 0.5f;
		return new Rect(left, top, Width, Height);
	}
}
