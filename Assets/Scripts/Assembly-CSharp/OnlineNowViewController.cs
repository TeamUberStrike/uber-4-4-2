using System;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class OnlineNowViewController : MonoBehaviour
{
	[SerializeField]
	private UIEventReceiver seeAllButton;

	[SerializeField]
	private UILabel defaultMessage;

	[SerializeField]
	private List<OnlineFriendViewController> friendViews = new List<OnlineFriendViewController>();

	private Dictionary<CommActorInfo, string> onlineFriends = new Dictionary<CommActorInfo, string>();

	private Vector3 cachedLocalPosition = Vector3.zero;

	private void Awake()
	{
		cachedLocalPosition = base.transform.localPosition;
	}

	private void OnEnable()
	{
		GameData.Instance.Players.AddEventAndFire(HandleFriendsListChanged);
		UIEventReceiver uIEventReceiver = seeAllButton;
		uIEventReceiver.OnClicked = (Action)Delegate.Combine(uIEventReceiver.OnClicked, new Action(HandleSeeAllButtonClicked));
	}

	private void OnDisable()
	{
		GameData.Instance.Players.Changed -= HandleFriendsListChanged;
		UIEventReceiver uIEventReceiver = seeAllButton;
		uIEventReceiver.OnClicked = (Action)Delegate.Remove(uIEventReceiver.OnClicked, new Action(HandleSeeAllButtonClicked));
	}

	private void Start()
	{
		if (!GameData.CanShowFacebookView)
		{
			base.transform.localPosition = Vector3.zero;
		}
		else
		{
			base.transform.localPosition = cachedLocalPosition;
		}
	}

	private void HandleFriendsListChanged(List<CommActorInfo> players)
	{
		FetchOnlineFriends(players);
		UpdateFriendsView();
	}

	private void FetchOnlineFriends(List<CommActorInfo> players)
	{
		onlineFriends.Clear();
		if (players != null)
		{
			UpdateStateForFriends(Singleton<PlayerDataManager>.Instance.FriendList);
			UpdateStateForFriends(Singleton<PlayerDataManager>.Instance.FacebookFriends);
		}
	}

	private void UpdateStateForFriends(IEnumerable<PublicProfileView> friendsList)
	{
		foreach (PublicProfileView friends in friendsList)
		{
			CommActorInfo actor;
			if (CommConnectionManager.IsPlayerOnline(friends.Cmid) && CommConnectionManager.TryGetActor(friends.Cmid, out actor))
			{
				onlineFriends.Add(actor, friends.FacebookId);
			}
		}
	}

	private void UpdateFriendsView()
	{
		foreach (OnlineFriendViewController friendView in friendViews)
		{
			friendView.IsOnline = false;
		}
		if (onlineFriends.Count == 0)
		{
			defaultMessage.gameObject.SetActive(true);
			seeAllButton.gameObject.SetActive(false);
			{
				foreach (OnlineFriendViewController friendView2 in friendViews)
				{
					friendView2.Show(false);
				}
				return;
			}
		}
		int num = 0;
		foreach (KeyValuePair<CommActorInfo, string> onlineFriend in onlineFriends)
		{
			if (onlineFriend.Key != null && num < friendViews.Count)
			{
				friendViews[num].Setup(onlineFriend.Value, onlineFriend.Key);
				num++;
			}
		}
		foreach (OnlineFriendViewController friendView3 in friendViews)
		{
			if (!friendView3.IsOnline)
			{
				friendView3.Show(false);
			}
		}
		seeAllButton.gameObject.SetActive(onlineFriends.Count > friendViews.Count);
		defaultMessage.gameObject.SetActive(false);
	}

	private void HandleSeeAllButtonClicked()
	{
		GameData.Instance.MainMenu.Value = MainMenuState.None;
		MenuPageManager.Instance.LoadPage(PageType.Chat);
		ChatPageGUI.SelectedTab = TabArea.Private;
	}
}
