using UnityEngine;

public class JoinNonTeamGameGUI : BaseJoinGameGUI
{
	private DeathMatchGameMode _gameMode;

	public JoinNonTeamGameGUI(DeathMatchGameMode gameMode)
	{
		_gameMode = gameMode;
	}

	public override void Draw(Rect rect)
	{
		int connectedPlayers = _gameMode.GameData.ConnectedPlayers;
		int num = Mathf.Min(8, _gameMode.GameData.MaxPlayers);
		int num2 = _gameMode.GameData.MaxPlayers - num;
		DrawPlayers(new Rect((rect.width - 130f) / 2f, 194f, 130f, 24f), Mathf.Min(num, connectedPlayers), num, StormFront.DotBlue);
		if (num2 > 0)
		{
			DrawPlayers(new Rect((rect.width - 130f) / 2f, 212f, 130f, 24f), Mathf.Max(0, connectedPlayers - num), num2, StormFront.DotBlue);
		}
		GUITools.PushGUIState();
		GUI.enabled = JoinGameUtil.CanJoinGame(_gameMode);
		if (GUITools.Button(new Rect((rect.width - 130f) / 2f, 64f, 130f, 130f), GUIContent.none, StormFront.ButtonJoinGray))
		{
			GameState.LocalAvatar.Decorator.HideWeapons();
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = false;
			GamePageManager.Instance.UnloadCurrentPage();
			_gameMode.InitializeMode();
		}
		GUITools.PopGUIState();
		DrawSpectateButton(new Rect(rect.width - 33f, rect.height - 60f, 33f, 33f));
	}
}
