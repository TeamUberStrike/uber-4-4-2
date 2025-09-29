using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class HudController : MonoBehaviour
{
	private XpPtsHud _xpPtsHud;

	private HudDrawFlags _finalDrawFlag;

	public static HudController Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public XpPtsHud XpPtsHud
	{
		get
		{
			if (_xpPtsHud == null)
			{
				_xpPtsHud = new XpPtsHud();
				CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(XpPtsHud.OnTeamChange);
				CmuneEventHandler.AddListener<ScreenResolutionEvent>(XpPtsHud.OnScreenResolutionChange);
			}
			return _xpPtsHud;
		}
	}

	public HudDrawFlags DrawFlags
	{
		get
		{
			return _finalDrawFlag;
		}
		set
		{
			_finalDrawFlag = value;
			UpdateHudVisibilityByFlag();
		}
	}

	public string DrawFlagString
	{
		get
		{
			return CmunePrint.Flag<HudDrawFlags>((ushort)DrawFlags);
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		UpdateHudVisibilityByFlag();
	}

	private void OnDisable()
	{
		if (!GameState.IsShuttingDown)
		{
			UpdateHudVisibilityByFlag();
		}
	}

	private void OnGUI()
	{
		GUI.depth = 100;
		if (IsDrawFlagEnabled(HudDrawFlags.InGameChat))
		{
			Singleton<InGameChatHud>.Instance.Draw();
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			XpPtsHud.PopupTemporarily();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.HealthArmor))
		{
			Singleton<HpApHud>.Instance.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.Ammo))
		{
			Singleton<AmmoHud>.Instance.Draw();
			Singleton<TemporaryWeaponHud>.Instance.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.XpPoints))
		{
			XpPtsHud.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.Weapons))
		{
			Singleton<WeaponsHud>.Instance.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.EventStream))
		{
			Singleton<EventStreamHud>.Instance.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.Score | HudDrawFlags.RoundTime | HudDrawFlags.RemainingKill))
		{
			Singleton<MatchStatusHud>.Instance.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.Reticle))
		{
			Singleton<ReticleHud>.Instance.Draw();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.InGameHelp))
		{
			Singleton<InGameHelpHud>.Instance.Draw();
		}
		Singleton<EventFeedbackHud>.Instance.Draw();
		Singleton<LevelUpHud>.Instance.Draw();
		Singleton<PopupHud>.Instance.Draw();
		Singleton<GameModeObjectiveHud>.Instance.Draw();
		Singleton<TeamChangeWarningHud>.Instance.Draw();
		Singleton<DamageFeedbackHud>.Instance.Draw();
		Singleton<PlayerStateMsgHud>.Instance.Draw();
		Singleton<ScreenshotHud>.Instance.Draw();
		Singleton<FrameRateHud>.Instance.Draw();
	}

	private void Update()
	{
		Singleton<HpApHud>.Instance.Update();
		XpPtsHud.Update();
		Singleton<PickupNameHud>.Instance.Update();
		Singleton<ReticleHud>.Instance.Update();
		Singleton<LocalShotFeedbackHud>.Instance.Update();
		Singleton<DamageFeedbackHud>.Instance.Update();
		Singleton<InGameFeatHud>.Instance.Update();
		Singleton<WeaponsHud>.Instance.Update();
		Singleton<EventStreamHud>.Instance.Update();
		Singleton<GameModeObjectiveHud>.Instance.Update();
		if (IsDrawFlagEnabled(HudDrawFlags.InGameHelp))
		{
			Singleton<InGameHelpHud>.Instance.Update();
		}
		if (IsDrawFlagEnabled(HudDrawFlags.InGameChat))
		{
			Singleton<InGameChatHud>.Instance.Update();
		}
	}

	private void UpdateHudVisibilityByFlag()
	{
		Singleton<HpApHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.HealthArmor);
		Singleton<AmmoHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.Ammo);
		XpPtsHud.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.XpPoints);
		Singleton<WeaponsHud>.Instance.SetEnabled(base.enabled && IsDrawFlagEnabled(HudDrawFlags.Weapons));
		Singleton<EventStreamHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.EventStream);
		Singleton<MatchStatusHud>.Instance.IsScoreEnabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.Score);
		Singleton<MatchStatusHud>.Instance.IsClockEnabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.RoundTime);
		Singleton<MatchStatusHud>.Instance.IsRemainingKillEnabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.RemainingKill);
		Singleton<InGameHelpHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.InGameHelp);
		Singleton<ReticleHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.Reticle);
		Singleton<InGameChatHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.InGameChat);
		Singleton<EventFeedbackHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.StateMsg);
		Singleton<GameModeObjectiveHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.StateMsg);
		Singleton<PickupNameHud>.Instance.Enabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.StateMsg);
		Singleton<PlayerStateMsgHud>.Instance.TemporaryMsgEnabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.StateMsg);
		Singleton<PlayerStateMsgHud>.Instance.PermanentMsgEnabled = base.enabled && IsDrawFlagEnabled(HudDrawFlags.StateMsg);
	}

	private bool IsDrawFlagEnabled(HudDrawFlags drawFlag)
	{
		return (DrawFlags & drawFlag) != 0;
	}
}
