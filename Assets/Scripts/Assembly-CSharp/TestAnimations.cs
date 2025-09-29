using System;
using UnityEngine;

public class TestAnimations : MonoBehaviour
{
	private Vector2 scroll;

	private Vector2 itemSize = new Vector2(220f, 30f);

	public bool stopall;

	private void OnGUI()
	{
		if (!(GameState.LocalAvatar.Decorator != null))
		{
			return;
		}
		scroll = GUITools.BeginScrollView(new Rect(1f, 100f, itemSize.x + 20f, Screen.height - 20), scroll, new Rect(0f, 0f, itemSize.x, (float)GameState.LocalAvatar.Decorator.Animation.GetClipCount() * itemSize.y));
		int num = 0;
		foreach (AnimationState item in GameState.LocalAvatar.Decorator.Animation)
		{
			if ((bool)item)
			{
				if (GUI.Button(new Rect(0f, (float)num * itemSize.y, itemSize.x, itemSize.y), "Play " + item.name))
				{
					AnimationIndex id = (AnimationIndex)(int)Enum.Parse(typeof(AnimationIndex), item.name, true);
					GameState.LocalAvatar.Decorator.AnimationController.TriggerAnimation(id, stopall);
				}
			}
			else
			{
				GUI.Label(new Rect(0f, (float)num * itemSize.y, itemSize.x, itemSize.y), "Missing clip");
			}
			num++;
		}
		GUITools.EndScrollView();
	}
}
