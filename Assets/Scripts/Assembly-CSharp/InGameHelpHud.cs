using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class InGameHelpHud : Singleton<InGameHelpHud>
{
	private float _overlayBoxHeight;

	private float _curTextScaleFactor;

	private MeshGUIQuad _scoreBoradKeyIcon;

	private MeshGUIText _scoreBoardKey;

	private MeshGUIText _scoreBoardHelpText;

	private Sprite2DGUI _scoreBoardOverlay;

	private Animatable2DGroup _scoreBoardHelpGroup;

	private MeshGUIText _fullscreenKey1;

	private Sprite2DGUI _fullscreenOverlay1;

	private MeshGUIText _fullscreenKey2;

	private Sprite2DGUI _fullscreenOverlay2;

	private MeshGUIText _fullscreenKeyPlus;

	private MeshGUIText _fullscreenHelpText;

	private Animatable2DGroup _fullScreenHelpGroup;

	private MeshGUIText _changeTeamKey1;

	private Sprite2DGUI _changeTeamOverlay1;

	private MeshGUIText _changeTeamKey2;

	private Sprite2DGUI _changeTeamOverlay2;

	private MeshGUIText _changeTeamKeyPlus;

	private MeshGUIText _changeTeamHelpText;

	private Animatable2DGroup _changeTeamHelpGroup;

	private Sprite2DButton _loadoutKeyIcon;

	private MeshGUIText _loadoutKey;

	private MeshGUIText _loadoutHelpText;

	private Sprite2DGUI _loadoutOverlay;

	private Animatable2DGroup _loadoutHelpGroup;

	private Animatable2DGroup _entireGroup;

	public bool Enabled
	{
		get
		{
			return _entireGroup.IsVisible;
		}
		set
		{
			if (!value)
			{
				_entireGroup.Hide();
			}
		}
	}

	public bool EnableChangeTeamHelp { get; set; }

	private InGameHelpHud()
	{
		InitScoreBoardHelpGroup();
		InitFullScreenHelpGroup();
		InitChangeTeamHelpGroup();
		InitLoadoutHelpGroup();
		_entireGroup = new Animatable2DGroup();
		_entireGroup.Group.Add(_scoreBoardHelpGroup);
		_entireGroup.Group.Add(_fullScreenHelpGroup);
		_entireGroup.Group.Add(_changeTeamHelpGroup);
		_entireGroup.Group.Add(_loadoutHelpGroup);
		ResetHud();
		Enabled = false;
		CmuneEventHandler.AddListener<OnSetPlayerTeamEvent>(OnTeamChange);
		CmuneEventHandler.AddListener<ScreenResolutionEvent>(OnScreenResolutionChange);
		CmuneEventHandler.AddListener<CameraWidthChangeEvent>(OnCameraRectChange);
	}

	public void Draw()
	{
		_entireGroup.Draw();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.L))
		{
			OnToggleLoadout();
		}
	}

	private void InitScoreBoardHelpGroup()
	{
		_scoreBoradKeyIcon = new MeshGUIQuad(HudTextures.DeathScreenTab);
		_scoreBoardKey = new MeshGUIText("Tab", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleLeft);
		_scoreBoardHelpText = new MeshGUIText("Scoreboard", HudAssets.Instance.InterparkBitmapFont);
		_scoreBoardOverlay = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_scoreBoardHelpGroup = new Animatable2DGroup();
		_scoreBoardHelpGroup.Group.Add(_scoreBoardHelpText);
		_scoreBoardHelpGroup.Group.Add(_scoreBoardOverlay);
		_scoreBoardHelpGroup.Group.Add(_scoreBoradKeyIcon);
		_scoreBoardHelpGroup.Group.Add(_scoreBoardKey);
	}

	private void InitFullScreenHelpGroup()
	{
		_fullscreenKey1 = new MeshGUIText("ALT", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_fullscreenOverlay1 = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_fullscreenKey2 = new MeshGUIText("F", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_fullscreenOverlay2 = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_fullscreenKeyPlus = new MeshGUIText("+", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_fullscreenHelpText = new MeshGUIText("Fullscreen", HudAssets.Instance.InterparkBitmapFont);
		_fullScreenHelpGroup = new Animatable2DGroup();
		_fullScreenHelpGroup.Group.Add(_fullscreenKey1);
		_fullScreenHelpGroup.Group.Add(_fullscreenOverlay1);
		_fullScreenHelpGroup.Group.Add(_fullscreenKey2);
		_fullScreenHelpGroup.Group.Add(_fullscreenOverlay2);
		_fullScreenHelpGroup.Group.Add(_fullscreenKeyPlus);
		_fullScreenHelpGroup.Group.Add(_fullscreenHelpText);
	}

	private void InitChangeTeamHelpGroup()
	{
		_changeTeamKey1 = new MeshGUIText("ALT", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_changeTeamOverlay1 = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_changeTeamKey2 = new MeshGUIText("M", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_changeTeamOverlay2 = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_changeTeamKeyPlus = new MeshGUIText("+", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_changeTeamHelpText = new MeshGUIText("Change Team", HudAssets.Instance.InterparkBitmapFont);
		_changeTeamHelpGroup = new Animatable2DGroup();
		_changeTeamHelpGroup.Group.Add(_changeTeamKey1);
		_changeTeamHelpGroup.Group.Add(_changeTeamOverlay1);
		_changeTeamHelpGroup.Group.Add(_changeTeamKey2);
		_changeTeamHelpGroup.Group.Add(_changeTeamOverlay2);
		_changeTeamHelpGroup.Group.Add(_changeTeamKeyPlus);
		_changeTeamHelpGroup.Group.Add(_changeTeamHelpText);
	}

	private void InitLoadoutHelpGroup()
	{
		_loadoutKey = new MeshGUIText("L", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_loadoutOverlay = new Sprite2DGUI(new GUIContent(), StormFront.BlueBox);
		_loadoutHelpText = new MeshGUIText("loadout", HudAssets.Instance.InterparkBitmapFont, TextAnchor.MiddleCenter);
		_loadoutKeyIcon = new Sprite2DButton(GUIContent.none, StormFront.ButtonLoadout);
		_loadoutKeyIcon.IsUsingGuiContentBounds = false;
		_loadoutKeyIcon.GUIBounds = new Vector2(64f, 64f);
		_loadoutKeyIcon.OnClick = OnToggleLoadout;
		_loadoutHelpGroup = new Animatable2DGroup();
		_loadoutHelpGroup.Group.Add(_loadoutKey);
		_loadoutHelpGroup.Group.Add(_loadoutOverlay);
		_loadoutHelpGroup.Group.Add(_loadoutHelpText);
		_loadoutHelpGroup.Group.Add(_loadoutKeyIcon);
	}

	private void OnToggleLoadout()
	{
		if (GamePageManager.IsCurrentPage(PageType.None))
		{
			EnterShop();
		}
		else
		{
			LeaveShop();
		}
	}

	private void ResetHud()
	{
		ResetStyle();
		ResetTransform();
	}

	private void ResetStyle()
	{
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_scoreBoardKey);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_scoreBoardHelpText);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_fullscreenKey1);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_fullscreenKey2);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_fullscreenKeyPlus);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_fullscreenHelpText);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_changeTeamKey1);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_changeTeamKey2);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_changeTeamKeyPlus);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_changeTeamHelpText);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_loadoutHelpText);
		Singleton<HudStyleUtility>.Instance.SetNoShadowStyle(_loadoutKey);
	}

	private void OnTeamChange(OnSetPlayerTeamEvent ev)
	{
		if (ev.TeamId == TeamID.RED)
		{
			_scoreBoardOverlay.Style = StormFront.RedBox;
			_fullscreenOverlay1.Style = StormFront.RedBox;
			_fullscreenOverlay2.Style = StormFront.RedBox;
			_changeTeamOverlay1.Style = StormFront.RedBox;
			_changeTeamOverlay2.Style = StormFront.RedBox;
			_loadoutOverlay.Style = StormFront.RedBox;
			_loadoutKeyIcon.Style = StormFront.ButtonLoadoutRed;
		}
		else
		{
			_scoreBoardOverlay.Style = StormFront.BlueBox;
			_fullscreenOverlay1.Style = StormFront.BlueBox;
			_fullscreenOverlay2.Style = StormFront.BlueBox;
			_changeTeamOverlay1.Style = StormFront.BlueBox;
			_changeTeamOverlay2.Style = StormFront.BlueBox;
			_loadoutOverlay.Style = StormFront.BlueBox;
			_loadoutKeyIcon.Style = StormFront.ButtonLoadout;
		}
	}

	private void OnCameraRectChange(CameraWidthChangeEvent ev)
	{
		ResetTransform();
	}

	private void OnScreenResolutionChange(ScreenResolutionEvent ev)
	{
		ResetTransform();
	}

	private void ResetTransform()
	{
		_curTextScaleFactor = 0.1f;
		_overlayBoxHeight = (float)Screen.height * 0.06f;
		ResetScoreBoardHelpGroup();
		ResetFullscreenHelpGroup();
		ResetChangeTeamHelpGroup();
		ResetLoadoutHelpGroup();
		_scoreBoardHelpGroup.Position = new Vector2(0f, 0f);
		float num = 0f;
		float num2 = _overlayBoxHeight * 0.4f;
		num += _scoreBoardHelpGroup.Rect.height + num2;
		_fullScreenHelpGroup.Position = new Vector2(0f, num);
		num += _fullScreenHelpGroup.Rect.height + num2;
		if (EnableChangeTeamHelp)
		{
			_changeTeamHelpGroup.Position = new Vector2(0f, num);
			num += _changeTeamHelpGroup.Rect.height + num2;
		}
		_loadoutHelpGroup.Position = new Vector2(0f, num);
		Vector2 position = new Vector2((float)Screen.width * 0.05f, (float)Screen.height * 0.2f);
		position.x += (float)Screen.width * (1f - AutoMonoBehaviour<CameraRectController>.Instance.NormalizedWidth) / 2f;
		_entireGroup.Position = position;
		_entireGroup.UpdateMeshGUIPosition();
	}

	private void ResetScoreBoardHelpGroup()
	{
		Rect rect = new Rect(0f, 0f, (float)Screen.width * 0.08f, _overlayBoxHeight);
		_scoreBoardOverlay.Position = new Vector2(0f, 0f);
		_scoreBoardOverlay.Scale = new Vector2(rect.width / _scoreBoardOverlay.GUIBounds.x, rect.height / _scoreBoardOverlay.GUIBounds.y);
		_scoreBoardKey.Position = new Vector2(0f, rect.height / 2f);
		_scoreBoardKey.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		float num = _scoreBoardKey.Size.x / (float)_scoreBoradKeyIcon.Texture.width;
		_scoreBoradKeyIcon.Position = new Vector2(rect.width * 0.92f - _scoreBoradKeyIcon.Size.x, rect.height / 2f - _scoreBoradKeyIcon.Size.y / 2f);
		_scoreBoradKeyIcon.Scale = new Vector2(num, num);
		_scoreBoardHelpText.Position = new Vector2(0f, rect.height * 1.05f);
		_scoreBoardHelpText.Scale = new Vector2(_curTextScaleFactor * 1.5f, _curTextScaleFactor * 1.5f);
	}

	private void ResetFullscreenHelpGroup()
	{
		Rect rect = new Rect(0f, 0f, _overlayBoxHeight, _overlayBoxHeight);
		_fullscreenOverlay1.Position = new Vector2(rect.x, rect.y);
		_fullscreenOverlay1.Scale = new Vector2(rect.width / _fullscreenOverlay1.GUIBounds.x, rect.height / _fullscreenOverlay1.GUIBounds.y);
		_fullscreenKey1.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		_fullscreenKey1.Position = new Vector2(rect.width / 2f, rect.height / 2f);
		_fullscreenKeyPlus.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		_fullscreenKeyPlus.Position = new Vector2(rect.width * 1.25f, rect.height / 2f);
		rect = new Rect(_overlayBoxHeight * 1.5f, 0f, _overlayBoxHeight, _overlayBoxHeight);
		_fullscreenOverlay2.Position = new Vector2(rect.x, rect.y);
		_fullscreenOverlay2.Scale = new Vector2(rect.width / _fullscreenOverlay2.GUIBounds.x, rect.height / _fullscreenOverlay2.GUIBounds.y);
		_fullscreenKey2.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		_fullscreenKey2.Position = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
		_fullscreenHelpText.Scale = new Vector2(_curTextScaleFactor * 1.5f, _curTextScaleFactor * 1.5f);
		_fullscreenHelpText.Position = new Vector2(0f, rect.height * 1.05f);
	}

	private void ResetChangeTeamHelpGroup()
	{
		Rect rect = new Rect(0f, 0f, _overlayBoxHeight, _overlayBoxHeight);
		_changeTeamOverlay1.Position = new Vector2(rect.x, rect.y);
		_changeTeamOverlay1.Scale = new Vector2(rect.width / _changeTeamOverlay1.GUIBounds.x, rect.height / _changeTeamOverlay1.GUIBounds.y);
		_changeTeamKey1.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		_changeTeamKey1.Position = new Vector2(rect.width / 2f, rect.height / 2f);
		_changeTeamKeyPlus.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		_changeTeamKeyPlus.Position = new Vector2(rect.width * 1.25f, rect.height / 2f);
		rect = new Rect(_overlayBoxHeight * 1.5f, 0f, _overlayBoxHeight, _overlayBoxHeight);
		_changeTeamOverlay2.Position = new Vector2(rect.x, rect.y);
		_changeTeamOverlay2.Scale = new Vector2(rect.width / _changeTeamOverlay2.GUIBounds.x, rect.height / _changeTeamOverlay2.GUIBounds.y);
		_changeTeamKey2.Scale = new Vector2(_curTextScaleFactor * 2f, _curTextScaleFactor * 2f);
		_changeTeamKey2.Position = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
		_changeTeamHelpText.Scale = new Vector2(_curTextScaleFactor * 1.5f, _curTextScaleFactor * 1.5f);
		_changeTeamHelpText.Position = new Vector2(0f, rect.height * 1.05f);
	}

	private void ResetLoadoutHelpGroup()
	{
		Rect rect = new Rect(0f, 0f, (float)Screen.width * 0.08f, _overlayBoxHeight * 3f);
		_loadoutOverlay.Position = new Vector2(0f, 0f);
		_loadoutOverlay.Scale = new Vector2(rect.width / _loadoutOverlay.GUIBounds.x, rect.height / _loadoutOverlay.GUIBounds.y);
		_loadoutKey.Scale = new Vector2(_curTextScaleFactor * 3f, _curTextScaleFactor * 3f);
		_loadoutKey.Position = new Vector2(rect.width / 2f, rect.height * 0.2f);
		_loadoutKeyIcon.Scale = new Vector2(1f, 1f);
		_loadoutKeyIcon.Position = new Vector2(rect.width / 2f - _loadoutKeyIcon.Size.x / 2f, rect.height / 2f - _loadoutKeyIcon.Size.y / 2f);
		_loadoutHelpText.Scale = new Vector2(_curTextScaleFactor * 2.7f, _curTextScaleFactor * 2.7f);
		_loadoutHelpText.Position = new Vector2(rect.width / 2f, rect.height * 0.85f);
	}

	private void EnterShop()
	{
		GameState.LocalPlayer.Pause();
		GamePageManager.Instance.LoadPage(PageType.Shop);
	}

	private void LeaveShop()
	{
		GamePageManager.Instance.UnloadCurrentPage();
	}
}
