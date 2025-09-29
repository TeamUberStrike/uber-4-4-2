using UnityEngine;

public static class GameAudio
{
	public static AudioClip SeletronRadioShort { get; private set; }

	public static AudioClip BigSplash { get; private set; }

	public static AudioClip ImpactCement1 { get; private set; }

	public static AudioClip ImpactCement2 { get; private set; }

	public static AudioClip ImpactCement3 { get; private set; }

	public static AudioClip ImpactCement4 { get; private set; }

	public static AudioClip ImpactGlass1 { get; private set; }

	public static AudioClip ImpactGlass2 { get; private set; }

	public static AudioClip ImpactGlass3 { get; private set; }

	public static AudioClip ImpactGlass4 { get; private set; }

	public static AudioClip ImpactGlass5 { get; private set; }

	public static AudioClip ImpactGrass1 { get; private set; }

	public static AudioClip ImpactGrass2 { get; private set; }

	public static AudioClip ImpactGrass3 { get; private set; }

	public static AudioClip ImpactGrass4 { get; private set; }

	public static AudioClip ImpactMetal1 { get; private set; }

	public static AudioClip ImpactMetal2 { get; private set; }

	public static AudioClip ImpactMetal3 { get; private set; }

	public static AudioClip ImpactMetal4 { get; private set; }

	public static AudioClip ImpactMetal5 { get; private set; }

	public static AudioClip ImpactSand1 { get; private set; }

	public static AudioClip ImpactSand2 { get; private set; }

	public static AudioClip ImpactSand3 { get; private set; }

	public static AudioClip ImpactSand4 { get; private set; }

	public static AudioClip ImpactSand5 { get; private set; }

	public static AudioClip ImpactStone1 { get; private set; }

	public static AudioClip ImpactStone2 { get; private set; }

	public static AudioClip ImpactStone3 { get; private set; }

	public static AudioClip ImpactStone4 { get; private set; }

	public static AudioClip ImpactStone5 { get; private set; }

	public static AudioClip ImpactWater1 { get; private set; }

	public static AudioClip ImpactWater2 { get; private set; }

	public static AudioClip ImpactWater3 { get; private set; }

	public static AudioClip ImpactWater4 { get; private set; }

	public static AudioClip ImpactWater5 { get; private set; }

	public static AudioClip ImpactWood1 { get; private set; }

	public static AudioClip ImpactWood2 { get; private set; }

	public static AudioClip ImpactWood3 { get; private set; }

	public static AudioClip ImpactWood4 { get; private set; }

	public static AudioClip ImpactWood5 { get; private set; }

	public static AudioClip MediumSplash { get; private set; }

	public static AudioClip BlueWins { get; private set; }

	public static AudioClip CountdownTonal1 { get; private set; }

	public static AudioClip CountdownTonal2 { get; private set; }

	public static AudioClip Draw { get; private set; }

	public static AudioClip Fight { get; private set; }

	public static AudioClip FocusEnemy { get; private set; }

	public static AudioClip GameOver { get; private set; }

	public static AudioClip GetPoints { get; private set; }

	public static AudioClip GetXP { get; private set; }

	public static AudioClip LevelUp { get; private set; }

	public static AudioClip LostLead { get; private set; }

	public static AudioClip MatchEndingCountdown1 { get; private set; }

	public static AudioClip MatchEndingCountdown2 { get; private set; }

	public static AudioClip MatchEndingCountdown3 { get; private set; }

	public static AudioClip MatchEndingCountdown4 { get; private set; }

	public static AudioClip MatchEndingCountdown5 { get; private set; }

	public static AudioClip RedWins { get; private set; }

	public static AudioClip TakenLead { get; private set; }

	public static AudioClip TiedLead { get; private set; }

	public static AudioClip YouWin { get; private set; }

	public static AudioClip AmmoPickup2D { get; private set; }

	public static AudioClip ArmorShard2D { get; private set; }

	public static AudioClip BigHealth2D { get; private set; }

	public static AudioClip GoldArmor2D { get; private set; }

	public static AudioClip MediumHealth2D { get; private set; }

	public static AudioClip MegaHealth2D { get; private set; }

	public static AudioClip SilverArmor2D { get; private set; }

	public static AudioClip SmallHealth2D { get; private set; }

	public static AudioClip WeaponPickup2D { get; private set; }

	public static AudioClip FootStepDirt1 { get; private set; }

	public static AudioClip FootStepDirt2 { get; private set; }

	public static AudioClip FootStepDirt3 { get; private set; }

	public static AudioClip FootStepDirt4 { get; private set; }

	public static AudioClip FootStepGlass1 { get; private set; }

	public static AudioClip FootStepGlass2 { get; private set; }

	public static AudioClip FootStepGlass3 { get; private set; }

	public static AudioClip FootStepGlass4 { get; private set; }

	public static AudioClip FootStepGrass1 { get; private set; }

	public static AudioClip FootStepGrass2 { get; private set; }

	public static AudioClip FootStepGrass3 { get; private set; }

	public static AudioClip FootStepGrass4 { get; private set; }

	public static AudioClip FootStepHeavyMetal1 { get; private set; }

	public static AudioClip FootStepHeavyMetal2 { get; private set; }

	public static AudioClip FootStepHeavyMetal3 { get; private set; }

	public static AudioClip FootStepHeavyMetal4 { get; private set; }

	public static AudioClip FootStepMetal1 { get; private set; }

	public static AudioClip FootStepMetal2 { get; private set; }

	public static AudioClip FootStepMetal3 { get; private set; }

	public static AudioClip FootStepMetal4 { get; private set; }

	public static AudioClip FootStepRock1 { get; private set; }

	public static AudioClip FootStepRock2 { get; private set; }

	public static AudioClip FootStepRock3 { get; private set; }

	public static AudioClip FootStepRock4 { get; private set; }

	public static AudioClip FootStepSand1 { get; private set; }

	public static AudioClip FootStepSand2 { get; private set; }

	public static AudioClip FootStepSand3 { get; private set; }

	public static AudioClip FootStepSand4 { get; private set; }

	public static AudioClip FootStepSnow1 { get; private set; }

	public static AudioClip FootStepSnow2 { get; private set; }

	public static AudioClip FootStepSnow3 { get; private set; }

	public static AudioClip FootStepSnow4 { get; private set; }

	public static AudioClip FootStepWater1 { get; private set; }

	public static AudioClip FootStepWater2 { get; private set; }

	public static AudioClip FootStepWater3 { get; private set; }

	public static AudioClip FootStepWood1 { get; private set; }

	public static AudioClip FootStepWood2 { get; private set; }

	public static AudioClip FootStepWood3 { get; private set; }

	public static AudioClip FootStepWood4 { get; private set; }

	public static AudioClip GotHeadshotKill { get; private set; }

	public static AudioClip GotNutshotKill { get; private set; }

	public static AudioClip KilledBySplatbat { get; private set; }

	public static AudioClip LandingGrunt { get; private set; }

	public static AudioClip LocalPlayerHitArmorRemaining { get; private set; }

	public static AudioClip LocalPlayerHitNoArmor { get; private set; }

	public static AudioClip LocalPlayerHitNoArmorLowHealth { get; private set; }

	public static AudioClip NormalKill1 { get; private set; }

	public static AudioClip NormalKill2 { get; private set; }

	public static AudioClip NormalKill3 { get; private set; }

	public static AudioClip QuickItemRecharge { get; private set; }

	public static AudioClip SwimAboveWater1 { get; private set; }

	public static AudioClip SwimAboveWater2 { get; private set; }

	public static AudioClip SwimAboveWater3 { get; private set; }

	public static AudioClip SwimAboveWater4 { get; private set; }

	public static AudioClip SwimUnderWater { get; private set; }

	public static AudioClip AmmoPickup { get; private set; }

	public static AudioClip ArmorShard { get; private set; }

	public static AudioClip BigHealth { get; private set; }

	public static AudioClip GoldArmor { get; private set; }

	public static AudioClip JumpPad { get; private set; }

	public static AudioClip JumpPad2D { get; private set; }

	public static AudioClip MediumHealth { get; private set; }

	public static AudioClip MegaHealth { get; private set; }

	public static AudioClip SilverArmor { get; private set; }

	public static AudioClip SmallHealth { get; private set; }

	public static AudioClip TargetDamage { get; private set; }

	public static AudioClip TargetPopup { get; private set; }

	public static AudioClip WeaponPickup { get; private set; }

	public static AudioClip ButtonClick { get; private set; }

	public static AudioClip ClickReady { get; private set; }

	public static AudioClip ClickUnready { get; private set; }

	public static AudioClip ClosePanel { get; private set; }

	public static AudioClip CreateGame { get; private set; }

	public static AudioClip DoubleKill { get; private set; }

	public static AudioClip EndOfRound { get; private set; }

	public static AudioClip EquipGear { get; private set; }

	public static AudioClip EquipItem { get; private set; }

	public static AudioClip EquipWeapon { get; private set; }

	public static AudioClip FBScreenshot { get; private set; }

	public static AudioClip HeadShot { get; private set; }

	public static AudioClip JoinGame { get; private set; }

	public static AudioClip JoinServer { get; private set; }

	public static AudioClip KillLeft1 { get; private set; }

	public static AudioClip KillLeft2 { get; private set; }

	public static AudioClip KillLeft3 { get; private set; }

	public static AudioClip KillLeft4 { get; private set; }

	public static AudioClip KillLeft5 { get; private set; }

	public static AudioClip LeaveServer { get; private set; }

	public static AudioClip MegaKill { get; private set; }

	public static AudioClip MysteryBoxMusic { get; private set; }

	public static AudioClip MysteryBoxWin { get; private set; }

	public static AudioClip NewMessage { get; private set; }

	public static AudioClip NewRequest { get; private set; }

	public static AudioClip NutShot { get; private set; }

	public static AudioClip Objective { get; private set; }

	public static AudioClip ObjectiveTick { get; private set; }

	public static AudioClip OpenPanel { get; private set; }

	public static AudioClip QuadKill { get; private set; }

	public static AudioClip RibbonClick { get; private set; }

	public static AudioClip Smackdown { get; private set; }

	public static AudioClip SubObjective { get; private set; }

	public static AudioClip TripleKill { get; private set; }

	public static AudioClip UberKill { get; private set; }

	public static AudioClip LauncherBounce1 { get; private set; }

	public static AudioClip LauncherBounce2 { get; private set; }

	public static AudioClip OutOfAmmoClick { get; private set; }

	public static AudioClip SniperScopeIn { get; private set; }

	public static AudioClip SniperScopeOut { get; private set; }

	public static AudioClip SniperZoomIn { get; private set; }

	public static AudioClip SniperZoomOut { get; private set; }

	public static AudioClip UnderwaterExplosion1 { get; private set; }

	public static AudioClip UnderwaterExplosion2 { get; private set; }

	public static AudioClip WeaponSwitch { get; private set; }

	static GameAudio()
	{
		AudioClipConfigurator component;
		try
		{
			component = GameObject.Find("GameAudio").GetComponent<AudioClipConfigurator>();
		}
		catch
		{
			Debug.LogError("Missing instance of the prefab with name: GameAudio!");
			return;
		}
		SeletronRadioShort = component.Assets[0];
		BigSplash = component.Assets[1];
		ImpactCement1 = component.Assets[2];
		ImpactCement2 = component.Assets[3];
		ImpactCement3 = component.Assets[4];
		ImpactCement4 = component.Assets[5];
		ImpactGlass1 = component.Assets[6];
		ImpactGlass2 = component.Assets[7];
		ImpactGlass3 = component.Assets[8];
		ImpactGlass4 = component.Assets[9];
		ImpactGlass5 = component.Assets[10];
		ImpactGrass1 = component.Assets[11];
		ImpactGrass2 = component.Assets[12];
		ImpactGrass3 = component.Assets[13];
		ImpactGrass4 = component.Assets[14];
		ImpactMetal1 = component.Assets[15];
		ImpactMetal2 = component.Assets[16];
		ImpactMetal3 = component.Assets[17];
		ImpactMetal4 = component.Assets[18];
		ImpactMetal5 = component.Assets[19];
		ImpactSand1 = component.Assets[20];
		ImpactSand2 = component.Assets[21];
		ImpactSand3 = component.Assets[22];
		ImpactSand4 = component.Assets[23];
		ImpactSand5 = component.Assets[24];
		ImpactStone1 = component.Assets[25];
		ImpactStone2 = component.Assets[26];
		ImpactStone3 = component.Assets[27];
		ImpactStone4 = component.Assets[28];
		ImpactStone5 = component.Assets[29];
		ImpactWater1 = component.Assets[30];
		ImpactWater2 = component.Assets[31];
		ImpactWater3 = component.Assets[32];
		ImpactWater4 = component.Assets[33];
		ImpactWater5 = component.Assets[34];
		ImpactWood1 = component.Assets[35];
		ImpactWood2 = component.Assets[36];
		ImpactWood3 = component.Assets[37];
		ImpactWood4 = component.Assets[38];
		ImpactWood5 = component.Assets[39];
		MediumSplash = component.Assets[40];
		BlueWins = component.Assets[41];
		CountdownTonal1 = component.Assets[42];
		CountdownTonal2 = component.Assets[43];
		Draw = component.Assets[44];
		Fight = component.Assets[45];
		FocusEnemy = component.Assets[46];
		GameOver = component.Assets[47];
		GetPoints = component.Assets[48];
		GetXP = component.Assets[49];
		LevelUp = component.Assets[50];
		LostLead = component.Assets[51];
		MatchEndingCountdown1 = component.Assets[52];
		MatchEndingCountdown2 = component.Assets[53];
		MatchEndingCountdown3 = component.Assets[54];
		MatchEndingCountdown4 = component.Assets[55];
		MatchEndingCountdown5 = component.Assets[56];
		RedWins = component.Assets[57];
		TakenLead = component.Assets[58];
		TiedLead = component.Assets[59];
		YouWin = component.Assets[60];
		AmmoPickup2D = component.Assets[61];
		ArmorShard2D = component.Assets[62];
		BigHealth2D = component.Assets[63];
		GoldArmor2D = component.Assets[64];
		MediumHealth2D = component.Assets[65];
		MegaHealth2D = component.Assets[66];
		SilverArmor2D = component.Assets[67];
		SmallHealth2D = component.Assets[68];
		WeaponPickup2D = component.Assets[69];
		FootStepDirt1 = component.Assets[70];
		FootStepDirt2 = component.Assets[71];
		FootStepDirt3 = component.Assets[72];
		FootStepDirt4 = component.Assets[73];
		FootStepGlass1 = component.Assets[74];
		FootStepGlass2 = component.Assets[75];
		FootStepGlass3 = component.Assets[76];
		FootStepGlass4 = component.Assets[77];
		FootStepGrass1 = component.Assets[78];
		FootStepGrass2 = component.Assets[79];
		FootStepGrass3 = component.Assets[80];
		FootStepGrass4 = component.Assets[81];
		FootStepHeavyMetal1 = component.Assets[82];
		FootStepHeavyMetal2 = component.Assets[83];
		FootStepHeavyMetal3 = component.Assets[84];
		FootStepHeavyMetal4 = component.Assets[85];
		FootStepMetal1 = component.Assets[86];
		FootStepMetal2 = component.Assets[87];
		FootStepMetal3 = component.Assets[88];
		FootStepMetal4 = component.Assets[89];
		FootStepRock1 = component.Assets[90];
		FootStepRock2 = component.Assets[91];
		FootStepRock3 = component.Assets[92];
		FootStepRock4 = component.Assets[93];
		FootStepSand1 = component.Assets[94];
		FootStepSand2 = component.Assets[95];
		FootStepSand3 = component.Assets[96];
		FootStepSand4 = component.Assets[97];
		FootStepSnow1 = component.Assets[98];
		FootStepSnow2 = component.Assets[99];
		FootStepSnow3 = component.Assets[100];
		FootStepSnow4 = component.Assets[101];
		FootStepWater1 = component.Assets[102];
		FootStepWater2 = component.Assets[103];
		FootStepWater3 = component.Assets[104];
		FootStepWood1 = component.Assets[105];
		FootStepWood2 = component.Assets[106];
		FootStepWood3 = component.Assets[107];
		FootStepWood4 = component.Assets[108];
		GotHeadshotKill = component.Assets[109];
		GotNutshotKill = component.Assets[110];
		KilledBySplatbat = component.Assets[111];
		LandingGrunt = component.Assets[112];
		LocalPlayerHitArmorRemaining = component.Assets[113];
		LocalPlayerHitNoArmor = component.Assets[114];
		LocalPlayerHitNoArmorLowHealth = component.Assets[115];
		NormalKill1 = component.Assets[116];
		NormalKill2 = component.Assets[117];
		NormalKill3 = component.Assets[118];
		QuickItemRecharge = component.Assets[119];
		SwimAboveWater1 = component.Assets[120];
		SwimAboveWater2 = component.Assets[121];
		SwimAboveWater3 = component.Assets[122];
		SwimAboveWater4 = component.Assets[123];
		SwimUnderWater = component.Assets[124];
		AmmoPickup = component.Assets[125];
		ArmorShard = component.Assets[126];
		BigHealth = component.Assets[127];
		GoldArmor = component.Assets[128];
		JumpPad = component.Assets[129];
		JumpPad2D = component.Assets[130];
		MediumHealth = component.Assets[131];
		MegaHealth = component.Assets[132];
		SilverArmor = component.Assets[133];
		SmallHealth = component.Assets[134];
		TargetDamage = component.Assets[135];
		TargetPopup = component.Assets[136];
		WeaponPickup = component.Assets[137];
		ButtonClick = component.Assets[138];
		ClickReady = component.Assets[139];
		ClickUnready = component.Assets[140];
		ClosePanel = component.Assets[141];
		CreateGame = component.Assets[142];
		DoubleKill = component.Assets[143];
		EndOfRound = component.Assets[144];
		EquipGear = component.Assets[145];
		EquipItem = component.Assets[146];
		EquipWeapon = component.Assets[147];
		FBScreenshot = component.Assets[148];
		HeadShot = component.Assets[149];
		JoinGame = component.Assets[150];
		JoinServer = component.Assets[151];
		KillLeft1 = component.Assets[152];
		KillLeft2 = component.Assets[153];
		KillLeft3 = component.Assets[154];
		KillLeft4 = component.Assets[155];
		KillLeft5 = component.Assets[156];
		LeaveServer = component.Assets[157];
		MegaKill = component.Assets[158];
		MysteryBoxMusic = component.Assets[159];
		MysteryBoxWin = component.Assets[160];
		NewMessage = component.Assets[161];
		NewRequest = component.Assets[162];
		NutShot = component.Assets[163];
		Objective = component.Assets[164];
		ObjectiveTick = component.Assets[165];
		OpenPanel = component.Assets[166];
		QuadKill = component.Assets[167];
		RibbonClick = component.Assets[168];
		Smackdown = component.Assets[169];
		SubObjective = component.Assets[170];
		TripleKill = component.Assets[171];
		UberKill = component.Assets[172];
		LauncherBounce1 = component.Assets[173];
		LauncherBounce2 = component.Assets[174];
		OutOfAmmoClick = component.Assets[175];
		SniperScopeIn = component.Assets[176];
		SniperScopeOut = component.Assets[177];
		SniperZoomIn = component.Assets[178];
		SniperZoomOut = component.Assets[179];
		UnderwaterExplosion1 = component.Assets[180];
		UnderwaterExplosion2 = component.Assets[181];
		WeaponSwitch = component.Assets[182];
	}
}
