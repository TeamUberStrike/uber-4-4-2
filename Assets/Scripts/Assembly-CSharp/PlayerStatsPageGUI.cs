using UnityEngine;

internal class PlayerStatsPageGUI : PageGUI
{
	public override void DrawGUI(Rect rect)
	{
		GUI.BeginGroup(rect, GUIContent.none, BlueStonez.window_standard_grey38);
		DrawStats(new Rect(2f, 2f, rect.width - 4f, 296f));
		DrawRewards(new Rect(2f, 300f, rect.width - 4f, rect.height - 296f));
		GUI.EndGroup();
	}

	private void DrawStats(Rect rect)
	{
		GUI.Button(new Rect(rect.x, rect.y, rect.width, 40f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(rect.x + 10f, rect.y + 2f, rect.width, 40f), "MY STATUS", BlueStonez.label_interparkbold_18pt_left);
		float width = rect.width;
		float num = rect.height - 40f;
		float num2 = 32f;
		GUI.BeginGroup(new Rect(rect.x, rect.y + 40f, rect.width, rect.height - 40f), GUIContent.none, BlueStonez.window);
		GUI.Label(new Rect(5f, num2 * 0f, width + 1f, num2), new GUIContent(LocalizedStrings.PlayTime, UberstrikeIcons.Time20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 0f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.PlayTime, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 1f, width + 1f, num2), new GUIContent(LocalizedStrings.Kills, ShopIcons.Stats1Kills20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 1f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.Kills, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 2f, width + 1f, num2), new GUIContent(LocalizedStrings.Headshot, ShopIcons.Stats3Headshots20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 2f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.Headshots, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 3f, width + 1f, num2), new GUIContent(LocalizedStrings.Nutshot, ShopIcons.Stats4Nutshots20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 3f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.Nutshots, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 4f, width + 1f, num2), new GUIContent(LocalizedStrings.Smackdown, ShopIcons.Stats2Smackdowns20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 4f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.Smackdowns, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 5f, width + 1f, num2), new GUIContent(LocalizedStrings.DeathsCaps, ShopIcons.Stats6Deaths20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 5f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.Deaths, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 6f, width + 1f, num2), new GUIContent(LocalizedStrings.KDR, ShopIcons.Stats7Kdr20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 6f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.KDR, BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, num2 * 7f, width + 1f, num2), new GUIContent(LocalizedStrings.SuicideXP, ShopIcons.Stats8Suicides20x20), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, num2 * 7f, width - 5f, num2), Singleton<EndOfMatchStats>.Instance.Suicides, BlueStonez.label_interparkbold_18pt_right);
		GUI.EndGroup();
	}

	private void DrawRewards(Rect rect)
	{
		GUI.Button(new Rect(rect.x, rect.y, rect.width, 40f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(rect.x + 10f, rect.y + 2f, rect.width, 40f), "MY REWARDS", BlueStonez.label_interparkbold_18pt_left);
		float height = rect.height - 40f;
		GUI.BeginGroup(new Rect(rect.x, rect.y + 40f, rect.width, height), GUIContent.none, BlueStonez.window);
		GUI.Label(new Rect(5f, 0f, rect.width, 32f), LocalizedStrings.PlayTime, BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, 0f, rect.width - 100f, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.PlayTimeXp, UberstrikeIcons.IconXP20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(0f, 0f, rect.width, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.PlayTimePts, ShopIcons.IconPoints20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, 32f, rect.width, 32f), LocalizedStrings.SkillBonus, BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, 32f, rect.width - 100f, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.SkillBonusXp, UberstrikeIcons.IconXP20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(0f, 32f, rect.width, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.SkillBonusPts, ShopIcons.IconPoints20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, 64f, rect.width, 32f), LocalizedStrings.Boost, BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, 64f, rect.width - 100f, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.BoostXp, UberstrikeIcons.IconXP20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(0f, 64f, rect.width, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.BoostPts, ShopIcons.IconPoints20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(5f, 96f, rect.width, 32f), LocalizedStrings.TOTAL, BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, 96f, rect.width - 100f, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.TotalXp, UberstrikeIcons.IconXP20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.Label(new Rect(0f, 96f, rect.width, 32f), new GUIContent(Singleton<EndOfMatchStats>.Instance.TotalPts, ShopIcons.IconPoints20x20), BlueStonez.label_interparkbold_18pt_right);
		GUI.EndGroup();
	}
}
