using UnityEngine;

public class DebugAnimation : IDebugPage
{
	private CharacterConfig config;

	public string Title
	{
		get
		{
			return "Animation";
		}
	}

	public void Draw()
	{
		if (GameState.HasCurrentGame)
		{
			GUILayout.BeginHorizontal();
			foreach (CharacterConfig allCharacter in GameState.CurrentGame.AllCharacters)
			{
				if (GUILayout.Button(allCharacter.name))
				{
					config = allCharacter;
				}
			}
			GUILayout.EndHorizontal();
			if (config == null)
			{
				GUILayout.Label("Select a player");
			}
			else if (config.Avatar.Decorator == null)
			{
				GUILayout.Label("Missing Decorator");
			}
			else if (config.Avatar.Decorator.AnimationController == null)
			{
				GUILayout.Label("Missing Animation");
			}
			else
			{
				GUILayout.Label(config.Avatar.Decorator.AnimationController.GetDebugInfo());
			}
		}
		else
		{
			GUILayout.Label("No Game Running");
		}
	}
}
