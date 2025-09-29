using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class EndOfRoundPageScene : PageScene
{
	private int _endOfMatchCountdown;

	private MeshGUIText _redTeamText;

	private MeshGUIText _blueTeamText;

	private MeshGUIText _redTeamSplats;

	private MeshGUIText _blueTeamSplats;

	private Animatable2DGroup _scoreGroup = new Animatable2DGroup();

	public override PageType PageType
	{
		get
		{
			return PageType.EndOfMatch;
		}
	}

	protected override void OnLoad()
	{
		CmuneEventHandler.AddListener<TeamGameEndEvent>(OnTeamGameEnd);
		CmuneEventHandler.AddListener<EndOfMatchCountdownEvent>(OnEndOfMatchCountdown);
	}

	protected override void OnUnload()
	{
		_scoreGroup.Hide();
		GameState.LocalAvatar.Decorator.SetLayers(UberstrikeLayer.LocalPlayer);
		CmuneEventHandler.RemoveListener<TeamGameEndEvent>(OnTeamGameEnd);
		CmuneEventHandler.RemoveListener<EndOfMatchCountdownEvent>(OnEndOfMatchCountdown);
	}

	private void OnGUI()
	{
		GUI.depth = 100;
		if (!GameState.HasCurrentGame)
		{
			return;
		}
		float num = (float)Screen.height * 0.2f;
		Rect pixelRect = GameState.CurrentSpace.Camera.pixelRect;
		if (GameState.CurrentGameMode == GameMode.TeamDeathMatch)
		{
			Rect position = new Rect((pixelRect.width - num * 2f - 40f) / 2f, GlobalUIRibbon.Instance.Height() + 30, num * 2f + 40f, num);
			GUI.BeginGroup(position);
			GUI.Label(new Rect(0f, 0f, num, num), GUIContent.none, StormFront.BlueBox);
			GUI.Label(new Rect(position.width - num, 0f, num, num), GUIContent.none, StormFront.RedBox);
			if (_redTeamSplats != null)
			{
				_redTeamSplats.Position = new Vector2((position.xMax - num / 2f) * ((float)Screen.width / pixelRect.width), position.y + num / 2f - position.height * 0.25f);
				_redTeamSplats.Draw();
			}
			if (_blueTeamSplats != null)
			{
				_blueTeamSplats.Position = new Vector2((position.x + num / 2f) * ((float)Screen.width / pixelRect.width), position.y + num / 2f - position.height * 0.25f);
				_blueTeamSplats.Draw();
			}
			if (_redTeamText != null)
			{
				_redTeamText.Position = new Vector2((position.xMax - num / 2f) * ((float)Screen.width / pixelRect.width), position.y + num / 2f + position.height * 0.3f);
				_redTeamText.Draw();
			}
			if (_blueTeamText != null)
			{
				_blueTeamText.Position = new Vector2((position.x + num / 2f) * ((float)Screen.width / pixelRect.width), position.y + num / 2f + position.height * 0.3f);
				_blueTeamText.Draw();
			}
			GUI.EndGroup();
		}
		DrawReadyButton(new Rect((pixelRect.width - num * 2f - 60f) / 2f, (float)GlobalUIRibbon.Instance.Height() + num + 40f, num * 2f + 60f, 50f));
	}

	private void OnTeamGameEnd(TeamGameEndEvent ev)
	{
		if (_redTeamText == null)
		{
			_redTeamText = new MeshGUIText(LocalizedStrings.RedCaps, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_redTeamText);
			_redTeamText.Scale = new Vector2(0.5f, 0.5f);
			_scoreGroup.Group.Add(_redTeamText);
		}
		if (_blueTeamText == null)
		{
			_blueTeamText = new MeshGUIText(LocalizedStrings.BlueCaps, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_blueTeamText);
			_blueTeamText.Scale = new Vector2(0.5f, 0.5f);
			_scoreGroup.Group.Add(_blueTeamText);
		}
		if (_redTeamSplats == null)
		{
			_redTeamSplats = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
			Singleton<HudStyleUtility>.Instance.SetRedStyle(_redTeamSplats);
			_redTeamSplats.Scale = new Vector2(0.8f, 0.8f);
			_scoreGroup.Group.Add(_redTeamSplats);
		}
		if (_blueTeamSplats == null)
		{
			_blueTeamSplats = new MeshGUIText(string.Empty, HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
			Singleton<HudStyleUtility>.Instance.SetBlueStyle(_blueTeamSplats);
			_blueTeamSplats.Scale = new Vector2(0.8f, 0.8f);
			_scoreGroup.Group.Add(_blueTeamSplats);
		}
		_redTeamSplats.Text = ev.RedTeamSplats.ToString();
		_blueTeamSplats.Text = ev.BlueTeamSplats.ToString();
		_scoreGroup.Show();
	}

	private void DrawReadyButton(Rect rect)
	{
		if (GameState.HasCurrentGame)
		{
			GUI.BeginGroup(rect);
			string text = string.Format("{0} {1}/{2}", LocalizedStrings.ReadyCaps, GameState.CurrentGame.PlayerReadyForNextRound, GameState.CurrentGame.Players.Count);
			GUITools.PushGUIState();
			GUI.enabled = !GameState.IsReadyForNextGame;
			bool flag = GUI.Toggle(new Rect(rect.width / 2f - 70f, rect.height / 2f - 23f, 140f, 45f), GameState.IsReadyForNextGame, text, StormFront.ButtonGray);
			GUITools.PopGUIState();
			GUI.Label(new Rect(rect.width / 2f + 80f, 0f, 60f, rect.height), _endOfMatchCountdown.ToString(), BlueStonez.label_interparkbold_48pt_left);
			if (flag && !GameState.IsReadyForNextGame)
			{
				GameState.IsReadyForNextGame = true;
				GUITools.Clicked();
				GameState.CurrentGame.SetReadyForNextMatch(true);
				SfxManager.Play2dAudioClip(GameAudio.ClickReady);
			}
			else if (!flag && GameState.IsReadyForNextGame)
			{
				SfxManager.Play2dAudioClip(GameAudio.ClickUnready);
			}
			GUI.EndGroup();
		}
	}

	private void OnEndOfMatchCountdown(EndOfMatchCountdownEvent ev)
	{
		_endOfMatchCountdown = ev.Countdown;
	}
}
