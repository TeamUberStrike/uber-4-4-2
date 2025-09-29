using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameData
{
	public static GameData Instance = new GameData();

	public Property<MainMenuState> MainMenu = new Property<MainMenuState>(MainMenuState.Home);

	public Property<List<CommActorInfo>> Players = new Property<List<CommActorInfo>>();

	public static bool CanShowFacebookView
	{
		get
		{
			return ApplicationDataManager.Channel == ChannelType.WebFacebook || ApplicationDataManager.Channel == ChannelType.IPad || Application.isEditor;
		}
	}
}
