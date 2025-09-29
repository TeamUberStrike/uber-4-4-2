using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class JoinTeamGameGUI : BaseJoinGameGUI
{
	private TeamDeathMatchGameMode _gameMode;

	public JoinTeamGameGUI(TeamDeathMatchGameMode gameMode)
	{
		_gameMode = gameMode;
	}

	public override void Draw(Rect rect)
	{
		float num = Mathf.Min(400f, rect.width);
		Vector2 zero = Vector2.zero;
		zero.x = (rect.width - num) / 2f;
		zero.y = zero.x + num;
		float left = zero.y - 130f;
		int maxPlayerCount = Mathf.CeilToInt((float)_gameMode.GameData.MaxPlayers / 2f);
		DrawPlayers(new Rect(zero.x, 194f, 130f, 24f), _gameMode.BlueTeamPlayerCount, maxPlayerCount, StormFront.DotBlue);
		GUITools.PushGUIState();
		GUI.enabled = JoinGameUtil.CanJoinBlueTeam(_gameMode);
		if (GUITools.Button(new Rect(zero.x, 64f, 130f, 130f), GUIContent.none, StormFront.ButtonJoinBlue))
		{
			GameState.LocalAvatar.Decorator.HideWeapons();
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = false;
			GamePageManager.Instance.UnloadCurrentPage();
			_gameMode.InitializeMode(TeamID.BLUE);
		}
		GUITools.PopGUIState();
		GUITools.PushGUIState();
		GUI.enabled = JoinGameUtil.CanJoinRedTeam(_gameMode);
		DrawPlayers(new Rect(left, 194f, 130f, 24f), _gameMode.RedTeamPlayerCount, maxPlayerCount, StormFront.DotRed);
		if (GUITools.Button(new Rect(left, 64f, 130f, 130f), GUIContent.none, StormFront.ButtonJoinRed))
		{
			GameState.LocalAvatar.Decorator.HideWeapons();
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = false;
			GamePageManager.Instance.UnloadCurrentPage();
			_gameMode.InitializeMode(TeamID.RED);
		}
		GUITools.PopGUIState();
		DrawSpectateButton(new Rect(rect.width - 33f, rect.height - 60f, 33f, 33f));
	}
}
