using UnityEngine;

public class JoinTrainingGameGUI : BaseJoinGameGUI
{
	private TrainingFpsMode _gameMode;

	public JoinTrainingGameGUI(TrainingFpsMode gameMode)
	{
		_gameMode = gameMode;
	}

	public override void Draw(Rect rect)
	{
		float left = 0f;
		if (GUITools.Button(new Rect(left, 45f, 130f, 130f), GUIContent.none, StormFront.ButtonJoinGray))
		{
			GamePageManager.Instance.UnloadCurrentPage();
			_gameMode.InitializeMode();
		}
	}
}
