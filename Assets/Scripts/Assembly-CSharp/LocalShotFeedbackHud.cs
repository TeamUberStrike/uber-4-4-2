using UnityEngine;

public class LocalShotFeedbackHud : Singleton<LocalShotFeedbackHud>
{
	private Animatable2DGroup _textGroup;

	private LocalShotFeedbackHud()
	{
		_textGroup = new Animatable2DGroup();
	}

	public void Update()
	{
		_textGroup.Draw();
	}

	public void DisplayLocalShotFeedback(InGameEventFeedbackType type)
	{
		MeshGUIText meshGUIText = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		AudioClip sound = null;
		switch (type)
		{
		case InGameEventFeedbackType.NutShot:
			meshGUIText.Text = "Nut Shot";
			sound = GameAudio.NutShot;
			break;
		case InGameEventFeedbackType.HeadShot:
			meshGUIText.Text = "Head Shot";
			sound = GameAudio.HeadShot;
			break;
		case InGameEventFeedbackType.Humiliation:
			meshGUIText.Text = "Smackdown";
			sound = GameAudio.Smackdown;
			break;
		}
		ResetTransform(meshGUIText);
		ResetStyle(meshGUIText);
		LocalShotFeedbackAnim anim = new LocalShotFeedbackAnim(_textGroup, meshGUIText, 1f, 1f, sound);
		Singleton<InGameFeatHud>.Instance.AnimationScheduler.EnqueueAnim(anim);
	}

	private void ResetTransform(MeshGUIText text)
	{
		float num = Singleton<InGameFeatHud>.Instance.TextHeight / text.TextBounds.y;
		text.Scale = new Vector2(num, num);
		text.Position = Singleton<InGameFeatHud>.Instance.AnchorPoint;
	}

	private void ResetStyle(MeshGUIText text)
	{
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(text);
		text.Alpha = 0f;
		text.ShadowColorAnim.Alpha = 0f;
	}
}
