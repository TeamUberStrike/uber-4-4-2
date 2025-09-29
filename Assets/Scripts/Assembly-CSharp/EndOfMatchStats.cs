using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class EndOfMatchStats : Singleton<EndOfMatchStats>
{
	private EndOfMatchData _data;

	public string PlayTimeXp { get; private set; }

	public string PlayTimePts { get; private set; }

	public string SkillBonusXp { get; private set; }

	public string SkillBonusPts { get; private set; }

	public string BoostXp { get; private set; }

	public string BoostPts { get; private set; }

	public string TotalXp { get; private set; }

	public string TotalPts { get; private set; }

	public string PlayTime { get; private set; }

	public string Kills { get; private set; }

	public string Nutshots { get; private set; }

	public string Headshots { get; private set; }

	public string Smackdowns { get; private set; }

	public string Deaths { get; private set; }

	public string KDR { get; private set; }

	public string Suicides { get; private set; }

	public string PointsEarned { get; private set; }

	public string XPEarned { get; private set; }

	public int GainedXp { get; private set; }

	public int GainedPts { get; private set; }

	public EndOfMatchData Data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
			OnDataUpdated();
		}
	}

	private EndOfMatchStats()
	{
		Data = new EndOfMatchData
		{
			MostValuablePlayers = new List<StatsSummary>(),
			PlayerStatsBestPerLife = new StatsCollection(),
			PlayerStatsTotal = new StatsCollection()
		};
	}

	private void OnDataUpdated()
	{
		if (Data.TimeInGameMinutes == 0)
		{
			PlayTime = "Less than 1 min";
		}
		else
		{
			PlayTime = string.Format("{0} min", Data.TimeInGameMinutes);
		}
		Kills = string.Format("{0}", Mathf.Max(0, Data.PlayerStatsTotal.GetKills()));
		Headshots = string.Format("{0}", Mathf.Max(0, Data.PlayerStatsTotal.Headshots));
		Smackdowns = string.Format("{0}", Mathf.Max(0, Data.PlayerStatsTotal.MeleeKills));
		Nutshots = string.Format("{0}", Mathf.Max(0, Data.PlayerStatsTotal.Nutshots));
		Deaths = Data.PlayerStatsTotal.Deaths.ToString();
		Suicides = (-Data.PlayerStatsTotal.Suicides).ToString();
		KDR = Data.PlayerStatsTotal.GetKdr().ToString("N1");
		CalculateXp();
		XPEarned = GainedXp.ToString();
		HudController.Instance.XpPtsHud.GainXp(GainedXp);
		CalculatePoints();
		PointsEarned = GainedPts.ToString();
		HudController.Instance.XpPtsHud.GainPoints(GainedPts);
	}

	private void CalculateXp()
	{
		int num = ((!Data.HasWonMatch) ? XpPointsUtil.Config.XpBaseLoser : XpPointsUtil.Config.XpBaseWinner);
		int num2 = Mathf.Max(0, Data.PlayerStatsTotal.GetKills()) * XpPointsUtil.Config.XpKill + Mathf.Max(0, Data.PlayerStatsTotal.Nutshots) * XpPointsUtil.Config.XpNutshot + Mathf.Max(0, Data.PlayerStatsTotal.Headshots) * XpPointsUtil.Config.XpHeadshot + Mathf.Max(0, Data.PlayerStatsTotal.MeleeKills) * XpPointsUtil.Config.XpSmackdown;
		float num3 = CalculateBoost(ItemPropertyType.XpBoost);
		int num4 = ((!Data.HasWonMatch) ? XpPointsUtil.Config.XpPerMinuteLoser : XpPointsUtil.Config.XpPerMinuteWinner);
		int num5 = Mathf.CeilToInt((float)(Data.TimeInGameMinutes * num4) * num3);
		int num6 = Data.TimeInGameMinutes * num4;
		GainedXp = num + num6 + num2 + num5;
		PlayTimeXp = num6.ToString();
		SkillBonusXp = num2.ToString();
		BoostXp = num5.ToString();
		TotalXp = GainedXp.ToString();
	}

	private void CalculatePoints()
	{
		int num = ((!Data.HasWonMatch) ? XpPointsUtil.Config.PointsBaseLoser : XpPointsUtil.Config.PointsBaseWinner);
		int num2 = Mathf.Max(0, Data.PlayerStatsTotal.GetKills()) * XpPointsUtil.Config.PointsKill + Mathf.Max(0, Data.PlayerStatsTotal.Nutshots) * XpPointsUtil.Config.PointsNutshot + Mathf.Max(0, Data.PlayerStatsTotal.Headshots) * XpPointsUtil.Config.PointsHeadshot + Mathf.Max(0, Data.PlayerStatsTotal.MeleeKills) * XpPointsUtil.Config.PointsSmackdown;
		float num3 = CalculateBoost(ItemPropertyType.PointsBoost);
		int num4 = ((!Data.HasWonMatch) ? XpPointsUtil.Config.PointsPerMinuteLoser : XpPointsUtil.Config.PointsPerMinuteWinner);
		int num5 = Mathf.CeilToInt((float)(Data.TimeInGameMinutes * num4) * num3);
		int num6 = Data.TimeInGameMinutes * num4;
		GainedPts = num + num6 + num2 + num5;
		PlayTimePts = num6.ToString();
		SkillBonusPts = num2.ToString();
		BoostPts = num5.ToString();
		TotalPts = GainedPts.ToString();
	}

	private float CalculateBoost(ItemPropertyType propType)
	{
		float num = 0f;
		foreach (InventoryItem inventoryItem in Singleton<InventoryManager>.Instance.InventoryItems)
		{
			if (inventoryItem.IsValid)
			{
				Dictionary<ItemPropertyType, int> itemProperties = inventoryItem.Item.View.ItemProperties;
				if (itemProperties != null && itemProperties.ContainsKey(propType))
				{
					num = Mathf.Max(num, (float)inventoryItem.Item.View.ItemProperties[propType] / 100f);
				}
			}
		}
		return num;
	}
}
