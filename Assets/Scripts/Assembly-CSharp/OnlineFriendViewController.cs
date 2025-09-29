using System;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class OnlineFriendViewController : MonoBehaviour
{
	[SerializeField]
	private UIRemoteTexture picture;

	[SerializeField]
	private UIEventReceiver joinButton;

	[SerializeField]
	private UIEventReceiver chatButton;

	[SerializeField]
	private UILabel nameLabel;

	[SerializeField]
	private UILabel statusLabel;

	private CommActorInfo user;

	public bool IsOnline { get; set; }

	public UIRemoteTexture Picture
	{
		get
		{
			return picture;
		}
	}

	private void OnEnable()
	{
		UIEventReceiver uIEventReceiver = joinButton;
		uIEventReceiver.OnClicked = (Action)Delegate.Combine(uIEventReceiver.OnClicked, new Action(HandleJoinClicked));
		UIEventReceiver uIEventReceiver2 = chatButton;
		uIEventReceiver2.OnClicked = (Action)Delegate.Combine(uIEventReceiver2.OnClicked, new Action(HandleChatClicked));
	}

	private void OnDisable()
	{
		UIEventReceiver uIEventReceiver = joinButton;
		uIEventReceiver.OnClicked = (Action)Delegate.Remove(uIEventReceiver.OnClicked, new Action(HandleJoinClicked));
		UIEventReceiver uIEventReceiver2 = chatButton;
		uIEventReceiver2.OnClicked = (Action)Delegate.Remove(uIEventReceiver2.OnClicked, new Action(HandleChatClicked));
	}

	public void Setup(string facebookID, CommActorInfo commUser)
	{
		user = commUser;
		picture.gameObject.SetActive(true);
		nameLabel.gameObject.SetActive(true);
		if (string.IsNullOrEmpty(facebookID))
		{
			picture.ShowDefault();
		}
		else
		{
			picture.Url = "http://graph.facebook.com/" + facebookID + "/picture?width=128&height=128";
		}
		nameLabel.text = user.PlayerName;
		CanJoin(user.IsInGame);
		IsOnline = true;
	}

	public void Show(bool bShow)
	{
		joinButton.gameObject.SetActive(bShow);
		chatButton.gameObject.SetActive(bShow);
		picture.gameObject.SetActive(bShow);
		nameLabel.gameObject.SetActive(bShow);
		statusLabel.gameObject.SetActive(false);
	}

	private void CanJoin(bool bJoin)
	{
		joinButton.gameObject.SetActive(bJoin);
		chatButton.gameObject.SetActive(!bJoin);
		statusLabel.gameObject.SetActive(false);
	}

	private void HandleJoinClicked()
	{
		if (user != null)
		{
			if (user.IsInGame && user.CurrentRoom.Number != 88 && !user.CurrentRoom.IsEmpty)
			{
				joinButton.gameObject.SetActive(false);
				statusLabel.gameObject.SetActive(true);
				GameConnectionManager.RequestRoomMetaData(user.CurrentRoom, OnRequestRoomMetaData);
			}
			else
			{
				CanJoin(false);
			}
		}
		else
		{
			Show(false);
		}
	}

	private void HandleChatClicked()
	{
		if (user != null)
		{
			GameData.Instance.MainMenu.Value = MainMenuState.None;
			MenuPageManager.Instance.LoadPage(PageType.Chat);
			Singleton<ChatManager>.Instance.CreatePrivateChat(user.Cmid);
		}
		else
		{
			Show(false);
		}
	}

	private void OnRequestRoomMetaData(int returncode, GameMetaData data)
	{
		switch (returncode)
		{
		case 0:
			if (PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA)
			{
				Singleton<GameStateController>.Instance.JoinGame(data);
			}
			else if (JoinGameUtil.IsMobileChannel(user.Channel) && !JoinGameUtil.IsMobileChannel(ApplicationDataManager.Channel))
			{
				PopupSystem.ShowMessage("Error Joining Game Server", "Sorry, only mobile players can join mobile game servers.");
			}
			else if (data.IsFull)
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ThisGameIsFull, PopupSystem.AlertType.OK, null);
			}
			else if (data.IsLevelAllowed(PlayerDataManager.PlayerLevelSecure))
			{
				if (data.IsPublic)
				{
					Singleton<GameStateController>.Instance.JoinGame(data);
				}
			}
			else
			{
				PopupSystem.ShowMessage(LocalizedStrings.Error, string.Format(LocalizedStrings.YouHaveToReachLevelNToJoinThisGame, data.LevelMin));
			}
			break;
		case 1:
			CanJoin(false);
			PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ThisGameNoLongerExists, PopupSystem.AlertType.OK, null);
			break;
		case 2:
			CanJoin(false);
			PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ServerIsNotReachable, PopupSystem.AlertType.OK, null);
			break;
		}
	}
}
