using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class GameModeObjectiveHud : Singleton<GameModeObjectiveHud>
{
	private MeshGUIText _gameModeText;

	private MeshGUITextFormat _objectiveText;

	private Animatable2DGroup _entireGroup;

	private float _textScale;

	private float _textHideTime;

	private float _textDisplayTime;

	private float _fadeInOutAnimTime;

	private bool _isDisplaying;

	private static string GAME_MODE_TITLE_SURFIX = "_";

	private static string OBJECTIVE_PREFIX = "> ";

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (value)
			{
				_entireGroup.Show();
			}
			else
			{
				_entireGroup.Hide();
			}
		}
	}

	private GameModeObjectiveHud()
	{
		_entireGroup = new Animatable2DGroup();
		_gameModeText = new MeshGUIText("GameModeTitle", HudAssets.Instance.InterparkBitmapFont);
		_objectiveText = new MeshGUITextFormat(string.Empty, HudAssets.Instance.InterparkBitmapFont);
		_objectiveText.SetStyleFunc = SetMeshTextStyle;
		_entireGroup.Group.Add(_gameModeText);
		_entireGroup.Group.Add(_objectiveText);
		_textDisplayTime = 10f;
		_fadeInOutAnimTime = 0.5f;
		ResetHud();
		Enabled = true;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
		CmuneEventHandler.AddListener<OnGlobalUIRibbonChanged>(OnGlobalUIRibbonChange);
	}

	public void Update()
	{
		if (_isDisplaying && Time.time > _textHideTime)
		{
			_entireGroup.Flicker(_fadeInOutAnimTime);
			_entireGroup.FadeAlphaTo(0f, _fadeInOutAnimTime, EaseType.In);
			_isDisplaying = false;
		}
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	public void Clear()
	{
		_gameModeText.Text = string.Empty;
		_objectiveText.FormatText = string.Empty;
	}

	public void DisplayGameMode(GameMode gameMode)
	{
		SetGameModeText(gameMode);
		FadeInGameModeText();
	}

	private void SetGameModeText(GameMode gameMode)
	{
		switch (gameMode)
		{
		case GameMode.DeathMatch:
			_gameModeText.Text = "Deathmatch" + GAME_MODE_TITLE_SURFIX;
			_objectiveText.FormatText = OBJECTIVE_PREFIX + "Get as many kills as you can before the time runs out.";
			break;
		case GameMode.TeamDeathMatch:
			_gameModeText.Text = "Team Deathmatch" + GAME_MODE_TITLE_SURFIX;
			_objectiveText.FormatText = OBJECTIVE_PREFIX + "Your team must get more kills than the opposing team \n before the time runs out.";
			break;
		}
		ResetStyle();
	}

	private void FadeInGameModeText()
	{
		_entireGroup.Flicker(_fadeInOutAnimTime);
		_entireGroup.FadeAlphaTo(1f, _fadeInOutAnimTime, EaseType.In);
		_isDisplaying = true;
		_textHideTime = Time.time + _textDisplayTime;
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		SetMeshTextStyle(_gameModeText);
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		SetMeshTextStyle(_gameModeText);
		_objectiveText.UpdateStyle();
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void OnGlobalUIRibbonChange(OnGlobalUIRibbonChanged ev)
	{
		ResetTransform();
	}

	private void SetMeshTextStyle(MeshGUIText meshText)
	{
		Singleton<HudStyleUtility>.Instance.SetTeamStyle(meshText);
		meshText.Alpha = 0f;
	}

	private void ResetTransform()
	{
		_textScale = 0.45499998f;
		_gameModeText.Scale = new Vector2(_textScale, _textScale);
		_gameModeText.Position = Vector2.zero;
		_objectiveText.Scale = new Vector2(_textScale, _textScale);
		_objectiveText.Position = new Vector2(50f, (float)Screen.height * 0.08f);
		float num = 0f;
		if (GlobalUIRibbon.Instance.IsEnabled)
		{
			num = 50f;
		}
		_entireGroup.Position = new Vector2((float)Screen.width * 0.05f, (float)Screen.height * 0.1f + num);
	}
}
