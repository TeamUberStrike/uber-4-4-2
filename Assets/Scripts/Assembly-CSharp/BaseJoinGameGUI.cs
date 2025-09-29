using Cmune.DataCenter.Common.Entities;
using UnityEngine;

public abstract class BaseJoinGameGUI
{
	public abstract void Draw(Rect rect);

	protected void DrawPlayers(Rect rect, int playerCount, int maxPlayerCount, GUIStyle style)
	{
		GUI.BeginGroup(new Rect(rect.x - 1f, rect.y, rect.width, rect.height));
		float num = 24f;
		float num2 = -8.857f;
		for (int i = 0; i < maxPlayerCount; i++)
		{
			GUIStyle style2 = ((i >= playerCount) ? StormFront.DotGray : style);
			GUI.Label(new Rect((float)i * (num + num2), 0f, num, num), GUIContent.none, style2);
		}
		GUI.EndGroup();
	}

	protected void DrawSpectateButton(Rect position)
	{
		if (PlayerDataManager.AccessLevel >= MemberAccessLevel.QA && GUITools.Button(position, GUIContent.none, StormFront.ButtonCam))
		{
			GamePageManager.Instance.UnloadCurrentPage();
			Singleton<GameStateController>.Instance.SpectateCurrentGame();
			GameState.LocalAvatar.Decorator.MeshRenderer.enabled = false;
		}
	}
}
