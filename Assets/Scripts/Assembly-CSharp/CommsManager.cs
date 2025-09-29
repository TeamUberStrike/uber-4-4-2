using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;

public class CommsManager : Singleton<CommsManager>
{
	public float NextFriendsRefresh { get; private set; }

	private CommsManager()
	{
	}

	public void SendFriendRequest(int cmid, string message)
	{
		message = TextUtilities.ShortenText(TextUtilities.Trim(message), 140, false);
		RelationshipWebServiceClient.SendContactRequest(PlayerDataManager.AuthToken, cmid, message, delegate
		{
			CommActorInfo info;
			if (CommConnectionManager.CommCenter.TryGetActorWithCmid(cmid, out info))
			{
				CommConnectionManager.CommCenter.UpdateInboxRequest(info.ActorId);
			}
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex);
		});
	}

	public IEnumerator GetContactsByGroups()
	{
		NextFriendsRefresh = Time.time + 30f;
		yield return RelationshipWebServiceClient.GetContactsByGroups(PlayerDataManager.AuthToken, AutoMonoBehaviour<FacebookInterface>.Instance.IsLoggedIn, delegate(List<ContactGroupView> ev)
		{
			List<PublicProfileView> list = new List<PublicProfileView>();
			foreach (ContactGroupView item in ev)
			{
				foreach (PublicProfileView contact in item.Contacts)
				{
					list.Add(contact);
				}
			}
			Singleton<PlayerDataManager>.Instance.FriendList = list;
			UpdateCommunicator();
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex);
		});
	}

	public IEnumerator MergeFacebookFriends()
	{
		yield return FacebookWebServiceClient.GetFacebookFriendsList(FacebookInterface.GameFriends.ConvertAll((FacebookUser el) => el.Id.ToString()), delegate(List<PublicProfileView> ev)
		{
			Singleton<PlayerDataManager>.Instance.FacebookFriends = ev;
			UpdateCommunicator();
		}, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex, "There was an error Claiming Facebook gift from server. Please refresh the page and try again.");
		});
	}

	public void UpdateCommunicator()
	{
		CommConnectionManager.CommCenter.SendContactList();
		Singleton<ChatManager>.Instance.UpdateFriendSection();
	}
}
