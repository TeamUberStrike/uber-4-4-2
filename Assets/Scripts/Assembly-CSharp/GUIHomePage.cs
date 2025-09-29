using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

internal class GUIHomePage : GUIPageBase
{
	[SerializeField]
	private UIEventReceiver play;

	[SerializeField]
	private UIEventReceiver shopButton;

	[SerializeField]
	private UIEventReceiver featuredButton;

	[SerializeField]
	private UIEventReceiver profileButton;

	[SerializeField]
	private UIEventReceiver clansButton;

	[SerializeField]
	private UIEventReceiver chatButton;

	[SerializeField]
	private UIEventReceiver inboxButton;

	[SerializeField]
	private UIRemoteTexture featuredTexture;

	[SerializeField]
	private UITweener inboxFlashTween;

	[SerializeField]
	private UITweener chatFlashTween;

	private void OnEnable()
	{
		Singleton<InboxManager>.Instance.UnreadMessageCount.AddEventAndFire(HandlePendingInboxMessages);
		Singleton<InboxManager>.Instance.FriendRequests.AddEventAndFire(HandlePendingFriendRequests);
		Singleton<InboxManager>.Instance.IncomingClanRequests.AddEventAndFire(HandlePendingClanRequests);
		Singleton<ChatManager>.Instance.HasUnreadClanMessage.AddEventAndFire(HandlePendingChatMessages);
		Singleton<ChatManager>.Instance.HasUnreadPrivateMessage.AddEventAndFire(HandlePendingChatMessages);
	}

	private void OnDisable()
	{
		Singleton<InboxManager>.Instance.UnreadMessageCount.Changed -= HandlePendingInboxMessages;
		Singleton<InboxManager>.Instance.FriendRequests.Changed -= HandlePendingFriendRequests;
		Singleton<InboxManager>.Instance.IncomingClanRequests.Changed -= HandlePendingClanRequests;
		Singleton<ChatManager>.Instance.HasUnreadClanMessage.Changed -= HandlePendingChatMessages;
		Singleton<ChatManager>.Instance.HasUnreadPrivateMessage.Changed -= HandlePendingChatMessages;
	}

	private void Start()
	{
		string url = string.Concat(ApplicationDataManager.Config.ContentRouterBaseUrl, ApplicationDataManager.Config.ChannelType, "/home_screen_large?cmid=", PlayerDataManager.CmidSecure);
		featuredTexture.Url = url;
		play.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Play);
			});
		};
		shopButton.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Shop);
			});
		};
		featuredButton.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Shop);
				CmuneEventHandler.Route(new SelectShopAreaEvent
				{
					ShopArea = ShopArea.Shop,
					ItemType = UberstrikeItemType.Special,
					ItemClass = UberstrikeItemClass.SpecialGeneral
				});
			});
		};
		profileButton.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Stats);
			});
		};
		clansButton.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Clans);
			});
		};
		chatButton.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Chat);
			});
		};
		inboxButton.OnClicked = delegate
		{
			Dismiss(delegate
			{
				GameData.Instance.MainMenu.Value = MainMenuState.None;
				MenuPageManager.Instance.LoadPage(PageType.Inbox);
			});
		};
	}

	private void HandlePendingInboxMessages(int unreadMessages)
	{
		FlashInbox(unreadMessages > 0 || Singleton<InboxManager>.Instance.FriendRequests.Value.Count > 0 || Singleton<InboxManager>.Instance.IncomingClanRequests.Value.Count > 0);
	}

	private void HandlePendingFriendRequests(List<ContactRequestView> friendRequests)
	{
		FlashInbox(friendRequests.Count > 0 || Singleton<InboxManager>.Instance.IncomingClanRequests.Value.Count > 0 || (int)Singleton<InboxManager>.Instance.UnreadMessageCount > 0);
	}

	private void HandlePendingClanRequests(List<GroupInvitationView> clanRequests)
	{
		FlashInbox(clanRequests.Count > 0 || Singleton<InboxManager>.Instance.FriendRequests.Value.Count > 0 || (int)Singleton<InboxManager>.Instance.UnreadMessageCount > 0);
	}

	private void HandlePendingChatMessages(bool hasUnreadMessages)
	{
		FlashChat((bool)Singleton<ChatManager>.Instance.HasUnreadClanMessage || (bool)Singleton<ChatManager>.Instance.HasUnreadPrivateMessage);
	}

	private void FlashInbox(bool bFlash)
	{
		FlashMenuIcon(inboxFlashTween, bFlash);
	}

	private void FlashChat(bool bFlash)
	{
		FlashMenuIcon(chatFlashTween, bFlash);
	}

	private void FlashMenuIcon(UITweener buttonTween, bool bFlash)
	{
		if (!(buttonTween == null))
		{
			if (!bFlash)
			{
				buttonTween.Reset();
				buttonTween.gameObject.GetComponent<UISprite>().alpha = 1f;
			}
			buttonTween.enabled = bFlash;
		}
	}

	protected override IEnumerator OnBringIn()
	{
		float duration = bringInDuration / 5f;
		yield return StartCoroutine(AnimateAlpha(1f, duration, play));
		yield return StartCoroutine(AnimateAlpha(1f, duration, shopButton));
		yield return StartCoroutine(AnimateAlpha(1f, duration, featuredButton));
		yield return StartCoroutine(AnimateAlpha(1f, duration, profileButton, clansButton, chatButton, inboxButton));
	}

	protected override IEnumerator OnDismiss()
	{
		float duration = dismissDuration;
		yield return StartCoroutine(AnimateAlpha(0f, duration, profileButton, clansButton, chatButton, inboxButton, featuredButton, shopButton, play));
	}
}
