using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.WebService.Unity;
using UnityEngine;

public class FBViewController : MonoBehaviour
{
	private const string GIFT_DATA = "gift_item_4.4.x";

	private const string GIFT_MESSAGE = "I've just sent you a mystery gift in UberStrike. Jump in game and claim it to see what it is!";

	[SerializeField]
	private GameObject facebookConnectBox;

	[SerializeField]
	private GameObject helpMyFriendsBox;

	[SerializeField]
	private GameObject receiveGiftsBox;

	[SerializeField]
	private UIEventReceiver facebookConnectButton;

	[SerializeField]
	private UIEventReceiver sendGiftsButton;

	[SerializeField]
	private UIEventReceiver inviteFriendsButton;

	[SerializeField]
	private UIEventReceiver acceptButton;

	[SerializeField]
	private UIEventReceiver declineButton;

	[SerializeField]
	private UIEventReceiver acceptAndThanksButton;

	[SerializeField]
	private UILabel messageLabel;

	[SerializeField]
	private UILabel badgeNumber;

	[SerializeField]
	private UILabel statusLabel;

	[SerializeField]
	private UILabel orLabel;

	[SerializeField]
	private UIRemoteTexture avatar;

	private List<FacebookRequest> gifts = new List<FacebookRequest>();

	private FacebookRequest currentGift;

	private void OnEnable()
	{
		UIEventReceiver uIEventReceiver = facebookConnectButton;
		uIEventReceiver.OnClicked = (Action)Delegate.Combine(uIEventReceiver.OnClicked, new Action(ConnectToFacebook));
		UIEventReceiver uIEventReceiver2 = sendGiftsButton;
		uIEventReceiver2.OnClicked = (Action)Delegate.Combine(uIEventReceiver2.OnClicked, new Action(OpenSendGiftsDialog));
		UIEventReceiver uIEventReceiver3 = inviteFriendsButton;
		uIEventReceiver3.OnClicked = (Action)Delegate.Combine(uIEventReceiver3.OnClicked, new Action(OpenInviteFriendsDialog));
		UIEventReceiver uIEventReceiver4 = acceptButton;
		uIEventReceiver4.OnClicked = (Action)Delegate.Combine(uIEventReceiver4.OnClicked, new Action(HandleAcceptGift));
		UIEventReceiver uIEventReceiver5 = declineButton;
		uIEventReceiver5.OnClicked = (Action)Delegate.Combine(uIEventReceiver5.OnClicked, new Action(HandleDeclineGift));
		UIEventReceiver uIEventReceiver6 = acceptAndThanksButton;
		uIEventReceiver6.OnClicked = (Action)Delegate.Combine(uIEventReceiver6.OnClicked, new Action(HandleAcceptAndThank));
		AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn.AddEventAndFire(HandleFacebookLoggedIn);
		AutoMonoBehaviour<FacebookInterface>.Instance.Requests.Changed += HandlePendingRequestsChanged;
	}

	private void OnDisable()
	{
		UIEventReceiver uIEventReceiver = facebookConnectButton;
		uIEventReceiver.OnClicked = (Action)Delegate.Remove(uIEventReceiver.OnClicked, new Action(ConnectToFacebook));
		UIEventReceiver uIEventReceiver2 = sendGiftsButton;
		uIEventReceiver2.OnClicked = (Action)Delegate.Remove(uIEventReceiver2.OnClicked, new Action(OpenSendGiftsDialog));
		UIEventReceiver uIEventReceiver3 = inviteFriendsButton;
		uIEventReceiver3.OnClicked = (Action)Delegate.Remove(uIEventReceiver3.OnClicked, new Action(OpenInviteFriendsDialog));
		UIEventReceiver uIEventReceiver4 = acceptButton;
		uIEventReceiver4.OnClicked = (Action)Delegate.Remove(uIEventReceiver4.OnClicked, new Action(HandleAcceptGift));
		UIEventReceiver uIEventReceiver5 = declineButton;
		uIEventReceiver5.OnClicked = (Action)Delegate.Remove(uIEventReceiver5.OnClicked, new Action(HandleDeclineGift));
		UIEventReceiver uIEventReceiver6 = acceptAndThanksButton;
		uIEventReceiver6.OnClicked = (Action)Delegate.Remove(uIEventReceiver6.OnClicked, new Action(HandleAcceptAndThank));
		AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn.Changed -= HandleFacebookLoggedIn;
		AutoMonoBehaviour<FacebookInterface>.Instance.Requests.Changed -= HandlePendingRequestsChanged;
	}

	private void HandleFacebookLoggedIn(bool isloggedIn)
	{
		if (GameData.CanShowFacebookView && !(helpMyFriendsBox == null) && !(facebookConnectBox == null) && !(receiveGiftsBox == null))
		{
			if (isloggedIn)
			{
				helpMyFriendsBox.SetActive(true);
				facebookConnectBox.SetActive(false);
				receiveGiftsBox.SetActive(false);
				AutoMonoBehaviour<FacebookInterface>.Instance.TryCheckForRequests();
			}
			else
			{
				facebookConnectBox.SetActive(true);
				receiveGiftsBox.SetActive(false);
				helpMyFriendsBox.SetActive(false);
			}
		}
	}

	private void HandlePendingRequestsChanged(List<FacebookRequest> requests)
	{
		if (requests == null)
		{
			return;
		}
		gifts.Clear();
		foreach (FacebookRequest request in requests)
		{
			bool flag = false;
			foreach (FacebookRequest gift in gifts)
			{
				if (request.User.Id == gift.User.Id)
				{
					AutoMonoBehaviour<FacebookInterface>.Instance.RemoveRequest(request.Id.ToString());
					flag = true;
				}
			}
			if (!flag)
			{
				gifts.Add(request);
			}
		}
		UpdateGiftView();
	}

	private void UpdateGiftView()
	{
		if (!GameData.CanShowFacebookView)
		{
			return;
		}
		if (gifts.Count == 0)
		{
			ArePendingGifts(false);
			return;
		}
		currentGift = null;
		List<FacebookRequest> list = gifts.FindAll((FacebookRequest req) => req != null && req.Data.Equals("gift_item_4.4.x"));
		if (list.Count > 0)
		{
			currentGift = list[0];
			ArePendingGifts(true);
			IsGiftInteractive(true);
			badgeNumber.text = list.Count.ToString();
			SetGiftValues(currentGift);
		}
		else
		{
			ArePendingGifts(false);
		}
	}

	private void SetGiftValues(FacebookRequest gift)
	{
		if (gift.User.Avatar.Equals(string.Empty))
		{
			avatar.ShowDefault();
		}
		else
		{
			avatar.Url = gift.User.Avatar;
		}
		messageLabel.text = gift.User.FirstName + "\nsent you a gift";
	}

	private void HandleAcceptGift()
	{
		StartCoroutine(HandleAcceptGiftCtr());
	}

	private IEnumerator HandleAcceptGiftCtr()
	{
		yield return StartCoroutine(AcceptGiftCrt());
		AutoMonoBehaviour<FacebookInterface>.Instance.TryCheckForRequests();
	}

	private IEnumerator AcceptGiftCrt()
	{
		IsGiftInteractive(false);
		statusLabel.text = "Unwrapping gift...";
		statusLabel.GetComponent<UITweener>().enabled = true;
		ClaimFacebookGiftView facebookGiftView = null;
		yield return FacebookWebServiceClient.ClaimFacebookGift(PlayerDataManager.AuthToken, currentGift.Id, delegate(ClaimFacebookGiftView ev)
		{
			facebookGiftView = ev;
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex, "There was an error Claiming Facebook gift from server. Please refresh the page and try again.");
		});
		string message = "Sorry, something went wrong unwrapping your gift";
		if (facebookGiftView != null)
		{
			string itemName = null;
			IUnityItem item = null;
			if (facebookGiftView.ItemId.HasValue)
			{
				item = Singleton<ItemManager>.Instance.GetItemInShop(facebookGiftView.ItemId.Value);
				if (item != null)
				{
					itemName = item.Name;
				}
			}
			Debug.Log("Claim Result = " + facebookGiftView.ClaimResult);
			if (!string.IsNullOrEmpty(itemName))
			{
				switch (facebookGiftView.ClaimResult)
				{
				case ClaimFacebookGiftResult.RentalTimeProlonged:
				case ClaimFacebookGiftResult.NewItemAttributed:
					message = "You received: " + itemName + " (24 hours)";
					StartCoroutine(UpdateGiftInShop());
					break;
				case ClaimFacebookGiftResult.AlreadyOwnedPermanently:
					message = "Oops, you already owned " + itemName;
					break;
				default:
					message = "Sorry, something went wrong unwrappiing " + itemName;
					break;
				}
			}
			ApplicationDataManager.EventsSystem.SendGiftFacebookReceived(PlayerDataManager.CmidSecure, currentGift.User.Id.ToString(), (item != null) ? item.View.ID.ToString() : string.Empty, facebookGiftView.ClaimResult.ToString());
		}
		statusLabel.GetComponent<UITweener>().enabled = false;
		statusLabel.text = message;
		yield return new WaitForSeconds(1.5f);
	}

	private IEnumerator UpdateGiftInShop()
	{
		List<ItemInventoryView> inventoryView = new List<ItemInventoryView>();
		yield return UserWebServiceClient.GetInventory(PlayerDataManager.AuthToken, delegate(List<ItemInventoryView> view)
		{
			inventoryView = view;
		}, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			DebugConsoleManager.SendExceptionReport(ex);
		});
		Singleton<InventoryManager>.Instance.UpdateInventoryItems(inventoryView);
	}

	private void HandleDeclineGift()
	{
		StartCoroutine(DeclineGiftCrt());
	}

	private IEnumerator DeclineGiftCrt()
	{
		AppRequest request = AutoMonoBehaviour<FacebookInterface>.Instance.RemoveRequest(currentGift.Id);
		IsGiftInteractive(false);
		statusLabel.text = "Throwing away your gift...";
		statusLabel.GetComponent<UITweener>().enabled = true;
		while (!request.Ready)
		{
			yield return 0;
		}
		AutoMonoBehaviour<FacebookInterface>.Instance.TryCheckForRequests();
	}

	private void HandleAcceptAndThank()
	{
		StartCoroutine(AcceptAndThankCrt());
	}

	private IEnumerator AcceptAndThankCrt()
	{
		string to = currentGift.User.Id.ToString();
		yield return StartCoroutine(AcceptGiftCrt());
		IsGiftInteractive(false);
		AppRequest giftRequest = AutoMonoBehaviour<FacebookInterface>.Instance.SendAppRequest("I've just sent you a mystery gift in UberStrike. Jump in game and claim it to see what it is!", new string[1] { to }, string.Empty, "gift_item_4.4.x", "Thank your friend back", "gift_facebook_sent");
		string baseMessage = statusLabel.text;
		statusLabel.text = baseMessage + "\nThanking back...";
		float timeLimit = 0f;
		while (!giftRequest.Ready && timeLimit < 15f)
		{
			timeLimit += Time.deltaTime;
			yield return 0;
		}
		Screen.showCursor = true;
		if (giftRequest.Response != null)
		{
			statusLabel.text = baseMessage + "\nGift successfully sent!";
		}
		else
		{
			statusLabel.text = "Oops, no gift sent";
		}
		yield return new WaitForSeconds(1.5f);
		AutoMonoBehaviour<FacebookInterface>.Instance.TryCheckForRequests();
	}

	private void ArePendingGifts(bool pending)
	{
		receiveGiftsBox.SetActive(pending);
		helpMyFriendsBox.SetActive(!pending);
	}

	private void ConnectToFacebook()
	{
		AutoMonoBehaviour<FacebookInterface>.Instance.Connect();
	}

	private void OpenSendGiftsDialog()
	{
		FacebookInterface.AppRequestSendGift("I've just sent you a mystery gift in UberStrike. Jump in game and claim it to see what it is!", "gift_item_4.4.x", "Send a gift to your friends");
	}

	private void OpenInviteFriendsDialog()
	{
		AutoMonoBehaviour<FacebookInterface>.Instance.SendAppRequest("Join the game and play with me", null, "[\"app_non_users\"]", string.Empty, "Invite your friends to play Uberstrike", "invite_facebook_friend");
	}

	private void IsGiftInteractive(bool bActive)
	{
		acceptAndThanksButton.gameObject.SetActive(bActive);
		acceptButton.gameObject.SetActive(bActive);
		declineButton.gameObject.SetActive(bActive);
		orLabel.gameObject.SetActive(bActive);
		statusLabel.gameObject.SetActive(!bActive);
	}
}
