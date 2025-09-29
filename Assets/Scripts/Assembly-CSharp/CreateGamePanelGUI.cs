using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Core.Models.Views;
using UberStrike.Core.Types;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class CreateGamePanelGUI : MonoBehaviour, IPanelGui
{
	private const int OFFSET = 6;

	private const int BUTTON_HEIGHT = 50;

	private const int MAP_WIDTH = 200;

	private const int MODE_WIDTH = 160;

	private const int DESC_WIDTH = 255;

	private const int MODS_WIDTH = 360;

	private const int MIN_WIDTH = 640;

	private const int MAX_WIDTH = 960;

	private const int MIN_NAME_FIELD_WIDTH = 115;

	private const int MAX_NAME_FIELD_WIDTH = 150;

	private const int LEFT_X = 0;

	private const int RIGHT_X = 370;

	private bool _animatingWidth;

	private bool _animatingIndex;

	private float _xOffset;

	private bool _viewingLeft = true;

	private GameFlags.GAME_FLAGS _gameFlags;

	private UberstrikeMap _mapSelected;

	private SelectionGroup<GameModeType> _modeSelection = new SelectionGroup<GameModeType>();

	private Rect _windowRect;

	private Vector2 _scroll = Vector2.zero;

	private string _gameName = string.Empty;

	private string _password = string.Empty;

	private float _textFieldWidth = 170f;

	private float _sliderWidth = 130f;

	private string _dmDescMsg = string.Empty;

	private string _tdmDescMsg = string.Empty;

	private bool IsModeSupported
	{
		get
		{
			return _mapSelected != null && _mapSelected.IsGameModeSupported(_modeSelection.Current);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return base.enabled;
		}
	}

	private void Awake()
	{
		_gameName = string.Empty;
	}

	private void Start()
	{
		_dmDescMsg = LocalizedStrings.DMModeDescriptionMsg;
		_tdmDescMsg = LocalizedStrings.TDMModeDescriptionMsg;
		_modeSelection.Add(GameModeType.TeamDeathMatch, new GUIContent(LocalizedStrings.TeamDeathMatch));
		_modeSelection.Add(GameModeType.DeathMatch, new GUIContent(LocalizedStrings.DeathMatch));
		_modeSelection.OnSelectionChange += delegate
		{
		};
	}

	private void Update()
	{
		if ((_windowRect.width != 960f && Screen.width >= 989) || (_windowRect.width != 640f && Screen.width < 989))
		{
			_animatingWidth = true;
		}
		if (_animatingWidth)
		{
			if (Screen.width < 989)
			{
				_sliderWidth = Mathf.Lerp(_sliderWidth, 160f, Time.deltaTime * 8f);
				_textFieldWidth = Mathf.Lerp(_textFieldWidth, 150f, Time.deltaTime * 8f);
				_windowRect.width = Mathf.Lerp(_windowRect.width, 640f, Time.deltaTime * 8f);
				if (Mathf.Approximately(_windowRect.width, 640f))
				{
					_animatingWidth = false;
					_sliderWidth = 160f;
					_textFieldWidth = 150f;
					_windowRect.width = 640f;
				}
			}
			else
			{
				_sliderWidth = Mathf.Lerp(_sliderWidth, 130f, Time.deltaTime * 8f);
				_textFieldWidth = Mathf.Lerp(_textFieldWidth, 115f, Time.deltaTime * 8f);
				_windowRect.width = Mathf.Lerp(_windowRect.width, 960f, Time.deltaTime * 8f);
				if (Mathf.Approximately(_windowRect.width, 960f))
				{
					_animatingWidth = false;
					_sliderWidth = 130f;
					_textFieldWidth = 115f;
					_windowRect.width = 960f;
				}
			}
		}
		if (_animatingIndex)
		{
			if (_viewingLeft)
			{
				_xOffset = Mathf.Lerp(_xOffset, 0f, Time.deltaTime * 8f);
				if (Mathf.Abs(_xOffset) < 2f)
				{
					_xOffset = 0f;
					_animatingIndex = false;
				}
			}
			else
			{
				_xOffset = Mathf.Lerp(_xOffset, 370f, Time.deltaTime * 8f);
				if (Mathf.Abs(370f - _xOffset) < 2f)
				{
					_xOffset = 370f;
					_animatingIndex = false;
				}
			}
		}
		_windowRect.x = ((float)Screen.width - _windowRect.width) * 0.5f;
		_windowRect.y = ((float)Screen.height - _windowRect.height) * 0.5f + 25f;
	}

	private void OnGUI()
	{
		GUI.BeginGroup(_windowRect, GUIContent.none, BlueStonez.window);
		DrawCreateGamePanel();
		GUI.EndGroup();
		GuiManager.DrawTooltip();
	}

	private void OnEnable()
	{
		_windowRect.width = ((Screen.width >= 989) ? 960 : 640);
		_windowRect.height = 600f;
		_password = string.Empty;
		if (Screen.width < 989)
		{
			_sliderWidth = 160f;
			_windowRect.width = 640f;
			_textFieldWidth = 150f;
		}
		else
		{
			_sliderWidth = 130f;
			_windowRect.width = 960f;
			_textFieldWidth = 115f;
		}
	}

	public void Show()
	{
		base.enabled = true;
		_viewingLeft = true;
		_gameName = PlayerDataManager.Name;
		if (_gameName.Length > 18)
		{
			_gameName = _gameName.Remove(18);
		}
	}

	public void Hide()
	{
		base.enabled = false;
	}

	private void DrawCreateGamePanel()
	{
		GUI.skin = BlueStonez.Skin;
		GUI.depth = 3;
		GUI.Label(new Rect(0f, 0f, _windowRect.width, 56f), LocalizedStrings.CreateGameCaps, BlueStonez.tab_strip);
		Rect rect = new Rect(0f, 60f, _windowRect.width, _windowRect.height - 60f);
		if (Screen.width < 989)
		{
			DrawRestrictedPanel(rect);
		}
		else
		{
			DrawFullPanel(rect);
		}
	}

	private void SelectMap(UberstrikeMap map)
	{
		_mapSelected = map;
		GameModeType[] items = _modeSelection.Items;
		foreach (GameModeType gameModeType in items)
		{
			if (_mapSelected.IsGameModeSupported(gameModeType))
			{
				_modeSelection.Select(gameModeType);
				break;
			}
		}
		AutoMonoBehaviour<TextureLoader>.Instance.SetFirstToLoadImages(new List<string> { map.MapIconUrl });
	}

	private void DrawMapSelection(Rect rect)
	{
		float width = ((Singleton<MapManager>.Instance.Count <= 8) ? rect.width : (rect.width - 18f));
		int num = 0;
		foreach (UberstrikeMap allMap in Singleton<MapManager>.Instance.AllMaps)
		{
			if (allMap.IsVisible)
			{
				num++;
			}
		}
		_scroll = GUITools.BeginScrollView(rect, _scroll, new Rect(0f, 0f, rect.width - 18f, 10 + num * 35));
		int num2 = 0;
		foreach (UberstrikeMap allMap2 in Singleton<MapManager>.Instance.AllMaps)
		{
			if (allMap2.IsVisible)
			{
				if (_mapSelected == null)
				{
					SelectMap(allMap2);
				}
				GUIContent content = ((!allMap2.IsBluebox) ? new GUIContent(allMap2.Name) : new GUIContent(" " + allMap2.Name, UberstrikeIcons.BlueLevel32));
				if (GUI.Toggle(new Rect(0f, num2 * 35, width, 35f), allMap2 == _mapSelected, content, BlueStonez.tab_large_left) && _mapSelected != allMap2)
				{
					SfxManager.Play2dAudioClip(GameAudio.CreateGame);
					SelectMap(allMap2);
				}
				num2++;
			}
		}
		GUITools.EndScrollView();
	}

	private void DrawGameModeSelection(Rect rect)
	{
		GUI.BeginGroup(rect);
		for (int i = 0; i < _modeSelection.Items.Length; i++)
		{
			GUITools.PushGUIState();
			if (_mapSelected != null && !_mapSelected.IsGameModeSupported(_modeSelection.Items[i]))
			{
				GUI.enabled = false;
			}
			if (GUI.Toggle(new Rect(0f, i * 20, rect.width, 20f), i == _modeSelection.Index, _modeSelection.GuiContent[i], BlueStonez.tab_medium) && _modeSelection.Index != i)
			{
				_modeSelection.SetIndex(i);
				if (GUI.changed)
				{
					GUI.changed = false;
					SfxManager.Play2dAudioClip(GameAudio.CreateGame);
				}
			}
			GUI.enabled = true;
			GUITools.PopGUIState();
		}
		GUI.EndGroup();
	}

	private void DrawGameDescription(Rect rect)
	{
		string text = string.Empty;
		switch (_modeSelection.Current)
		{
		case GameModeType.DeathMatch:
			text = _dmDescMsg;
			break;
		case GameModeType.TeamDeathMatch:
			text = _tdmDescMsg;
			break;
		}
		GUI.BeginGroup(rect);
		if (_mapSelected != null)
		{
			int num = 0;
			_mapSelected.Icon.Draw(new Rect(0f, 6f, rect.width, rect.width * _mapSelected.Icon.Aspect));
			num += 6 + Mathf.RoundToInt(rect.width * _mapSelected.Icon.Aspect);
			GUI.Label(new Rect(6f, num, rect.width - 12f, 20f), "Mission", BlueStonez.label_interparkbold_11pt_left);
			num += 20;
			GUI.Label(new Rect(6f, num, rect.width - 12f, 60f), text, BlueStonez.label_itemdescription);
			num += 36;
			GUI.Label(new Rect(6f, num, rect.width - 12f, 20f), "Location", BlueStonez.label_interparkbold_11pt_left);
			num += 20;
			GUI.Label(new Rect(6f, num, rect.width - 12f, 100f), _mapSelected.Description, BlueStonez.label_itemdescription);
		}
		else
		{
			GUI.Label(new Rect(6f, 100f, rect.width - 12f, 100f), "Please select a map", BlueStonez.label_interparkbold_16pt);
		}
		GUI.EndGroup();
	}

	private void DrawGameConfiguration(Rect rect)
	{
		if (IsModeSupported)
		{
			MapSettings mapSettings = _mapSelected.View.Settings[_modeSelection.Current];
			if (ApplicationDataManager.IsMobile)
			{
				mapSettings.PlayersMax = Mathf.Min(mapSettings.PlayersMax, 6);
			}
			GUI.BeginGroup(rect);
			GUI.Label(new Rect(6f, 3f, 100f, 30f), LocalizedStrings.GameName, BlueStonez.label_interparkbold_18pt_left);
			if (PlayerDataManager.AccessLevel > MemberAccessLevel.Default)
			{
				GUI.SetNextControlName("GameName");
				_gameName = GUI.TextField(new Rect(120f, 8f, _textFieldWidth, 24f), _gameName, 18, BlueStonez.textField);
				if (string.IsNullOrEmpty(_gameName) && !GUI.GetNameOfFocusedControl().Equals("GameName"))
				{
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
					GUI.Label(new Rect(128f, 15f, 200f, 24f), LocalizedStrings.EnterGameName, BlueStonez.label_interparkmed_11pt_left);
					GUI.color = Color.white;
				}
				if (_gameName.Length > 18)
				{
					_gameName = _gameName.Remove(18);
				}
			}
			else
			{
				GUI.Label(new Rect(120f, 8f, _textFieldWidth, 24f), _gameName, BlueStonez.label);
			}
			GUI.Label(new Rect(120f + _textFieldWidth + 16f, 8f, 100f, 24f), "(" + _gameName.Length + "/18)", BlueStonez.label_interparkbold_11pt_left);
			GUI.Label(new Rect(6f, 36f, 100f, 30f), LocalizedStrings.Password, BlueStonez.label_interparkbold_18pt_left);
			GUI.SetNextControlName("GamePasswd");
			_password = GUI.PasswordField(new Rect(120f, 38f, _textFieldWidth, 24f), _password, '*', 8);
			_password = _password.Trim('\n');
			if (string.IsNullOrEmpty(_password) && !GUI.GetNameOfFocusedControl().Equals("GamePasswd"))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
				GUI.Label(new Rect(128f, 45f, 200f, 24f), "No password", BlueStonez.label_interparkmed_11pt_left);
				GUI.color = Color.white;
			}
			if (_password.Length > 8)
			{
				_password = _password.Remove(8);
			}
			GUI.Label(new Rect(120f + _textFieldWidth + 16f, 38f, 100f, 24f), "(" + _password.Length + "/8)", BlueStonez.label_interparkbold_11pt_left);
			GUI.Label(new Rect(6f, 70f, 100f, 30f), LocalizedStrings.MaxPlayers, BlueStonez.label_interparkbold_18pt_left);
			GUI.Label(new Rect(120f, 73f, 33f, 20f), Mathf.RoundToInt(mapSettings.PlayersCurrent).ToString(), BlueStonez.label_dropdown);
			mapSettings.PlayersCurrent = ((!ApplicationDataManager.IsMobile) ? mapSettings.PlayersCurrent : Mathf.Clamp(mapSettings.PlayersCurrent, 0, 6));
			mapSettings.PlayersCurrent = (int)GUI.HorizontalSlider(new Rect(160f, 77f, _sliderWidth, 20f), mapSettings.PlayersCurrent, mapSettings.PlayersMin, mapSettings.PlayersMax);
			int num = Mathf.RoundToInt(mapSettings.TimeCurrent / 60);
			GUI.Label(new Rect(6f, 100f, 100f, 30f), LocalizedStrings.TimeLimit, BlueStonez.label_interparkbold_18pt_left);
			GUI.Label(new Rect(120f, 103f, 33f, 20f), num.ToString(), BlueStonez.label_dropdown);
			mapSettings.TimeCurrent = 60 * (int)GUI.HorizontalSlider(new Rect(160f, 107f, _sliderWidth, 20f), num, mapSettings.TimeMin / 60, mapSettings.TimeMax / 60);
			GUI.Label(new Rect(6f, 132f, 100f, 30f), LocalizedStrings.MaxKills, BlueStonez.label_interparkbold_18pt_left);
			GUI.Label(new Rect(120f, 135f, 33f, 20f), Mathf.RoundToInt(mapSettings.KillsCurrent).ToString(), BlueStonez.label_dropdown);
			mapSettings.KillsCurrent = (int)GUI.HorizontalSlider(new Rect(160f, 139f, _sliderWidth, 20f), mapSettings.KillsCurrent, mapSettings.KillsMin, mapSettings.KillsMax);
			GUI.EndGroup();
		}
		else
		{
			GUI.Label(rect, "Unsupported Game Mode!", BlueStonez.label_interparkbold_18pt);
		}
	}

	private void ToggleGameFlag(GameFlags.GAME_FLAGS flag, int y, string content)
	{
		if (GUI.Toggle(new Rect(6f, y, 160f, 16f), _gameFlags == flag, content, BlueStonez.toggle))
		{
			_gameFlags = flag;
		}
		else if (_gameFlags == flag)
		{
			_gameFlags = GameFlags.GAME_FLAGS.None;
		}
	}

	private void DrawFullPanel(Rect rect)
	{
		int num = 6;
		int num2 = (int)rect.height - 50;
		GUI.BeginGroup(rect);
		GUI.Box(new Rect(6f, 0f, rect.width - 12f, num2), GUIContent.none, BlueStonez.window_standard_grey38);
		DrawMapSelection(new Rect(num, 0f, 200f, num2));
		num += 206;
		DrawVerticalLine(num - 3, 2f, num2);
		DrawGameModeSelection(new Rect(num, 0f, 160f, num2));
		num += 166;
		DrawVerticalLine(num - 3, 2f, num2);
		DrawGameDescription(new Rect(num, 0f, 255f, num2));
		num += 261;
		DrawVerticalLine(num - 3, 2f, num2);
		DrawGameConfiguration(new Rect(num, 0f, 360f, num2));
		if (GUITools.Button(new Rect(rect.width - 138f, rect.height - 40f, 120f, 32f), new GUIContent(LocalizedStrings.CancelCaps), BlueStonez.button))
		{
			PanelManager.Instance.ClosePanel(PanelType.CreateGame);
		}
		GUITools.PushGUIState();
		GUI.enabled = IsModeSupported && Singleton<GameServerController>.Instance.SelectedServer.IsValid && LocalizationHelper.ValidateMemberName(_gameName, ApplicationDataManager.CurrentLocale) && (string.IsNullOrEmpty(_password) || ValidateGamePassword(_password));
		if (GUITools.Button(new Rect(rect.width - 138f - 125f, rect.height - 40f, 120f, 32f), new GUIContent(LocalizedStrings.CreateCaps), BlueStonez.button_green, GameAudio.JoinGame))
		{
			PanelManager.Instance.ClosePanel(PanelType.CreateGame);
			MapSettings mapSettings = _mapSelected.View.Settings[_modeSelection.Current];
			_gameName = TextUtilities.Trim(_gameName);
			int timeMinutes = Mathf.RoundToInt(mapSettings.TimeCurrent / 60) * 60;
			Singleton<GameStateController>.Instance.CreateGame(_mapSelected, _gameName, _password, timeMinutes, mapSettings.KillsCurrent, mapSettings.PlayersCurrent, _modeSelection.Current, _gameFlags);
		}
		GUITools.PopGUIState();
		GUI.EndGroup();
	}

	private void DrawRestrictedPanel(Rect rect)
	{
		float num = 6f - _xOffset;
		int num2 = (int)rect.height - 50;
		GUI.BeginGroup(rect);
		GUI.Box(new Rect(6f, 0f, rect.width - 12f, num2), GUIContent.none, BlueStonez.window_standard_grey38);
		if (_animatingIndex || _viewingLeft)
		{
			DrawMapSelection(new Rect(num, 0f, 200f, num2));
		}
		num += 206f;
		if (_animatingIndex || _viewingLeft)
		{
			DrawVerticalLine(num - 3f, 2f, 300f);
			DrawGameModeSelection(new Rect(num, 0f, 160f, num2));
		}
		num += 166f;
		if (_animatingIndex || _viewingLeft)
		{
			DrawVerticalLine(num - 3f, 2f, 300f);
		}
		DrawGameDescription(new Rect(num, 0f, 255f, num2));
		num += 261f;
		if (_animatingIndex || !_viewingLeft)
		{
			DrawVerticalLine(num - 3f, 2f, 300f);
		}
		DrawGameConfiguration(new Rect(num, 0f, 360f, num2));
		if (GUITools.Button(new Rect(rect.width - 138f, rect.height - 40f, 120f, 32f), new GUIContent(LocalizedStrings.CancelCaps), BlueStonez.button))
		{
			PanelManager.Instance.ClosePanel(PanelType.CreateGame);
		}
		GUITools.PushGUIState();
		GUI.enabled = !_animatingIndex && !_animatingWidth;
		string text = ((!_viewingLeft) ? "Back" : "Customize");
		if (GUITools.Button(new Rect(rect.width - 138f - 125f, rect.height - 40f, 120f, 32f), new GUIContent(text), BlueStonez.button))
		{
			_animatingIndex = true;
			_viewingLeft = !_viewingLeft;
		}
		GUITools.PopGUIState();
		GUITools.PushGUIState();
		GUI.enabled = IsModeSupported && Singleton<GameServerController>.Instance.SelectedServer.IsValid && LocalizationHelper.ValidateMemberName(_gameName, ApplicationDataManager.CurrentLocale) && (string.IsNullOrEmpty(_password) || ValidateGamePassword(_password));
		if (GUITools.Button(new Rect(rect.width - 138f - 250f, rect.height - 40f, 120f, 32f), new GUIContent(LocalizedStrings.CreateCaps), BlueStonez.button_green))
		{
			PanelManager.Instance.ClosePanel(PanelType.CreateGame);
			MapSettings mapSettings = _mapSelected.View.Settings[_modeSelection.Current];
			Singleton<GameStateController>.Instance.CreateGame(_mapSelected, _gameName, _password, mapSettings.TimeCurrent, mapSettings.KillsCurrent, mapSettings.PlayersCurrent, _modeSelection.Current, _gameFlags);
		}
		GUITools.PopGUIState();
		GUI.EndGroup();
	}

	private void DrawVerticalLine(float x, float y, float height)
	{
		GUI.Label(new Rect(x, y, 1f, height), GUIContent.none, BlueStonez.vertical_line_grey95);
	}

	private bool ValidateGamePassword(string psv)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(psv) && psv.Length <= 8)
		{
			result = true;
		}
		return result;
	}
}
