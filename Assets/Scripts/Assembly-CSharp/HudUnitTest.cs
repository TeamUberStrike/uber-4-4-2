using System.Collections.Generic;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class HudUnitTest : MonoBehaviour
{
	public GUISkin skin;

	public TeamID teamID;

	public bool enableMultiKill;

	public bool enableHpAp;

	public bool enableAmmo;

	public bool enableXpPtsBar;

	public bool enableEventStream;

	public bool enableTeamScoreAndClock;

	public bool enableHelpInfo;

	public bool enablePermanentStateMsg;

	public bool enableTemporaryWeapon;

	private TeamID lastTeamId;

	private MeshGUIList weaponHud;

	private QuickItemHud armorQuickItem = new QuickItemHud("ArmorQuickItem");

	private QuickItemHud healthQuickItem = new QuickItemHud("HealthQuickItem");

	private QuickItemHud springQuickItem = new QuickItemHud("SpringQuickItem");

	private Dictionary<LoadoutSlotType, QuickItemHud> _quickItemsHud;

	private Animatable2DGroup _quickItemsGroup;

	private void Start()
	{
		Singleton<HudStyleUtility>.Instance.OnTeamChange(new OnSetPlayerTeamEvent());
		LocalizedStrings.KillsRemain = "Kills Remaining";
		LocalizedStrings.WaitingForOtherPlayers = "Waiting For Other Players";
		LocalizedStrings.SpectatorMode = "Spectator Mode";
		lastTeamId = TeamID.NONE;
		teamID = TeamID.BLUE;
		Singleton<MatchStatusHud>.Instance.RemainingSeconds = 31;
		Singleton<MatchStatusHud>.Instance.RemainingKills = 6;
		Singleton<InGameHelpHud>.Instance.EnableChangeTeamHelp = true;
		Singleton<TemporaryWeaponHud>.Instance.StartCounting(30);
		weaponHud = new MeshGUIList();
		weaponHud.Enabled = true;
		weaponHud.AddItem("Melee");
		weaponHud.AddItem("Machine Gun");
		weaponHud.AddItem("Sniper");
		_quickItemsHud = new Dictionary<LoadoutSlotType, QuickItemHud>();
		armorQuickItem = new QuickItemHud("ArmorQuickItem");
		healthQuickItem = new QuickItemHud("HealthQuickItem");
		springQuickItem = new QuickItemHud("SpringQuickItem");
		_quickItemsGroup = new Animatable2DGroup();
		_quickItemsHud.Add(LoadoutSlotType.QuickUseItem1, springQuickItem);
		_quickItemsHud.Add(LoadoutSlotType.QuickUseItem2, healthQuickItem);
		_quickItemsHud.Add(LoadoutSlotType.QuickUseItem3, armorQuickItem);
		foreach (QuickItemHud value in _quickItemsHud.Values)
		{
			_quickItemsGroup.Group.Add(value.Group);
		}
		springQuickItem.Amount = 3;
		ResetQuickItemsTransform();
	}

	private void Update()
	{
		Singleton<HpApHud>.Instance.Enabled = enableHpAp;
		Singleton<AmmoHud>.Instance.Enabled = enableAmmo;
		HudController.Instance.XpPtsHud.Enabled = enableXpPtsBar;
		Singleton<EventStreamHud>.Instance.Enabled = enableEventStream;
		Singleton<MatchStatusHud>.Instance.Enabled = enableTeamScoreAndClock;
		Singleton<InGameHelpHud>.Instance.Enabled = enableHelpInfo;
		Singleton<PlayerStateMsgHud>.Instance.PermanentMsgEnabled = enablePermanentStateMsg;
		Singleton<TemporaryWeaponHud>.Instance.Enabled = enableTemporaryWeapon;
		Singleton<HpApHud>.Instance.Update();
		HudController.Instance.XpPtsHud.Update();
		Singleton<LocalShotFeedbackHud>.Instance.Update();
		Singleton<PickupNameHud>.Instance.Update();
		Singleton<InGameFeatHud>.Instance.Update();
		Singleton<EventStreamHud>.Instance.Update();
		Singleton<GameModeObjectiveHud>.Instance.Update();
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			weaponHud.AnimDownward();
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			weaponHud.AnimUpward();
		}
		if (teamID != lastTeamId)
		{
			Singleton<HudUtil>.Instance.SetPlayerTeam(teamID);
			lastTeamId = teamID;
		}
	}

	private void OnGUI()
	{
		if (enableMultiKill)
		{
			if (GUI.Button(new Rect(0f, 0f, 100f, 30f), "Popup Double"))
			{
				Singleton<PopupHud>.Instance.PopupMultiKill(2);
			}
			if (GUI.Button(new Rect(0f, 40f, 100f, 30f), "Popup Triple"))
			{
				Singleton<PopupHud>.Instance.PopupMultiKill(3);
			}
			if (GUI.Button(new Rect(0f, 80f, 100f, 30f), "Popup Quad"))
			{
				Singleton<PopupHud>.Instance.PopupMultiKill(4);
			}
			if (GUI.Button(new Rect(0f, 120f, 100f, 30f), "Popup Mega"))
			{
				Singleton<PopupHud>.Instance.PopupMultiKill(5);
			}
			if (GUI.Button(new Rect(0f, 160f, 100f, 30f), "Popup Uber"))
			{
				Singleton<PopupHud>.Instance.PopupMultiKill(6);
			}
			if (GUI.Button(new Rect(0f, 200f, 100f, 30f), "Round Start"))
			{
				Singleton<PopupHud>.Instance.PopupRoundStart();
			}
			Singleton<PopupHud>.Instance.Draw();
		}
		if (enableHpAp)
		{
			if (GUI.Button(new Rect((float)Screen.width - 100f, 200f, 100f, 30f), "Increase HP"))
			{
				Singleton<HpApHud>.Instance.HP = Singleton<HpApHud>.Instance.HP + 1;
			}
			if (GUI.Button(new Rect((float)Screen.width - 100f, 240f, 100f, 30f), "Decrease HP"))
			{
				Singleton<HpApHud>.Instance.HP = Singleton<HpApHud>.Instance.HP - 1;
			}
			if (GUI.Button(new Rect((float)Screen.width - 150f, 280f, 150f, 30f), "Decrease Armor"))
			{
				Singleton<HpApHud>.Instance.AP = Singleton<HpApHud>.Instance.AP - 1;
			}
			Singleton<HpApHud>.Instance.Draw();
		}
		if (enableXpPtsBar)
		{
			if (GUI.Button(new Rect((float)Screen.width - 100f, 320f, 100f, 30f), "Generate XP"))
			{
				HudController.Instance.XpPtsHud.GainXp(3);
			}
			if (GUI.Button(new Rect((float)Screen.width - 100f, 360f, 100f, 30f), "Generate Pts"))
			{
				HudController.Instance.XpPtsHud.GainPoints(3);
			}
			if (GUI.Button(new Rect((float)Screen.width - 100f, 120f, 100f, 30f), "Popup Xp bar"))
			{
				HudController.Instance.XpPtsHud.PopupTemporarily();
			}
			HudController.Instance.XpPtsHud.Draw();
		}
		if (enableAmmo)
		{
			if (GUI.Button(new Rect((float)Screen.width - 150f, 400f, 150f, 30f), "Increase Ammo"))
			{
				Singleton<AmmoHud>.Instance.Ammo++;
			}
			Singleton<AmmoHud>.Instance.Draw();
		}
		if (enableEventStream)
		{
			if (GUI.Button(new Rect((float)Screen.width - 100f, 440f, 100f, 30f), "Add Event"))
			{
				Singleton<EventStreamHud>.Instance.AddEventText("UberPlayerUberPlayerUberPlayer", TeamID.BLUE, "smackdown", "Bulletsponge", TeamID.RED);
			}
			Singleton<EventStreamHud>.Instance.Draw();
		}
		if (GUI.Button(new Rect((float)Screen.width - 200f, 480f, 200f, 30f), "Set Deathmatch Mode"))
		{
			Singleton<GameModeObjectiveHud>.Instance.DisplayGameMode(GameMode.DeathMatch);
		}
		Singleton<GameModeObjectiveHud>.Instance.Draw();
		if (enableTeamScoreAndClock)
		{
			if (GUI.Button(new Rect(0f, 520f, 150f, 30f), "Decrement Clock"))
			{
				Singleton<MatchStatusHud>.Instance.RemainingSeconds--;
			}
			if (GUI.Button(new Rect(0f, 560f, 200f, 30f), "Decrement Remaining Kill"))
			{
				Singleton<MatchStatusHud>.Instance.RemainingRoundsText = "Final Round RED";
			}
			Singleton<MatchStatusHud>.Instance.Draw();
		}
		if (enableHelpInfo)
		{
			Singleton<InGameHelpHud>.Instance.Draw();
		}
		if (GUI.Button(new Rect(0f, 480f, 150f, 30f), "Add Event Feedback"))
		{
			Singleton<EventFeedbackHud>.Instance.EnqueueFeedback(InGameEventFeedbackType.CustomMessage, "Headshot from Benny");
		}
		Singleton<EventFeedbackHud>.Instance.Draw();
		Singleton<PickupNameHud>.Instance.Draw();
		if (GUI.Button(new Rect(0f, 220f, 150f, 30f), "Anim Pickup Item"))
		{
			Singleton<PickupNameHud>.Instance.DisplayPickupName("Ammo Small", PickUpMessageType.Armor5);
		}
		if (GUI.Button(new Rect(0f, 360f, 150f, 30f), "Nut Shot"))
		{
			Singleton<LocalShotFeedbackHud>.Instance.DisplayLocalShotFeedback(InGameEventFeedbackType.NutShot);
		}
		if (GUI.Button(new Rect(0f, 400f, 150f, 30f), "Head Shot"))
		{
			Singleton<LocalShotFeedbackHud>.Instance.DisplayLocalShotFeedback(InGameEventFeedbackType.HeadShot);
		}
		if (GUI.Button(new Rect(0f, 440f, 150f, 30f), "Smackdown"))
		{
			Singleton<LocalShotFeedbackHud>.Instance.DisplayLocalShotFeedback(InGameEventFeedbackType.Humiliation);
		}
		if (GUI.Button(new Rect(0f, 260f, 150f, 30f), "Decrement Spring"))
		{
			springQuickItem.Amount--;
		}
		weaponHud.Draw();
		_quickItemsGroup.Draw();
		Singleton<PlayerStateMsgHud>.Instance.Draw();
		if (enablePermanentStateMsg)
		{
			if (GUI.Button(new Rect((float)Screen.width - 200f, 0f, 200f, 30f), "WaitingForPlayer"))
			{
				Singleton<PlayerStateMsgHud>.Instance.DisplayWaitingForOtherPlayerMsg();
			}
			if (GUI.Button(new Rect((float)Screen.width - 100f, 40f, 100f, 30f), "SpectatorMsg"))
			{
				Singleton<PlayerStateMsgHud>.Instance.DisplaySpectatorModeMsg();
			}
		}
		if (GUI.Button(new Rect(300f, 0f, 150f, 30f), "Set temporary remaining"))
		{
			Singleton<TemporaryWeaponHud>.Instance.RemainingSeconds--;
		}
		Singleton<TemporaryWeaponHud>.Instance.Draw();
	}

	private void ResetQuickItemsTransform()
	{
		foreach (QuickItemHud value in _quickItemsHud.Values)
		{
			value.ResetHud();
		}
		Animatable2DGroup animatable2DGroup = _quickItemsHud[LoadoutSlotType.QuickUseItem1].Group;
		Animatable2DGroup animatable2DGroup2 = _quickItemsHud[LoadoutSlotType.QuickUseItem2].Group;
		Animatable2DGroup animatable2DGroup3 = _quickItemsHud[LoadoutSlotType.QuickUseItem3].Group;
		animatable2DGroup3.Position = Vector2.zero;
		animatable2DGroup2.Position = animatable2DGroup3.Position - new Vector2(0f, animatable2DGroup3.Rect.height * 0.75f);
		animatable2DGroup.Position = animatable2DGroup2.Position - new Vector2(0f, animatable2DGroup2.Rect.height * 0.75f);
		_quickItemsGroup.Position = new Vector2((float)Screen.width - 50f, (float)Screen.height - _quickItemsGroup.Rect.height / 2f);
	}
}
