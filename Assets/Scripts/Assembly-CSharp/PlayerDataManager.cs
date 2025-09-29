using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Types;
using UberStrike.Core.ViewModel;
using UberStrike.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UberStrike.WebService.Unity;
using UnityEngine;

public class PlayerDataManager : Singleton<PlayerDataManager>
{
	private PlayerStatisticsView _serverLocalPlayerPlayerStatisticsView;

	private Dictionary<int, PublicProfileView> _friends = new Dictionary<int, PublicProfileView>();

	private Dictionary<int, PublicProfileView> _facebookFriends = new Dictionary<int, PublicProfileView>();

	private Dictionary<int, ClanMemberView> _clanMembers = new Dictionary<int, ClanMemberView>();

	private Color _localPlayerSkinColor = Color.white;

	private SecureMemory<int> _cmid;

	private SecureMemory<string> _name;

	private SecureMemory<string> _email;

	private SecureMemory<int> _accessLevel;

	private SecureMemory<int> _clanID;

	private SecureMemory<int> _experience;

	private SecureMemory<int> _level;

	private SecureMemory<int> _points;

	private SecureMemory<int> _credits;

	private SecureMemory<string> _authToken;

	private ClanView _playerClanData;

	private GroupPosition _playerClanPosition = GroupPosition.Member;

	private float _updateLoadoutTime;

	public float GearWeight { get; private set; }

	public int FriendsCount
	{
		get
		{
			return _friends.Count + _facebookFriends.Count;
		}
	}

	public PlayerStatisticsView ServerLocalPlayerStatisticsView
	{
		get
		{
			return _serverLocalPlayerPlayerStatisticsView;
		}
	}

	public static Color SkinColor
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._localPlayerSkinColor;
		}
	}

	public IEnumerable<PublicProfileView> FriendList
	{
		get
		{
			return _friends.Values;
		}
		set
		{
			_friends.Clear();
			if (value == null)
			{
				return;
			}
			foreach (PublicProfileView item in value)
			{
				_friends.Add(item.Cmid, item);
			}
		}
	}

	public IEnumerable<PublicProfileView> FacebookFriends
	{
		get
		{
			return _facebookFriends.Values;
		}
		set
		{
			_facebookFriends.Clear();
			if (value == null)
			{
				return;
			}
			foreach (PublicProfileView item in value)
			{
				if (!_friends.ContainsKey(item.Cmid))
				{
					_facebookFriends.Add(item.Cmid, item);
				}
			}
		}
	}

	public List<PublicProfileView> MergedFriends
	{
		get
		{
			List<PublicProfileView> list = new List<PublicProfileView>(FriendList);
			list.AddRange(FacebookFriends);
			return list;
		}
	}

	public static bool IsPlayerLoggedIn
	{
		get
		{
			return Cmid > 0;
		}
	}

	public static MemberAccessLevel AccessLevel
	{
		get
		{
			return (MemberAccessLevel)Singleton<PlayerDataManager>.Instance._accessLevel.ReadData(false);
		}
	}

	public static int Cmid
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._cmid.ReadData(false);
		}
	}

	public static string Name
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._name.ReadData(false);
		}
	}

	public static string Email
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._email.ReadData(false);
		}
	}

	public static int Credits
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._credits.ReadData(false);
		}
	}

	public static int Points
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._points.ReadData(false);
		}
	}

	public static int PlayerExperience
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._experience.ReadData(false);
		}
	}

	public static int PlayerLevel
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._level.ReadData(false);
		}
	}

	public static string AuthToken
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._authToken.ReadData(false);
		}
		set
		{
			Singleton<PlayerDataManager>.Instance._authToken.WriteData(value);
		}
	}

	public static ClanView ClanData
	{
		set
		{
			Singleton<PlayerDataManager>.Instance._playerClanData = value;
			Singleton<PlayerDataManager>.Instance._clanMembers.Clear();
			if (value != null)
			{
				Singleton<PlayerDataManager>.Instance._clanID.WriteData(value.GroupId);
				int cmidSecure = CmidSecure;
				if (value.Members == null)
				{
					return;
				}
				{
					foreach (ClanMemberView member in value.Members)
					{
						Singleton<PlayerDataManager>.Instance._clanMembers[member.Cmid] = member;
						if (member.Cmid == cmidSecure)
						{
							Singleton<PlayerDataManager>.Instance._playerClanPosition = member.Position;
						}
					}
					return;
				}
			}
			Singleton<PlayerDataManager>.Instance._clanID.WriteData(0);
			Singleton<PlayerDataManager>.Instance._clanMembers.Clear();
			Singleton<PlayerDataManager>.Instance._playerClanPosition = GroupPosition.Member;
		}
	}

	public static bool IsPlayerInClan
	{
		get
		{
			return ClanID > 0;
		}
	}

	public static int ClanID
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._clanID.ReadData(false);
		}
		set
		{
			Singleton<PlayerDataManager>.Instance._clanID.WriteData(value);
		}
	}

	public GroupPosition RankInClan
	{
		get
		{
			return _playerClanPosition;
		}
		set
		{
			_playerClanPosition = value;
		}
	}

	public static string ClanName
	{
		get
		{
			return (Singleton<PlayerDataManager>.Instance._playerClanData == null) ? string.Empty : Singleton<PlayerDataManager>.Instance._playerClanData.Name;
		}
	}

	public static string ClanTag
	{
		get
		{
			return (Singleton<PlayerDataManager>.Instance._playerClanData == null) ? string.Empty : Singleton<PlayerDataManager>.Instance._playerClanData.Tag;
		}
	}

	public static string ClanMotto
	{
		get
		{
			return (Singleton<PlayerDataManager>.Instance._playerClanData == null) ? string.Empty : Singleton<PlayerDataManager>.Instance._playerClanData.Motto;
		}
	}

	public static DateTime ClanFoundingDate
	{
		get
		{
			return (Singleton<PlayerDataManager>.Instance._playerClanData == null) ? DateTime.Now : Singleton<PlayerDataManager>.Instance._playerClanData.FoundingDate;
		}
	}

	public static string ClanOwnerName
	{
		get
		{
			return (Singleton<PlayerDataManager>.Instance._playerClanData == null) ? string.Empty : Singleton<PlayerDataManager>.Instance._playerClanData.OwnerName;
		}
	}

	public static int ClanMembersLimit
	{
		get
		{
			return (Singleton<PlayerDataManager>.Instance._playerClanData != null) ? Singleton<PlayerDataManager>.Instance._playerClanData.MembersLimit : 0;
		}
	}

	public int ClanMembersCount
	{
		get
		{
			return (_playerClanData != null) ? _playerClanData.Members.Count : 0;
		}
	}

	public List<ClanMemberView> ClanMembers
	{
		get
		{
			return (_playerClanData == null) ? new List<ClanMemberView>(0) : _playerClanData.Members;
		}
	}

	public static bool CanInviteToClan
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._playerClanPosition == GroupPosition.Leader || Singleton<PlayerDataManager>.Instance._playerClanPosition == GroupPosition.Officer;
		}
	}

	public static MemberAccessLevel AccessLevelSecure
	{
		get
		{
			return (MemberAccessLevel)Singleton<PlayerDataManager>.Instance._accessLevel.ReadData(true);
		}
	}

	public static int CmidSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._cmid.ReadData(true);
		}
	}

	public static string NameSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._name.ReadData(true);
		}
		set
		{
			Singleton<PlayerDataManager>.Instance._name.WriteData(value);
			ClanMemberView view;
			if (TryGetClanMember(CmidSecure, out view))
			{
				view.Name = value;
			}
		}
	}

	public static string EmailSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._email.ReadData(true);
		}
		set
		{
			Singleton<PlayerDataManager>.Instance._email.WriteData(value);
		}
	}

	public static int CreditsSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._credits.ReadData(true);
		}
	}

	public static int PointsSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._points.ReadData(true);
		}
	}

	public static int PlayerExperienceSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._experience.ReadData(true);
		}
	}

	public static int PlayerLevelSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._level.ReadData(true);
		}
	}

	public static int ClanIDSecure
	{
		get
		{
			return Singleton<PlayerDataManager>.Instance._clanID.ReadData(true);
		}
	}

	public static string NameAndTag
	{
		get
		{
			return (!IsPlayerInClan) ? Name : string.Format("[{0}] {1}", ClanTag, Name);
		}
	}

	private PlayerDataManager()
	{
		bool useAOTCompatibleMode = ApplicationDataManager.Channel == ChannelType.IPad || ApplicationDataManager.Channel == ChannelType.IPhone;
		_cmid = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_name = new SecureMemory<string>(string.Empty, true, useAOTCompatibleMode);
		_email = new SecureMemory<string>(string.Empty, true, useAOTCompatibleMode);
		_accessLevel = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_points = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_credits = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_level = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_experience = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_clanID = new SecureMemory<int>(0, true, useAOTCompatibleMode);
		_authToken = new SecureMemory<string>(string.Empty, true, useAOTCompatibleMode);
		_serverLocalPlayerPlayerStatisticsView = new PlayerStatisticsView();
		_playerClanData = new ClanView();
	}

	public void SetLocalPlayerMemberView(MemberView memberView)
	{
		_cmid.WriteData(memberView.PublicProfile.Cmid);
		_accessLevel.WriteData((int)memberView.PublicProfile.AccessLevel);
		_name.WriteData(memberView.PublicProfile.Name);
		_points.WriteData(memberView.MemberWallet.Points);
		_credits.WriteData(memberView.MemberWallet.Credits);
	}

	public void SetPlayerStatisticsView(PlayerStatisticsView value)
	{
		if (value != null)
		{
			_serverLocalPlayerPlayerStatisticsView = value;
			int levelForXp = XpPointsUtil.GetLevelForXp(value.Xp);
			UpdateSecureLevelAndXp(levelForXp, value.Xp);
		}
	}

	public void UpdatePlayerStats(StatsCollection stats, StatsCollection best)
	{
		PlayerStatisticsView serverLocalPlayerStatisticsView = ServerLocalPlayerStatisticsView;
		int xp = serverLocalPlayerStatisticsView.Xp + Singleton<EndOfMatchStats>.Instance.GainedXp;
		int levelForXp = XpPointsUtil.GetLevelForXp(xp);
		SetPlayerStatisticsView(new PlayerStatisticsView(serverLocalPlayerStatisticsView.Cmid, serverLocalPlayerStatisticsView.Splats + stats.GetKills(), serverLocalPlayerStatisticsView.Splatted + stats.Deaths, serverLocalPlayerStatisticsView.Shots + stats.GetShots(), serverLocalPlayerStatisticsView.Hits + stats.GetHits(), serverLocalPlayerStatisticsView.Headshots + stats.Headshots, serverLocalPlayerStatisticsView.Nutshots + stats.Nutshots, xp, levelForXp, new PlayerPersonalRecordStatisticsView((serverLocalPlayerStatisticsView.PersonalRecord.MostHeadshots <= best.Headshots) ? best.Headshots : serverLocalPlayerStatisticsView.PersonalRecord.MostHeadshots, (serverLocalPlayerStatisticsView.PersonalRecord.MostNutshots <= best.Nutshots) ? best.Nutshots : serverLocalPlayerStatisticsView.PersonalRecord.MostNutshots, (serverLocalPlayerStatisticsView.PersonalRecord.MostConsecutiveSnipes <= best.ConsecutiveSnipes) ? best.ConsecutiveSnipes : serverLocalPlayerStatisticsView.PersonalRecord.MostConsecutiveSnipes, 0, (serverLocalPlayerStatisticsView.PersonalRecord.MostSplats <= best.GetKills()) ? best.GetKills() : serverLocalPlayerStatisticsView.PersonalRecord.MostSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostDamageDealt <= best.GetDamageDealt()) ? best.GetDamageDealt() : serverLocalPlayerStatisticsView.PersonalRecord.MostDamageDealt, (serverLocalPlayerStatisticsView.PersonalRecord.MostDamageReceived <= best.DamageReceived) ? best.DamageReceived : serverLocalPlayerStatisticsView.PersonalRecord.MostDamageReceived, (serverLocalPlayerStatisticsView.PersonalRecord.MostArmorPickedUp <= best.ArmorPickedUp) ? best.ArmorPickedUp : serverLocalPlayerStatisticsView.PersonalRecord.MostArmorPickedUp, (serverLocalPlayerStatisticsView.PersonalRecord.MostHealthPickedUp <= best.HealthPickedUp) ? best.HealthPickedUp : serverLocalPlayerStatisticsView.PersonalRecord.MostHealthPickedUp, (serverLocalPlayerStatisticsView.PersonalRecord.MostMeleeSplats <= best.MeleeKills) ? best.MeleeKills : serverLocalPlayerStatisticsView.PersonalRecord.MostMeleeSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostMachinegunSplats <= best.MachineGunKills) ? best.MachineGunKills : serverLocalPlayerStatisticsView.PersonalRecord.MostMachinegunSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostShotgunSplats <= best.ShotgunSplats) ? best.ShotgunSplats : serverLocalPlayerStatisticsView.PersonalRecord.MostShotgunSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostSniperSplats <= best.SniperKills) ? best.SniperKills : serverLocalPlayerStatisticsView.PersonalRecord.MostSniperSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostSplattergunSplats <= best.SplattergunKills) ? best.SplattergunKills : serverLocalPlayerStatisticsView.PersonalRecord.MostSplattergunSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostCannonSplats <= best.CannonKills) ? best.CannonKills : serverLocalPlayerStatisticsView.PersonalRecord.MostCannonSplats, (serverLocalPlayerStatisticsView.PersonalRecord.MostLauncherSplats <= best.LauncherKills) ? best.LauncherKills : serverLocalPlayerStatisticsView.PersonalRecord.MostLauncherSplats), new PlayerWeaponStatisticsView(serverLocalPlayerStatisticsView.WeaponStatistics.MeleeTotalSplats + stats.MeleeKills, serverLocalPlayerStatisticsView.WeaponStatistics.MachineGunTotalSplats + stats.MachineGunKills, serverLocalPlayerStatisticsView.WeaponStatistics.ShotgunTotalSplats + stats.ShotgunSplats, serverLocalPlayerStatisticsView.WeaponStatistics.SniperTotalSplats + stats.SniperKills, serverLocalPlayerStatisticsView.WeaponStatistics.SplattergunTotalSplats + stats.SplattergunKills, serverLocalPlayerStatisticsView.WeaponStatistics.CannonTotalSplats + stats.CannonKills, serverLocalPlayerStatisticsView.WeaponStatistics.LauncherTotalSplats + stats.LauncherKills, serverLocalPlayerStatisticsView.WeaponStatistics.MeleeTotalShotsFired + stats.MeleeShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.MeleeTotalShotsHit + stats.MeleeShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.MeleeTotalDamageDone + stats.MeleeDamageDone, serverLocalPlayerStatisticsView.WeaponStatistics.MachineGunTotalShotsFired + stats.MachineGunShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.MachineGunTotalShotsHit + stats.MachineGunShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.MachineGunTotalDamageDone + stats.MachineGunDamageDone, serverLocalPlayerStatisticsView.WeaponStatistics.ShotgunTotalShotsFired + stats.ShotgunShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.ShotgunTotalShotsHit + stats.ShotgunShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.ShotgunTotalDamageDone + stats.ShotgunDamageDone, serverLocalPlayerStatisticsView.WeaponStatistics.SniperTotalShotsFired + stats.SniperShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.SniperTotalShotsHit + stats.SniperShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.SniperTotalDamageDone + stats.SniperDamageDone, serverLocalPlayerStatisticsView.WeaponStatistics.SplattergunTotalShotsFired + stats.SplattergunShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.SplattergunTotalShotsHit + stats.SplattergunShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.SplattergunTotalDamageDone + stats.SplattergunDamageDone, serverLocalPlayerStatisticsView.WeaponStatistics.CannonTotalShotsFired + stats.CannonShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.CannonTotalShotsHit + stats.CannonShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.CannonTotalDamageDone + stats.CannonDamageDone, serverLocalPlayerStatisticsView.WeaponStatistics.LauncherTotalShotsFired + stats.LauncherShotsFired, serverLocalPlayerStatisticsView.WeaponStatistics.LauncherTotalShotsHit + stats.LauncherShotsHit, serverLocalPlayerStatisticsView.WeaponStatistics.LauncherTotalDamageDone + stats.LauncherDamageDone)));
	}

	public void AddFriend(PublicProfileView view)
	{
		_friends.Add(view.Cmid, view);
	}

	public void RemoveFriend(int friendCmid)
	{
		_friends.Remove(friendCmid);
	}

	public static void AddPointsSecure(int points)
	{
		Singleton<PlayerDataManager>.Instance._points.WriteData(PointsSecure + points);
	}

	private void HandleWebServiceError()
	{
	}

	public void SetSkinColor(Color skinColor)
	{
		_localPlayerSkinColor = skinColor;
	}

	private LoadoutView CreateLocalPlayerLoadoutView()
	{
		return new LoadoutView(0, 0, Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearBoots), CmidSecure, Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearFace), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.FunctionalItem1), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.FunctionalItem2), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.FunctionalItem3), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearGloves), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearHead), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearLowerBody), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.WeaponMelee), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.QuickUseItem1), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.QuickUseItem2), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.QuickUseItem3), AvatarType.LutzRavinoff, Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearUpperBody), Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.WeaponPrimary), 0, 0, 0, Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.WeaponSecondary), 0, 0, 0, Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.WeaponTertiary), 0, 0, 0, Singleton<LoadoutManager>.Instance.GetItemIdOnSlot(LoadoutSlotType.GearHolo), ColorConverter.ColorToHex(_localPlayerSkinColor));
	}

	public IEnumerator StartGetMemberWallet()
	{
		if (CmidSecure < 1)
		{
			Debug.LogError("Player CMID is invalid! Have you called AuthenticationManager.StartAuthenticateMember?");
			ApplicationDataManager.LockApplication("The authentication process failed. Please sign in on www.uberstrike.com and restart UberStrike.");
			yield break;
		}
		IPopupDialog popupDialog = PopupSystem.ShowMessage("Updating", "Updating your points and credits balance...", PopupSystem.AlertType.None);
		yield return UserWebServiceClient.GetMemberWallet(AuthToken, OnGetMemberWalletEventReturn, delegate(Exception ex)
		{
			DebugConsoleManager.SendExceptionReport(ex);
		});
		yield return new WaitForSeconds(0.5f);
		PopupSystem.HideMessage(popupDialog);
	}

	public IEnumerator StartSetLoadout()
	{
		if (_updateLoadoutTime == 0f)
		{
			_updateLoadoutTime = Time.time + 5f;
			while (_updateLoadoutTime > Time.time)
			{
				yield return new WaitForEndOfFrame();
			}
			_updateLoadoutTime = 0f;
			yield return UserWebServiceClient.SetLoadout(AuthToken, CreateLocalPlayerLoadoutView(), delegate(MemberOperationResult ev)
			{
				if (ev != MemberOperationResult.Ok)
				{
					Debug.LogError("SetLoadout failed with error=" + ev);
				}
			}, delegate(Exception ex)
			{
				DebugConsoleManager.SendExceptionReport(ex);
			});
		}
		else
		{
			_updateLoadoutTime = Time.time + 5f;
		}
	}

	public IEnumerator StartGetLoadout()
	{
		if (!Singleton<ItemManager>.Instance.ValidateItemMall())
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			PopupSystem.ShowMessage("Error Getting Shop Data", "The shop is empty, perhaps there\nwas an error getting the Shop data?", PopupSystem.AlertType.OK, HandleWebServiceError);
			yield break;
		}
		yield return UserWebServiceClient.GetLoadout(AuthToken, delegate(LoadoutView ev)
		{
			if (ev != null)
			{
				CheckLoadoutForExpiredItems(ev);
				Singleton<LoadoutManager>.Instance.RefreshLoadoutFromServerCache(ev);
				_localPlayerSkinColor = ColorConverter.HexToColor(ev.SkinColor);
			}
			else
			{
				ApplicationDataManager.EventsSystem.SendLoadingError();
				ApplicationDataManager.LockApplication("It seems that you account is corrupted. Please visit support.uberstrike.com for advice.");
			}
		}, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			DebugConsoleManager.SendExceptionReport(ex);
			ApplicationDataManager.LockApplication("There was an error getting your loadout.");
		});
	}

	public IEnumerator StartGetMember()
	{
		if (CmidSecure < 1)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			Debug.LogError("Player CMID is invalid!");
			ApplicationDataManager.LockApplication("The authentication process failed. Please sign in on www.uberstrike.com and restart UberStrike.");
			yield break;
		}
		yield return UserWebServiceClient.GetMember(AuthToken, OnGetMemberEventReturn, delegate(Exception ex)
		{
			ApplicationDataManager.EventsSystem.SendLoadingError();
			DebugConsoleManager.SendExceptionReport(ex);
			ApplicationDataManager.LockApplication("There was an error getting your player data.");
		});
	}

	private void OnGetMemberWalletEventReturn(MemberWalletView ev)
	{
		NotifyPointsAndCreditsChanges(ev.Points, ev.Credits);
		UpdateSecurePointsAndCredits(ev.Points, ev.Credits);
	}

	private void OnGetMemberEventReturn(UberstrikeUserViewModel ev)
	{
		NotifyPointsAndCreditsChanges(ev.CmuneMemberView.MemberWallet.Points, ev.CmuneMemberView.MemberWallet.Credits);
		SetPlayerStatisticsView(ev.UberstrikeMemberView.PlayerStatisticsView);
		SetLocalPlayerMemberView(ev.CmuneMemberView);
	}

	private void NotifyPointsAndCreditsChanges(int newPoints, int newCredits)
	{
		int num = _points.ReadData(true);
		int num2 = _credits.ReadData(true);
		if (newPoints != num)
		{
			GlobalUIRibbon.Instance.AddPointsEvent(newPoints - num);
		}
		if (newCredits != num2)
		{
			GlobalUIRibbon.Instance.AddCreditsEvent(newCredits - num2);
		}
	}

	public bool ValidateMemberData()
	{
		return CmidSecure > 0 && _serverLocalPlayerPlayerStatisticsView.Cmid > 0;
	}

	public void AttributeXp(int xp)
	{
		int xp2 = PlayerExperienceSecure + xp;
		int levelForXp = XpPointsUtil.GetLevelForXp(xp2);
		_serverLocalPlayerPlayerStatisticsView.Xp = xp2;
		_serverLocalPlayerPlayerStatisticsView.Level = levelForXp;
		UpdateSecureLevelAndXp(levelForXp, xp2);
	}

	private void UpdateSecureLevelAndXp(int level, int xp)
	{
		_experience.WriteData(xp);
		_level.WriteData(level);
	}

	public void UpdateSecurePointsAndCredits(int points, int credits)
	{
		_points.WriteData(points);
		_credits.WriteData(credits);
	}

	public void CheckLoadoutForExpiredItems(LoadoutView view)
	{
		view = new LoadoutView(view.LoadoutId, (!IsExpired(view.Backpack, "Backpack")) ? view.Backpack : 0, (!IsExpired(view.Boots, "Boots")) ? view.Boots : 0, view.Cmid, (!IsExpired(view.Face, "Face")) ? view.Face : 0, (!IsExpired(view.FunctionalItem1, "FunctionalItem1")) ? view.FunctionalItem1 : 0, (!IsExpired(view.FunctionalItem2, "FunctionalItem2")) ? view.FunctionalItem2 : 0, (!IsExpired(view.FunctionalItem3, "FunctionalItem3")) ? view.FunctionalItem3 : 0, (!IsExpired(view.Gloves, "Gloves")) ? view.Gloves : 0, (!IsExpired(view.Head, "Head")) ? view.Head : 0, (!IsExpired(view.LowerBody, "LowerBody")) ? view.LowerBody : 0, (!IsExpired(view.MeleeWeapon, "MeleeWeapon")) ? view.MeleeWeapon : 0, (!IsExpired(view.QuickItem1, "QuickItem1")) ? view.QuickItem1 : 0, (!IsExpired(view.QuickItem2, "QuickItem2")) ? view.QuickItem2 : 0, (!IsExpired(view.QuickItem3, "QuickItem3")) ? view.QuickItem3 : 0, view.Type, (!IsExpired(view.UpperBody, "UpperBody")) ? view.UpperBody : 0, (!IsExpired(view.Weapon1, "Weapon1")) ? view.Weapon1 : 0, (!IsExpired(view.Weapon1Mod1, "Weapon1Mod1")) ? view.Weapon1Mod1 : 0, (!IsExpired(view.Weapon1Mod2, "Weapon1Mod2")) ? view.Weapon1Mod2 : 0, (!IsExpired(view.Weapon1Mod3, "Weapon1Mod3")) ? view.Weapon1Mod3 : 0, (!IsExpired(view.Weapon2, "Weapon2")) ? view.Weapon2 : 0, (!IsExpired(view.Weapon2Mod1, "Weapon2Mod1")) ? view.Weapon2Mod1 : 0, (!IsExpired(view.Weapon2Mod2, "Weapon2Mod2")) ? view.Weapon2Mod2 : 0, (!IsExpired(view.Weapon2Mod3, "Weapon2Mod3")) ? view.Weapon2Mod3 : 0, (!IsExpired(view.Weapon3, "Weapon3")) ? view.Weapon3 : 0, (!IsExpired(view.Weapon3Mod1, "Weapon3Mod1")) ? view.Weapon3Mod1 : 0, (!IsExpired(view.Weapon3Mod2, "Weapon3Mod2")) ? view.Weapon3Mod2 : 0, (!IsExpired(view.Weapon3Mod3, "Weapon3Mod3")) ? view.Weapon3Mod3 : 0, (!IsExpired(view.Webbing, "Webbing")) ? view.Webbing : 0, view.SkinColor);
	}

	private bool IsExpired(int itemId, string debug)
	{
		return !Singleton<InventoryManager>.Instance.Contains(itemId);
	}

	public static bool IsClanMember(int cmid)
	{
		return Singleton<PlayerDataManager>.Instance._clanMembers.ContainsKey(cmid);
	}

	public static bool IsFriend(int cmid)
	{
		return Singleton<PlayerDataManager>.Instance._friends.ContainsKey(cmid);
	}

	public static bool IsFacebookFriend(int cmid)
	{
		return Singleton<PlayerDataManager>.Instance._facebookFriends.ContainsKey(cmid);
	}

	public static bool TryGetFriend(int cmid, out PublicProfileView view)
	{
		return Singleton<PlayerDataManager>.Instance._friends.TryGetValue(cmid, out view) && view != null;
	}

	public static bool TryGetClanMember(int cmid, out ClanMemberView view)
	{
		return Singleton<PlayerDataManager>.Instance._clanMembers.TryGetValue(cmid, out view) && view != null;
	}
}
