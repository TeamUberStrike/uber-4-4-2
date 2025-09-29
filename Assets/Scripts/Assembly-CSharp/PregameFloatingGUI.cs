using UnityEngine;

public class PregameFloatingGUI : MonoBehaviour
{
	private BaseJoinGameGUI _joinGameGUI;

	private MeshGUIText _gameModeText;

	private void OnEnable()
	{
		InitializeJoinGameGUI();
	}

	private void OnDisable()
	{
		if (_gameModeText != null)
		{
			_gameModeText.Hide();
		}
	}

	private void OnGUI()
	{
		GUI.depth = 100;
		if (GameState.HasCurrentSpace)
		{
			Rect rect = new Rect(30f, 80f, GameState.CurrentSpace.Camera.pixelRect.width - 60f, Screen.height - 80);
			GUI.BeginGroup(rect, string.Empty);
			DrawJoinArea(rect);
			GUI.EndGroup();
		}
	}

	private void InitializeJoinGameGUI()
	{
		if (GameState.HasCurrentGame)
		{
			switch (GameState.CurrentGameMode)
			{
			case GameMode.Training:
				_joinGameGUI = new JoinTrainingGameGUI(GameState.CurrentGame as TrainingFpsMode);
				break;
			case GameMode.DeathMatch:
				_joinGameGUI = new JoinNonTeamGameGUI(GameState.CurrentGame as DeathMatchGameMode);
				break;
			case GameMode.TeamDeathMatch:
				_joinGameGUI = new JoinTeamGameGUI(GameState.CurrentGame as TeamDeathMatchGameMode);
				break;
			}
			string text = string.Empty;
			switch (GameState.CurrentGameMode)
			{
			case GameMode.DeathMatch:
				text = LocalizedStrings.DeathMatch.ToUpper();
				break;
			case GameMode.TeamDeathMatch:
				text = LocalizedStrings.TeamDeathMatch.ToUpper();
				break;
			}
			if (_gameModeText != null)
			{
				_gameModeText.Text = text;
				_gameModeText.Show();
			}
			else
			{
				_gameModeText = new MeshGUIText(text, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
				Singleton<HudStyleUtility>.Instance.SetBlackStyle(_gameModeText);
				_gameModeText.Scale = new Vector2(0.4f, 0.4f);
			}
		}
	}

	private void DrawJoinArea(Rect rect)
	{
		_gameModeText.Position = new Vector2(Screen.width / 2, 120f);
		_gameModeText.Draw();
		if (_joinGameGUI != null)
		{
			_joinGameGUI.Draw(rect);
		}
	}
}
