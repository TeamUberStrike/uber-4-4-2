using System;
using System.Collections;
using System.Collections.Generic;
using Cmune.DataCenter.Common.Entities;
using UberStrike.Realtime.UnitySdk;
using UnityEngine;

public class PlayPageGUI : MonoBehaviour
{
	private enum ServerLatency
	{
		Fast = 100,
		Med = 300
	}

	private enum GameListColumns
	{
		None = 0,
		Lock = 1,
		Star = 2,
		GameName = 3,
		GameMap = 4,
		GameMode = 5,
		PlayerCount = 6,
		GameServerPing = 7,
		GameTime = 8
	}

	private enum ServerListColumns
	{
		None = 0,
		ServerName = 1,
		ServerCapacity = 2,
		ServerSpeed = 3
	}

	private class FilterSavedData
	{
		public bool UseFilter;

		public string MapName = string.Empty;

		public string GameMode = string.Empty;

		public bool NoFriendlyFire;

		public bool ISGameNotFull;

		public bool NoPasswordProtection;
	}

	private const float _doubleClickFrame = 0.5f;

	private const int _dropDownListMap = 1;

	private const int _dropDownListGameMode = 2;

	private const int _dropDownListSingleWeapon = 4;

	public const int MAX_PLAYERS_ON_MOBILE = 6;

	private const int _mapImageWidth = 110;

	private const int _gameTimeWidth = 80;

	private const int _playerCountWidth = 80;

	private const int _joinGameWidth = 110;

	private const float _serverSpeedColumnWidth = 110f;

	private const float _serverPlayerCountColumnWidth = 130f;

	private const int LobbyConnectionMaxTime = 30;

	private const int ServerCheckDelay = 5;

	private string[] _mapsFilter;

	private string[] _modesFilter;

	private float _joinServerButtonWidth = 130f;

	private Dictionary<int, DynamicTexture> _mapImages = new Dictionary<int, DynamicTexture>();

	[SerializeField]
	private Texture2D _level1GameIcon;

	[SerializeField]
	private Texture2D _level2GameIcon;

	[SerializeField]
	private Texture2D _level3GameIcon;

	[SerializeField]
	private Texture2D _level5GameIcon;

	[SerializeField]
	private Texture2D _level10GameIcon;

	[SerializeField]
	private Texture2D _level20GameIcon;

	[SerializeField]
	private Texture2D _privateGameIcon;

	[SerializeField]
	private Texture2D _sortUpArrow;

	[SerializeField]
	private Texture2D _sortDownArrow;

	private float _gameJoinDoubleClick;

	private float _serverJoinDoubleClick;

	private bool _isConnectedToGameServer;

	private bool _refreshGameListOnSortChange;

	private Vector2 _serverScroll;

	private Vector2 _filterScroll;

	private bool _gameNotFull;

	private bool _noPrivateGames;

	private bool _instasplat;

	private bool _lowGravity;

	private bool _justForFun;

	private bool _singleWeapon;

	private bool _noFriendFire;

	private bool _showFilters;

	private string[] _weaponClassTexts;

	private int _dropDownList;

	private Rect _dropDownRect;

	private int _currentMap;

	private int _currentMode;

	private int _currentWeapon;

	private GameMetaData _selectedGame;

	private int _mapNameWidth = 135;

	private int _gameModeWidth = 170;

	private int _gameNameWidth = 200;

	private float _serverNameColumnWidth;

	private string _inputedPassword = string.Empty;

	private bool _unFocus;

	private bool _checkForPassword;

	private bool _isPasswordOk = true;

	private float _timeDelayOnCheckPassword = 2f;

	private float _nextPasswordCheck;

	private bool _sortServerAscending;

	private bool _sortGamesAscending;

	private int _filteredActiveRoomCount;

	private GameListColumns _lastSortedColumn;

	private FilterSavedData _filterSavedData;

	private List<GameMetaData> _cachedGameList;

	private IComparer<GameMetaData> _gameSortingMethod;

	private Vector2 _serverSelectionHelpScrollBar;

	private Vector2 _serverSelectionScrollBar;

	private ServerListColumns _lastSortedServerColumn;

	private float _nextServerCheckTime;

	private float _timeToLobbyDisconnect;

	private SearchBarGUI _searchBar;

	public static PlayPageGUI Instance { get; private set; }

	public static bool Exists
	{
		get
		{
			return Instance != null;
		}
	}

	public bool CheckForPassword
	{
		set
		{
			_checkForPassword = value;
		}
	}

	public bool IsConnectedToGameServer
	{
		get
		{
			return _isConnectedToGameServer;
		}
	}

	private bool AreFiltersActive
	{
		get
		{
			if (_currentMap != 0)
			{
				return true;
			}
			if (_currentMode != 0)
			{
				return true;
			}
			if (_currentWeapon != 0)
			{
				return true;
			}
			if (_noFriendFire)
			{
				return true;
			}
			if (_gameNotFull)
			{
				return true;
			}
			if (_noPrivateGames)
			{
				return true;
			}
			if (_instasplat)
			{
				return true;
			}
			if (_lowGravity)
			{
				return true;
			}
			if (_justForFun)
			{
				return true;
			}
			if (_singleWeapon)
			{
				return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		Instance = this;
		_filterSavedData = new FilterSavedData();
		_cachedGameList = new List<GameMetaData>();
		_sortGamesAscending = false;
		_gameSortingMethod = new GameDataPlayerComparer();
		_lastSortedColumn = GameListColumns.PlayerCount;
		_searchBar = new SearchBarGUI("SearchGame");
		if (_privateGameIcon == null)
		{
			throw new Exception("_privateGameIcon not assigned");
		}
	}

	private void Start()
	{
		_weaponClassTexts = new string[5]
		{
			LocalizedStrings.Handguns,
			LocalizedStrings.Machineguns,
			LocalizedStrings.SniperRifles,
			LocalizedStrings.Shotguns,
			LocalizedStrings.Launchers
		};
		_modesFilter = new string[3]
		{
			LocalizedStrings.All + " Modes",
			LocalizedStrings.DeathMatch,
			LocalizedStrings.TeamDeathMatch
		};
		List<string> list = new List<string>();
		list.Add(LocalizedStrings.All + " Maps");
		foreach (UberstrikeMap allMap in Singleton<MapManager>.Instance.AllMaps)
		{
			if (allMap.Id != 0)
			{
				list.Add(allMap.Name);
			}
		}
		list.RemoveAll((string s) => string.IsNullOrEmpty(s));
		_mapsFilter = list.ToArray();
	}

	private void OnEnable()
	{
		_showFilters = false;
		ResetFilters();
		_unFocus = true;
	}

	private void OnDisable()
	{
	}

	private void OnGUI()
	{
		GUI.depth = 11;
		GUI.skin = BlueStonez.Skin;
		if (_unFocus)
		{
			if (GUIUtility.keyboardControl != 0)
			{
				GUIUtility.keyboardControl = 0;
			}
			_unFocus = false;
		}
		Rect rect = new Rect(0f, GlobalUIRibbon.Instance.Height(), Screen.width, Screen.height - GlobalUIRibbon.Instance.Height());
		GUI.Box(rect, string.Empty, BlueStonez.box_grey31);
		if (!_isConnectedToGameServer || Singleton<GameServerController>.Instance.SelectedServer == null)
		{
			DoServerPage(rect);
		}
		else if (_isConnectedToGameServer && Singleton<GameServerController>.Instance.SelectedServer != null)
		{
			DoGamePage(rect);
		}
		if (_checkForPassword)
		{
			PasswordCheck(new Rect((Screen.width - 280) / 2, (Screen.height - 200) / 2, 280f, 200f));
		}
		GuiManager.DrawTooltip();
	}

	private void ResetFilters()
	{
		_currentMap = 0;
		_currentMode = 0;
		_currentWeapon = 0;
		_noFriendFire = false;
		_gameNotFull = false;
		_noPrivateGames = false;
		_instasplat = false;
		_lowGravity = false;
		_justForFun = false;
		_singleWeapon = false;
		_searchBar.ClearFilter();
	}

	private void DoServerPage(Rect rect)
	{
		float num = 200f;
		GUI.BeginGroup(rect);
		GUI.Label(new Rect(0f, 0f, rect.width, 56f), LocalizedStrings.ChooseYourRegionCaps, BlueStonez.tab_strip);
		GUI.Box(new Rect(0f, 55f, rect.width, rect.height - 57f), string.Empty, BlueStonez.window_standard_grey38);
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		GUI.Label(new Rect(0f, 28f, rect.width - 5f, 28f), string.Format("{0} {1}, {2} {3} ", Singleton<GameServerManager>.Instance.AllPlayersCount, LocalizedStrings.PlayersOnline, Singleton<GameServerManager>.Instance.AllGamesCount, LocalizedStrings.Games), BlueStonez.label_interparkbold_18pt_right);
		GUI.color = Color.white;
		bool flag = GUI.enabled;
		GUI.enabled = flag && Time.time > _nextServerCheckTime;
		if (GUITools.Button(new Rect(rect.width - 150f, 6f, 140f, 23f), new GUIContent(LocalizedStrings.Refresh), BlueStonez.buttondark_medium))
		{
			RefreshServerLoad();
		}
		GUI.enabled = flag;
		DoServerList(new Rect(10f, 55f, rect.width - num - 10f, rect.height - 49f));
		DoServerHelpText(new Rect(rect.width - num, 55f, num - 10f, rect.height - 49f));
		DrawServerListButtons(rect, num);
		GUI.EndGroup();
	}

	private void DoServerHelpText(Rect position)
	{
		GUI.BeginGroup(position);
		GUI.Box(new Rect(0f, 0f, position.width, 32f), LocalizedStrings.HelpCaps, BlueStonez.box_grey50);
		GUI.Box(new Rect(0f, 31f, position.width, position.height - 31f - 55f), string.Empty, BlueStonez.box_grey50);
		_serverSelectionHelpScrollBar = GUITools.BeginScrollView(new Rect(0f, 33f, position.width, position.height - 31f - 60f), _serverSelectionHelpScrollBar, new Rect(0f, 0f, position.width - 20f, 400f));
		DrawGroupLabel(new Rect(5f, 5f, position.width - 25f, 100f), "1. " + LocalizedStrings.ServerName, LocalizedStrings.ServerNameDesc);
		DrawGroupLabel(new Rect(5f, 105f, position.width - 25f, 70f), "2. " + LocalizedStrings.Capacity, LocalizedStrings.CapacityDesc);
		DrawGroupLabel(new Rect(5f, 180f, position.width - 25f, 180f), "3. " + LocalizedStrings.Speed, LocalizedStrings.SpeedDesc);
		GUITools.EndScrollView();
		GUI.EndGroup();
	}

	private void DoServerList(Rect position)
	{
		_serverNameColumnWidth = position.width - 130f - 110f - _joinServerButtonWidth + 1f - 20f;
		GUI.BeginGroup(position);
		GUI.Box(new Rect(0f, 0f, _serverNameColumnWidth + 1f, 32f), string.Empty, BlueStonez.box_grey50);
		GUI.Box(new Rect(_serverNameColumnWidth, 0f, 131f, 32f), string.Empty, BlueStonez.box_grey50);
		GUI.Box(new Rect(_serverNameColumnWidth + 130f, 0f, 111f, 32f), string.Empty, BlueStonez.box_grey50);
		GUI.Box(new Rect(_serverNameColumnWidth + 130f + _joinServerButtonWidth - 20f, 0f, _joinServerButtonWidth + 20f, 32f), string.Empty, BlueStonez.box_grey50);
		GUI.Box(new Rect(0f, 31f, position.width + 1f, position.height - 31f - 55f), string.Empty, BlueStonez.box_grey50);
		if (_lastSortedServerColumn == ServerListColumns.ServerName)
		{
			if (_sortServerAscending)
			{
				GUI.Label(new Rect(5f, 0f, _serverNameColumnWidth + 1f - 5f, 32f), new GUIContent(LocalizedStrings.ServerName, _sortUpArrow), BlueStonez.label_interparkbold_18pt_left);
			}
			else
			{
				GUI.Label(new Rect(5f, 0f, _serverNameColumnWidth + 1f - 5f, 32f), new GUIContent(LocalizedStrings.ServerName, _sortDownArrow), BlueStonez.label_interparkbold_18pt_left);
			}
		}
		else
		{
			GUI.Label(new Rect(12f, 0f, _serverNameColumnWidth + 1f - 5f, 32f), LocalizedStrings.ServerName, BlueStonez.label_interparkbold_18pt_left);
		}
		if (GUI.Button(new Rect(0f, 0f, _serverNameColumnWidth + 1f - 5f, 32f), GUIContent.none, BlueStonez.label_interparkbold_11pt_left))
		{
			SortServerList(ServerListColumns.ServerName);
		}
		if (_lastSortedServerColumn == ServerListColumns.ServerCapacity)
		{
			if (_sortServerAscending)
			{
				GUI.Label(new Rect(5f + _serverNameColumnWidth, 0f, 126f, 32f), new GUIContent(LocalizedStrings.Capacity, _sortUpArrow), BlueStonez.label_interparkbold_18pt_left);
			}
			else
			{
				GUI.Label(new Rect(5f + _serverNameColumnWidth, 0f, 126f, 32f), new GUIContent(LocalizedStrings.Capacity, _sortDownArrow), BlueStonez.label_interparkbold_18pt_left);
			}
		}
		else
		{
			GUI.Label(new Rect(_serverNameColumnWidth + 12f, 0f, 126f, 32f), LocalizedStrings.Capacity, BlueStonez.label_interparkbold_18pt_left);
		}
		if (GUI.Button(new Rect(_serverNameColumnWidth, 0f, 126f, 32f), GUIContent.none, BlueStonez.label_interparkbold_11pt_left))
		{
			SortServerList(ServerListColumns.ServerCapacity);
		}
		if (_lastSortedServerColumn == ServerListColumns.ServerSpeed)
		{
			if (_sortServerAscending)
			{
				GUI.Label(new Rect(5f + _serverNameColumnWidth + 130f, 0f, 105f, 32f), new GUIContent(LocalizedStrings.Speed, _sortUpArrow), BlueStonez.label_interparkbold_18pt_left);
			}
			else
			{
				GUI.Label(new Rect(5f + _serverNameColumnWidth + 130f, 0f, 105f, 32f), new GUIContent(LocalizedStrings.Speed, _sortDownArrow), BlueStonez.label_interparkbold_18pt_left);
			}
		}
		else
		{
			GUI.Label(new Rect(_serverNameColumnWidth + 130f + 12f, 0f, 105f, 32f), LocalizedStrings.Speed, BlueStonez.label_interparkbold_18pt_left);
		}
		if (GUI.Button(new Rect(_serverNameColumnWidth + 130f, 0f, 105f, 32f), GUIContent.none, BlueStonez.label_interparkbold_11pt_left))
		{
			SortServerList(ServerListColumns.ServerSpeed);
		}
		DrawAllServers(position);
		GUI.EndGroup();
	}

	private void DrawProgressBarLarge(Rect position, float amount)
	{
		amount = Mathf.Clamp01(amount);
		GUI.Box(new Rect(position.x, position.y, position.width, 23f), GUIContent.none, BlueStonez.progressbar_large_background);
		GUI.color = ColorScheme.ProgressBar;
		GUI.Box(new Rect(position.x + 2f, position.y + 2f, Mathf.RoundToInt((position.width - 4f) * amount), 19f), GUIContent.none, BlueStonez.progressbar_large_thumb);
		GUI.color = Color.white;
	}

	private void DrawAllServers(Rect pos)
	{
		int num = Singleton<GameServerManager>.Instance.PhotonServerCount * 48;
		GUI.color = Color.white;
		_serverSelectionScrollBar = GUITools.BeginScrollView(new Rect(0f, 31f, pos.width + 1f, pos.height - 31f - 55f), _serverSelectionScrollBar, new Rect(0f, 0f, pos.width - 20f, num));
		List<string> list = new List<string>();
		int num2 = 0;
		string empty = string.Empty;
		foreach (GameServerView photonServer in Singleton<GameServerManager>.Instance.PhotonServerList)
		{
			GUI.BeginGroup(new Rect(0f, num2 * 48, pos.width + 2f, 49f), BlueStonez.box_grey50);
			if (Singleton<GameServerController>.Instance.SelectedServer != null && Singleton<GameServerController>.Instance.SelectedServer == photonServer)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.03f);
				GUI.DrawTexture(new Rect(1f, 0f, pos.width + 1f, 49f), UberstrikeIconsHelper.White);
				GUI.color = Color.white;
			}
			photonServer.Flag.Draw(new Rect(5f, 8f, 32f, 32f));
			list.Add(photonServer.Flag.Url);
			GUI.Label(new Rect(42f, 1f, _serverNameColumnWidth + 1f - 42f, 48f), photonServer.Name, BlueStonez.label_interparkbold_16pt_left);
			if (photonServer.Data.State == ServerLoadData.Status.Alive)
			{
				GUI.BeginGroup(new Rect(5f + _serverNameColumnWidth, 0f, 126f, 48f));
				int num3 = 0;
				num3 = ((PlayerDataManager.AccessLevel != MemberAccessLevel.Admin) ? Mathf.Clamp(photonServer.Data.PlayersConnected, 0, (int)photonServer.Data.MaxPlayerCount) : photonServer.Data.PlayersConnected);
				float num4 = 0f;
				DrawProgressBarLarge(amount: (!((float)num3 >= photonServer.Data.MaxPlayerCount)) ? ((float)num3 / photonServer.Data.MaxPlayerCount) : 1f, position: new Rect(2f, 12f, 58f, 20f));
				empty = string.Format("{0}/{1}", num3, photonServer.Data.MaxPlayerCount);
				GUI.Label(new Rect(64f, 14f, 60f, 20f), empty, BlueStonez.label_interparkmed_10pt_left);
				GUI.EndGroup();
				GUI.BeginGroup(new Rect(5f + _serverNameColumnWidth + 130f, 0f, 105f - (float)(((float)num > pos.height - 31f) ? 21 : 0), 48f));
				int latency = photonServer.Latency;
				empty = string.Empty;
				if (latency < 100)
				{
					GUI.color = ColorConverter.RgbToColor(80f, 99f, 42f);
					empty = LocalizedStrings.FastCaps;
				}
				else if (latency < 300)
				{
					GUI.color = ColorConverter.RgbToColor(234f, 112f, 13f);
					empty = LocalizedStrings.MedCaps;
				}
				else
				{
					GUI.color = ColorConverter.RgbToColor(192f, 80f, 70f);
					empty = LocalizedStrings.SlowCaps;
				}
				GUI.DrawTexture(new Rect(0f, 14f, 45f, 20f), UberstrikeIconsHelper.White);
				GUI.color = Color.white;
				GUI.Label(new Rect(2f, 14f, 40f, 20f), empty, BlueStonez.label_interparkbold_16pt);
				GUI.Label(new Rect(48f, 4f, 40f, 40f), string.Format("{0}ms", latency), BlueStonez.label_interparkmed_10pt_left);
				GUI.EndGroup();
				GUI.BeginGroup(new Rect(5f + _serverNameColumnWidth + 130f + _joinServerButtonWidth - 5f, 0f, _joinServerButtonWidth - 5f - (float)(((float)num > pos.height - 31f) ? 21 : 0), 48f));
				int height = BlueStonez.button.normal.background.height;
				if (GUI.Button(new Rect(2f, 24 - height / 2, _joinServerButtonWidth - 30f, height), LocalizedStrings.JoinCaps, BlueStonez.button_white))
				{
					Singleton<GameServerController>.Instance.SelectedServer = photonServer;
					SelectedServerUpdated();
					SfxManager.Play2dAudioClip(GameAudio.JoinServer);
				}
				GUI.EndGroup();
			}
			else if (photonServer.Data.State == ServerLoadData.Status.None)
			{
				Rect position = new Rect(5f + _serverNameColumnWidth, 0f, 236f - (float)(((float)num > pos.height - 31f) ? 21 : 0), 48f);
				GUI.BeginGroup(position);
				GUI.Label(new Rect(0f, 0f, position.width, 48f), LocalizedStrings.RefreshingServer, BlueStonez.label_interparkbold_16pt);
				GUI.EndGroup();
			}
			else if (photonServer.Data.State == ServerLoadData.Status.NotReachable)
			{
				Rect position2 = new Rect(5f + _serverNameColumnWidth, 0f, 236f - (float)(((float)num > pos.height - 31f) ? 21 : 0), 48f);
				GUI.BeginGroup(position2);
				GUI.Label(new Rect(0f, 0f, position2.width, 48f), LocalizedStrings.ServerIsNotReachable, BlueStonez.label_interparkbold_16pt);
				GUI.EndGroup();
			}
			if (GUI.Button(new Rect(0f, 0f, pos.width + 1f, 49f), GUIContent.none, GUIStyle.none) && photonServer.Data.State != ServerLoadData.Status.NotReachable)
			{
				if (Singleton<GameServerController>.Instance.SelectedServer == photonServer && _serverJoinDoubleClick > Time.time)
				{
					_serverJoinDoubleClick = 0f;
					SelectedServerUpdated();
					SfxManager.Play2dAudioClip(GameAudio.JoinServer);
				}
				else
				{
					_serverJoinDoubleClick = Time.time + 0.5f;
				}
				Singleton<GameServerController>.Instance.SelectedServer = photonServer;
			}
			GUI.EndGroup();
			num2++;
		}
		GUITools.EndScrollView();
		if (list.Count > 0)
		{
			list.Reverse();
			AutoMonoBehaviour<TextureLoader>.Instance.SetFirstToLoadImages(list);
		}
	}

	private void DrawServerListButtons(Rect rect, float helpPartWidth)
	{
		if (GUITools.Button(new Rect(rect.width - 180f, rect.height - 42f, 160f, 32f), new GUIContent(LocalizedStrings.ExploreMaps), BlueStonez.button_white))
		{
			MenuPageManager.Instance.LoadPage(PageType.Training);
		}
	}

	public void SelectedServerUpdated()
	{
		if (Singleton<GameServerController>.Instance.SelectedServer != null && Singleton<GameServerController>.Instance.SelectedServer.Data.State == ServerLoadData.Status.Alive)
		{
			if (PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA)
			{
				ShowGameSelection();
			}
			else if ((float)Singleton<GameServerController>.Instance.SelectedServer.Data.PlayersConnected >= Singleton<GameServerController>.Instance.SelectedServer.Data.MaxPlayerCount)
			{
				PopupSystem.ShowMessage(LocalizedStrings.ServerFull, LocalizedStrings.ServerFullMsg);
			}
			else if (!Singleton<GameServerController>.Instance.SelectedServer.CheckLatency())
			{
				PopupSystem.ShowMessage(LocalizedStrings.Warning, "Your connection to this server is too slow.", PopupSystem.AlertType.OK, null);
			}
			else if (Singleton<GameServerController>.Instance.SelectedServer.Latency >= 300)
			{
				PopupSystem.ShowMessage(LocalizedStrings.Warning, LocalizedStrings.ConnectionSlowMsg, PopupSystem.AlertType.OKCancel, ShowGameSelection, LocalizedStrings.OkCaps, null, LocalizedStrings.CancelCaps);
			}
			else if (Singleton<GameServerController>.Instance.SelectedServer.UsageType != PhotonUsageType.Mobile)
			{
				PopupSystem.ShowMessage(LocalizedStrings.Warning, LocalizedStrings.NonMobileServer, PopupSystem.AlertType.OKCancel, ShowGameSelection, LocalizedStrings.OkCaps, null, LocalizedStrings.CancelCaps);
			}
			else
			{
				ShowGameSelection();
			}
		}
		else
		{
			Debug.LogError("Couldn't connect to server!");
		}
	}

	private void DrawGroupLabel(Rect position, string header, string text)
	{
		GUI.color = Color.white;
		GUI.Label(new Rect(position.x, position.y, position.width, 16f), header, BlueStonez.label_interparkbold_13pt);
		GUI.color = new Color(1f, 1f, 1f, 0.8f);
		GUI.Label(new Rect(position.x, position.y + 16f, position.width, position.height - 16f), text, BlueStonez.label_interparkbold_11pt_left_wrap);
		GUI.color = Color.white;
	}

	private void DoGamePage(Rect rect)
	{
		GUI.BeginGroup(rect);
		GUI.Label(new Rect(0f, 0f, rect.width, 56f), LocalizedStrings.ChooseAGameCaps, BlueStonez.tab_strip);
		GUI.color = new Color(1f, 1f, 1f, 0.5f);
		GUI.Label(new Rect(10f, 28f, rect.width - 37f, 28f), string.Format("{0} ({1}ms)", Singleton<GameServerController>.Instance.SelectedServer.Name, Singleton<GameServerController>.Instance.SelectedServer.Latency.ToString()), BlueStonez.label_interparkbold_18pt_left);
		GUI.Label(new Rect(0f, 28f, rect.width - 5f, 28f), string.Format("{0} {1}, {2} {3} ", Singleton<GameListManager>.Instance.PlayersCount, LocalizedStrings.PlayersOnline, _filteredActiveRoomCount, LocalizedStrings.Games), BlueStonez.label_interparkbold_18pt_right);
		GUI.color = Color.white;
		GUI.Box(new Rect(0f, 55f, rect.width, rect.height - 57f), string.Empty, BlueStonez.window_standard_grey38);
		DrawQuickSearch(rect);
		if (GUITools.Button(new Rect(rect.width - 150f, 6f, 140f, 23f), new GUIContent(LocalizedStrings.Refresh), BlueStonez.buttondark_medium))
		{
			if (!LobbyConnectionManager.IsConnected)
			{
				ResetLobbyDisconnectTimeout();
				LobbyConnectionManager.StartConnection();
			}
			RefreshGameList();
		}
		bool flag = GUI.enabled;
		GUI.enabled &= _dropDownList == 0 && !_checkForPassword;
		DoGameList(rect);
		DoBottomArea(rect);
		GUI.enabled = flag;
		if (_showFilters)
		{
			DoFilterArea(rect);
		}
		GUI.EndGroup();
	}

	private void DoGameList(Rect rect)
	{
		UpdateColumnWidth();
		int num = Mathf.RoundToInt(rect.height) - 73 - 104;
		if (!_showFilters)
		{
			num += 73;
		}
		Rect rect2 = new Rect(10f, 55f, rect.width - 20f, num);
		GUI.Box(rect2, string.Empty, BlueStonez.box_grey50);
		GUI.BeginGroup(rect2);
		if (!LobbyConnectionManager.IsConnected)
		{
			GUI.Label(new Rect(0f, rect2.height * 0.5f, rect2.width, 23f), LocalizedStrings.PressRefreshToSeeCurrentGames, BlueStonez.label_interparkmed_11pt);
			if (GUITools.Button(new Rect(rect2.width * 0.5f - 70f, rect2.height * 0.5f - 30f, 140f, 23f), new GUIContent(LocalizedStrings.Refresh), BlueStonez.buttondark_medium))
			{
				if (!LobbyConnectionManager.IsConnected)
				{
					ResetLobbyDisconnectTimeout();
					LobbyConnectionManager.StartConnection();
				}
				RefreshGameList();
			}
		}
		else if (LobbyConnectionManager.IsConnecting)
		{
			GUI.Label(new Rect(0f, 0f, rect2.width, rect2.height - 1f), LocalizedStrings.ConnectingToLobby, BlueStonez.label_interparkmed_11pt);
		}
		else if (LobbyConnectionManager.IsInLobby && _cachedGameList.Count == 0)
		{
			GUI.Label(new Rect(0f, 0f, rect2.width, rect2.height - 1f), LocalizedStrings.LoadingGameList, BlueStonez.label_interparkmed_11pt);
		}
		num = ((Singleton<GameServerController>.Instance.SelectedServer != null) ? (70 * ((_filteredActiveRoomCount < 0 || _filteredActiveRoomCount > _cachedGameList.Count) ? _cachedGameList.Count : _filteredActiveRoomCount) + 5) : 0);
		int num2 = 0;
		int num3 = 0;
		Texture2D texture2D = null;
		num2 = -1;
		GUI.Box(new Rect(num2, 0f, 110f, 25f), string.Empty, BlueStonez.box_grey50);
		num2 = 108;
		texture2D = ((_lastSortedColumn != GameListColumns.GameName) ? null : ((!_sortGamesAscending) ? _sortDownArrow : _sortUpArrow));
		num3 = ((_lastSortedColumn != GameListColumns.GameName) ? 12 : 5);
		GUI.Box(new Rect(num2, 0f, _gameNameWidth, 25f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(num2 + num3, 0f, _gameNameWidth, 25f), new GUIContent(LocalizedStrings.Name, texture2D), BlueStonez.label_interparkbold_16pt_left);
		if (GUI.Button(new Rect(num2, 0f, _gameNameWidth, 25f), GUIContent.none, BlueStonez.label_interparkbold_11pt_left))
		{
			SortGameList(GameListColumns.GameName);
		}
		num2 = 110 + _gameNameWidth - 3;
		texture2D = ((_lastSortedColumn != GameListColumns.GameMap) ? null : ((!_sortGamesAscending) ? _sortDownArrow : _sortUpArrow));
		num3 = ((_lastSortedColumn != GameListColumns.GameMap) ? 12 : 5);
		GUI.Box(new Rect(num2, 0f, _mapNameWidth, 25f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(num2 + num3, 0f, _mapNameWidth, 25f), new GUIContent(LocalizedStrings.Map, texture2D), BlueStonez.label_interparkbold_16pt_left);
		if (GUI.Button(new Rect(num2, 0f, _mapNameWidth, 25f), string.Empty, BlueStonez.label_interparkbold_11pt_left))
		{
			SortGameList(GameListColumns.GameMap);
		}
		num2 = 110 + _gameNameWidth + _mapNameWidth - 4;
		texture2D = ((_lastSortedColumn != GameListColumns.GameMode) ? null : ((!_sortGamesAscending) ? _sortDownArrow : _sortUpArrow));
		num3 = ((_lastSortedColumn != GameListColumns.GameMode) ? 12 : 5);
		GUI.Box(new Rect(num2, 0f, _gameModeWidth, 25f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(num2 + num3, 0f, _gameModeWidth, 25f), new GUIContent(LocalizedStrings.Mode, texture2D), BlueStonez.label_interparkbold_16pt_left);
		if (GUI.Button(new Rect(num2, 0f, _gameModeWidth, 25f), string.Empty, BlueStonez.label_interparkbold_11pt_left))
		{
			SortGameList(GameListColumns.GameMode);
		}
		num2 = 110 + _gameNameWidth + _mapNameWidth + _gameModeWidth - 10;
		texture2D = ((_lastSortedColumn != GameListColumns.GameTime) ? null : ((!_sortGamesAscending) ? _sortDownArrow : _sortUpArrow));
		num3 = ((_lastSortedColumn != GameListColumns.GameTime) ? 12 : 5);
		GUI.Box(new Rect(num2, 0f, 85f, 25f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(num2 + num3, 0f, 80f, 25f), new GUIContent(LocalizedStrings.Minutes, texture2D), BlueStonez.label_interparkbold_16pt_left);
		if (GUI.Button(new Rect(num2, 0f, 80f, 25f), string.Empty, BlueStonez.label_interparkbold_11pt_left))
		{
			SortGameList(GameListColumns.GameTime);
		}
		num2 = 110 + _gameNameWidth + _mapNameWidth + _gameModeWidth + 80 - 6;
		texture2D = ((_lastSortedColumn != GameListColumns.PlayerCount) ? null : ((!_sortGamesAscending) ? _sortDownArrow : _sortUpArrow));
		num3 = ((_lastSortedColumn != GameListColumns.PlayerCount) ? 12 : 5);
		GUI.Box(new Rect(num2, 0f, 80f, 25f), string.Empty, BlueStonez.box_grey50);
		GUI.Label(new Rect(num2 + num3, 0f, 80f, 25f), new GUIContent(LocalizedStrings.Players, texture2D), BlueStonez.label_interparkbold_16pt_left);
		if (GUI.Button(new Rect(num2, 0f, 80f, 25f), string.Empty, BlueStonez.label_interparkbold_11pt_left))
		{
			SortGameList(GameListColumns.PlayerCount);
		}
		if (LobbyConnectionManager.IsConnected)
		{
			Vector2 serverScroll = _serverScroll;
			_serverScroll = GUITools.BeginScrollView(new Rect(0f, 25f, rect2.width, rect2.height - 1f - 25f), _serverScroll, new Rect(0f, 0f, rect2.width - 60f, num), BlueStonez.horizontalScrollbar, BlueStonez.verticalScrollbar);
			_filteredActiveRoomCount = DrawAllGames(rect2, rect2.height <= (float)num);
			GUITools.EndScrollView();
			if (serverScroll != _serverScroll)
			{
				ResetLobbyDisconnectTimeout();
			}
		}
		GUI.EndGroup();
	}

	private void SortServerList(ServerListColumns sortedColumn)
	{
		SortServerList(sortedColumn, true);
	}

	private void SortServerList(ServerListColumns sortedColumn, bool changeDirection)
	{
		if (changeDirection && sortedColumn == _lastSortedServerColumn)
		{
			_sortServerAscending = !_sortServerAscending;
		}
		_lastSortedServerColumn = sortedColumn;
		switch (sortedColumn)
		{
		case ServerListColumns.ServerName:
			Singleton<GameServerManager>.Instance.SortServers(new GameServerNameComparer(), _sortServerAscending);
			break;
		case ServerListColumns.ServerCapacity:
			Singleton<GameServerManager>.Instance.SortServers(new GameServerPlayerCountComparer(), _sortServerAscending);
			break;
		case ServerListColumns.ServerSpeed:
			Singleton<GameServerManager>.Instance.SortServers(new GameServerLatencyComparer(), _sortServerAscending);
			break;
		default:
			Singleton<GameServerManager>.Instance.SortServers(new GameServerLatencyComparer(), _sortServerAscending);
			break;
		}
	}

	private void SortGameList(GameListColumns sortedColumn)
	{
		if (sortedColumn == _lastSortedColumn)
		{
			_sortGamesAscending = !_sortGamesAscending;
		}
		_lastSortedColumn = sortedColumn;
		switch (sortedColumn)
		{
		case GameListColumns.Lock:
			SortGameList(new GameDataAccessComparer());
			break;
		case GameListColumns.GameName:
			SortGameList(new GameDataNameComparer());
			break;
		case GameListColumns.GameMap:
			SortGameList(new GameDataMapComparer());
			break;
		case GameListColumns.GameMode:
			SortGameList(new GameDataRuleComparer());
			break;
		case GameListColumns.PlayerCount:
			SortGameList(new GameDataPlayerComparer());
			break;
		case GameListColumns.GameTime:
			SortGameList(new GameDataTimeComparer());
			break;
		default:
			SortGameList(new GameDataLatencyComparer());
			break;
		}
	}

	private void SortGameList(IComparer<GameMetaData> sortedColumn)
	{
		_gameSortingMethod = sortedColumn;
		RefreshGameList();
	}

	private void DoFilterArea(Rect rect)
	{
		bool flag = GUI.enabled;
		Rect position = new Rect(10f, rect.height - 73f - 50f, rect.width - 20f, 74f);
		GUI.Box(position, string.Empty, BlueStonez.box_grey50);
		GUI.BeginGroup(new Rect(position.x, position.y, position.width, position.width + 60f));
		GUI.enabled = flag && (_dropDownList == 0 || _dropDownList == 1);
		GUI.Label(new Rect(10f, 10f, 115f, 21f), _mapsFilter[_currentMap], BlueStonez.label_dropdown);
		if (GUI.Button(new Rect(123f, 9f, 21f, 21f), GUIContent.none, BlueStonez.dropdown_button))
		{
			_dropDownList = ((_dropDownList == 0) ? 1 : 0);
			_dropDownRect = new Rect(10f, 31f, 133f, 80f);
		}
		GUI.enabled = flag && (_dropDownList == 0 || _dropDownList == 2);
		GUI.Label(new Rect(10f, 42f, 115f, 21f), _modesFilter[_currentMode], BlueStonez.label_dropdown);
		if (GUI.Button(new Rect(123f, 41f, 21f, 21f), GUIContent.none, BlueStonez.dropdown_button))
		{
			_dropDownList = ((_dropDownList == 0) ? 2 : 0);
			_dropDownRect = new Rect(10f, 63f, 133f, 60f);
		}
		GUI.enabled = flag && (_dropDownList == 0 || _dropDownList == 4) && _singleWeapon;
		GUI.enabled = flag && _dropDownList == 0;
		_gameNotFull = GUI.Toggle(new Rect(165f, 7f, 170f, 16f), _gameNotFull, LocalizedStrings.GameNotFull, BlueStonez.toggle);
		_noPrivateGames = GUI.Toggle(new Rect(165f, 28f, 170f, 16f), _noPrivateGames, LocalizedStrings.NotPasswordProtected, BlueStonez.toggle);
		GUI.enabled = false;
		GUI.enabled = flag;
		if (_dropDownList != 0)
		{
			DoDropDownList();
		}
		GUI.EndGroup();
	}

	private bool CheckChangesInFilter()
	{
		bool result = false;
		if (_filterSavedData.UseFilter != _showFilters)
		{
			_filterSavedData.UseFilter = _showFilters;
			result = true;
		}
		if (_filterSavedData.MapName != _mapsFilter[_currentMap])
		{
			_filterSavedData.MapName = _mapsFilter[_currentMap];
			result = true;
		}
		if (_filterSavedData.GameMode != _modesFilter[_currentMode])
		{
			_filterSavedData.GameMode = _modesFilter[_currentMode];
			result = true;
		}
		if (_filterSavedData.NoFriendlyFire != _noFriendFire)
		{
			_filterSavedData.NoFriendlyFire = _noFriendFire;
			result = true;
		}
		if (_filterSavedData.ISGameNotFull != _gameNotFull)
		{
			_filterSavedData.ISGameNotFull = _gameNotFull;
			result = true;
		}
		if (_filterSavedData.NoPasswordProtection != _noPrivateGames)
		{
			_filterSavedData.NoPasswordProtection = _noPrivateGames;
			result = true;
		}
		return result;
	}

	private void DoBottomArea(Rect rect)
	{
		GUITools.PushGUIState();
		GUI.enabled = _dropDownList == 0;
		GUI.enabled = true;
		if (GUITools.Button(new Rect(rect.width - 160f, rect.height - 42f, 140f, 32f), new GUIContent(LocalizedStrings.CreateGameCaps), BlueStonez.button))
		{
			ResetLobbyDisconnectTimeout();
			PanelManager.Instance.OpenPanel(PanelType.CreateGame);
		}
		GUI.enabled = LobbyConnectionManager.IsConnected && _selectedGame != null && Singleton<GameServerController>.Instance.SelectedServer != null && Singleton<GameServerController>.Instance.SelectedServer.Data.RoomsCreated != 0 && !PanelManager.Instance.IsPanelOpen(PanelType.CreateGame);
		GUITools.PopGUIState();
	}

	private void DrawQuickSearch(Rect position)
	{
		_searchBar.Draw(new Rect(position.width - 304f, 8f, 142f, 20f));
		if (!_refreshGameListOnSortChange && _searchBar.FilterText.Length > 0)
		{
			RefreshGameList();
			_refreshGameListOnSortChange = true;
		}
		if (_refreshGameListOnSortChange && _searchBar.FilterText.Length == 0)
		{
			RefreshGameList();
			_refreshGameListOnSortChange = false;
		}
	}

	private void DoDropDownList()
	{
		string[] array = null;
		switch (_dropDownList)
		{
		case 1:
			array = _mapsFilter;
			break;
		case 2:
			array = _modesFilter;
			break;
		case 4:
			array = _weaponClassTexts;
			break;
		default:
			Debug.LogError("Nondefined drop down list: " + _dropDownList);
			return;
		}
		GUI.Box(_dropDownRect, string.Empty, BlueStonez.window);
		_filterScroll = GUITools.BeginScrollView(_dropDownRect, _filterScroll, new Rect(0f, 0f, _dropDownRect.width - 20f, 20 * array.Length));
		for (int i = 0; i < array.Length; i++)
		{
			GUI.Label(new Rect(2f, 20 * i, _dropDownRect.width, 20f), array[i], BlueStonez.dropdown_list);
			if (GUI.Button(new Rect(2f, 20 * i, _dropDownRect.width, 20f), string.Empty, BlueStonez.dropdown_list))
			{
				switch (_dropDownList)
				{
				case 1:
					_currentMap = i;
					break;
				case 2:
					_currentMode = i;
					break;
				case 4:
					_currentWeapon = i;
					break;
				}
				_dropDownList = 0;
				_filterScroll.y = 0f;
			}
		}
		GUITools.EndScrollView();
	}

	private void JoinGame()
	{
		if ((_selectedGame.ConnectedPlayers < _selectedGame.MaxPlayers && (_selectedGame.ConnectedPlayers > 0 || _selectedGame.HasLevelRestriction)) || PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA)
		{
			if (ApplicationDataManager.IsMobile && _selectedGame.MaxPlayers > 6)
			{
				PopupSystem.ShowMessage(LocalizedStrings.Warning, LocalizedStrings.MobileGameMoreThan6Players, PopupSystem.AlertType.OKCancel, delegate
				{
					Singleton<GameStateController>.Instance.JoinGame(_selectedGame);
				}, LocalizedStrings.OkCaps, null, LocalizedStrings.CancelCaps);
			}
			else
			{
				Singleton<GameStateController>.Instance.JoinGame(_selectedGame);
			}
		}
		else if (_selectedGame.ConnectedPlayers == 0)
		{
			PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ThisGameNoLongerExists);
		}
		else if (_selectedGame.ConnectedPlayers == _selectedGame.MaxPlayers)
		{
			PopupSystem.ShowMessage(LocalizedStrings.Error, LocalizedStrings.ThisGameIsFull);
		}
	}

	private void PasswordCheck(Rect position)
	{
		GUITools.PushGUIState();
		GUI.BeginGroup(position, GUIContent.none, BlueStonez.window);
		GUI.Label(new Rect(0f, 0f, position.width, 56f), LocalizedStrings.EnterPassword, BlueStonez.tab_strip);
		GUI.Box(new Rect(16f, 55f, position.width - 32f, position.height - 56f - 64f), GUIContent.none, BlueStonez.window_standard_grey38);
		GUI.SetNextControlName("EnterPassword");
		Rect position2 = new Rect((position.width - 188f) / 2f, 80f, 188f, 24f);
		_inputedPassword = GUI.PasswordField(position2, _inputedPassword, '*', 18, BlueStonez.textField);
		_inputedPassword = _inputedPassword.Trim('\n');
		if (string.IsNullOrEmpty(_inputedPassword) && GUI.GetNameOfFocusedControl() != "EnterPassword")
		{
			GUI.color = new Color(1f, 1f, 1f, 0.3f);
			GUI.Label(position2, LocalizedStrings.TypePasswordHere, BlueStonez.label_interparkmed_11pt);
			GUI.color = Color.white;
		}
		GUI.enabled = Time.time > _nextPasswordCheck;
		if (GUITools.Button(new Rect(position.width - 100f - 10f, 152f, 100f, 32f), new GUIContent(LocalizedStrings.OkCaps), BlueStonez.button) || (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.Layout && Time.time > _nextPasswordCheck))
		{
			if (_selectedGame != null && _inputedPassword == _selectedGame.Password)
			{
				_checkForPassword = false;
				_inputedPassword = string.Empty;
				_isPasswordOk = true;
				JoinGame();
			}
			else
			{
				_inputedPassword = string.Empty;
				_isPasswordOk = false;
				_nextPasswordCheck = Time.time + _timeDelayOnCheckPassword;
			}
			Input.ResetInputAxes();
		}
		GUI.enabled = true;
		if (GUITools.Button(new Rect(10f, 152f, 100f, 32f), new GUIContent(LocalizedStrings.CancelCaps), BlueStonez.button))
		{
			_isPasswordOk = true;
			_checkForPassword = false;
			_inputedPassword = string.Empty;
		}
		if (!_isPasswordOk && string.IsNullOrEmpty(_inputedPassword))
		{
			GUI.color = Color.red;
			GUI.Label(new Rect((position.width - 188f) / 2f, 110f, 188f, 24f), LocalizedStrings.PasswordIncorrect, BlueStonez.label_interparkbold_11pt);
			GUI.color = Color.white;
		}
		GUI.EndGroup();
		GUITools.PopGUIState();
	}

	private bool CanPassFilter(GameMetaData game)
	{
		if (game == null)
		{
			return false;
		}
		GameFlags gameFlags = new GameFlags();
		gameFlags.SetFlags(game.GameModifierFlags);
		bool flag = _searchBar.CheckIfPassFilter(game.RoomName);
		bool flag2 = true;
		bool flag3 = true;
		bool flag4 = true;
		bool flag5 = true;
		if (ApplicationDataManager.IsMobile && game.HasLevelRestriction)
		{
			return false;
		}
		if (!Singleton<MapManager>.Instance.MapExistsWithId(game.MapID))
		{
			return false;
		}
		flag2 = _mapsFilter[_currentMap] == LocalizedStrings.All + " Maps" || Singleton<MapManager>.Instance.GetMapName(game.MapID) == _mapsFilter[_currentMap];
		flag3 = _modesFilter[_currentMode] == LocalizedStrings.All + " Modes" || GameModes.GetModeName((GameMode)game.GameMode) == _modesFilter[_currentMode];
		if (_gameNotFull)
		{
			flag4 = !game.IsFull;
		}
		if (_noPrivateGames)
		{
			flag5 = game.IsPublic;
		}
		if (_showFilters)
		{
			return flag && flag2 && flag3 && flag4 && flag5 && _showFilters;
		}
		return flag;
	}

	private string DisplayMapIcon(int mapId, Rect rect)
	{
		string mapSceneName = Singleton<MapManager>.Instance.GetMapSceneName(mapId);
		string text = ApplicationDataManager.BaseImageURL + "MapIcons/" + mapSceneName + ".jpg";
		if (_mapImages.ContainsKey(mapId))
		{
			_mapImages[mapId].Draw(rect);
		}
		else
		{
			DynamicTexture dynamicTexture = new DynamicTexture(text);
			_mapImages[mapId] = dynamicTexture;
			dynamicTexture.Draw(rect);
		}
		return text;
	}

	private int DrawAllGames(Rect rect, bool hasVScroll)
	{
		int playerLevelSecure = PlayerDataManager.PlayerLevelSecure;
		List<string> list = new List<string>();
		int num = 0;
		foreach (GameMetaData cachedGame in _cachedGameList)
		{
			if (!CanPassFilter(cachedGame))
			{
				continue;
			}
			bool flag = GUI.enabled;
			bool flag2 = (!cachedGame.IsFull && cachedGame.IsLevelAllowed(playerLevelSecure) && (cachedGame.ConnectedPlayers > 0 || cachedGame.HasLevelRestriction)) || PlayerDataManager.AccessLevel >= MemberAccessLevel.QA;
			flag2 &= Singleton<MapManager>.Instance.HasMapWithId(cachedGame.MapID);
			GUI.enabled = flag && flag2 && _dropDownList == 0 && !_checkForPassword;
			int num2 = 70;
			int num3 = num2 * num - 1;
			GUI.Box(new Rect(0f, num3, rect.width, num2 + 1), new GUIContent(string.Empty), BlueStonez.box_grey50);
			if (!ApplicationDataManager.IsMobile)
			{
				string tooltip = LocalizedStrings.PlayCaps;
				if (!cachedGame.IsLevelAllowed(playerLevelSecure) && cachedGame.LevelMin > playerLevelSecure)
				{
					tooltip = string.Format(LocalizedStrings.YouHaveToReachLevelNToJoinThisGame, cachedGame.LevelMin);
				}
				else if (!cachedGame.IsLevelAllowed(playerLevelSecure) && cachedGame.LevelMax < playerLevelSecure)
				{
					tooltip = string.Format(LocalizedStrings.YouAlreadyMasteredThisLevel);
				}
				else if (cachedGame.IsFull)
				{
					tooltip = string.Format(LocalizedStrings.ThisGameIsFull);
				}
				else if (!cachedGame.HasLevelRestriction && cachedGame.ConnectedPlayers == 0)
				{
					tooltip = string.Format(LocalizedStrings.ThisGameNoLongerExists);
				}
				GUI.Box(new Rect(0f, num3, rect.width, num2), new GUIContent(string.Empty, tooltip), BlueStonez.box_grey50);
			}
			if (_selectedGame != null && _selectedGame.RoomID == cachedGame.RoomID)
			{
				GUI.color = new Color(1f, 1f, 1f, 0.03f);
				GUI.DrawTexture(new Rect(1f, num3, rect.width + 1f, num2), UberstrikeIconsHelper.White);
				GUI.color = Color.white;
			}
			GUIStyle style = ((!flag2) ? BlueStonez.label_interparkmed_10pt_left : BlueStonez.label_interparkbold_11pt_left);
			GUI.color = ((!cachedGame.HasLevelRestriction) ? Color.white : new Color(1f, 0.7f, 0f));
			int num4 = 0;
			string item = DisplayMapIcon(cachedGame.MapID, new Rect(num4, num3, 110f, num2));
			list.Add(item);
			if (!cachedGame.IsPublic)
			{
				GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _privateGameIcon);
			}
			else if (cachedGame.HasLevelRestriction)
			{
				if (cachedGame.LevelMax <= 5)
				{
					GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _level1GameIcon);
				}
				else if (cachedGame.LevelMax <= 10)
				{
					GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _level2GameIcon);
				}
				else if (cachedGame.LevelMax <= 20)
				{
					GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _level3GameIcon);
				}
				else if (cachedGame.LevelMin >= 40)
				{
					GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _level20GameIcon);
				}
				else if (cachedGame.LevelMin >= 30)
				{
					GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _level10GameIcon);
				}
				else if (cachedGame.LevelMin >= 20)
				{
					GUI.DrawTexture(new Rect(80f, num3 + num2 - 30, 25f, 25f), _level5GameIcon);
				}
				if (playerLevelSecure > cachedGame.LevelMax)
				{
					GUI.DrawTexture(new Rect(0f, num3 + num2 - 50, 50f, 50f), UberstrikeIcons.LevelMastered);
				}
			}
			else if (Singleton<MapManager>.Instance.IsBlueBox(cachedGame.MapID))
			{
				GUI.Label(new Rect(5f, num3 + 5, 64f, 64f), new GUIContent(UberstrikeIcons.BlueLevel32, LocalizedStrings.BlueLevelTooltip));
			}
			num4 = 120;
			GUI.Label(new Rect(num4, num3, _gameNameWidth, num2 / 2), cachedGame.RoomName, BlueStonez.label_interparkbold_13pt_left);
			GUI.Label(new Rect(num4, num3 + num2 / 2, _gameNameWidth, num2 / 2), Singleton<MapManager>.Instance.GetMapName(cachedGame.MapID) + " " + LevelRestrictionText(cachedGame), BlueStonez.label_interparkmed_10pt_left);
			num4 = 122 + _gameNameWidth - 4;
			int num5 = cachedGame.RoundTime / 60;
			GUI.Label(new Rect(num4, num3, _gameModeWidth, num2 / 2), GameModes.GetModeName(cachedGame.GameMode) + " - " + num5 + " mins " + FpsGameMode.GetGameFlagText(cachedGame), BlueStonez.label_interparkmed_10pt_left);
			GUI.Label(new Rect(num4 + 64, num3 + num2 / 2, _gameModeWidth, num2 / 2), cachedGame.ConnectedPlayersString + " players", style);
			GUI.color = Color.white;
			DrawProgressBarLarge(new Rect(num4, num3 + num2 / 2 + 5, 58f, num2 / 2), (float)cachedGame.ConnectedPlayers / (float)cachedGame.MaxPlayers);
			num4 = 110 + _gameNameWidth + _gameModeWidth - 6;
			int height = BlueStonez.button.normal.background.height;
			if (GUI.Button(new Rect(num4, num3 + num2 / 2 - height / 2, 110f, height), LocalizedStrings.JoinCaps, BlueStonez.button_white))
			{
				_selectedGame = cachedGame;
				if (_selectedGame.IsPublic || PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA)
				{
					JoinGame();
					SfxManager.Play2dAudioClip(GameAudio.JoinGame);
				}
				else
				{
					_checkForPassword = true;
					_nextPasswordCheck = Time.time;
				}
			}
			if (GUI.Button(new Rect(0f, num3, rect.width, num2), string.Empty, BlueStonez.label_interparkbold_11pt_left))
			{
				ResetLobbyDisconnectTimeout();
				if (_selectedGame != null && _selectedGame.RoomID == cachedGame.RoomID && _gameJoinDoubleClick > Time.time)
				{
					_gameJoinDoubleClick = 0f;
					if (_selectedGame.IsPublic || PlayerDataManager.AccessLevelSecure >= MemberAccessLevel.QA)
					{
						JoinGame();
						SfxManager.Play2dAudioClip(GameAudio.JoinGame);
					}
					else
					{
						_checkForPassword = true;
						_nextPasswordCheck = Time.time;
					}
				}
				else
				{
					_gameJoinDoubleClick = Time.time + 0.5f;
				}
				_selectedGame = cachedGame;
			}
			num++;
			GUI.color = Color.white;
			GUI.enabled = flag;
		}
		if (list.Count > 0)
		{
			list.Reverse();
			AutoMonoBehaviour<TextureLoader>.Instance.SetFirstToLoadImages(list);
		}
		if (num == 0 && Singleton<GameServerController>.Instance.SelectedServer != null && Singleton<GameServerController>.Instance.SelectedServer.Data.RoomsCreated > 0 && _cachedGameList.Count > 0)
		{
			GUI.Label(new Rect(0f, rect.height * 0.5f, rect.width, 23f), "No games running on this server", BlueStonez.label_interparkmed_11pt);
			if (GUITools.Button(new Rect(rect.width * 0.5f - 70f, rect.height * 0.5f - 30f, 140f, 23f), new GUIContent(LocalizedStrings.CreateGameCaps), BlueStonez.button))
			{
				ResetLobbyDisconnectTimeout();
				PanelManager.Instance.OpenPanel(PanelType.CreateGame);
			}
		}
		return num;
	}

	private string LevelRestrictionText(GameMetaData m)
	{
		if (m.HasLevelRestriction)
		{
			if (m.LevelMax == m.LevelMin)
			{
				return string.Format(LocalizedStrings.PlayerLevelNRestriction, m.LevelMin);
			}
			if (m.LevelMax == 255)
			{
				return string.Format(LocalizedStrings.PlayerLevelNPlusRestriction, m.LevelMin);
			}
			if (m.LevelMin == 0)
			{
				return string.Format(LocalizedStrings.PlayerLevelNMinusRestriction, m.LevelMax + 1);
			}
			return string.Format(LocalizedStrings.PlayerLevelNToNRestriction, m.LevelMin, m.LevelMax);
		}
		return string.Empty;
	}

	private void UpdateColumnWidth()
	{
		int num = Screen.width - 40;
		_gameNameWidth = num - 110 - _gameModeWidth - 110;
	}

	public void Show()
	{
		if (_isConnectedToGameServer)
		{
			ShowGameSelection();
		}
		else
		{
			ShowServerSelection();
		}
	}

	private void ShowGameSelection()
	{
		if (Singleton<GameServerController>.Instance.SelectedServer != null)
		{
			_isConnectedToGameServer = true;
			_cachedGameList.Clear();
			CmuneNetworkManager.CurrentLobbyServer = Singleton<GameServerController>.Instance.SelectedServer;
			LobbyConnectionManager.StartConnection();
			CoroutineManager.StartCoroutine(StartUpdatingGameListFromServer);
			CoroutineManager.StartCoroutine(StartDisconnectFormServerAfterTimeout);
		}
	}

	public void ShowServerSelection()
	{
		_isConnectedToGameServer = false;
		if (_lastSortedServerColumn == ServerListColumns.None)
		{
			_lastSortedServerColumn = ServerListColumns.ServerSpeed;
			SortServerList(_lastSortedServerColumn, false);
		}
		StopGameSelection();
		RefreshServerLoad();
	}

	public void Hide()
	{
		StopGameSelection();
	}

	private void StopGameSelection()
	{
		LobbyConnectionManager.Stop();
		CoroutineManager.StopCoroutine(StartUpdatingGameListFromServer);
		CoroutineManager.StopCoroutine(StartDisconnectFormServerAfterTimeout);
		CheckForPassword = false;
	}

	public void RefreshGameList()
	{
		bool flag = false;
		_cachedGameList.Clear();
		if (Singleton<GameListManager>.Instance.GamesCount > 0)
		{
			foreach (GameMetaData game in Singleton<GameListManager>.Instance.GameList)
			{
				_cachedGameList.Add(game);
				if (_selectedGame != null && game.RoomID == _selectedGame.RoomID)
				{
					flag = true;
				}
			}
			SortGames(_gameSortingMethod);
			ResetLobbyDisconnectTimeout();
		}
		else
		{
			Debug.LogWarning("Failed to sort game list because games count is zero!");
		}
		if (!flag)
		{
			_selectedGame = null;
		}
	}

	private void SortGames(IComparer<GameMetaData> method)
	{
		GameDataComparer.SortAscending = _sortGamesAscending;
		_cachedGameList.Sort(method);
	}

	private IEnumerator StartUpdatingGameListFromServer()
	{
		int routineID = CoroutineManager.Begin(StartUpdatingGameListFromServer);
		while (CoroutineManager.IsCurrent(StartUpdatingGameListFromServer, routineID))
		{
			foreach (GameMetaData g in _cachedGameList)
			{
				GameMetaData game;
				if (Singleton<GameListManager>.Instance.TryGetGame(g.RoomID, out game))
				{
					g.ConnectedPlayers = game.ConnectedPlayers;
				}
				else
				{
					g.ConnectedPlayers = 0;
				}
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private IEnumerator StartDisconnectFormServerAfterTimeout()
	{
		int routineId = CoroutineManager.Begin(StartDisconnectFormServerAfterTimeout);
		ResetLobbyDisconnectTimeout();
		while (CoroutineManager.IsCurrent(StartDisconnectFormServerAfterTimeout, routineId))
		{
			if (_timeToLobbyDisconnect < Time.time && LobbyConnectionManager.IsConnected)
			{
				LobbyConnectionManager.Stop();
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void ResetLobbyDisconnectTimeout()
	{
		_timeToLobbyDisconnect = Time.time + 30f;
	}

	private void RefreshServerLoad()
	{
		if (_nextServerCheckTime < Time.time)
		{
			_nextServerCheckTime = Time.time + 5f;
			StartCoroutine(Singleton<GameServerManager>.Instance.StartUpdatingServerLoads());
		}
	}
}
